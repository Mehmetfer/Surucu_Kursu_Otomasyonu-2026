using Kolera.Mebbis.Models;
using Kolera.Evrak.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisAdayKimlikGetirIslem : MebbisIslemBase
    {
        private static readonly Dictionary<string, string> IstenenSinifValueMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "A1", "2502" },
            { "A2", "2503" },
            { "B", "2504" },
            { "D", "2506" }
        };

        private static readonly Dictionary<string, string> MevcutSinifValueMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "A", "2514" }, { "A1", "2502" }, { "A2", "2503" }, { "B", "2504" }, { "BE", "2518" },
            { "B1", "2515" }, { "C", "2505" }, { "CE", "2520" }, { "C1", "2516" }, { "C1E", "2519" },
            { "D", "2506" }, { "DE", "2522" }, { "D1", "2517" }, { "D1E", "2521" }, { "E", "2507" },
            { "F", "2508" }, { "G", "2509" }, { "H", "2512" }, { "M", "2513" }
        };

        private readonly KursiyerEvrak_Model _evrak;
        private DateTime _last = DateTime.MinValue;
        private int _step;
        private int _fillRetry;
        private int _sinifRetry;
        private int _ucretRetry;
        private bool _mevcutSinifPostbackDone;
        private bool _ucretRadioPostbackDone;

        public override string IslemAdi => "Aday Kimlik Getir";

        public MebbisAdayKimlikGetirIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, KursiyerEvrak_Model evrak = null)
            : base(webBrowser, kursiyer)
        {
            _evrak = evrak;
        }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
            _fillRetry = 0;
            _sinifRetry = 0;
            _ucretRetry = 0;
            _mevcutSinifPostbackDone = false;
            _ucretRadioPostbackDone = false;
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
                    var tcInput = WebBrowser.Document.GetElementById("txt_TcKimlikNo");
                    if (tcInput == null)
                        return;

                    if (!string.IsNullOrWhiteSpace(Kursiyer?.TC_NO))
                        tcInput.SetAttribute("value", Kursiyer.TC_NO);

                    // TC yazıldıktan sonra butona basmadan doğum tarihi + seri no alanlarını doldurmayı dene.
                    _step = 1;
                    _last = DateTime.Now;
                    break;

                case 1:
                    // Kimlik alanlarını doldur.
                    var seriInput = WebBrowser.Document.GetElementById("txt_serino");
                    var dogumInput = WebBrowser.Document.GetElementById("Us_tarih1_txtTarihGiris");

                    // Alanlar henüz gelmediyse kısa süre bekle.
                    if (seriInput == null && dogumInput == null)
                    {
                        _fillRetry++;
                        if (_fillRetry > 15)
                            Tamamlandi = true;
                        _last = DateTime.Now;
                        return;
                    }

                    var dogumTarihi = GetDateText(
                        "DOGUM_TARIHI", "DogumTarihi", "DOGUMTARIHI",
                        "NufusDogumTarihi", "KimlikDogumTarihi",
                        "KIM_DOGUM_TARIHI", "KimDogumTarihi", "KIMLIK_DOGUM_TARIHI",
                        "D_TARIHI", "DOGUM_TAR", "DTARIHI");
                    if (dogumInput != null && !string.IsNullOrWhiteSpace(dogumTarihi))
                        SetInputValueAndFireEvents(dogumInput, dogumTarihi);

                    var seriNo = GetStringValue(
                        "SERI_NO", "SeriNo", "KimlikSeriNo", "KIMLIK_SERI_NO",
                        "NufusSeriNo", "KimlikNoSeri", "KIMLIK_KAYIT_NO", "KimKayitNo");

                    // Yabancı uyruklu (TC 99...) adaylarda seri no yerine baba adı kullanılır.
                    if (IsForeignTc(Kursiyer?.TC_NO))
                    {
                        var babaAdi = GetStringValue("KIMLIK_BABA_ADI", "KimBabaAdi", "BABA_ADI", "BabaAdi");
                        if (!string.IsNullOrWhiteSpace(babaAdi))
                            seriNo = babaAdi;
                    }
                    if (seriInput != null && !string.IsNullOrWhiteSpace(seriNo))
                        SetInputValueAndFireEvents(seriInput, seriNo);

                    // Bazı MEBBİS ekranlarında id değişebiliyor, keyword fallback uygula.
                    if (!string.IsNullOrWhiteSpace(dogumTarihi))
                        MebbisDomHelper.SetInputByKeywords(WebBrowser, dogumTarihi, "tarihgiris", "dogum", "tarih1");
                    if (!string.IsNullOrWhiteSpace(seriNo))
                        MebbisDomHelper.SetInputByKeywords(WebBrowser, seriNo, "txt_serino", "serino", "seri_no");

                    _step = 2;
                    _last = DateTime.Now;
                    break;

                case 2:
                    // Önce istenilen sınıfı seç (postback tetiklenir, ücret paneli açılabilir).
                    if (!string.IsNullOrWhiteSpace(Kursiyer?.SERTIFIKA_SINIFI))
                    {
                        var parsed = ParseSinifAndSanziman(Kursiyer.SERTIFIKA_SINIFI);
                        var istenenSecildi = SelectEhliyetSinifiAndPostBack(
                            "cmbIstenenEhliyetSinifi", parsed.SinifKodu, true);

                        if (!istenenSecildi)
                        {
                            _sinifRetry++;
                            if (_sinifRetry > 12)
                                _step = 3;
                            _last = DateTime.Now;
                            return;
                        }

                        // Otomatik şanzıman bilgisi sınıf metninden geliyorsa işaretle.
                        if (parsed.Otomatik)
                            SetCheckboxChecked("chkbSanziman", true);
                    }

                    _step = 3;
                    _last = DateTime.Now;
                    break;

                case 3:
                    var belgeNoInput = WebBrowser.Document.GetElementById("txtEhliyetBelgeNo");
                    var cepTelInput = WebBrowser.Document.GetElementById("txtCepTelefonNo");
                    var ucretInput = WebBrowser.Document.GetElementById("txtUcret");
                    var ucretliRadio = WebBrowser.Document.GetElementById("rbUcret_0");

                    // Mevcut ehliyet sınıfı seçimi postback tetikler, bir kez yapıp bekle.
                    if (!_mevcutSinifPostbackDone && !string.IsNullOrWhiteSpace(Kursiyer?.ONCE_SERT_SINIFI))
                    {
                        var mevcut = ParseSinifAndSanziman(Kursiyer.ONCE_SERT_SINIFI);
                        SelectEhliyetSinifiAndPostBack("cmbMevcutEhliyetSinifi", mevcut.SinifKodu, false);
                        _mevcutSinifPostbackDone = true;
                        _last = DateTime.Now;
                        return;
                    }

                    // Ücretli radyo postback tetikler, bir kez yapıp bekle.
                    if (!_ucretRadioPostbackDone)
                    {
                        try
                        {
                            ucretliRadio?.SetAttribute("checked", "true");
                            ucretliRadio?.InvokeMember("click");
                            WebBrowser.Document?.InvokeScript("__doPostBack", new object[] { "rbUcret$0", string.Empty });
                        }
                        catch
                        {
                            // ignore
                        }
                        _ucretRadioPostbackDone = true;
                        _last = DateTime.Now;
                        return;
                    }

                    // Postback sonrası alanlar yerleşince metinleri doldur.
                    var belgeNo = GetStringValue("ONCE_SERT_BELGESAYI", "OnceSertBelgeSayi", "ONCE_SERTIFIKA_BELGE_NO");
                    if (belgeNoInput != null && !string.IsNullOrWhiteSpace(belgeNo))
                        SetInputValueAndFireEvents(belgeNoInput, belgeNo);

                    var gsm = NormalizePhone10(GetPreferredGsmValue());
                    if (cepTelInput != null && !string.IsNullOrWhiteSpace(gsm))
                        SetInputValueAndFireEvents(cepTelInput, gsm);

                    // SinifParam taban ücretini yaz.
                    if (ucretInput != null && Kursiyer?.TABAN_UCRET.HasValue == true)
                    {
                        SetInputValueAndFireEvents(
                            ucretInput,
                            Kursiyer.TABAN_UCRET.Value.ToString("0.##", CultureInfo.InvariantCulture));
                        Tamamlandi = true;
                        break;
                    }

                    // Ücret alanı postback ile geç gelebilir, bir süre bekle.
                    _ucretRetry++;
                    if (_ucretRetry > 12)
                        Tamamlandi = true;
                    _last = DateTime.Now;
                    break;
            }
        }

        private void SetInputValueAndFireEvents(HtmlElement input, string value)
        {
            if (input == null || string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                input.SetAttribute("value", value);
                input.Focus();
            }
            catch
            {
                // ignore
            }

            try
            {
                var id = input.GetAttribute("id") ?? string.Empty;
                var safeId = id.Replace("\\", "\\\\").Replace("'", "\\'");
                var safeValue = value.Replace("\\", "\\\\").Replace("'", "\\'");
                var script = @"
(function() {
  var el = document.getElementById('" + safeId + @"');
  if(!el) return;
  el.value = '" + safeValue + @"';
  if (document.createEvent) {
    var ev1 = document.createEvent('HTMLEvents');
    ev1.initEvent('change', true, false);
    el.dispatchEvent(ev1);
    var ev2 = document.createEvent('HTMLEvents');
    ev2.initEvent('blur', true, false);
    el.dispatchEvent(ev2);
  }
})();";
                WebBrowser.Document?.InvokeScript("eval", new object[] { script });
            }
            catch
            {
                // ignore
            }
        }

        private string GetDateText(params string[] names)
        {
            var date = TryGetDateFromObject(Kursiyer, names) ?? TryGetDateFromObject(_evrak, names);
            return date?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        private DateTime? TryGetDateFromObject(object obj, params string[] names)
        {
            if (obj == null || names == null) return null;

            var t = obj.GetType();
            foreach (var n in names)
            {
                var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) continue;

                var v = p.GetValue(obj);
                if (v == null) continue;

                if (v is DateTime dt)
                    return dt > new DateTime(1900, 1, 1) ? dt : (DateTime?)null;

                // Unix epoch/sn gibi sayısal tarih gelen durumlar
                if (long.TryParse(v.ToString(), out var numeric))
                {
                    try
                    {
                        if (numeric > 10_000_000_000) // ms
                        {
                            var unixMs = DateTimeOffset.FromUnixTimeMilliseconds(numeric).DateTime;
                            if (unixMs > new DateTime(1900, 1, 1))
                                return unixMs;
                        }
                        else if (numeric > 10_000_000) // s
                        {
                            var unixS = DateTimeOffset.FromUnixTimeSeconds(numeric).DateTime;
                            if (unixS > new DateTime(1900, 1, 1))
                                return unixS;
                        }
                    }
                    catch
                    {
                        // parse edilemeyen numeric tarihleri atla
                    }
                }

                if (DateTime.TryParse(v.ToString(), out var parsed)) return parsed;
            }

            return null;
        }

        private string GetStringValue(params string[] names)
        {
            var value = TryGetStringFromObject(Kursiyer, names);
            if (string.IsNullOrWhiteSpace(value))
                value = TryGetStringFromObject(_evrak, names);
            return value;
        }

        private static bool IsForeignTc(string tcNo)
        {
            if (string.IsNullOrWhiteSpace(tcNo))
                return false;

            var trimmed = tcNo.Trim();
            return trimmed.StartsWith("99", StringComparison.Ordinal);
        }

        private string GetPreferredGsmValue()
        {
            var tcDigits = DigitsOnly(Kursiyer?.TC_NO);
            var gsmFromKursiyer = GetStringValue("GSM_1", "GSM1");
            var gsmDigits = DigitsOnly(gsmFromKursiyer);

            // TC ile aynı değerse cep alanına yazma.
            if (!string.IsNullOrWhiteSpace(gsmDigits) && gsmDigits != tcDigits)
                return gsmFromKursiyer;

            // Evrak/model farklı isimli alan fallback'leri
            return GetStringValue("CEP_TELEFON", "CEP_TEL", "CepTelefonNo", "CepTel", "Telefon");
        }

        private bool SelectEhliyetSinifiAndPostBack(string comboId, string targetClass, bool isIstenen)
        {
            if (WebBrowser?.Document == null || string.IsNullOrWhiteSpace(comboId) || string.IsNullOrWhiteSpace(targetClass))
                return false;

            var valueMap = isIstenen ? IstenenSinifValueMap : MevcutSinifValueMap;
            var normalizedClass = NormalizeClassCode(targetClass);
            string targetValue;
            if (valueMap.TryGetValue(normalizedClass, out targetValue))
            {
                if (MebbisDomHelper.SelectComboByValueAndPostBack(WebBrowser, comboId, targetValue))
                    return true;
            }

            // İlk deneme: mevcut helper (contains + normalize)
            if (MebbisDomHelper.SelectComboByTextAndPostBack(WebBrowser, comboId, targetClass))
                return true;

            var combo = WebBrowser.Document.GetElementById(comboId);
            if (combo == null || combo.Children == null)
                return false;

            var target = targetClass.Trim().ToUpperInvariant();
            HtmlElement best = null;

            foreach (HtmlElement opt in combo.Children)
            {
                var text = (opt.InnerText ?? string.Empty).Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                if (text == target || text.StartsWith(target + " ", StringComparison.Ordinal) || text.Contains(target + " SINIFI"))
                {
                    best = opt;
                    break;
                }
            }

            if (best == null)
                return false;

            combo.SetAttribute("value", best.GetAttribute("value"));
            try
            {
                WebBrowser.Document.InvokeScript("__doPostBack", new object[] { comboId, string.Empty });
            }
            catch
            {
                // ignore
            }
            return true;
        }

        private void SetCheckboxChecked(string id, bool value)
        {
            if (WebBrowser?.Document == null || string.IsNullOrWhiteSpace(id))
                return;

            var checkbox = WebBrowser.Document.GetElementById(id);
            if (checkbox == null)
                return;

            try
            {
                checkbox.SetAttribute("checked", value ? "true" : string.Empty);
                if (value)
                    checkbox.InvokeMember("click");
            }
            catch
            {
                // ignore
            }

            try
            {
                WebBrowser.Document.InvokeScript("__doPostBack", new object[] { id, string.Empty });
            }
            catch
            {
                // ignore
            }
        }

        private static (string SinifKodu, bool Otomatik) ParseSinifAndSanziman(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return (string.Empty, false);

            var s = raw.Trim().ToUpperInvariant();
            var otomatik = s.Contains("OTOMAT");

            // En olası sınıf kodları uzunluktan kısaya.
            var candidates = new[] { "D1E", "C1E", "D1", "C1", "A2", "A1", "BE", "CE", "DE", "B1", "A", "B", "C", "D", "E", "F", "G", "H", "M" };
            for (int i = 0; i < candidates.Length; i++)
            {
                if (s.Contains(candidates[i]))
                    return (candidates[i], otomatik);
            }

            return (NormalizeClassCode(raw), otomatik);
        }

        private static string NormalizeClassCode(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            var s = raw.Trim().ToUpperInvariant();
            s = s.Replace("SINIFI", string.Empty).Replace("SERTIFIKA", string.Empty).Trim();
            return s;
        }

        private static string NormalizePhone10(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var onlyDigits = DigitsOnly(raw);

            if (string.IsNullOrWhiteSpace(onlyDigits))
                return null;

            if (onlyDigits.Length > 10)
                onlyDigits = onlyDigits.Substring(onlyDigits.Length - 10);

            return onlyDigits;
        }

        private static string DigitsOnly(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;

            var chars = raw.Trim().ToCharArray();
            var onlyDigits = string.Empty;
            for (int i = 0; i < chars.Length; i++)
            {
                if (char.IsDigit(chars[i]))
                    onlyDigits += chars[i];
            }
            return onlyDigits;
        }

        private string TryGetStringFromObject(object obj, params string[] names)
        {
            if (obj == null || names == null) return null;

            var t = obj.GetType();
            foreach (var n in names)
            {
                var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) continue;

                var v = p.GetValue(obj);
                var s = v?.ToString();
                if (!string.IsNullOrWhiteSpace(s))
                    return s.Trim();
            }

            return null;
        }
    }
}
