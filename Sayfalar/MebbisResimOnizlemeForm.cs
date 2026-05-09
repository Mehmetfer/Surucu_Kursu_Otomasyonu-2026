using System;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class MebbisResimOnizlemeForm : Form
    {
        private const string MebAktarFolder = @"D:\Kolera_Mtsk\Kolera_Mtsk\AKTARILANLAR";
        private const string MebAktarFileName = "MEBAKTAR.JPG";

        private readonly byte[] _resimData;
        private readonly string _dialogTitle;
        private readonly string _infoText;
        private readonly string _emptyText;

        public string SavedImagePath { get; private set; }

        // WinForms Designer bu formu parametresiz olusturur.
        public MebbisResimOnizlemeForm()
            : this(null)
        {
        }

        public MebbisResimOnizlemeForm(
            byte[] resimData,
            string dialogTitle = "MEBBIS Resim Onizleme",
            string infoText = "Yuklenecek resmi kontrol edin. Dogruysa 'Aktar' secin.",
            string emptyText = "Resim bulunamadi. Once ilgili evrak resmini yuklemelisiniz.")
        {
            _resimData = resimData;
            _dialogTitle = string.IsNullOrWhiteSpace(dialogTitle) ? "MEBBIS Resim Onizleme" : dialogTitle;
            _infoText = string.IsNullOrWhiteSpace(infoText) ? "Yuklenecek resmi kontrol edin. Dogruysa 'Aktar' secin." : infoText;
            _emptyText = string.IsNullOrWhiteSpace(emptyText) ? "Resim bulunamadi. Once ilgili evrak resmini yuklemelisiniz." : emptyText;

            InitializeComponent();
            Text = _dialogTitle;
            _lblInfo.Text = _infoText;

            Load += MebbisResimOnizlemeForm_Load;
            _btnAktar.Click += BtnAktar_Click;
            _btnVazgec.Click += BtnVazgec_Click;
        }

        private void BtnAktar_Click(object sender, EventArgs e)
        {
            var savePath = SaveImageToDisk();
            if (string.IsNullOrWhiteSpace(savePath))
                return;

            SavedImagePath = savePath;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnVazgec_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void MebbisResimOnizlemeForm_Load(object sender, EventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            if (_resimData == null || _resimData.Length == 0)
            {
                _lblInfo.Text = _emptyText;
                _btnAktar.Enabled = false;
                return;
            }

            try
            {
                using (var ms = new MemoryStream(_resimData))
                {
                    _picture.Image = Image.FromStream(ms);
                }
            }
            catch
            {
                _lblInfo.Text = "Resim onizlenemedi. Resim formatini kontrol edin.";
                _btnAktar.Enabled = false;
            }
        }

        private string SaveImageToDisk()
        {
            try
            {
                if (_resimData == null || _resimData.Length == 0)
                {
                    MessageBox.Show("Kaydedilecek resim verisi bulunamadi.", "MEBBIS Resim Onizleme", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return null;
                }

                Directory.CreateDirectory(MebAktarFolder);
                var path = Path.Combine(MebAktarFolder, MebAktarFileName);
                File.WriteAllBytes(path, _resimData);
                return path;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Resim kaydedilemedi: " + ex.Message, "MEBBIS Resim Onizleme", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
