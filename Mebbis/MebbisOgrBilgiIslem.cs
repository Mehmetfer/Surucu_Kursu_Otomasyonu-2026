using Kolera.Mebbis;
using Kolera.Mebbis.Models;
using Kolera.Evrak.Models;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisOgrBilgiIslem : MebbisIslemBase
    {
        public override string IslemAdi => "Öğrenim Bilgileri";

        private int _step = 0;
        private readonly KursiyerEvrak_Model _evrak;
        private readonly byte[] _ogrBelgeResimData;
        private DateTime _last = DateTime.MinValue;
        private int _retry = 0;
        private string _tempOgrBelgePath;

        public MebbisOgrBilgiIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, KursiyerEvrak_Model evrak = null, byte[] ogrBelgeResimData = null)
            : base(webBrowser, kursiyer)
        {
            _evrak = evrak;
            _ogrBelgeResimData = ogrBelgeResimData;
        }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
            _retry = 0;
            _last = DateTime.Now;
            _tempOgrBelgePath = PrepareOgrBelgeTempImage();
        }

        public override void Tick()
        {
            if (!Basladi || Tamamlandi) return;
            if (WebBrowser == null || WebBrowser.Document == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 800) return;

            switch (_step)
            {
                case 0:
                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Öğrenim Bilgisi Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Öğrenim Bilgisi Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Öğrenim Bilgileri") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Aday Öğrenim Bilgisi Kayıt"))
                    {
                        _step++;
                        _last = DateTime.Now;
                    }
                    break;

                case 1:
                    // SKT02003 ilk adım: dönem + aday seçimi
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
                        var tur = WebBrowser.Document.GetElementById("cmbOgrenimBelgesiTuru");
                        if (tur != null && !string.IsNullOrWhiteSpace(_evrak.OgrBelgeTuru))
                        {
                            MebbisDomHelper.SelectComboByText(WebBrowser, "cmbOgrenimBelgesiTuru", _evrak.OgrBelgeTuru);
                        }

                        var veren = WebBrowser.Document.GetElementById("txtOgrenimBelgesiVeren");
                        if (veren != null) veren.SetAttribute("value", _evrak.OgrBelgeVerenKurum ?? string.Empty);

                        var tarih = WebBrowser.Document.GetElementById("Us_tarih1_txtTarihGiris");
                        if (tarih != null)
                        {
                            var tarihText = FormatDate(_evrak.OgrBelgeTarihi);
                            tarih.SetAttribute("value", tarihText);
                            TryFireInputEvents("Us_tarih1_txtTarihGiris");
                        }

                        var sayi = WebBrowser.Document.GetElementById("txtOgrenimBelgeSayi");
                        if (sayi != null) sayi.SetAttribute("value", _evrak.OgrBelgeSayisi ?? string.Empty);
                    }

                    _step++;
                    _last = DateTime.Now;
                    break;

                case 3:
                    // Ogrenim belgesi resmi yuklemesi (flResimSec + btnGoster).
                    if (string.IsNullOrWhiteSpace(_tempOgrBelgePath) || !File.Exists(_tempOgrBelgePath))
                    {
                        _step++;
                        _last = DateTime.Now;
                        break;
                    }

                    try
                    {
                        var input = WebBrowser.Document.GetElementById("flResimSec");
                        if (input == null)
                        {
                            _retry++;
                            if (_retry > 10)
                            {
                                _step++;
                            }
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
                    // Dosya seciciye tam yolu gonder.
                    try
                    {
                        var frm = WebBrowser.FindForm();
                        frm?.Activate();
                        SendKeys.SendWait("\"" + _tempOgrBelgePath + "\"");
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
                    // Resim Ekle butonu.
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

        private static string FormatDate(DateTime? dt)
        {
            if (!dt.HasValue)
                return string.Empty;

            return dt.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        private void TryFireInputEvents(string elementId)
        {
            if (WebBrowser?.Document == null || string.IsNullOrWhiteSpace(elementId))
                return;

            try
            {
                WebBrowser.Document.InvokeScript("eval", new object[]
                {
                    "try{" +
                    "var el=document.getElementById('" + elementId + "');" +
                    "if(el){" +
                    "if(typeof el.onchange==='function') el.onchange();" +
                    "if(typeof el.onblur==='function') el.onblur();" +
                    "}" +
                    "}catch(e){}"
                });
            }
            catch
            {
                // ignore
            }
        }

        private string PrepareOgrBelgeTempImage()
        {
            try
            {
                var data = _ogrBelgeResimData ?? _evrak?.ImgOgrBel;
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
    }
}