using Kolera_MTSK.Login;
using Kolera_Mtsk.Services;
using Microsoft.Win32;
using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class DbSecimForm : Form
    {
        public string Secim { get; private set; }

        private bool testBasarili = false;

        public DbSecimForm()
        {
            InitializeComponent();

            RdbLocal.Checked = true;

            this.Load += DbSecimForm_Load;

            Btn_Server_Ara.Enabled = false;
            Btn_Server_Ara.Click += Btn_Server_Ara_Click;

            RdbLocal.CheckedChanged += RdbLocal_CheckedChanged;
            RdbSql.CheckedChanged += RdbSql_CheckedChanged;

            Btn_testConnection.Click += BtnTest_Click;
            Btn_Devam.Click += Btn_Devam_Click;
            Btn_save.Click += Btn_save_Click;

            Cmb_Datalistele.SelectedIndexChanged += Cmb_Datalistele_SelectedIndexChanged;

        }

        // FORM LOAD
        private void DbSecimForm_Load(object sender, EventArgs e)
        {
            XmlOku("baglanti.xml");
        }

        // XML OKU
        private void XmlOku(string xmlDosya)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, xmlDosya);

                if (!File.Exists(path))
                    return;

                XmlSerializer xs = new XmlSerializer(typeof(ServerAyarModel));

                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    var model = (ServerAyarModel)xs.Deserialize(fs);

                    KSunucuAdresiBox.Text = model.Sunucu;
                    KKullaniciAdiBox.Text = model.KullaniciAdi;
                    KParolaBox.Text = model.Parola;
                    Txt_datam1.Text = model.VeritabaniAdi;
                }
            }
            catch
            {
                MessageBox.Show("XML okunamadı");
            }
        }

        // SERVER ARA
        private void Btn_Server_Ara_Click(object sender, EventArgs e)
        {
            try
            {
                Listele.Items.Clear();
                Listele.Items.Add("Local SQL Server instance'ları aranıyor...");

                string foundServer = null;

                // Registry'den olası SQL Server instance'larını kontrol et
                string[] possibleKeys = new string[]
                {
            @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL",
            @"SOFTWARE\Wow6432Node\Microsoft\Microsoft SQL Server\Instance Names\SQL"
                };

                foreach (string keyPath in possibleKeys)
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
                    {
                        if (key != null)
                        {
                            foreach (string instanceName in key.GetValueNames())
                            {
                                string fullName = (instanceName == "MSSQLSERVER")
                                    ? Environment.MachineName
                                    : Environment.MachineName + "\\" + instanceName;

                                Listele.Items.Add("Server bulundu: " + fullName);

                                if (foundServer == null)
                                    foundServer = fullName;
                            }
                        }
                    }
                }

                // Registry'de yoksa default olarak SQLEXPRESS
                if (foundServer == null)
                {
                    foundServer = @".\SQLEXPRESS"; // veya (localdb)\MSSQLLocalDB
                    Listele.Items.Add("Server bulunamadı, varsayılan olarak: " + foundServer);
                }

                // TextBox ve ComboBox güncelle
                KSunucuAdresiBox.Text = foundServer;
                Cmb_Datalistele.Items.Clear();
                Cmb_Datalistele.Items.Add(foundServer);
                Cmb_Datalistele.SelectedIndex = 0;

                Listele.Items.Add("Local SQL Server hazır.");
            }
            catch (Exception ex)
            {
                Listele.Items.Add("Hata: " + ex.Message);
            }
        }

        // DB LİSTELE
        private void Baglanti_Kur_Click(object sender, EventArgs e)
        {
            try
            {
                Listele.Items.Add("Bağlantı kuruluyor...");

                string server = KSunucuAdresiBox.Text;

                string connStr =
                    $"Server={server};" +
                    $"User Id={KKullaniciAdiBox.Text};" +
                    $"Password={KParolaBox.Text};" +
                    $"Initial Catalog=master;" +
                    $"TrustServerCertificate=True;";

                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    SqlCommand cmd =
                        new SqlCommand(
                        "SELECT name FROM sys.databases ORDER BY name",
                        con);

                    SqlDataReader dr = cmd.ExecuteReader();

                    Cmb_Datalistele.Items.Clear();

                    while (dr.Read())
                        Cmb_Datalistele.Items.Add(dr[0].ToString());
                }

                if (Cmb_Datalistele.Items.Count > 0)
                {
                    Cmb_Datalistele.SelectedIndex = 0;
                    Txt_datam1.Text =
                        Cmb_Datalistele.SelectedItem.ToString();
                }

                baglantiDurumu.Text = "AKTİF";
                baglantiDurumu.ForeColor = Color.Green;

                Listele.Items.Add("Bağlantı başarılı.");

                testBasarili = true;
            }
            catch (Exception ex)
            {
                testBasarili = false;

                baglantiDurumu.Text = "BAŞARISIZ";
                baglantiDurumu.ForeColor = Color.Red;

                MessageBox.Show(ex.Message);
            }
        }

        // TEST
        private void BtnTest_Click(object sender, EventArgs e)
        {
            try
            {
                Listele.Items.Add("Bağlantı test ediliyor...");

                string connStr;

                if (RdbLocal.Checked)
                {
                    string mdfPath =
                        Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        Txt_datam1.Text);

                    connStr =
                        $"Data Source={KSunucuAdresiBox.Text};" +
                        $"AttachDbFilename={mdfPath};" +
                        $"Integrated Security=True;";
                }
                else
                {
                    connStr =
                        $"Server={KSunucuAdresiBox.Text};" +
                        $"Initial Catalog={Txt_datam1.Text};" +
                        $"User Id={KKullaniciAdiBox.Text};" +
                        $"Password={KParolaBox.Text};" +
                        $"TrustServerCertificate=True;";
                }

                using (SqlConnection con =
                    new SqlConnection(connStr))
                {
                    con.Open();
                }

                Listele.Items.Add("Bağlantı başarılı");

                baglantiDurumu.Text = "AKTİF";
                baglantiDurumu.ForeColor = Color.Green;

                testBasarili = true;

                SaveXml();
            }
            catch (Exception ex)
            {
                testBasarili = false;

                baglantiDurumu.Text = "BAŞARISIZ";
                baglantiDurumu.ForeColor = Color.Red;

                MessageBox.Show(ex.Message);
            }
        }

        // XML KAYDET
        private void SaveXml()
        {
            var model = new ServerAyarModel()
            {
                Sunucu = KSunucuAdresiBox.Text,
                KullaniciAdi = KKullaniciAdiBox.Text,
                Parola = KParolaBox.Text,
                VeritabaniAdi = Txt_datam1.Text,
                BaglantiTuru =
                    RdbLocal.Checked ?
                    "AttachDbFilename" :
                    "SqlServer"
            };

            string xmlPath =
                RdbLocal.Checked ?
                Path.Combine(Application.StartupPath, "baglanti.xml") :
                Path.Combine(Application.StartupPath, "baglantisql.xml");

            XmlBaglantiService service =
                new XmlBaglantiService();

            service.SaveServerAyar(xmlPath, model);
            if (!RdbLocal.Checked)
            {
                string sqlXmlPathLegacy = Path.Combine(Application.StartupPath, "Baglantisql.xml");
                service.SaveServerAyar(sqlXmlPathLegacy, model);
            }

            File.WriteAllText(
                Path.Combine(Application.StartupPath, "Aktifdb.txt"),
                RdbLocal.Checked ? "LOCAL" : "SQL");
        }

        private string XmlDbAdiniOku(string path)
        {
            if (!File.Exists(path))
                return string.Empty;

            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(ServerAyarModel));
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    var model = (ServerAyarModel)xs.Deserialize(fs);
                    return model?.VeritabaniAdi?.Trim() ?? string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        // DB SEÇİM
        private void Cmb_Datalistele_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Cmb_Datalistele.SelectedItem != null)
                Txt_datam1.Text =
                    Cmb_Datalistele.SelectedItem.ToString();
        }

        // RADIO LOCAL
        private void RdbLocal_CheckedChanged(object sender, EventArgs e)
        {
            if (RdbLocal.Checked)
            {
                XmlOku("baglanti.xml");
                Btn_Server_Ara.Enabled = false;
            }
        }

        // RADIO SQL
        private void RdbSql_CheckedChanged(object sender, EventArgs e)
        {
            if (RdbSql.Checked)
            {
                XmlOku("baglantisql.xml");
                Btn_Server_Ara.Enabled = true;
            }
        }

        // DEVAM
        private void BtnDevam_Click(object sender, EventArgs e)
        {
            if (!testBasarili)
            {
                MessageBox.Show("Önce bağlantıyı test edin");
                return;
            }

            Secim =
                RdbLocal.Checked ?
                "LOCAL" :
                "SQL";

            // Eski davranis: secilen veritabaniyla baglantiyi aktif edip formu kapat.
            SaveXml();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Btn_Devam_Click(object sender, EventArgs e)
        {
            if (!testBasarili)
            {
                MessageBox.Show("Önce testi yapınız");
                return;
            }

            try
            {
                // Secili baglanti bilgilerini kaydet; kurulum ekrani ayni ayarlari kullansin.
                SaveXml();

                var frm = new Db_olsturvekopyala
                {
                    SunucuAdi = (KSunucuAdresiBox.Text ?? string.Empty).Trim(),
                    KullaniciAdi = (KKullaniciAdiBox.Text ?? string.Empty).Trim(),
                    Parola = KParolaBox.Text ?? string.Empty,
                    KaynakVeritabaniAdi = (Txt_datam1.Text ?? string.Empty).Trim()
                };

                frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Kurulum ekranı açılamadı: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Btn_save_Click(object sender, EventArgs e)
        {
            try
            {
                // Otomatik bağlantı testi çalıştır
                if (!testBasarili)
                {
                    BtnTest_Click(null, null); // Testi tetikle
                    if (!testBasarili)
                    {
                        MessageBox.Show(
                            "Bağlantı testi başarısız. Kaydedilemiyor.",
                            "Hata",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                }

                // Seçim türünü belirle
                Secim = RdbLocal.Checked ? "LOCAL" : "SQL";

                // Uygulamanin aktif kullandigi xml/txt dosyalarina da yaz.
                SaveXml();

                // Bağlantı modelini oluştur
                ServerAyarModel model = new ServerAyarModel
                {
                    Sunucu = KSunucuAdresiBox.Text.Trim(),
                    KullaniciAdi = KKullaniciAdiBox.Text.Trim(),
                    Parola = KParolaBox.Text,
                    VeritabaniAdi =
                        !string.IsNullOrWhiteSpace(Txt_datam1.Text)
                        ? Txt_datam1.Text.Trim()
                        : (Cmb_Datalistele.SelectedItem != null
                            ? Cmb_Datalistele.SelectedItem.ToString()
                            : string.Empty),
                    BaglantiTuru = RdbLocal.Checked ? "AttachDbFilename" : "SqlServer"
                };

                // ApplicationData altına klasör oluştur
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Kolera_MTSK");
                Directory.CreateDirectory(appData);

                // XML ve Aktifdb.txt yolları
                string xmlYolu = Path.Combine(
                    appData,
                    RdbLocal.Checked ? "baglanti.xml" : "baglantisql.xml");
                string aktifDbPath = Path.Combine(appData, "Aktifdb.txt");

                // XML ve TXT kaydı
                XmlBaglantiService service = new XmlBaglantiService();
                service.SaveServerAyar(xmlYolu, model);
                File.WriteAllText(aktifDbPath, Secim);

                // Kaydin gercekten yazildigini dogrula.
                string startupSqlXml = Path.Combine(Application.StartupPath, "Baglantisql.xml");
                string startupLocalXml = Path.Combine(Application.StartupPath, "baglanti.xml");
                string kontrolPath = RdbLocal.Checked ? startupLocalXml : startupSqlXml;
                string kayitliDb = XmlDbAdiniOku(kontrolPath);
                string beklenenDb = (model.VeritabaniAdi ?? string.Empty).Trim();

                if (!string.Equals(kayitliDb, beklenenDb, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "XML kaydı doğrulanamadı.\nBeklenen DB: " + beklenenDb + "\nYazılan DB: " + kayitliDb + "\nDosya: " + kontrolPath,
                        "Kayıt Doğrulama Hatası",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show(
                    "Bağlantı başarıyla kaydedildi.\nDB: " + beklenenDb + "\nDosya: " + kontrolPath,
                    "Başarılı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Formu başarılı kapat
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Beklenmedik bir hata oluştu: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

    }
}