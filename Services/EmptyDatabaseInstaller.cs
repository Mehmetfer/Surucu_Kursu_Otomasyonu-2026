using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kolera_Mtsk.Services
{
    internal sealed class EmptyDatabaseInstaller
    {
        private readonly string _masterConnectionString;
        private readonly string _targetDatabaseName;
        private readonly Action<string> _log;

        public event Action<int, int> ProgressChanged;

        public EmptyDatabaseInstaller(string masterConnectionString, string targetDatabaseName, Action<string> log)
        {
            _masterConnectionString = masterConnectionString ?? throw new ArgumentNullException(nameof(masterConnectionString));
            _targetDatabaseName = targetDatabaseName ?? throw new ArgumentNullException(nameof(targetDatabaseName));
            _log = log ?? (_ => { });
        }

        public async Task InstallAsync()
        {
            await Task.Run(EnsureDatabaseExists);

            var scripts = GetSchemaScripts();
            int total = scripts.Count;
            int done = 0;
            ProgressChanged?.Invoke(done, total);

            foreach (var script in scripts)
            {
                _log("Kuruluyor: " + script.Name);
                await Task.Run(() => ExecuteTarget(script.Sql));
                done++;
                ProgressChanged?.Invoke(done, total);
                _log("Tamam: " + script.Name);
            }

            await Task.Run(ApplyDefaultReportTemplates);
        }

        private void EnsureDatabaseExists()
        {
            using (var con = new SqlConnection(_masterConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("IF DB_ID(@db) IS NULL EXEC('CREATE DATABASE [' + @db + ']');", con))
                {
                    cmd.Parameters.Add("@db", SqlDbType.NVarChar, 128).Value = _targetDatabaseName;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string BuildTargetConnectionString()
        {
            var sb = new SqlConnectionStringBuilder(_masterConnectionString)
            {
                InitialCatalog = _targetDatabaseName
            };
            return sb.ConnectionString;
        }

        private void ExecuteTarget(string sql)
        {
            using (var con = new SqlConnection(BuildTargetConnectionString()))
            {
                con.Open();
                if (TryHandleProcedureScript(con, sql))
                    return;

                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.CommandTimeout = 0;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool TryHandleProcedureScript(SqlConnection con, string sql)
        {
            string trimmed = (sql ?? string.Empty).TrimStart();
            bool isCreateOrAlter = trimmed.StartsWith("CREATE OR ALTER PROCEDURE", StringComparison.OrdinalIgnoreCase);
            bool isCreateOnly = trimmed.StartsWith("CREATE PROCEDURE", StringComparison.OrdinalIgnoreCase);
            if (!isCreateOrAlter && !isCreateOnly)
                return false;

            var nameMatch = Regex.Match(trimmed, @"^CREATE(\s+OR\s+ALTER)?\s+PROCEDURE\s+([^\s(]+)", RegexOptions.IgnoreCase);
            if (!nameMatch.Success)
                return false;

            string procName = nameMatch.Groups[2].Value;
            string createSql = isCreateOrAlter
                ? Regex.Replace(trimmed, @"^CREATE\s+OR\s+ALTER\s+PROCEDURE", "CREATE PROCEDURE", RegexOptions.IgnoreCase)
                : trimmed;

            using (var dropCmd = new SqlCommand("IF OBJECT_ID(@name, 'P') IS NOT NULL EXEC('DROP PROCEDURE " + procName + "')", con))
            {
                dropCmd.Parameters.Add("@name", SqlDbType.NVarChar, 256).Value = procName;
                dropCmd.CommandTimeout = 0;
                dropCmd.ExecuteNonQuery();
            }

            using (var createCmd = new SqlCommand(createSql, con))
            {
                createCmd.CommandTimeout = 0;
                createCmd.ExecuteNonQuery();
            }

            return true;
        }

        private void ApplyDefaultReportTemplates()
        {
            try
            {
                var kayitlar = new List<ReportSeedRow>();
                using (var con = new SqlConnection(BuildTargetConnectionString()))
                using (var cmd = new SqlCommand(@"
SELECT ID, ISNULL(RAPOR_GRUBU,''), ISNULL(RAPOR_ADI,''), ISNULL(RAPOR_YOLU,'')
FROM dbo.RAPOR_TANIMLARI;", con))
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            kayitlar.Add(new ReportSeedRow
                            {
                                Id = Convert.ToInt32(reader.GetValue(0)),
                                RaporGrubu = Convert.ToString(reader.GetValue(1)) ?? string.Empty,
                                RaporAdi = Convert.ToString(reader.GetValue(2)) ?? string.Empty,
                                RaporYolu = Convert.ToString(reader.GetValue(3)) ?? string.Empty
                            });
                        }
                    }
                }

                if (kayitlar.Count == 0)
                    return;

                int guncellenen = 0;
                using (var con = new SqlConnection(BuildTargetConnectionString()))
                using (var cmd = new SqlCommand(@"
UPDATE dbo.RAPOR_TANIMLARI
SET
    RAPOR_YOLU = @YOL,
    SABLON_BINARY = @BIN,
    GUNCELLEME_TARIHI = GETDATE()
WHERE ID = @ID;", con))
                {
                    cmd.Parameters.Add("@YOL", SqlDbType.NVarChar, 1000);
                    cmd.Parameters.Add("@BIN", SqlDbType.VarBinary, -1);
                    cmd.Parameters.Add("@ID", SqlDbType.Int);

                    con.Open();
                    foreach (var kayit in kayitlar)
                    {
                        string normalizedPath = NormalizeReportPath(kayit.RaporYolu, kayit.RaporAdi);
                        byte[] binary = TryLoadTemplateBinary(normalizedPath, kayit.RaporAdi);
                        if (binary == null || binary.Length == 0)
                            binary = BuildDefaultFrxBinary(kayit.RaporAdi, kayit.RaporGrubu);

                        cmd.Parameters["@YOL"].Value = normalizedPath;
                        cmd.Parameters["@BIN"].Value = (object)binary ?? DBNull.Value;
                        cmd.Parameters["@ID"].Value = kayit.Id;
                        guncellenen += cmd.ExecuteNonQuery();
                    }
                }

                _log("Rapor sablon binary guncellenen kayit: " + guncellenen);
            }
            catch (Exception ex)
            {
                _log("Sablon binary yazma hatasi: " + ex.Message);
            }
        }

        private static byte[] TryLoadTemplateBinary(string normalizedPath, string raporAdi)
        {
            string fileName = Path.GetFileName(normalizedPath);
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = BuildSafeFileName(raporAdi);

            string[] adaylar =
            {
                normalizedPath,
                Path.Combine(@"C:\Raporlar", fileName),
                Path.Combine(@"D:\Kolera_Mtsk\Kolera_Mtsk\Raporlar", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Raporlar", fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName)
            };

            foreach (var yol in adaylar)
            {
                if (File.Exists(yol))
                {
                    try
                    {
                        byte[] data = File.ReadAllBytes(yol);
                        if (data != null && data.Length > 0)
                            return data;
                    }
                    catch
                    {
                        // sonraki yol denenecek
                    }
                }
            }

            return null;
        }

        private static string NormalizeReportPath(string raporYolu, string raporAdi)
        {
            string yol = (raporYolu ?? string.Empty).Trim();
            string fileName = string.Empty;
            try
            {
                fileName = Path.GetFileName(yol);
            }
            catch
            {
                fileName = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".frx", StringComparison.OrdinalIgnoreCase))
                fileName = BuildSafeFileName(raporAdi);

            return Path.Combine(@"C:\Raporlar", fileName);
        }

        private static string BuildSafeFileName(string raporAdi)
        {
            string name = (raporAdi ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                name = "Rapor";

            foreach (char ch in Path.GetInvalidFileNameChars())
                name = name.Replace(ch, '_');

            name = name.Replace(" ", "_");
            if (!name.EndsWith(".frx", StringComparison.OrdinalIgnoreCase))
                name += ".frx";
            return name;
        }

        private static byte[] BuildDefaultFrxBinary(string raporAdi, string raporGrubu)
        {
            string title = EscapeXml(string.IsNullOrWhiteSpace(raporAdi) ? "Rapor" : raporAdi);
            string group = EscapeXml(string.IsNullOrWhiteSpace(raporGrubu) ? "Rapor Grubu" : raporGrubu);
            string grupMesaji = EscapeXml(GetGroupDefaultMessage(raporGrubu));

            string frx = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n"
                + "<Report ScriptLanguage=\"CSharp\" ReportInfo.Created=\"05/01/2026 12:00:00\" ReportInfo.Modified=\"05/01/2026 12:00:00\" ReportInfo.CreatorVersion=\"2026.1.0.0\">\r\n"
                + "  <Dictionary/>\r\n"
                + "  <ReportPage Name=\"Page1\" LeftMargin=\"10\" TopMargin=\"10\" RightMargin=\"10\" BottomMargin=\"10\" Watermark.Font=\"Arial, 60pt\">\r\n"
                + "    <ReportTitleBand Name=\"ReportTitle1\" Width=\"718.2\" Height=\"220.5\">\r\n"
                + "      <TextObject Name=\"TextTitle\" Top=\"28\" Width=\"718.2\" Height=\"37.8\" Text=\"" + title + "\" HorzAlign=\"Center\" Font=\"Arial, 18pt, style=Bold\"/>\r\n"
                + "      <TextObject Name=\"TextGroup\" Top=\"75.6\" Width=\"718.2\" Height=\"18.9\" Text=\"" + group + "\" HorzAlign=\"Center\" Font=\"Arial, 10pt, style=Bold\"/>\r\n"
                + "      <TextObject Name=\"TextDate\" Top=\"113.4\" Width=\"718.2\" Height=\"18.9\" Text=\"Tarih: [Date]\" HorzAlign=\"Center\" Font=\"Arial, 10pt\"/>\r\n"
                + "      <TextObject Name=\"TextType\" Top=\"136.08\" Width=\"718.2\" Height=\"18.9\" Text=\"" + grupMesaji + "\" HorzAlign=\"Center\" Font=\"Arial, 10pt, style=Bold\"/>\r\n"
                + "      <TextObject Name=\"TextInfo\" Top=\"160.65\" Width=\"718.2\" Height=\"37.8\" Text=\"Bu rapor varsayilan olarak otomatik olusturulmustur. Isterseniz FastReport Designer ile duzenleyebilirsiniz.\" HorzAlign=\"Center\" Font=\"Arial, 9pt\"/>\r\n"
                + "    </ReportTitleBand>\r\n"
                + "  </ReportPage>\r\n"
                + "</Report>";

            return Encoding.UTF8.GetBytes(frx);
        }

        private static string GetGroupDefaultMessage(string raporGrubu)
        {
            string g = (raporGrubu ?? string.Empty).ToUpperInvariant();
            if (g.Contains("PERSONEL"))
                return "PERSONEL RAPOR TASLAĞI";
            if (g.Contains("KURSIYER"))
                return "KURSIYER RAPOR TASLAĞI";
            if (g.Contains("SINAV"))
                return "SINAV RAPOR TASLAĞI";
            if (g.Contains("BORC"))
                return "BORÇ ÖDEME RAPOR TASLAĞI";
            if (g.Contains("SERTIFIKA"))
                return "SERTİFİKA RAPOR TASLAĞI";
            return "GENEL RAPOR TASLAĞI";
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                        .Replace("\"", "&quot;")
                        .Replace("'", "&apos;");
        }

        private List<ScriptItem> GetSchemaScripts()
        {
            return new List<ScriptItem>
            {
                new ScriptItem("KULLANICI tablosu", @"IF OBJECT_ID('dbo.KULLANICI','U') IS NULL
CREATE TABLE dbo.KULLANICI(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  KULLANICI_ADI VARCHAR(100) NULL,
  KULLANICI_SIFRE VARCHAR(100) NULL,
  KAYIT_TARIHI DATETIME NULL,
  YETKI VARCHAR(100) NULL
);"),
                new ScriptItem("KULLANICI sade kolonlar", @"
IF OBJECT_ID('dbo.KULLANICI','U') IS NOT NULL
BEGIN
    DECLARE @dropSql NVARCHAR(MAX) = N'';
    SELECT @dropSql = @dropSql +
        N'ALTER TABLE dbo.KULLANICI DROP COLUMN [' + c.name + N'];'
    FROM sys.columns c
    WHERE c.object_id = OBJECT_ID('dbo.KULLANICI')
      AND c.name NOT IN ('ID','KULLANICI_ADI','KULLANICI_SIFRE','KAYIT_TARIHI','YETKI')
      AND c.is_identity = 0
      AND c.is_computed = 0;

    IF LEN(@dropSql) > 0
        EXEC sp_executesql @dropSql;
END
"),
                new ScriptItem("KULLANICI varsayilan kayit", @"
IF OBJECT_ID('dbo.KULLANICI','U') IS NOT NULL
BEGIN
    DECLARE @DefaultUserName VARCHAR(100) = LEFT(DB_NAME(), 100);
    IF NOT EXISTS (SELECT 1 FROM dbo.KULLANICI WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@DefaultUserName))
    BEGIN
        INSERT INTO dbo.KULLANICI (KULLANICI_ADI, KULLANICI_SIFRE, KAYIT_TARIHI, YETKI)
        VALUES (@DefaultUserName, 'Admin', GETDATE(), 'ADMIN');
    END
END
"),

                new ScriptItem("KURSIYER tablosu", @"IF OBJECT_ID('dbo.KURSIYER','U') IS NULL
CREATE TABLE dbo.KURSIYER(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  ADI NVARCHAR(200) NULL,
  SOYADI NVARCHAR(200) NULL,
  TC_NO VARCHAR(11) NULL,
  GSM_1 NVARCHAR(40) NULL,
  GSM_2 NVARCHAR(40) NULL,
  KIMLIK_BABA_ADI NVARCHAR(200) NULL,
  KIM_ANA_ADI NVARCHAR(200) NULL,
  KIMLIK_DOGUM_YERI NVARCHAR(200) NULL,
  EV_ADRESI NVARCHAR(500) NULL,
  ADAY_NO INT NULL,
  ON_NOTLAR NVARCHAR(MAX) NULL,
  DOGUM_TARIHI DATE NULL,
  KAYIT_TARIHI DATE NULL,
  RESIM VARBINARY(MAX) NULL,
  ID_GRUP_KARTI INT NULL,
  SERTIFIKA_SINIFI NVARCHAR(100) NULL,
  ONCE_SERT_SINIFI NVARCHAR(100) NULL,
  KURSIYER_DURUMU INT NULL,
  ONCE_SERT_BELGESAYI NVARCHAR(100) NULL,
  RESIM_WEBCAM VARBINARY(MAX) NULL,
  KIMLIK_KAYIT_NO NVARCHAR(100) NULL,
  EV_TELEFON NVARCHAR(40) NULL,
  KALANBORC MONEY NULL,
  TOPLAM_BORC MONEY NULL,
  TOPLAM_ODENEN MONEY NULL,
  CINSIYET VARCHAR(20) NULL,
  TAHSILI NVARCHAR(100) NULL
);"),

                new ScriptItem("KURSIYERLER tablosu", @"IF OBJECT_ID('dbo.KURSIYERLER','U') IS NULL
CREATE TABLE dbo.KURSIYERLER(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  ADI NVARCHAR(200) NULL,
  SOYADI NVARCHAR(200) NULL,
  TC_NO VARCHAR(11) NULL,
  TC VARCHAR(11) NULL,
  GSM_1 NVARCHAR(40) NULL,
  GSM NVARCHAR(40) NULL,
  ID_GRUP_KARTI INT NULL,
  DOGUM_TARIHI DATE NULL,
  KAYIT_TARIHI DATE NULL,
  SERTIFIKA_SINIFI NVARCHAR(100) NULL,
  CINSIYET VARCHAR(20) NULL,
  TAHSILI NVARCHAR(100) NULL
);"),

                new ScriptItem("KURSIYER garanti kolonlar", @"
IF COL_LENGTH('dbo.KURSIYER', 'CINSIYET') IS NULL ALTER TABLE dbo.KURSIYER ADD CINSIYET VARCHAR(20) NULL;
IF COL_LENGTH('dbo.KURSIYER', 'TAHSILI') IS NULL ALTER TABLE dbo.KURSIYER ADD TAHSILI NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.KURSIYER', 'EV_IL') IS NULL ALTER TABLE dbo.KURSIYER ADD EV_IL NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.KURSIYER', 'EV_ILCE') IS NULL ALTER TABLE dbo.KURSIYER ADD EV_ILCE NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.KURSIYER', 'IS_ADRESI') IS NULL ALTER TABLE dbo.KURSIYER ADD IS_ADRESI NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.KURSIYER', 'IS_TELEFON_1') IS NULL ALTER TABLE dbo.KURSIYER ADD IS_TELEFON_1 NVARCHAR(40) NULL;
IF COL_LENGTH('dbo.KURSIYER', 'KALANBORC') IS NULL ALTER TABLE dbo.KURSIYER ADD KALANBORC MONEY NULL;
IF COL_LENGTH('dbo.KURSIYER', 'TOPLAM_BORC') IS NULL ALTER TABLE dbo.KURSIYER ADD TOPLAM_BORC MONEY NULL;
IF COL_LENGTH('dbo.KURSIYER', 'TOPLAM_ODENEN') IS NULL ALTER TABLE dbo.KURSIYER ADD TOPLAM_ODENEN MONEY NULL;
IF COL_LENGTH('dbo.KURSIYERLER', 'CINSIYET') IS NULL ALTER TABLE dbo.KURSIYERLER ADD CINSIYET VARCHAR(20) NULL;
IF COL_LENGTH('dbo.KURSIYERLER', 'TAHSILI') IS NULL ALTER TABLE dbo.KURSIYERLER ADD TAHSILI NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.KURSIYERLER', 'IS_ADRESI') IS NULL ALTER TABLE dbo.KURSIYERLER ADD IS_ADRESI NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.KURSIYERLER', 'IS_TELEFON_1') IS NULL ALTER TABLE dbo.KURSIYERLER ADD IS_TELEFON_1 NVARCHAR(40) NULL;
"),

                new ScriptItem("SINAV_TARIHLERI tablosu", @"IF OBJECT_ID('dbo.SINAV_TARIHLERI','U') IS NULL
CREATE TABLE dbo.SINAV_TARIHLERI(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  SINAV_TARIHI DATE NULL,
  SINAV_TURU VARCHAR(20) NULL,
  SINAV_DURUMU VARCHAR(20) NULL,
  SINAV_ACIKLAMA VARCHAR(150) NULL,
  AKT INT NULL
);"),

                new ScriptItem("SINAV_LISTE_TEORI tablosu", @"IF OBJECT_ID('dbo.SINAV_LISTE_TEORI','U') IS NULL
CREATE TABLE dbo.SINAV_LISTE_TEORI(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  ID_SINAV_TARIHI INT NULL,
  ID_KURSIYER INT NULL,
  TEO_HAK INT NULL,
  TRAFIK VARCHAR(15) NULL,
  ILK_YARDIM VARCHAR(15) NULL,
  MOTOR VARCHAR(15) NULL,
  TEO_DURUM VARCHAR(15) NULL,
  TEO_NOT VARCHAR(15) NULL,
  E_SINAV_TARIHI DATE NULL,
  E_SINAV_SAATI VARCHAR(10) NULL,
  E_SINAV_YERI VARCHAR(350) NULL,
  E_SINAV_ACIKLAMA VARCHAR(150) NULL
);"),
                new ScriptItem("SINAV_LISTE_TEORI garanti kolonlar", @"
IF OBJECT_ID('dbo.SINAV_LISTE_TEORI','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.SINAV_LISTE_TEORI','E_SINAV_YERI') IS NULL ALTER TABLE dbo.SINAV_LISTE_TEORI ADD E_SINAV_YERI VARCHAR(350) NULL;
  IF COL_LENGTH('dbo.SINAV_LISTE_TEORI','E_SINAV_ACIKLAMA') IS NULL ALTER TABLE dbo.SINAV_LISTE_TEORI ADD E_SINAV_ACIKLAMA VARCHAR(150) NULL;
END
"),

                new ScriptItem("SINAV_LISTE_DIREKSIYON tablosu", @"IF OBJECT_ID('dbo.SINAV_LISTE_DIREKSIYON','U') IS NULL
CREATE TABLE dbo.SINAV_LISTE_DIREKSIYON(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  ID_SINAV_TARIHI INT NULL,
  ID_KURSIYER INT NULL,
  DIR_HAK INT NULL,
  DIR_NOT VARCHAR(15) NULL,
  DIR_DURUM VARCHAR(15) NULL,
  RANDEVU_SAATI VARCHAR(20) NULL,
  DIR_PERSONEL VARCHAR(150) NULL,
  DIR_ARAC VARCHAR(50) NULL,
  ID_PERSONEL INT NULL,
  ID_ARAC INT NULL
);"),
                new ScriptItem("KURSIYER_EVRAK tablosu", @"IF OBJECT_ID('dbo.KURSIYER_EVRAK','U') IS NULL
CREATE TABLE dbo.KURSIYER_EVRAK(
  ID_KURSIYER INT PRIMARY KEY,
  EKSIK_OGRNIM_BEL VARCHAR(5) NULL,
  EKSIK_SAGLIK VARCHAR(5) NULL,
  EKSIK_SAVCILIK VARCHAR(5) NULL,
  EKSIK_SOZLESME VARCHAR(5) NULL,
  OGRNM_BEL_TURU VARCHAR(70) NULL,
  OGRNM_BEL_VEREN_KURUM VARCHAR(200) NULL,
  OGRNM_BEL_TARIHI DATE NULL,
  OGRNM_BEL_SAYISI VARCHAR(20) NULL,
  SAG_RAP_VEREN_KURUM VARCHAR(200) NULL,
  SAG_RAP_TARIHI DATE NULL,
  SAG_RAP_BELGENO VARCHAR(50) NULL,
  SAVCILIK_BEL_VEREN_KURUM VARCHAR(200) NULL,
  SAVCILIK_BEL_TARIHI DATE NULL,
  CriminalNo VARCHAR(60) NULL,
  RES_OGRNIM_BEL VARBINARY(MAX) NULL,
  RES_SAGLIK VARBINARY(MAX) NULL,
  RES_SAVCILIK VARBINARY(MAX) NULL,
  RES_SOZLESME_ON VARBINARY(MAX) NULL,
  RES_SOZLESME_ARKA VARBINARY(MAX) NULL,
  EKSIK_IMZA VARCHAR(5) NULL,
  RES_IMZA VARBINARY(MAX) NULL,
  OZUR_DURUMU VARCHAR(200) NULL,
  YABANCI_DIL VARCHAR(100) NULL,
  SAG_RAPOR_REFERANS VARCHAR(80) NULL,
  SAG_RAPOR_HESKODU VARCHAR(30) NULL,
  EKSIK_WEPCAM VARCHAR(5) NULL,
  FATURA_NO VARCHAR(100) NULL,
  FATURA_TARIHI DATE NULL,
  FATURA_TUTARI MONEY NULL
);"),
                new ScriptItem("KURSIYER_EVRAK binary migration", @"
IF OBJECT_ID('dbo.KURSIYER_EVRAK','U') IS NOT NULL
BEGIN
    DECLARE @ImageCols TABLE (ColName SYSNAME);
    INSERT INTO @ImageCols(ColName)
    VALUES
    ('RES_OGRNIM_BEL'),
    ('RES_SAGLIK'),
    ('RES_SAVCILIK'),
    ('RES_SOZLESME_ON'),
    ('RES_SOZLESME_ARKA'),
    ('RES_IMZA');

    DECLARE @col SYSNAME;
    DECLARE c CURSOR LOCAL FAST_FORWARD FOR SELECT ColName FROM @ImageCols;
    OPEN c;
    FETCH NEXT FROM c INTO @col;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF EXISTS
        (
            SELECT 1
            FROM sys.columns sc
            INNER JOIN sys.types st ON sc.user_type_id = st.user_type_id
            WHERE sc.object_id = OBJECT_ID('dbo.KURSIYER_EVRAK')
              AND sc.name = @col
              AND st.name = 'image'
        )
        BEGIN
            DECLARE @sql NVARCHAR(MAX) =
                N'ALTER TABLE dbo.KURSIYER_EVRAK ALTER COLUMN [' + @col + N'] VARBINARY(MAX) NULL;';
            EXEC sp_executesql @sql;
        END

        FETCH NEXT FROM c INTO @col;
    END

    CLOSE c;
    DEALLOCATE c;
END
"),

                new ScriptItem("GRUP_KARTI tablosu", @"IF OBJECT_ID('dbo.GRUP_KARTI','U') IS NULL
CREATE TABLE dbo.GRUP_KARTI(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  DONEM_YILI VARCHAR(6) NULL,
  DONEM_AYI VARCHAR(50) NULL,
  DONEM_SUBESI VARCHAR(50) NULL,
  DONEM_GRUBU VARCHAR(50) NULL,
  DONEM_ADI VARCHAR(200) NULL,
  SERTIFIKA_SINIFI VARCHAR(250) NULL,
  BAS_TAR DATE NULL,
  BIT_TAR DATE NULL,
  KAPASITE INT NULL,
  MEVCUT INT NULL,
  AKT INT NULL,
  GRUP_DURUMU BIT NULL
);"),

                new ScriptItem("DONEMLER tablosu", @"IF OBJECT_ID('dbo.DONEMLER','U') IS NULL
CREATE TABLE dbo.DONEMLER(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  DONEM_ADI VARCHAR(200) NULL
);"),

                new ScriptItem("PERSONEL tablosu", @"IF OBJECT_ID('dbo.PERSONEL','U') IS NULL
CREATE TABLE dbo.PERSONEL(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  PERSONEL_DURUMU VARCHAR(20) NULL,
  TC_NO VARCHAR(11) NULL,
  ADI VARCHAR(50) NULL,
  SOYADI VARCHAR(50) NULL,
  EHLIYET_SINIFI VARCHAR(20) NULL,
  EHLIYET_IKINCI VARCHAR(20) NULL,
  CINSIYET VARCHAR(10) NULL,
  MEDENI_DUR VARCHAR(10) NULL,
  DOGUM_TARIHI DATE NULL,
  YONETICI_GOREVI VARCHAR(100) NULL,
  VERDIGI_DERS_1 VARCHAR(100) NULL,
  SOZ_BASLAMA_TAR DATE NULL,
  RESIM VARBINARY(MAX) NULL
);"),

                new ScriptItem("PERSONEL rapor kolonlari", @"
IF OBJECT_ID('dbo.PERSONEL','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.PERSONEL','EV_ADRESI') IS NULL ALTER TABLE dbo.PERSONEL ADD EV_ADRESI NVARCHAR(400) NULL;
  IF COL_LENGTH('dbo.PERSONEL','GSM_1') IS NULL ALTER TABLE dbo.PERSONEL ADD GSM_1 NVARCHAR(40) NULL;
  IF COL_LENGTH('dbo.PERSONEL','KIM_BABA_ADI') IS NULL ALTER TABLE dbo.PERSONEL ADD KIM_BABA_ADI NVARCHAR(200) NULL;
  IF COL_LENGTH('dbo.PERSONEL','KIM_ANA_ADI') IS NULL ALTER TABLE dbo.PERSONEL ADD KIM_ANA_ADI NVARCHAR(200) NULL;
  IF COL_LENGTH('dbo.PERSONEL','KIM_DOGUM_YERI') IS NULL ALTER TABLE dbo.PERSONEL ADD KIM_DOGUM_YERI NVARCHAR(200) NULL;
END
"),

                new ScriptItem("AracParam tablosu", @"IF OBJECT_ID('dbo.AracParam','U') IS NULL
CREATE TABLE dbo.AracParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  ARAC_TIPI VARCHAR(150) NULL,
  ARAC_PLAKA VARCHAR(50) NULL,
  ARAC_ACIKLAMASI VARCHAR(250) NULL,
  DURUMU VARCHAR(50) NULL,
  KULLANIM VARCHAR(50) NULL,
  MARKASI VARCHAR(100) NULL,
  RENGI VARCHAR(50) NULL,
  VITES_TURU VARCHAR(50) NULL,
  YAKIT_TIPI VARCHAR(50) NULL,
  MODEL VARCHAR(50) NULL,
  MODEL_YILI VARCHAR(50) NULL,
  ARAC_TESCIL_TAR DATE NULL,
  HIZ_BAS_TAR DATE NULL,
  MUHAYENE_TAR DATE NULL,
  SIGORTA_BAS_TAR DATE NULL,
  SIGORTA_BEL_NO VARCHAR(100) NULL,
  KASKO_BAS_TAR DATE NULL,
  KASKO_BIT_TAR DATE NULL,
  KASKO_ISL_BEDELI MONEY NULL,
  SIGORTA_BIT_TAR DATE NULL,
  AKT INT NULL
);"),

                new ScriptItem("KursBilgiParam tablosu", @"IF OBJECT_ID('dbo.KursBilgiParam','U') IS NULL
CREATE TABLE dbo.KursBilgiParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  KURS_ADI VARCHAR(150) NULL,
  KURUM_KODU VARCHAR(50) NULL,
  ADRES VARCHAR(500) NULL,
  ILCE VARCHAR(50) NULL,
  IL VARCHAR(40) NULL,
  TELEFON VARCHAR(40) NULL,
  GSM VARCHAR(40) NULL,
  PK VARCHAR(10) NULL,
  WEB VARCHAR(200) NULL,
  E_POSTA VARCHAR(150) NULL,
  KURUCU_ADI VARCHAR(200) NULL,
  MUDUR_ADI VARCHAR(200) NULL,
  MUSTERI_NO VARCHAR(20) NULL,
  KURS_ADI_KISA VARCHAR(100) NULL,
  SOZLESME_BANKA_HESAPNO VARCHAR(400) NULL
);"),

                new ScriptItem("KursBilgiParam varsayilan kayit", @"
IF OBJECT_ID('dbo.KursBilgiParam','U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.KursBilgiParam)
BEGIN
  INSERT INTO dbo.KursBilgiParam (KURS_ADI, KURUM_KODU, MUDUR_ADI, ADRES, IL, ILCE, TELEFON, GSM, KURS_ADI_KISA, MUSTERI_NO)
  VALUES (N'KOLERA MTSK', N'1234', N'', N'', N'', N'', N'', N'', N'KOLERA', N'1234');
END
"),

                new ScriptItem("GenelParam tablosu", @"IF OBJECT_ID('dbo.GenelParam','U') IS NULL
CREATE TABLE dbo.GenelParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  MEBBIS_KUL_ADI_1 VARCHAR(100) NULL,
  MEBBIS_KUL_SIF_1 VARCHAR(100) NULL,
  MEBBIS_KUL_YET_1 VARCHAR(MAX) NULL,
  MEBBIS_KUL_ADI_2 VARCHAR(100) NULL,
  MEBBIS_KUL_SIF_2 VARCHAR(100) NULL,
  MEBBIS_KUL_YET_2 VARCHAR(MAX) NULL,
  MEBBIS_KUL_ADI_3 VARCHAR(100) NULL,
  MEBBIS_KUL_SIF_3 VARCHAR(100) NULL,
  MEBBIS_KUL_YET_3 VARCHAR(MAX) NULL,
  MEBBIS_KUL_ADI_4 VARCHAR(100) NULL,
  MEBBIS_KUL_SIF_4 VARCHAR(100) NULL,
  MEBBIS_KUL_YET_4 VARCHAR(MAX) NULL,
  MEBBIS_KUL_ADI_5 VARCHAR(100) NULL,
  MEBBIS_KUL_SIF_5 VARCHAR(100) NULL,
  MEBBIS_KUL_YET_5 VARCHAR(MAX) NULL
);"),

                new ScriptItem("SinifParam tablosu", @"IF OBJECT_ID('dbo.SinifParam','U') IS NULL
CREATE TABLE dbo.SinifParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  SINIF_DURUMU BIT NULL,
  SINIF_MEVCUT VARCHAR(30) NULL,
  SINIF_YENI VARCHAR(30) NULL,
  SINIF_YAS VARCHAR(5) NULL,
  SINIF_KUL_ARACLAR VARCHAR(300) NULL,
  SINIF_KAPSAMI VARCHAR(300) NULL,
  SINIF_DENEYIM VARCHAR(300) NULL,
  SINIF_KURS_UCRETI MONEY NULL,
  SINIF_TEORI_UCRETI MONEY NULL,
  SINIF_DRKS_UCRETI MONEY NULL,
  SINIF_TEORI_TRAFIK VARCHAR(10) NULL,
  SINIF_TEORI_MOTOR VARCHAR(10) NULL,
  SINIF_TEORI_ILKYRDM VARCHAR(10) NULL,
  SINIF_TEORI_TRAFIK_ADABI VARCHAR(10) NULL,
  SINIF_TEORI_TOP_SAAT VARCHAR(10) NULL,
  SINIF_TEORI_1SAAT_UCRETI MONEY NULL,
  SINIF_TEORI_TOP_UCRETI MONEY NULL,
  SINIF_DRKS_SAAT VARCHAR(10) NULL,
  SINIF_DRKS_1SAAT_UCRETI MONEY NULL,
  SINIF_DRKS_TOP_UCRETI MONEY NULL,
  SINIF_TABAN_FIYAT MONEY NULL,
  SINIF_DRKS_SMLT_EGTM VARCHAR(15) NULL,
  SINIF_DRKS_TOP_SAAT VARCHAR(15) NULL,
  YIL VARCHAR(4) NULL,
  SERT_2016_ONCESI BIT NULL,
  YUZ_YIRMI_BES_CC BIT NULL,
  E_SINAV_MUAF BIT NULL
);"),

                new ScriptItem("SertifikaSinifParam tablosu", @"IF OBJECT_ID('dbo.SertifikaSinifParam','U') IS NULL
CREATE TABLE dbo.SertifikaSinifParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  MEVCUT_SINIF VARCHAR(20) NULL,
  YENI_SINIF VARCHAR(20) NULL,
  YAS VARCHAR(10) NULL,
  UCRET MONEY NULL,
  TRAFIK VARCHAR(10) NULL,
  MOTOR VARCHAR(11) NULL,
  ILK_YARDIM VARCHAR(10) NULL,
  DIREKSIYON VARCHAR(10) NULL,
  KULLANDIGI_ARACLAR VARCHAR(300) NULL,
  KAPSAMI VARCHAR(300) NULL,
  DENEYIM VARCHAR(300) NULL,
  TEORI_HARC MONEY NULL,
  DRKS_HARC MONEY NULL
);"),

                new ScriptItem("APP_LOCAL_LISANS tablosu", @"IF OBJECT_ID('dbo.APP_LOCAL_LISANS','U') IS NULL
CREATE TABLE dbo.APP_LOCAL_LISANS(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  LISANS_NO NVARCHAR(200) NULL,
  UPDATED_AT DATETIME NOT NULL,
  KURUM_ADI NVARCHAR(400) NULL,
  BITIS_TARIHI DATETIME NULL,
  MUSTERI_NO NVARCHAR(200) NULL
);"),
                new ScriptItem("APP_LOG_KAYITLARI tablosu", @"IF OBJECT_ID('dbo.APP_LOG_KAYITLARI','U') IS NULL
CREATE TABLE dbo.APP_LOG_KAYITLARI(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  LOG_TARIHI DATETIME NOT NULL DEFAULT(GETDATE()),
  LOG_SEVIYE VARCHAR(20) NOT NULL,
  MODUL VARCHAR(100) NULL,
  KULLANICI_ADI VARCHAR(100) NULL,
  ACIKLAMA NVARCHAR(1000) NULL
);"),

                new ScriptItem("RAPOR_TANIMLARI tablosu", @"IF OBJECT_ID('dbo.RAPOR_TANIMLARI','U') IS NULL
CREATE TABLE dbo.RAPOR_TANIMLARI(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  RAPOR_GRUBU NVARCHAR(200) NOT NULL,
  RAPOR_ADI NVARCHAR(400) NOT NULL,
  RAPOR_YOLU NVARCHAR(1000) NOT NULL,
  SIRA_NO INT NOT NULL,
  AKTIF BIT NOT NULL,
  RENK INT NOT NULL,
  SABLON_BINARY VARBINARY(MAX) NULL,
  OLUSTURMA_TARIHI DATETIME2(7) NOT NULL,
  GUNCELLEME_TARIHI DATETIME2(7) NULL
);"),

                new ScriptItem("RAPOR_TANIMLARI varsayilan kayitlar", @"IF OBJECT_ID('dbo.RAPOR_TANIMLARI','U') IS NOT NULL
BEGIN
  ;WITH SeedRows AS
  (
    SELECT
      CAST(v.RAPOR_GRUBU AS NVARCHAR(200)) AS RAPOR_GRUBU,
      CAST(v.RAPOR_ADI AS NVARCHAR(400)) AS RAPOR_ADI,
      CAST(v.RAPOR_YOLU AS NVARCHAR(1000)) AS RAPOR_YOLU,
      CAST(1 AS BIT) AS AKTIF,
      v.RENK
    FROM (VALUES
      (N'KURSIYER', N'MURACAAT', N'C:\Raporlar\Muracaat_FormuSON2.frx', 0),
      (N'KURSIYER FORMLARI', N'KURSIYER MURACAAT MEB', N'C:\Raporlar\KursiyerMuracaatMeb.frx', 0),
      (N'KURSIYER FORMLARI', N'SOZLESME ON', N'C:\Raporlar\KursiyerSozlesmeOn.frx', 0),
      (N'KURSIYER FORMLARI', N'SOZLESME ARKA', N'C:\Raporlar\KursiyerSozlesmeArka.frx', 0),
      (N'KURSIYER FORMLARI', N'TRAFIK VE CEVRE BILGISI BASVURU FORMU', N'C:\Raporlar\TrafikCevreBilgisiBasvuruFormu.frx', 0),
      (N'KURSIYER FORMLARI', N'TRAFIK VE CEVRE DERS TAKIP CIZELGESI', N'C:\Raporlar\TrafikCevreDersTakipCizelgesi.frx', 0),
      (N'KURSIYER FORMLARI', N'ADAY IMZA VE ADRES BEYANI', N'C:\Raporlar\AdayImzaAdresBeyani.frx', 0),
      (N'KURSIYER FORMLARI', N'K SINIFI SURUCU ADAY BELGESI', N'C:\Raporlar\KSinifiSurucuAdayBelgesi.frx', 0),
      (N'KURSIYER FORMLARI', N'K SINIFI SURUCU ADAY BELGESI MATBU', N'C:\Raporlar\KSinifiSurucuAdayBelgesiMatbu.frx', 0),
      (N'KURSIYER FORMLARI', N'MURACAAT FORMU HAZIR', N'C:\Raporlar\MuracaatFormuHazir.frx', 0),
      (N'KURSIYER FORMLARI', N'MURACAAT FORMU MATBU', N'C:\Raporlar\MuracaatFormuMatbu.frx', 0),
      (N'KURSIYER FORMLARI', N'VELI MUVAFAKAT BELGESI', N'C:\Raporlar\VeliMuvafakatBelgesi.frx', 0),
      (N'KURSIYER FORMLARI', N'ADLI SICIL BEYANI', N'C:\Raporlar\AdliSicilBeyani.frx', 0),
      (N'SINAV LISTESI - ESINAV', N'E-SINAV SONUC LISTESI', N'C:\Raporlar\ESinavSonucListesi.frx', 0),
      (N'SINAV LISTESI - DIREKSIYON', N'SINAV GSM LISTESI', N'C:\Raporlar\SinavGsmListesi.frx', 0),
      (N'SINAV LISTESI - DIREKSIYON', N'DIREKSIYON SINAV KURSIYER LISTESI', N'C:\Raporlar\DireksiyonSinavKursiyerListesi.frx', 0),
      (N'SINAV LISTESI - DIREKSIYON', N'RANDEVU MEHMET', N'C:\Raporlar\DireksiyonEgitimiSinavTakipSonucListesi.frx', 0),
      (N'PERSONEL FORMLARI', N'KIMLIKKARTI', N'C:\Raporlar\PersonelKarti.frx', -16744448),
      (N'PERSONEL FORMLARI', N'PERSONEL KIMLIK KARTI MATBU', N'C:\Raporlar\PersonelKimlikKartiMatbu.frx', 0),
      (N'PERSONEL FORMLARI', N'PERSONEL MURACAAT FORMU', N'C:\Raporlar\PersonelMuracaatFormu.frx', 0),
      (N'KURSIYER FORMLARI', N'RAPOR 02 - KURSIYER FORMLARI', N'C:\Raporlar\Rapor_02.frx', -16744448),
      (N'GRUP RAPORLARI', N'RAPOR 03 - GRUP RAPORLARI', N'C:\Raporlar\Rapor_03.frx', -16744448),
      (N'KURSIYER RAPORLARI', N'RAPOR 04 - KURSIYER RAPORLARI', N'C:\Raporlar\Rapor_04.frx', -16744448),
      (N'SERTIFIKA RAPORLARI VE FORMLARI', N'RAPOR 05 - SERTIFIKA RAPORLARI VE FORMLARI', N'C:\Raporlar\Rapor_05.frx', -16744448),
      (N'SINAV LISTESI - ESINAV', N'RAPOR 06 - SINAV LISTESI - ESINAV', N'C:\Raporlar\Rapor_06.frx', -16744448),
      (N'SINAV LISTESI - DIREKSIYON', N'RAPOR 07 - SINAV LISTESI - DIREKSIYON', N'C:\Raporlar\Rapor_07.frx', -16744448),
      (N'PERSONEL FORMLARI', N'RAPOR 08 -YAKAKARTI', N'C:\Raporlar\Rapor_08.frx', -16744448),
      (N'PERSONEL FORMLARI', N'RAPOR 09 - PERSONEL FORMLARI', N'C:\Raporlar\DENEME.frx', -16744448),
      (N'PERSONEL RAPORLARI', N'RAPOR 10 - PERSONEL RAPORLARI', N'C:\Raporlar\Rapor_10.frx', -16744448),
      (N'BORC ODEME RAPORLARI', N'RAPOR 11 - BORC ODEME RAPORLARI', N'C:\Raporlar\Rapor_11.frx', -16744448),
      (N'BORC ODEME RAPORLARI', N'RAPOR 12 - BORC ODEME RAPORLARI', N'C:\Raporlar\Rapor_12.frx', -16744448),
      (N'MAKBUZ - SOZLESME', N'RAPOR 13 - MAKBUZ - SOZLESME', N'C:\Raporlar\Rapor_13.frx', -16744448),
      (N'KASA RAPORLARI', N'RAPOR 14 - KASA RAPORLARI', N'C:\Raporlar\Rapor_14.frx', -16744448),
      (N'PERSONEL MAKBUZ FORMLARI', N'RAPOR 15 - PERSONEL MAKBUZ FORMLARI', N'C:\Raporlar\Rapor_15.frx', -16744448),
      (N'KURSIYER FORMLARI', N'RAPOR 16 - KURSIYER FORMLARI', N'C:\Raporlar\Rapor_16.frx', -16744448),
      (N'SERTIFIKA RAPORLARI VE FORMLARI', N'RAPOR 17 - SERTIFIKA RAPORLARI VE FORMLARI', N'C:\Raporlar\Rapor_17.frx', -16744448),
      (N'OZEL DIREKSIYON DERS PROGRAMI', N'RAPOR 18 - OZEL DIREKSIYON DERS PROGRAMI', N'C:\Raporlar\Rapor_18.frx', -16744448),
      (N'BORC ODEME RAPORLARI', N'RAPOR 19 - BORC ODEME RAPORLARI', N'C:\Raporlar\Rapor_19.frx', -16744448),
      (N'BORC ODEME RAPORLARI', N'RAPOR 20 - BORC ODEME RAPORLARI', N'C:\Raporlar\Rapor_20.frx', -16744448),
      (N'SINAV LISTESI - DIREKSIYON', N'RAPOR 21 - SINAV LISTESI - DIREKSIYON', N'C:\Raporlar\Rapor_21.frx', -16744448),
      (N'GRUP RAPORLARI', N'RAPOR 22 - GRUP RAPORLARI', N'C:\Raporlar\Rapor_22.frx', -16744448),
      (N'KURSIYER FORMLARI', N'RAPOR 23 - KURSIYER FORMLARI', N'C:\Raporlar\Rapor_23.frx', -16744448),
      (N'KURSIYER FORMLARI', N'RAPOR 24 - KURSIYER FORMLARI', N'C:\Raporlar\Rapor_24.frx', -16744448),
      (N'KURSIYER RAPORLARI', N'RAPOR 25 - KURSIYER RAPORLARI', N'C:\Raporlar\Rapor_25.frx', -16744448),
      (N'SINAV LISTESI - DIREKSIYON', N'RAPOR 26 - SINAV LISTESI - DIREKSIYON', N'C:\Raporlar\Rapor_26.frx', -16744448),
      (N'KURSIYER FORMLARI', N'RAPOR 27 - KURSIYER FORMLARI', N'C:\Raporlar\Rapor_27.frx', -16744448),
      (N'GRUP RAPORLARI', N'RAPOR 28 - GRUP RAPORLARI', N'C:\Raporlar\Rapor_28.frx', -16744448),
      (N'SINAV LISTESI - DIREKSIYON', N'RAPOR 29 - SINAV LISTESI - DIREKSIYON', N'C:\Raporlar\Rapor_29.frx', -16744448),
      (N'SINAV LISTESI - DIREKSIYON', N'RAPOR 30 - SINAV LISTESI - DIREKSIYON', N'C:\Raporlar\Rapor_30.frx', -16744448),
      (N'SINAV LISTESI - DIREKSIYON', N'RAPOR 31 - SINAV LISTESI - DIREKSIYON', N'C:\Raporlar\Rapor_31.frx', -16744448),
      (N'KURSIYER FORMLARI', N'RAPOR 32 - KURSIYER FORMLARI', N'C:\Raporlar\Rapor_32.frx', -16744448),
      (N'GRUP RAPORLARI', N'RAPOR 33 - GRUP RAPORLARI', N'C:\Raporlar\Rapor_33.frx', -16744448),
      (N'EVRAK LISTESI', N'RAPOR 34 - EVRAK LISTESI', N'C:\Raporlar\Rapor_34.frx', -16744448),
      (N'KURSIYER FORMLARI', N'RAPOR 35 - KURSIYER FORMLARI', N'C:\Raporlar\Rapor_35.frx', -16744448),
      (N'DERS PROGRAMI - DIREKSIYON', N'RAPOR 36 - DERS PROGRAMI - DIREKSIYON', N'C:\Raporlar\Rapor_36.frx', -16744448)
    ) v(RAPOR_GRUBU, RAPOR_ADI, RAPOR_YOLU, RENK)
  ),
  InsertList AS
  (
    SELECT
      s.*,
      CASE
        WHEN UPPER(s.RAPOR_GRUBU) = N'PERSONEL FORMLARI' AND UPPER(s.RAPOR_ADI) = N'KIMLIKKARTI' THEN 1
        ELSE ROW_NUMBER() OVER (ORDER BY NEWID()) + 1
      END AS SIRA_NO_RANDOM
    FROM SeedRows s
    WHERE NOT EXISTS
    (
      SELECT 1
      FROM dbo.RAPOR_TANIMLARI r
      WHERE r.RAPOR_GRUBU = s.RAPOR_GRUBU
        AND r.RAPOR_ADI = s.RAPOR_ADI
    )
  )
  INSERT INTO dbo.RAPOR_TANIMLARI
    (RAPOR_GRUBU, RAPOR_ADI, RAPOR_YOLU, SIRA_NO, AKTIF, RENK, SABLON_BINARY, OLUSTURMA_TARIHI, GUNCELLEME_TARIHI)
  SELECT
    i.RAPOR_GRUBU,
    i.RAPOR_ADI,
    i.RAPOR_YOLU,
    i.SIRA_NO_RANDOM,
    i.AKTIF,
    0,
    NULL,
    GETDATE(),
    GETDATE()
  FROM InsertList i;
END"),

                new ScriptItem("SMSSABLONLARI tablosu", @"IF OBJECT_ID('dbo.SMSSABLONLARI','U') IS NULL
CREATE TABLE dbo.SMSSABLONLARI(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  SABLON_ADI NVARCHAR(200) NOT NULL,
  SABLON_METNI NVARCHAR(MAX) NOT NULL,
  SIRA_NO INT NOT NULL CONSTRAINT DF_SMSSABLONLARI_SIRA DEFAULT(0),
  AKTIF BIT NOT NULL CONSTRAINT DF_SMSSABLONLARI_AKTIF DEFAULT(1),
  OLUSTURMA_TARIHI DATETIME2(7) NOT NULL CONSTRAINT DF_SMSSABLONLARI_OLUSTUR DEFAULT(SYSUTCDATETIME()),
  GUNCELLEME_TARIHI DATETIME2(7) NULL
);"),

                new ScriptItem("SMSSABLONLARI varsayilan kayitlar", @"IF OBJECT_ID('dbo.SMSSABLONLARI','U') IS NOT NULL
BEGIN
  IF NOT EXISTS (SELECT 1 FROM dbo.SMSSABLONLARI WHERE SABLON_ADI = N'Kursiyer - direksiyon bilgi')
    INSERT INTO dbo.SMSSABLONLARI (SABLON_ADI, SABLON_METNI, SIRA_NO, AKTIF)
    VALUES (N'Kursiyer - direksiyon bilgi', N'SAYIN [AD SOYAD]; DIREKSIYON SINAV BILGILERI ICIN [KURS ADI] KURSUMUZU ARAYINIZ. Tel: [TELEFON]', 1, 1);
  IF NOT EXISTS (SELECT 1 FROM dbo.SMSSABLONLARI WHERE SABLON_ADI = N'E-sinav hatirlatma')
    INSERT INTO dbo.SMSSABLONLARI (SABLON_ADI, SABLON_METNI, SIRA_NO, AKTIF)
    VALUES (N'E-sinav hatirlatma', N'SAYIN [AD SOYAD]; [TARIH] SAAT:[SAAT] E-SINAVINIZ VARDIR. SINAV SAATINDEN ONCE KIMLIGINIZLE HAZIR BULUNUNUZ. [KURS ADI] [TELEFON]', 2, 1);
  IF NOT EXISTS (SELECT 1 FROM dbo.SMSSABLONLARI WHERE SABLON_ADI = N'Direksiyon sinav hatirlatma')
    INSERT INTO dbo.SMSSABLONLARI (SABLON_ADI, SABLON_METNI, SIRA_NO, AKTIF)
    VALUES (N'Direksiyon sinav hatirlatma', N'SAYIN [AD SOYAD]; [TARIH] YARIN SAAT:[SAAT] DIREKSIYON SINAVINIZ VARDIR. SINAV SAATINDEN YARIM SAAT ONCE DIREKSIYON EGITIM PISTINDE KIMLIGINIZLE BULUNMANIZ ONEMLE RICA OLUNUR. [KURS ADI] [TELEFON]', 3, 1);
END"),

                new ScriptItem("DB_VERSION tablosu", @"IF OBJECT_ID('dbo.DB_VERSION','U') IS NULL
CREATE TABLE dbo.DB_VERSION(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  VERSION_NO INT NOT NULL,
  SCRIPT_NAME NVARCHAR(400) NOT NULL,
  APPLIED_ON DATETIME NOT NULL DEFAULT(GETDATE())
);"),

                new ScriptItem("KOLERA_LISANS tablosu", @"IF OBJECT_ID('dbo.KOLERA_LISANS','U') IS NULL
CREATE TABLE dbo.KOLERA_LISANS(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  LISANS_KURUM_KODU NVARCHAR(300) NULL,
  LISANS_NO NVARCHAR(300) NULL,
  LISANS_BITIS_TARIHI DATETIME NULL,
  PROGRAM_VERSION NVARCHAR(100) NULL,
  OLUSTURMA_TARIHI DATETIME NOT NULL DEFAULT(GETDATE())
);"),

                new ScriptItem("KOLERA_LISANS ilk sablon", @"
IF OBJECT_ID('dbo.KOLERA_LISANS','U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.KOLERA_LISANS)
  INSERT INTO dbo.KOLERA_LISANS (LISANS_KURUM_KODU, LISANS_NO, LISANS_BITIS_TARIHI)
  VALUES (N'KOLERA_MTSK', N'ABC-456-XYZ', CAST(N'2099-12-31' AS DATE));
"),

                new ScriptItem("KOLERA_ISTAMPA tablosu", @"IF OBJECT_ID('dbo.KOLERA_ISTAMPA','U') IS NULL
CREATE TABLE dbo.KOLERA_ISTAMPA(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  ALAN_KODU NVARCHAR(50) NOT NULL,
  ALAN_ADI NVARCHAR(100) NOT NULL,
  RESIM VARBINARY(MAX) NULL,
  ACIKLAMA NVARCHAR(250) NULL,
  GUNCELLEME_TARIHI DATETIME NULL
);"),

                new ScriptItem("KOLERA_ISTAMPA varsayilan satirlar", @"
IF OBJECT_ID('dbo.KOLERA_ISTAMPA','U') IS NOT NULL
BEGIN
  IF NOT EXISTS (SELECT 1 FROM dbo.KOLERA_ISTAMPA WHERE ALAN_KODU = N'IMZA')
    INSERT INTO dbo.KOLERA_ISTAMPA (ALAN_KODU, ALAN_ADI, GUNCELLEME_TARIHI) VALUES (N'IMZA', N'Imza', GETDATE());
  IF NOT EXISTS (SELECT 1 FROM dbo.KOLERA_ISTAMPA WHERE ALAN_KODU = N'MUHUR')
    INSERT INTO dbo.KOLERA_ISTAMPA (ALAN_KODU, ALAN_ADI, GUNCELLEME_TARIHI) VALUES (N'MUHUR', N'Muhur', GETDATE());
  IF NOT EXISTS (SELECT 1 FROM dbo.KOLERA_ISTAMPA WHERE ALAN_KODU = N'KASE')
    INSERT INTO dbo.KOLERA_ISTAMPA (ALAN_KODU, ALAN_ADI, GUNCELLEME_TARIHI) VALUES (N'KASE', N'Kase', GETDATE());
  IF NOT EXISTS (SELECT 1 FROM dbo.KOLERA_ISTAMPA WHERE ALAN_KODU = N'ASLI_GIBIDIR')
    INSERT INTO dbo.KOLERA_ISTAMPA (ALAN_KODU, ALAN_ADI, GUNCELLEME_TARIHI) VALUES (N'ASLI_GIBIDIR', N'Asli Gibidir', GETDATE());
  IF NOT EXISTS (SELECT 1 FROM dbo.KOLERA_ISTAMPA WHERE ALAN_KODU = N'INCELENDI')
    INSERT INTO dbo.KOLERA_ISTAMPA (ALAN_KODU, ALAN_ADI, GUNCELLEME_TARIHI) VALUES (N'INCELENDI', N'Incelendi', GETDATE());
END
"),

                new ScriptItem("REFERANS_KART tablosu", @"IF OBJECT_ID('dbo.REFERANS_KART','U') IS NULL
CREATE TABLE dbo.REFERANS_KART(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  REF_SEC BIT NULL,
  REF_FIRMA_UNVANI VARCHAR(300) NULL,
  REF_ADI_SOYADI VARCHAR(200) NOT NULL,
  REF_GSM_1 VARCHAR(15) NULL,
  REF_GSM_2 VARCHAR(15) NULL,
  REF_NOTLAR VARCHAR(300) NULL
);"),

                new ScriptItem("PARAM_DIREKSIYON_SAAT tablosu", @"IF OBJECT_ID('dbo.PARAM_DIREKSIYON_SAAT','U') IS NULL
CREATE TABLE dbo.PARAM_DIREKSIYON_SAAT(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  SAAT VARCHAR(30) NULL
);"),

                new ScriptItem("DurumParam tablosu", @"IF OBJECT_ID('dbo.DurumParam','U') IS NULL
CREATE TABLE dbo.DurumParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  KOD INT NOT NULL,
  ACIKLAMA VARCHAR(20) NULL
);"),

                new ScriptItem("IlParam tablosu", @"IF OBJECT_ID('dbo.IlParam','U') IS NULL
CREATE TABLE dbo.IlParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  IL_KODU INT NULL,
  IL_ADI VARCHAR(150) NULL
);"),

                new ScriptItem("IlceParam tablosu", @"IF OBJECT_ID('dbo.IlceParam','U') IS NULL
CREATE TABLE dbo.IlceParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  IL_KODU INT NULL,
  ILCE_ADI VARCHAR(150) NULL
);"),

                new ScriptItem("SettingsParam tablosu", @"IF OBJECT_ID('dbo.SettingsParam','U') IS NULL
CREATE TABLE dbo.SettingsParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  LSN_KURUM_KODU VARCHAR(80) NULL,
  LSN_LISANS_NO VARCHAR(80) NULL,
  LSN_BITIS_TARIHI VARCHAR(10) NULL
);"),

                new ScriptItem("SettingsParam ilk sablon", @"
IF OBJECT_ID('dbo.SettingsParam','U') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.SettingsParam)
  INSERT INTO dbo.SettingsParam (LSN_KURUM_KODU, LSN_LISANS_NO, LSN_BITIS_TARIHI)
  VALUES (N'1234', N'ABC-456-XYZ', N'2099-12-31');
"),

                new ScriptItem("SertifikaUcretParam tablosu", @"IF OBJECT_ID('dbo.SertifikaUcretParam','U') IS NULL
CREATE TABLE dbo.SertifikaUcretParam(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  ID_SERTIFIKA INT NULL,
  UC_SINIF VARCHAR(30) NULL,
  UC_DONEM_ADI VARCHAR(40) NULL,
  UC_DONEM_YILI VARCHAR(15) NULL,
  UC_TEORIK_1_SAAT MONEY NULL,
  UC_DIREKS_1_SAAT MONEY NULL,
  UC_TOPLAM_DERS_SA INT NULL,
  UC_ACIKLAMA VARCHAR(300) NULL,
  UC_SINIF_ONC VARCHAR(30) NULL
);"),

                new ScriptItem("SP_KOLERA_KULLANICI", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_KULLANICI
@ISLEM CHAR(1), @ID INT = NULL, @KULLANICI_ADI NVARCHAR(100)=NULL, @PAROLA NVARCHAR(100)=NULL, @YETKI NVARCHAR(100)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @ISLEM='S' SELECT ID, KULLANICI_ADI, YETKI, KAYIT_TARIHI FROM KULLANICI ORDER BY KULLANICI_ADI;
END;"),

                new ScriptItem("SP_KOLERA_KURSIYER_ESINAVI", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_KURSIYER_ESINAVI
@ID_KURSIYER INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT ID AS TEORISNV_ID, ID_KURSIYER, E_SINAV_TARIHI AS ESINAV_TARIHI, TEO_NOT, TEO_HAK
  FROM SINAV_LISTE_TEORI WHERE ID_KURSIYER=@ID_KURSIYER ORDER BY TEO_HAK ASC;
END;"),

                new ScriptItem("SP_KOLERA_ESINAV_TARIHLERI_LISTELE", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_ESINAV_TARIHLERI_LISTELE
AS
BEGIN
  SET NOCOUNT ON;
  SELECT ID, SINAV_TARIHI, ISNULL(SINAV_ACIKLAMA, '') AS ACIKLAMA FROM SINAV_TARIHLERI ORDER BY SINAV_TARIHI DESC, ID DESC;
END;"),

                new ScriptItem("SP_KOLERA_ARACLAR", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_ARACLAR
@ISLEM CHAR(1),
@ID INT = NULL,
@ARAC_TIPI NVARCHAR(150)=NULL,
@ARAC_PLAKA NVARCHAR(50)=NULL,
@DURUMU NVARCHAR(50)=NULL,
@RENGI NVARCHAR(50)=NULL,
@VITES_TURU NVARCHAR(50)=NULL,
@MODEL NVARCHAR(50)=NULL,
@MUHAYENE_TAR DATETIME=NULL,
@AKT INT=NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @ISLEM='S'
    SELECT * FROM AracParam WHERE (@ID IS NULL OR ID=@ID);
  ELSE IF @ISLEM='I'
    INSERT INTO AracParam(ARAC_TIPI,ARAC_PLAKA,DURUMU,RENGI,VITES_TURU,MODEL,MUHAYENE_TAR,AKT)
    VALUES(@ARAC_TIPI,@ARAC_PLAKA,@DURUMU,@RENGI,@VITES_TURU,@MODEL,@MUHAYENE_TAR,ISNULL(@AKT,1));
  ELSE IF @ISLEM='U'
    UPDATE AracParam
       SET ARAC_TIPI=@ARAC_TIPI,
           ARAC_PLAKA=@ARAC_PLAKA,
           DURUMU=@DURUMU,
           RENGI=@RENGI,
           VITES_TURU=@VITES_TURU,
           MODEL=@MODEL,
           MUHAYENE_TAR=@MUHAYENE_TAR,
           AKT=@AKT
     WHERE ID=@ID;
  ELSE IF @ISLEM='D'
    DELETE FROM AracParam WHERE ID=@ID;
  ELSE IF @ISLEM='P'
    UPDATE AracParam SET AKT=0, DURUMU='Pasif' WHERE ID=@ID;
END;"),

                new ScriptItem("SP_KOLERA_PERSONELLER", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_PERSONELLER
@ISLEM CHAR(1), @ID INT = NULL, @TC_NO VARCHAR(11)=NULL, @ADI VARCHAR(50)=NULL, @SOYADI VARCHAR(50)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @ISLEM='S' SELECT * FROM PERSONEL WHERE (@ID IS NULL OR ID=@ID) ORDER BY ADI ASC;
  ELSE IF @ISLEM='I' INSERT INTO PERSONEL(TC_NO,ADI,SOYADI) VALUES(@TC_NO,@ADI,@SOYADI);
  ELSE IF @ISLEM='U' UPDATE PERSONEL SET TC_NO=@TC_NO, ADI=@ADI, SOYADI=@SOYADI WHERE ID=@ID;
  ELSE IF @ISLEM='D' DELETE FROM PERSONEL WHERE ID=@ID;
END;"),

                new ScriptItem("SP_KOLERA_KURSIYER", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_KURSIYER
@ISLEM_TIPI CHAR(1),
@ID INT = NULL,
@ADI NVARCHAR(200)=NULL,
@SOYADI NVARCHAR(200)=NULL,
@TC_NO NVARCHAR(20)=NULL,
@KIM_KAYIT_NO NVARCHAR(50)=NULL,
@ID_GRUP_KARTI INT=NULL,
@SERTIFIKA_SINIFI NVARCHAR(50)=NULL,
@ONCE_SERT_SINIFI NVARCHAR(50)=NULL,
@ONCE_SERT_BELGESAYI NVARCHAR(50)=NULL,
@ADAY_NO INT=NULL,
@KURSIYER_DURUMU INT=NULL,
@DOGUM_TARIHI DATETIME=NULL,
@KAYIT_TARIHI DATETIME=NULL,
@RESIM VARBINARY(MAX)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @ISLEM_TIPI='I'
  BEGIN
    INSERT INTO KURSIYER
    (ADI,SOYADI,TC_NO,KIMLIK_KAYIT_NO,ID_GRUP_KARTI,SERTIFIKA_SINIFI,ONCE_SERT_SINIFI,ONCE_SERT_BELGESAYI,ADAY_NO,KURSIYER_DURUMU,DOGUM_TARIHI,KAYIT_TARIHI,RESIM)
    VALUES
    (@ADI,@SOYADI,@TC_NO,@KIM_KAYIT_NO,@ID_GRUP_KARTI,@SERTIFIKA_SINIFI,@ONCE_SERT_SINIFI,@ONCE_SERT_BELGESAYI,@ADAY_NO,ISNULL(@KURSIYER_DURUMU,1),@DOGUM_TARIHI,ISNULL(@KAYIT_TARIHI,GETDATE()),@RESIM);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NEW_ID;
  END
  ELSE IF @ISLEM_TIPI='U'
  BEGIN
    UPDATE KURSIYER SET
      ADI=@ADI,
      SOYADI=@SOYADI,
      TC_NO=@TC_NO,
      KIMLIK_KAYIT_NO=@KIM_KAYIT_NO,
      ID_GRUP_KARTI=@ID_GRUP_KARTI,
      SERTIFIKA_SINIFI=@SERTIFIKA_SINIFI,
      ONCE_SERT_SINIFI=@ONCE_SERT_SINIFI,
      ONCE_SERT_BELGESAYI=@ONCE_SERT_BELGESAYI,
      ADAY_NO=@ADAY_NO,
      KURSIYER_DURUMU=@KURSIYER_DURUMU,
      DOGUM_TARIHI=@DOGUM_TARIHI,
      KAYIT_TARIHI=@KAYIT_TARIHI,
      RESIM=@RESIM
    WHERE ID=@ID;
    SELECT @ID AS NEW_ID;
  END
  ELSE IF @ISLEM_TIPI='D'
    DELETE FROM KURSIYER WHERE ID=@ID;
END;"),

                new ScriptItem("SP_KOLERA_KURSIYER_DIREKSIYONSINAVI", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_KURSIYER_DIREKSIYONSINAVI
@ID_KURSIYER INT
AS
BEGIN
  SET NOCOUNT ON;
  SELECT d.ID, st.SINAV_TARIHI, ISNULL(d.RANDEVU_SAATI,'') AS RANDEVU_SAATI
  FROM SINAV_LISTE_DIREKSIYON d
  INNER JOIN SINAV_TARIHLERI st ON st.ID=d.ID_SINAV_TARIHI
  WHERE d.ID_KURSIYER=@ID_KURSIYER
  ORDER BY st.SINAV_TARIHI DESC;
END;"),

                new ScriptItem("SP_KOLERA_DONEMGRUP", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_DONEMGRUP
@ISLEM CHAR(1), @ID INT = NULL, @YIL INT = NULL, @AY NVARCHAR(20)=NULL, @SUBE NVARCHAR(100)=NULL, @DONEM_ADI NVARCHAR(150)=NULL, @GRUP_ADI NVARCHAR(100)=NULL, @BAS_TAR DATE=NULL, @BIT_TAR DATE=NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @ISLEM='L' SELECT * FROM GRUP_KARTI ORDER BY BAS_TAR DESC;
  ELSE IF @ISLEM='I' INSERT INTO GRUP_KARTI (DONEM_YILI,DONEM_AYI,DONEM_SUBESI,DONEM_ADI,DONEM_GRUBU,BAS_TAR,BIT_TAR) VALUES (@YIL,@AY,@SUBE,@DONEM_ADI,@GRUP_ADI,@BAS_TAR,@BIT_TAR);
  ELSE IF @ISLEM='U' UPDATE GRUP_KARTI SET DONEM_YILI=@YIL,DONEM_AYI=@AY,DONEM_SUBESI=@SUBE,DONEM_ADI=@DONEM_ADI,DONEM_GRUBU=@GRUP_ADI,BAS_TAR=@BAS_TAR,BIT_TAR=@BIT_TAR WHERE ID=@ID;
  ELSE IF @ISLEM='D' DELETE FROM GRUP_KARTI WHERE ID=@ID;
END;"),

                new ScriptItem("SP_KOLERA_GRUPLAR", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_GRUPLAR
@IslemTipi NVARCHAR(10), @Id INT = NULL, @Yil INT = NULL, @Ay NVARCHAR(20)=NULL, @Sube NVARCHAR(50)=NULL, @DonemAdi NVARCHAR(100)=NULL, @GrupAdi NVARCHAR(50)=NULL, @Baslangic DATETIME=NULL, @Bitis DATETIME=NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @IslemTipi='INSERT'
    INSERT INTO GRUP_KARTI(DONEM_YILI,DONEM_AYI,DONEM_SUBESI,DONEM_ADI,DONEM_GRUBU,BAS_TAR,BIT_TAR) VALUES(@Yil,@Ay,@Sube,@DonemAdi,@GrupAdi,@Baslangic,@Bitis);
  ELSE IF @IslemTipi='UPDATE'
    UPDATE GRUP_KARTI SET DONEM_YILI=@Yil,DONEM_AYI=@Ay,DONEM_SUBESI=@Sube,DONEM_ADI=@DonemAdi,DONEM_GRUBU=@GrupAdi,BAS_TAR=@Baslangic,BIT_TAR=@Bitis WHERE ID=@Id;
  ELSE IF @IslemTipi='DELETE'
    DELETE FROM GRUP_KARTI WHERE ID=@Id;
END;"),

                new ScriptItem("SP_KOLERA_UPSERT_KURSIYER_EVRAK", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_UPSERT_KURSIYER_EVRAK
@ID_KURSIYER INT, @EKSIK_OGRNIM_BEL VARCHAR(5)=NULL, @EKSIK_SAGLIK VARCHAR(5)=NULL, @EKSIK_SAVCILIK VARCHAR(5)=NULL, @EKSIK_SOZLESME VARCHAR(5)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF EXISTS (SELECT 1 FROM KURSIYER_EVRAK WHERE ID_KURSIYER=@ID_KURSIYER)
    UPDATE KURSIYER_EVRAK SET EKSIK_OGRNIM_BEL=@EKSIK_OGRNIM_BEL,EKSIK_SAGLIK=@EKSIK_SAGLIK,EKSIK_SAVCILIK=@EKSIK_SAVCILIK,EKSIK_SOZLESME=@EKSIK_SOZLESME WHERE ID_KURSIYER=@ID_KURSIYER;
  ELSE
    INSERT INTO KURSIYER_EVRAK(ID_KURSIYER,EKSIK_OGRNIM_BEL,EKSIK_SAGLIK,EKSIK_SAVCILIK,EKSIK_SOZLESME) VALUES(@ID_KURSIYER,@EKSIK_OGRNIM_BEL,@EKSIK_SAGLIK,@EKSIK_SAVCILIK,@EKSIK_SOZLESME);
END;"),

                new ScriptItem("UPSERT_KURSIYER_EVRAK", @"CREATE OR ALTER PROCEDURE dbo.UPSERT_KURSIYER_EVRAK
@ID_KURSIYER INT, @EKSIK_OGRNIM_BEL VARCHAR(5)=NULL, @EKSIK_SAGLIK VARCHAR(5)=NULL, @EKSIK_SAVCILIK VARCHAR(5)=NULL, @EKSIK_SOZLESME VARCHAR(5)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  EXEC dbo.SP_KOLERA_UPSERT_KURSIYER_EVRAK @ID_KURSIYER=@ID_KURSIYER, @EKSIK_OGRNIM_BEL=@EKSIK_OGRNIM_BEL, @EKSIK_SAGLIK=@EKSIK_SAGLIK, @EKSIK_SAVCILIK=@EKSIK_SAVCILIK, @EKSIK_SOZLESME=@EKSIK_SOZLESME;
END;"),

                new ScriptItem("SP_UPSERT_KURSIYER_EVRAK", @"CREATE OR ALTER PROCEDURE dbo.SP_UPSERT_KURSIYER_EVRAK
@ID_KURSIYER INT, @EKSIK_OGRNIM_BEL VARCHAR(5)=NULL, @EKSIK_SAGLIK VARCHAR(5)=NULL, @EKSIK_SAVCILIK VARCHAR(5)=NULL, @EKSIK_SOZLESME VARCHAR(5)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  EXEC dbo.SP_KOLERA_UPSERT_KURSIYER_EVRAK @ID_KURSIYER=@ID_KURSIYER, @EKSIK_OGRNIM_BEL=@EKSIK_OGRNIM_BEL, @EKSIK_SAGLIK=@EKSIK_SAGLIK, @EKSIK_SAVCILIK=@EKSIK_SAVCILIK, @EKSIK_SOZLESME=@EKSIK_SOZLESME;
END;"),

                new ScriptItem("SP_KOLERA_DIREKSIYON_SINAVI_HAZIRLA", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_DIREKSIYON_SINAVI_HAZIRLA
@ISLEM CHAR(1), @SINAV_ID INT=NULL, @KURSIYER_ID INT=NULL, @ID INT=NULL, @PERSONEL_ID INT=NULL, @ARAC_ID INT=NULL, @SAAT VARCHAR(20)=NULL
AS
BEGIN
  SET NOCOUNT ON;
  IF @ISLEM='L'
    SELECT * FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@SINAV_ID;
  ELSE IF @ISLEM='E'
  BEGIN
    IF NOT EXISTS (SELECT 1 FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@SINAV_ID AND ID_KURSIYER=@KURSIYER_ID)
      INSERT INTO SINAV_LISTE_DIREKSIYON (ID_SINAV_TARIHI, ID_KURSIYER, DIR_HAK, DIR_DURUM)
      VALUES (@SINAV_ID, @KURSIYER_ID, 0, 'GIRMEDI');
  END
  ELSE IF @ISLEM='G'
    UPDATE SINAV_LISTE_DIREKSIYON
       SET ID_PERSONEL=@PERSONEL_ID,
           ID_ARAC=@ARAC_ID,
           RANDEVU_SAATI=@SAAT
     WHERE ID=@ID;
  ELSE IF @ISLEM='D'
    DELETE FROM SINAV_LISTE_DIREKSIYON WHERE ID=@ID;
END;"),

                new ScriptItem("SP_KOLERA_MEBBISAKTAR", @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_MEBBISAKTAR
@ID_KURSIYER INT = NULL
AS
BEGIN
  SET NOCOUNT ON;
  SELECT k.*, e.OGRNM_BEL_TURU, e.SAG_RAP_BELGENO, e.CriminalNo
  FROM KURSIYER k
  LEFT JOIN KURSIYER_EVRAK e ON e.ID_KURSIYER = k.ID
  WHERE (@ID_KURSIYER IS NULL OR k.ID=@ID_KURSIYER);
END;")
            };
        }

        private sealed class ScriptItem
        {
            public string Name { get; private set; }
            public string Sql { get; private set; }

            public ScriptItem(string name, string sql)
            {
                Name = name;
                Sql = sql;
            }
        }

        private sealed class ReportSeedRow
        {
            public int Id { get; set; }
            public string RaporGrubu { get; set; }
            public string RaporAdi { get; set; }
            public string RaporYolu { get; set; }
        }
    }
}
