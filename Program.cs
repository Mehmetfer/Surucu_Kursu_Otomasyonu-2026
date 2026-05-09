using CefSharp;
using CefSharp.WinForms;
using Kolera_Mtsk.Sayfalar;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Kolera_MTSK.Login;
using Microsoft.Win32;

namespace Kolera_Mtsk
{
    static class Program
    {
        private static readonly Mutex mutex = new Mutex(true, "KOLERA_MTSK_SINGLE_INSTANCE");

        /// <summary>
        /// WebBrowser (IE modu) ile mebbis.meb.gov.tr icin TLS 1.2 ve IE11 dokuman modu.
        /// </summary>
        private static void PrepareLegacyWebBrowserForMebbis()
        {
            try
            {
                System.Net.ServicePointManager.SecurityProtocol |=
                    System.Net.SecurityProtocolType.Tls12;
            }
            catch
            {
            }

            try
            {
                string exe = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(exe))
                    exe = "Kolera_Mtsk.exe";
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    key?.SetValue(exe, 11001, RegistryValueKind.DWord);
                }
            }
            catch
            {
            }
        }

        [STAThread]
        static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Program zaten çalışıyor.", "Kolera MTSK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                PrepareLegacyWebBrowserForMebbis();
                Application.ThreadException += Application_ThreadException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                Thread.CurrentThread.CurrentCulture = new CultureInfo("tr-TR");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("tr-TR");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // CefSharp 7.3 initialize
                CefSettings settings = new CefSettings { IgnoreCertificateErrors = true };
                Cef.Initialize(settings);

                // DB ayarlarını yükle
                ServerAyarModel ayar = null;
                XmlBaglantiService xmlService = new XmlBaglantiService();
                BaglantiTestService testService = new BaglantiTestService();
                string appDataDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Kolera_MTSK");
                string xmlPath = File.Exists(Path.Combine(appDataDir, "baglantisql.xml"))
                    ? Path.Combine(appDataDir, "baglantisql.xml")
                    : Path.Combine(Application.StartupPath, "Baglantisql.xml");
                bool sqlBaglandi = false;

                if (File.Exists(xmlPath))
                {
                    try
                    {
                        ayar = xmlService.GetServerAyar(xmlPath);
                        testService.Test(ayar.ConnectionString);
                        sqlBaglandi = true;
                    }
                    catch
                    {
                        sqlBaglandi = false;
                        MessageBox.Show("SQL Server bağlantısı kurulamadı. Login formu LOCAL/demo modda açılacak.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                if (!sqlBaglandi)
                {
                    string localXml = File.Exists(Path.Combine(appDataDir, "baglanti.xml"))
                        ? Path.Combine(appDataDir, "baglanti.xml")
                        : Path.Combine(Application.StartupPath, "baglanti.xml");
                    if (File.Exists(localXml))
                    {
                        try { ayar = xmlService.GetServerAyar(localXml); } catch { ayar = null; }
                    }
                }

                // Login formunu aç
                Application.Run(new Kolera_Login(ayar));
            }
            catch (Exception ex)
            {
                LogYaz(ex);
                MessageBox.Show("Program başlatılırken hata oluştu:\n\n" + ex.Message, "HATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => LogYaz(e.Exception);
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => LogYaz(e.ExceptionObject as Exception);

        static void LogYaz(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "HataLog.txt");
                File.AppendAllText(logPath, DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "\r\n" + ex.ToString() + "\r\n--------------------\r\n");
            }
            catch { }
        }
    }
}