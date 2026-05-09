using System;
using System.Windows.Forms;
using Kolera.Mebbis.Models;
using Kolera.Evrak.Models;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisSaglikIslem : MebbisIslemBase
    {
        public override string IslemAdi => "Sağlık İşlemi";
        private readonly KursiyerEvrak_Model _evrak;
        private int _step;
        private DateTime _last = DateTime.MinValue;

        public MebbisSaglikIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, KursiyerEvrak_Model evrak = null)
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

            if ((DateTime.Now - _last).TotalMilliseconds < 800)
                return;

            switch (_step)
            {
                case 0:
                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Sağlık Raporu Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Sağlık Raporu Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Sağlık") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Aday Sağlık Raporu Kayıt"))
                    {
                        _step++;
                        _last = DateTime.Now;
                    }
                    break;
                case 1:
                    // SKT02004 ilk adım: dönem + aday seçimi + listele
                    if (WebBrowser.Document.GetElementById("cmbEgitimDonemi") != null)
                    {
                        MebbisDomHelper.SelectComboByText(WebBrowser, "cmbEgitimDonemi", Kursiyer.DONEM_ADI);

                        var adaySecildi =
                            MebbisDomHelper.SelectComboByValue(WebBrowser, "cmbAdayAdSoyad", Kursiyer.TC_NO) ||
                            MebbisDomHelper.SelectComboByText(WebBrowser, "cmbAdayAdSoyad", (Kursiyer.ADI + " " + Kursiyer.SOYADI).Trim());

                        if (adaySecildi)
                        {
                            MebbisDomHelper.ClickById(WebBrowser, "Button1");
                            _last = DateTime.Now;
                            _step++;
                        }
                        else
                        {
                            _last = DateTime.Now;
                        }
                        break;
                    }

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
                        var veren = WebBrowser.Document.GetElementById("txtSaglikRaporuVeren");
                        if (veren != null) veren.SetAttribute("value", _evrak.SaglikBelverenKurum ?? string.Empty);

                        var tarih = WebBrowser.Document.GetElementById("Us_tarih1_txtTarihGiris");
                        if (tarih != null) tarih.SetAttribute("value", _evrak.SaglikBelgeTarihi?.ToString("dd/MM/yyyy") ?? string.Empty);

                        var sayi = WebBrowser.Document.GetElementById("txtSaglikRaporSayi");
                        if (sayi != null) sayi.SetAttribute("value", _evrak.SaglikBelgeNo ?? string.Empty);
                    }

                    // Bu alan sayfada zorunlu; veri yoksa "Engeli Yok" seçilir.
                    MebbisDomHelper.SelectComboByText(WebBrowser, "cmbAdayOzurDurumu", "Engeli Yok");

                    MebbisDomHelper.ClickByText(WebBrowser, "Kaydet");
                    MebbisDomHelper.ClickByText(WebBrowser, "Bilgileri Gönder");
                    Tamamlandi = true;
                    break;
            }
        }
    }
}