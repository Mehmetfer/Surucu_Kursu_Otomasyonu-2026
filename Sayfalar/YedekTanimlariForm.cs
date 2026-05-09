using System;
using System.Diagnostics;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class YedekTanimlariForm : Form
    {
        private readonly string _connectionString;
        private readonly string _ayarDosyaYolu;

        public YedekTanimlariForm() : this(string.Empty)
        {
        }

        public YedekTanimlariForm(string connectionString)
        {
            _connectionString = connectionString ?? string.Empty;
            _ayarDosyaYolu = Path.Combine(Application.StartupPath, "YedekAyarlari.xml");
            InitializeComponent();
            LoadAyarlar();
        }

        private void SaveAyarlar()
        {
            try
            {
                var x = new XElement("YedekAyarlari",
                    new XElement("YedekKonumu", txtYedekKonumu.Text.Trim()),
                    new XElement("SonYedekSayisi", (int)numSonYedekSayisi.Value)
                );
                x.Save(_ayarDosyaYolu);
                MessageBox.Show("Yedek ayarlari kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ayarlar kaydedilemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAyarlar()
        {
            txtYedekKonumu.Text = Path.Combine(Application.StartupPath, "yedek");
            if (!File.Exists(_ayarDosyaYolu))
                return;

            try
            {
                var x = XElement.Load(_ayarDosyaYolu);
                txtYedekKonumu.Text = (string)x.Element("YedekKonumu") ?? txtYedekKonumu.Text;
                int son;
                if (int.TryParse((string)x.Element("SonYedekSayisi"), out son) && son >= 1 && son <= 30)
                    numSonYedekSayisi.Value = son;
            }
            catch
            {
            }
        }

        private void HemenYedekAl()
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            var klasor = Path.Combine(Application.StartupPath, "Kolera");
            txtYedekKonumu.Text = klasor;

            try
            {
                Directory.CreateDirectory(klasor);
                var dbName = GetDatabaseNameFromConnection(_connectionString);
                if (string.IsNullOrWhiteSpace(dbName))
                {
                    MessageBox.Show("Veritabani adi bulunamadi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var fileName = dbName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bak";
                var fullPath = Path.Combine(klasor, fileName);

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("BACKUP DATABASE [" + dbName + "] TO DISK = @path WITH INIT, COPY_ONLY", conn))
                {
                    cmd.Parameters.AddWithValue("@path", fullPath);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                TemizleEskiYedekler(klasor, dbName, (int)numSonYedekSayisi.Value);
                LisansPolitikasi.RegisterSuccessfulWrite();
                MessageBox.Show("Yedek alindi:\n" + fullPath, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start("explorer.exe", klasor);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yedek alma hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TemizleEskiYedekler(string klasor, string dbName, int tutulacakSayi)
        {
            try
            {
                var files = new DirectoryInfo(klasor)
                    .GetFiles(dbName + "_*.bak")
                    .OrderByDescending(f => f.CreationTimeUtc)
                    .ToList();

                for (int i = tutulacakSayi; i < files.Count; i++)
                    files[i].Delete();
            }
            catch
            {
            }
        }

        private string GetDatabaseNameFromConnection(string connectionString)
        {
            try
            {
                var b = new SqlConnectionStringBuilder(connectionString);
                return b.InitialCatalog;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            SaveAyarlar();
        }

        private void btnHemenYedekAl_Click(object sender, EventArgs e)
        {
            HemenYedekAl();
        }

        private void btnKapat_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnGozat_Click(object sender, EventArgs e)
        {
            GozatKlasor();
        }

        private void GozatKlasor()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Yedek klasorunu seciniz";
                if (Directory.Exists(txtYedekKonumu.Text))
                    fbd.SelectedPath = txtYedekKonumu.Text;
                if (fbd.ShowDialog(this) == DialogResult.OK)
                    txtYedekKonumu.Text = fbd.SelectedPath;
            }
        }

    }
}
