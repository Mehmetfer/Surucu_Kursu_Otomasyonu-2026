using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class SmsSablonDuzenleForm : Form
    {
        private sealed class SablonListEntry
        {
            public string DisplayName;
            public string Metin;

            public override string ToString()
            {
                return DisplayName ?? string.Empty;
            }
        }

        private readonly SmsSablonOnizlemeVerisi _onizleme;
        private readonly string _dbConnectionString;
        private bool _sablonYukleniyor;

        private Label _lblYeniSablonAdi;
        private TextBox _txtYeniSablonAdi;
        private Button _btnVeritabaniKaydet;

        public string TemplateText
        {
            get { return txtSablon.Text; }
        }

        public SmsSablonDuzenleForm(string currentTemplate, string initialPreview, SmsSablonOnizlemeVerisi onizleme = null, string dbConnectionString = null)
        {
            _onizleme = onizleme;
            _dbConnectionString = dbConnectionString;
            InitializeComponent();
            txtSablon.Text = currentTemplate ?? string.Empty;
            txtOnizleme.Text = initialPreview ?? string.Empty;

            const int ekstraAltSatir = 52;
            ClientSize = new Size(ClientSize.Width, ClientSize.Height + ekstraAltSatir);
            btnOnizle.Top += ekstraAltSatir;
            btnKaydetDevam.Top += ekstraAltSatir;
            btnIptal.Top += ekstraAltSatir;

            _lblYeniSablonAdi = new Label
            {
                AutoSize = true,
                Text = "Yeni sablon adi:",
                Location = new Point(12, 332)
            };
            _txtYeniSablonAdi = new TextBox
            {
                Location = new Point(118, 329),
                Width = 450
            };
            _btnVeritabaniKaydet = new Button
            {
                Text = "Veritabanina kaydet",
                Location = new Point(575, 327),
                Size = new Size(150, 26),
                UseVisualStyleBackColor = true
            };
            _btnVeritabaniKaydet.Click += BtnVeritabaniKaydet_Click;

            Controls.Add(_lblYeniSablonAdi);
            Controls.Add(_txtYeniSablonAdi);
            Controls.Add(_btnVeritabaniKaydet);
            _lblYeniSablonAdi.BringToFront();
            _txtYeniSablonAdi.BringToFront();
            _btnVeritabaniKaydet.BringToFront();

            if (string.IsNullOrWhiteSpace(_dbConnectionString))
            {
                _txtYeniSablonAdi.Enabled = false;
                _btnVeritabaniKaydet.Enabled = false;
                _lblYeniSablonAdi.ForeColor = SystemColors.GrayText;
            }

            Load += SmsSablonDuzenleForm_Load;
        }

        private void SmsSablonDuzenleForm_Load(object sender, EventArgs e)
        {
            DoldurKayitliSablonlar();
            if (string.IsNullOrWhiteSpace(txtOnizleme.Text))
                txtOnizleme.Text = SablonuUygula(txtSablon.Text);
        }

        private void DoldurKayitliSablonlar()
        {
            cmbKayitliSablonlar.Items.Clear();
            cmbKayitliSablonlar.Items.Add(new SablonListEntry
            {
                DisplayName = "-- Kayitli sablon secin veya asagida duzenleyin --",
                Metin = null
            });

            if (!string.IsNullOrWhiteSpace(_dbConnectionString))
            {
                try
                {
                    const string sql = @"
SELECT SABLON_ADI, SABLON_METNI
FROM dbo.SMSSABLONLARI
WHERE ISNULL(AKTIF,1) = 1
ORDER BY ISNULL(SIRA_NO,0), SABLON_ADI;";

                    using (var cn = new SqlConnection(_dbConnectionString))
                    using (var cmd = new SqlCommand(sql, cn))
                    {
                        cn.Open();
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                string ad = Convert.ToString(r["SABLON_ADI"]) ?? string.Empty;
                                string metin = Convert.ToString(r["SABLON_METNI"]) ?? string.Empty;
                                if (string.IsNullOrWhiteSpace(ad))
                                    continue;
                                cmbKayitliSablonlar.Items.Add(new SablonListEntry
                                {
                                    DisplayName = ad.Trim(),
                                    Metin = metin
                                });
                            }
                        }
                    }
                }
                catch
                {
                    // Tablo yok / baglanti hatasi: sadece bos liste + manuel metin
                }
            }

            _sablonYukleniyor = true;
            string cur = (txtSablon.Text ?? string.Empty).Trim();
            int sec = 0;
            if (!string.IsNullOrEmpty(cur))
            {
                for (int i = 1; i < cmbKayitliSablonlar.Items.Count; i++)
                {
                    var ent = (SablonListEntry)cmbKayitliSablonlar.Items[i];
                    if (ent.Metin != null && string.Equals(ent.Metin.Trim(), cur, StringComparison.Ordinal))
                    {
                        sec = i;
                        break;
                    }
                }
            }
            cmbKayitliSablonlar.SelectedIndex = sec;
            _sablonYukleniyor = false;
        }

        private void cmbKayitliSablonlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_sablonYukleniyor)
                return;
            var ent = cmbKayitliSablonlar.SelectedItem as SablonListEntry;
            if (ent == null || ent.Metin == null)
                return;
            txtSablon.Text = ent.Metin;
            txtOnizleme.Text = SablonuUygula(ent.Metin);
        }

        private void btnOnizle_Click(object sender, EventArgs e)
        {
            txtOnizleme.Text = SablonuUygula(txtSablon.Text);
        }

        private string SablonuUygula(string sablon)
        {
            if (string.IsNullOrEmpty(sablon))
                return string.Empty;

            if (_onizleme != null)
            {
                return sablon
                    .Replace("[AD SOYAD]", _onizleme.AdSoyad ?? string.Empty)
                    .Replace("[TARIH]", _onizleme.Tarih.ToString("dd.MM.yyyy"))
                    .Replace("[SAAT]", _onizleme.Saat ?? string.Empty)
                    .Replace("[KURS ADI]", _onizleme.KursAdi ?? string.Empty)
                    .Replace("[TELEFON]", _onizleme.Telefon ?? string.Empty);
            }

            return sablon
                .Replace("[AD SOYAD]", "BEKIR KARABEKIROGLU")
                .Replace("[TARIH]", DateTime.Today.ToString("dd.MM.yyyy"))
                .Replace("[SAAT]", "08:30")
                .Replace("[KURS ADI]", "ILKMETRO")
                .Replace("[TELEFON]", "3223636567");
        }

        private void btnKaydetDevam_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSablon.Text))
            {
                MessageBox.Show("Sablon metni bos olamaz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnIptal_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnVeritabaniKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_dbConnectionString))
            {
                MessageBox.Show("Veritabani baglantisi yok; sablon kaydedilemez.", "Uyari",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSablon.Text))
            {
                MessageBox.Show("Sablon metni bos olamaz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ad = (_txtYeniSablonAdi.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(ad))
            {
                MessageBox.Show("Yeni sablon adini girin.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ad.Length > 200)
            {
                MessageBox.Show("Sablon adi en fazla 200 karakter olabilir.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string metin = txtSablon.Text.Trim();

            bool mevcut = false;
            try
            {
                using (var cn = new SqlConnection(_dbConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT 1 FROM dbo.SMSSABLONLARI WHERE SABLON_ADI = @ad", cn))
                {
                    cmd.Parameters.AddWithValue("@ad", ad);
                    cn.Open();
                    mevcut = cmd.ExecuteScalar() != null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sablon kontrolu basarisiz: " + ex.Message, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (mevcut)
            {
                if (MessageBox.Show(
                        "'" + ad + "' adli sablon zaten var. Uzerine yazilsin mi?",
                        "Onay",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
            }

            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.SMSSABLONLARI WHERE SABLON_ADI = @ad)
BEGIN
  UPDATE dbo.SMSSABLONLARI
  SET SABLON_METNI = @metin, GUNCELLEME_TARIHI = SYSUTCDATETIME()
  WHERE SABLON_ADI = @ad;
END
ELSE
BEGIN
  DECLARE @sira INT;
  SELECT @sira = ISNULL(MAX(SIRA_NO), 0) + 1 FROM dbo.SMSSABLONLARI;
  INSERT INTO dbo.SMSSABLONLARI (SABLON_ADI, SABLON_METNI, SIRA_NO, AKTIF, OLUSTURMA_TARIHI, GUNCELLEME_TARIHI)
  VALUES (@ad, @metin, @sira, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END";

            try
            {
                using (var cn = new SqlConnection(_dbConnectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@ad", ad);
                    cmd.Parameters.AddWithValue("@metin", metin);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Sablon veritabanina kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DoldurKayitliSablonlar();
                SecComboOgeAdinaGore(ad);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayit hatasi: " + ex.Message + "\r\n\r\nSMSSABLONLARI tablosu yoksa veritabani guncellemesini calistirin.",
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SecComboOgeAdinaGore(string sablonAdi)
        {
            if (string.IsNullOrEmpty(sablonAdi))
                return;
            _sablonYukleniyor = true;
            try
            {
                for (int i = 1; i < cmbKayitliSablonlar.Items.Count; i++)
                {
                    var ent = cmbKayitliSablonlar.Items[i] as SablonListEntry;
                    if (ent != null && string.Equals(ent.DisplayName, sablonAdi.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        cmbKayitliSablonlar.SelectedIndex = i;
                        return;
                    }
                }
            }
            finally
            {
                _sablonYukleniyor = false;
            }
        }
    }
}
