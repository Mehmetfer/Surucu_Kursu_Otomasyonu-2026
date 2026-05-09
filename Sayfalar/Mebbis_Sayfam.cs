using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Kolera.Mebbis.Services;
using Kolera.Mebbis.Models;
using Kolera.Evrak.Models;
using Kolera.Evrak.Services;
using Kolera_Mtsk.Services;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Mebbis_Sayfam : Form
    {
        private readonly MebbisService _service;
        private readonly KursiyerEvrakService _evrakService;
        private DataTable _dt;
        private MebbisKursiyerModel _secili;
        private int _seciliKursiyerId;
        private MebbisWebForm _mebbisForm;
        private readonly string _connectionString;

        private bool _loaded = false;

        public Mebbis_Sayfam() : this(string.Empty)
        {
        }

        public Mebbis_Sayfam(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString;
            _service = new MebbisService(connectionString);
            _evrakService = new KursiyerEvrakService(connectionString);

            Load += Form_Load;
        }

        // ================= LOAD =================
        private void Form_Load(object sender, EventArgs e)
        {
            ConfigureGrid();
            ApplyGridStyle();

            LoadDonemler();

            Combo_Donemler.SelectedIndexChanged += Combo_Donemler_SelectedIndexChanged;
            Dtg_Donemlerlistele.SelectionChanged += Grid_SelectionChanged;
            Tnk_Arama.TextChanged += Tnk_Arama_TextChanged;

            Btn_Aktar.Click += Btn_Aktar_Click;

            _loaded = true;
        }

        // ================= GRID CONFIG =================
        private void ConfigureGrid()
        {
            Dtg_Donemlerlistele.AutoGenerateColumns = true;
            Dtg_Donemlerlistele.ReadOnly = true;
            Dtg_Donemlerlistele.AllowUserToAddRows = false;
            Dtg_Donemlerlistele.AllowUserToDeleteRows = false;
            Dtg_Donemlerlistele.RowHeadersVisible = false;
            Dtg_Donemlerlistele.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dtg_Donemlerlistele.MultiSelect = false;
        }

        // ================= STYLE (ESKİ GÖZ ALICI TASARIM) =================
        private void ApplyGridStyle()
        {
            Dtg_Donemlerlistele.BorderStyle = BorderStyle.None;
            Dtg_Donemlerlistele.BackgroundColor = Color.White;
            Dtg_Donemlerlistele.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);

            Dtg_Donemlerlistele.DefaultCellStyle.SelectionBackColor = Color.IndianRed;
            Dtg_Donemlerlistele.DefaultCellStyle.SelectionForeColor = Color.White;

            Dtg_Donemlerlistele.EnableHeadersVisualStyles = false;
            Dtg_Donemlerlistele.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            Dtg_Donemlerlistele.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dtg_Donemlerlistele.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            Dtg_Donemlerlistele.RowTemplate.Height = 60;
            Dtg_Donemlerlistele.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // ================= DÖNEMLER =================
        private void LoadDonemler()
        {
            var dt = GetDonemlerDirect();
            if (dt == null)
                return;

            DataRow r = dt.NewRow();
            r["ID"] = -1;
            r["DONEM_ADI"] = "SEÇİNİZ";
            dt.Rows.InsertAt(r, 0);

            Combo_Donemler.DisplayMember = "DONEM_ADI";
            Combo_Donemler.ValueMember = "ID";
            Combo_Donemler.DataSource = dt;
        }

        private DataTable GetDonemlerDirect()
        {
            var dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("DONEM_ADI", typeof(string));

            if (string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            const string sql = @"
SELECT 
    ID,
    LTRIM(RTRIM(
        CASE 
            WHEN ISNULL(DONEM_ADI,'') <> '' THEN DONEM_ADI
            ELSE ISNULL(DONEM_YILI,'') + ' ' + ISNULL(DONEM_AYI,'') + ' ' + ISNULL(DONEM_GRUBU,'')
        END
    )) AS DONEM_ADI
FROM GRUP_KARTI
ORDER BY ISNULL(BAS_TAR, '19000101') DESC, ID DESC";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                con.Open();
                da.Fill(dt);
            }

            return dt;
        }

        // ================= DÖNEM SEÇ =================
        private void Combo_Donemler_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_loaded) return;

            int id = GetSelectedDonemId();
            if (id == int.MinValue)
                return;

            if (id == -1)
            {
                _dt = GetTumKursiyerDirect();
                Dtg_Donemlerlistele.DataSource = _dt;
                Hide("ID_KURSIYER");
                return;
            }

            _dt = GetKursiyerByDonemIdDirect(id);

            Dtg_Donemlerlistele.DataSource = _dt;

            Hide("ID_KURSIYER");
        }

        private DataTable GetKursiyerByDonemIdDirect(int donemId)
        {
            var dt = new DataTable();
            if (string.IsNullOrWhiteSpace(_connectionString) || donemId <= 0)
                return dt;

            const string sql = @"
IF OBJECT_ID('dbo.KURSIYERLER','U') IS NOT NULL
BEGIN
    SELECT 
        ID AS ID_KURSIYER,
        ISNULL(TC_NO,'') AS TC_NO,
        ISNULL(ADI,'') AS ADI,
        ISNULL(SOYADI,'') AS SOYADI,
        ISNULL(GSM_1,'') AS GSM_1,
        ISNULL(SERTIFIKA_SINIFI,'') AS SERTIFIKA_SINIFI,
        ISNULL(ONCE_SERT_SINIFI,'') AS ONCE_SERT_SINIFI,
        DOGUM_TARIHI,
        ISNULL(KIMLIK_KAYIT_NO,'') AS KIMLIK_KAYIT_NO
    FROM KURSIYER
    WHERE ID_GRUP_KARTI = @ID
    UNION ALL
    SELECT 
        ID AS ID_KURSIYER,
        ISNULL(TC_NO,'') AS TC_NO,
        ISNULL(ADI,'') AS ADI,
        ISNULL(SOYADI,'') AS SOYADI,
        ISNULL(GSM_1,'') AS GSM_1,
        ISNULL(SERTIFIKA_SINIFI,'') AS SERTIFIKA_SINIFI,
        ISNULL('', '') AS ONCE_SERT_SINIFI,
        DOGUM_TARIHI,
        ISNULL('', '') AS KIMLIK_KAYIT_NO
    FROM KURSIYERLER
    WHERE ID_GRUP_KARTI = @ID
    ORDER BY ID_KURSIYER DESC;
END
ELSE
BEGIN
    SELECT 
        ID AS ID_KURSIYER,
        ISNULL(TC_NO,'') AS TC_NO,
        ISNULL(ADI,'') AS ADI,
        ISNULL(SOYADI,'') AS SOYADI,
        ISNULL(GSM_1,'') AS GSM_1,
        ISNULL(SERTIFIKA_SINIFI,'') AS SERTIFIKA_SINIFI,
        ISNULL(ONCE_SERT_SINIFI,'') AS ONCE_SERT_SINIFI,
        DOGUM_TARIHI,
        ISNULL(KIMLIK_KAYIT_NO,'') AS KIMLIK_KAYIT_NO
    FROM KURSIYER
    WHERE ID_GRUP_KARTI = @ID
    ORDER BY ID DESC;
END";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@ID", donemId);
                con.Open();
                da.Fill(dt);
            }

            return dt;
        }

        private DataTable GetTumKursiyerDirect()
        {
            var dt = new DataTable();
            if (string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            const string sql = @"
IF OBJECT_ID('dbo.KURSIYERLER','U') IS NOT NULL
BEGIN
    SELECT 
        ID AS ID_KURSIYER,
        ISNULL(TC_NO,'') AS TC_NO,
        ISNULL(ADI,'') AS ADI,
        ISNULL(SOYADI,'') AS SOYADI,
        ISNULL(GSM_1,'') AS GSM_1,
        ISNULL(SERTIFIKA_SINIFI,'') AS SERTIFIKA_SINIFI,
        ISNULL(ONCE_SERT_SINIFI,'') AS ONCE_SERT_SINIFI,
        DOGUM_TARIHI,
        ISNULL(KIMLIK_KAYIT_NO,'') AS KIMLIK_KAYIT_NO
    FROM KURSIYER
    UNION ALL
    SELECT 
        ID AS ID_KURSIYER,
        ISNULL(TC_NO,'') AS TC_NO,
        ISNULL(ADI,'') AS ADI,
        ISNULL(SOYADI,'') AS SOYADI,
        ISNULL(GSM_1,'') AS GSM_1,
        ISNULL(SERTIFIKA_SINIFI,'') AS SERTIFIKA_SINIFI,
        ISNULL('', '') AS ONCE_SERT_SINIFI,
        DOGUM_TARIHI,
        ISNULL('', '') AS KIMLIK_KAYIT_NO
    FROM KURSIYERLER
    ORDER BY ID_KURSIYER DESC;
END
ELSE
BEGIN
    SELECT 
        ID AS ID_KURSIYER,
        ISNULL(TC_NO,'') AS TC_NO,
        ISNULL(ADI,'') AS ADI,
        ISNULL(SOYADI,'') AS SOYADI,
        ISNULL(GSM_1,'') AS GSM_1,
        ISNULL(SERTIFIKA_SINIFI,'') AS SERTIFIKA_SINIFI,
        ISNULL(ONCE_SERT_SINIFI,'') AS ONCE_SERT_SINIFI,
        DOGUM_TARIHI,
        ISNULL(KIMLIK_KAYIT_NO,'') AS KIMLIK_KAYIT_NO
    FROM KURSIYER
    ORDER BY ID DESC;
END";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                con.Open();
                da.Fill(dt);
            }

            return dt;
        }

        // ================= SEARCH =================
        private void Tnk_Arama_TextChanged(object sender, EventArgs e)
        {
            if (_dt == null) return;

            string f = Tnk_Arama.Text.Replace("'", "''");

            _dt.DefaultView.RowFilter =
                $"TC_NO LIKE '%{f}%' OR ADI LIKE '%{f}%' OR SOYADI LIKE '%{f}%' OR GSM_1 LIKE '%{f}%'";
        }

        // ================= GRID SELECTION =================
        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (Dtg_Donemlerlistele.CurrentRow == null) return;

            int id = GetInt(Dtg_Donemlerlistele.CurrentRow, "ID_KURSIYER");
            if (id <= 0) return;
            _seciliKursiyerId = id;

            try
            {
                _secili = _service.GetKursiyer(id);
            }
            catch
            {
                _secili = null;
            }

            if (_secili == null)
                _secili = BuildModelFromGridRow(Dtg_Donemlerlistele.CurrentRow, id);

            if (_secili == null) return;
            EnsureSeciliFotoLoaded(id);

            // Bazı servis sürümlerinde DOGUM_TARIHI/seri no modelde boş gelebiliyor.
            // Grid satırındaki değerlerle modeli tamamlayalım.
            EnrichKursiyerFromGrid(Dtg_Donemlerlistele.CurrentRow);

            // TEXTLER
            Tnk_TC_NO.Text = _secili.TC_NO;
            Tnk_ADI.Text = _secili.ADI;
            Tnk_SOYADI.Text = _secili.SOYADI;
            Tnk_ADAY_NO.Text = _seciliKursiyerId.ToString();

            Tnk_GSM_1.Text = _secili.GSM_1;
            SER_Sinifi.Text = _secili.SERTIFIKA_SINIFI;
            SINIFI_ONCEKI.Text = _secili.ONCE_SERT_SINIFI;

            SetImage(_secili.Foto);
        }

        // ================= RESİM =================
        private void SetImage(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                Tnk_RESIM_Kursiyer.Image = null;
                return;
            }

            using (var ms = new MemoryStream(data))
            {
                Tnk_RESIM_Kursiyer.Image = new Bitmap(Image.FromStream(ms));
                Tnk_RESIM_Kursiyer.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        // ================= AKTAR BUTONU =================
        private void Btn_Aktar_Click(object sender, EventArgs e)
        {
            if (_secili == null || _seciliKursiyerId <= 0)
                EnsureSelectionFromGrid();

            if (_secili == null || _seciliKursiyerId <= 0)
            {
                MessageBox.Show("Önce kursiyer seçin");
                return;
            }

            string mebbisKullanici;
            string mebbisSifre;
            bool bulundu = MebbisCredentialResolver.TryResolve(_connectionString, AppSession.CurrentUserName, out mebbisKullanici, out mebbisSifre);
            if (!bulundu || string.IsNullOrWhiteSpace(mebbisKullanici))
            {
                MessageBox.Show("MEBBİS kullanıcı yok (GenelParam tablosundaki MEBBİS kullanıcı/sifre alanlarını kontrol edin).");
                return;
            }

            KursiyerEvrak_Model evrak = null;
            try
            {
                evrak = _evrakService.GetKursiyerEvrak(_seciliKursiyerId);
            }
            catch
            {
                evrak = null;
            }

            if (_mebbisForm == null || _mebbisForm.IsDisposed)
            {
                _mebbisForm = new MebbisWebForm(
                    mebbisKullanici,
                    mebbisSifre,
                    _secili,
                    _secili.Foto,
                    evrak,
                    _connectionString);

                _mebbisForm.Show(this);
                return;
            }

            // Form yeniden açılmasın, mevcut oturum üzerinde aday değiştirilsin.
            _mebbisForm.KursiyerYukle(_secili, _secili.Foto, evrak);

            if (!_mebbisForm.Visible)
                _mebbisForm.Show(this);
            else
                _mebbisForm.BringToFront();
        }

        private void EnsureSelectionFromGrid()
        {
            var row = Dtg_Donemlerlistele.CurrentRow;
            if (row == null && Dtg_Donemlerlistele.SelectedRows.Count > 0)
                row = Dtg_Donemlerlistele.SelectedRows[0];
            if (row == null)
                return;

            int id = GetInt(row, "ID_KURSIYER");
            if (id <= 0)
                return;

            _seciliKursiyerId = id;

            if (_secili == null)
            {
                try
                {
                    _secili = _service.GetKursiyer(id);
                }
                catch
                {
                    _secili = null;
                }
            }

            if (_secili == null)
                _secili = BuildModelFromGridRow(row, id);

            if (_secili != null)
            {
                EnsureSeciliFotoLoaded(id);
                EnrichKursiyerFromGrid(row);
                Tnk_TC_NO.Text = _secili.TC_NO;
                Tnk_ADI.Text = _secili.ADI;
                Tnk_SOYADI.Text = _secili.SOYADI;
                Tnk_ADAY_NO.Text = _seciliKursiyerId.ToString();
                Tnk_GSM_1.Text = _secili.GSM_1;
                SER_Sinifi.Text = _secili.SERTIFIKA_SINIFI;
                SINIFI_ONCEKI.Text = _secili.ONCE_SERT_SINIFI;
                SetImage(_secili.Foto);
            }
        }

        private void EnsureSeciliFotoLoaded(int kursiyerId)
        {
            if (_secili == null || (_secili.Foto != null && _secili.Foto.Length > 0) || kursiyerId <= 0)
                return;

            var foto = GetKursiyerFotoDirect(kursiyerId);
            if (foto != null && foto.Length > 0)
                _secili.Foto = foto;
        }

        private byte[] GetKursiyerFotoDirect(int kursiyerId)
        {
            if (kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return null;

            const string sql = @"
SELECT TOP 1 RESIM
FROM KURSIYER
WHERE ID = @ID;";

            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ID", kursiyerId);
                    con.Open();
                    return cmd.ExecuteScalar() as byte[];
                }
            }
            catch
            {
                return null;
            }
        }

        // ================= HELPERS =================
        private int GetInt(DataGridViewRow row, string col)
        {
            if (row?.Cells[col]?.Value == null)
                return 0;

            int.TryParse(row.Cells[col].Value.ToString(), out int v);
            return v;
        }

        private int GetSelectedDonemId()
        {
            object sv = Combo_Donemler.SelectedValue;
            if (sv == null)
                return int.MinValue;

            int id;
            if (int.TryParse(Convert.ToString(sv), out id))
                return id;

            var drv = sv as DataRowView;
            if (drv != null && int.TryParse(Convert.ToString(drv["ID"]), out id))
                return id;

            return int.MinValue;
        }

        private void Hide(string col)
        {
            if (Dtg_Donemlerlistele.Columns.Contains(col))
                Dtg_Donemlerlistele.Columns[col].Visible = false;
        }

        private void EnrichKursiyerFromGrid(DataGridViewRow row)
        {
            if (_secili == null || row == null)
                return;

            SetModelDateIfPresent(_secili, row, "DOGUM_TARIHI", "DOGUMTARIHI", "DogumTarihi");
            SetModelStringIfPresent(_secili, row, "SERI_NO", "SERINO", "SeriNo", "KIMLIK_SERI_NO", "KIMLIK_KAYIT_NO", "KimKayitNo");
        }

        private MebbisKursiyerModel BuildModelFromGridRow(DataGridViewRow row, int id)
        {
            if (row == null)
                return null;

            return new MebbisKursiyerModel
            {
                TC_NO = GetCellString(row, "TC_NO"),
                ADI = GetCellString(row, "ADI"),
                SOYADI = GetCellString(row, "SOYADI"),
                GSM_1 = GetCellString(row, "GSM_1"),
                SERTIFIKA_SINIFI = GetCellString(row, "SERTIFIKA_SINIFI"),
                ONCE_SERT_SINIFI = GetCellString(row, "ONCE_SERT_SINIFI"),
                Foto = null
            };
        }

        private static string GetCellString(DataGridViewRow row, string col)
        {
            if (row == null || !row.DataGridView.Columns.Contains(col))
                return string.Empty;

            var v = row.Cells[col].Value;
            return v == null || v == DBNull.Value ? string.Empty : Convert.ToString(v);
        }

        private static void SetModelDateIfPresent(object model, DataGridViewRow row, params string[] propertyNames)
        {
            if (model == null || row == null || propertyNames == null)
                return;

            object dateValue = null;
            foreach (DataGridViewCell cell in row.Cells)
            {
                var colName = (cell.OwningColumn?.Name ?? string.Empty).Trim();
                if (string.Equals(colName, "DOGUM_TARIHI", StringComparison.OrdinalIgnoreCase) ||
                    (colName.IndexOf("DOGUM", StringComparison.OrdinalIgnoreCase) >= 0 &&
                     colName.IndexOf("TARIH", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    dateValue = cell.Value;
                    break;
                }
            }

            if (dateValue == null || dateValue == DBNull.Value)
                return;

            if (!DateTime.TryParse(dateValue.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsed) &&
                !DateTime.TryParse(dateValue.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                return;

            var type = model.GetType();
            foreach (var propName in propertyNames)
            {
                var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null || !prop.CanWrite)
                    continue;

                if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                {
                    prop.SetValue(model, parsed);
                    return;
                }

                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(model, parsed.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));
                    return;
                }
            }
        }

        private static void SetModelStringIfPresent(object model, DataGridViewRow row, params string[] propertyNames)
        {
            if (model == null || row == null || propertyNames == null)
                return;

            string raw = null;
            foreach (DataGridViewCell cell in row.Cells)
            {
                var colName = (cell.OwningColumn?.Name ?? string.Empty).Trim();
                if (string.Equals(colName, "SERI_NO", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(colName, "SERINO", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(colName, "KIMLIK_SERI_NO", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(colName, "KIMLIK_KAYIT_NO", StringComparison.OrdinalIgnoreCase))
                {
                    raw = cell.Value?.ToString();
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(raw))
                return;

            var type = model.GetType();
            foreach (var propName in propertyNames)
            {
                var prop = type.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (prop == null || !prop.CanWrite || prop.PropertyType != typeof(string))
                    continue;

                prop.SetValue(model, raw.Trim());
                return;
            }
        }
        private void Btn_adres_Click(object sender, EventArgs e) { }

        private void Btn_fatura_Click(object sender, EventArgs e) { }

        private void Btn_imzasi_Click(object sender, EventArgs e) { }

        private void Btn_OgrnBilgileri_Click(object sender, EventArgs e) { }

        private void Btn_OzelMTSK_Click(object sender, EventArgs e) { }

        private void Btn_Resim_Click(object sender, EventArgs e) { }

        private void Btn_SABIKA_Click(object sender, EventArgs e) { }

        private void Btn_Saglik_Click(object sender, EventArgs e) { }

        private void Btn_Sozlesme_Click(object sender, EventArgs e) { }
    }
}