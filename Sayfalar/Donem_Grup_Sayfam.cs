using Kolera.Donem.Services;
using Kolera.Donemler;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Donem_Grup_Sayfam : Form
    {
        private IDonemGrupService _service;
        private string _connectionString;
        private int _seciliID = 0;

        // WinForms Designer parametresiz ctor ister.
        public Donem_Grup_Sayfam()
        {
            InitializeComponent();
            _connectionString = string.Empty;
            GridAyarla();
            WireEvents();
        }

        public Donem_Grup_Sayfam(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? string.Empty;
            GridAyarla();
            WireEvents();

            if (IsDesignerMode() || string.IsNullOrWhiteSpace(_connectionString))
                return;

            _service = new DonemGrupService(_connectionString);
        }

        private void WireEvents()
        {
            Load -= Donem_Grup_Sayfam_Load;
            Dtg_goster.SelectionChanged -= Dtg_goster_SelectionChanged;
            Btn_Ekle.Click -= Btn_Ekle_Click;
            Btn_Guncelle.Click -= Btn_Guncelle_Click;
            Btn_Sil.Click -= Btn_Sil_Click;

            Load += Donem_Grup_Sayfam_Load;
            Dtg_goster.SelectionChanged += Dtg_goster_SelectionChanged;
            Btn_Ekle.Click += Btn_Ekle_Click;
            Btn_Guncelle.Click += Btn_Guncelle_Click;
            Btn_Sil.Click += Btn_Sil_Click;
        }

        private static bool IsDesignerMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }
        private async void Donem_Grup_Sayfam_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                Btn_Ekle.Enabled = false;
                Btn_Guncelle.Enabled = false;
                Btn_Sil.Enabled = false;
                return;
            }

            ComboboxlariDoldur();      // Yıl + Ay
            await SubeVeGrupDoldur();  // Veritabanından

            await ListeleAsync();
            Btn_Guncelle.Enabled = false;
            Btn_Sil.Enabled = false;
            await ListeleAsync();
        }

        private void ComboboxlariDoldur()
        {
            int minOffset = GetIntParam("DONEM_YILI_MIN_OFFSET", 5);
            int maxOffset = GetIntParam("DONEM_YILI_MAX_OFFSET", 1);
            var ekstraYillar = GetExtraYears("DONEM_YILI_OZEL_LISTE");

            Cmb_Donemyili.Items.Clear();
            for (int yil = DateTime.Now.Year - minOffset; yil <= DateTime.Now.Year + maxOffset; yil++)
                Cmb_Donemyili.Items.Add(yil.ToString());
            foreach (int y in ekstraYillar)
            {
                if (!Cmb_Donemyili.Items.Contains(y.ToString()))
                    Cmb_Donemyili.Items.Add(y.ToString());
            }

            // Ay
            Cmb_Donemay.Items.Clear();
            for (int ay = 1; ay <= 12; ay++)
                Cmb_Donemay.Items.Add(AyToString(ay));

            // Varsayılan seçim
            if (Cmb_Donemyili.Items.Count > 0)
                Cmb_Donemyili.SelectedItem = DateTime.Now.Year.ToString();

            if (Cmb_Donemay.Items.Count > 0)
                Cmb_Donemay.SelectedItem = AyToString(DateTime.Now.Month);
        }

        private int GetIntParam(string key, int defaultValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                    return defaultValue;
                const string sql = "SELECT TOP 1 ParamValue FROM APP_PARAMETRELER WHERE ParamKey=@k";
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@k", key);
                    con.Open();
                    object o = cmd.ExecuteScalar();
                    int v;
                    return int.TryParse(Convert.ToString(o), out v) && v >= 0 ? v : defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        private List<int> GetExtraYears(string key)
        {
            var list = new List<int>();
            try
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                    return list;
                const string sql = "SELECT TOP 1 ParamValue FROM APP_PARAMETRELER WHERE ParamKey=@k";
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@k", key);
                    con.Open();
                    string s = Convert.ToString(cmd.ExecuteScalar()) ?? string.Empty;
                    foreach (string token in s.Split(','))
                    {
                        int y;
                        if (int.TryParse(token.Trim(), out y) && y > 1900 && y < 3000 && !list.Contains(y))
                            list.Add(y);
                    }
                }
            }
            catch
            {
            }
            return list;
        }

        private string AyToString(int ay)
        {
            switch (ay)
            {
                case 1: return "OCAK";
                case 2: return "ŞUBAT";
                case 3: return "MART";
                case 4: return "NİSAN";
                case 5: return "MAYIS";
                case 6: return "HAZİRAN";
                case 7: return "TEMMUZ";
                case 8: return "AĞUSTOS";
                case 9: return "EYLÜL";
                case 10: return "EKİM";
                case 11: return "KASIM";
                case 12: return "ARALIK";
                default: return ay.ToString();
            }
        }

        private DonemGrupModel FormdanModelAl()
        {
            int yil = 0;
            int.TryParse(Cmb_Donemyili.Text, out yil);

            return new DonemGrupModel
            {
                DonemYili = yil,
                DonemAyi = Cmb_Donemay.Text,
                DonemSubesi = Cmb_Subesi.Text,
                DonemAdi = Text_DonemAdi.Text,
                DonemGrubu = Cmb_Grubu.Text,
                BasTar = Data_Baslama.Value,
                BitTar = Date_Bitis.Value
            };
        }
        private async Task SubeVeGrupDoldur()
        {
            var liste = await GetListeSafeAsync();

            var subeler = liste
                .Where(x => !string.IsNullOrEmpty(x.DonemSubesi))
                .Select(x => x.DonemSubesi)
                .Distinct()
                .ToList();

            var gruplar = liste
                .Where(x => !string.IsNullOrEmpty(x.DonemGrubu))
                .Select(x => x.DonemGrubu)
                .Distinct()
                .ToList();

            Cmb_Subesi.Items.Clear();
            Cmb_Grubu.Items.Clear();

            foreach (var s in subeler)
                Cmb_Subesi.Items.Add(s);

            foreach (var g in gruplar)
                Cmb_Grubu.Items.Add(g);

            if (Cmb_Subesi.Items.Count > 0)
                Cmb_Subesi.SelectedIndex = 0;

            if (Cmb_Grubu.Items.Count > 0)
                Cmb_Grubu.SelectedIndex = 0;
        }


        private async Task ListeleAsync()
        {
            var liste = await GetListeSafeAsync();
            Dtg_goster.DataSource = liste;

            if (Dtg_goster.Columns.Contains("ID"))
                Dtg_goster.Columns["ID"].Visible = false;

            foreach (DataGridViewColumn col in Dtg_goster.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            Dtg_goster.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Dtg_goster.ClearSelection();
        }

        private async Task<List<DonemGrupModel>> GetListeSafeAsync()
        {
            try
            {
                var liste = await _service.ListeAsync();
                if (liste != null && liste.Count > 0)
                    return liste;
            }
            catch
            {
            }

            return await GetListeFallbackAsync();
        }

        private Task<List<DonemGrupModel>> GetListeFallbackAsync()
        {
            return Task.Run(() =>
            {
                var result = new List<DonemGrupModel>();
                if (string.IsNullOrWhiteSpace(_connectionString))
                    return result;

                const string sql = @"
SELECT ID, DONEM_YILI, DONEM_AYI, DONEM_SUBESI, DONEM_GRUBU, DONEM_ADI, BAS_TAR, BIT_TAR
FROM GRUP_KARTI
ORDER BY ID DESC";

                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var m = new DonemGrupModel();
                            m.ID = r["ID"] == DBNull.Value ? 0 : Convert.ToInt32(r["ID"]);
                            m.DonemYili = ParseInt(r["DONEM_YILI"]);
                            m.DonemAyi = Convert.ToString(r["DONEM_AYI"]);
                            m.DonemSubesi = Convert.ToString(r["DONEM_SUBESI"]);
                            m.DonemGrubu = Convert.ToString(r["DONEM_GRUBU"]);
                            m.DonemAdi = Convert.ToString(r["DONEM_ADI"]);
                            m.BasTar = r["BAS_TAR"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["BAS_TAR"]);
                            m.BitTar = r["BIT_TAR"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["BIT_TAR"]);
                            result.Add(m);
                        }
                    }
                }

                return result;
            });
        }

        private int ParseInt(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;
            int i;
            return int.TryParse(Convert.ToString(value), out i) ? i : 0;
        }

        private void Dtg_goster_SelectionChanged(object sender, EventArgs e)
        {
            bool secili = Dtg_goster.SelectedRows.Count > 0;
            Btn_Guncelle.Enabled = secili;
            Btn_Sil.Enabled = secili;

            if (!secili)
            {
                _seciliID = 0;
                return;
            }

            var row = Dtg_goster.SelectedRows[0];

            _seciliID = Convert.ToInt32(row.Cells["ID"].Value);

            // Formu doldur
            Cmb_Donemyili.Text = row.Cells["DonemYili"].Value?.ToString();
            Cmb_Donemay.Text = row.Cells["DonemAyi"].Value?.ToString();
            Cmb_Subesi.Text = row.Cells["DonemSubesi"].Value?.ToString();
            Cmb_Grubu.Text = row.Cells["DonemGrubu"].Value?.ToString();

            Text_DonemAdi.Text = row.Cells["DonemAdi"].Value?.ToString();

            if (row.Cells["BasTar"].Value != null)
                Data_Baslama.Value = Convert.ToDateTime(row.Cells["BasTar"].Value);

            if (row.Cells["BitTar"].Value != null)
                Date_Bitis.Value = Convert.ToDateTime(row.Cells["BitTar"].Value);
        }


        private async void Btn_Ekle_Click(object sender, EventArgs e)
        {
            try
            {
                var model = FormdanModelAl();
                int etkilenen = await EkleSafeAsync(model);
                if (etkilenen <= 0)
                {
                    MessageBox.Show("Ekleme başarısız.");
                    return;
                }
                await ListeleAsync();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ekleme hatası: " + ex.Message);
            }
        }

        private async void Btn_Guncelle_Click(object sender, EventArgs e)
        {
            if (_seciliID == 0) return;

            var model = FormdanModelAl();
            model.ID = _seciliID;

            int etkilenen = await GuncelleSafeAsync(model);

            if (etkilenen > 0)
                MessageBox.Show("Güncelleme başarılı");
            else
                MessageBox.Show("Güncellenecek kayıt bulunamadı");

            await ListeleAsync();
            Temizle();
        }


        private async void Btn_Sil_Click(object sender, EventArgs e)
        {
            if (_seciliID == 0) return;

            if (MessageBox.Show("Seçili dönem silinecek. Emin misiniz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int etkilenen = await SilSafeAsync(_seciliID);
                if (etkilenen <= 0)
                {
                    MessageBox.Show("Silinecek kayıt bulunamadı.");
                    return;
                }
                await ListeleAsync();
                Temizle();
            }
        }

        private async Task<int> EkleSafeAsync(DonemGrupModel model)
        {
            if (_service != null)
            {
                try
                {
                    await _service.EkleAsync(model);
                    return 1;
                }
                catch
                {
                }
            }
            return await EkleDirectAsync(model);
        }

        private async Task<int> GuncelleSafeAsync(DonemGrupModel model)
        {
            if (_service != null)
            {
                try
                {
                    return await _service.GuncelleAsync(model);
                }
                catch
                {
                }
            }
            return await GuncelleDirectAsync(model);
        }

        private async Task<int> SilSafeAsync(int id)
        {
            if (_service != null)
            {
                try
                {
                    await _service.SilAsync(id);
                    return 1;
                }
                catch
                {
                }
            }
            return await SilDirectAsync(id);
        }

        private Task<int> EkleDirectAsync(DonemGrupModel model)
        {
            return Task.Run(() =>
            {
                const string sql = @"
INSERT INTO GRUP_KARTI (DONEM_YILI, DONEM_AYI, DONEM_SUBESI, DONEM_ADI, DONEM_GRUBU, BAS_TAR, BIT_TAR)
VALUES (@YIL, @AY, @SUBE, @ADI, @GRUP, @BAS, @BIT);";

                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@YIL", model.DonemYili);
                    cmd.Parameters.AddWithValue("@AY", (object)model.DonemAyi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SUBE", (object)model.DonemSubesi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ADI", (object)model.DonemAdi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GRUP", (object)model.DonemGrubu ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BAS", model.BasTar);
                    cmd.Parameters.AddWithValue("@BIT", model.BitTar);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            });
        }

        private Task<int> GuncelleDirectAsync(DonemGrupModel model)
        {
            return Task.Run(() =>
            {
                const string sql = @"
UPDATE GRUP_KARTI
SET DONEM_YILI=@YIL, DONEM_AYI=@AY, DONEM_SUBESI=@SUBE, DONEM_ADI=@ADI, DONEM_GRUBU=@GRUP, BAS_TAR=@BAS, BIT_TAR=@BIT
WHERE ID=@ID;";

                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ID", model.ID);
                    cmd.Parameters.AddWithValue("@YIL", model.DonemYili);
                    cmd.Parameters.AddWithValue("@AY", (object)model.DonemAyi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SUBE", (object)model.DonemSubesi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ADI", (object)model.DonemAdi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GRUP", (object)model.DonemGrubu ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BAS", model.BasTar);
                    cmd.Parameters.AddWithValue("@BIT", model.BitTar);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            });
        }

        private Task<int> SilDirectAsync(int id)
        {
            return Task.Run(() =>
            {
                const string sql = "DELETE FROM GRUP_KARTI WHERE ID=@ID;";
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    con.Open();
                    return cmd.ExecuteNonQuery();
                }
            });
        }

        #region GRID

        private void GridAyarla()
        {
            Dtg_goster.AutoGenerateColumns = true;
            Dtg_goster.ReadOnly = true;
            Dtg_goster.MultiSelect = false;
            Dtg_goster.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            Dtg_goster.AllowUserToAddRows = false;
            Dtg_goster.AllowUserToDeleteRows = false;
            Dtg_goster.AllowUserToResizeColumns = false;
            Dtg_goster.AllowUserToResizeRows = false;
            Dtg_goster.AllowUserToOrderColumns = false;

            Dtg_goster.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            Dtg_goster.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;

            Dtg_goster.RowHeadersVisible = false;
            Dtg_goster.RowTemplate.Height = 65;

            Dtg_goster.EnableHeadersVisualStyles = false;
            Dtg_goster.ColumnHeadersHeight = 40;
            Dtg_goster.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            Dtg_goster.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dtg_goster.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            Dtg_goster.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            Dtg_goster.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            Dtg_goster.DefaultCellStyle.SelectionForeColor = Color.White;

            Dtg_goster.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            Dtg_goster.BackgroundColor = Color.White;
            Dtg_goster.GridColor = Color.Gainsboro;
            Dtg_goster.BorderStyle = BorderStyle.None;
            Dtg_goster.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        }

        #endregion

        private void Temizle()
        {
            _seciliID = 0;
             Cmb_Donemyili.SelectedIndex = -1;
            Text_DonemAdi.Clear();
            Cmb_Grubu.SelectedIndex = -1;
            Cmb_Donemay.SelectedIndex = -1;
            Cmb_Subesi.SelectedIndex = -1;
            Data_Baslama.Value = DateTime.Now;
            Date_Bitis.Value = DateTime.Now;
            Dtg_goster.ClearSelection();
        }

        private void Btn_Guncelle_Click_1(object sender, EventArgs e)
        {

        }
    }
}




