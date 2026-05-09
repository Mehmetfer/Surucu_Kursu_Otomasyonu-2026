using Kolera.Arama;
using Kolera_Mtsk.Services;
using FastReport.Export.PdfSimple;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Kursiyer_Detay_Raporlar : Form
    {
        private readonly string _connectionString;
        private readonly Arama_Model _kursiyer;

        public Kursiyer_Detay_Raporlar()
        {
            InitializeComponent();
            DgvGorunumuHazirla();
        }

        public Kursiyer_Detay_Raporlar(string connectionString, Arama_Model kursiyer)
            : this()
        {
            _connectionString = connectionString;
            _kursiyer = kursiyer;
        }

        private void KursiyerRaporSecimForm_Load(object sender, EventArgs e)
        {
            RaporlariYenile();
        }

        private void BtnYenile_Click(object sender, EventArgs e)
        {
            RaporlariYenile();
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (_dgv.CurrentRow == null)
            {
                _lblBilgi.Text = "Kursiyer raporu seciniz.";
                return;
            }

            string raporAdi = Convert.ToString(_dgv.CurrentRow.Cells["RAPOR_ADI"]?.Value) ?? string.Empty;
            _lblBilgi.Text = string.IsNullOrWhiteSpace(raporAdi) ? "Kursiyer raporu seciniz." : "Secili: " + raporAdi;
        }

        private void DgvGorunumuHazirla()
        {
            _dgv.AutoGenerateColumns = false;
            _dgv.Columns.Clear();

            var colSira = new DataGridViewTextBoxColumn
            {
                Name = "SIRA_NO",
                DataPropertyName = "SIRA_NO",
                HeaderText = "Sira",
                Width = 55,
                MinimumWidth = 45,
                FillWeight = 15f,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };

            var colRenk = new DataGridViewTextBoxColumn
            {
                Name = "RENK",
                DataPropertyName = "RENK",
                HeaderText = "",
                Width = 40,
                MinimumWidth = 35,
                FillWeight = 12f
            };

            var colRaporAdi = new DataGridViewTextBoxColumn
            {
                Name = "RAPOR_ADI",
                DataPropertyName = "RAPOR_ADI",
                HeaderText = "Rapor Adi",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 73f
            };

            _dgv.Columns.Add(colSira);
            _dgv.Columns.Add(colRenk);
            _dgv.Columns.Add(colRaporAdi);

            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "ID", DataPropertyName = "ID", Visible = false });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "RAPOR_GRUBU", DataPropertyName = "RAPOR_GRUBU", Visible = false });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "RAPOR_YOLU", DataPropertyName = "RAPOR_YOLU", Visible = false });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "AKTIF", DataPropertyName = "AKTIF", Visible = false });
            _dgv.Columns.Add(new DataGridViewTextBoxColumn { Name = "SABLON_BINARY", DataPropertyName = "SABLON_BINARY", Visible = false });

            _dgv.RowTemplate.Height = 26;
            _dgv.EnableHeadersVisualStyles = false;
            _dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.Gainsboro;
            _dgv.ColumnHeadersDefaultCellStyle.Font = new Font(_dgv.Font, FontStyle.Bold);
            _dgv.CellPainting += Dgv_CellPainting;
        }

        private void Dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (!string.Equals(_dgv.Columns[e.ColumnIndex].Name, "RENK", StringComparison.Ordinal))
                return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

            Color kutuRenk = RenkDegerindenColor(e.Value);
            var kutu = new Rectangle(
                e.CellBounds.Left + (e.CellBounds.Width - 18) / 2,
                e.CellBounds.Top + (e.CellBounds.Height - 12) / 2,
                18,
                12);

            using (var brush = new SolidBrush(kutuRenk))
                e.Graphics.FillRectangle(brush, kutu);
            using (var pen = new Pen(Color.DimGray))
                e.Graphics.DrawRectangle(pen, kutu);

            e.Handled = true;
        }

        private static Color RenkDegerindenColor(object value)
        {
            int renkDegeri;
            if (!int.TryParse(Convert.ToString(value), out renkDegeri))
                return Color.Silver;

            if (renkDegeri <= 0)
                return Color.Silver;

            if (renkDegeri >= 0 && renkDegeri <= 15)
            {
                Color[] palet =
                {
                    Color.Silver, Color.Green, Color.Olive, Color.Red,
                    Color.Cyan, Color.Purple, Color.Blue, Color.Orange,
                    Color.Teal, Color.Brown, Color.DarkViolet, Color.Goldenrod,
                    Color.DarkSeaGreen, Color.Crimson, Color.SteelBlue, Color.Gray
                };
                return palet[renkDegeri];
            }

            try
            {
                return ColorTranslator.FromOle(renkDegeri);
            }
            catch
            {
                return Color.Silver;
            }
        }

        private void RaporlariYenile()
        {
            _dgv.DataSource = KursiyerRaporlariniGetir();
            if (_dgv.Columns.Contains("ID"))
                _dgv.Columns["ID"].Visible = false;
            if (_dgv.Columns.Contains("SABLON_BINARY"))
                _dgv.Columns["SABLON_BINARY"].Visible = false;
        }

        private DataTable KursiyerRaporlariniGetir()
        {
            var dt = BosRaporTablosu();
            if (string.IsNullOrWhiteSpace(_connectionString))
                return dt;

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
ORDER BY ISNULL(SIRA_NO, 0), RAPOR_ADI;";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }

                if (dt.Rows.Count == 0)
                    return dt;

                DataTable filtreli = dt.Clone();
                foreach (DataRow row in dt.Rows)
                {
                    if (!KursiyerRaporMu(row))
                        continue;

                    filtreli.ImportRow(row);
                }

                return filtreli;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kursiyer raporlari yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dt;
        }

        private static bool KursiyerRaporMu(DataRow row)
        {
            string grup = NormalizeAramaIcin(Convert.ToString(row["RAPOR_GRUBU"]));
            return string.Equals(grup, "KURSIYER FORMLARI", StringComparison.Ordinal);
        }

        private static string NormalizeAramaIcin(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string normalized = value.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD);
            var chars = new List<char>(normalized.Length);
            foreach (char c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    chars.Add(c);
            }

            return new string(chars.ToArray());
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

        private void BtnOnizle_Click(object sender, EventArgs e)
        {
            string dosya = HazirlaRaporDosyasi();
            if (string.IsNullOrWhiteSpace(dosya))
            {
                MessageBox.Show("Rapor dosyasi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (string.Equals(Path.GetExtension(dosya), ".frx", StringComparison.OrdinalIgnoreCase))
                {
                    string hataDetayi;
                    string preparedYol = FrxDosyasiniViewerIcinHazirla(dosya, out hataDetayi);
                    if (!string.IsNullOrWhiteSpace(preparedYol) && File.Exists(preparedYol) && FrxViewerAc(preparedYol))
                        return;

                    MessageBox.Show(
                        "Viewer onizleme acilamadi.\n\nDetay:\n" + (hataDetayi ?? "Bilinmeyen hata"),
                        "Onizleme Hatasi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Process.Start(new ProcessStartInfo { FileName = dosya, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Onizleme acilamadi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnYazdir_Click(object sender, EventArgs e)
        {
            string dosya = HazirlaRaporDosyasi();
            if (string.IsNullOrWhiteSpace(dosya))
            {
                MessageBox.Show("Rapor dosyasi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = dosya,
                    Verb = "print",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch
            {
                MessageBox.Show("Bu dosya turu icin dogrudan yazdirma desteklenmiyor. Once Onizle ile acabilirsiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDuzenle_Click(object sender, EventArgs e)
        {
            string dosya = HazirlaRaporDosyasi();
            if (string.IsNullOrWhiteSpace(dosya) || !File.Exists(dosya))
            {
                MessageBox.Show("Duzenlenecek rapor dosyasi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals(Path.GetExtension(dosya), ".frx", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Sadece FRX dosyalari tasarim ekraninda duzenlenebilir.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!FastReportDesignerLauncher.TryOpenFrx(dosya))
            {
                MessageBox.Show(
                    "FastReport Designer acilamadi.\n\n" +
                    "FastReport Designer Community Edition'i (GitHub Releases) indirip Designer.exe dosyasini " +
                    "uygulama klasorundeki FastReport klasorune veya exe'nin yanina koyun; " +
                    "App.config icindeki FastReportDesignerPath tam yolu da kullanilabilir.",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private string HazirlaRaporDosyasi()
        {
            if (_dgv.CurrentRow == null)
                return string.Empty;

            string raporYolu = Convert.ToString(_dgv.CurrentRow.Cells["RAPOR_YOLU"]?.Value) ?? string.Empty;
            byte[] binary = _dgv.CurrentRow.Cells["SABLON_BINARY"]?.Value as byte[];

            if (!string.IsNullOrWhiteSpace(raporYolu) && File.Exists(raporYolu))
                return raporYolu;

            string alternatifYol = AlternatifRaporYoluBul(raporYolu);
            if (!string.IsNullOrWhiteSpace(alternatifYol))
                return alternatifYol;

            if (binary == null || binary.Length == 0)
                return string.Empty;

            string dosyaAdi = Path.GetFileName(raporYolu);
            if (string.IsNullOrWhiteSpace(dosyaAdi))
                dosyaAdi = "kursiyer_rapor_" + DateTime.Now.Ticks + ".frx";

            string tempYol = Path.Combine(Path.GetTempPath(), "KoleraRaporOnizleme");
            Directory.CreateDirectory(tempYol);

            string hedef = Path.Combine(tempYol, dosyaAdi);
            File.WriteAllBytes(hedef, binary);
            return hedef;
        }

        private static string AlternatifRaporYoluBul(string raporYolu)
        {
            if (string.IsNullOrWhiteSpace(raporYolu))
                return string.Empty;

            string dosyaAdi = Path.GetFileName(raporYolu);
            if (string.IsNullOrWhiteSpace(dosyaAdi))
                return string.Empty;

            var adaylar = new List<string>
            {
                Path.Combine(@"C:\Raporlar", dosyaAdi),
                Path.Combine(Application.StartupPath, "Raporlar", dosyaAdi),
                Path.Combine(Application.StartupPath, dosyaAdi)
            };

            foreach (string aday in adaylar)
            {
                if (File.Exists(aday))
                    return aday;
            }

            return string.Empty;
        }

        private string FrxDosyasiniPdfOlarakHazirla(string frxYolu, out string hataDetayi)
        {
            hataDetayi = string.Empty;
            try
            {
                string tempYol = Path.Combine(Path.GetTempPath(), "KoleraRaporOnizleme");
                Directory.CreateDirectory(tempYol);

                string pdfAdi = Path.GetFileNameWithoutExtension(frxYolu) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                string pdfYolu = Path.Combine(tempYol, pdfAdi);
                string temizFrxYolu = FrxIceriginiTemizleyipHazirla(frxYolu);

                using (var report = new FastReport.Report())
                using (var pdfExport = new PDFSimpleExport())
                {
                    report.Load(temizFrxYolu);
                    RaporIciScriptBaglantilariniTemizle(report);
                    FastReportVerisiniBagla(report);
                    try
                    {
                        report.SetParameterValue("KursiyerAdiSoyadi", (_kursiyer?.ADI ?? string.Empty) + " " + (_kursiyer?.SOYADI ?? string.Empty));
                    }
                    catch
                    {
                    }

                    try
                    {
                        report.SetParameterValue("KursiyerTcNo", _kursiyer?.TC_NO ?? string.Empty);
                    }
                    catch
                    {
                    }

                    report.Prepare();
                    report.Export(pdfExport, pdfYolu);
                }

                return pdfYolu;
            }
            catch (Exception ex)
            {
                hataDetayi = ex.ToString();
                return string.Empty;
            }
        }

        private string FrxDosyasiniViewerIcinHazirla(string frxYolu, out string hataDetayi)
        {
            hataDetayi = string.Empty;
            try
            {
                string tempYol = Path.Combine(Path.GetTempPath(), "KoleraRaporOnizleme");
                Directory.CreateDirectory(tempYol);

                string fpxAdi = Path.GetFileNameWithoutExtension(frxYolu) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".fpx";
                string fpxYolu = Path.Combine(tempYol, fpxAdi);
                string temizFrxYolu = FrxIceriginiTemizleyipHazirla(frxYolu);

                using (var report = new FastReport.Report())
                {
                    report.Load(temizFrxYolu);
                    RaporIciScriptBaglantilariniTemizle(report);
                    FastReportVerisiniBagla(report);
                    report.Prepare();
                    report.SavePrepared(fpxYolu);
                }

                return fpxYolu;
            }
            catch (Exception ex)
            {
                hataDetayi = ex.ToString();
                return string.Empty;
            }
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

            doc.Save(temizFrxYolu);
            return temizFrxYolu;
        }

        private static bool FrxViewerAc(string frxYolu)
        {
            try
            {
                string ayarYolu = (ConfigurationManager.AppSettings["FastReportViewerPath"] ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(ayarYolu) && File.Exists(ayarYolu))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = ayarYolu,
                        Arguments = "\"" + frxYolu + "\"",
                        UseShellExecute = true
                    });
                    return true;
                }

                var viewerYollari = new[]
                {
                    Path.Combine(Application.StartupPath, "FastReport", "Viewer.exe"),
                    Path.Combine(Application.StartupPath, "Viewer.exe"),
                    @"D:\Kolera_Mtsk\Kolera_Mtsk\FastReport\Viewer.exe",
                    @"C:\Program Files\FastReport\FastReport Viewer\Viewer.exe",
                    @"C:\Program Files (x86)\FastReport\FastReport Viewer\Viewer.exe"
                };

                foreach (string exeYolu in viewerYollari)
                {
                    if (!File.Exists(exeYolu))
                        continue;

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exeYolu,
                        Arguments = "\"" + frxYolu + "\"",
                        UseShellExecute = true
                    });
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private void FastReportVerisiniBagla(FastReport.Report report)
        {
            if (report == null)
                return;

            try
            {
                report.SetParameterValue("P_ConnectionString", _connectionString ?? string.Empty);
            }
            catch
            {
            }

            int kursiyerId = KursiyerIdBul();
            try
            {
                report.SetParameterValue("P_KursiyerId", kursiyerId);
            }
            catch
            {
            }

            var kursiyerTable = KursiyerVerisiniGetir(kursiyerId);
            KursiyerRaporKolonlariniGarantiEt(kursiyerTable);
            KursiyerBosDegerleriTamamla(kursiyerTable);

            report.RegisterData(kursiyerTable, "Kursiyer");
            report.RegisterData(kursiyerTable, "KURSIYER");

            var dsKursiyer = report.GetDataSource("Kursiyer");
            if (dsKursiyer != null)
                dsKursiyer.Enabled = true;

            var dsKURSIYER = report.GetDataSource("KURSIYER");
            if (dsKURSIYER != null)
                dsKURSIYER.Enabled = true;

            var kursTable = KursRaporKursTablosu.Olustur(_connectionString);
            report.RegisterData(kursTable, "KURS");
            var dsKurs = report.GetDataSource("KURS");
            if (dsKurs != null)
                dsKurs.Enabled = true;

            KursRaporKursTablosu.ProgramatikTablolardaBaglantiyiYoksay(report);
        }

        private static void RaporIciScriptBaglantilariniTemizle(FastReport.Report report)
        {
            if (report == null)
                return;

            try
            {
                // Eski FRX scriptleri MsSqlDataConnection tipine bagli olabiliyor.
                // OpenSource surumde bu tip olmadiginda Prepare asamasinda patlar.
                report.ScriptText = MinimalFastReportScript();
            }
            catch
            {
            }

            try
            {
                report.Dictionary.Connections.Clear();
            }
            catch
            {
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

        private int KursiyerIdBul()
        {
            if (_kursiyer != null)
            {
                var tip = _kursiyer.GetType();
                string[] alanlar = { "ID", "Id", "ID_KURSIYER", "KURSIYER_ID" };
                foreach (string alan in alanlar)
                {
                    var prop = tip.GetProperty(alan);
                    if (prop == null)
                        continue;

                    object deger = prop.GetValue(_kursiyer, null);
                    int id;
                    if (int.TryParse(Convert.ToString(deger), out id) && id > 0)
                        return id;
                }
            }

            string tc = (_kursiyer?.TC_NO ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(tc) || string.IsNullOrWhiteSpace(_connectionString))
                return 0;

            const string sql = @"
SELECT TOP 1 ID
FROM KURSIYER
WHERE REPLACE(REPLACE(ISNULL(TC_NO,''),' ',''),'-','') = @TC";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TC", tc.Replace(" ", string.Empty).Replace("-", string.Empty));
                    conn.Open();
                    object sonuc = cmd.ExecuteScalar();
                    int id;
                    if (int.TryParse(Convert.ToString(sonuc), out id) && id > 0)
                        return id;
                }
            }
            catch
            {
            }

            return 0;
        }

        private DataTable KursiyerVerisiniGetir(int kursiyerId)
        {
            var dt = new DataTable("Kursiyer");

            if (kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            var tabloAdlari = new[] { "KURSIYER", "KURSIYERLER", "KURSİYER" };

            foreach (string tabloAdi in tabloAdlari)
            {
                string sql = @"
SELECT TOP 1 *
FROM " + tabloAdi + @"
WHERE ID = @ID";

                try
                {
                    dt = new DataTable("Kursiyer");
                    using (var conn = new SqlConnection(_connectionString))
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        cmd.Parameters.AddWithValue("@ID", kursiyerId);
                        da.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                        return dt;
                }
                catch
                {
                    // Sonraki tablo varyanti denenecek.
                }
            }

            return dt;
        }

        private static void KursiyerRaporKolonlariniGarantiEt(DataTable dt)
        {
            if (dt == null)
                return;

            // Muracaat formu ve benzer raporlarda kullanilan alanlar.
            EnsureColumn(dt, "ADI", typeof(string));
            EnsureColumn(dt, "SOYADI", typeof(string));
            EnsureColumn(dt, "TC_NO", typeof(string));
            EnsureColumn(dt, "KIMLIK_BABA_ADI", typeof(string));
            EnsureColumn(dt, "KIM_ANA_ADI", typeof(string));
            EnsureColumn(dt, "KIMLIK_DOGUM_YERI", typeof(string));
            EnsureColumn(dt, "DOGUM_TARIHI", typeof(DateTime));
            EnsureColumn(dt, "TAHSILI", typeof(string));
            EnsureColumn(dt, "EV_ADRESI", typeof(string));
            EnsureColumn(dt, "EV_ILCE", typeof(string));
            EnsureColumn(dt, "EV_IL", typeof(string));
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
        }

        private static void EnsureColumn(DataTable dt, string columnName, Type columnType)
        {
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
                    else if (col.DataType == typeof(byte[]))
                        bos[col.ColumnName] = DBNull.Value;
                    else if (col.DataType == typeof(DateTime))
                        bos[col.ColumnName] = DBNull.Value;
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

    }
}
