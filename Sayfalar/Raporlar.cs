using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.Web.WebView2.WinForms;
using Kolera_Mtsk.Reporting.Services;
using System.Threading.Tasks;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Raporlar : Form
    {
        private readonly string _connectionString;
        private readonly ReportDataService _reportService;
        private readonly WebView2 _onizlemeWebView;
        private int _sonOnizlenenRaporId = -1;
        private bool _onizlemeDevamEdiyor;

        public Raporlar(string connectionString)
        {
            InitializeComponent();

            _connectionString = connectionString;
            _reportService = new ReportDataService(connectionString);

            _onizlemeWebView = new WebView2();
            _onizlemeWebView.Dock = DockStyle.Fill;

            Load += Raporlar_Load;

            Btn_Onizle.Click += Btn_Onizle_Click;
            Btn_Yazdir.Click += Btn_Yazdir_Click;
            Btn_Pdf.Click += Btn_Pdf_Click;
            Cmb_RaporGrubu.SelectedIndexChanged += Cmb_RaporGrubu_SelectedIndexChanged;
        }

        private async void Raporlar_Load(object sender, EventArgs e)
        {
            OnizlemePaneliHazirla();
            RaporGruplariniYukle();
            GridKolonAyarla();
            SeciliGrupRaporlariniYukle();

            await _onizlemeWebView.EnsureCoreWebView2Async();
        }

        private void OnizlemePaneliHazirla()
        {
            splitContainer1.Panel2.Controls.Clear();
            splitContainer1.Panel2.Controls.Add(_onizlemeWebView);
        }

        // =========================
        // GRUPLAR
        // =========================
        private void RaporGruplariniYukle()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = @"
SELECT DISTINCT RAPOR_GRUBU
FROM RAPOR_TANIMLARI
WHERE RAPOR_GRUBU <> ''
ORDER BY RAPOR_GRUBU";

            Cmb_RaporGrubu.Items.Clear();

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                con.Open();

                using (SqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        Cmb_RaporGrubu.Items.Add(rdr[0].ToString());
                    }
                }
            }

            if (Cmb_RaporGrubu.Items.Count > 0)
                Cmb_RaporGrubu.SelectedIndex = 0;
        }

        private void Cmb_RaporGrubu_SelectedIndexChanged(object sender, EventArgs e)
        {
            SeciliGrupRaporlariniYukle();
        }

        private void SeciliGrupRaporlariniYukle()
        {
            DataTable dt = BosRaporTablosu();

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                Dgv_Raporlar.DataSource = dt;
                return;
            }

            string seciliGrup = Cmb_RaporGrubu.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(seciliGrup))
            {
                Dgv_Raporlar.DataSource = dt;
                Lbl_Durum.Text = "Rapor grubu seciniz";
                return;
            }

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
WHERE ISNULL(RAPOR_GRUBU, '') = @RAPOR_GRUBU
ORDER BY ISNULL(SIRA_NO, 0), RAPOR_ADI;";

            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, con))
                {
                    da.SelectCommand.Parameters.AddWithValue("@RAPOR_GRUBU", seciliGrup);
                    da.Fill(dt);
                }

                Dgv_Raporlar.DataSource = dt;
                Lbl_Durum.Text = dt.Rows.Count + " rapor listelendi";
                _sonOnizlenenRaporId = -1;
            }
            catch (Exception ex)
            {
                Dgv_Raporlar.DataSource = dt;
                Lbl_Durum.Text = "Raporlar yuklenemedi";
                MessageBox.Show("Rapor listesi yuklenemedi:\n" + ex.Message);
            }
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

        // =========================
        // ÖNİZLEME
        // =========================
        private async void Btn_Onizle_Click(object sender, EventArgs e)
        {
            await OnizleAsync();
        }

        private async Task OnizleAsync()
        {
            if (_onizlemeDevamEdiyor)
                return;

            string frx = SeciliRaporDosyasi();

            if (!File.Exists(frx))
            {
                MessageBox.Show("Rapor bulunamadı.");
                return;
            }

            _onizlemeDevamEdiyor = true;
            try
            {
                string pdf = FrxToPdf(frx);

                if (_onizlemeWebView.CoreWebView2 == null)
                    await _onizlemeWebView.EnsureCoreWebView2Async();

                _onizlemeWebView.CoreWebView2.Navigate(new Uri(pdf).AbsoluteUri);
            }
            finally
            {
                _onizlemeDevamEdiyor = false;
            }
        }

        // =========================
        // FASTREPORT (7.3 UYUMLU)
        // =========================
        private string FrxToPdf(string frxPath)
        {
            string pdfPath = Path.Combine(
                Path.GetTempPath(),
                "rapor_" + DateTime.Now.Ticks + ".pdf");

            Report report = new Report();
            PDFSimpleExport export = new PDFSimpleExport();

            try
            {
                report.Load(frxPath);

                DataTable dt = _reportService.GetTekKayit("KURSIYER", SeciliId());

                report.RegisterData(dt, "KURSIYER");
                report.GetDataSource("KURSIYER").Enabled = true;

                report.Prepare();
                report.Export(export, pdfPath);

                return pdfPath;
            }
            finally
            {
                report.Dispose();
                export.Dispose();
            }
        }

        // =========================
        // SEÇİM
        // =========================
        private int SeciliId()
        {
            if (Dgv_Raporlar.CurrentRow == null)
                return 0;

            int id;
            int.TryParse(Dgv_Raporlar.CurrentRow.Cells["ID"].Value.ToString(), out id);
            return id;
        }

        private string SeciliRaporDosyasi()
        {
            if (Dgv_Raporlar.CurrentRow == null)
                return "";
            return Convert.ToString(Dgv_Raporlar.CurrentRow.Cells["RAPOR_YOLU"].Value);
        }

        // =========================
        // YAZDIR
        // =========================
        private void Btn_Yazdir_Click(object sender, EventArgs e)
        {
            string frx = SeciliRaporDosyasi();

            if (!File.Exists(frx))
            {
                MessageBox.Show("Rapor bulunamadı.");
                return;
            }

            string pdf = FrxToPdf(frx);

            Process.Start(new ProcessStartInfo
            {
                FileName = pdf,
                UseShellExecute = true,
                Verb = "print"
            });
        }

        private void Btn_Pdf_Click(object sender, EventArgs e)
        {
            Btn_Onizle_Click(sender, e);
        }

        // =========================
        // GRID
        // =========================
        private void GridKolonAyarla()
        {
            Dgv_Raporlar.AutoGenerateColumns = true;
            Dgv_Raporlar.ReadOnly = true;
            Dgv_Raporlar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dgv_Raporlar.MultiSelect = false;
            Dgv_Raporlar.AllowUserToAddRows = false;
            Dgv_Raporlar.AllowUserToDeleteRows = false;
            Dgv_Raporlar.RowHeadersVisible = false;
            Dgv_Raporlar.SelectionChanged += Dgv_Raporlar_SelectionChanged;

            Dgv_Raporlar.DataBindingComplete += (s, e) =>
            {
                if (Dgv_Raporlar.Columns.Contains("ID"))
                    Dgv_Raporlar.Columns["ID"].Visible = false;
                if (Dgv_Raporlar.Columns.Contains("SABLON_BINARY"))
                    Dgv_Raporlar.Columns["SABLON_BINARY"].Visible = false;
            };
        }

        private async void Dgv_Raporlar_SelectionChanged(object sender, EventArgs e)
        {
            int seciliId = SeciliId();
            if (seciliId <= 0 || seciliId == _sonOnizlenenRaporId)
                return;

            _sonOnizlenenRaporId = seciliId;
            await OnizleAsync();
        }
    }
}