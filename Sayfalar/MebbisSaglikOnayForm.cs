using Kolera.Mebbis.Models;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class MebbisSaglikOnayForm : Form
    {
        private readonly MebbisKursiyerModel _kursiyer;
        private readonly string _evrakAdi;
        private readonly byte[] _evrakResim;
        private readonly string _captchaText;

        public MebbisSaglikOnayForm()
        {
            _kursiyer = BuildFallbackKursiyer();
            _evrakAdi = "Saglik";
            _evrakResim = null;
            _captchaText = "0000";

            InitializeComponent();
            if (!IsDesignerMode())
                ApplyDataToUi();
        }

        public MebbisSaglikOnayForm(MebbisKursiyerModel kursiyer, string evrakAdi = "Saglik", byte[] evrakResim = null)
        {
            _kursiyer = kursiyer ?? BuildFallbackKursiyer();
            _evrakAdi = string.IsNullOrWhiteSpace(evrakAdi) ? "Evrak" : evrakAdi;
            _evrakResim = evrakResim;
            _captchaText = new Random().Next(1000, 9999).ToString();

            InitializeComponent();
            if (!IsDesignerMode())
                ApplyDataToUi();
        }

        private static MebbisKursiyerModel BuildFallbackKursiyer()
        {
            return new MebbisKursiyerModel
            {
                TC_NO = "-",
                ADI = "Kursiyer",
                SOYADI = "Secilmedi",
                DONEM_ADI = "-",
                SERTIFIKA_SINIFI = "-",
                ONCE_SERT_SINIFI = "-"
            };
        }

        private bool IsDesignerMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (Site != null && Site.DesignMode)
                   || DesignMode;
        }

        private void ApplyDataToUi()
        {
            Text = $"{_evrakAdi} Evrak Onayi";
            lblTitle.Text = $"{_evrakAdi.ToUpperInvariant()} EVRAK AKISI ON KONTROL";
            lblDataState.Text = HasRealKursiyerData()
                ? "Kursiyer verisi yuklendi"
                : "Uyari: Kursiyer verisi eksik, varsayilan bilgiler gosteriliyor";
            lblDataState.ForeColor = HasRealKursiyerData() ? Color.LightGreen : Color.Gold;

            lblKursiyer.Text =
                $"TC: {_kursiyer?.TC_NO ?? "-"}\r\n" +
                $"Ad Soyad: {GetAdSoyad()}\r\n" +
                $"Donem: {_kursiyer?.DONEM_ADI ?? "-"}\r\n" +
                $"Yeni Sinif: {_kursiyer?.SERTIFIKA_SINIFI ?? "-"}\r\n" +
                $"Onceki Sinif: {_kursiyer?.ONCE_SERT_SINIFI ?? "-"}";

            chkBelgeDogru.Text = $"{_evrakAdi} evragi dogru kursiyere ait.";
            chkSorumlulukOnay.Text = $"Bilgileri kontrol ettim, {_evrakAdi} aktarimini onayliyorum.";
            lblCaptcha.Text = $"Guvenlik kodu: {_captchaText}";

            if (_evrakResim != null && _evrakResim.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(_evrakResim))
                    {
                        picEvrak.Image = Image.FromStream(ms);
                    }
                    lblImageState.Text = "Evrak Onizleme";
                }
                catch
                {
                    picEvrak.Image = null;
                    lblImageState.Text = "Evrak resmi okunamadi";
                }
            }
            else
            {
                picEvrak.Image = null;
                lblImageState.Text = "Evrak resmi bulunamadi";
            }
        }

        private void btnDevam_Click(object sender, EventArgs e)
        {
            if (!chkBelgeDogru.Checked || !chkSorumlulukOnay.Checked)
            {
                MessageBox.Show(this, "Lutfen iki onay kutusunu da isaretleyin.", "Uyari",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.Equals((txtCaptcha.Text ?? string.Empty).Trim(), _captchaText, StringComparison.Ordinal))
            {
                MessageBox.Show(this, "Guvenlik kodu hatali.", "Uyari",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private string GetAdSoyad()
        {
            var ad = (_kursiyer?.ADI ?? string.Empty).Trim();
            var soyad = (_kursiyer?.SOYADI ?? string.Empty).Trim();
            var full = $"{ad} {soyad}".Trim();
            return string.IsNullOrWhiteSpace(full) ? "-" : full;
        }

        private bool HasRealKursiyerData()
        {
            return !string.IsNullOrWhiteSpace(_kursiyer?.TC_NO) && _kursiyer.TC_NO != "-";
        }
    }
}
