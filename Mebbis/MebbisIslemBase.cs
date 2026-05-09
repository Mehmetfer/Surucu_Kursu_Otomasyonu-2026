using System;
using System.Windows.Forms;
using Kolera.Mebbis.Models;

namespace Kolera_Mtsk.Mebbis
{
    public abstract class MebbisIslemBase : IMebbisIslem
    {
        protected WebBrowser WebBrowser;
        protected MebbisKursiyerModel Kursiyer;

        public bool Basladi { get; private set; }
        public bool Tamamlandi { get; protected set; }

        public abstract string IslemAdi { get; }

        protected MebbisIslemBase(WebBrowser webBrowser, MebbisKursiyerModel kursiyer)
        {
            WebBrowser = webBrowser;
            Kursiyer = kursiyer;
        }

        public virtual void Baslat()
        {
            Basladi = true;
            Tamamlandi = false;
        }

        public abstract void Tick();

        public virtual void Iptal()
        {
            Tamamlandi = true;
        }
    }
}