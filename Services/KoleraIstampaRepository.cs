using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace Kolera_Mtsk.Services
{
    internal sealed class KoleraIstampaRepository
    {
        public const string KodImza = "IMZA";
        public const string KodMuhur = "MUHUR";
        public const string KodKase = "KASE";
        public const string KodAsliGibidir = "ASLI_GIBIDIR";
        public const string KodIncelendi = "INCELENDI";

        private readonly string _connectionString;

        public KoleraIstampaRepository(string connectionString)
        {
            _connectionString = connectionString ?? string.Empty;
        }

        public DataTable GetAll()
        {
            var dt = new DataTable("KOLERA_ISTAMPA")
            {
                Locale = CultureInfo.InvariantCulture
            };

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                EnsureColumns(dt);
                EnsureDefaultRows(dt);
                return dt;
            }

            const string sql = @"
IF OBJECT_ID('dbo.KOLERA_ISTAMPA','U') IS NULL
BEGIN
    SELECT CAST(NULL AS INT) AS ID, CAST(NULL AS NVARCHAR(50)) AS ALAN_KODU, CAST(NULL AS NVARCHAR(100)) AS ALAN_ADI,
           CAST(NULL AS VARBINARY(MAX)) AS RESIM, CAST(NULL AS NVARCHAR(250)) AS ACIKLAMA, CAST(NULL AS DATETIME) AS GUNCELLEME_TARIHI
    WHERE 1 = 0;
END
ELSE
BEGIN
    SELECT ID, ALAN_KODU, ALAN_ADI, RESIM, ACIKLAMA, GUNCELLEME_TARIHI
    FROM dbo.KOLERA_ISTAMPA
    ORDER BY ID;
END";

            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(sql, con))
                {
                    da.Fill(dt);
                }
            }
            catch
            {
                // sessiz gec
            }

            EnsureColumns(dt);
            EnsureDefaultRows(dt);
            return dt;
        }

        public byte[] GetImageByCode(string alanKodu)
        {
            string kod = NormalizeCode(alanKodu);
            if (string.IsNullOrWhiteSpace(kod) || string.IsNullOrWhiteSpace(_connectionString))
                return null;

            const string sql = @"
IF OBJECT_ID('dbo.KOLERA_ISTAMPA','U') IS NULL
    SELECT CAST(NULL AS VARBINARY(MAX)) AS RESIM WHERE 1 = 0;
ELSE
    SELECT TOP 1 RESIM
    FROM dbo.KOLERA_ISTAMPA
    WHERE UPPER(ISNULL(ALAN_KODU,'')) = @KOD;";

            try
            {
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@KOD", SqlDbType.NVarChar, 50).Value = kod;
                    con.Open();
                    object o = cmd.ExecuteScalar();
                    var raw = (o == null || o == DBNull.Value) ? null : (byte[])o;
                    return NormalizeStampImage(kod, raw);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <returns>true ise KOLERA_ISTAMPA tablosuna yazildi.</returns>
        public bool SaveImage(string alanKodu, string alanAdi, byte[] resim, string aciklama, out string hataMesaji)
        {
            hataMesaji = null;
            string kod = NormalizeCode(alanKodu);
            if (string.IsNullOrWhiteSpace(kod))
            {
                hataMesaji = "Alan kodu bos.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                hataMesaji = "Veritabani baglanti satiri tanimli degil.";
                return false;
            }

            try
            {
                DatabaseSchemaMigration.EnsureKoleraIstampaTable(_connectionString);
            }
            catch (Exception ex)
            {
                hataMesaji = "KOLERA_ISTAMPA tablosu hazirlanamadi: " + ex.Message;
                return false;
            }

            const string sql = @"
IF EXISTS (SELECT 1 FROM dbo.KOLERA_ISTAMPA WHERE UPPER(ISNULL(ALAN_KODU,''))=@KOD)
BEGIN
    UPDATE dbo.KOLERA_ISTAMPA
    SET ALAN_ADI = @ADI,
        RESIM = @RESIM,
        ACIKLAMA = @ACIKLAMA,
        GUNCELLEME_TARIHI = GETDATE()
    WHERE UPPER(ISNULL(ALAN_KODU,''))=@KOD;
END
ELSE
BEGIN
    INSERT INTO dbo.KOLERA_ISTAMPA(ALAN_KODU, ALAN_ADI, RESIM, ACIKLAMA, GUNCELLEME_TARIHI)
    VALUES(@KOD, @ADI, @RESIM, @ACIKLAMA, GETDATE());
END";

            try
            {
                byte[] normalizedImage = NormalizeStampImage(kod, resim);
                using (var con = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.Add("@KOD", SqlDbType.NVarChar, 50).Value = kod;
                    cmd.Parameters.Add("@ADI", SqlDbType.NVarChar, 100).Value = (object)(alanAdi ?? string.Empty) ?? DBNull.Value;
                    cmd.Parameters.Add("@RESIM", SqlDbType.VarBinary, -1).Value = (object)normalizedImage ?? DBNull.Value;
                    cmd.Parameters.Add("@ACIKLAMA", SqlDbType.NVarChar, 250).Value = (object)(aciklama ?? string.Empty) ?? DBNull.Value;
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                hataMesaji = ex.Message;
                return false;
            }
        }

        private static void EnsureColumns(DataTable dt)
        {
            if (dt == null)
                return;

            if (FindColumnOrdinal(dt, "ID") < 0) dt.Columns.Add("ID", typeof(int));
            if (FindColumnOrdinal(dt, "ALAN_KODU") < 0) dt.Columns.Add("ALAN_KODU", typeof(string));
            if (FindColumnOrdinal(dt, "ALAN_ADI") < 0) dt.Columns.Add("ALAN_ADI", typeof(string));
            if (FindColumnOrdinal(dt, "RESIM") < 0) dt.Columns.Add("RESIM", typeof(byte[]));
            if (FindColumnOrdinal(dt, "ACIKLAMA") < 0) dt.Columns.Add("ACIKLAMA", typeof(string));
            if (FindColumnOrdinal(dt, "GUNCELLEME_TARIHI") < 0) dt.Columns.Add("GUNCELLEME_TARIHI", typeof(DateTime));
        }

        /// <summary>
        /// Turkce yerel ayarda DataRow[string] ACIKLAMA / I harfi iceren kolon adlarinda hata verebilir; ordinal okuma kullanin.
        /// </summary>
        internal static int FindColumnOrdinal(DataTable table, string columnName)
        {
            if (table == null || string.IsNullOrEmpty(columnName))
                return -1;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (string.Equals(table.Columns[i].ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        internal static object GetColumnValue(DataRow row, string columnName)
        {
            if (row == null)
                return DBNull.Value;
            int ord = FindColumnOrdinal(row.Table, columnName);
            if (ord < 0)
                return DBNull.Value;
            return row[ord];
        }

        internal static bool TryFindStampRow(DataTable dt, string alanKodu, out DataRow row)
        {
            row = null;
            if (dt == null)
                return false;
            string kod = NormalizeCode(alanKodu);
            if (string.IsNullOrEmpty(kod))
                return false;
            foreach (DataRow r in dt.Rows)
            {
                if (r.RowState == DataRowState.Deleted)
                    continue;
                string k = NormalizeCode(Convert.ToString(GetColumnValue(r, "ALAN_KODU")));
                if (string.Equals(k, kod, StringComparison.Ordinal))
                {
                    row = r;
                    return true;
                }
            }
            return false;
        }

        private static void EnsureDefaultRows(DataTable dt)
        {
            var defs = GetDefaultDefinitions();
            foreach (var def in defs)
            {
                bool exists = false;
                foreach (DataRow row in dt.Rows)
                {
                    if (row.RowState == DataRowState.Deleted)
                        continue;
                    if (string.Equals(NormalizeCode(Convert.ToString(GetColumnValue(row, "ALAN_KODU"))), def.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        int adOrd = FindColumnOrdinal(dt, "ALAN_ADI");
                        if (adOrd >= 0 && string.IsNullOrWhiteSpace(Convert.ToString(row[adOrd])))
                            row[adOrd] = def.Value;
                        break;
                    }
                }

                if (!exists)
                {
                    DataRow n = dt.NewRow();
                    SetCell(n, "ID", 0);
                    SetCell(n, "ALAN_KODU", def.Key);
                    SetCell(n, "ALAN_ADI", def.Value);
                    SetCell(n, "ACIKLAMA", string.Empty);
                    dt.Rows.Add(n);
                }
            }
        }

        private static void SetCell(DataRow row, string columnName, object value)
        {
            int ord = FindColumnOrdinal(row.Table, columnName);
            if (ord >= 0)
                row[ord] = value ?? DBNull.Value;
        }

        public static Dictionary<string, string> GetDefaultDefinitions()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { KodImza, "Imza" },
                { KodMuhur, "Muhur" },
                { KodKase, "Kase" },
                { KodAsliGibidir, "Asli Gibidir" },
                { KodIncelendi, "Incelendi" }
            };
        }

        private static string NormalizeCode(string code)
        {
            return (code ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static bool IsStampCode(string kod)
        {
            if (string.IsNullOrWhiteSpace(kod))
                return false;
            return string.Equals(kod, KodImza, StringComparison.OrdinalIgnoreCase)
                || string.Equals(kod, KodMuhur, StringComparison.OrdinalIgnoreCase)
                || string.Equals(kod, KodKase, StringComparison.OrdinalIgnoreCase)
                || string.Equals(kod, KodAsliGibidir, StringComparison.OrdinalIgnoreCase)
                || string.Equals(kod, KodIncelendi, StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] NormalizeStampImage(string kod, byte[] data)
        {
            if (!IsStampCode(kod) || data == null || data.Length == 0)
                return data;

            try
            {
                using (var ms = new MemoryStream(data))
                using (var src = new Bitmap(ms))
                {
                    using (var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb))
                    {
                        for (int y = 0; y < src.Height; y++)
                        {
                            for (int x = 0; x < src.Width; x++)
                            {
                                Color c = src.GetPixel(x, y);
                                bool nearWhite = c.R >= 245 && c.G >= 245 && c.B >= 245;
                                bool transparent = c.A < 10 || nearWhite;
                                dst.SetPixel(x, y, transparent ? Color.Transparent : Color.FromArgb(255, c.R, c.G, c.B));
                            }
                        }

                        using (var outMs = new MemoryStream())
                        {
                            dst.Save(outMs, ImageFormat.Png);
                            return outMs.ToArray();
                        }
                    }
                }
            }
            catch
            {
                return data;
            }
        }
    }
}
