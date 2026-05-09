using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public static class MebbisDomHelper
    {
        public static bool ClickTd(WebBrowser wb, string text)
        {
            if (wb?.Document == null) return false;

            string t = text.ToLower();

            foreach (HtmlElement el in wb.Document.GetElementsByTagName("td"))
            {
                string inner = (el.InnerText ?? "").ToLower();
                string title = (el.GetAttribute("title") ?? "").ToLower();

                if (inner.Contains(t) || title.Contains(t))
                {
                    try { el.InvokeMember("click"); return true; }
                    catch { }
                }
            }
            return false;
        }

        public static bool ClickTdByTitleOrText(WebBrowser wb, string text)
            => ClickTd(wb, text);

        public static bool ClickById(WebBrowser wb, string id)
        {
            var el = wb?.Document?.GetElementById(id);
            if (el == null) return false;

            try { el.InvokeMember("click"); return true; }
            catch { return false; }
        }

        public static bool ClickInputByValue(WebBrowser wb, string value)
        {
            if (wb?.Document == null) return false;

            string v = value.ToLower();

            foreach (HtmlElement el in wb.Document.GetElementsByTagName("input"))
            {
                string val = (el.GetAttribute("value") ?? "").ToLower();

                if (val.Contains(v))
                {
                    try { el.InvokeMember("click"); return true; }
                    catch { }
                }
            }
            return false;
        }

        public static bool SelectComboByText(WebBrowser wb, string id, string text)
        {
            if (wb?.Document == null) return false;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var combo = wb.Document.GetElementById(id);
            if (combo == null) return false;

            string t = text.Trim().ToLowerInvariant();
            var tNorm = NormalizeForMatch(text);
            var tTokens = tNorm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (HtmlElement opt in combo.Children)
            {
                string inner = ((opt.InnerText ?? string.Empty).Trim()).ToLowerInvariant();
                var innerNorm = NormalizeForMatch(inner);

                // 1) Doğrudan contains
                if (inner.Contains(t))
                {
                    combo.SetAttribute("value", opt.GetAttribute("value"));
                    return true;
                }

                // 2) Normalize edilmiş metinlerde contains (Türkçe karakter/noktalama farklarını tolere et)
                if (!string.IsNullOrWhiteSpace(tNorm) &&
                    (innerNorm.Contains(tNorm) || tNorm.Contains(innerNorm)))
                {
                    combo.SetAttribute("value", opt.GetAttribute("value"));
                    return true;
                }

                // 3) Token bazlı benzerlik (örn: "1 Grup A" vs "1. Grup A Şubesi")
                if (tTokens.Length > 0 && tTokens.All(tok => innerNorm.Contains(tok)))
                {
                    combo.SetAttribute("value", opt.GetAttribute("value"));
                    return true;
                }
            }
            return false;
        }

        public static bool SelectComboByTextAndPostBack(WebBrowser wb, string id, string text)
        {
            if (!SelectComboByText(wb, id, text))
                return false;

            TriggerComboPostBack(wb, id);
            return true;
        }

        public static bool SelectComboByValue(WebBrowser wb, string id, string value)
        {
            if (wb?.Document == null) return false;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var combo = wb.Document.GetElementById(id);
            if (combo == null) return false;

            var target = value.Trim();

            foreach (HtmlElement opt in combo.Children)
            {
                if (string.Equals((opt.GetAttribute("value") ?? string.Empty).Trim(), target, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SetAttribute("value", opt.GetAttribute("value"));
                    return true;
                }
            }

            return false;
        }

        public static bool SelectComboByValueAndPostBack(WebBrowser wb, string id, string value)
        {
            if (!SelectComboByValue(wb, id, value))
                return false;

            TriggerComboPostBack(wb, id);
            return true;
        }

        private static void TriggerComboPostBack(WebBrowser wb, string id)
        {
            if (wb?.Document == null || string.IsNullOrWhiteSpace(id))
                return;

            var combo = wb.Document.GetElementById(id);
            if (combo == null)
                return;

            try
            {
                combo.InvokeMember("onchange");
            }
            catch
            {
                // Bazı sayfalarda onchange invoke başarısız olabilir.
            }

            try
            {
                wb.Document.InvokeScript("__doPostBack", new object[] { id, string.Empty });
            }
            catch
            {
                // __doPostBack tanımlı değilse sessizce devam.
            }
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

        public static bool SetInputByKeywords(WebBrowser wb, string value, params string[] keywords)
        {
            if (wb?.Document == null || string.IsNullOrWhiteSpace(value) || keywords == null || keywords.Length == 0)
                return false;

            foreach (HtmlElement el in wb.Document.GetElementsByTagName("input"))
            {
                string type = (el.GetAttribute("type") ?? "").ToLowerInvariant();
                if (type == "hidden" || type == "button" || type == "submit")
                    continue;

                string id = (el.GetAttribute("id") ?? "").ToLowerInvariant();
                string name = (el.GetAttribute("name") ?? "").ToLowerInvariant();
                string cls = (el.GetAttribute("className") ?? "").ToLowerInvariant();
                string title = (el.GetAttribute("title") ?? "").ToLowerInvariant();
                string meta = id + "|" + name + "|" + cls + "|" + title;

                foreach (var keyword in keywords)
                {
                    if (string.IsNullOrWhiteSpace(keyword))
                        continue;

                    string k = keyword.ToLowerInvariant();
                    if (!meta.Contains(k))
                        continue;

                    try
                    {
                        el.SetAttribute("value", value);
                        el.Focus();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public static bool ClickByText(WebBrowser wb, string text)
        {
            if (wb?.Document == null || string.IsNullOrWhiteSpace(text))
                return false;

            string t = text.ToLowerInvariant();

            foreach (HtmlElement el in wb.Document.GetElementsByTagName("button"))
            {
                string inner = (el.InnerText ?? "").ToLowerInvariant();
                if (inner.Contains(t))
                {
                    try { el.InvokeMember("click"); return true; } catch { }
                }
            }

            foreach (HtmlElement el in wb.Document.GetElementsByTagName("a"))
            {
                string inner = (el.InnerText ?? "").ToLowerInvariant();
                if (inner.Contains(t))
                {
                    try { el.InvokeMember("click"); return true; } catch { }
                }
            }

            foreach (HtmlElement el in wb.Document.GetElementsByTagName("input"))
            {
                string value = (el.GetAttribute("value") ?? "").ToLowerInvariant();
                if (value.Contains(t))
                {
                    try { el.InvokeMember("click"); return true; } catch { }
                }
            }

            return false;
        }
    }
}