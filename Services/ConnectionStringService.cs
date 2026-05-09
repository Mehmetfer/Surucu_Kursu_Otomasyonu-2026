
using System;
using Kolera_MTSK.Login;
namespace Kolera_Mtsk.Services
{
    public class ConnectionStringService
    {
        public string Build(ServerAyarModel ayar)
        {
            if (ayar == null)
                throw new ArgumentNullException(nameof(ayar));

            if (string.IsNullOrWhiteSpace(ayar.BaglantiTuru))
                throw new ArgumentException("Bağlantı türü boş olamaz.");

            // AttachDbFilename (LocalDB)
            if (ayar.BaglantiTuru.Equals("AttachDbFilename",
                StringComparison.OrdinalIgnoreCase))
            {
                return
                    "Data Source=(LocalDB)\\MSSQLLocalDB;" +
                    $"AttachDbFilename={ayar.VeritabaniAdi};" +
                    "Integrated Security=True;" +
                    "TrustServerCertificate=True;";
            }

            // Windows Authentication
            return
                $"Server={ayar.Sunucu};" +
                $"Database={ayar.VeritabaniAdi};" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;";
        }
    }
}
