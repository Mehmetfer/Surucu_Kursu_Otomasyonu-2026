using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Kursiyer_Odeme_Karti : Form
    {
        private readonly int _kursiyerId;
        private readonly string _adSoyad;
        private readonly string _tcNo;
        private readonly string _connectionString;
        private string _odemeKursiyerKolonAdi = "KURSIYER_ID";
        private Panel _pnlBorcOzet;

        public Kursiyer_Odeme_Karti()
            : this(string.Empty, 0, string.Empty, string.Empty)
        {
        }

        public Kursiyer_Odeme_Karti(string connectionString, int kursiyerId, string adSoyad, string tcNo)
        {
            InitializeComponent();
            UygulaKurumsalTasarim();
            _connectionString = connectionString ?? string.Empty;
            _kursiyerId = kursiyerId;
            _adSoyad = adSoyad ?? string.Empty;
            _tcNo = tcNo ?? string.Empty;
            Load += Kursiyer_Odeme_Karti_Load;
            btnYeniSatir.Click += BtnYeniSatir_Click;
            btnSilSatir.Click += BtnSilSatir_Click;
            btnOdemeYap.Click += BtnOdemeYap_Click;
            btnMakbuz.Click += BtnOdemeYap_Click;
            btnMakbuzGor.Click += BtnMakbuzGor_Click;
            btnMakbuzSil.Click += BtnMakbuzSil_Click;
            btnMakbuzDuzelt.Click += BtnMakbuzDuzelt_Click;
            btnKaydet.Click += BtnKaydet_Click;
            btnKapat.Click += BtnKapat_Click;
        }

        private void Kursiyer_Odeme_Karti_Load(object sender, EventArgs e)
        {
            if (_kursiyerId > 0)
                lblBaslik.Text = "Kursiyer Odeme Karti - ID: " + _kursiyerId;
            if (!string.IsNullOrWhiteSpace(_adSoyad))
                txtAdSoyad.Text = _adSoyad;
            if (!string.IsNullOrWhiteSpace(_tcNo))
                txtTcNo.Text = _tcNo;
            EnsureOdemeColumns();
            EnsureOdemeHareketTable();
            _odemeKursiyerKolonAdi = GetOdemeKursiyerKolonAdi();
            YukleBorcBilgileri();
            YukleOdemeHareketleri();
        }

        private void UygulaKurumsalTasarim()
        {
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(920, 620);

            lblBaslik.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
            lblBaslik.ForeColor = Color.FromArgb(35, 55, 86);

            grpKursiyer.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpKursiyer.ForeColor = Color.FromArgb(35, 55, 86);
            grpKursiyer.BackColor = Color.White;

            grpOdemeler.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpOdemeler.ForeColor = Color.FromArgb(35, 55, 86);
            grpOdemeler.BackColor = Color.White;
            HazirlaBorcOzetPaneli();

            pnlButtons.BackColor = Color.FromArgb(236, 239, 243);
            StilButon(btnYeniSatir, Color.FromArgb(52, 152, 219));
            StilButon(btnSilSatir, Color.FromArgb(231, 76, 60));
            StilButon(btnOdemeYap, Color.FromArgb(142, 68, 173));
            StilButon(btnMakbuz, Color.FromArgb(41, 128, 185));
            StilButon(btnMakbuzGor, Color.FromArgb(52, 73, 94));
            StilButon(btnMakbuzSil, Color.FromArgb(192, 57, 43));
            StilButon(btnMakbuzDuzelt, Color.FromArgb(243, 156, 18));
            StilButon(btnKaydet, Color.FromArgb(46, 125, 50));
            StilButon(btnKapat, Color.FromArgb(52, 73, 94));

            dgvOdemeler.EnableHeadersVisualStyles = false;
            dgvOdemeler.BackgroundColor = Color.White;
            dgvOdemeler.BorderStyle = BorderStyle.None;
            dgvOdemeler.GridColor = Color.FromArgb(220, 224, 230);
            dgvOdemeler.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 255);
            dgvOdemeler.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvOdemeler.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvOdemeler.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvOdemeler.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvOdemeler.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvOdemeler.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvOdemeler.RowTemplate.Height = 26;
            dgvOdemeler.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOdemeler.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgvOdemeler.MultiSelect = false;
            dgvOdemeler.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOdemeler.RowHeadersVisible = false;
            dgvOdemeler.ColumnHeadersHeight = 34;
            dgvOdemeler.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvOdemeler.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvOdemeler.DefaultCellStyle.BackColor = Color.White;
            dgvOdemeler.DefaultCellStyle.ForeColor = Color.FromArgb(41, 52, 64);
            dgvOdemeler.DefaultCellStyle.Padding = new Padding(3, 0, 3, 0);
            dgvOdemeler.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOdemeler.Columns["colTarih"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvOdemeler.Columns["colToplam"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvOdemeler.Columns["colOdenen"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvOdemeler.Columns["colKalan"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            btnYeniSatir.Text = "BORCLANDIR";
            lblToplamBorc.ForeColor = Color.DarkRed;
            lblToplamOdenen.ForeColor = Color.DarkRed;
            lblKalanBorc.ForeColor = Color.DarkRed;
            lblToplamBorc.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblToplamOdenen.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblKalanBorc.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtToplamBorc.ForeColor = Color.DarkRed;
            txtToplamOdenen.ForeColor = Color.DarkRed;
            txtKalanBorc.ForeColor = Color.DarkRed;
            txtToplamBorc.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtToplamOdenen.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            txtKalanBorc.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private static void StilButon(Button button, Color renk)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = renk;
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            button.Height = 30;
        }

        private void HazirlaBorcOzetPaneli()
        {
            if (_pnlBorcOzet == null)
            {
                _pnlBorcOzet = new Panel
                {
                    Name = "pnlBorcOzet",
                    Height = 44,
                    Dock = DockStyle.Bottom,
                    BackColor = Color.FromArgb(252, 244, 244)
                };
                grpOdemeler.Controls.Add(_pnlBorcOzet);
                _pnlBorcOzet.BringToFront();
            }

            txtToplamBorc.Parent = _pnlBorcOzet;
            lblToplamBorc.Parent = _pnlBorcOzet;
            txtToplamOdenen.Parent = _pnlBorcOzet;
            lblToplamOdenen.Parent = _pnlBorcOzet;
            txtKalanBorc.Parent = _pnlBorcOzet;
            lblKalanBorc.Parent = _pnlBorcOzet;

            lblToplamBorc.Location = new Point(10, 14);
            txtToplamBorc.Location = new Point(98, 10);
            txtToplamBorc.Size = new Size(120, 23);

            lblToplamOdenen.Location = new Point(235, 14);
            txtToplamOdenen.Location = new Point(334, 10);
            txtToplamOdenen.Size = new Size(130, 23);

            lblKalanBorc.Location = new Point(486, 14);
            txtKalanBorc.Location = new Point(558, 10);
            txtKalanBorc.Size = new Size(130, 23);
        }

        private void BtnYeniSatir_Click(object sender, EventArgs e)
        {
            decimal mevcutToplam = ParseMoney(txtToplamBorc.Text);
            decimal yeniBorc = TutarGir("Borclandir", "Toplam borc tutarini TL olarak giriniz:", mevcutToplam);
            if (yeniBorc < 0m)
                return;

            txtToplamBorc.Text = ToMoneyText(yeniBorc);
            RecalculateTotalsFromGrid();
        }

        private void BtnSilSatir_Click(object sender, EventArgs e)
        {
            if (dgvOdemeler.CurrentRow == null || dgvOdemeler.CurrentRow.IsNewRow)
                return;
            dgvOdemeler.Rows.Remove(dgvOdemeler.CurrentRow);
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            decimal toplamBorc = ParseMoney(txtToplamBorc.Text);
            decimal toplamOdenen = ParseMoney(txtToplamOdenen.Text);
            decimal kalanBorc = ParseMoney(txtKalanBorc.Text);

            if (_kursiyerId <= 0)
            {
                MessageBox.Show("Gecerli bir kursiyer secimi yok.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabani baglantisi yok.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string sql = @"UPDATE KURSIYER
SET TOPLAM_BORC=@TOPLAM_BORC,
    TOPLAM_ODENEN=@TOPLAM_ODENEN,
    KALANBORC=@KALANBORC
WHERE ID=@ID;";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        using (var cmd = new SqlCommand(sql, con, tx))
                        {
                            cmd.Parameters.AddWithValue("@ID", _kursiyerId);
                            cmd.Parameters.AddWithValue("@TOPLAM_BORC", toplamBorc);
                            cmd.Parameters.AddWithValue("@TOPLAM_ODENEN", toplamOdenen);
                            cmd.Parameters.AddWithValue("@KALANBORC", kalanBorc);
                            cmd.ExecuteNonQuery();
                        }

                        string silSql = "DELETE FROM dbo.KURSIYER_ODEME_HAREKET WHERE " + _odemeKursiyerKolonAdi + "=@ID;";
                        using (var silCmd = new SqlCommand(silSql, con, tx))
                        {
                            silCmd.Parameters.AddWithValue("@ID", _kursiyerId);
                            silCmd.ExecuteNonQuery();
                        }

                        string insertSql = @"INSERT INTO dbo.KURSIYER_ODEME_HAREKET
(" + _odemeKursiyerKolonAdi + @", MAKBUZ_NO, TARIH, TOPLAM, ODENEN, KALAN, NOTLAR)
VALUES
(@KURSIYER_ID, @MAKBUZ_NO, @TARIH, @TOPLAM, @ODENEN, @KALAN, @NOTLAR);";
                        foreach (var row in BuildOdemeRowsFromGrid())
                        {
                            using (var ekleCmd = new SqlCommand(insertSql, con, tx))
                            {
                                ekleCmd.Parameters.AddWithValue("@KURSIYER_ID", _kursiyerId);
                                ekleCmd.Parameters.AddWithValue("@MAKBUZ_NO", row.MakbuzNo.HasValue ? (object)row.MakbuzNo.Value : DBNull.Value);
                                ekleCmd.Parameters.AddWithValue("@TARIH", row.TarihText);
                                ekleCmd.Parameters.AddWithValue("@TOPLAM", row.Toplam);
                                ekleCmd.Parameters.AddWithValue("@ODENEN", row.Odenen);
                                ekleCmd.Parameters.AddWithValue("@KALAN", row.Kalan);
                                ekleCmd.Parameters.AddWithValue("@NOTLAR", row.Notlar);
                                ekleCmd.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                }

                MessageBox.Show("Odeme bilgileri kursiyer kaydina islenmistir.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayit hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOdemeYap_Click(object sender, EventArgs e)
        {
            string adSoyad = (txtAdSoyad.Text ?? string.Empty).Trim();
            string aciklama = (txtAciklama.Text ?? string.Empty).Trim();
            decimal kalan = ParseMoney(txtKalanBorc.Text);
            int makbuzNo = GetNextMakbuzNo();
            using (var frm = new TahsilatForm(adSoyad, _tcNo, kalan, aciklama, makbuzNo))
            {
                if (frm.ShowDialog(this) != DialogResult.OK || frm.Sonuc == null)
                    return;

                var sonuc = frm.Sonuc;
                decimal toplamBorc = ParseMoney(txtToplamBorc.Text);
                decimal toplamOdenen = ParseMoney(txtToplamOdenen.Text) + sonuc.OdenecekTutar;
                decimal kalanBorc = toplamBorc - toplamOdenen;
                if (kalanBorc < 0m)
                    kalanBorc = 0m;

                txtToplamOdenen.Text = toplamOdenen.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
                txtKalanBorc.Text = kalanBorc.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));

                string notText = string.IsNullOrWhiteSpace(sonuc.Aciklama)
                    ? "Makbuz No: " + sonuc.MakbuzNo.ToString(CultureInfo.InvariantCulture)
                    : "Makbuz No: " + sonuc.MakbuzNo.ToString(CultureInfo.InvariantCulture) + " - " + sonuc.Aciklama;

                dgvOdemeler.Rows.Add(
                    sonuc.OdemeTarihi.ToString("dd.MM.yyyy"),
                    txtToplamBorc.Text,
                    sonuc.OdenecekTutar.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")),
                    txtKalanBorc.Text,
                    notText,
                    sonuc.MakbuzNo);

                RecalculateTotalsFromGrid();
                BtnKaydet_Click(this, EventArgs.Empty);
            }
        }

        private void BtnMakbuzGor_Click(object sender, EventArgs e)
        {
            var row = GetSelectedMakbuzRow();
            if (row == null)
            {
                MessageBox.Show("Makbuz görüntülemek için makbuzlu bir ödeme satırı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int makbuzNo = ParseMakbuzNo(row.Cells[5].Value);
            DateTime odemeTarihi = ParseDate(row.Cells[0].Value);
            decimal odenen = ParseMoney(Convert.ToString(row.Cells[2].Value));
            string aciklama = Convert.ToString(row.Cells[4].Value);
            using (var frm = new TahsilatForm(txtAdSoyad.Text, _tcNo, ParseMoney(txtKalanBorc.Text), aciklama, makbuzNo, odemeTarihi, odenen, true))
            {
                frm.ShowDialog(this);
            }
        }

        private void BtnMakbuzSil_Click(object sender, EventArgs e)
        {
            var row = GetSelectedMakbuzRow();
            if (row == null)
            {
                MessageBox.Show("Silmek için makbuzlu bir ödeme satırı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (MessageBox.Show("Seçili makbuz silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            dgvOdemeler.Rows.Remove(row);
            RecalculateTotalsFromGrid();
            BtnKaydet_Click(this, EventArgs.Empty);
        }

        private void BtnMakbuzDuzelt_Click(object sender, EventArgs e)
        {
            var row = GetSelectedMakbuzRow();
            if (row == null)
            {
                MessageBox.Show("Düzeltmek için makbuzlu bir ödeme satırı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int makbuzNo = ParseMakbuzNo(row.Cells[5].Value);
            DateTime odemeTarihi = ParseDate(row.Cells[0].Value);
            decimal odenen = ParseMoney(Convert.ToString(row.Cells[2].Value));
            string aciklama = Convert.ToString(row.Cells[4].Value);
            using (var frm = new TahsilatForm(txtAdSoyad.Text, _tcNo, ParseMoney(txtKalanBorc.Text), aciklama, makbuzNo, odemeTarihi, odenen, false))
            {
                if (frm.ShowDialog(this) != DialogResult.OK || frm.Sonuc == null)
                    return;

                var sonuc = frm.Sonuc;
                row.Cells[0].Value = sonuc.OdemeTarihi.ToString("dd.MM.yyyy");
                row.Cells[2].Value = sonuc.OdenecekTutar.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
                row.Cells[4].Value = "Makbuz No: " + sonuc.MakbuzNo.ToString(CultureInfo.InvariantCulture) +
                                     (string.IsNullOrWhiteSpace(sonuc.Aciklama) ? string.Empty : " - " + sonuc.Aciklama);
                row.Cells[5].Value = sonuc.MakbuzNo;

                RecalculateTotalsFromGrid();
                BtnKaydet_Click(this, EventArgs.Empty);
            }
        }

        private void BtnKapat_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void YukleBorcBilgileri()
        {
            if (_kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = @"SELECT TOPLAM_BORC, TOPLAM_ODENEN, KALANBORC FROM KURSIYER WHERE ID=@ID";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@ID", _kursiyerId);
                    con.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (!r.Read())
                            return;
                        txtToplamBorc.Text = ToMoneyText(r["TOPLAM_BORC"]);
                        txtToplamOdenen.Text = ToMoneyText(r["TOPLAM_ODENEN"]);
                        txtKalanBorc.Text = ToMoneyText(r["KALANBORC"]);
                    }
                }
            }
            catch
            {
            }
        }

        private void EnsureOdemeColumns()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = @"
IF OBJECT_ID('dbo.KURSIYER','U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.KURSIYER','TOPLAM_BORC') IS NULL
        ALTER TABLE dbo.KURSIYER ADD TOPLAM_BORC MONEY NULL;
    IF COL_LENGTH('dbo.KURSIYER','TOPLAM_ODENEN') IS NULL
        ALTER TABLE dbo.KURSIYER ADD TOPLAM_ODENEN MONEY NULL;
    IF COL_LENGTH('dbo.KURSIYER','KALANBORC') IS NULL
        ALTER TABLE dbo.KURSIYER ADD KALANBORC MONEY NULL;
END";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Kolon olusturma yetkisi yoksa sessiz gec; kaydetme/yukleme asamasinda detayli hata gosterilir.
            }
        }

        private void EnsureOdemeHareketTable()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = @"
IF OBJECT_ID('dbo.KURSIYER_ODEME_HAREKET','U') IS NULL
BEGIN
    CREATE TABLE dbo.KURSIYER_ODEME_HAREKET
    (
        ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        KURSIYER_ID INT NOT NULL,
        MAKBUZ_NO INT NULL,
        TARIH NVARCHAR(20) NULL,
        TOPLAM MONEY NULL,
        ODENEN MONEY NULL,
        KALAN MONEY NULL,
        NOTLAR NVARCHAR(500) NULL
    );
    CREATE INDEX IX_KURSIYER_ODEME_HAREKET_KURSIYER_ID ON dbo.KURSIYER_ODEME_HAREKET(KURSIYER_ID);
END";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"IF COL_LENGTH('dbo.KURSIYER_ODEME_HAREKET','MAKBUZ_NO') IS NULL
ALTER TABLE dbo.KURSIYER_ODEME_HAREKET ADD MAKBUZ_NO INT NULL;", con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
            }
        }

        private void YukleOdemeHareketleri()
        {
            dgvOdemeler.Rows.Clear();
            if (_kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return;

            try
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    var columns = GetTableColumns(con, "KURSIYER_ODEME_HAREKET");
                    if (columns.Count == 0 || !_odemeKursiyerKolonAdi.Any() || !columns.Contains(_odemeKursiyerKolonAdi))
                        return;

                    string orderByCol = columns.Contains("ID") ? "ID" : (columns.Contains("TARIH") ? "TARIH" : _odemeKursiyerKolonAdi);
                    string sql = @"SELECT " +
                                 (columns.Contains("MAKBUZ_NO") ? "MAKBUZ_NO" : "NULL AS MAKBUZ_NO") + ", " +
                                 (columns.Contains("TARIH") ? "TARIH" : "NULL AS TARIH") + ", " +
                                 (columns.Contains("TOPLAM") ? "TOPLAM" : "NULL AS TOPLAM") + ", " +
                                 (columns.Contains("ODENEN") ? "ODENEN" : "NULL AS ODENEN") + ", " +
                                 (columns.Contains("KALAN") ? "KALAN" : "NULL AS KALAN") + ", " +
                                 (columns.Contains("NOTLAR") ? "NOTLAR" : "NULL AS NOTLAR") +
                                 " FROM dbo.KURSIYER_ODEME_HAREKET WHERE " + _odemeKursiyerKolonAdi + "=@ID ORDER BY " + orderByCol + " ASC;";

                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@ID", _kursiyerId);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                dgvOdemeler.Rows.Add(
                                    (r["TARIH"] ?? string.Empty).ToString(),
                                    ToMoneyText(r["TOPLAM"]),
                                    ToMoneyText(r["ODENEN"]),
                                    ToMoneyText(r["KALAN"]),
                                    (r["NOTLAR"] ?? string.Empty).ToString(),
                                    r["MAKBUZ_NO"] == DBNull.Value ? (object)DBNull.Value : Convert.ToInt32(r["MAKBUZ_NO"], CultureInfo.InvariantCulture));
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            if (dgvOdemeler.Rows.Count == 0)
            {
                dgvOdemeler.Rows.Add(
                    DateTime.Today.ToString("dd.MM.yyyy"),
                    txtToplamBorc.Text,
                    txtToplamOdenen.Text,
                    txtKalanBorc.Text,
                    "",
                    DBNull.Value);
            }
        }

        private List<OdemeSatiri> BuildOdemeRowsFromGrid()
        {
            var rows = new List<OdemeSatiri>();
            foreach (DataGridViewRow dgRow in dgvOdemeler.Rows)
            {
                if (dgRow == null || dgRow.IsNewRow)
                    continue;

                string tarih = (dgRow.Cells[0].Value ?? string.Empty).ToString().Trim();
                string notlar = (dgRow.Cells[4].Value ?? string.Empty).ToString().Trim();
                int makbuzNo;
                int? makbuz = int.TryParse(Convert.ToString(dgRow.Cells[5].Value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out makbuzNo)
                    ? (int?)makbuzNo
                    : null;
                decimal toplam = ParseMoney((dgRow.Cells[1].Value ?? "0").ToString());
                decimal odenen = ParseMoney((dgRow.Cells[2].Value ?? "0").ToString());
                decimal kalan = ParseMoney((dgRow.Cells[3].Value ?? "0").ToString());

                bool tamamenBos = string.IsNullOrWhiteSpace(tarih)
                                  && string.IsNullOrWhiteSpace(notlar)
                                  && toplam == 0m
                                  && odenen == 0m
                                  && kalan == 0m;
                if (tamamenBos)
                    continue;

                rows.Add(new OdemeSatiri
                {
                    TarihText = tarih,
                    MakbuzNo = makbuz,
                    Toplam = toplam,
                    Odenen = odenen,
                    Kalan = kalan,
                    Notlar = notlar
                });
            }
            return rows;
        }

        private class OdemeSatiri
        {
            public string TarihText { get; set; }
            public int? MakbuzNo { get; set; }
            public decimal Toplam { get; set; }
            public decimal Odenen { get; set; }
            public decimal Kalan { get; set; }
            public string Notlar { get; set; }
        }

        private int GetNextMakbuzNo()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return 1;
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(@"SELECT ISNULL(MAX(MAKBUZ_NO),0)+1 FROM dbo.KURSIYER_ODEME_HAREKET", con))
                {
                    con.Open();
                    var o = cmd.ExecuteScalar();
                    return o == null || o == DBNull.Value ? 1 : Convert.ToInt32(o, CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                return 1;
            }
        }

        private DataGridViewRow GetSelectedMakbuzRow()
        {
            if (dgvOdemeler.CurrentRow == null || dgvOdemeler.CurrentRow.IsNewRow)
                return null;
            var row = dgvOdemeler.CurrentRow;
            return ParseMakbuzNo(row.Cells[5].Value) > 0 ? row : null;
        }

        private static int ParseMakbuzNo(object value)
        {
            int no;
            return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out no) ? no : 0;
        }

        private static DateTime ParseDate(object value)
        {
            DateTime dt;
            if (DateTime.TryParseExact(Convert.ToString(value), "dd.MM.yyyy", CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.None, out dt))
                return dt;
            return DateTime.Today;
        }

        private string GetOdemeKursiyerKolonAdi()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return "KURSIYER_ID";

            const string sql = @"
SELECT TOP 1 COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo'
  AND TABLE_NAME='KURSIYER_ODEME_HAREKET'
  AND COLUMN_NAME IN ('KURSIYER_ID', 'ID_KURSIYER')
ORDER BY CASE WHEN COLUMN_NAME='KURSIYER_ID' THEN 0 ELSE 1 END;";
            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    con.Open();
                    var o = cmd.ExecuteScalar();
                    string col = o == null || o == DBNull.Value ? string.Empty : o.ToString();
                    return string.IsNullOrWhiteSpace(col) ? "KURSIYER_ID" : col;
                }
            }
            catch
            {
                return "KURSIYER_ID";
            }
        }

        private static HashSet<string> GetTableColumns(SqlConnection con, string tableName)
        {
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            const string sql = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TABLE;";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@TABLE", tableName);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        string col = Convert.ToString(r["COLUMN_NAME"]);
                        if (!string.IsNullOrWhiteSpace(col))
                            cols.Add(col);
                    }
                }
            }

            return cols;
        }

        private void RecalculateTotalsFromGrid()
        {
            decimal toplamBorc = ParseMoney(txtToplamBorc.Text);
            decimal toplamOdenen = 0m;

            foreach (DataGridViewRow row in dgvOdemeler.Rows)
            {
                if (row == null || row.IsNewRow)
                    continue;
                decimal odenen = ParseMoney(Convert.ToString(row.Cells[2].Value));
                toplamOdenen += odenen;
                decimal kalan = toplamBorc - toplamOdenen;
                if (kalan < 0m)
                    kalan = 0m;
                row.Cells[1].Value = toplamBorc.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
                row.Cells[3].Value = kalan.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
            }

            decimal formKalan = toplamBorc - toplamOdenen;
            if (formKalan < 0m)
                formKalan = 0m;
            txtToplamOdenen.Text = toplamOdenen.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
            txtKalanBorc.Text = formKalan.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
        }

        private static string ToMoneyText(object value)
        {
            if (value == null || value == DBNull.Value)
                return "0,00 TL";
            return Convert.ToDecimal(value).ToString("N2", CultureInfo.GetCultureInfo("tr-TR")) + " TL";
        }

        private static decimal ParseMoney(string text)
        {
            string t = (text ?? "0").Trim();
            if (string.IsNullOrWhiteSpace(t))
                return 0m;
            t = t.Replace("TL", "")
                 .Replace("tl", "")
                 .Replace("Tl", "")
                 .Replace("tL", "")
                 .Replace("₺", "")
                 .Trim();

            // TR para formatini once dogrudan dene: 15.000,00
            if (decimal.TryParse(t, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out decimal d))
                return d;

            // Sadece nokta ile ondalik girilirse (15000.50 gibi) invariant dene.
            if (decimal.TryParse(t, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
                return d;

            // Son fallback: bosluklari/sayisal olmayanlari temizleyip tekrar dene.
            t = new string(t.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray());
            if (decimal.TryParse(t, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out d))
                return d;

            return 0m;
        }

        private decimal TutarGir(string baslik, string mesaj, decimal varsayilan)
        {
            using (var frm = new Form())
            {
                frm.Text = baslik;
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox = false;
                frm.MinimizeBox = false;
                frm.ClientSize = new Size(360, 120);

                var lbl = new Label { Left = 12, Top = 12, Width = 334, Text = mesaj };
                var txt = new TextBox { Left = 12, Top = 36, Width = 334, Text = varsayilan.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")) + " TL" };
                var btnTamam = new Button { Left = 190, Top = 72, Width = 75, Text = "Tamam", DialogResult = DialogResult.OK };
                var btnIptal = new Button { Left = 271, Top = 72, Width = 75, Text = "Iptal", DialogResult = DialogResult.Cancel };

                frm.Controls.Add(lbl);
                frm.Controls.Add(txt);
                frm.Controls.Add(btnTamam);
                frm.Controls.Add(btnIptal);
                frm.AcceptButton = btnTamam;
                frm.CancelButton = btnIptal;

                if (frm.ShowDialog(this) != DialogResult.OK)
                    return -1m;

                return ParseMoney(txt.Text);
            }
        }
    }
}
