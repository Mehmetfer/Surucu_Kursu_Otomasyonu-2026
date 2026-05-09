using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Kolera_Mtsk
{
    public enum LisansCalismaModu
    {
        Demo = 0,
        Active = 1,
        Passive = 2
    }

    public static class LisansPolitikasi
    {
        private const int OfflineMaxWriteCount = 10;
        private static readonly string StateFilePath = Path.Combine(Application.StartupPath, "lisans_runtime_state.txt");
        private static int _suppressWebLicenseCallDepth = 0;

        public static LisansCalismaModu Mod { get; private set; } = LisansCalismaModu.Demo;
        public static DateTime LastLicenseCheckAt { get; private set; } = DateTime.MinValue;
        public static bool IsInternetAvailableForLicense { get; private set; } = true;
        public static int OfflineRemainingWriteCount { get; private set; } = OfflineMaxWriteCount;

        static LisansPolitikasi()
        {
            LoadState();
        }

        public static void SetMode(LisansCalismaModu mode)
        {
            Mod = mode;
        }

        public static void RegisterLicenseCheck(bool internetAvailable)
        {
            LastLicenseCheckAt = DateTime.Now;
            IsInternetAvailableForLicense = internetAvailable;
            if (internetAvailable)
                OfflineRemainingWriteCount = OfflineMaxWriteCount;
            SaveState();
        }

        public static bool IsWriteAllowed
        {
            get
            {
                if (Mod == LisansCalismaModu.Passive)
                    return false;
                return true;
            }
        }

        public static bool EnsureWriteAllowed()
        {
            if (Mod == LisansCalismaModu.Passive)
            {
                MessageBox.Show(
                    "Lisans durumu PASIVE oldugu icin veritabani yazma islemleri kapatildi.",
                    "Lisans Kisitlamasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        public static void RegisterSuccessfulWrite()
        {
            // Offline yazma limiti devre disi: sadece lisans passive modunda yazma engellenir.
        }

        public static bool IsWebLicenseCallSuppressed
        {
            get { return _suppressWebLicenseCallDepth > 0; }
        }

        public static void BeginSuppressWebLicenseCalls()
        {
            System.Threading.Interlocked.Increment(ref _suppressWebLicenseCallDepth);
        }

        public static void EndSuppressWebLicenseCalls()
        {
            int value = System.Threading.Interlocked.Decrement(ref _suppressWebLicenseCallDepth);
            if (value < 0)
                System.Threading.Interlocked.Exchange(ref _suppressWebLicenseCallDepth, 0);
        }

        public static string GetLastLicenseCheckText()
        {
            if (LastLicenseCheckAt == DateTime.MinValue)
                return "Son Lisans Kontrol: -";
            return "Son Lisans Kontrol: " + LastLicenseCheckAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("tr-TR"));
        }

        private static void LoadState()
        {
            try
            {
                if (!File.Exists(StateFilePath))
                    return;

                var lines = File.ReadAllLines(StateFilePath);
                foreach (var line in lines)
                {
                    if (line.StartsWith("last=", StringComparison.OrdinalIgnoreCase))
                    {
                        DateTime dt;
                        if (DateTime.TryParse(line.Substring(5), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out dt))
                            LastLicenseCheckAt = dt.ToLocalTime();
                    }
                    else if (line.StartsWith("online=", StringComparison.OrdinalIgnoreCase))
                    {
                        bool online;
                        if (bool.TryParse(line.Substring(7), out online))
                            IsInternetAvailableForLicense = online;
                    }
                    else if (line.StartsWith("remaining=", StringComparison.OrdinalIgnoreCase))
                    {
                        int rem;
                        if (int.TryParse(line.Substring(10), out rem))
                            OfflineRemainingWriteCount = rem < 0 ? 0 : rem;
                    }
                }
            }
            catch
            {
            }
        }

        private static void SaveState()
        {
            try
            {
                var content =
                    "last=" + LastLicenseCheckAt.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture) + Environment.NewLine +
                    "online=" + IsInternetAvailableForLicense + Environment.NewLine +
                    "remaining=" + OfflineRemainingWriteCount;
                File.WriteAllText(StateFilePath, content);
            }
            catch
            {
            }
        }
    }
}
