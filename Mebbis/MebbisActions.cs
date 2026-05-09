using System.Windows.Forms;
using Kolera_Mtsk.Mebbis;
namespace Kolera_Mtsk.Mebbis
{
    public static class MebbisActions
    {
        public static void OpenOzelMtsk(MebbisEngine engine)
        {
            foreach (HtmlElement td in engine.Browser.Document.GetElementsByTagName("td"))
            {
                if (td.GetAttribute("title") == "Özel Motorlu Taşıt Sürücüleri Kursu Modülü")
                {
                    engine.WaitPage();
                    td.InvokeMember("click");
                    return;
                }
            }
        }

        public static void ClickKurumAday(MebbisEngine engine)
        {
            foreach (HtmlElement td in engine.Browser.Document.GetElementsByTagName("td"))
            {
                if (td.GetAttribute("title") == "Kurum Aday Kayıt İşlemleri")
                {
                    engine.WaitPage();
                    td.InvokeMember("click");
                    return;
                }
            }
        }
    }
}