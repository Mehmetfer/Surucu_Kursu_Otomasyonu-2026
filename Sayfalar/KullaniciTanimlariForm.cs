using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using Kolera_Mtsk.Services;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class KullaniciTanimlariForm : Form
    {
        private const string UserPermissionParamPrefix = "KULLANICI_YETKI_";
        private readonly string _connectionString;
        private DataTable _kullaniciTable;
        private string _colId;
        private string _colUser;
        private string _colPass;
        private string _colRole;
        private int? _editingId;
        private bool _isCurrentUserAdmin;
        private bool _suppressYetkiEvents;

        public KullaniciTanimlariForm() : this(string.Empty)
        {
        }

        public KullaniciTanimlariForm(string connectionString)
        {
            _connectionString = connectionString ?? string.Empty;
            InitializeComponent();
            Shown += KullaniciTanimlariForm_Shown;
            dgvYetkiler.CellValueChanged += dgvYetkiler_CellValueChanged;
            dgvYetkiler.CurrentCellDirtyStateChanged += dgvYetkiler_CurrentCellDirtyStateChanged;
        }

        private void KullaniciTanimlariForm_Shown(object sender, EventArgs e)
        {
            YukleKullaniciListesi();
            YukleSabitYetkiler();
            ApplyYetkiToUi();
        }

        private void YukleKullaniciListesi()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = "SELECT TOP (1000) ID, KULLANICI_ADI, KULLANICI_SIFRE, KAYIT_TARIHI, YETKI FROM KULLANICI";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    _kullaniciTable = new DataTable();
                    da.Fill(_kullaniciTable);
                    KolonlariBelirle();
                    _isCurrentUserAdmin = GirisYapanAdminMi();
                    dgvKullanicilar.DataSource = _kullaniciTable;
                    GuncelleSeciliKullaniciBaslik();
                    ApplyYetkiToUi();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanici listesi yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void YukleSabitYetkiler()
        {
            if (dgvYetkiler.Rows.Count > 0)
                return;

            string[] satirlar =
            {
                "YARDIM",
                "PARAMETRELER",
                "RAPORLAR",
                "TAKIP/SMS/PERSONEL",
                "FINANS",
                "SINAV ISLEMLERI",
                "DERS PROGRAMI ISLEMLERI",
                "KURSIYER/GRUP ISLEMLERI",
                "KURSIYER KARTI",
                "RAPOR EKRANI",
                "DIGER YETKILER"
            };

            foreach (var satir in satirlar)
                dgvYetkiler.Rows.Add(satir, true);
        }

        private void dgvKullanicilar_SelectionChanged(object sender, EventArgs e)
        {
            GuncelleSeciliKullaniciBaslik();
            SeciliKaydiFormaDoldur();
        }

        private void GuncelleSeciliKullaniciBaslik()
        {
            if (dgvKullanicilar.CurrentRow == null)
            {
                lblSecilenKullanici.Text = "Secili kullanici yok";
                return;
            }

            var row = dgvKullanicilar.CurrentRow;
            string ad = DegerGetir(row, "KULLANICI_ADI");
            if (string.IsNullOrWhiteSpace(ad) && row.Cells.Count > 0)
                ad = Convert.ToString(row.Cells[0].Value);

            lblSecilenKullanici.Text = string.IsNullOrWhiteSpace(ad)
                ? "Secili kullanici yok"
                : ad.ToUpperInvariant() + " - KULLANICI YETKILERI";
        }

        private static string DegerGetir(DataGridViewRow row, string kolonAdi)
        {
            if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(kolonAdi))
                return string.Empty;
            var val = row.Cells[kolonAdi].Value;
            return val == null ? string.Empty : val.ToString().Trim();
        }

        private void KolonlariBelirle()
        {
            _colId = KolonBul("ID");
            _colUser = KolonBul("KULLANICI_ADI");
            _colPass = KolonBul("KULLANICI_SIFRE");
            _colRole = KolonBul("YETKI");
        }

        private string KolonBul(params string[] adaylar)
        {
            if (_kullaniciTable == null) return null;
            foreach (var a in adaylar)
            {
                foreach (DataColumn c in _kullaniciTable.Columns)
                {
                    if (string.Equals(c.ColumnName, a, StringComparison.OrdinalIgnoreCase))
                        return c.ColumnName;
                }
            }
            return null;
        }

        private bool GirisYapanAdminMi()
        {
            if (_kullaniciTable == null || string.IsNullOrWhiteSpace(_colUser) || string.IsNullOrWhiteSpace(_colRole))
                return false;

            string aktif = (AppSession.CurrentUserName ?? string.Empty).Trim();
            foreach (DataRow r in _kullaniciTable.Rows)
            {
                var user = Convert.ToString(r[_colUser]).Trim();
                if (!string.Equals(user, aktif, StringComparison.OrdinalIgnoreCase))
                    continue;
                var role = Convert.ToString(r[_colRole]).Trim();
                return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private void ApplyYetkiToUi()
        {
            txtKullaniciAdi.ReadOnly = !_isCurrentUserAdmin;
            txtSifre.ReadOnly = !_isCurrentUserAdmin;
            cmbYetki.Enabled = _isCurrentUserAdmin;
            btnYeni.Enabled = _isCurrentUserAdmin;
            btnKaydet.Enabled = _isCurrentUserAdmin;
            btnSil.Enabled = _isCurrentUserAdmin;
            lblAdminUyari.Visible = !_isCurrentUserAdmin;
            colYetki.ReadOnly = !_isCurrentUserAdmin;
        }

        private void SeciliKaydiFormaDoldur()
        {
            if (dgvKullanicilar.CurrentRow == null)
                return;

            var row = dgvKullanicilar.CurrentRow;
            int id;
            _editingId = (string.IsNullOrWhiteSpace(_colId) || !int.TryParse(DegerGetir(row, _colId), out id)) ? (int?)null : id;
            txtKullaniciAdi.Text = string.IsNullOrWhiteSpace(_colUser) ? string.Empty : DegerGetir(row, _colUser);
            txtSifre.Text = string.IsNullOrWhiteSpace(_colPass) ? string.Empty : DegerGetir(row, _colPass);
            string yetki = string.IsNullOrWhiteSpace(_colRole) ? string.Empty : DegerGetir(row, _colRole);
            if (cmbYetki.Items.IndexOf(yetki) < 0 && !string.IsNullOrWhiteSpace(yetki))
                cmbYetki.Items.Add(yetki);
            cmbYetki.Text = yetki;
            YukleSeciliKullaniciYetkileri();
        }

        private void dgvYetkiler_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvYetkiler.IsCurrentCellDirty)
                dgvYetkiler.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvYetkiler_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressYetkiEvents || e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (!string.Equals(dgvYetkiler.Columns[e.ColumnIndex].Name, colYetki.Name, StringComparison.OrdinalIgnoreCase))
                return;

            KaydetSeciliKullaniciYetkileri();
        }

        private void YukleSeciliKullaniciYetkileri()
        {
            string kullaniciAdi = SeciliKullaniciAdiniGetir();
            _suppressYetkiEvents = true;
            try
            {
                foreach (DataGridViewRow row in dgvYetkiler.Rows)
                    row.Cells[colYetki.Name].Value = true;

                if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(_connectionString))
                    return;

                if (!EnsureAppParametrelerTable())
                    return;

                string key = BuildUserPermissionParamKey(kullaniciAdi);
                string value = string.Empty;

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("SELECT TOP (1) ParamValue FROM APP_PARAMETRELER WHERE ParamKey=@k ORDER BY ID DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@k", key);
                    conn.Open();
                    var o = cmd.ExecuteScalar();
                    value = o == null || o == DBNull.Value ? string.Empty : Convert.ToString(o);
                }

                if (string.IsNullOrWhiteSpace(value))
                    return;

                var enabled = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var trimmed = item.Trim();
                    if (trimmed.Length > 0)
                        enabled.Add(trimmed);
                }

                foreach (DataGridViewRow row in dgvYetkiler.Rows)
                {
                    string ozellik = Convert.ToString(row.Cells[colOzellik.Name].Value ?? string.Empty).Trim();
                    row.Cells[colYetki.Name].Value = enabled.Contains(ozellik);
                }
            }
            catch
            {
                // Yetki listesi okunamazsa ekran varsayilan (tum yetkiler acik) kalsin.
            }
            finally
            {
                _suppressYetkiEvents = false;
            }
        }

        private void KaydetSeciliKullaniciYetkileri()
        {
            if (!_isCurrentUserAdmin)
                return;

            string kullaniciAdi = SeciliKullaniciAdiniGetir();
            if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(_connectionString))
                return;

            try
            {
                if (!EnsureAppParametrelerTable())
                    return;

                var allowed = new System.Collections.Generic.List<string>();
                foreach (DataGridViewRow row in dgvYetkiler.Rows)
                {
                    bool isChecked = row.Cells[colYetki.Name].Value is bool b && b;
                    if (!isChecked)
                        continue;

                    string ozellik = Convert.ToString(row.Cells[colOzellik.Name].Value ?? string.Empty).Trim();
                    if (ozellik.Length > 0)
                        allowed.Add(ozellik);
                }

                string serialized = string.Join("|", allowed);
                string key = BuildUserPermissionParamKey(kullaniciAdi);
                const string sql = @"
IF EXISTS (SELECT 1 FROM APP_PARAMETRELER WHERE ParamKey=@k)
BEGIN
    UPDATE APP_PARAMETRELER
       SET ParamValue=@v,
           Aciklama='Kullanici bazli ekran yetkileri'
     WHERE ParamKey=@k;
END
ELSE
BEGIN
    INSERT INTO APP_PARAMETRELER(ParamKey, ParamValue, Aciklama)
    VALUES(@k, @v, 'Kullanici bazli ekran yetkileri');
END";

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@k", key);
                    cmd.Parameters.AddWithValue("@v", serialized);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanici yetkileri kaydedilemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string SeciliKullaniciAdiniGetir()
        {
            if (dgvKullanicilar.CurrentRow == null || string.IsNullOrWhiteSpace(_colUser))
                return string.Empty;
            return DegerGetir(dgvKullanicilar.CurrentRow, _colUser);
        }

        private static string BuildUserPermissionParamKey(string kullaniciAdi)
        {
            return UserPermissionParamPrefix + (kullaniciAdi ?? string.Empty).Trim().ToUpperInvariant();
        }

        private bool EnsureAppParametrelerTable()
        {
            const string sql = @"
IF OBJECT_ID('dbo.APP_PARAMETRELER','U') IS NULL
BEGIN
    CREATE TABLE dbo.APP_PARAMETRELER(
      ID INT IDENTITY(1,1) PRIMARY KEY,
      ParamKey VARCHAR(120) NOT NULL,
      ParamValue VARCHAR(500) NULL,
      Aciklama VARCHAR(500) NULL
    );
END;";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void btnYeni_Click(object sender, EventArgs e)
        {
            _editingId = null;
            txtKullaniciAdi.Text = string.Empty;
            txtSifre.Text = string.Empty;
            cmbYetki.SelectedIndex = -1;
            txtKullaniciAdi.Focus();
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!_isCurrentUserAdmin)
            {
                MessageBox.Show("Sadece ADMIN kullanici degisiklik yapabilir.", "Yetki", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_colUser) || string.IsNullOrWhiteSpace(_colPass) || string.IsNullOrWhiteSpace(_colRole))
            {
                MessageBox.Show("KULLANICI tablosunda gerekli kolonlar bulunamadi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string user = txtKullaniciAdi.Text.Trim();
            string pass = txtSifre.Text.Trim();
            string role = cmbYetki.Text.Trim();
            if (user.Length == 0 || pass.Length == 0 || role.Length == 0)
            {
                MessageBox.Show("Kullanici adi, sifre ve yetki zorunludur.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    if (_editingId.HasValue && !string.IsNullOrWhiteSpace(_colId))
                    {
                        bool mevcutAdmin = SeciliKayitAdminMi();
                        bool hedefAdminDegil = !string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
                        if (mevcutAdmin && hedefAdminDegil && AdminSayisi() <= 1)
                        {
                            MessageBox.Show("Sistemde en az bir ADMIN kalmak zorunda. Son adminin yetkisi dusurulemez.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        cmd.CommandText = "UPDATE KULLANICI SET [" + _colUser + "]=@u,[" + _colPass + "]=@p,[" + _colRole + "]=@r WHERE [" + _colId + "]=@id";
                        cmd.Parameters.AddWithValue("@id", _editingId.Value);
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO KULLANICI ([" + _colUser + "],[" + _colPass + "],[" + _colRole + "], [KAYIT_TARIHI]) VALUES (@u,@p,@r,GETDATE())";
                    }
                    cmd.Parameters.AddWithValue("@u", user);
                    cmd.Parameters.AddWithValue("@p", pass);
                    cmd.Parameters.AddWithValue("@r", role);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Kullanici kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                YukleKullaniciListesi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydetme hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSil_Click(object sender, EventArgs e)
        {
            if (!_isCurrentUserAdmin)
            {
                MessageBox.Show("Sadece ADMIN kullanici silebilir.", "Yetki", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (dgvKullanicilar.CurrentRow == null)
                return;

            var row = dgvKullanicilar.CurrentRow;
            string yetki = string.IsNullOrWhiteSpace(_colRole) ? string.Empty : DegerGetir(row, _colRole);
            bool silinenAdmin = string.Equals(yetki, "ADMIN", StringComparison.OrdinalIgnoreCase);
            if (silinenAdmin && AdminSayisi() <= 1)
            {
                MessageBox.Show("Sistemde en az bir ADMIN kalmak zorunda. Son admin silinemez.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Secili kullanici silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    if (_editingId.HasValue && !string.IsNullOrWhiteSpace(_colId))
                    {
                        cmd.CommandText = "DELETE FROM KULLANICI WHERE [" + _colId + "]=@id";
                        cmd.Parameters.AddWithValue("@id", _editingId.Value);
                    }
                    else if (!string.IsNullOrWhiteSpace(_colUser))
                    {
                        cmd.CommandText = "DELETE FROM KULLANICI WHERE [" + _colUser + "]=@u";
                        cmd.Parameters.AddWithValue("@u", DegerGetir(row, _colUser));
                    }
                    else
                    {
                        MessageBox.Show("Silme icin anahtar kolon bulunamadi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Kullanici silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                YukleKullaniciListesi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int AdminSayisi()
        {
            if (string.IsNullOrWhiteSpace(_colRole))
                return 0;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM KULLANICI WHERE UPPER(ISNULL([" + _colRole + "],''))='ADMIN'", conn))
                {
                    conn.Open();
                    var val = cmd.ExecuteScalar();
                    int count;
                    return int.TryParse(Convert.ToString(val), out count) ? count : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private bool SeciliKayitAdminMi()
        {
            if (dgvKullanicilar.CurrentRow == null || string.IsNullOrWhiteSpace(_colRole))
                return false;

            string yetki = DegerGetir(dgvKullanicilar.CurrentRow, _colRole);
            return string.Equals(yetki, "ADMIN", StringComparison.OrdinalIgnoreCase);
        }
    }
}
