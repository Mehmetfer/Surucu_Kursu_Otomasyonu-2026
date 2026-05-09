using Kolera.Mebbis.Models;
using Kolera_Mtsk.Mebbis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Kolera.Mebbis
{
    public class MebbisDonemSecIslem : MebbisIslemBase
    {
        private bool _adayKayitSekmesiAcik;
        private bool _donem;
        private bool _grup;
        private bool _sube;
        private bool _listele;
        private bool _adayBilgiFormuHazir;

        private DateTime _last = DateTime.MinValue;

        public override string IslemAdi => "Dönem Seçim";

        public MebbisDonemSecIslem(WebBrowser wb, MebbisKursiyerModel k)
            : base(wb, k) { }

        public override void Baslat()
        {
            base.Baslat();
            _adayKayitSekmesiAcik = false;
            _donem = _grup = _sube = _listele = _adayBilgiFormuHazir = false;
            _last = DateTime.Now;
        }

        public override void Tick()
        {
            if (WebBrowser?.Document == null) return;

            if ((DateTime.Now - _last).TotalMilliseconds < 1000)
                return;

            // SKT02001 sayfasında "Aday Kayıt İşlemleri" sekmesi kapalıysa önce aç.
            if (!_adayKayitSekmesiAcik)
            {
                var donemCombo = WebBrowser.Document.GetElementById("cmbEgitimDonemi");
                if (donemCombo == null)
                {
                    MebbisDomHelper.ClickById(WebBrowser, "Button1");
                    _last = DateTime.Now;
                    return;
                }

                _adayKayitSekmesiAcik = true;
            }

            // DÖNEM
            if (!_donem)
            {
                var donemText = Kursiyer?.DONEM_ADI ?? string.Empty;
                var donemValue = BuildDonemValueFromText(donemText);

                _donem =
                    MebbisDomHelper.SelectComboByValueAndPostBack(WebBrowser, "cmbEgitimDonemi", donemValue) ||
                    MebbisDomHelper.SelectComboByTextAndPostBack(WebBrowser, "cmbEgitimDonemi", donemText);

                if (_donem) _last = DateTime.Now;
                return;
            }

            // GRUP
            if (!_grup)
            {
                _grup = SelectComboByBestTextMatchAndPostBack(
                    "cmbGrubu", Kursiyer?.GRUP_ADI);

                if (_grup) _last = DateTime.Now;
                return;
            }

            // ŞUBE
            if (!_sube)
            {
                _sube = SelectComboByBestTextMatchAndPostBack(
                    "cmbSubesi", Kursiyer?.SUBE);

                if (_sube) _last = DateTime.Now;
                return;
            }

            // "Listele" butonuna basıp aday listesini getir.
            if (!_listele)
            {
                _listele = MebbisDomHelper.ClickById(WebBrowser, "btnImageButton");
                if (_listele) _last = DateTime.Now;
                return;
            }

            // Listele sonrası gelen aday kimlik formunun açıldığını doğrula.
            if (!_adayBilgiFormuHazir)
            {
                var doc = WebBrowser.Document;
                _adayBilgiFormuHazir =
                    doc.GetElementById("pnlBilgiGiris") != null ||
                    doc.GetElementById("txt_TcKimlikNo") != null ||
                    doc.GetElementById("lblDonemi") != null;

                if (!_adayBilgiFormuHazir)
                {
                    _last = DateTime.Now;
                    return;
                }
            }

            Tamamlandi = true;
        }

        private bool SelectComboByBestTextMatchAndPostBack(string comboId, string targetText)
        {
            if (WebBrowser?.Document == null) return false;
            if (string.IsNullOrWhiteSpace(comboId) || string.IsNullOrWhiteSpace(targetText)) return false;

            var combo = WebBrowser.Document.GetElementById(comboId);
            if (combo == null) return false;

            var targetNorm = NormalizeForMatch(targetText);
            if (string.IsNullOrWhiteSpace(targetNorm)) return false;

            HtmlElement bestOpt = null;
            var bestScore = -1;

            foreach (HtmlElement opt in combo.Children)
            {
                var value = (opt.GetAttribute("value") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value) || value == "-1")
                    continue;

                var inner = (opt.InnerText ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(inner))
                    continue;

                var score = ScoreMatch(targetNorm, NormalizeForMatch(inner));
                if (score > bestScore)
                {
                    bestScore = score;
                    bestOpt = opt;
                }
            }

            if (bestOpt == null || bestScore < 0)
                return false;

            return MebbisDomHelper.SelectComboByValueAndPostBack(
                WebBrowser,
                comboId,
                bestOpt.GetAttribute("value"));
        }

        private static int ScoreMatch(string targetNorm, string optionNorm)
        {
            if (string.IsNullOrWhiteSpace(targetNorm) || string.IsNullOrWhiteSpace(optionNorm))
                return -1;

            if (optionNorm == targetNorm) return 1000;                  // birebir
            if (optionNorm.StartsWith(targetNorm + " ")) return 800;    // baştan eşleşme
            if (optionNorm.Contains(" " + targetNorm + " ")) return 700;// ortada tam token
            if (optionNorm.EndsWith(" " + targetNorm)) return 650;      // sondan eşleşme
            if (optionNorm.Contains(targetNorm)) return 500;            // genel içerme

            // token bazlı kısmi skor
            var targetTokens = targetNorm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var hit = 0;
            foreach (var tok in targetTokens)
            {
                if (optionNorm.Contains(tok))
                    hit++;
            }

            return hit > 0 ? (hit * 100) : -1;
        }

        private static string NormalizeForMatch(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var s = text.Trim().ToLowerInvariant()
                .Replace('ı', 'i')
                .Replace('ğ', 'g')
                .Replace('ü', 'u')
                .Replace('ş', 's')
                .Replace('ö', 'o')
                .Replace('ç', 'c');

            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                if (char.IsLetterOrDigit(ch))
                    sb.Append(ch);
                else
                    sb.Append(' ');
            }

            return string.Join(" ", sb.ToString()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static string BuildDonemValueFromText(string donemText)
        {
            // Örnek: "2026 - Mayıs" => "202605"
            if (string.IsNullOrWhiteSpace(donemText))
                return string.Empty;

            var parts = donemText.Split('-');
            if (parts.Length < 2)
                return string.Empty;

            var yearText = (parts[0] ?? string.Empty).Trim();
            var monthText = (parts[1] ?? string.Empty).Trim().ToLowerInvariant()
                .Replace('ı', 'i')
                .Replace('ğ', 'g')
                .Replace('ü', 'u')
                .Replace('ş', 's')
                .Replace('ö', 'o')
                .Replace('ç', 'c');

            if (!int.TryParse(yearText, out var year) || year < 2000 || year > 2100)
                return string.Empty;

            var monthMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["ocak"] = 1,
                ["subat"] = 2,
                ["mart"] = 3,
                ["nisan"] = 4,
                ["mayis"] = 5,
                ["haziran"] = 6,
                ["temmuz"] = 7,
                ["agustos"] = 8,
                ["eylul"] = 9,
                ["ekim"] = 10,
                ["kasim"] = 11,
                ["aralik"] = 12
            };

            if (!monthMap.TryGetValue(monthText, out var month))
                return string.Empty;

            return $"{year}{month:00}";
        }
    }
}