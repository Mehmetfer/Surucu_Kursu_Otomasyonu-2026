using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Rapor2 : Form
    {
        private readonly string _connectionString;
        private readonly int _personelId;
        private readonly WebView2 _onizlemeWebView;
        private bool _onizlemePdfZoomKancasi;
        private const double OnizlemePdfZoomFactor = 1.5;

        private readonly ToolStripButton _btnYeni;
        private readonly ToolStripButton _btnDuzenle;
        private readonly ToolStripButton _btnSil;
        private readonly ToolStripButton _btnYenile;

        public Rapor2(string connectionString, int personelId)
        {
            InitializeComponent();

            _connectionString = connectionString ?? "";
            _personelId = personelId;

            _onizlemeWebView = new WebView2();

            _btnYeni = new ToolStripButton("Yeni");
            _btnDuzenle = new ToolStripButton("Düzenle");
            _btnSil = new ToolStripButton("Sil");
            _btnYenile = new ToolStripButton("Yenile");

            _btnYeni.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _btnDuzenle.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _btnSil.DisplayStyle = ToolStripItemDisplayStyle.Text;
            _btnYenile.DisplayStyle = ToolStripItemDisplayStyle.Text;

            toolStrip1.Items.Insert(0, _btnYenile);
            toolStrip1.Items.Insert(0, _btnSil);
            toolStrip1.Items.Insert(0, _btnDuzenle);
            toolStrip1.Items.Insert(0, _btnYeni);
            toolStrip1.Items.Insert(4, new ToolStripSeparator());

            Load += Rapor2_Load;

            Dgv_Raporlar.SelectionChanged += Dgv_Raporlar_SelectionChanged;
            Btn_Onizle.Click += Btn_Onizle_Click;
            Btn_Yazdir.Click += Btn_Yazdir_Click;

            _btnYeni.Click += Btn_Yeni_Click;
            _btnDuzenle.Click += Btn_Duzenle_Click;
            _btnSil.Click += Btn_Sil_Click;
            _btnYenile.Click += Btn_Yenile_Click;
        }

        private void Rapor2_Load(object sender, EventArgs e)
        {
            OnizlemePaneliHazirla();
            RaporListele();
            GridKolonAyarla();
            GuncelleDurumMetni();
        }

        private void OnizlemePaneliHazirla()
        {
            splitContainer1.Panel2.Controls.Clear();
            _onizlemeWebView.Dock = DockStyle.Fill;
            splitContainer1.Panel2.Controls.Add(_onizlemeWebView);
        }

        private void RaporListele()
        {
            Dgv_Raporlar.DataSource = RaporlariGetir();
        }

        private DataTable RaporlariGetir()
        {
            DataTable dt = BosRaporTablosu();

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
ORDER BY ISNULL(SIRA_NO, 0), RAPOR_ADI;";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor listesi alınamadı:\n" + ex.Message);
            }

            return dt;
        }

        private static DataTable BosRaporTablosu()
        {
            DataTable dt = new DataTable();

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

        private void GridKolonAyarla()
        {
            Dgv_Raporlar.AutoGenerateColumns = true;
            Dgv_Raporlar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dgv_Raporlar.MultiSelect = false;
            Dgv_Raporlar.ReadOnly = true;
            Dgv_Raporlar.RowHeadersVisible = false;
            Dgv_Raporlar.AllowUserToAddRows = false;
            Dgv_Raporlar.AllowUserToDeleteRows = false;
            Dgv_Raporlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Dgv_Raporlar.EnableHeadersVisualStyles = false;
            Dgv_Raporlar.ColumnHeadersDefaultCellStyle.BackColor = Color.DarkRed;
            Dgv_Raporlar.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dgv_Raporlar.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 9, FontStyle.Bold);

            if (Dgv_Raporlar.Columns.Contains("ID"))
                Dgv_Raporlar.Columns["ID"].Visible = false;

            if (Dgv_Raporlar.Columns.Contains("SABLON_BINARY"))
                Dgv_Raporlar.Columns["SABLON_BINARY"].Visible = false;

            if (Dgv_Raporlar.Columns.Contains("RAPOR_GRUBU"))
                Dgv_Raporlar.Columns["RAPOR_GRUBU"].HeaderText = "Rapor Grubu";

            if (Dgv_Raporlar.Columns.Contains("RAPOR_ADI"))
                Dgv_Raporlar.Columns["RAPOR_ADI"].HeaderText = "Rapor Adı";

            if (Dgv_Raporlar.Columns.Contains("RAPOR_YOLU"))
                Dgv_Raporlar.Columns["RAPOR_YOLU"].HeaderText = "Rapor Yolu";

            if (Dgv_Raporlar.Columns.Contains("SIRA_NO"))
                Dgv_Raporlar.Columns["SIRA_NO"].HeaderText = "Sıra";

            if (Dgv_Raporlar.Columns.Contains("AKTIF"))
                Dgv_Raporlar.Columns["AKTIF"].HeaderText = "Aktif";
        }

        private void Dgv_Raporlar_SelectionChanged(object sender, EventArgs e)
        {
            GuncelleDurumMetni();
        }

        private void GuncelleDurumMetni()
        {
            if (Dgv_Raporlar.CurrentRow == null)
            {
                Lbl_Durum.Text = "Rapor seçiniz";
                return;
            }

            string raporAdi = Convert.ToString(Dgv_Raporlar.CurrentRow.Cells["RAPOR_ADI"]?.Value);

            Lbl_Durum.Text = string.IsNullOrWhiteSpace(raporAdi)
                ? "Rapor seçiniz"
                : "Seçili: " + raporAdi;
        }

        private string SeciliRaporYolu()
        {
            if (Dgv_Raporlar.CurrentRow == null)
                return "";

            return Convert.ToString(Dgv_Raporlar.CurrentRow.Cells["RAPOR_YOLU"]?.Value);
        }

        private int SeciliRaporId()
        {
            if (Dgv_Raporlar.CurrentRow == null)
                return 0;

            return ToInt(Dgv_Raporlar.CurrentRow.Cells["ID"]?.Value);
        }

        private RaporModel SeciliRaporModeli()
        {
            DataGridViewRow row = Dgv_Raporlar.CurrentRow;

            if (row == null)
                return new RaporModel();

            return new RaporModel
            {
                RaporGrubu = Convert.ToString(row.Cells["RAPOR_GRUBU"]?.Value),
                RaporAdi = Convert.ToString(row.Cells["RAPOR_ADI"]?.Value),
                RaporYolu = Convert.ToString(row.Cells["RAPOR_YOLU"]?.Value),
                SiraNo = ToInt(row.Cells["SIRA_NO"]?.Value),
                Aktif = ToBool(row.Cells["AKTIF"]?.Value),
                Renk = ToInt(row.Cells["RENK"]?.Value),
                SablonBinary = row.Cells["SABLON_BINARY"]?.Value as byte[]
            };
        }

        private async void Btn_Onizle_Click(object sender, EventArgs e)
        {
            string yol = SeciliRaporYolu();

            if (_personelId <= 0)
            {
                MessageBox.Show("Seçili personel ID bulunamadı.");
                return;
            }

            if (string.IsNullOrWhiteSpace(yol))
            {
                MessageBox.Show("Rapor yolu boş.");
                return;
            }

            if (!File.Exists(yol))
            {
                MessageBox.Show("Rapor dosyası bulunamadı:\n" + yol);
                return;
            }

            string ext = Path.GetExtension(yol).ToLower();

            if (ext == ".frx")
            {
                await FrxOnizle(yol);
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = yol,
                    UseShellExecute = true
                });
            }
        }

        private async System.Threading.Tasks.Task FrxOnizle(string frxYolu)
        {
            string pdf = FrxToPdf(frxYolu);

            if (string.IsNullOrWhiteSpace(pdf) || !File.Exists(pdf))
            {
                MessageBox.Show("PDF üretilemedi.");
                return;
            }

            if (_onizlemeWebView.CoreWebView2 == null)
                await _onizlemeWebView.EnsureCoreWebView2Async();

            OnizlemePdfZoomKancasiniBagla();
            _onizlemeWebView.CoreWebView2.Navigate(new Uri(pdf).AbsoluteUri);
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
                _onizlemeWebView.ZoomFactor = OnizlemePdfZoomFactor;
            }
            catch
            {
            }
        }

        private string FrxToPdf(string frxYolu)
        {
            try
            {
                string pdfPath = Path.Combine(
                    Path.GetTempPath(),
                    "personel_rapor_" + DateTime.Now.Ticks + ".pdf");

                using (Report report = new Report())
                using (PDFSimpleExport pdf = new PDFSimpleExport())
                {
                    report.Load(frxYolu);

                    DataTable personelDt = GetPersonelData();
                    DataTable kursDt = GetKursData();

                    if (personelDt.Rows.Count == 0)
                    {
                        MessageBox.Show("Seçili personele ait kayıt bulunamadı.");
                        return "";
                    }

                    report.RegisterData(personelDt, "PERSONEL");
                    report.RegisterData(kursDt, "KURS");

                    report.GetDataSource("PERSONEL").Enabled = true;
                    report.GetDataSource("KURS").Enabled = true;
                    KursRaporKursTablosu.ProgramatikTablolardaBaglantiyiYoksay(report);

                    if (report.Parameters.FindByName("RaporTarihi") != null)
                    {
                        report.SetParameterValue("RaporTarihi", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
                    }

                    report.Prepare();
                    report.Export(pdf, pdfPath);
                }

                return pdfPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("FastReport PDF hatası:\n" + ex.Message);
                return "";
            }
        }

        private DataTable GetPersonelData()
        {
            DataTable dt = new DataTable("PERSONEL");

            if (string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            if (_personelId <= 0)
                return dt;

            string sql = @"
SELECT
    ID,
    PERSONEL_DURUMU,
    TC_NO,
    ADI,
    SOYADI,
    EHLIYET_SINIFI,
    EHLIYET_IKINCI,
    CINSIYET,
    MEDENI_DUR,
    DOGUM_TARIHI,
    YONETICI_GOREVI,
    VERDIGI_DERS_1,
    SOZ_BASLAMA_TAR,
    RESIM
FROM PERSONEL
WHERE ID = @ID;";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = _personelId;
                    da.Fill(dt);
                }

                dt.TableName = "PERSONEL";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Personel verisi alınamadı:\n" + ex.Message);
            }

            return dt;
        }

        private DataTable GetKursData()
        {
            DataTable dt = new DataTable("KURS");

            dt.Columns.Add("KURS_ADI", typeof(string));
            dt.Columns.Add("MUDUR_ADI", typeof(string));
            dt.Columns.Add("RAPOR_TARIHI", typeof(string));

            DataRow row = dt.NewRow();

            row["KURS_ADI"] = "KOLERA MTSK";
            row["MUDUR_ADI"] = "";
            row["RAPOR_TARIHI"] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            dt.Rows.Add(row);

            return dt;
        }

        private void Btn_Yazdir_Click(object sender, EventArgs e)
        {
            string yol = SeciliRaporYolu();

            if (_personelId <= 0)
            {
                MessageBox.Show("Seçili personel ID bulunamadı.");
                return;
            }

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
                        MessageBox.Show("Yazdırma için PDF üretilemedi.");
                        return;
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = pdf,
                        Verb = "print",
                        UseShellExecute = true
                    });

                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = yol,
                    Verb = "print",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yazdırma hatası:\n" + ex.Message);
            }
        }

        private void Btn_Yeni_Click(object sender, EventArgs e)
        {
            using (RaporDuzenleForm frm = new RaporDuzenleForm())
            {
                if (frm.ShowDialog() != DialogResult.OK)
                    return;

                if (!RaporEkle(frm.Model))
                    return;
            }

            YenidenYukle();
        }

        private void Btn_Duzenle_Click(object sender, EventArgs e)
        {
            int id = SeciliRaporId();

            if (id <= 0)
            {
                MessageBox.Show("Düzenlemek için bir rapor seçiniz.");
                return;
            }

            using (RaporDuzenleForm frm = new RaporDuzenleForm(SeciliRaporModeli()))
            {
                if (frm.ShowDialog() != DialogResult.OK)
                    return;

                if (!RaporGuncelle(id, frm.Model))
                    return;
            }

            YenidenYukle();
        }

        private void Btn_Sil_Click(object sender, EventArgs e)
        {
            int id = SeciliRaporId();

            if (id <= 0)
            {
                MessageBox.Show("Silmek için bir rapor seçiniz.");
                return;
            }

            if (MessageBox.Show("Seçili rapor silinsin mi?", "Onay",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (!RaporSil(id))
                return;

            YenidenYukle();
        }

        private void Btn_Yenile_Click(object sender, EventArgs e)
        {
            YenidenYukle();
        }

        private void YenidenYukle()
        {
            RaporListele();
            GridKolonAyarla();
            GuncelleDurumMetni();
        }

        private bool RaporEkle(RaporModel model)
        {
            const string sql = @"
INSERT INTO RAPOR_TANIMLARI
(
    RAPOR_GRUBU,
    RAPOR_ADI,
    RAPOR_YOLU,
    SIRA_NO,
    AKTIF,
    RENK,
    SABLON_BINARY,
    OLUSTURMA_TARIHI,
    GUNCELLEME_TARIHI
)
VALUES
(
    @RAPOR_GRUBU,
    @RAPOR_ADI,
    @RAPOR_YOLU,
    @SIRA_NO,
    @AKTIF,
    @RENK,
    @SABLON_BINARY,
    GETDATE(),
    GETDATE()
);";

            return ExecuteYazma(sql, model, null);
        }

        private bool RaporGuncelle(int id, RaporModel model)
        {
            const string sql = @"
UPDATE RAPOR_TANIMLARI
SET
    RAPOR_GRUBU = @RAPOR_GRUBU,
    RAPOR_ADI = @RAPOR_ADI,
    RAPOR_YOLU = @RAPOR_YOLU,
    SIRA_NO = @SIRA_NO,
    AKTIF = @AKTIF,
    RENK = @RENK,
    SABLON_BINARY = @SABLON_BINARY,
    GUNCELLEME_TARIHI = GETDATE()
WHERE ID = @ID;";

            return ExecuteYazma(sql, model, id);
        }

        private bool RaporSil(int id)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return false;

            const string sql = "DELETE FROM RAPOR_TANIMLARI WHERE ID = @ID;";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    conn.Open();

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme işlemi başarısız:\n" + ex.Message);
                return false;
            }
        }

        private bool ExecuteYazma(string sql, RaporModel model, int? id)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return false;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@RAPOR_GRUBU", SqlDbType.NVarChar).Value = (object)(model.RaporGrubu ?? "") ?? DBNull.Value;
                    cmd.Parameters.Add("@RAPOR_ADI", SqlDbType.NVarChar).Value = (object)(model.RaporAdi ?? "") ?? DBNull.Value;
                    cmd.Parameters.Add("@RAPOR_YOLU", SqlDbType.NVarChar).Value = (object)(model.RaporYolu ?? "") ?? DBNull.Value;
                    cmd.Parameters.Add("@SIRA_NO", SqlDbType.Int).Value = model.SiraNo;
                    cmd.Parameters.Add("@AKTIF", SqlDbType.Bit).Value = model.Aktif;
                    cmd.Parameters.Add("@RENK", SqlDbType.Int).Value = model.Renk;

                    SqlParameter p = cmd.Parameters.Add("@SABLON_BINARY", SqlDbType.VarBinary, -1);
                    p.Value = (object)model.SablonBinary ?? DBNull.Value;

                    if (id.HasValue)
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id.Value;

                    conn.Open();

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt işlemi başarısız:\n" + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return false;
            }
        }

        private static int ToInt(object value)
        {
            int n;
            return int.TryParse(Convert.ToString(value), out n) ? n : 0;
        }

        private static bool ToBool(object value)
        {
            if (value == null || value == DBNull.Value)
                return false;

            if (value is bool)
                return (bool)value;

            if (value is int)
                return Convert.ToInt32(value) == 1;

            bool b;
            if (bool.TryParse(Convert.ToString(value), out b))
                return b;

            return Convert.ToString(value) == "1";
        }

        private sealed class RaporModel
        {
            public string RaporGrubu { get; set; }
            public string RaporAdi { get; set; }
            public string RaporYolu { get; set; }
            public int SiraNo { get; set; }
            public bool Aktif { get; set; }
            public int Renk { get; set; }
            public byte[] SablonBinary { get; set; }
        }

        private sealed class RaporDuzenleForm : Form
        {
            private readonly TextBox _txtGrup;
            private readonly TextBox _txtAdi;
            private readonly TextBox _txtYol;
            private readonly NumericUpDown _numSira;
            private readonly CheckBox _chkAktif;
            private readonly NumericUpDown _numRenk;
            private readonly Button _btnDosyaSec;
            private readonly Button _btnSablonYukle;
            private readonly Label _lblSablonDurum;

            private byte[] _binary;

            public RaporModel Model { get; private set; }

            public RaporDuzenleForm() : this(null)
            {
            }

            public RaporDuzenleForm(RaporModel mevcut)
            {
                Text = mevcut == null ? "Yeni Rapor" : "Rapor Düzenle";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                Width = 560;
                Height = 380;

                TableLayoutPanel pnl = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 8,
                    Padding = new Padding(10)
                };

                pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
                pnl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

                _txtGrup = new TextBox { Dock = DockStyle.Fill };
                _txtAdi = new TextBox { Dock = DockStyle.Fill };
                _txtYol = new TextBox { Dock = DockStyle.Fill };

                _numSira = new NumericUpDown
                {
                    Dock = DockStyle.Left,
                    Width = 120,
                    Minimum = 0,
                    Maximum = 10000
                };

                _chkAktif = new CheckBox
                {
                    Text = "Aktif",
                    AutoSize = true,
                    Checked = true
                };

                _numRenk = new NumericUpDown
                {
                    Dock = DockStyle.Left,
                    Width = 120,
                    Minimum = int.MinValue,
                    Maximum = int.MaxValue
                };

                _btnDosyaSec = new Button { Text = "Dosya Seç", AutoSize = true };
                _btnSablonYukle = new Button { Text = "Şablon Binary Yükle", AutoSize = true };
                _lblSablonDurum = new Label { AutoSize = true, Text = "Binary yok", Margin = new Padding(8) };

                Button btnKaydet = new Button { Text = "Kaydet", DialogResult = DialogResult.OK };
                Button btnIptal = new Button { Text = "İptal", DialogResult = DialogResult.Cancel };

                _btnDosyaSec.Click += BtnDosyaSec_Click;
                _btnSablonYukle.Click += BtnSablonYukle_Click;
                btnKaydet.Click += BtnKaydet_Click;

                EkleSatir(pnl, 0, "Rapor Grubu:", _txtGrup);
                EkleSatir(pnl, 1, "Rapor Adı:", _txtAdi);

                FlowLayoutPanel yolPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true
                };

                yolPanel.Controls.Add(_txtYol);
                yolPanel.Controls.Add(_btnDosyaSec);
                _txtYol.Width = 300;

                EkleSatir(pnl, 2, "Rapor Yolu:", yolPanel);
                EkleSatir(pnl, 3, "Sıra No:", _numSira);
                EkleSatir(pnl, 4, "Aktif:", _chkAktif);
                EkleSatir(pnl, 5, "Renk:", _numRenk);
                EkleSatir(pnl, 6, "Şablon:", SablonPaneliHazirla());

                FlowLayoutPanel butonPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Right,
                    AutoSize = true
                };

                butonPanel.Controls.Add(btnKaydet);
                butonPanel.Controls.Add(btnIptal);

                EkleSatir(pnl, 7, "", butonPanel);

                Controls.Add(pnl);

                AcceptButton = btnKaydet;
                CancelButton = btnIptal;

                if (mevcut == null)
                    return;

                _txtGrup.Text = mevcut.RaporGrubu;
                _txtAdi.Text = mevcut.RaporAdi;
                _txtYol.Text = mevcut.RaporYolu;
                _numSira.Value = mevcut.SiraNo;
                _chkAktif.Checked = mevcut.Aktif;
                _numRenk.Value = mevcut.Renk;

                _binary = mevcut.SablonBinary;

                _lblSablonDurum.Text = _binary == null
                    ? "Binary yok"
                    : "Yüklü (" + _binary.Length + " byte)";
            }

            private FlowLayoutPanel SablonPaneliHazirla()
            {
                FlowLayoutPanel binaryPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true
                };

                binaryPanel.Controls.Add(_btnSablonYukle);
                binaryPanel.Controls.Add(_lblSablonDurum);

                return binaryPanel;
            }

            private static void EkleSatir(TableLayoutPanel pnl, int row, string etiket, Control control)
            {
                Label lbl = new Label
                {
                    Text = etiket,
                    AutoSize = true,
                    Margin = new Padding(3, 8, 3, 3)
                };

                pnl.Controls.Add(lbl, 0, row);
                pnl.Controls.Add(control, 1, row);
            }

            private void BtnDosyaSec_Click(object sender, EventArgs e)
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "FastReport Dosyası (*.frx)|*.frx|Tüm Dosyalar (*.*)|*.*";

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;

                    _txtYol.Text = dlg.FileName;

                    if (string.IsNullOrWhiteSpace(_txtAdi.Text))
                        _txtAdi.Text = Path.GetFileNameWithoutExtension(dlg.FileName);
                }
            }

            private void BtnSablonYukle_Click(object sender, EventArgs e)
            {
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Filter = "Rapor Dosyası (*.frx;*.repx)|*.frx;*.repx|Tüm Dosyalar (*.*)|*.*";

                    if (dlg.ShowDialog() != DialogResult.OK)
                        return;

                    _binary = File.ReadAllBytes(dlg.FileName);

                    if (string.IsNullOrWhiteSpace(_txtYol.Text))
                        _txtYol.Text = dlg.FileName;

                    if (string.IsNullOrWhiteSpace(_txtAdi.Text))
                        _txtAdi.Text = Path.GetFileNameWithoutExtension(dlg.FileName);

                    _lblSablonDurum.Text = "Yüklü (" + _binary.Length + " byte)";
                }
            }

            private void BtnKaydet_Click(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(_txtAdi.Text))
                {
                    MessageBox.Show("Rapor adı boş olamaz.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    DialogResult = DialogResult.None;
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtYol.Text))
                {
                    MessageBox.Show("Rapor yolu boş olamaz.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    DialogResult = DialogResult.None;
                    return;
                }

                Model = new RaporModel
                {
                    RaporGrubu = _txtGrup.Text.Trim(),
                    RaporAdi = _txtAdi.Text.Trim(),
                    RaporYolu = _txtYol.Text.Trim(),
                    SiraNo = Convert.ToInt32(_numSira.Value),
                    Aktif = _chkAktif.Checked,
                    Renk = Convert.ToInt32(_numRenk.Value),
                    SablonBinary = _binary
                };
            }
        }
    }
}