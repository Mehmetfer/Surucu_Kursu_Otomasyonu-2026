using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Kolera_MTSK.Login;
using Kolera_MTSK.LoginL;

namespace Kolera_Mtsk.Services
{
    /// <summary>
    /// Acilista web/txt lisans modeline gore KursBilgiParam.MUSTERI_NO, SettingsParam lisans alanlari ve KOLERA_LISANS guncellenir.
    /// KOLERA_LISANS.OLUSTURMA_TARIHI dokunulmaz.
    /// </summary>
    public static class LisansDbStartupSync
    {
        public static async Task ApplyIfPossibleAsync(string connectionString, LisansModel lisans, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(connectionString) || lisans == null)
                return;

            string musteriNo = (lisans.MusteriNo ?? string.Empty).Trim();
            string kurumKodu = !string.IsNullOrWhiteSpace(musteriNo)
                ? musteriNo
                : string.Empty;

            string lisansNo = (lisans.LisansNo ?? string.Empty).Trim();
            string versiyon = string.IsNullOrWhiteSpace(lisans.Versiyon)
                ? VersionService.GetVersion()
                : lisans.Versiyon.Trim();

            string bitisStr = lisans.ValidUntil != DateTime.MinValue
                ? lisans.ValidUntil.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : string.Empty;

            if (string.IsNullOrWhiteSpace(kurumKodu) && string.IsNullOrWhiteSpace(lisansNo)
                && string.IsNullOrWhiteSpace(bitisStr) && string.IsNullOrWhiteSpace(versiyon))
                return;

            using (var con = new SqlConnection(connectionString))
            {
                await con.OpenAsync(cancellationToken).ConfigureAwait(false);
                await ApplyKursBilgiIdentityAsync(con, musteriNo, kurumKodu, cancellationToken).ConfigureAwait(false);
                await ApplySettingsParamAsync(con, kurumKodu, lisansNo, bitisStr, cancellationToken).ConfigureAwait(false);
                await ApplyKoleraLisansAsync(con, kurumKodu, lisansNo, lisans.ValidUntil, versiyon, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task ApplyKursBilgiIdentityAsync(SqlConnection con, string musteriNo, string kurumKodu, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(musteriNo) && string.IsNullOrWhiteSpace(kurumKodu))
                return;

            const string sql = @"
IF OBJECT_ID('dbo.KursBilgiParam','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.KursBilgiParam','MUSTERI_NO') IS NOT NULL
    UPDATE dbo.KursBilgiParam SET MUSTERI_NO = CASE WHEN LEN(ISNULL(@m, N'')) > 0 THEN @m ELSE MUSTERI_NO END;
END";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@m", SqlDbType.NVarChar, 300).Value = ToParam(musteriNo);
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        private static async Task ApplySettingsParamAsync(SqlConnection con, string kurumKodu, string lisansNo, string bitisStr, CancellationToken ct)
        {
            if (!await TableExistsAsync(con, "SettingsParam", ct).ConfigureAwait(false))
                return;

            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.SettingsParam)
BEGIN
  UPDATE dbo.SettingsParam SET
    LSN_KURUM_KODU = CASE WHEN LEN(ISNULL(@kurum, N'')) > 0 THEN @kurum ELSE LSN_KURUM_KODU END,
    LSN_LISANS_NO = CASE WHEN LEN(ISNULL(@lisans, N'')) > 0 THEN @lisans ELSE LSN_LISANS_NO END,
    LSN_BITIS_TARIHI = CASE WHEN LEN(ISNULL(@bitis, N'')) > 0 THEN @bitis ELSE LSN_BITIS_TARIHI END
  WHERE ID = (SELECT TOP 1 ID FROM dbo.SettingsParam ORDER BY ID DESC);
END
ELSE IF LEN(ISNULL(@kurum, N'')) > 0 OR LEN(ISNULL(@lisans, N'')) > 0 OR LEN(ISNULL(@bitis, N'')) > 0
  INSERT INTO dbo.SettingsParam (LSN_KURUM_KODU, LSN_LISANS_NO, LSN_BITIS_TARIHI)
  VALUES (@kurum, @lisans, NULLIF(@bitis, N''));";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@kurum", SqlDbType.NVarChar, 300).Value = ToParam(kurumKodu);
                cmd.Parameters.Add("@lisans", SqlDbType.NVarChar, 300).Value = ToParam(lisansNo);
                cmd.Parameters.Add("@bitis", SqlDbType.NVarChar, 10).Value = ToParam(bitisStr);
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        private static async Task ApplyKoleraLisansAsync(SqlConnection con, string kurumKodu, string lisansNo, DateTime bitis, string versiyon, CancellationToken ct)
        {
            if (!await TableExistsAsync(con, "KOLERA_LISANS", ct).ConfigureAwait(false))
                return;

            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.KOLERA_LISANS)
BEGIN
  INSERT INTO dbo.KOLERA_LISANS (LISANS_KURUM_KODU, LISANS_NO, LISANS_BITIS_TARIHI, PROGRAM_VERSION)
  VALUES (@kurum, @no, @bitis, @version);
END
ELSE
BEGIN
  UPDATE dbo.KOLERA_LISANS SET
    LISANS_KURUM_KODU = CASE WHEN LEN(ISNULL(@kurum, N'')) > 0 THEN @kurum ELSE LISANS_KURUM_KODU END,
    LISANS_NO = CASE WHEN LEN(ISNULL(@no, N'')) > 0 THEN @no ELSE LISANS_NO END,
    LISANS_BITIS_TARIHI = CASE WHEN @bitis IS NOT NULL THEN @bitis ELSE LISANS_BITIS_TARIHI END,
    PROGRAM_VERSION = CASE WHEN LEN(ISNULL(@version, N'')) > 0 THEN @version ELSE PROGRAM_VERSION END;
END";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@kurum", SqlDbType.NVarChar, 300).Value = ToParam(kurumKodu);
                cmd.Parameters.Add("@no", SqlDbType.NVarChar, 300).Value = ToParam(lisansNo);
                var pBitis = cmd.Parameters.Add("@bitis", SqlDbType.DateTime);
                if (bitis == DateTime.MinValue)
                    pBitis.Value = DBNull.Value;
                else
                    pBitis.Value = bitis.Date;
                cmd.Parameters.Add("@version", SqlDbType.NVarChar, 100).Value = ToParam(versiyon);
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
        }

        private static object ToParam(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();
        }

        private static async Task<bool> TableExistsAsync(SqlConnection con, string tableName, CancellationToken ct)
        {
            const string sql = @"
SELECT CASE WHEN EXISTS (
  SELECT 1 FROM INFORMATION_SCHEMA.TABLES
  WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = @t AND TABLE_TYPE = 'BASE TABLE'
) THEN 1 ELSE 0 END;";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@t", SqlDbType.NVarChar, 128).Value = tableName;
                var o = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
                return o != null && o != DBNull.Value && Convert.ToInt32(o) == 1;
            }
        }
    }
}
