using Kolera.Mebbis.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisFotoAdaySecIslem : MebbisIslemBase
    {
        private int _step;
        private int _retry;
        private DateTime _last = DateTime.MinValue;

        public override string IslemAdi => "Foto Donem Aday Sec";

        public MebbisFotoAdaySecIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer)
            : base(webBrowser, kursiyer)
        {
        }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
            _retry = 0;
            _last = DateTime.Now;
        }

        public override void Tick()
        {
            if (!Basladi || Tamamlandi) return;
            if (WebBrowser?.Document == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 900) return;

            switch (_step)
            {
                case 0:
                    // cmbEgitimDonemi
                    var donemText = Kursiyer?.DONEM_ADI ?? string.Empty;
                    var donemValue = BuildDonemValueFromText(donemText);
                    var donemOk =
                        MebbisDomHelper.SelectComboByValueAndPostBack(WebBrowser, "cmbEgitimDonemi", donemValue) ||
                        MebbisDomHelper.SelectComboByTextAndPostBack(WebBrowser, "cmbEgitimDonemi", donemText);

                    if (!donemOk)
                    {
                        _retry++;
                        if (_retry > 20)
                        {
                            MessageBox.Show("Donem secilemedi. Fotograf aktarim adimi sonlandirildi.", "MEBBIS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            Tamamlandi = true;
                        }
                        return;
                    }

                    _retry = 0;
                    _step = 1;
                    _last = DateTime.Now;
                    break;

                case 1:
                    // cmbAdayAdSoyad
                    if (SelectAdayByBestMatch())
                    {
                        Tamamlandi = true;
                        return;
                    }

                    _retry++;
                    if (_retry > 25)
                    {
                        MessageBox.Show("Aday listesinde kursiyer bulunamadi. Fotograf aktarim adimi sonlandirildi.", "MEBBIS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Tamamlandi = true;
                    }
                    break;
            }
        }

        private bool SelectAdayByBestMatch()
        {
            var combo = WebBrowser.Document?.GetElementById("cmbAdayAdSoyad");
            if (combo == null)
                return false;

            var hedef = Normalize($"{Kursiyer?.ADI} {Kursiyer?.SOYADI}");
            if (string.IsNullOrWhiteSpace(hedef))
                return false;

            HtmlElement best = null;
            var bestScore = -1;

            foreach (HtmlElement opt in combo.Children)
            {
                var value = (opt.GetAttribute("value") ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value) || value == "-1")
                    continue;

                var txt = Normalize(opt.InnerText ?? string.Empty);
                if (string.IsNullOrWhiteSpace(txt))
                    continue;

                var score = Score(hedef, txt);
                if (score > bestScore)
                {
                    best = opt;
                    bestScore = score;
                }
            }

            if (best == null || bestScore < 0)
                return false;

            return MebbisDomHelper.SelectComboByValueAndPostBack(
                WebBrowser, "cmbAdayAdSoyad", best.GetAttribute("value"));
        }

        private static int Score(string hedef, string secenek)
        {
            if (secenek == hedef) return 1000;
            if (secenek.Contains(hedef)) return 700;

            var tokens = hedef.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var hit = 0;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (secenek.Contains(tokens[i]))
                    hit++;
            }

            return hit > 0 ? hit * 100 : -1;
        }

        private static string Normalize(string text)
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

            int year;
            if (!int.TryParse(yearText, out year))
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

            int month;
            if (!monthMap.TryGetValue(monthText, out month))
                return string.Empty;

            return $"{year}{month:00}";
        }
    }
}
