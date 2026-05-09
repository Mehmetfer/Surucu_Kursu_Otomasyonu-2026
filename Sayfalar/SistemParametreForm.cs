using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class SistemParametreForm : Form
    {
        private readonly string _connectionString;

        public SistemParametreForm(string connectionString)
        {
            _connectionString = connectionString ?? string.Empty;
            InitializeComponent();

            Load += SistemParametreForm_Load;
            btnKaydet.Click += BtnKaydet_Click;
            btnYeni.Click += BtnYeni_Click;
            btnSil.Click += BtnSil_Click;
        }

        private void SistemParametreForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabani baglantisi bulunamadi.");
                Close();
                return;
            }

            EnsureParamTable();
            EnsureDefaultRows();
            Yukle();
        }

        private void BtnYeni_Click(object sender, EventArgs e)
        {
            var dt = dgvParametreler.DataSource as DataTable;
            if (dt == null) return;
            dt.Rows.Add(0, string.Empty, string.Empty, string.Empty);
        }

        private void BtnSil_Click(object sender, EventArgs e)
        {
            if (dgvParametreler.CurrentRow == null) return;
            dgvParametreler.Rows.RemoveAt(dgvParametreler.CurrentRow.Index);
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            dgvParametreler.EndEdit();
            var dt = dgvParametreler.DataSource as DataTable;
            if (dt == null) return;

            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                foreach (DataRow row in dt.Rows)
                {
                    string key = Convert.ToString(row["ParamKey"]) ?? string.Empty;
                    string value = Convert.ToString(row["ParamValue"]) ?? string.Empty;
                    string aciklama = Convert.ToString(row["Aciklama"]) ?? string.Empty;
                    int id = 0;
                    int.TryParse(Convert.ToString(row["ID"]), out id);

                    key = key.Trim();
                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    if (id > 0)
                    {
                        using (var cmd = new SqlCommand("UPDATE APP_PARAMETRELER SET ParamKey=@k, ParamValue=@v, Aciklama=@a WHERE ID=@id", con))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@k", key);
                            cmd.Parameters.AddWithValue("@v", value.Trim());
                            cmd.Parameters.AddWithValue("@a", aciklama.Trim());
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new SqlCommand("INSERT INTO APP_PARAMETRELER (ParamKey, ParamValue, Aciklama) VALUES (@k, @v, @a)", con))
                        {
                            cmd.Parameters.AddWithValue("@k", key);
                            cmd.Parameters.AddWithValue("@v", value.Trim());
                            cmd.Parameters.AddWithValue("@a", aciklama.Trim());
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            Yukle();
            MessageBox.Show("Parametreler kaydedildi.");
        }

        private void Yukle()
        {
            var dt = new DataTable();
            using (var con = new SqlConnection(_connectionString))
            using (var da = new SqlDataAdapter("SELECT ID, ParamKey, ParamValue, Aciklama FROM APP_PARAMETRELER ORDER BY ParamKey", con))
            {
                da.Fill(dt);
            }
            dgvParametreler.DataSource = dt;
            if (dgvParametreler.Columns.Contains("ID"))
                dgvParametreler.Columns["ID"].Width = 60;
        }

        private void EnsureParamTable()
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
    CREATE UNIQUE INDEX UX_APP_PARAMETRELER_KEY ON dbo.APP_PARAMETRELER(ParamKey);
END";
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureDefaultRows()
        {
            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM APP_PARAMETRELER WHERE ParamKey='DONEM_YILI_MIN_OFFSET')
    INSERT INTO APP_PARAMETRELER(ParamKey, ParamValue, Aciklama) VALUES('DONEM_YILI_MIN_OFFSET','5','Geriye kac yil gosterilsin');
IF NOT EXISTS (SELECT 1 FROM APP_PARAMETRELER WHERE ParamKey='DONEM_YILI_MAX_OFFSET')
    INSERT INTO APP_PARAMETRELER(ParamKey, ParamValue, Aciklama) VALUES('DONEM_YILI_MAX_OFFSET','1','Ileriye kac yil gosterilsin');
IF NOT EXISTS (SELECT 1 FROM APP_PARAMETRELER WHERE ParamKey='DONEM_YILI_OZEL_LISTE')
    INSERT INTO APP_PARAMETRELER(ParamKey, ParamValue, Aciklama) VALUES('DONEM_YILI_OZEL_LISTE','','Virgullu ekstra yil listesi. Ornek: 2027,2028,2030');";
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
