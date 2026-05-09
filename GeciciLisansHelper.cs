using Kolera_MTSK.LoginL;

namespace Kolera_Mtsk
{
    public static class GeciciLisansHelper
    {
        public static bool GeciciAktif { get; private set; } = false;

        public static void AktifEt()
        {
            GeciciAktif = true;
            LisansDurum.Demo = false;
        }

        public static void Sifirla()
        {
            GeciciAktif = false;
        }
    }
}