using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class UstYazilarForm : Form
    {
        private const string VarsayilanOrnekDocPath = @"C:\Users\PC\Downloads\ATAMA ÜST YAZI (1).doc";
        private readonly string _sablonKlasoru;
        private string _aktifDosyaYolu;
        private int _printCharIndex;
        private bool _toolbarUpdating;
        private string[] _tumSablonlar = new string[0];

        public UstYazilarForm()
        {
            InitializeComponent();
            _sablonKlasoru = Path.Combine(Application.StartupPath, "UstYazilar");
            ConfigurePageLayout();
            EnsureSablonKlasoru();
            TryImportDefaultSample();
            YukleSablonListesi();
            InitFormatToolbar();
            KurumsalTasarimiUygula();
            ButonDurumlariniGuncelle();
        }

        private void ConfigurePageLayout()
        {
            // PrintDocument birimleri 1/100 inch oldugu icin A4 olculeri bu sekildedir.
            docYazdir.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
            docYazdir.DefaultPageSettings.Margins = new Margins(79, 79, 79, 79); // Yaklasik 20mm
            docYazdir.DefaultPageSettings.Landscape = false;
            docYazdir.OriginAtMargins = true;
        }

        private void EnsureSablonKlasoru()
        {
            if (!Directory.Exists(_sablonKlasoru))
                Directory.CreateDirectory(_sablonKlasoru);

            string ornek = Path.Combine(_sablonKlasoru, "Ornek_UstYazi.txt");
            if (!File.Exists(ornek))
            {
                File.WriteAllText(ornek,
@"SAYI:
KONU:
TARIH:

ILGILI MAKAMA,

Bu bir ornek ust yazi sablonudur. Gerekli alanlari duzenleyip kullanabilirsiniz.

Saygilarimizla.
");
            }
        }

        private void TryImportDefaultSample()
        {
            try
            {
                if (!File.Exists(VarsayilanOrnekDocPath))
                    return;

                string hedef = Path.Combine(_sablonKlasoru, Path.GetFileName(VarsayilanOrnekDocPath));
                if (!File.Exists(hedef))
                    File.Copy(VarsayilanOrnekDocPath, hedef, false);
            }
            catch
            {
                // Ornek dosya aktarimi basarisiz olsa bile ekran acilmaya devam etsin.
            }
        }

        private void YukleSablonListesi()
        {
            lstSablonlar.Items.Clear();
            var dosyalar = Directory.GetFiles(_sablonKlasoru, "*.*", SearchOption.TopDirectoryOnly);
            var sablonlar = new System.Collections.Generic.List<string>();
            foreach (var dosya in dosyalar)
            {
                string ext = Path.GetExtension(dosya);
                if (string.Equals(ext, ".txt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ext, ".rtf", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ext, ".doc", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(ext, ".docx", StringComparison.OrdinalIgnoreCase))
                {
                    sablonlar.Add(Path.GetFileName(dosya));
                }
            }
            _tumSablonlar = sablonlar.OrderBy(x => x).ToArray();
            var arama = (txtSablonAra == null || txtSablonAra.ForeColor == Color.Gray) ? string.Empty : txtSablonAra.Text;
            SablonAra(arama);
            DurumYaz("Şablon listesi yenilendi.");
        }

        private void lstSablonlar_DoubleClick(object sender, EventArgs e)
        {
            SablonAc();
        }

        private void btnAc_Click(object sender, EventArgs e)
        {
            SablonAc();
        }

        private void SablonAc()
        {
            if (lstSablonlar.SelectedItem == null)
                return;

            string ad = lstSablonlar.SelectedItem.ToString();
            string yol = Path.Combine(_sablonKlasoru, ad);
            _aktifDosyaYolu = yol;
            lblAktifDosya.Text = "Aktif: " + ad;
            lblDurumDosya.Text = "Dosya: " + ad;

            string ext = Path.GetExtension(yol);
            if (string.Equals(ext, ".txt", StringComparison.OrdinalIgnoreCase))
            {
                rtbEditor.LoadFile(yol, RichTextBoxStreamType.PlainText);
                RtbEditor_SelectionChanged(null, EventArgs.Empty);
                DurumYaz("TXT şablonu açıldı.");
                ButonDurumlariniGuncelle();
                return;
            }

            if (string.Equals(ext, ".rtf", StringComparison.OrdinalIgnoreCase))
            {
                rtbEditor.LoadFile(yol, RichTextBoxStreamType.RichText);
                RtbEditor_SelectionChanged(null, EventArgs.Empty);
                DurumYaz("RTF şablonu açıldı.");
                ButonDurumlariniGuncelle();
                return;
            }

            rtbEditor.Clear();
            MessageBox.Show(this,
                "Bu dosya Word formatında. Editöre yüklenmez, sağ panelden 'Word ile Aç' kullanabilirsiniz.",
                "Bilgi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            DurumYaz("Word şablonu seçildi.");
            ButonDurumlariniGuncelle();
        }

        private void btnYeni_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.InitialDirectory = _sablonKlasoru;
                sfd.Filter = "Metin Dosyasi (*.txt)|*.txt|Rich Text (*.rtf)|*.rtf";
                sfd.FileName = "Yeni_UstYazi.txt";
                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                _aktifDosyaYolu = sfd.FileName;
                rtbEditor.Text =
@"SAYI:
KONU:
TARIH:

ILGILI MAKAMA,

Kurumsal yazışma metninizi bu alandan düzenleyiniz.

Saygılarımızla.";
                lblAktifDosya.Text = "Aktif: " + Path.GetFileName(_aktifDosyaYolu);
                KaydetAktifDosya();
                YukleSablonListesi();
                DurumYaz("Yeni şablon oluşturuldu.");
                ButonDurumlariniGuncelle();
            }
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            KaydetAktifDosya();
        }

        private void KaydetAktifDosya()
        {
            if (string.IsNullOrWhiteSpace(_aktifDosyaYolu))
            {
                MessageBox.Show(this, "Kaydetmek icin once bir sablon seciniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DurumYaz("Kaydetme için aktif şablon yok.");
                return;
            }

            string ext = Path.GetExtension(_aktifDosyaYolu);
            if (string.Equals(ext, ".txt", StringComparison.OrdinalIgnoreCase))
                rtbEditor.SaveFile(_aktifDosyaYolu, RichTextBoxStreamType.PlainText);
            else if (string.Equals(ext, ".rtf", StringComparison.OrdinalIgnoreCase))
                rtbEditor.SaveFile(_aktifDosyaYolu, RichTextBoxStreamType.RichText);
            else
            {
                MessageBox.Show(this, "Word dosyaları uygulama içinde kaydedilemez. Lütfen Word ile kaydediniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DurumYaz("Word dosyası için kaydetme engellendi.");
                return;
            }

            MessageBox.Show(this, "Dosya kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DurumYaz("Şablon kaydedildi.");
            YukleSablonListesi();
            ButonDurumlariniGuncelle();
        }

        private void btnOnizleme_Click(object sender, EventArgs e)
        {
            if (!EditorYazdirilabilirMi())
                return;

            _printCharIndex = 0;
            using (var dlg = new PrintPreviewDialog())
            {
                dlg.Document = docYazdir;
                dlg.Width = 1000;
                dlg.Height = 700;
                dlg.ShowDialog(this);
            }
        }

        private void btnYazdir_Click(object sender, EventArgs e)
        {
            if (!EditorYazdirilabilirMi())
                return;

            _printCharIndex = 0;
            using (var dlg = new PrintDialog())
            {
                dlg.Document = docYazdir;
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;
            }
            docYazdir.Print();
        }

        private bool EditorYazdirilabilirMi()
        {
            if (string.IsNullOrWhiteSpace(_aktifDosyaYolu))
            {
                MessageBox.Show(this, "Once bir sablon seciniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            string ext = Path.GetExtension(_aktifDosyaYolu);
            if (!string.Equals(ext, ".txt", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(ext, ".rtf", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this,
                    "Word dosyalari icin yazdirma/preview Word uzerinden yapilir. Dosyayi Word ile acip yazdirabilirsiniz.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            return true;
        }

        private void docYazdir_PrintPage(object sender, PrintPageEventArgs e)
        {
            string text = rtbEditor.Text ?? string.Empty;
            if (_printCharIndex >= text.Length)
            {
                e.HasMorePages = false;
                return;
            }

            var bounds = e.MarginBounds;
            string kalan = text.Substring(_printCharIndex);
            int charsFitted;
            int linesFilled;
            e.Graphics.MeasureString(kalan, rtbEditor.Font, bounds.Size, StringFormat.GenericTypographic, out charsFitted, out linesFilled);
            string sayfaMetni = kalan.Substring(0, Math.Max(charsFitted, 0));
            e.Graphics.DrawString(sayfaMetni, rtbEditor.Font, Brushes.Black, bounds, StringFormat.GenericTypographic);

            _printCharIndex += sayfaMetni.Length;
            e.HasMorePages = _printCharIndex < text.Length;
        }

        private void btnKlasorAc_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = _sablonKlasoru,
                UseShellExecute = true
            });
            DurumYaz("Şablon klasörü açıldı.");
        }

        private void btnYenile_Click(object sender, EventArgs e)
        {
            YukleSablonListesi();
            ButonDurumlariniGuncelle();
        }

        private void btnDosyaEkle_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Desteklenen Dosyalar|*.txt;*.rtf;*.doc;*.docx";
                ofd.Multiselect = true;
                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                foreach (var kaynak in ofd.FileNames)
                {
                    string hedef = Path.Combine(_sablonKlasoru, Path.GetFileName(kaynak));
                    File.Copy(kaynak, hedef, true);
                }

                YukleSablonListesi();
                MessageBox.Show(this, "Dosyalar sablon klasorune eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DurumYaz("Dosyalar şablon klasörüne eklendi.");
            }
        }

        private void InitFormatToolbar()
        {
            _toolbarUpdating = true;
            cmbFont.Items.Clear();
            foreach (var f in FontFamily.Families.OrderBy(x => x.Name))
                cmbFont.Items.Add(f.Name);

            cmbFontSize.Items.Clear();
            cmbFontSize.Items.AddRange(new object[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "24", "28", "32" });
            cmbFont.Text = rtbEditor.Font.FontFamily.Name;
            cmbFontSize.Text = ((int)rtbEditor.Font.Size).ToString();
            _toolbarUpdating = false;
        }

        private void RtbEditor_SelectionChanged(object sender, EventArgs e)
        {
            _toolbarUpdating = true;
            var selectedFont = rtbEditor.SelectionFont ?? rtbEditor.Font;
            cmbFont.Text = selectedFont.FontFamily.Name;
            cmbFontSize.Text = ((int)selectedFont.Size).ToString();
            btnBold.Checked = selectedFont.Bold;
            btnItalic.Checked = selectedFont.Italic;
            btnUnderline.Checked = selectedFont.Underline;
            btnBullet.Checked = rtbEditor.SelectionBullet;
            _toolbarUpdating = false;
        }

        private void cmbFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_toolbarUpdating) return;
            ApplySelectedFont();
        }

        private void cmbFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_toolbarUpdating) return;
            ApplySelectedFont();
        }

        private void ApplySelectedFont()
        {
            float size;
            if (!float.TryParse(cmbFontSize.Text, out size))
                size = rtbEditor.SelectionFont?.Size ?? rtbEditor.Font.Size;

            string family = string.IsNullOrWhiteSpace(cmbFont.Text)
                ? (rtbEditor.SelectionFont?.FontFamily.Name ?? rtbEditor.Font.FontFamily.Name)
                : cmbFont.Text;

            var style = rtbEditor.SelectionFont?.Style ?? rtbEditor.Font.Style;
            try
            {
                rtbEditor.SelectionFont = new Font(family, size, style);
            }
            catch
            {
                // Gecersiz font seciminde editor bozulmasin.
            }
        }

        private void btnBold_Click(object sender, EventArgs e)
        {
            ToggleStyle(FontStyle.Bold);
        }

        private void btnItalic_Click(object sender, EventArgs e)
        {
            ToggleStyle(FontStyle.Italic);
        }

        private void btnUnderline_Click(object sender, EventArgs e)
        {
            ToggleStyle(FontStyle.Underline);
        }

        private void ToggleStyle(FontStyle styleFlag)
        {
            var current = rtbEditor.SelectionFont ?? rtbEditor.Font;
            var newStyle = current.Style ^ styleFlag;
            try
            {
                rtbEditor.SelectionFont = new Font(current.FontFamily, current.Size, newStyle);
            }
            catch
            {
                // Font bu stili desteklemiyorsa sessiz gec.
            }
            RtbEditor_SelectionChanged(null, EventArgs.Empty);
        }

        private void btnAlignLeft_Click(object sender, EventArgs e)
        {
            rtbEditor.SelectionAlignment = HorizontalAlignment.Left;
        }

        private void btnAlignCenter_Click(object sender, EventArgs e)
        {
            rtbEditor.SelectionAlignment = HorizontalAlignment.Center;
        }

        private void btnAlignRight_Click(object sender, EventArgs e)
        {
            rtbEditor.SelectionAlignment = HorizontalAlignment.Right;
        }

        private void btnBullet_Click(object sender, EventArgs e)
        {
            rtbEditor.SelectionBullet = !rtbEditor.SelectionBullet;
            RtbEditor_SelectionChanged(null, EventArgs.Empty);
        }

        private void KurumsalTasarimiUygula()
        {
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);
            CreateHeaderPanel();
            CreateLeftTemplatePanel();
            CreateEditorPanel();
            CreateRightActionPanel();
            CreateStatusBar();

            splitMain.BackColor = BackColor;
            splitEditorRight.BackColor = BackColor;
            pnlLeftCard.BackColor = Color.White;
            pnlEditorCard.BackColor = Color.White;
            pnlRightCard.BackColor = Color.White;

            lblHeaderTitle.ForeColor = Color.White;
            lblHeaderTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            lblHeaderSub.ForeColor = Color.FromArgb(255, 230, 230);
            lblHeaderSub.Font = new Font("Segoe UI", 9.5F);

            lblAktifDosya.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblAktifDosya.ForeColor = Color.FromArgb(52, 73, 94);
            tsFormat.GripStyle = ToolStripGripStyle.Hidden;
            tsFormat.BackColor = Color.FromArgb(250, 250, 250);
            rtbEditor.BorderStyle = BorderStyle.FixedSingle;
            rtbEditor.BackColor = Color.White;
            rtbEditor.Font = new Font("Segoe UI", 10F);
        }

        private Panel CreateHeaderPanel()
        {
            pnlHeader.BackColor = Color.FromArgb(192, 0, 0);
            lblHeaderTitle.Text = "ÜST YAZI YÖNETİM MERKEZİ";
            lblHeaderSub.Text = "Kurumsal yazışma şablonlarını oluşturun, düzenleyin ve yazdırın.";
            return pnlHeader;
        }

        private Panel CreateLeftTemplatePanel()
        {
            txtSablonAra.Text = "Şablon ara...";
            txtSablonAra.ForeColor = Color.Gray;
            txtSablonAra.Enter += (s, e) =>
            {
                if (txtSablonAra.ForeColor == Color.Gray)
                {
                    txtSablonAra.Text = string.Empty;
                    txtSablonAra.ForeColor = Color.Black;
                }
            };
            txtSablonAra.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSablonAra.Text))
                {
                    txtSablonAra.Text = "Şablon ara...";
                    txtSablonAra.ForeColor = Color.Gray;
                }
            };
            txtSablonAra.TextChanged += (s, e) =>
            {
                var arama = txtSablonAra.ForeColor == Color.Gray ? string.Empty : txtSablonAra.Text;
                SablonAra(arama);
            };

            lstSablonlar.SelectedIndexChanged += (s, e) => ButonDurumlariniGuncelle();

            StilButon(btnYeni, Color.FromArgb(25, 118, 210));
            StilButon(btnAc, Color.FromArgb(52, 73, 94));
            StilButon(btnDosyaEkle, Color.FromArgb(52, 73, 94));
            StilButon(btnYenile, Color.FromArgb(52, 73, 94));
            StilButon(btnKlasorAc, Color.FromArgb(52, 73, 94));
            StilButon(btnSil, Color.FromArgb(192, 0, 0));
            StilButon(btnKopyaOlustur, Color.FromArgb(52, 73, 94));
            btnSil.Click += (s, e) => SablonSil();
            btnKopyaOlustur.Click += (s, e) => SablonKopyala();

            return pnlLeftCard;
        }

        private Panel CreateEditorPanel()
        {
            return pnlEditorCard;
        }

        private Panel CreateRightActionPanel()
        {
            StilButon(btnKaydet, Color.FromArgb(46, 125, 50));
            StilButon(btnOnizleme, Color.FromArgb(52, 73, 94));
            StilButon(btnYazdir, Color.FromArgb(52, 73, 94));
            StilButon(btnWordIleAc, Color.FromArgb(25, 118, 210));
            StilButon(btnSablonBilgisi, Color.FromArgb(52, 73, 94));

            btnWordIleAc.Click += (s, e) => WordIleAc();
            btnSablonBilgisi.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_aktifDosyaYolu))
                {
                    MessageBox.Show(this, "Önce bir şablon seçiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var fi = new FileInfo(_aktifDosyaYolu);
                MessageBox.Show(this, "Dosya: " + fi.Name + "\nBoyut: " + fi.Length + " byte\nSon Güncelleme: " + fi.LastWriteTime.ToString("dd.MM.yyyy HH:mm"),
                    "Şablon Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            return pnlRightCard;
        }

        private StatusStrip CreateStatusBar()
        {
            return statusMain;
        }

        private void SablonAra(string arama)
        {
            lstSablonlar.Items.Clear();
            var filtre = (arama ?? string.Empty).Trim();
            var secilenler = _tumSablonlar;
            if (!string.IsNullOrWhiteSpace(filtre))
                secilenler = _tumSablonlar.Where(x => x.IndexOf(filtre, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();

            foreach (var item in secilenler)
                lstSablonlar.Items.Add(item);

            if (lblDurumSayi != null)
                lblDurumSayi.Text = "Şablon Sayısı: " + lstSablonlar.Items.Count;
        }

        private void SablonSil()
        {
            if (lstSablonlar.SelectedItem == null)
            {
                MessageBox.Show(this, "Silmek için bir şablon seçiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ad = lstSablonlar.SelectedItem.ToString();
            var yol = Path.Combine(_sablonKlasoru, ad);
            if (MessageBox.Show(this, "Seçili şablon silinsin mi?\n" + ad, "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                File.Delete(yol);
                if (string.Equals(_aktifDosyaYolu, yol, StringComparison.OrdinalIgnoreCase))
                {
                    _aktifDosyaYolu = null;
                    rtbEditor.Clear();
                    lblAktifDosya.Text = "Aktif: -";
                }
                YukleSablonListesi();
                DurumYaz("Şablon silindi.");
                ButonDurumlariniGuncelle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Şablon silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SablonKopyala()
        {
            if (lstSablonlar.SelectedItem == null)
            {
                MessageBox.Show(this, "Kopyalamak için bir şablon seçiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var ad = lstSablonlar.SelectedItem.ToString();
                var kaynak = Path.Combine(_sablonKlasoru, ad);
                var ext = Path.GetExtension(ad);
                var baseName = Path.GetFileNameWithoutExtension(ad);
                var hedef = Path.Combine(_sablonKlasoru, baseName + "_Kopya" + ext);
                int i = 1;
                while (File.Exists(hedef))
                {
                    hedef = Path.Combine(_sablonKlasoru, baseName + "_Kopya_" + i + ext);
                    i++;
                }
                File.Copy(kaynak, hedef, false);
                YukleSablonListesi();
                DurumYaz("Şablon kopyası oluşturuldu.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Kopyalama sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WordIleAc()
        {
            if (string.IsNullOrWhiteSpace(_aktifDosyaYolu))
            {
                MessageBox.Show(this, "Önce bir dosya seçiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _aktifDosyaYolu,
                    UseShellExecute = true
                });
                DurumYaz("Dosya Word ile açıldı.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Word ile açılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DurumYaz(string mesaj)
        {
            if (lblDurumMesaj != null)
                lblDurumMesaj.Text = "Son İşlem: " + (mesaj ?? string.Empty);
        }

        private void ButonDurumlariniGuncelle()
        {
            var dosyaVar = !string.IsNullOrWhiteSpace(_aktifDosyaYolu) && File.Exists(_aktifDosyaYolu);
            var ext = dosyaVar ? (Path.GetExtension(_aktifDosyaYolu) ?? string.Empty).ToLowerInvariant() : string.Empty;
            var editorKaydetilebilir = ext == ".txt" || ext == ".rtf";
            var wordDosyasi = ext == ".doc" || ext == ".docx";

            btnKaydet.Enabled = editorKaydetilebilir;
            btnOnizleme.Enabled = editorKaydetilebilir;
            btnYazdir.Enabled = editorKaydetilebilir;
            if (btnWordIleAc != null) btnWordIleAc.Enabled = wordDosyasi || editorKaydetilebilir;
            if (btnSil != null) btnSil.Enabled = lstSablonlar.SelectedItem != null;
            if (btnKopyaOlustur != null) btnKopyaOlustur.Enabled = lstSablonlar.SelectedItem != null;
        }

        private static void StilButon(Button btn, Color arkaPlan)
        {
            if (btn == null) return;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = arkaPlan;
            btn.ForeColor = Color.White;
        }
    }

}
