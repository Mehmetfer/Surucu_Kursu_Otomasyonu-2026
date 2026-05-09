using FastReport;
using FastReport.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace Kolera_Mtsk.Sayfalar
{
    /// <summary>
    /// KURS veri kaynagi: FRX icindeki [KURS.*] ifadeleri.
    /// RegisterData sonrasi (mumkunse) IgnoreConnection: bazi FR surumlerinde yalnizca yansima ile erisilebilir;
    /// aksi halde ifadeler derlenirken CS0103 ('KURS' tanimli degil) olusabiliyor.
    /// </summary>
    internal static class KursRaporKursTablosu
    {
        private static readonly string[] KursTabloAdaylari =
        {
            "dbo.KursBilgiParam", "KursBilgiParam", "dbo.PARAM_KURSBILGILERI", "PARAM_KURSBILGILERI"
        };

        public static DataTable Olustur(string connectionString)
        {
            DataTable dt = null;
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                foreach (string tableName in KursTabloAdaylari)
                {
                    if (!TabloVarMi(connectionString, tableName))
                        continue;
                    try
                    {
                        var tryDt = new DataTable("KURS");
                        using (var conn = new SqlConnection(connectionString))
                        using (var cmd = new SqlCommand("SELECT TOP 1 * FROM [" + tableName + "]", conn))
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(tryDt);
                        }

                        tryDt.TableName = "KURS";
                        if (tryDt.Columns.Count > 0)
                        {
                            dt = tryDt;
                            break;
                        }
                    }
                    catch
                    {
                        // sonraki tablo
                    }
                }
            }

            if (dt == null)
                dt = new DataTable("KURS");

            RaporIcinEksikKursKolonlariEkle(dt);

            if (dt.Rows.Count == 0)
                dt.Rows.Add(dt.NewRow());

            DataRow row = dt.Rows[0];
            foreach (DataColumn col in dt.Columns)
            {
                if (col.DataType == typeof(string) && row[col] == DBNull.Value)
                    row[col] = string.Empty;
            }

            if (dt.Columns.Contains("KURS_ADI"))
            {
                string kursAdi = Convert.ToString(row["KURS_ADI"]).Trim();
                if (string.IsNullOrWhiteSpace(kursAdi))
                    row["KURS_ADI"] = "KOLERA MTSK";
            }

            string raporTarih = DateTime.Now.ToString("dd.MM.yyyy");
            if (dt.Columns.Contains("RAPOR_TARIHI"))
                row["RAPOR_TARIHI"] = raporTarih;
            if (dt.Columns.Contains("KURS ADI") && dt.Columns.Contains("KURS_ADI"))
                row["KURS ADI"] = row["KURS_ADI"];
            if (dt.Columns.Contains("MUDUR ADI") && dt.Columns.Contains("MUDUR_ADI"))
                row["MUDUR ADI"] = row["MUDUR_ADI"] == DBNull.Value ? string.Empty : row["MUDUR_ADI"];
            if (dt.Columns.Contains("RAPOR TARIHI"))
                row["RAPOR TARIHI"] = raporTarih;

            return dt;
        }

        public static void ProgramatikTablolardaBaglantiyiYoksay(Report report)
        {
            if (report?.Dictionary?.DataSources == null)
                return;
            foreach (DataSourceBase ds in report.Dictionary.DataSources)
                BaglantiYoksaymayiYansimaylaDene(ds);
        }

        private static void BaglantiYoksaymayiYansimaylaDene(object hedef)
        {
            if (hedef == null)
                return;
            const BindingFlags bayraklar = BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo ozellik = hedef.GetType().GetProperty("IgnoreConnection", bayraklar);
            if (ozellik == null || !ozellik.CanWrite || ozellik.PropertyType != typeof(bool))
                return;
            try
            {
                ozellik.SetValue(hedef, true, null);
            }
            catch
            {
            }
        }

        private static void RaporIcinEksikKursKolonlariEkle(DataTable dt)
        {
            void Col(string name, Type t)
            {
                if (!dt.Columns.Contains(name))
                    dt.Columns.Add(name, t);
            }

            Col("KURS_ADI", typeof(string));
            Col("MUDUR_ADI", typeof(string));
            Col("ADRES", typeof(string));
            Col("IL", typeof(string));
            Col("ILCE", typeof(string));
            Col("TELEFON", typeof(string));
            Col("GSM", typeof(string));
            Col("KURS_ADI_KISA", typeof(string));
            Col("SOZLESME_BANKA_HESAPNO", typeof(string));
            Col("KURUCU_ADI", typeof(string));
            Col("E_POSTA", typeof(string));
            Col("WEB", typeof(string));
            Col("PK", typeof(string));
            Col("MUSTERI_NO", typeof(string));
            Col("RAPOR_TARIHI", typeof(string));
            Col("KURS ADI", typeof(string));
            Col("MUDUR ADI", typeof(string));
            Col("RAPOR TARIHI", typeof(string));
        }

        private static bool TabloVarMi(string connectionString, string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            string onlyTableName = tableName;
            if (tableName.Contains("."))
                onlyTableName = tableName.Substring(tableName.LastIndexOf('.') + 1).Trim('[', ']');

            const string sql = @"
SELECT TOP 1 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND TABLE_NAME=@T;";
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@T", SqlDbType.NVarChar, 128).Value = onlyTableName;
                    conn.Open();
                    object o = cmd.ExecuteScalar();
                    return o != null && o != DBNull.Value;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
