using Kolera.Mebbis.Models;
using System;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisAdayMenuIslem : MebbisIslemBase
    {
        private readonly string _menuText;
        private readonly string _urlContains;
        private DateTime _last = DateTime.MinValue;

        public override string IslemAdi => "Aday Menü Yönlendirme";

        public MebbisAdayMenuIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, string menuText, string urlContains)
            : base(webBrowser, kursiyer)
        {
            _menuText = menuText ?? string.Empty;
            _urlContains = (urlContains ?? string.Empty).ToLowerInvariant();
        }

        public override void Baslat()
        {
            base.Baslat();
            _last = DateTime.Now;
        }

        public override void Tick()
        {
            if (!Basladi || Tamamlandi) return;
            if (WebBrowser?.Document == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 800) return;

            var currentUrl = (WebBrowser.Url?.ToString() ?? string.Empty).ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(_urlContains) && currentUrl.Contains(_urlContains))
            {
                Tamamlandi = true;
                return;
            }

            if (MebbisDomHelper.ClickTdByTitleOrText(WebBrowser, _menuText) ||
                MebbisDomHelper.ClickByText(WebBrowser, _menuText))
            {
                _last = DateTime.Now;
            }
        }
    }
}
