using System;
using System.Data.SqlClient;

namespace Kolera_Mtsk.Services
{
    public static class AppSession
    {
        public static string CurrentUserName { get; set; }
    }

    public static class AppLogService
    {
        public static void Write(string connectionString, string seviye, string modul, string aciklama)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            try
            {
                EnsureLogTable(connectionString);

                const string sql = @"
INSERT INTO APP_LOG_KAYITLARI(LOG_TARIHI, LOG_SEVIYE, MODUL, KULLANICI_ADI, ACIKLAMA)
VALUES(GETDATE(), @SEVIYE, @MODUL, @KULLANICI, @ACIKLAMA);";

                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SEVIYE", ((seviye ?? "INFO").Trim().ToUpperInvariant()));
                    cmd.Parameters.AddWithValue("@MODUL", (object)(modul ?? string.Empty).Trim());
                    cmd.Parameters.AddWithValue("@KULLANICI", (object)(AppSession.CurrentUserName ?? string.Empty).Trim());
                    cmd.Parameters.AddWithValue("@ACIKLAMA", (object)(aciklama ?? string.Empty).Trim());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Log yazımı uygulama akışını kesmemeli.
            }
        }

        private static void EnsureLogTable(string connectionString)
        {
            const string sql = @"
IF OBJECT_ID('dbo.APP_LOG_KAYITLARI','U') IS NULL
BEGIN
    CREATE TABLE dbo.APP_LOG_KAYITLARI
    (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        LOG_TARIHI DATETIME NOT NULL DEFAULT(GETDATE()),
        LOG_SEVIYE VARCHAR(20) NOT NULL,
        MODUL VARCHAR(100) NULL,
        KULLANICI_ADI VARCHAR(100) NULL,
        ACIKLAMA NVARCHAR(1000) NULL
    );
END;";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
