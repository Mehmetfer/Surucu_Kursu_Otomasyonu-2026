using System.Windows.Forms;
using Kolera.Mebbis.Models;
using Kolera.Evrak.Models;
using System;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisSabikaIslem : MebbisIslemBase
    {
        public override string IslemAdi => "Sabıka İşlemi";
        private readonly KursiyerEvrak_Model _evrak;
        private int _step;
        private DateTime _last = DateTime.MinValue;

        public MebbisSabikaIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, KursiyerEvrak_Model evrak = null)
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
            if (WebBrowser == null || WebBrowser.Document == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 800) return;

            switch (_step)
            {
                case 0:
                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Sabıka Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Sabıka Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Sabıka") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Aday Sabıka Kayıt"))
                    {
                        _step++;
                        _last = DateTime.Now;
                    }
                    break;
                case 1:
                    // SKT02005 ilk adım: dönem + aday seçimi.
                    if (WebBrowser.Document.GetElementById("cmbEgitimDonemi") != null)
                    {
                        MebbisDomHelper.SelectComboByText(WebBrowser, "cmbEgitimDonemi", Kursiyer.DONEM_ADI);

                        var adaySecildi =
                            MebbisDomHelper.SelectComboByValue(WebBrowser, "cmbAdayAdSoyad", Kursiyer.TC_NO) ||
                            MebbisDomHelper.SelectComboByText(WebBrowser, "cmbAdayAdSoyad", (Kursiyer.ADI + " " + Kursiyer.SOYADI).Trim());

                        if (adaySecildi)
                        {
                            _last = DateTime.Now;
                            _step++;
                        }
                        else
                        {
                            _last = DateTime.Now;
                        }
                        break;
                    }

                    // Eğer bu aşamaya doğrudan detay formunda geldiysek devam et.
                    _step++;
                    _last = DateTime.Now;
                    break;
                case 2:
                    var tcLbl = WebBrowser.Document.GetElementById("lblTcKimlikNo");
                    var tcHazir = tcLbl != null && !string.IsNullOrWhiteSpace(tcLbl.InnerText);
                    if (!tcHazir)
                    {
                        _last = DateTime.Now;
                        return;
                    }

                    if (_evrak != null)
                    {
                        var veren = WebBrowser.Document.GetElementById("txtSavcilikBelgesiVeren");
                        if (veren != null) veren.SetAttribute("value", _evrak.SavcilikBelgeVerenKurum ?? string.Empty);

                        var tarih = WebBrowser.Document.GetElementById("Us_tarih1_txtTarihGiris");
                        if (tarih != null) tarih.SetAttribute("value", _evrak.SavcilikBelgeTarihi?.ToString("dd/MM/yyyy") ?? string.Empty);

                        var sayi = WebBrowser.Document.GetElementById("txtSavcilikBelgeSayi");
                        if (sayi != null) sayi.SetAttribute("value", _evrak.SavcilikBelgeNo ?? string.Empty);
                    }
                    MebbisDomHelper.ClickByText(WebBrowser, "Kaydet");
                    MebbisDomHelper.ClickByText(WebBrowser, "Bilgileri Gönder");
                    Tamamlandi = true;
                    break;
            }
        }
    }
}