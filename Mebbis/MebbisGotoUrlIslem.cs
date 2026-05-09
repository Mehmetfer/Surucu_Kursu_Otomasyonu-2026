using Kolera.Mebbis.Models;
using System;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisGotoUrlIslem : MebbisIslemBase
    {
        private readonly string _targetPath;
        private readonly string _urlContains;
        private DateTime _last = DateTime.MinValue;
        private bool _navigated;

        public override string IslemAdi => "Mebbis URL Gecis";

        public MebbisGotoUrlIslem(WebBrowser webBrowser, MebbisKursiyerModel kursiyer, string targetPath)
            : base(webBrowser, kursiyer)
        {
            _targetPath = string.IsNullOrWhiteSpace(targetPath) ? "/SKT/skt02002.aspx" : targetPath;
            _urlContains = _targetPath.ToLowerInvariant();
        }

        public override void Baslat()
        {
            base.Baslat();
            _last = DateTime.Now;
            _navigated = false;
        }

        public override void Tick()
        {
            if (!Basladi || Tamamlandi) return;
            if (WebBrowser == null) return;
            if ((DateTime.Now - _last).TotalMilliseconds < 800) return;

            var currentUrl = (WebBrowser.Url?.ToString() ?? string.Empty).ToLowerInvariant();
            if (currentUrl.Contains(_urlContains))
            {
                Tamamlandi = true;
                return;
            }

            if (_navigated)
            {
                _last = DateTime.Now;
                return;
            }

            var baseUrl = (WebBrowser.Url != null && !string.IsNullOrWhiteSpace(WebBrowser.Url.Host))
                ? $"{WebBrowser.Url.Scheme}://{WebBrowser.Url.Host}"
                : "https://mebbis.meb.gov.tr";

            WebBrowser.Navigate($"{baseUrl}{_targetPath}");
            _navigated = true;
            _last = DateTime.Now;
        }
    }
}
