namespace Kolera_Mtsk.Mebbis
{
    public interface IMebbisIslem
    {
        void Baslat();
        void Tick();
        bool Tamamlandi { get; }
        string IslemAdi { get; }
    }
}