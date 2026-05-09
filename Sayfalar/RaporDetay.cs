using Kolera_Mtsk.Services;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class RaporDetay : Form
    {
        private readonly string _connectionString;
        private readonly string _raporGrupPrefix;
        private readonly int _kayitId;
        private readonly string _kaynakTipi;
        private readonly string _seciliAdSoyad;
        private readonly WebView2 _onizlemeWebView;
        private bool _onizlemePdfZoomKancasi;

        /// <summary>WebView2 PDF onizleme yaklastirma (1.0 = %100).</summary>
        private const double OnizlemePdfZoomFactor = 1.5;

        public RaporDetay(string connectionString, string raporGrupPrefix, int kayitId = 0, string kaynakTipi = "", string seciliAdSoyad = "")
        {
            InitializeComponent();
            _connectionString = connectionString ?? string.Empty;
            _raporGrupPrefix = (raporGrupPrefix ?? string.Empty).Trim();
            _kayitId = kayitId;
            _kaynakTipi = (kaynakTipi ?? string.Empty).Trim().ToUpperInvariant();
            _seciliAdSoyad = (seciliAdSoyad ?? string.Empty).Trim();
            _onizlemeWebView = new WebView2
            {
                Dock = DockStyle.Fill,
                Visible = true
            };

            Load += RaporDetay_Load;
            Dgv_Raporlar.SelectionChanged += Dgv_Raporlar_SelectionChanged;
            Dgv_Raporlar.CellDoubleClick += Dgv_Raporlar_CellDoubleClick;
            Btn_Onizle.Click += Btn_Onizle_Click;
            Btn_Yazdir.Click += Btn_Yazdir_Click;
            Btn_OnizleAlt.Click += Btn_Onizle_Click;
            Btn_YazdirAlt.Click += Btn_Yazdir_Click;
            Btn_DuzenleAlt.Click += Btn_DuzenleAlt_Click;
            Btn_PdfSag.Click += Btn_PdfSag_Click;
            Btn_XlsSag.Click += Btn_XlsSag_Click;
            Btn_Doc.Click += Btn_Doc_Click;
            Btn_Jpg.Click += Btn_Jpg_Click;
            Btn_Html.Click += Btn_Html_Click;
        }

        private async void RaporDetay_Load(object sender, EventArgs e)
        {
            Text = "RAPOR - " + (_raporGrupPrefix.Length == 0 ? "FORMLAR" : _raporGrupPrefix + " FORMLARI");
            Lbl_SeciliKayit.Text = BaslikAdiniBul().ToUpperInvariant();
            WindowState = FormWindowState.Maximized;
            OnizlemePaneliHazirla();
            RaporListele();
            GridAyarla();
            GuncelleDurum();
            try
            {
                await _onizlemeWebView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "PDF onizleme icin WebView2 calisma zamani gerekli.\n\n" + ex.Message,
                    "Onizleme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private string BaslikAdiniBul()
        {
            if (!string.IsNullOrWhiteSpace(_seciliAdSoyad))
                return _seciliAdSoyad;

            if (_kayitId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return "SECILI KAYIT";

            string table = _kaynakTipi == "PERSONEL" ? "PERSONEL" : (_kaynakTipi == "KURSIYER" ? "KURSIYER" : "");
            if (string.IsNullOrWhiteSpace(table))
                return "SECILI KAYIT";

            string sql = "SELECT TOP 1 ISNULL(ADI,'') + ' ' + ISNULL(SOYADI,'') FROM [" + table + "] WHERE ID=@ID;";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = _kayitId;
                    con.Open();
                    var o = cmd.ExecuteScalar();
                    var adSoyad = Convert.ToString(o) ?? string.Empty;
                    adSoyad = adSoyad.Trim();
                    return adSoyad.Length == 0 ? "SECILI KAYIT" : adSoyad;
                }
            }
            catch
            {
                return "SECILI KAYIT";
            }
        }

        private void OnizlemePaneliHazirla()
        {
            if (_onizlemeWebView.Parent != null)
                _onizlemeWebView.Parent.Controls.Remove(_onizlemeWebView);

            _onizlemeWebView.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(_onizlemeWebView);
            if (panelSag.Parent != splitContainer1.Panel2)
                splitContainer1.Panel2.Controls.Add(panelSag);
            panelSag.Dock = DockStyle.Right;
            panelSag.BringToFront();
        }

        private void RaporListele()
        {
            Dgv_Raporlar.DataSource = RaporlariGetir();
        }

        private DataTable RaporlariGetir()
        {
            var dt = BosRaporTablosu();
            if (string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            string filtrePrefix = GetListePrefix();
            string filtrePrefixNorm = NormalizeRaporGrubu(filtrePrefix);
            string filtrePrefixAltNorm = GetAlternatifPrefixNorm(filtrePrefixNorm);

            const string sql = @"
SELECT
    ID,
    ISNULL(RAPOR_GRUBU, '') AS RAPOR_GRUBU,
    ISNULL(RAPOR_ADI, '') AS RAPOR_ADI,
    ISNULL(RAPOR_YOLU, '') AS RAPOR_YOLU,
    ISNULL(SIRA_NO, 0) AS SIRA_NO,
    ISNULL(AKTIF, 1) AS AKTIF,
    ISNULL(RENK, 0) AS RENK,
    SABLON_BINARY
FROM RAPOR_TANIMLARI
WHERE ISNULL(AKTIF, 1) = 1
  AND
  (
    @PREFIX_NORM = ''
    OR REPLACE(REPLACE(REPLACE(REPLACE(UPPER(ISNULL(RAPOR_GRUBU, '')), 'İ', 'I'), ' ', ''), '-', ''), '_', '') LIKE @PREFIX_NORM + '%'
    OR (@PREFIX_ALT_NORM <> '' AND REPLACE(REPLACE(REPLACE(REPLACE(UPPER(ISNULL(RAPOR_GRUBU, '')), 'İ', 'I'), ' ', ''), '-', ''), '_', '') LIKE @PREFIX_ALT_NORM + '%')
  )
ORDER BY ISNULL(SIRA_NO, 0), RAPOR_ADI;";

            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(sql, con))
                {
                    da.SelectCommand.Parameters.Add("@PREFIX_NORM", SqlDbType.NVarChar, 200).Value = filtrePrefixNorm;
                    da.SelectCommand.Parameters.Add("@PREFIX_ALT_NORM", SqlDbType.NVarChar, 200).Value = filtrePrefixAltNorm;
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor listesi yuklenemedi: " + ex.Message);
            }

            return dt;
        }

        private string GetListePrefix()
        {
            if (string.Equals(_kaynakTipi, "PERSONEL", StringComparison.OrdinalIgnoreCase))
                return "PERSONEL";

            if (string.Equals(_kaynakTipi, "KURSIYER", StringComparison.OrdinalIgnoreCase))
                return "KURSIYER";

            if (string.Equals(_kaynakTipi, "SINAV", StringComparison.OrdinalIgnoreCase))
                return "SINAV";

            return _raporGrupPrefix;
        }

        private static string NormalizeRaporGrubu(string value)
        {
            string s = (value ?? string.Empty).Trim().ToUpperInvariant();
            s = s.Replace('İ', 'I')
                 .Replace(" ", string.Empty)
                 .Replace("-", string.Empty)
                 .Replace("_", string.Empty);
            return s;
        }

        private static string GetAlternatifPrefixNorm(string normalizedPrefix)
        {
            return string.Empty;
        }

        private static DataTable BosRaporTablosu()
        {
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("RAPOR_GRUBU", typeof(string));
            dt.Columns.Add("RAPOR_ADI", typeof(string));
            dt.Columns.Add("RAPOR_YOLU", typeof(string));
            dt.Columns.Add("SIRA_NO", typeof(int));
            dt.Columns.Add("AKTIF", typeof(bool));
            dt.Columns.Add("RENK", typeof(int));
            dt.Columns.Add("SABLON_BINARY", typeof(byte[]));
            return dt;
        }

        private void GridAyarla()
        {
            Dgv_Raporlar.AutoGenerateColumns = true;
            Dgv_Raporlar.ReadOnly = true;
            Dgv_Raporlar.MultiSelect = false;
            Dgv_Raporlar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dgv_Raporlar.RowHeadersVisible = false;
            Dgv_Raporlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            Dgv_Raporlar.EnableHeadersVisualStyles = false;
            Dgv_Raporlar.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
            Dgv_Raporlar.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            Dgv_Raporlar.BackgroundColor = Color.White;
            Dgv_Raporlar.GridColor = Color.Silver;
            Dgv_Raporlar.RowTemplate.Height = 24;

            if (Dgv_Raporlar.Columns.Contains("ID"))
                Dgv_Raporlar.Columns["ID"].Visible = false;
            if (Dgv_Raporlar.Columns.Contains("SABLON_BINARY"))
                Dgv_Raporlar.Columns["SABLON_BINARY"].Visible = false;
            if (Dgv_Raporlar.Columns.Contains("RAPOR_GRUBU"))
                Dgv_Raporlar.Columns["RAPOR_GRUBU"].Visible = false;
            if (Dgv_Raporlar.Columns.Contains("AKTIF"))
                Dgv_Raporlar.Columns["AKTIF"].Visible = false;
            if (Dgv_Raporlar.Columns.Contains("RAPOR_YOLU"))
                Dgv_Raporlar.Columns["RAPOR_YOLU"].Visible = false;
            if (Dgv_Raporlar.Columns.Contains("RAPOR_ADI"))
            {
                Dgv_Raporlar.Columns["RAPOR_ADI"].HeaderText = "Rapor Adı";
                Dgv_Raporlar.Columns["RAPOR_ADI"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                Dgv_Raporlar.Columns["RAPOR_ADI"].DisplayIndex = 1;
            }
            if (Dgv_Raporlar.Columns.Contains("SIRA_NO"))
            {
                Dgv_Raporlar.Columns["SIRA_NO"].HeaderText = "Sıra";
                Dgv_Raporlar.Columns["SIRA_NO"].Width = 46;
                Dgv_Raporlar.Columns["SIRA_NO"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Dgv_Raporlar.Columns["SIRA_NO"].DisplayIndex = 0;
            }
            if (Dgv_Raporlar.Columns.Contains("RENK"))
                Dgv_Raporlar.Columns["RENK"].Visible = false;
        }

        private void Dgv_Raporlar_SelectionChanged(object sender, EventArgs e)
        {
            GuncelleDurum();
        }

        private void GuncelleDurum()
        {
            if (Dgv_Raporlar.CurrentRow == null)
            {
                Lbl_Durum.Text = "Rapor seciniz";
                return;
            }

            string raporAdi = Convert.ToString(Dgv_Raporlar.CurrentRow.Cells["RAPOR_ADI"]?.Value);
            Lbl_Durum.Text = string.IsNullOrWhiteSpace(raporAdi) ? "Rapor seciniz" : "Secili: " + raporAdi;
        }

        private string SeciliRaporYolu()
        {
            if (Dgv_Raporlar.CurrentRow == null)
                return string.Empty;
            return Convert.ToString(Dgv_Raporlar.CurrentRow.Cells["RAPOR_YOLU"]?.Value) ?? string.Empty;
        }

        private string SeciliRaporDosyasiniHazirla()
        {
            if (Dgv_Raporlar.CurrentRow == null)
                return string.Empty;

            string raporYolu = SeciliRaporYolu();
            string mevcut = RaporDosyasiniBul(raporYolu);
            if (!string.IsNullOrWhiteSpace(mevcut))
                return mevcut;

            byte[] binary = Dgv_Raporlar.CurrentRow.Cells["SABLON_BINARY"]?.Value as byte[];
            if (binary == null || binary.Length == 0)
                return string.Empty;

            string dosyaAdi = string.Empty;
            try { dosyaAdi = Path.GetFileName(raporYolu); } catch { dosyaAdi = string.Empty; }
            if (string.IsNullOrWhiteSpace(dosyaAdi))
                dosyaAdi = "rapor_detay_" + DateTime.Now.Ticks + ".frx";
            if (!dosyaAdi.EndsWith(".frx", StringComparison.OrdinalIgnoreCase))
                dosyaAdi += ".frx";

            string tempYol = Path.Combine(Path.GetTempPath(), "KoleraRaporOnizleme");
            Directory.CreateDirectory(tempYol);
            string hedef = Path.Combine(tempYol, dosyaAdi);
            File.WriteAllBytes(hedef, binary);
            return hedef;
        }

        private static string RaporDosyasiniBul(string raporYolu)
        {
            if (!string.IsNullOrWhiteSpace(raporYolu) && File.Exists(raporYolu))
                return raporYolu;

            string dosyaAdi = string.Empty;
            try { dosyaAdi = Path.GetFileName(raporYolu); } catch { dosyaAdi = string.Empty; }
            if (string.IsNullOrWhiteSpace(dosyaAdi))
                return string.Empty;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? string.Empty;
            string[] adaylar =
            {
                Path.Combine(@"C:\Raporlar", dosyaAdi),
                Path.Combine(Application.StartupPath, "Raporlar", dosyaAdi),
                Path.Combine(Application.StartupPath, dosyaAdi),
                Path.Combine(baseDir, "Raporlar", dosyaAdi),
                Path.Combine(baseDir, dosyaAdi)
            };

            foreach (string aday in adaylar)
            {
                if (File.Exists(aday))
                    return aday;
            }

            return string.Empty;
        }

        private async void Btn_Onizle_Click(object sender, EventArgs e)
        {
            await SeciliRaporuOnizleAsync();
        }

        private async void Dgv_Raporlar_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            DataGridViewRow row = Dgv_Raporlar.Rows[e.RowIndex];
            if (row.IsNewRow)
                return;

            Dgv_Raporlar.ClearSelection();
            row.Selected = true;
            if (e.ColumnIndex >= 0 && e.ColumnIndex < row.Cells.Count)
                Dgv_Raporlar.CurrentCell = row.Cells[e.ColumnIndex];
            else if (row.Cells.Count > 0)
                Dgv_Raporlar.CurrentCell = row.Cells[0];

            await SeciliRaporuOnizleAsync();
        }

        private async System.Threading.Tasks.Task SeciliRaporuOnizleAsync()
        {
            string yol = SeciliRaporDosyasiniHazirla();
            if (string.IsNullOrWhiteSpace(yol) || !File.Exists(yol))
            {
                MessageBox.Show(
                    "Rapor dosyasi bulunamadi.\n"
                    + "RAPOR_YOLU tam yol olmali, veya dosya C:\\Raporlar / uygulama\\Raporlar altinda olmali,\n"
                    + "veya SABLON_BINARY dolu olmali.",
                    "Onizleme",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(Path.GetExtension(yol), ".frx", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = yol, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Dosya acilamadi: " + ex.Message);
                }
                return;
            }

            string pdf = FrxToPdf(yol);
            if (string.IsNullOrWhiteSpace(pdf) || !File.Exists(pdf))
            {
                MessageBox.Show("PDF uretilemedi.");
                return;
            }

            OnizlemePaneliHazirla();
            if (_onizlemeWebView.CoreWebView2 == null)
            {
                try
                {
                    await _onizlemeWebView.EnsureCoreWebView2Async();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("WebView2 baslatilamadi: " + ex.Message);
                    return;
                }
            }

            OnizlemePdfZoomKancasiniBagla();
            string uri = new Uri(Path.GetFullPath(pdf)).AbsoluteUri;
            _onizlemeWebView.CoreWebView2.Navigate(uri);
        }

        private void OnizlemePdfZoomKancasiniBagla()
        {
            if (_onizlemePdfZoomKancasi || _onizlemeWebView?.CoreWebView2 == null)
                return;
            _onizlemeWebView.CoreWebView2.NavigationCompleted += OnizlemeWebView_NavigationCompleted;
            _onizlemePdfZoomKancasi = true;
        }

        private void OnizlemeWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
                return;
            try
            {
                double z = OnizlemePdfZoomFactor;
                if (z < 0.25)
                    z = 0.25;
                if (z > 4.0)
                    z = 4.0;
                _onizlemeWebView.ZoomFactor = z;
            }
            catch
            {
                // WebView2 surumune bagli; yoksay
            }
        }

        private void Btn_Yazdir_Click(object sender, EventArgs e)
        {
            string yol = SeciliRaporDosyasiniHazirla();
            if (string.IsNullOrWhiteSpace(yol) || !File.Exists(yol))
            {
                MessageBox.Show("Rapor yok.");
                return;
            }

            try
            {
                if (string.Equals(Path.GetExtension(yol), ".frx", StringComparison.OrdinalIgnoreCase))
                {
                    string pdf = FrxToPdf(yol);
                    if (string.IsNullOrWhiteSpace(pdf) || !File.Exists(pdf))
                    {
                        MessageBox.Show("Yazdirma icin PDF uretilemedi.");
                        return;
                    }

                    Process.Start(new ProcessStartInfo { FileName = pdf, Verb = "print", UseShellExecute = true });
                    return;
                }

                Process.Start(new ProcessStartInfo { FileName = yol, Verb = "print", UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yazdirma hatasi: " + ex.Message);
            }
        }

        private void Btn_DuzenleAlt_Click(object sender, EventArgs e)
        {
            string yol = SeciliRaporDosyasiniHazirla();
            if (string.IsNullOrWhiteSpace(yol) || !File.Exists(yol))
            {
                MessageBox.Show("Duzenlenecek rapor dosyasi bulunamadi.");
                return;
            }

            if (!string.Equals(Path.GetExtension(yol), ".frx", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Sadece FRX dosyalari FastReport Designer ile duzenlenebilir.");
                return;
            }

            if (!FastReportDesignerLauncher.TryOpenFrx(yol))
            {
                MessageBox.Show(
                    "FastReport Designer acilamadi.\n\n" +
                    "FastReport Designer Community Edition'i (GitHub Releases) indirip Designer.exe dosyasini " +
                    "uygulama klasorundeki FastReport klasorune veya exe'nin yanina koyun; " +
                    "App.config icindeki FastReportDesignerPath tam yolu da kullanilabilir.");
            }
        }

        private void Btn_XlsSag_Click(object sender, EventArgs e)
        {
            RaporuDisaAktar("XLS");
        }

        private void Btn_PdfSag_Click(object sender, EventArgs e)
        {
            RaporuDisaAktar("PDF");
        }

        private void Btn_Doc_Click(object sender, EventArgs e)
        {
            RaporuDisaAktar("DOC");
        }

        private void Btn_Jpg_Click(object sender, EventArgs e)
        {
            RaporuDisaAktar("JPG");
        }

        private void Btn_Html_Click(object sender, EventArgs e)
        {
            RaporuDisaAktar("HTML");
        }

        private void RaporuDisaAktar(string format)
        {
            string yol = SeciliRaporDosyasiniHazirla();
            if (string.IsNullOrWhiteSpace(yol) || !File.Exists(yol))
            {
                MessageBox.Show("Rapor dosyasi bulunamadi.");
                return;
            }

            if (!string.Equals(Path.GetExtension(yol), ".frx", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Export islemi yalnizca FRX raporlarda destekleniyor.");
                return;
            }

            string defaultExt = GetDefaultExt(format);
            string filter = GetSaveFilter(format);
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = format + " olarak kaydet";
                sfd.Filter = filter;
                sfd.DefaultExt = defaultExt;
                sfd.FileName = Path.GetFileNameWithoutExtension(yol) + "." + defaultExt;
                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                if (!TryExportFrxToFormat(yol, sfd.FileName, format, out string hata))
                {
                    MessageBox.Show(hata, "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Export tamamlandi:\n" + sfd.FileName, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool TryExportFrxToFormat(string frxYolu, string hedefDosya, string format, out string hata)
        {
            hata = string.Empty;
            try
            {
                if (string.Equals(format, "PDF", StringComparison.OrdinalIgnoreCase))
                {
                    string pdf = FrxToPdf(frxYolu);
                    if (string.IsNullOrWhiteSpace(pdf) || !File.Exists(pdf))
                    {
                        hata = "PDF uretilemedi.";
                        return false;
                    }
                    File.Copy(pdf, hedefDosya, true);
                    return true;
                }

                string temizFrxYolu = FrxIceriginiTemizleyipHazirla(frxYolu);
                using (var report = new Report())
                {
                    report.Load(temizFrxYolu);
                    RaporIciScriptBaglantilariniTemizle(report);
                    VeriBagla(report);
                    report.Prepare();

                    object exporter = CreateExporter(format);
                    if (exporter == null)
                    {
                        hata = format + " export modulu bulunamadi. Bu format icin FastReport export paketi yuklenmeli.";
                        return false;
                    }

                    ConfigureExporter(exporter, format);
                    var exportMethod = typeof(Report).GetMethod("Export", new[] { exporter.GetType(), typeof(string) });
                    if (exportMethod != null)
                    {
                        exportMethod.Invoke(report, new[] { exporter, hedefDosya });
                    }
                    else
                    {
                        MethodInfo genericExport = null;
                        foreach (var m in typeof(Report).GetMethods())
                        {
                            if (m.Name != "Export")
                                continue;
                            var ps = m.GetParameters();
                            if (ps.Length == 2 && ps[1].ParameterType == typeof(string))
                            {
                                if (ps[0].ParameterType.IsAssignableFrom(exporter.GetType()))
                                {
                                    genericExport = m;
                                    break;
                                }
                            }
                        }

                        if (genericExport == null)
                        {
                            hata = "Rapor export metodu bulunamadi.";
                            return false;
                        }

                        genericExport.Invoke(report, new[] { exporter, hedefDosya });
                    }

                    (exporter as IDisposable)?.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                hata = "Export hatasi: " + ex.Message;
                return false;
            }
        }

        private static string GetDefaultExt(string format)
        {
            switch ((format ?? string.Empty).ToUpperInvariant())
            {
                case "HTML": return "html";
                case "JPG": return "jpg";
                case "DOC": return "rtf";
                case "XLS": return "xlsx";
                case "PDF":
                default: return "pdf";
            }
        }

        private static string GetSaveFilter(string format)
        {
            switch ((format ?? string.Empty).ToUpperInvariant())
            {
                case "HTML": return "HTML (*.html)|*.html";
                case "JPG": return "JPEG (*.jpg)|*.jpg";
                case "DOC": return "RTF (*.rtf)|*.rtf";
                case "XLS": return "Excel (*.xlsx)|*.xlsx";
                case "PDF":
                default: return "PDF (*.pdf)|*.pdf";
            }
        }

        private static object CreateExporter(string format)
        {
            string key = (format ?? string.Empty).ToUpperInvariant();
            string[] typeNames;
            switch (key)
            {
                case "HTML":
                    typeNames = new[] { "FastReport.Export.Html.HTMLExport" };
                    break;
                case "JPG":
                    typeNames = new[] { "FastReport.Export.Image.ImageExport" };
                    break;
                case "DOC":
                    typeNames = new[] { "FastReport.Export.RichText.RTFExport" };
                    break;
                case "XLS":
                    typeNames = new[] { "FastReport.Export.OoXML.Excel2007Export", "FastReport.Export.Xml.XMLExport" };
                    break;
                case "PDF":
                    typeNames = new[] { "FastReport.Export.PdfSimple.PDFSimpleExport" };
                    break;
                default:
                    return null;
            }

            foreach (string t in typeNames)
            {
                var type = FindLoadedType(t);
                if (type != null)
                    return Activator.CreateInstance(type);
            }
            return null;
        }

        private static Type FindLoadedType(string fullTypeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = null;
                try { t = asm.GetType(fullTypeName, false); } catch { }
                if (t != null)
                    return t;
            }
            return null;
        }

        private static void ConfigureExporter(object exporter, string format)
        {
            if (exporter == null)
                return;

            if (string.Equals(format, "JPG", StringComparison.OrdinalIgnoreCase))
            {
                var type = exporter.GetType();
                var prop = type.GetProperty("ImageFormat");
                if (prop != null && prop.CanWrite)
                {
                    var enumType = prop.PropertyType;
                    try
                    {
                        var jpegValue = Enum.Parse(enumType, "Jpeg", true);
                        prop.SetValue(exporter, jpegValue, null);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

        private string FrxToPdf(string frxYolu)
        {
            try
            {
                string pdfPath = Path.Combine(Path.GetTempPath(), "rapor_detay_" + DateTime.Now.Ticks + ".pdf");
                string temizFrxYolu = FrxIceriginiTemizleyipHazirla(frxYolu);
                using (var report = new Report())
                using (var pdf = new PDFSimpleExport())
                {
                    report.Load(temizFrxYolu);
                    RaporIciScriptBaglantilariniTemizle(report);
                    VeriBagla(report);
                    report.Prepare();
                    report.Export(pdf, pdfPath);
                }
                return pdfPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("FastReport hatasi: " + ex.Message);
                return string.Empty;
            }
        }

        private void VeriBagla(Report report)
        {
            var kurs = GetKursData();
            report.RegisterData(kurs, "KURS");

            if (_kaynakTipi == "PERSONEL")
            {
                var personel = GetTekKayit("PERSONEL", _kayitId);
                report.RegisterData(personel, "PERSONEL");
            }
            else if (_kaynakTipi == "KURSIYER")
            {
                var kursiyer = GetTekKayit("KURSIYER", _kayitId);
                KursiyerRaporKolonlariniGarantiEt(kursiyer);
                KursiyerBosDegerleriTamamla(kursiyer);
                report.RegisterData(kursiyer, "KURSIYER");
                report.RegisterData(kursiyer, "Kursiyer");
            }
            else if (_kaynakTipi == "SINAV")
            {
                var sinav = GetSinavData(_kayitId);
                report.RegisterData(sinav, "SINAV_LISTESI");
                report.RegisterData(sinav, "SINAV");

                var kursiyerListe = GetSinavRaporKursiyerData(_kayitId);
                KursiyerRaporKolonlariniGarantiEt(kursiyerListe);
                KursiyerBosDegerleriTamamla(kursiyerListe);
                report.RegisterData(kursiyerListe, "KURSIYER");
                report.RegisterData(kursiyerListe, "Kursiyer");
            }

            EnableReportDataSource(report, "KURS");
            EnableReportDataSource(report, "PERSONEL");
            EnableReportDataSource(report, "KURSIYER");
            EnableReportDataSource(report, "Kursiyer");
            EnableReportDataSource(report, "SINAV_LISTESI");
            EnableReportDataSource(report, "SINAV");

            KursRaporKursTablosu.ProgramatikTablolardaBaglantiyiYoksay(report);
        }

        private static void EnableReportDataSource(Report report, string dataSourceName)
        {
            if (report == null || string.IsNullOrWhiteSpace(dataSourceName))
                return;

            var ds = report.GetDataSource(dataSourceName);
            if (ds != null)
                ds.Enabled = true;
        }

        private DataTable GetTekKayit(string table, int id)
        {
            var dt = new DataTable(table);
            bool kursiyer = string.Equals(table, "KURSIYER", StringComparison.OrdinalIgnoreCase);
            bool personel = string.Equals(table, "PERSONEL", StringComparison.OrdinalIgnoreCase);

            if (!kursiyer && !personel)
            {
                if (id <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                    return dt;
            }
            else if (id <= 0 || string.IsNullOrWhiteSpace(_connectionString))
            {
                if (kursiyer)
                {
                    KursiyerRaporKolonlariniGarantiEt(dt);
                    KursiyerBosDegerleriTamamla(dt);
                }
                else if (personel)
                {
                    PersonelRaporKolonlariniGarantiEt(dt);
                    KursiyerBosDegerleriTamamla(dt);
                }
                return dt;
            }

            string sql = "SELECT TOP 1 * FROM [" + table + "] WHERE [ID]=@ID;";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(sql, con))
                {
                    da.SelectCommand.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    da.Fill(dt);
                }
            }
            catch
            {
                // sessiz gec
            }

            if (kursiyer)
            {
                KursiyerRaporKolonlariniGarantiEt(dt);
                KursiyerBosDegerleriTamamla(dt);
            }
            else if (personel)
            {
                PersonelRaporKolonlariniGarantiEt(dt);
                KursiyerBosDegerleriTamamla(dt);
            }

            return dt;
        }

        private DataTable GetSinavRaporKursiyerData(int anchorKursiyerId)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                var bos = new DataTable("KURSIYER");
                KursiyerRaporKolonlariniGarantiEt(bos);
                KursiyerBosDegerleriTamamla(bos);
                return bos;
            }

            // Direksiyon ekraninda grid ayni tarihte birden fazla SINAV_TARIHLERI.ID'den dolabilir.
            // Bu nedenle raporda once secili tarihe gore tum kayitlari toplu cekmeyi deneriz.
            DateTime seciliSinavTarihi;
            if (TryParseSinavTarihiFromBaslik(out seciliSinavTarihi))
            {
                var dTarih = KursiyerleriSinavTarihineGoreYukle(seciliSinavTarihi.Date, teoriListe: false);
                if (dTarih != null && dTarih.Rows.Count > 0)
                    return dTarih;

                var tTarih = KursiyerleriSinavTarihineGoreYukle(seciliSinavTarihi.Date, teoriListe: true);
                if (tTarih != null && tTarih.Rows.Count > 0)
                    return tTarih;
            }

            if (anchorKursiyerId > 0)
            {
                DateTime sinavIdTarihi;
                if (TryGetSinavTarihiBySinavId(anchorKursiyerId, out sinavIdTarihi))
                {
                    var dByIdTarih = KursiyerleriSinavTarihineGoreYukle(sinavIdTarihi.Date, teoriListe: false);
                    if (dByIdTarih != null && dByIdTarih.Rows.Count > 0)
                        return dByIdTarih;
                }

                var dirBySinavId = KursiyerleriSinavListesindenYukle(anchorKursiyerId, teoriListe: false);
                if (dirBySinavId != null && dirBySinavId.Rows.Count > 0)
                    return dirBySinavId;

                var teoBySinavId = KursiyerleriSinavListesindenYukle(anchorKursiyerId, teoriListe: true);
                if (teoBySinavId != null && teoBySinavId.Rows.Count > 0)
                    return teoBySinavId;

                int teorıSinavId = SinavTarihiIdBulTeori(anchorKursiyerId);
                if (teorıSinavId > 0)
                {
                    var t = KursiyerleriSinavListesindenYukle(teorıSinavId, teoriListe: true);
                    if (t != null && t.Rows.Count > 0)
                        return t;
                }

                int dirSinavId = SinavTarihiIdBulDireksiyon(anchorKursiyerId);
                if (dirSinavId > 0)
                {
                    var d = KursiyerleriSinavListesindenYukle(dirSinavId, teoriListe: false);
                    if (d != null && d.Rows.Count > 0)
                        return d;
                }
            }

            // SINAV raporunda tek kursiyere dusmek yerine bos tablo don.
            var bosTablo = new DataTable("KURSIYER");
            KursiyerRaporKolonlariniGarantiEt(bosTablo);
            return bosTablo;
        }

        private bool TryParseSinavTarihiFromBaslik(out DateTime tarih)
        {
            tarih = DateTime.MinValue;
            string text = (_seciliAdSoyad ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text))
                return false;

            int idx = text.IndexOf(':');
            string raw = idx >= 0 ? text.Substring(idx + 1).Trim() : text;

            return DateTime.TryParseExact(raw, "dd.MM.yyyy", CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out tarih)
                || DateTime.TryParse(raw, CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out tarih);
        }

        private DataTable KursiyerleriSinavTarihineGoreYukle(DateTime sinavTarihi, bool teoriListe)
        {
            string listeTablo = teoriListe ? "SINAV_LISTE_TEORI" : "SINAV_LISTE_DIREKSIYON";
            string alias = teoriListe ? "slt" : "sld";
            string listeAlanlari = teoriListe
                ? ", " + alias + ".TEO_HAK AS TEO_HAK, " + alias + ".TEO_NOT AS TEO_NOT, " + alias + ".TEO_DURUM AS TEO_DURUM"
                : ", " + alias + ".DIR_HAK AS TEO_HAK, " + alias + ".DIR_NOT AS TEO_NOT, " + alias + ".DIR_DURUM AS TEO_DURUM, ISNULL(" + alias + ".RANDEVU_SAATI, '') AS SINAV_SAATI";

            string[] kursTablolari = { "KURSIYER", "KURSIYERLER" };
            foreach (string kt in kursTablolari)
            {
                string sql = @"
SELECT k.*" + listeAlanlari + @"
FROM [" + listeTablo + @"] " + alias + @"
INNER JOIN SINAV_TARIHLERI st ON st.ID = " + alias + @".ID_SINAV_TARIHI
INNER JOIN [" + kt + @"] k ON k.ID = " + alias + @".ID_KURSIYER
WHERE st.SINAV_TARIHI IS NOT NULL
  AND CAST(st.SINAV_TARIHI AS date) = @T
ORDER BY k.ADAY_NO, k.ID;";
                try
                {
                    var dt = new DataTable("KURSIYER");
                    using (var con = new SqlConnection(_connectionString))
                    using (var da = new SqlDataAdapter(sql, con))
                    {
                        da.SelectCommand.Parameters.Add("@T", SqlDbType.Date).Value = sinavTarihi.Date;
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                        return dt;
                }
                catch
                {
                }
            }

            return null;
        }

        private bool TryGetSinavTarihiBySinavId(int sinavId, out DateTime tarih)
        {
            tarih = DateTime.MinValue;
            if (sinavId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return false;

            const string sql = @"
SELECT TOP 1 st.SINAV_TARIHI
FROM SINAV_TARIHLERI st
WHERE st.ID = @ID
  AND st.SINAV_TARIHI IS NOT NULL;";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = sinavId;
                    con.Open();
                    object o = cmd.ExecuteScalar();
                    if (o == null || o == DBNull.Value)
                        return false;

                    tarih = Convert.ToDateTime(o);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private int SinavTarihiIdBulTeori(int kursiyerId)
        {
            const string sql = @"
SELECT TOP 1 slt.ID_SINAV_TARIHI
FROM SINAV_LISTE_TEORI slt
WHERE slt.ID_KURSIYER = @K
  AND slt.ID_SINAV_TARIHI IS NOT NULL
  AND slt.ID_SINAV_TARIHI > 0
ORDER BY slt.ID DESC;";
            return SqlScalarInt(sql, kursiyerId);
        }

        private int SinavTarihiIdBulDireksiyon(int kursiyerId)
        {
            const string sql = @"
SELECT TOP 1 sld.ID_SINAV_TARIHI
FROM SINAV_LISTE_DIREKSIYON sld
WHERE sld.ID_KURSIYER = @K
  AND sld.ID_SINAV_TARIHI IS NOT NULL
  AND sld.ID_SINAV_TARIHI > 0
ORDER BY sld.ID DESC;";
            return SqlScalarInt(sql, kursiyerId);
        }

        private int SqlScalarInt(string sql, int kursiyerId)
        {
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@K", SqlDbType.Int).Value = kursiyerId;
                    con.Open();
                    object o = cmd.ExecuteScalar();
                    if (o == null || o == DBNull.Value)
                        return 0;
                    int n;
                    return int.TryParse(Convert.ToString(o), out n) ? n : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private DataTable KursiyerleriSinavListesindenYukle(int idSinavTarihi, bool teoriListe)
        {
            string listeTablo = teoriListe ? "SINAV_LISTE_TEORI" : "SINAV_LISTE_DIREKSIYON";
            string alias = teoriListe ? "slt" : "sld";
            // Rapor sablonu E-SINAV icin TEO_* bekler; direksiyon listesinde DIR_* ayni isimle gelir.
            string listeAlanlari = teoriListe
                ? ", " + alias + ".TEO_HAK AS TEO_HAK, " + alias + ".TEO_NOT AS TEO_NOT, " + alias + ".TEO_DURUM AS TEO_DURUM"
                : ", " + alias + ".DIR_HAK AS TEO_HAK, " + alias + ".DIR_NOT AS TEO_NOT, " + alias + ".DIR_DURUM AS TEO_DURUM, ISNULL(" + alias + ".RANDEVU_SAATI, '') AS SINAV_SAATI";
            string[] kursTablolari = { "KURSIYER", "KURSIYERLER" };

            foreach (string kt in kursTablolari)
            {
                string sql = @"
SELECT k.*" + listeAlanlari + @"
FROM [" + listeTablo + @"] " + alias + @"
INNER JOIN [" + kt + @"] k ON k.ID = " + alias + @".ID_KURSIYER
WHERE " + alias + @".ID_SINAV_TARIHI = @S
ORDER BY k.ADAY_NO, k.ID;";
                try
                {
                    var t = new DataTable("KURSIYER");
                    using (var con = new SqlConnection(_connectionString))
                    using (var da = new SqlDataAdapter(sql, con))
                    {
                        da.SelectCommand.Parameters.Add("@S", SqlDbType.Int).Value = idSinavTarihi;
                        da.Fill(t);
                    }

                    t.TableName = "KURSIYER";
                    if (t.Rows.Count > 0)
                        return t;
                }
                catch
                {
                    // sonraki tablo adayi
                }
            }

            return null;
        }

        private static void KursiyerRaporKolonlariniGarantiEt(DataTable dt)
        {
            if (dt == null)
                return;

            EnsureColumn(dt, "ADI", typeof(string));
            EnsureColumn(dt, "SOYADI", typeof(string));
            EnsureColumn(dt, "TC_NO", typeof(string));
            EnsureColumn(dt, "KIMLIK_BABA_ADI", typeof(string));
            EnsureColumn(dt, "KIM_ANA_ADI", typeof(string));
            EnsureColumn(dt, "KIMLIK_DOGUM_YERI", typeof(string));
            EnsureColumn(dt, "DOGUM_TARIHI", typeof(DateTime));
            EnsureColumn(dt, "KIMLIK_KAYIT_NO", typeof(string));
            EnsureColumn(dt, "TAHSILI", typeof(string));
            EnsureColumn(dt, "EV_ADRESI", typeof(string));
            EnsureColumn(dt, "EV_TELEFON", typeof(string));
            EnsureColumn(dt, "GSM_1", typeof(string));
            EnsureColumn(dt, "SERTIFIKA_SINIFI", typeof(string));
            EnsureColumn(dt, "KAYIT_TARIHI", typeof(DateTime));
            EnsureColumn(dt, "ONCE_SERT_VER_IL", typeof(string));
            EnsureColumn(dt, "ONCE_SERT_SINIFI", typeof(string));
            EnsureColumn(dt, "ONCE_SERT_VER_TAR", typeof(DateTime));
            EnsureColumn(dt, "ONCE_SERT_BELGESAYI", typeof(string));
            EnsureColumn(dt, "RESIM", typeof(byte[]));
            EnsureColumn(dt, "ADAY_NO", typeof(string));
            EnsureColumn(dt, "IS_ADRESI", typeof(string));
            EnsureColumn(dt, "IS_TELEFON_1", typeof(string));
            EnsureColumn(dt, "GSM_2", typeof(string));
            EnsureColumn(dt, "TEO_HAK", typeof(int));
            EnsureColumn(dt, "TEO_NOT", typeof(string));
            EnsureColumn(dt, "TEO_DURUM", typeof(string));
            EnsureColumn(dt, "SINAV_SAATI", typeof(string));
        }

        private static void PersonelRaporKolonlariniGarantiEt(DataTable dt)
        {
            if (dt == null)
                return;

            EnsureColumn(dt, "ADI", typeof(string));
            EnsureColumn(dt, "SOYADI", typeof(string));
            EnsureColumn(dt, "TC_NO", typeof(string));
            EnsureColumn(dt, "RESIM", typeof(byte[]));
            EnsureColumn(dt, "DOGUM_TARIHI", typeof(DateTime));
            EnsureColumn(dt, "PERSONEL_DURUMU", typeof(string));
            EnsureColumn(dt, "CINSIYET", typeof(string));
            EnsureColumn(dt, "MEDENI_DUR", typeof(string));
            EnsureColumn(dt, "YONETICI_GOREVI", typeof(string));
            EnsureColumn(dt, "VERDIGI_DERS_1", typeof(string));
            EnsureColumn(dt, "EHLIYET_SINIFI", typeof(string));
            EnsureColumn(dt, "EHLIYET_IKINCI", typeof(string));
            EnsureColumn(dt, "SOZ_BASLAMA_TAR", typeof(DateTime));
            EnsureColumn(dt, "EV_ADRESI", typeof(string));
            EnsureColumn(dt, "GSM_1", typeof(string));
            EnsureColumn(dt, "KIM_BABA_ADI", typeof(string));
            EnsureColumn(dt, "KIM_ANA_ADI", typeof(string));
            EnsureColumn(dt, "KIM_DOGUM_YERI", typeof(string));
        }

        private static void EnsureColumn(DataTable dt, string columnName, Type columnType)
        {
            if (dt == null || string.IsNullOrWhiteSpace(columnName) || columnType == null)
                return;

            if (!dt.Columns.Contains(columnName))
                dt.Columns.Add(columnName, columnType);
        }

        private static void KursiyerBosDegerleriTamamla(DataTable dt)
        {
            if (dt == null)
                return;

            if (dt.Rows.Count == 0)
            {
                var bos = dt.NewRow();
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.DataType == typeof(string))
                        bos[col.ColumnName] = string.Empty;
                    else
                        bos[col.ColumnName] = DBNull.Value;
                }
                dt.Rows.Add(bos);
                return;
            }

            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.DataType == typeof(string) && row[col.ColumnName] == DBNull.Value)
                        row[col.ColumnName] = string.Empty;
                }
            }
        }

        private DataTable GetSinavData(int id)
        {
            var dt = new DataTable("SINAV_LISTESI");
            if (id <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            const string sqlBySinav = @"
SELECT TOP 50 *
FROM SINAV_LISTE_DIREKSIYON
WHERE ID_SINAV_TARIHI=@ID
ORDER BY ID DESC;";
            const string sqlByKursiyer = @"
SELECT TOP 50 *
FROM SINAV_LISTE_DIREKSIYON
WHERE ID_KURSIYER=@ID
ORDER BY ID DESC;";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    using (var da = new SqlDataAdapter(sqlBySinav, con))
                    {
                        da.SelectCommand.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count == 0)
                    {
                        using (var da = new SqlDataAdapter(sqlByKursiyer, con))
                        {
                            da.SelectCommand.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                            da.Fill(dt);
                        }
                    }
                }
            }
            catch
            {
                // sessiz gec
            }
            return dt;
        }

        private DataTable GetKursData()
        {
            return KursRaporKursTablosu.Olustur(_connectionString);
        }

        private static void RaporIciScriptBaglantilariniTemizle(Report report)
        {
            if (report == null)
                return;

            try
            {
                report.ScriptText = MinimalFastReportScript();
            }
            catch
            {
                // ignore
            }

            try
            {
                report.Dictionary.Connections.Clear();
            }
            catch
            {
                // ignore
            }
        }

        private static string MinimalFastReportScript()
        {
            return "using System;\r\n"
                 + "namespace FastReport\r\n"
                 + "{\r\n"
                 + "  public class ReportScript\r\n"
                 + "  {\r\n"
                 + "  }\r\n"
                 + "}";
        }

        private static string FrxIceriginiTemizleyipHazirla(string frxYolu)
        {
            if (string.IsNullOrWhiteSpace(frxYolu) || !File.Exists(frxYolu))
                return frxYolu;

            string tempYol = Path.Combine(Path.GetTempPath(), "KoleraRaporOnizleme");
            Directory.CreateDirectory(tempYol);

            string temizFrxYolu = Path.Combine(
                tempYol,
                Path.GetFileNameWithoutExtension(frxYolu) + "_clean_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".frx");

            var doc = new XmlDocument();
            doc.Load(frxYolu);

            var scriptNode = doc.SelectSingleNode("/Report/ScriptText");
            if (scriptNode != null)
                scriptNode.InnerText = MinimalFastReportScript();

            var dictionaryNode = doc.SelectSingleNode("/Report/Dictionary");
            if (dictionaryNode != null)
            {
                for (int i = dictionaryNode.ChildNodes.Count - 1; i >= 0; i--)
                {
                    var child = dictionaryNode.ChildNodes[i];
                    if (child == null)
                        continue;

                    if (string.Equals(child.Name, "MsSqlDataConnection", StringComparison.OrdinalIgnoreCase))
                        dictionaryNode.RemoveChild(child);
                }
            }

            // Eski/harici FRX dosyalarinda SINAV kaynagi bazen tanimli olmayabiliyor.
            // Bu durumda [SINAV.SINAV_TARIHI] derleme hatasi uretir (CS0103).
            // Tum raporlarda bu ifadeyi guvenli alan olan KURS.RAPOR_TARIHI'ne normalize et.
            var textNodes = doc.SelectNodes("//*[@Text]");
            if (textNodes != null)
            {
                foreach (XmlNode node in textNodes)
                {
                    var attr = node.Attributes == null ? null : node.Attributes["Text"];
                    if (attr == null || string.IsNullOrWhiteSpace(attr.Value))
                        continue;

                    if (attr.Value.IndexOf("[SINAV.SINAV_TARIHI]", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        attr.Value = attr.Value.Replace("[SINAV.SINAV_TARIHI]", "[KURS.RAPOR_TARIHI]");
                        attr.Value = attr.Value.Replace("[sinav.sinav_tarihi]", "[KURS.RAPOR_TARIHI]");
                    }
                }
            }

            doc.Save(temizFrxYolu);
            return temizFrxYolu;
        }


    }
}
