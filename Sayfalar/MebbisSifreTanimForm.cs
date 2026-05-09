using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class MebbisSifreTanimForm : Form
    {
        private readonly string _connectionString;

        public MebbisSifreTanimForm(string connectionString)
        {
            _connectionString = connectionString ?? string.Empty;
            InitializeComponent();
            Shown += MebbisSifreTanimForm_Shown;
        }

        private void MebbisSifreTanimForm_Shown(object sender, EventArgs e)
        {
            YukleYetkiKullanicilari();
            YukleMebbisBilgileri();
        }

        private void YukleYetkiKullanicilari()
        {
            cmbMebbisYetki1.Items.Clear();
            cmbMebbisYetki2.Items.Clear();
            cmbMebbisYetki3.Items.Clear();

            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = "SELECT TOP (1000) KULLANICI_ADI FROM KULLANICI";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string kullaniciAdi = ReaderKolonDegeri(reader, "KULLANICI_ADI");
                            if (string.IsNullOrWhiteSpace(kullaniciAdi))
                                continue;

                            cmbMebbisYetki1.Items.Add(kullaniciAdi);
                            cmbMebbisYetki2.Items.Add(kullaniciAdi);
                            cmbMebbisYetki3.Items.Add(kullaniciAdi);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void YukleMebbisBilgileri()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            string genelTable = ResolveGenelParamTableName();
            if (string.IsNullOrWhiteSpace(genelTable))
                return;

            string sql = @"
SELECT TOP (1)
    ISNULL(MEBBIS_KUL_ADI_1,'') AS MEBBIS_KUL_ADI_1,
    ISNULL(MEBBIS_KUL_SIF_1,'') AS MEBBIS_KUL_SIF_1,
    ISNULL(MEBBIS_KUL_YET_1,'') AS MEBBIS_KUL_YET_1,
    ISNULL(MEBBIS_KUL_ADI_2,'') AS MEBBIS_KUL_ADI_2,
    ISNULL(MEBBIS_KUL_SIF_2,'') AS MEBBIS_KUL_SIF_2,
    ISNULL(MEBBIS_KUL_YET_2,'') AS MEBBIS_KUL_YET_2,
    ISNULL(MEBBIS_KUL_ADI_3,'') AS MEBBIS_KUL_ADI_3,
    ISNULL(MEBBIS_KUL_SIF_3,'') AS MEBBIS_KUL_SIF_3,
    ISNULL(MEBBIS_KUL_YET_3,'') AS MEBBIS_KUL_YET_3
FROM [" + genelTable + @"]
ORDER BY ID DESC";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return;

                        txtMebbisKulAdi1.Text = reader["MEBBIS_KUL_ADI_1"].ToString();
                        txtMebbisSifre1.Text = reader["MEBBIS_KUL_SIF_1"].ToString();
                        SetComboTextSafe(cmbMebbisYetki1, reader["MEBBIS_KUL_YET_1"].ToString());

                        txtMebbisKulAdi2.Text = reader["MEBBIS_KUL_ADI_2"].ToString();
                        txtMebbisSifre2.Text = reader["MEBBIS_KUL_SIF_2"].ToString();
                        SetComboTextSafe(cmbMebbisYetki2, reader["MEBBIS_KUL_YET_2"].ToString());

                        txtMebbisKulAdi3.Text = reader["MEBBIS_KUL_ADI_3"].ToString();
                        txtMebbisSifre3.Text = reader["MEBBIS_KUL_SIF_3"].ToString();
                        SetComboTextSafe(cmbMebbisYetki3, reader["MEBBIS_KUL_YET_3"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mebbis bilgileri yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabani baglantisi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string genelTable = ResolveGenelParamTableName();
            if (string.IsNullOrWhiteSpace(genelTable))
            {
                MessageBox.Show("GenelParam tablosu bulunamadi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string sql = @"
IF EXISTS (SELECT 1 FROM [" + genelTable + @"])
BEGIN
    UPDATE [" + genelTable + @"]
    SET
        MEBBIS_KUL_ADI_1 = @ADI1,
        MEBBIS_KUL_SIF_1 = @SIF1,
        MEBBIS_KUL_YET_1 = @YET1,
        MEBBIS_KUL_ADI_2 = @ADI2,
        MEBBIS_KUL_SIF_2 = @SIF2,
        MEBBIS_KUL_YET_2 = @YET2,
        MEBBIS_KUL_ADI_3 = @ADI3,
        MEBBIS_KUL_SIF_3 = @SIF3,
        MEBBIS_KUL_YET_3 = @YET3
    WHERE ID = (SELECT TOP (1) ID FROM [" + genelTable + @"] ORDER BY ID DESC);
END
ELSE
BEGIN
    INSERT INTO [" + genelTable + @"]
    (
        MEBBIS_KUL_ADI_1, MEBBIS_KUL_SIF_1, MEBBIS_KUL_YET_1,
        MEBBIS_KUL_ADI_2, MEBBIS_KUL_SIF_2, MEBBIS_KUL_YET_2,
        MEBBIS_KUL_ADI_3, MEBBIS_KUL_SIF_3, MEBBIS_KUL_YET_3,
        MEBBIS_KUL_ADI_4, MEBBIS_KUL_SIF_4, MEBBIS_KUL_YET_4,
        MEBBIS_KUL_ADI_5, MEBBIS_KUL_SIF_5, MEBBIS_KUL_YET_5
    )
    VALUES
    (
        @ADI1, @SIF1, @YET1,
        @ADI2, @SIF2, @YET2,
        @ADI3, @SIF3, @YET3,
        '', '', '',
        '', '', ''
    );
END";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ADI1", txtMebbisKulAdi1.Text.Trim());
                    cmd.Parameters.AddWithValue("@SIF1", txtMebbisSifre1.Text.Trim());
                    cmd.Parameters.AddWithValue("@YET1", cmbMebbisYetki1.Text.Trim());
                    cmd.Parameters.AddWithValue("@ADI2", txtMebbisKulAdi2.Text.Trim());
                    cmd.Parameters.AddWithValue("@SIF2", txtMebbisSifre2.Text.Trim());
                    cmd.Parameters.AddWithValue("@YET2", cmbMebbisYetki2.Text.Trim());
                    cmd.Parameters.AddWithValue("@ADI3", txtMebbisKulAdi3.Text.Trim());
                    cmd.Parameters.AddWithValue("@SIF3", txtMebbisSifre3.Text.Trim());
                    cmd.Parameters.AddWithValue("@YET3", cmbMebbisYetki3.Text.Trim());

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                LisansPolitikasi.RegisterSuccessfulWrite();
                MessageBox.Show("Mebbis sifre bilgileri kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mebbis kaydetme hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string ReaderKolonDegeri(SqlDataReader reader, string kolonAdi)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (!string.Equals(reader.GetName(i), kolonAdi, StringComparison.OrdinalIgnoreCase))
                    continue;
                var val = reader.GetValue(i);
                return val == null ? string.Empty : val.ToString().Trim();
            }
            return string.Empty;
        }

        private string ResolveGenelParamTableName()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return null;

            const string sql = @"
SELECT TOP 1 TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND UPPER(TABLE_NAME) IN ('GENELPARAM')
ORDER BY TABLE_NAME;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var o = cmd.ExecuteScalar();
                return o == null || o == DBNull.Value ? null : Convert.ToString(o);
            }
        }

        private static void SetComboTextSafe(ComboBox combo, string text)
        {
            var value = (text ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                combo.SelectedIndex = -1;
                combo.Text = string.Empty;
                return;
            }
            if (combo.Items.IndexOf(value) < 0)
                combo.Items.Add(value);
            combo.SelectedItem = value;
        }

        private void btnSifreGoster1_Click(object sender, EventArgs e)
        {
            txtMebbisSifre1.UseSystemPasswordChar = !txtMebbisSifre1.UseSystemPasswordChar;
            btnSifreGoster1.Text = txtMebbisSifre1.UseSystemPasswordChar ? "Goster" : "Gizle";
        }

        private void btnSifreGoster2_Click(object sender, EventArgs e)
        {
            txtMebbisSifre2.UseSystemPasswordChar = !txtMebbisSifre2.UseSystemPasswordChar;
            btnSifreGoster2.Text = txtMebbisSifre2.UseSystemPasswordChar ? "Goster" : "Gizle";
        }

        private void btnSifreGoster3_Click(object sender, EventArgs e)
        {
            txtMebbisSifre3.UseSystemPasswordChar = !txtMebbisSifre3.UseSystemPasswordChar;
            btnSifreGoster3.Text = txtMebbisSifre3.UseSystemPasswordChar ? "Goster" : "Gizle";
        }
    }
}
