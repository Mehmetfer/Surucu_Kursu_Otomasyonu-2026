using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;
using Kolera.Evrak.Models;
using Kolera.Mebbis.Models;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisFaturaIslem : MebbisIslemBase
    {
        public override string IslemAdi => "Fatura İşlemi";

        private readonly KursiyerEvrak_Model _evrak;
        private int _step;
        private DateTime _last = DateTime.MinValue;

        public MebbisFaturaIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, KursiyerEvrak_Model evrak = null)
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
                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Fatura Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Fatura Kayıt İşlemleri") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Aday Fatura Kayıt"))
                    {
                        _step++;
                        _last = DateTime.Now;
                        return;
                    }

                    if ((WebBrowser.Url?.ToString() ?? string.Empty).ToLowerInvariant().Contains("/skt/skt02013.aspx"))
                    {
                        _step++;
                        _last = DateTime.Now;
                    }
                    break;

                case 1:
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
                    // Bazı durumlarda TC sorgula adımı gerekiyor.
                    var tcInput = WebBrowser.Document.GetElementById("txtTC");
                    if (tcInput != null)
                    {
                        tcInput.SetAttribute("value", Kursiyer?.TC_NO ?? string.Empty);
                        MebbisDomHelper.ClickById(WebBrowser, "btnTC");
                    }

                    _step++;
                    _last = DateTime.Now;
                    break;

                case 3:
                    var tcLbl = WebBrowser.Document.GetElementById("lblTcKimlikNo");
                    var tcHazir = tcLbl != null && !string.IsNullOrWhiteSpace(tcLbl.InnerText);

                    // Panel gelmediyse postback tamamlanana kadar bekle.
                    if (!tcHazir && WebBrowser.Document.GetElementById("pnlAdayBilgileri") == null)
                    {
                        _last = DateTime.Now;
                        return;
                    }

                    FillInvoiceForm();

                    MebbisDomHelper.ClickByText(WebBrowser, "Kaydet");
                    MebbisDomHelper.ClickByText(WebBrowser, "Bilgileri Gönder");
                    Tamamlandi = true;
                    break;
            }
        }

        private void FillInvoiceForm()
        {
            // Tarih/Tutar bilinen alanlar
            var tarih = WebBrowser.Document.GetElementById("Us_tarih1_txtTarihGiris");
            if (tarih != null)
            {
                var tarihText = GetDateText(
                    "FaturaTarihi",
                    "FaturaBelgeTarihi",
                    "BelgeTarihi");
                if (!string.IsNullOrWhiteSpace(tarihText))
                    tarih.SetAttribute("value", tarihText);
            }

            var tutar = WebBrowser.Document.GetElementById("txtTutar");
            if (tutar != null)
            {
                var tutarText = GetNumberText(
                    "FaturaTutari",
                    "FaturaTutar",
                    "Tutar");
                if (!string.IsNullOrWhiteSpace(tutarText))
                    tutar.SetAttribute("value", tutarText);
            }

            // Fatura sayı alanı
            var sayi = WebBrowser.Document.GetElementById("txtSayi");
            if (sayi != null)
            {
                var sayiText = GetStringValue(
                    "FaturaSayi",
                    "FaturaNo",
                    "FaturaBelgeNo",
                    "BelgeNo");
                if (!string.IsNullOrWhiteSpace(sayiText))
                    sayi.SetAttribute("value", sayiText);
            }
        }

        private string GetDateText(params string[] propertyNames)
        {
            if (_evrak == null) return null;

            foreach (var propertyName in propertyNames)
            {
                var prop = _evrak.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop == null) continue;

                var value = prop.GetValue(_evrak);
                if (value == null) continue;

                if (value is DateTime dt)
                    return dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                if (DateTime.TryParse(value.ToString(), out var parsed))
                    return parsed.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            }

            return null;
        }

        private string GetNumberText(params string[] propertyNames)
        {
            if (_evrak == null) return null;

            foreach (var propertyName in propertyNames)
            {
                var prop = _evrak.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop == null) continue;

                var value = prop.GetValue(_evrak);
                if (value == null) continue;

                if (value is decimal dec)
                    return dec.ToString("0.##", CultureInfo.InvariantCulture);
                if (value is double dbl)
                    return dbl.ToString("0.##", CultureInfo.InvariantCulture);
                if (value is float flt)
                    return flt.ToString("0.##", CultureInfo.InvariantCulture);
                if (value is int i)
                    return i.ToString(CultureInfo.InvariantCulture);
                if (value is long l)
                    return l.ToString(CultureInfo.InvariantCulture);

                if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedInvariant))
                    return parsedInvariant.ToString("0.##", CultureInfo.InvariantCulture);
                if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out var parsedTr))
                    return parsedTr.ToString("0.##", CultureInfo.InvariantCulture);
            }

            return null;
        }

        private string GetStringValue(params string[] propertyNames)
        {
            if (_evrak == null) return null;

            foreach (var propertyName in propertyNames)
            {
                var prop = _evrak.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (prop == null) continue;

                var value = prop.GetValue(_evrak);
                if (value == null) continue;

                var text = value.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                    return text.Trim();
            }

            return null;
        }
    }
}
