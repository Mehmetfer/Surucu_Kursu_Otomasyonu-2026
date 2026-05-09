using Kolera.Mebbis.Models;
using Kolera_Mtsk.Mebbis;
using System;
using System.Windows.Forms;

namespace Kolera_Mtsk.Mebbis
{
    public class MebbisGenelIslem : MebbisIslemBase
    {
        private int _step = 0;
        private DateTime _last = DateTime.MinValue;

        public override string IslemAdi => "Genel MTSK";

        public MebbisGenelIslem(WebBrowser wb, MebbisKursiyerModel kursiyer)
            : base(wb, kursiyer) { }

        public override void Baslat()
        {
            base.Baslat();
            _step = 0;
        }

        public override void Tick()
        {
            if (WebBrowser == null || WebBrowser.Document == null)
                return;

            if (WebBrowser.ReadyState != WebBrowserReadyState.Complete)
                return;

            if ((DateTime.Now - _last).TotalMilliseconds < 800)
                return;

            switch (_step)
            {
                case 0:
                    WebBrowser.Navigate("https://mebbisyd.meb.gov.tr/SKT/skt00001.aspx");
                    _step++;
                    _last = DateTime.Now;
                    break;

                case 1:
                    foreach (HtmlElement el in WebBrowser.Document.GetElementsByTagName("td"))
                    {
                        if (el.GetAttribute("title") == "Kurum Aday Kayıt İşlemleri")
                        {
                            el.InvokeMember("click");
                            _step++;
                            _last = DateTime.Now;
                            break;
                        }
                    }
                    break;

                case 2:
                    Tamamlandi = true;
                    break;
            }
        }
    }
}