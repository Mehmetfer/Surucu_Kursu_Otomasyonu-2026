using System;
using System.Globalization;

namespace Kolera_Mtsk.Sayfalar.EkProgramlar
{
    public class TakvimDers
    {
        public string Donem { get; set; }
        public string GrupAdi { get; set; }
        public string Subesi { get; set; }
        public string DersTuru { get; set; }
        public string DerslikAdi { get; set; }
        public string DersTarihi { get; set; }
        public string DersSaati { get; set; }
        public string DersiVeren { get; set; }
        public string EgitimTuru { get; set; }
        public string AdayAdSoyad { get; set; }
        public string AracPlakasi { get; set; }

        public DateTime Baslangic
        {
            get
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(DersTarihi) || string.IsNullOrWhiteSpace(DersSaati))
                        return DateTime.MinValue;

                    string saatText = DersSaati.Split('-')[0].Trim();
                    DateTime tarih = DateTime.ParseExact(DersTarihi.Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    TimeSpan saat = TimeSpan.Parse(saatText);
                    return tarih.Date.Add(saat);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }
    }
}
