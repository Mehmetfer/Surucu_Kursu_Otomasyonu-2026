using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Kolera.Evrak.Models;
using Kolera.Mebbis.Models;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisAdresIslem : MebbisIslemBase
    {
        public override string IslemAdi => "Adres Beyan İşlemi";

        private readonly KursiyerEvrak_Model _evrak;
        private DateTime _last = DateTime.MinValue;
        private int _step;

        public MebbisAdresIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, KursiyerEvrak_Model evrak = null)
            : base(webBrowser, kursiyer)
        {
            _evrak = evrak;
        }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
            _last = DateTime.Now;
        }

        public override void Tick()
        {
            if (!Basladi || Tamamlandi) return;
            if (WebBrowser?.Document == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 800) return;

            switch (_step)
            {
                case 0:
                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Adres Beyan") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Aday Adres Beyan"))
                    {
                        _step++;
                        _last = DateTime.Now;
                        return;
                    }

                    // Zaten doğru sayfadaysa menü klik beklemeyelim.
                    if ((WebBrowser.Url?.ToString() ?? string.Empty).ToLowerInvariant().Contains("/skt/skt02012.aspx"))
                    {
                        _step++;
                        _last = DateTime.Now;
                    }
                    break;

                case 1:
                    // İlk ekran: dönem + aday seçimi (postback tetikler).
                    if (WebBrowser.Document.GetElementById("cmbEgitimDonemi") != null)
                    {
                        MebbisDomHelper.SelectComboByText(WebBrowser, "cmbEgitimDonemi", Kursiyer?.DONEM_ADI ?? string.Empty);

                        var adaySecildi =
                            MebbisDomHelper.SelectComboByValue(WebBrowser, "cmbAdayAdSoyad", Kursiyer?.TC_NO ?? string.Empty) ||
                            MebbisDomHelper.SelectComboByText(WebBrowser, "cmbAdayAdSoyad", ((Kursiyer?.ADI ?? string.Empty) + " " + (Kursiyer?.SOYADI ?? string.Empty)).Trim());

                        if (adaySecildi)
                        {
                            _step++;
                            _last = DateTime.Now;
                        }
                        else
                        {
                            _last = DateTime.Now;
                        }
                        return;
                    }

                    _step++;
                    _last = DateTime.Now;
                    break;

                case 2:
                    // Detay paneli açılana kadar bekle.
                    if (WebBrowser.Document.GetElementById("txtTcKimlikNo") == null &&
                        WebBrowser.Document.GetElementById("pnlBilgiGiris") == null)
                    {
                        _last = DateTime.Now;
                        return;
                    }

                    var tcInput = WebBrowser.Document.GetElementById("txtTcKimlikNo");
                    if (tcInput != null)
                    {
                        tcInput.SetAttribute("value", Kursiyer?.TC_NO ?? string.Empty);
                    }

                    MebbisDomHelper.ClickById(WebBrowser, "btnTCGetir");
                    _step++;
                    _last = DateTime.Now;
                    break;

                case 3:
                    // Kimlik bilgisi geldikten sonra adres seçimlerine geç.
                    var adLabel = WebBrowser.Document.GetElementById("lblAd");
                    var adHazir = adLabel != null && !string.IsNullOrWhiteSpace(adLabel.InnerText);
                    if (!adHazir && WebBrowser.Document.GetElementById("ddlIl") == null)
                    {
                        _last = DateTime.Now;
                        return;
                    }

                    TryFillAddressCombos();

                    MebbisDomHelper.ClickByText(WebBrowser, "Kaydet");
                    MebbisDomHelper.ClickByText(WebBrowser, "Bilgileri Gönder");
                    Tamamlandi = true;
                    break;
            }
        }

        private void TryFillAddressCombos()
        {
            // Modelin tipini bilmiyoruz; reflection ile yaygın property adlarını deniyoruz.
            var il = GetEvrakValue("AdresIl", "Il", "IlAdi", "AdresIlAdi", "IlKodu", "AdresIlKodu");
            if (!string.IsNullOrWhiteSpace(il))
            {
                if (IsNumeric(il))
                    MebbisDomHelper.SelectComboByValue(WebBrowser, "ddlIl", il);
                else
                    MebbisDomHelper.SelectComboByText(WebBrowser, "ddlIl", il);
            }

            var ilce = GetEvrakValue("AdresIlce", "Ilce", "IlceAdi", "AdresIlceAdi", "IlceKodu", "AdresIlceKodu");
            if (!string.IsNullOrWhiteSpace(ilce))
            {
                if (IsNumeric(ilce))
                    MebbisDomHelper.SelectComboByValue(WebBrowser, "ddlIlce", ilce);
                else
                    MebbisDomHelper.SelectComboByText(WebBrowser, "ddlIlce", ilce);
            }

            var mahalle = GetEvrakValue("AdresMahalle", "Mahalle", "MahalleKoy", "MahalleKodu", "AdresMahalleKodu");
            if (!string.IsNullOrWhiteSpace(mahalle))
            {
                if (IsNumeric(mahalle))
                    MebbisDomHelper.SelectComboByValue(WebBrowser, "ddlMahalle", mahalle);
                else
                    MebbisDomHelper.SelectComboByText(WebBrowser, "ddlMahalle", mahalle);
            }

            var cadde = GetEvrakValue("AdresCadde", "Cadde", "Sokak", "CaddeSokak", "CaddeKodu", "AdresCaddeKodu");
            if (!string.IsNullOrWhiteSpace(cadde))
            {
                if (IsNumeric(cadde))
                    MebbisDomHelper.SelectComboByValue(WebBrowser, "ddlCadde", cadde);
                else
                    MebbisDomHelper.SelectComboByText(WebBrowser, "ddlCadde", cadde);
            }

            var disKapi = GetEvrakValue("AdresDisKapiNo", "DisKapiNo", "DisKapi", "AdresDisKapiKodu");
            if (!string.IsNullOrWhiteSpace(disKapi))
            {
                if (IsNumeric(disKapi))
                    MebbisDomHelper.SelectComboByValue(WebBrowser, "ddlDisKapi", disKapi);
                else
                    MebbisDomHelper.SelectComboByText(WebBrowser, "ddlDisKapi", disKapi);
            }

            var icKapi = GetEvrakValue("AdresIcKapiNo", "IcKapiNo", "IcKapi", "AdresIcKapiKodu");
            if (!string.IsNullOrWhiteSpace(icKapi))
            {
                if (IsNumeric(icKapi))
                    MebbisDomHelper.SelectComboByValue(WebBrowser, "ddlIcKapi", icKapi);
                else
                    MebbisDomHelper.SelectComboByText(WebBrowser, "ddlIcKapi", icKapi);
            }
        }

        private string GetEvrakValue(params string[] propertyNames)
        {
            if (_evrak == null || propertyNames == null || propertyNames.Length == 0)
                return null;

            var type = _evrak.GetType();
            foreach (var name in propertyNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var prop = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop == null)
                    continue;

                var raw = prop.GetValue(_evrak);
                if (raw == null)
                    continue;

                switch (raw)
                {
                    case string s when !string.IsNullOrWhiteSpace(s):
                        return s.Trim();
                    case int i:
                        return i.ToString(CultureInfo.InvariantCulture);
                    case long l:
                        return l.ToString(CultureInfo.InvariantCulture);
                    default:
                        var text = raw.ToString();
                        if (!string.IsNullOrWhiteSpace(text))
                            return text.Trim();
                        break;
                }
            }

            return null;
        }

        private static bool IsNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return long.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }
    }
}
