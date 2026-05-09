using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Kolera_Mtsk.Services
{
    /// <summary>
    /// Ilk kurulum / kopya sonrasi: SettingsParam, KursBilgiParam.MUSTERI_NO, KOLERA_LISANS varsayilan sablonlari.
    /// </summary>
    public static class LisansIlkKurulumDefaults
    {
        public const string VarsayilanKurumKodu = "1234";
        public const string VarsayilanLisansNo = "ABC-456-XYZ";
        public const string VarsayilanBitis = "2099-12-31";
        public const string VarsayilanMusteriNo = "1234";
        public const string VarsayilanKoleraLisansKurumKodu = "KOLERA_MTSK";

        public static string BuildApplySql()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, @"
IF OBJECT_ID('dbo.KOLERA_LISANS','U') IS NULL
BEGIN
  CREATE TABLE dbo.KOLERA_LISANS(
    ID INT IDENTITY(1,1) PRIMARY KEY,
    LISANS_KURUM_KODU NVARCHAR(300) NULL,
    LISANS_NO NVARCHAR(300) NULL,
    LISANS_BITIS_TARIHI DATETIME NULL,
    PROGRAM_VERSION NVARCHAR(100) NULL,
    OLUSTURMA_TARIHI DATETIME NOT NULL CONSTRAINT DF_KOLERA_LISANS_OLUSTUR DEFAULT(GETDATE())
  );
END

IF OBJECT_ID('dbo.SettingsParam','U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.SettingsParam)
  INSERT INTO dbo.SettingsParam (LSN_KURUM_KODU, LSN_LISANS_NO, LSN_BITIS_TARIHI)
  VALUES (N'{0}', N'{1}', N'{2}');

IF OBJECT_ID('dbo.KursBilgiParam','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.KursBilgiParam','KURUM_KODU') IS NULL
    ALTER TABLE dbo.KursBilgiParam ADD KURUM_KODU VARCHAR(50) NULL;

  IF COL_LENGTH('dbo.KursBilgiParam','MUSTERI_NO') IS NOT NULL
    UPDATE dbo.KursBilgiParam SET MUSTERI_NO = N'{3}' WHERE ISNULL(MUSTERI_NO, N'') = N'';

  UPDATE dbo.KursBilgiParam SET KURUM_KODU = N'{0}' WHERE ISNULL(KURUM_KODU, N'') = N'';
END;

IF OBJECT_ID('dbo.KOLERA_LISANS','U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.KOLERA_LISANS)
  INSERT INTO dbo.KOLERA_LISANS (LISANS_KURUM_KODU, LISANS_NO, LISANS_BITIS_TARIHI)
  VALUES (N'{4}', N'{1}', CAST(N'{2}' AS DATE));
",
                VarsayilanKurumKodu,
                VarsayilanLisansNo,
                VarsayilanBitis,
                VarsayilanMusteriNo,
                VarsayilanKoleraLisansKurumKodu);
        }

        public static void Apply(SqlConnection con)
        {
            if (con == null)
                throw new ArgumentNullException(nameof(con));
            if (con.State != ConnectionState.Open)
                throw new InvalidOperationException("Baglanti acik olmalidir.");

            using (var cmd = new SqlCommand(BuildApplySql(), con))
            {
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
            }
        }

        public static async Task ApplyAsync(SqlConnection con, CancellationToken cancellationToken = default)
        {
            if (con == null)
                throw new ArgumentNullException(nameof(con));
            if (con.State != ConnectionState.Open)
                throw new InvalidOperationException("Baglanti acik olmalidir.");

            using (var cmd = new SqlCommand(BuildApplySql(), con))
            {
                cmd.CommandTimeout = 0;
                await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
