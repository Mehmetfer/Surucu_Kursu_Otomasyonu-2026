using System;
using System.Data.SqlClient;

namespace Kolera_Mtsk.Services
{
    public static class MebbisCredentialResolver
    {
        public static bool TryResolve(string connectionString, string appUserName, out string mebbisUserName, out string mebbisPassword)
        {
            mebbisUserName = string.Empty;
            mebbisPassword = string.Empty;

            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            string genelTable = ResolveGenelParamTableName(connectionString);
            if (string.IsNullOrWhiteSpace(genelTable))
                return false;

            string sql = @"
SELECT TOP (1)
    ISNULL(MEBBIS_KUL_ADI_1,'') AS ADI1, ISNULL(MEBBIS_KUL_SIF_1,'') AS SIF1, ISNULL(MEBBIS_KUL_YET_1,'') AS YET1,
    ISNULL(MEBBIS_KUL_ADI_2,'') AS ADI2, ISNULL(MEBBIS_KUL_SIF_2,'') AS SIF2, ISNULL(MEBBIS_KUL_YET_2,'') AS YET2,
    ISNULL(MEBBIS_KUL_ADI_3,'') AS ADI3, ISNULL(MEBBIS_KUL_SIF_3,'') AS SIF3, ISNULL(MEBBIS_KUL_YET_3,'') AS YET3,
    ISNULL(MEBBIS_KUL_ADI_4,'') AS ADI4, ISNULL(MEBBIS_KUL_SIF_4,'') AS SIF4, ISNULL(MEBBIS_KUL_YET_4,'') AS YET4,
    ISNULL(MEBBIS_KUL_ADI_5,'') AS ADI5, ISNULL(MEBBIS_KUL_SIF_5,'') AS SIF5, ISNULL(MEBBIS_KUL_YET_5,'') AS YET5
FROM [" + genelTable + @"]
ORDER BY ID DESC";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return false;

                        string hedef = (appUserName ?? string.Empty).Trim();
                        for (int i = 1; i <= 5; i++)
                        {
                            string yet = Convert.ToString(reader["YET" + i]).Trim();
                            if (!string.Equals(yet, hedef, StringComparison.OrdinalIgnoreCase))
                                continue;

                            mebbisUserName = Convert.ToString(reader["ADI" + i]).Trim();
                            mebbisPassword = Convert.ToString(reader["SIF" + i]).Trim();
                            if (!string.IsNullOrWhiteSpace(mebbisUserName))
                                return true;
                        }

                        // Fallback: ilk dolu MEBBIS hesabını kullan.
                        for (int i = 1; i <= 5; i++)
                        {
                            string adi = Convert.ToString(reader["ADI" + i]).Trim();
                            if (string.IsNullOrWhiteSpace(adi))
                                continue;

                            mebbisUserName = adi;
                            mebbisPassword = Convert.ToString(reader["SIF" + i]).Trim();
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static string ResolveGenelParamTableName(string connectionString)
        {
            const string sql = @"
SELECT TOP 1 TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND UPPER(TABLE_NAME) IN ('GENELPARAM')
ORDER BY TABLE_NAME;";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var o = cmd.ExecuteScalar();
                return o == null || o == DBNull.Value ? null : Convert.ToString(o);
            }
        }
    }
}
