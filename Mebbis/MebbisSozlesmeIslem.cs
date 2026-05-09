using System.Windows.Forms;
using Kolera.Mebbis.Models;
using Kolera.Evrak.Models;
using System;
using System.IO;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisSozlesmeIslem : MebbisIslemBase
    {
        public override string IslemAdi => "Sözleşme İşlemi";
        private readonly KursiyerEvrak_Model _evrak;
        private readonly byte[] _sozlesmeResimData;
        private readonly bool _arkaYuz;
        private int _step;
        private int _retry;
        private DateTime _last = DateTime.MinValue;
        private string _tempImagePath;

        public MebbisSozlesmeIslem(
            WebBrowser webBrowser,
            MebbisKursiyerModel kursiyer,
            KursiyerEvrak_Model evrak = null,
            byte[] sozlesmeResimData = null,
            bool arkaYuz = false)
            : base(webBrowser, kursiyer)
        {
            _evrak = evrak;
            _sozlesmeResimData = sozlesmeResimData;
            _arkaYuz = arkaYuz;
        }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
            _retry = 0;
            _last = DateTime.Now;
            _tempImagePath = PrepareTempImage();
        }

        public override void Tick()
        {
            if (!Basladi || Tamamlandi) return;
            if (WebBrowser == null || WebBrowser.Document == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 800) return;

            switch (_step)
            {
                case 0:
                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Sözleşme Bilgisi Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Sözleşme Bilgisi Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Sözleşme") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Aday Sözleşme Bilgisi Kayıt"))
                    {
                        _step++;
                        _last = DateTime.Now;
                    }
                    break;
                case 1:
                    // SKT02011 ilk adım: dönem + aday seçimi
                    if (WebBrowser.Document.GetElementById("cmbEgitimDonemi") != null)
                    {
                        MebbisDomHelper.SelectComboByText(WebBrowser, "cmbEgitimDonemi", Kursiyer.DONEM_ADI);

                        var adaySecildi =
                            MebbisDomHelper.SelectComboByValue(WebBrowser, "cmbAdayAdSoyad", Kursiyer.TC_NO) ||
                            MebbisDomHelper.SelectComboByText(WebBrowser, "cmbAdayAdSoyad", (Kursiyer.ADI + " " + Kursiyer.SOYADI).Trim());

                        if (adaySecildi)
                        {
                            _step++;
                            _last = DateTime.Now;
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
                        var tarih = WebBrowser.Document.GetElementById("Us_tarih1_txtTarihGiris");
                        if (tarih != null) tarih.SetAttribute("value", _evrak.FaturaTarihi?.ToString("dd/MM/yyyy") ?? string.Empty);

                        var ucret = WebBrowser.Document.GetElementById("txtUcret");
                        if (ucret != null)
                        {
                            string ucretText = _evrak.FaturaTutari.HasValue
                                ? $"{_evrak.FaturaTutari.Value:0.##} TL"
                                : string.Empty;
                            ucret.SetAttribute("value", ucretText);
                        }
                    }

                    _step++;
                    _last = DateTime.Now;
                    break;

                case 3:
                    // Sozlesme resmi yuklemesi: on yuz -> flResimSec/btnGoster, arka yuz -> File1/btnGoster2
                    if (string.IsNullOrWhiteSpace(_tempImagePath) || !File.Exists(_tempImagePath))
                    {
                        _step++;
                        _last = DateTime.Now;
                        break;
                    }

                    try
                    {
                        var input = FindFileInput();
                        if (input == null)
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
                        input.InvokeMember("click");
                        _step++;
                        _last = DateTime.Now;
                    }
                    catch
                    {
                        _retry++;
                        if (_retry > 10)
                            _step++;
                        _last = DateTime.Now;
                    }
                    break;

                case 4:
                    try
                    {
                        var frm = WebBrowser.FindForm();
                        frm?.Activate();
                        SendKeys.SendWait("\"" + _tempImagePath + "\"");
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
                    FindUploadButton()?.InvokeMember("click");
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
                var data = _sozlesmeResimData;
                if (data == null || data.Length == 0)
                    return null;

                var folder = @"D:\Kolera_Mtsk\Kolera_Mtsk\AKTARILANLAR";
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

        private HtmlElement FindFileInput()
        {
            if (WebBrowser?.Document == null)
                return null;

            var primaryId = _arkaYuz ? "File1" : "flResimSec";
            var byId = WebBrowser.Document.GetElementById(primaryId);
            if (byId != null)
                return byId;

            foreach (HtmlElement input in WebBrowser.Document.GetElementsByTagName("input"))
            {
                var type = (input.GetAttribute("type") ?? string.Empty).ToLowerInvariant();
                if (type == "file")
                    return input;
            }
            return null;
        }

        private HtmlElement FindUploadButton()
        {
            if (WebBrowser?.Document == null)
                return null;

            var primaryId = _arkaYuz ? "btnGoster2" : "btnGoster";
            var byId = WebBrowser.Document.GetElementById(primaryId);
            if (byId != null)
                return byId;

            foreach (HtmlElement input in WebBrowser.Document.GetElementsByTagName("input"))
            {
                var type = (input.GetAttribute("type") ?? string.Empty).ToLowerInvariant();
                if (type != "submit" && type != "button" && type != "image")
                    continue;

                var value = (input.GetAttribute("value") ?? string.Empty).ToLowerInvariant();
                if (value.Contains("resim ekle"))
                    return input;
            }
            return null;
        }
    }
}