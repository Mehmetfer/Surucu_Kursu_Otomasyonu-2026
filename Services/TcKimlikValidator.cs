using System;
using System.Linq;

namespace Kolera_Mtsk.Services
{
    /// <summary>
    /// T.C. kimlik numarasi (11 hane) giris kontrolu ve veritabani icin normalize.
    /// </summary>
    public static class TcKimlikValidator
    {
        /// <summary>Bos kabul; dolu ise hata aciklamasi doner (true = hata var).</summary>
        public static bool TryExplainProblem(string raw, out string uyari)
        {
            uyari = null;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            foreach (char c in raw)
            {
                if (!char.IsDigit(c) && !char.IsWhiteSpace(c))
                {
                    uyari = "TC Kimlik numarasi yalnizca rakam ve istege bagli bosluk icerebilir.";
                    return true;
                }
            }

            string digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.Length < 11)
            {
                uyari = "TC Kimlik No tam 11 rakamdan olusur. Su an " + digits.Length + " rakam girildi.";
                return true;
            }

            if (digits.Length > 11)
            {
                uyari = "TC Kimlik No tam 11 rakamdan olusur. Fazla rakam (" + digits.Length + " hane).";
                return true;
            }

            if (digits[0] == '0')
            {
                uyari = "TC Kimlik numarasi 0 ile baslayamaz.";
                return true;
            }

            if (!IsValidChecksum(digits))
            {
                uyari = "TC Kimlik numarasi kontrol hanesi hatasi (10. ve 11. hane algoritmasi). Lutfen rakamlari kontrol edin.";
                return true;
            }

            return false;
        }

        public static string NormalizeForDb(string tc)
        {
            if (string.IsNullOrWhiteSpace(tc))
                return null;
            string digits = new string(tc.Where(char.IsDigit).ToArray());
            if (digits.Length == 0)
                return null;
            return digits.Length <= 11 ? digits : digits.Substring(0, 11);
        }

        public static bool IsValidChecksum(string digits11)
        {
            if (digits11 == null || digits11.Length != 11)
                return false;

            int[] d = new int[11];
            for (int i = 0; i < 11; i++)
            {
                if (!char.IsDigit(digits11[i]))
                    return false;
                d[i] = digits11[i] - '0';
            }

            int odd = d[0] + d[2] + d[4] + d[6] + d[8];
            int even = d[1] + d[3] + d[5] + d[7];
            int check10 = (odd * 7 - even) % 10;
            if (check10 < 0)
                check10 += 10;
            if (d[9] != check10)
                return false;

            int sum10 = 0;
            for (int i = 0; i < 10; i++)
                sum10 += d[i];
            return d[10] == sum10 % 10;
        }
    }
}
