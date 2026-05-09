using Kolera.Evrak.Models;
using Kolera.Mebbis.Models;
using System;
using System.IO;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisImzaIslem : MebbisIslemBase
    {
        public override string IslemAdi => "İmza İşlemi";

        private readonly KursiyerEvrak_Model _evrak;
        private int _step;
        private int _retry;
        private DateTime _last = DateTime.MinValue;
        private string _tempImzaPath;

        public MebbisImzaIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, KursiyerEvrak_Model evrak = null)
            : base(webBrowser, kursiyer)
        {
            _evrak = evrak;
        }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
            _retry = 0;
            _last = DateTime.Now;
            _tempImzaPath = PrepareTempImage();
        }

        public override void Tick()
        {
            if (!Basladi || Tamamlandi) return;
            if (WebBrowser?.Document == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 800) return;

            switch (_step)
            {
                case 0:
                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday İmza Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "İmza Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday İmza") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Aday İmza Kayıt"))
                    {
                        _step++;
                        _last = DateTime.Now;
                    }
                    break;

                case 1:
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

                    // Sayfada eski parmak izi alanları varsa doldur.
                    if (_evrak != null)
                    {
                        var veren = WebBrowser.Document.GetElementById("txtParmakİziBelgesiVeren");
                        if (veren != null) veren.SetAttribute("value", _evrak.SavcilikBelgeVerenKurum ?? string.Empty);

                        var tarih = WebBrowser.Document.GetElementById("Us_tarih1_txtTarihGiris");
                        if (tarih != null) tarih.SetAttribute("value", _evrak.SavcilikBelgeTarihi?.ToString("dd/MM/yyyy") ?? string.Empty);

                        var sayi = WebBrowser.Document.GetElementById("txtParmakİziSayi");
                        if (sayi != null) sayi.SetAttribute("value", _evrak.SavcilikBelgeNo ?? string.Empty);
                    }

                    _step++;
                    _last = DateTime.Now;
                    break;

                case 3:
                    if (string.IsNullOrWhiteSpace(_tempImzaPath) || !File.Exists(_tempImzaPath))
                    {
                        _step++;
                        _last = DateTime.Now;
                        break;
                    }

                    try
                    {
                        var fileInput = WebBrowser.Document.GetElementById("flResimSec");
                        if (fileInput == null)
                        {
                            _retry++;
                            if (_retry > 10)
                                _step++;
                            _last = DateTime.Now;
                            return;
                        }

                        var frm = WebBrowser.FindForm();
                        frm?.Activate();
                        WebBrowser.Focus();
                        fileInput.InvokeMember("click");
                    }
                    catch
                    {
                        _retry++;
                        if (_retry > 10)
                            _step++;
                        _last = DateTime.Now;
                        return;
                    }

                    _step++;
                    _last = DateTime.Now;
                    break;

                case 4:
                    try
                    {
                        var frm = WebBrowser.FindForm();
                        frm?.Activate();
                        SendKeys.SendWait("\"" + _tempImzaPath + "\"");
                        SendKeys.SendWait("{ENTER}");
                    }
                    catch
                    {
                        // ignore
                    }

                    _step++;
                    _last = DateTime.Now;
                    break;

                case 5:
                    MebbisDomHelper.ClickById(WebBrowser, "btnGoster");
                    _step++;
                    _last = DateTime.Now;
                    break;

                case 6:
                    MebbisDomHelper.ClickByText(WebBrowser, "Kaydet");
                    MebbisDomHelper.ClickByText(WebBrowser, "Bilgileri Gönder");
                    Tamamlandi = true;
                    break;
            }
        }

        private string PrepareTempImage()
        {
            try
            {
                var data = _evrak?.ImgImza;
                if (data == null || data.Length == 0)
                    return null;

                const string folder = @"D:\Kolera_Mtsk\Kolera_Mtsk\AKTARILANLAR";
                Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, "MEBAKTAR.JPG");
                File.WriteAllBytes(path, data);
                return path;
            }
            catch
            {
                return null;
            }
        }
    }
}
