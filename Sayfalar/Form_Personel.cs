using Kolera.Personel.Models;
using Kolera.Personel.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Form_Personel : Form
    {
        private PersonelService _personelService;
        private readonly string _connectionString;
        private DataTable _gridKaynak;
        private int _seciliId;
        private TextBox _txtAra;

        public Form_Personel() : this(string.Empty)
        {
        }

        public Form_Personel(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? string.Empty;
            if (!IsDesignMode())
            {
                _personelService = new PersonelService(_connectionString);
                Load += Form_Personel_Load;
            }
        }

        private static bool IsDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private async void Form_Personel_Load(object sender, EventArgs e)
        {
            EkKontrolleriHazirla();
            GridAyarla();
            ComboDoldur();
            HazirlaDate(Tnk_DOGUM_TARIHI);
            HazirlaDate(Date_Sozlesme);
            await PersonelleriYukleAsync();
        }

        private void EkKontrolleriHazirla()
        {
            if (_txtAra == null)
            {
                _txtAra = new TextBox { Dock = DockStyle.Top, Height = 24 };
                _txtAra.TextChanged += (s, e) => GridFiltrele();
                panel1.Controls.Add(_txtAra);
                _txtAra.BringToFront();
            }

            Dvg_Personel.CellClick -= Dgv_CellClick;
            Dvg_Personel.CellClick += Dgv_CellClick;
            Dvg_Personel.DataBindingComplete -= Dgv_DataBindingComplete;
            Dvg_Personel.DataBindingComplete += Dgv_DataBindingComplete;
            Btn_Resim_Ekle.Click -= BtnResim_Click;
            Btn_Resim_Ekle.Click += BtnResim_Click;
            Btn_Yeniekle.Click -= BtnYeniEkle_Click;
            Btn_Yeniekle.Click += BtnYeniEkle_Click;
            Btn_Save.Click -= BtnSave_Click;
            Btn_Save.Click += BtnSave_Click;
            Btn_Sil.Click -= BtnSilTop_Click;
            Btn_Sil.Click += BtnSilTop_Click;
        }

        private void GridAyarla()
        {
            Dvg_Personel.AutoGenerateColumns = true;
            Dvg_Personel.ReadOnly = true;
            Dvg_Personel.MultiSelect = false;
            Dvg_Personel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dvg_Personel.AllowUserToAddRows = false;
            Dvg_Personel.AllowUserToDeleteRows = false;
            Dvg_Personel.RowHeadersVisible = false;
            Dvg_Personel.EnableHeadersVisualStyles = false;
            Dvg_Personel.BorderStyle = BorderStyle.None;
            Dvg_Personel.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            Dvg_Personel.GridColor = Color.FromArgb(220, 224, 230);
            Dvg_Personel.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            Dvg_Personel.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dvg_Personel.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Dvg_Personel.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            Dvg_Personel.DefaultCellStyle.SelectionForeColor = Color.White;
            Dvg_Personel.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(232, 240, 252);
            Dvg_Personel.RowTemplate.Height = 24;
        }

        private void BtnYeniEkle_Click(object sender, EventArgs e) => Temizle();

        private void BtnSave_Click(object sender, EventArgs e) => BtnKaydet_Click(sender, e);

        private void BtnSilTop_Click(object sender, EventArgs e) => BtnSil_Click(sender, e);

        private void ComboDoldur()
        {
            Cmb_durumu.Items.Clear();
            Cmb_durumu.Items.AddRange(new[] { "DEVAM EDİYOR", "AYRILDI" });
            Cmb_SINIFI.Items.Clear();
            Cmb_SINIFI.Items.AddRange(new[] { "B", "C", "D", "E" });
            Cmd_ikinci.Items.Clear();
            Cmd_ikinci.Items.AddRange(new[] { "B", "C", "D", "E" });
            Cmb_Cinsiyet.Items.Clear();
            Cmb_Cinsiyet.Items.AddRange(new[] { "ERKEK", "KADIN" });
            Cmb_Medeni.Items.Clear();
            Cmb_Medeni.Items.AddRange(new[] { "Bekar", "Evli" });
            Cmb_Yonetici.Items.Clear();
            Cmb_Yonetici.Items.AddRange(new[] { "Yönetici", "Personel" });
            Cmb_Gorev1.Items.Clear();
            Cmb_Gorev1.Items.AddRange(new[] { "Sürücü", "Öğretmen", "İdari" });
            Cmb_durumu.SelectedIndex = 0;
        }

        private async Task PersonelleriYukleAsync()
        {
            if (_personelService == null)
                return;

            try
            {
                var dt = await _personelService.GetPersonellerAsync();
                _gridKaynak = BuildGridTable(dt);
                GridFiltrele();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Personeller yüklenirken hata: " + ex.Message);
            }
        }

        private DataTable BuildGridTable(DataTable source)
        {
            var result = new DataTable();
            string[] cols = { "ID", "TC_NO", "ADI", "SOYADI", "PERSONEL_DURUMU" };
            foreach (var c in cols)
            {
                if (source.Columns.Contains(c))
                    result.Columns.Add(c, source.Columns[c].DataType);
            }

            var rows = source.AsEnumerable()
                .GroupBy(r =>
                {
                    string tc = source.Columns.Contains("TC_NO") ? (Convert.ToString(r["TC_NO"]) ?? string.Empty).Trim() : string.Empty;
                    if (!string.IsNullOrWhiteSpace(tc))
                        return "TC:" + tc;
                    return "AD:" + (Convert.ToString(r["ADI"]) ?? string.Empty).Trim().ToUpperInvariant() + "|" + (Convert.ToString(r["SOYADI"]) ?? string.Empty).Trim().ToUpperInvariant();
                })
                .Select(g => source.Columns.Contains("ID") ? g.OrderByDescending(x => ToInt(x["ID"])).First() : g.First())
                .OrderBy(r => NormalizeDurum(source.Columns.Contains("PERSONEL_DURUMU") ? Convert.ToString(r["PERSONEL_DURUMU"]) : null) == "AYRILDI" ? 1 : 0)
                .ThenBy(r => Convert.ToString(r["ADI"]) ?? string.Empty)
                .ThenBy(r => Convert.ToString(r["SOYADI"]) ?? string.Empty);

            foreach (var src in rows)
            {
                var nr = result.NewRow();
                foreach (DataColumn col in result.Columns)
                    nr[col.ColumnName] = src[col.ColumnName];
                result.Rows.Add(nr);
            }
            return result;
        }

        private void GridFiltrele()
        {
            if (_gridKaynak == null)
                return;

            var tr = new CultureInfo("tr-TR");
            string ara = (_txtAra?.Text ?? string.Empty).Trim().ToUpper(tr);
            var rows = _gridKaynak.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ara))
            {
                rows = rows.Where(r =>
                    (Convert.ToString(r["TC_NO"]) ?? string.Empty).ToUpper(tr).Contains(ara) ||
                    (Convert.ToString(r["ADI"]) ?? string.Empty).ToUpper(tr).Contains(ara) ||
                    (Convert.ToString(r["SOYADI"]) ?? string.Empty).ToUpper(tr).Contains(ara) ||
                    (Convert.ToString(r["PERSONEL_DURUMU"]) ?? string.Empty).ToUpper(tr).Contains(ara));
            }

            Dvg_Personel.DataSource = rows.Any() ? rows.CopyToDataTable() : _gridKaynak.Clone();
        }

        private void Dgv_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (Dvg_Personel.Columns.Contains("ID")) Dvg_Personel.Columns["ID"].Visible = false;
            if (Dvg_Personel.Columns.Contains("TC_NO")) Dvg_Personel.Columns["TC_NO"].Width = 130;
            if (Dvg_Personel.Columns.Contains("ADI")) Dvg_Personel.Columns["ADI"].Width = 130;
            if (Dvg_Personel.Columns.Contains("SOYADI")) Dvg_Personel.Columns["SOYADI"].Width = 130;
            if (Dvg_Personel.Columns.Contains("PERSONEL_DURUMU")) Dvg_Personel.Columns["PERSONEL_DURUMU"].Width = 150;
            Dvg_Personel.ClearSelection();
        }

        private async void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            _seciliId = Convert.ToInt32(Dvg_Personel.Rows[e.RowIndex].Cells["ID"].Value);
            DataRow dr = await GetPersonelRowByIdDirectAsync(_seciliId);
            if (dr == null)
                return;

            Tnk_TC_NO.Text = GetRowString(dr, "TC_NO");
            Tnk_ADI.Text = GetRowString(dr, "ADI");
            Tnk_SOYADI.Text = GetRowString(dr, "SOYADI");
            Tnk_GSM_1.Text = GetRowString(dr, "GSM_1");
            Cmb_durumu.Text = NormalizeDurum(GetRowString(dr, "PERSONEL_DURUMU"));
            Cmb_Cinsiyet.Text = GetRowString(dr, "CINSIYET");
            Cmb_Medeni.Text = GetRowString(dr, "MEDENI_DUR");
            Cmb_Yonetici.Text = GetRowString(dr, "YONETICI_GOREVI");
            Cmb_Gorev1.Text = GetRowString(dr, "VERDIGI_DERS_1");
            Cmb_SINIFI.Text = GetRowString(dr, "EHLIYET_SINIFI");
            Cmd_ikinci.Text = GetRowString(dr, "EHLIYET_IKINCI");
            TarihAyarla(Tnk_DOGUM_TARIHI, GetRowObject(dr, "DOGUM_TARIHI"));
            TarihAyarla(Date_Sozlesme, GetRowObject(dr, "SOZ_BASLAMA_TAR"));
            ResimYukle(GetRowObject(dr, "RESIM"));
        }

        private void BtnResim_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Resimler|*.jpg;*.png;*.bmp";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    using (var img = Image.FromFile(dlg.FileName))
                        Tnk_RESIM_Personel.Image = new System.Drawing.Bitmap(img);
                }
            }
        }

        private async void BtnKaydet_Click(object sender, EventArgs e)
        {
            try
            {
                await UpsertPersonelDirectAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydetme/Güncelleme sırasında hata: " + ex.Message);
                return;
            }

            await PersonelleriYukleAsync();
            Temizle();
        }

        private async void BtnSil_Click(object sender, EventArgs e)
        {
            if (_seciliId == 0) return;
            if (MessageBox.Show("Personeli silmek istiyor musunuz?", "Personel Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            await DeleteOrMarkAsAyrildiAsync(_seciliId);
            await PersonelleriYukleAsync();
            Temizle();
        }

        private void BtnRaporAl_Click(object sender, EventArgs e)
        {
            try
            {
                if (_seciliId <= 0)
                {
                    MessageBox.Show("Lütfen önce personel seçiniz.");
                    return;
                }

                string seciliAdSoyad = (Tnk_ADI.Text + " " + Tnk_SOYADI.Text).Trim();
                using (var raporDetay = new RaporDetay(_connectionString, "PERSONEL", _seciliId, "PERSONEL", seciliAdSoyad))
                {
                    raporDetay.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor ekranı açılamadı: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Temizle()
        {
            _seciliId = 0;
            Tnk_TC_NO.Clear();
            Tnk_ADI.Clear();
            Tnk_SOYADI.Clear();
            if (Cmb_durumu.Items.Count > 0) Cmb_durumu.SelectedIndex = 0;
            Tnk_RESIM_Personel.Image = null;
            TarihAyarla(Tnk_DOGUM_TARIHI, null);
            TarihAyarla(Date_Sozlesme, null);
        }

        private void HazirlaDate(DateTimePicker dtp)
        {
            dtp.Format = DateTimePickerFormat.Custom;
            dtp.CustomFormat = " ";
            dtp.ValueChanged += (s, e) => dtp.Format = DateTimePickerFormat.Short;
        }

        private void TarihAyarla(DateTimePicker dtp, object value)
        {
            if (value != null && value != DBNull.Value)
            {
                dtp.Format = DateTimePickerFormat.Short;
                dtp.Value = Convert.ToDateTime(value);
            }
            else
            {
                dtp.Format = DateTimePickerFormat.Custom;
                dtp.CustomFormat = " ";
            }
        }

        private DateTime? TarihOku(DateTimePicker dtp)
        {
            return dtp.Format == DateTimePickerFormat.Custom ? (DateTime?)null : dtp.Value;
        }

        private void ResimYukle(object value)
        {
            Tnk_RESIM_Personel.Image = null;
            if (value == null || value == DBNull.Value) return;
            using (var ms = new MemoryStream((byte[])value))
                Tnk_RESIM_Personel.Image = System.Drawing.Image.FromStream(ms);
        }

        private byte[] ImageToByte(Image img)
        {
            using (var bmp = new Bitmap(img))
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private static string NormalizeDurum(string durum)
        {
            if (string.IsNullOrWhiteSpace(durum)) return "DEVAM EDİYOR";
            string v = durum.Trim().ToUpper(new CultureInfo("tr-TR"));
            return v == "AYRILDI" ? "AYRILDI" : "DEVAM EDİYOR";
        }

        private static int ToInt(object value)
        {
            int n;
            return int.TryParse(Convert.ToString(value), out n) ? n : 0;
        }

        private async Task AddPersonelSafeAsync(Personel_Model model)
        {
            if (_personelService == null)
                return;

            try { await _personelService.AddPersonelAsync(model); }
            catch { await AddPersonelDirectAsync(model); }
        }

        private async Task UpdatePersonelSafeAsync(Personel_Model model)
        {
            if (_personelService == null)
                return;

            try { await _personelService.UpdatePersonelAsync(model); }
            catch { await UpdatePersonelDirectAsync(model); }
        }

        private async Task DeleteOrMarkAsAyrildiAsync(int personelId)
        {
            if (_personelService == null)
                return;

            try { await _personelService.DeletePersonelAsync(personelId); }
            catch { await MarkPersonelAsAyrildiDirectAsync(personelId); }
        }

        private Task AddPersonelDirectAsync(Personel_Model model)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                    throw new InvalidOperationException("Veritabanı bağlantısı bulunamadı.");

                const string sql = @"
INSERT INTO PERSONEL (TC_NO, ADI, SOYADI, PERSONEL_DURUMU, DOGUM_TARIHI, SOZ_BASLAMA_TAR, RESIM)
VALUES (@TC_NO, @ADI, @SOYADI, @PERSONEL_DURUMU, @DOGUM_TARIHI, @SOZ_BASLAMA_TAR, @RESIM);";

                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@TC_NO", (object)(model.TC_NO ?? string.Empty).Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ADI", (object)(model.ADI ?? string.Empty).Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SOYADI", (object)(model.SOYADI ?? string.Empty).Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PERSONEL_DURUMU", (object)(model.PERSONEL_DURUMU ?? string.Empty).Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DOGUM_TARIHI", (object)model.DOGUM_TARIHI ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SOZ_BASLAMA_TAR", (object)model.SOZ_BASLAMA_TAR ?? DBNull.Value);
                    var pResim = cmd.Parameters.Add("@RESIM", SqlDbType.VarBinary, -1);
                    pResim.Value = (object)model.RESIM ?? DBNull.Value;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        private Task UpdatePersonelDirectAsync(Personel_Model model)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                    throw new InvalidOperationException("Veritabanı bağlantısı bulunamadı.");
                if (model.ID <= 0)
                    throw new InvalidOperationException("Güncelleme için personel ID bulunamadı.");

                const string sql = @"
UPDATE PERSONEL
SET TC_NO=@TC_NO,
    ADI=@ADI,
    SOYADI=@SOYADI,
    PERSONEL_DURUMU=@PERSONEL_DURUMU,
    DOGUM_TARIHI=@DOGUM_TARIHI,
    SOZ_BASLAMA_TAR=@SOZ_BASLAMA_TAR,
    RESIM=@RESIM
WHERE ID=@ID;";

                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ID", model.ID);
                    cmd.Parameters.AddWithValue("@TC_NO", (object)(model.TC_NO ?? string.Empty).Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ADI", (object)(model.ADI ?? string.Empty).Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SOYADI", (object)(model.SOYADI ?? string.Empty).Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PERSONEL_DURUMU", (object)(model.PERSONEL_DURUMU ?? string.Empty).Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DOGUM_TARIHI", (object)model.DOGUM_TARIHI ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SOZ_BASLAMA_TAR", (object)model.SOZ_BASLAMA_TAR ?? DBNull.Value);
                    var pResim = cmd.Parameters.Add("@RESIM", SqlDbType.VarBinary, -1);
                    pResim.Value = (object)model.RESIM ?? DBNull.Value;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        private Task MarkPersonelAsAyrildiDirectAsync(int personelId)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                    throw new InvalidOperationException("Veritabanı bağlantısı bulunamadı.");
                if (personelId <= 0)
                    throw new InvalidOperationException("Silme işlemi için personel ID bulunamadı.");

                const string sql = @"
UPDATE PERSONEL
SET PERSONEL_DURUMU = @PERSONEL_DURUMU
WHERE ID = @ID;";

                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ID", personelId);
                    cmd.Parameters.AddWithValue("@PERSONEL_DURUMU", "AYRILDI");
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            });
        }

        private async Task<DataRow> GetPersonelRowByIdDirectAsync(int personelId)
        {
            if (personelId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return null;

            return await Task.Run(() =>
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("SELECT TOP 1 * FROM dbo.PERSONEL WHERE ID=@ID;", con))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("@ID", personelId);
                    var dt = new DataTable();
                    con.Open();
                    da.Fill(dt);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            });
        }

        private async Task UpsertPersonelDirectAsync()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("Veritabanı bağlantısı bulunamadı.");

            await Task.Run(() =>
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    var cols = GetPersonelColumns(con);
                    if (_seciliId <= 0)
                        InsertPersonelDynamic(con, cols);
                    else
                        UpdatePersonelDynamic(con, cols);
                }
            });
        }

        private HashSet<string> GetPersonelColumns(SqlConnection con)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand("SELECT TOP 0 * FROM dbo.PERSONEL;", con))
            using (var r = cmd.ExecuteReader())
            {
                var schema = r.GetSchemaTable();
                foreach (DataRow row in schema.Rows)
                    set.Add(Convert.ToString(row["ColumnName"]));
            }
            return set;
        }

        private void InsertPersonelDynamic(SqlConnection con, HashSet<string> cols)
        {
            var insertCols = new System.Collections.Generic.List<string>();
            var insertVals = new System.Collections.Generic.List<string>();
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "TC_NO", Tnk_TC_NO.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "ADI", Tnk_ADI.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "SOYADI", Tnk_SOYADI.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "GSM_1", Tnk_GSM_1.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "PERSONEL_DURUMU", NormalizeDurum(Cmb_durumu.Text));
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "DOGUM_TARIHI", TarihOku(Tnk_DOGUM_TARIHI));
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "SOZ_BASLAMA_TAR", TarihOku(Date_Sozlesme));
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "CINSIYET", Cmb_Cinsiyet.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "MEDENI_DUR", Cmb_Medeni.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "YONETICI_GOREVI", Cmb_Yonetici.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "VERDIGI_DERS_1", Cmb_Gorev1.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "EHLIYET_SINIFI", Cmb_SINIFI.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "EHLIYET_IKINCI", Cmd_ikinci.Text);
                AddInsertIfExists(cmd, cols, insertCols, insertVals, "RESIM", Tnk_RESIM_Personel.Image != null ? ImageToByte(Tnk_RESIM_Personel.Image) : null, SqlDbType.VarBinary);

                if (insertCols.Count == 0)
                    throw new InvalidOperationException("Kaydedilecek personel kolonu bulunamadı.");

                cmd.CommandText = "INSERT INTO dbo.PERSONEL (" + string.Join(",", insertCols) + ") VALUES (" + string.Join(",", insertVals) + ");";
                cmd.ExecuteNonQuery();
            }
        }

        private void UpdatePersonelDynamic(SqlConnection con, HashSet<string> cols)
        {
            var sets = new System.Collections.Generic.List<string>();
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                AddSetIfExists(cmd, cols, sets, "TC_NO", Tnk_TC_NO.Text);
                AddSetIfExists(cmd, cols, sets, "ADI", Tnk_ADI.Text);
                AddSetIfExists(cmd, cols, sets, "SOYADI", Tnk_SOYADI.Text);
                AddSetIfExists(cmd, cols, sets, "GSM_1", Tnk_GSM_1.Text);
                AddSetIfExists(cmd, cols, sets, "PERSONEL_DURUMU", NormalizeDurum(Cmb_durumu.Text));
                AddSetIfExists(cmd, cols, sets, "DOGUM_TARIHI", TarihOku(Tnk_DOGUM_TARIHI));
                AddSetIfExists(cmd, cols, sets, "SOZ_BASLAMA_TAR", TarihOku(Date_Sozlesme));
                AddSetIfExists(cmd, cols, sets, "CINSIYET", Cmb_Cinsiyet.Text);
                AddSetIfExists(cmd, cols, sets, "MEDENI_DUR", Cmb_Medeni.Text);
                AddSetIfExists(cmd, cols, sets, "YONETICI_GOREVI", Cmb_Yonetici.Text);
                AddSetIfExists(cmd, cols, sets, "VERDIGI_DERS_1", Cmb_Gorev1.Text);
                AddSetIfExists(cmd, cols, sets, "EHLIYET_SINIFI", Cmb_SINIFI.Text);
                AddSetIfExists(cmd, cols, sets, "EHLIYET_IKINCI", Cmd_ikinci.Text);
                AddSetIfExists(cmd, cols, sets, "RESIM", Tnk_RESIM_Personel.Image != null ? ImageToByte(Tnk_RESIM_Personel.Image) : null, SqlDbType.VarBinary);
                cmd.Parameters.AddWithValue("@ID", _seciliId);

                if (sets.Count == 0)
                    throw new InvalidOperationException("Güncellenecek personel kolonu bulunamadı.");

                cmd.CommandText = "UPDATE dbo.PERSONEL SET " + string.Join(",", sets) + " WHERE ID=@ID;";
                cmd.ExecuteNonQuery();
            }
        }

        private static string GetRowString(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
                return string.Empty;
            object v = row[columnName];
            return (v == null || v == DBNull.Value) ? string.Empty : Convert.ToString(v);
        }

        private static object GetRowObject(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
                return null;
            object v = row[columnName];
            return (v == null || v == DBNull.Value) ? null : v;
        }

        private static void AddInsertIfExists(SqlCommand cmd, HashSet<string> cols, System.Collections.Generic.List<string> insertCols, System.Collections.Generic.List<string> insertVals, string col, object value)
        {
            AddInsertIfExists(cmd, cols, insertCols, insertVals, col, value, null);
        }

        private static void AddInsertIfExists(SqlCommand cmd, HashSet<string> cols, System.Collections.Generic.List<string> insertCols, System.Collections.Generic.List<string> insertVals, string col, object value, SqlDbType? type)
        {
            if (!cols.Contains(col))
                return;
            string p = "@P_" + col;
            insertCols.Add("[" + col + "]");
            insertVals.Add(p);
            if (type.HasValue)
            {
                var prm = cmd.Parameters.Add(p, type.Value, -1);
                prm.Value = value ?? DBNull.Value;
            }
            else
            {
                cmd.Parameters.AddWithValue(p, value ?? DBNull.Value);
            }
        }

        private static void AddSetIfExists(SqlCommand cmd, HashSet<string> cols, System.Collections.Generic.List<string> sets, string col, object value)
        {
            AddSetIfExists(cmd, cols, sets, col, value, null);
        }

        private static void AddSetIfExists(SqlCommand cmd, HashSet<string> cols, System.Collections.Generic.List<string> sets, string col, object value, SqlDbType? type)
        {
            if (!cols.Contains(col))
                return;
            string p = "@P_" + col;
            sets.Add("[" + col + "]=" + p);
            if (type.HasValue)
            {
                var prm = cmd.Parameters.Add(p, type.Value, -1);
                prm.Value = value ?? DBNull.Value;
            }
            else
            {
                cmd.Parameters.AddWithValue(p, value ?? DBNull.Value);
            }
        }
    }
}
