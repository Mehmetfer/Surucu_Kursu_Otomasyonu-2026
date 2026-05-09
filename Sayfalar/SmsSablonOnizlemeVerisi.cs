using System;

namespace Kolera_Mtsk.Sayfalar
{
    /// <summary>
    /// SMS sablonunda kullanilacak gercek degerler (onizleme ve Kaydet/Devam oncesi).
    /// </summary>
    public sealed class SmsSablonOnizlemeVerisi
    {
        public string AdSoyad { get; set; }
        public string Telefon { get; set; }
        public string KursAdi { get; set; }
        public DateTime Tarih { get; set; }
        public string Saat { get; set; }

        public SmsSablonOnizlemeVerisi()
        {
            Tarih = DateTime.Today;
            Saat = DateTime.Now.ToString("HH:mm");
        }
    }
}
