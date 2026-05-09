using Kolera.Arama.Services;
using Kolera_Kursiyer;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kolera.Arama;
using Kolera_Kursiyer.Services;
using System.Data.SqlClient;

using Kolera_Mtsk.Sayfalar;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Arama_Sayfam : Form
    {
        private readonly string _connectionString;
        private readonly Panel _anaPanel;
        private readonly AramaService _service;
        private readonly Timer _searchTimer;

        public Arama_Sayfam() : this(string.Empty, null)
        {
        }

        public Arama_Sayfam(string connectionString, Panel anaPanel)
        {
            InitializeComponent();

            _connectionString = connectionString;
            _anaPanel = anaPanel;

            _service = new AramaService(connectionString);

            Dvg_Kursiyerler.ReadOnly = true;
            Dvg_Kursiyerler.AllowUserToAddRows = false;
            Dvg_Kursiyerler.AllowUserToDeleteRows = false;
            Dvg_Kursiyerler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dvg_Kursiyerler.MultiSelect = false;
            Dvg_Kursiyerler.AutoGenerateColumns = false;

            this.Load += Form_Arama_Load;
            Dvg_Kursiyerler.CellDoubleClick += Dvg_Kursiyerler_CellDoubleClick;

            _searchTimer = new Timer { Interval = 400 };
            _searchTimer.Tick += async (s, e) =>
            {
                _searchTimer.Stop();
                await AraAsync();
            };

            Txt_Ara.TextChanged += (s, e) =>
            {
                _searchTimer.Stop();
                _searchTimer.Start();
            };
        }
        

        private async void Form_Arama_Load(object sender, EventArgs e)
        {
            DataGridStyle();
            await ListeleAsync();
        }
        public enum AramaModu
        {
            DetayAc,
            SecimYap
        }

        public AramaModu Mod { get; set; } = AramaModu.DetayAc;

        public event Action<int> KursiyerSecildi;
        public int SecilenKursiyerId { get; private set; }

        private void DataGridStyle()
        {
            Dvg_Kursiyerler.BorderStyle = BorderStyle.None;
            Dvg_Kursiyerler.BackgroundColor = Color.White;
            Dvg_Kursiyerler.EnableHeadersVisualStyles = false;
            Dvg_Kursiyerler.ColumnHeadersDefaultCellStyle.BackColor = Color.SteelBlue;
            Dvg_Kursiyerler.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dvg_Kursiyerler.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            Dvg_Kursiyerler.ColumnHeadersHeight = 40;
            Dvg_Kursiyerler.RowTemplate.Height = 35;
            Dvg_Kursiyerler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Dvg_Kursiyerler.RowHeadersVisible = false;
        }

        private async Task ListeleAsync()
        {
            try
            {
                PrgBar.Visible = true;
                var dt = await GetKursiyerListSafeAsync(null);
                BindData(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri yükleme hatası: " + ex.Message);
            }
            finally { PrgBar.Visible = false; }
        }

        private async Task AraAsync()
        {
            try
            {
                PrgBar.Visible = true;
                string kw = Txt_Ara.Text.Trim();
                var dt = await GetKursiyerListSafeAsync(string.IsNullOrWhiteSpace(kw) ? null : kw);
                BindData(dt);
            }
            finally { PrgBar.Visible = false; }
        }

        private Task<DataTable> GetKursiyerListSafeAsync(string keyword)
        {
            // Kullanici istegi: eski kolon adlarini hic kullanma.
            // Bu nedenle servis yerine dogrudan yeni kolon adlariyla SQL kullanilir.
            return GetKursiyerListDirectAsync(keyword);
        }

        private Task<DataTable> GetKursiyerListDirectAsync(string keyword)
        {
            return Task.Run(() =>
            {
                var dt = new DataTable();
                if (string.IsNullOrWhiteSpace(_connectionString))
                    return dt;

                const string sql = @"
SELECT
    k.ID,
    k.KAYIT_TARIHI,
    ISNULL(k.TC_NO, '') AS TC_NO,
    ISNULL(k.ADI, '') AS ADI,
    ISNULL(k.SOYADI, '') AS SOYADI,
    ISNULL(k.ONCE_SERT_SINIFI, '') AS ONCE_SERT_SINIFI,
    ISNULL(k.SERTIFIKA_SINIFI, '') AS SERTIFIKA_SINIFI,
    ISNULL(g.DONEM_ADI, '') AS DONEM,
    ISNULL(k.KIMLIK_BABA_ADI, '') AS BABA_ADI,
    ISNULL(k.KIM_ANA_ADI, '') AS KIM_ANA_ADI,
    ISNULL(k.GSM_1, '') AS GSM_1,
    ISNULL(k.GSM_2, '') AS GSM_2,
    ISNULL(k.ON_NOTLAR, '') AS NOTLAR
FROM KURSIYER k
LEFT JOIN GRUP_KARTI g ON g.ID = k.ID_GRUP_KARTI
WHERE
    (@kw IS NULL OR @kw = '' OR
     k.ADI LIKE '%' + @kw + '%' OR
     k.SOYADI LIKE '%' + @kw + '%' OR
     k.TC_NO LIKE '%' + @kw + '%' OR
     k.KIMLIK_KAYIT_NO LIKE '%' + @kw + '%' OR
     k.GSM_1 LIKE '%' + @kw + '%' OR
     k.GSM_2 LIKE '%' + @kw + '%')
ORDER BY k.ID DESC;";
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("@kw", (object)keyword ?? DBNull.Value);
                    con.Open();
                    da.Fill(dt);
                }
                return dt;
            });
        }

        private void BindData(DataTable dt)
        {
            Dvg_Kursiyerler.Columns.Clear();

            AddColumn("ID", "ID", "No", 60, false);
            AddColumn("KAYIT_TARIHI", "KAYIT_TARIHI", "Kayıt Tarihi", 120);
            AddColumn("TC_NO", "TC_NO", "TC Kimlik No", 130);
            AddColumn("ADI", "ADI", "Adı", 130);
            AddColumn("SOYADI", "SOYADI", "Soyadı", 130);
            AddColumn("ONCE_SERT_SINIFI", "ONCE_SERT_SINIFI", "Önceki Ehliyet", 80);
            AddColumn("SERTIFIKA_SINIFI", "SERTIFIKA_SINIFI", "Ehliyet Sınıfı", 80);
            AddColumn("DONEM", "DONEM", "Dönem", 100);
            AddColumn("BABA_ADI", "BABA_ADI", "Baba Adı", 120);
            AddColumn("KIM_ANA_ADI", "KIM_ANA_ADI", "Ana Adı", 120);
            AddColumn("GSM_1", "GSM_1", "Cep Telefonu", 110);
            AddColumn("GSM_2", "GSM_2", "Diğer Telefon", 110);
            AddColumn("NOTLAR", "NOTLAR", "Notlar", 150);
            

            Dvg_Kursiyerler.DataSource = dt;
        }

        private async void DvgKursiyerler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!(Dvg_Kursiyerler.Rows[e.RowIndex].DataBoundItem is DataRowView rv)) return;

            var row = rv.Row;
            int id = Convert.ToInt32(row["ID"]);

            try
            {
                var bytes = await _service.GetKursiyerResimByIdAsync(id);
                if (bytes != null)
                {
                    using (var ms = new MemoryStream(bytes))
                    using (var img = Image.FromStream(ms))
                    {
                        Tnk_RESIM.SizeMode = PictureBoxSizeMode.Zoom;
                        Tnk_RESIM.Image = new Bitmap(img);
                    }
                }
                else
                {
                    Tnk_RESIM.Image = null;
                }
            }
            catch { Tnk_RESIM.Image = null; }
        }

        private void AddColumn(string name, string dataProperty, string headerText, int width, bool visible = true)
        {
            var col = new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = dataProperty,
                HeaderText = headerText,
                Width = width,
                ReadOnly = true,
                Visible = visible,
                SortMode = DataGridViewColumnSortMode.Automatic
            };
            Dvg_Kursiyerler.Columns.Add(col);
        }
        private void Dvg_Kursiyerler_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (!(Dvg_Kursiyerler.Rows[e.RowIndex].DataBoundItem is DataRowView rv)) return;

            int id = Convert.ToInt32(rv.Row["ID"]);

            // Seçim moduysa geri döndür
            if (Mod == AramaModu.SecimYap)
            {
                SecilenKursiyerId = id;
                KursiyerSecildi?.Invoke(id);
                this.DialogResult = DialogResult.OK;
                this.Close();
                return;
            }

            // Normal detay açma modu
            var detay = new KursiyerDetay_Sayfam(_connectionString, id);

            detay.TopLevel = false;
            detay.FormBorderStyle = FormBorderStyle.None;
            detay.Dock = DockStyle.Fill;

            _anaPanel.Controls.Clear();
            _anaPanel.Controls.Add(detay);
            detay.Show();
        }


    }
}