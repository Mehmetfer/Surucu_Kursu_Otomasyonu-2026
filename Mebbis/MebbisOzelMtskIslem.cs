using Kolera.Mebbis;
using Kolera.Mebbis.Models;
using System;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisOzelMtskIslem : MebbisIslemBase
    {
        private int _step;
        private DateTime _last = DateTime.Now;
        private int _menuRetry;

        public override string IslemAdi => "Özel MTSK";

        public MebbisOzelMtskIslem(WebBrowser wb, MebbisKursiyerModel kursiyer)
            : base(wb, kursiyer)
        {
        }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
            _last = DateTime.Now;
            _menuRetry = 0;
        }

        public override void Tick()
        {
            if (WebBrowser == null || WebBrowser.Document == null)
                return;

            if (WebBrowser.IsBusy)
                return;

            var currentUrl = (WebBrowser.Url?.ToString() ?? string.Empty).ToLowerInvariant();
            if (currentUrl.Contains("/skt/skt02001.aspx"))
            {
                Tamamlandi = true;
                return;
            }

            if ((DateTime.Now - _last).TotalMilliseconds < 800)
                return;

            switch (_step)
            {
                case 0:
                    // Zaten SKT modülündeysek (örn. SKT00001), modül seçimi adımını atla.
                    if (currentUrl.Contains("/skt/"))
                    {
                        _step = 2;
                        _last = DateTime.Now;
                        break;
                    }

                    // URL ile zorla yönlendirme bazı oturumlarda login ekranına düşürüyor.
                    // Bu yüzden sadece mevcut menüden modül tıklaması yapıyoruz.
                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Özel Motorlu Taşıt Sürücüleri Kursu Modülü") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Özel MTSK Modülü") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "MTSK") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Özel Motorlu Taşıt Sürücüleri Kursu Modülü") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Özel MTSK Modülü"))
                    {
                        _step++;
                        _last = DateTime.Now;
                    }
                    break;

                case 1:
                    // Kullanıcının tarif ettiği gibi önce SKT02000 alt menüsünü aç.
                    if (IsAdayKayitMenuExpanded())
                    {
                        _step++;
                        _last = DateTime.Now;
                        break;
                    }

                    EnsureAdayKayitMenuExpanded();
                    _menuRetry++;

                    // Menü hiç açılmıyorsa doğrudan aday dönem ekranına git.
                    if (_menuRetry >= 4)
                    {
                        NavigateDirectToAdayDonem();
                    }
                    _last = DateTime.Now;
                    break;

                case 2:
                    // Menü tekrar kapanmışsa yeniden açıp sonra alt maddeyi tıkla.
                    if (!IsAdayKayitMenuExpanded())
                    {
                        _step = 1;
                        _last = DateTime.Now;
                        break;
                    }

                    if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Dönem Kayıt İşlemleri") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Dönem Kayıt") ||
                        MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Aday Dönem") ||
                        MebbisDomHelper.ClickByText(WebBrowser, "Aday Dönem Kayıt İşlemleri") ||
                        ClickKabartmaByTitle("Aday Dönem Kayıt İşlemleri") ||
                        ClickTdByOnclickContains("/skt/skt02001.aspx"))
                    {
                        _menuRetry = 0;
                        _last = DateTime.Now;
                    }
                    else
                    {
                        _menuRetry++;
                        if (_menuRetry >= 4)
                        {
                            NavigateDirectToAdayDonem();
                        }
                        _last = DateTime.Now;
                    }
                    break;
            }
        }

        private bool IsAdayKayitMenuExpanded()
        {
            var menu = WebBrowser?.Document?.GetElementById("SKT02000");
            if (menu == null)
                return false;

            var style = (menu.GetAttribute("style") ?? string.Empty).ToLowerInvariant();
            if (style.Contains("visibility:hidden") || style.Contains("position:absolute"))
                return false;

            return true;
        }

        private void EnsureAdayKayitMenuExpanded()
        {
            // Parent satırı aç: "Kurum Aday Kayıt İşlemleri"
            if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, "Kurum Aday Kayıt İşlemleri") ||
                MebbisDomHelper.ClickByText(WebBrowser, "Kurum Aday Kayıt İşlemleri"))
            {
                return;
            }

            // HTML text/title bozuk gelse de parent menünün onclick'i sabit.
            ClickTdByOnclickContains("gostersakla('skt02000')");
            ClickTdByOnclickContains("gostersakla(\"skt02000\")");
        }

        private bool ClickTdByOnclickContains(string token)
        {
            if (WebBrowser?.Document == null || string.IsNullOrWhiteSpace(token))
                return false;

            var t = token.ToLowerInvariant();
            foreach (HtmlElement el in WebBrowser.Document.GetElementsByTagName("td"))
            {
                var onclick = (el.GetAttribute("onclick") ?? string.Empty).ToLowerInvariant();
                if (!onclick.Contains(t))
                    continue;

                try
                {
                    el.InvokeMember("click");
                    return true;
                }
                catch
                {
                    // Bir sonraki adaya geç.
                }
            }

            return false;
        }

        private bool ClickKabartmaByTitle(string titleText)
        {
            if (WebBrowser?.Document == null || string.IsNullOrWhiteSpace(titleText))
                return false;

            var target = titleText.Trim().ToLowerInvariant();
            foreach (HtmlElement el in WebBrowser.Document.GetElementsByTagName("td"))
            {
                var cls = (el.GetAttribute("className") ?? string.Empty).ToLowerInvariant();
                if (!cls.Contains("kabartma"))
                    continue;

                var title = (el.GetAttribute("title") ?? string.Empty).Trim().ToLowerInvariant();
                if (!title.Contains(target))
                    continue;

                try
                {
                    el.InvokeMember("click");
                    return true;
                }
                catch
                {
                    // Bazı durumlarda click yerine onclick scriptini eval ile tetiklemek gerekir.
                    try
                    {
                        var onClick = el.GetAttribute("onclick");
                        if (!string.IsNullOrWhiteSpace(onClick))
                        {
                            WebBrowser.Document.InvokeScript("eval", new object[] { onClick });
                            return true;
                        }
                    }
                    catch
                    {
                        // Sonraki elemana geç.
                    }
                }
            }

            return false;
        }

        private void NavigateDirectToAdayDonem()
        {
            try
            {
                var current = WebBrowser?.Url;
                if (current == null)
                    return;

                var target = $"{current.Scheme}://{current.Host}/SKT/skt02001.aspx";
                WebBrowser.Navigate(target);
            }
            catch
            {
                // Güvenli fallback: navigation başarısız olursa mevcut akış devam eder.
            }
        }
    }
}