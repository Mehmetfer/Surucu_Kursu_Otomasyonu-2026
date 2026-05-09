using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace Kolera_Mtsk.Services
{
    /// <summary>
    /// Mevcut veritabanlarina, bos kurulum scriptleriyle eklenen idempotent degisiklikleri
    /// tek seferlik uygular (DB_VERSION ile izlenir).
    /// </summary>
    internal static class DatabaseSchemaMigration
    {
        public const int Migration202602RaporUyumu = 20260202;
        /// <summary>PROX / eski MTSK KURSIYER adres kolonlari ile ayni genislik (kopya araci uzunluk uyumu).</summary>
        public const int Migration202605KursiyerAdresProx = 20260502;
        public const int Migration202605SmsSablonlari = 20260503;
        public const int Migration202605KoleraIstampa = 20260504;

        /// <summary>
        /// SQL baglantisi acikken cagirin. Hata olursa giris akisini bloklamaz;
        /// detay SchemaMigrationLog.txt dosyasina yazilir.
        /// </summary>
        /// <summary>
        /// KOLERA_ISTAMPA tablosunu ve varsayilan satirlari idempotent olarak olusturur.
        /// Kayit oncesi cagrildiginda tablo hic yoksa bile INSERT/UPDATE calisir.
        /// </summary>
        internal static void EnsureKoleraIstampaTable(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Baglanti satiri bos.", nameof(connectionString));

            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(GetMigration20260504KoleraIstampaSql(), con))
                {
                    cmd.CommandTimeout = 120;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static void ApplyIfNeeded(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return;

            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    EnsureDbVersionTable(con);
                    RunMigrationIfPending(con, Migration202602RaporUyumu, GetMigration20260202Sql(),
                        "Rapor/parametre kolonlari ve KursBilgiParam varsayilan (2026-02)");
                    RunMigrationIfPending(con, Migration202605KursiyerAdresProx, GetMigration20260502KursiyerAdresSql(),
                        "KURSIYER adres kolonlari genisligi (PROX kopya uyumu)");
                    RunMigrationIfPending(con, Migration202605SmsSablonlari, GetMigration20260503SmsSablonlariSql(),
                        "SMSSABLONLARI tablosu ve varsayilan SMS sablonlari");
                    RunMigrationIfPending(con, Migration202605KoleraIstampa, GetMigration20260504KoleraIstampaSql(),
                        "KOLERA_ISTAMPA tablosu ve varsayilan satirlar");
                }
            }
            catch (Exception ex)
            {
                WriteMigrationFailureLog(ex, 0);
            }
        }

        private static void RunMigrationIfPending(SqlConnection con, int version, string sql, string scriptName)
        {
            if (con == null || IsMigrationApplied(con, version))
                return;
            try
            {
                using (var tx = con.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = new SqlCommand(sql, con, tx))
                        {
                            cmd.CommandTimeout = 120;
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new SqlCommand(@"
INSERT INTO dbo.DB_VERSION (VERSION_NO, SCRIPT_NAME)
VALUES (@V, @N);", con, tx))
                        {
                            cmd.Parameters.Add("@V", SqlDbType.Int).Value = version;
                            cmd.Parameters.Add("@N", SqlDbType.NVarChar, 400).Value = scriptName;
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch (Exception)
                    {
                        try { tx.Rollback(); } catch { /* ignore */ }
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMigrationFailureLog(ex, version);
            }
        }

        private static void WriteMigrationFailureLog(Exception ex, int versionNo)
        {
            if (ex == null)
                return;
            try
            {
                string baseDir = Application.StartupPath;
                if (string.IsNullOrWhiteSpace(baseDir))
                    baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string path = Path.Combine(baseDir, "SchemaMigrationLog.txt");
                string blok = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")
                    + " Veritabani migrasyonu basarisiz (VERSION_NO=" + versionNo + ")\r\n"
                    + ex.ToString()
                    + "\r\n--------------------\r\n";
                File.AppendAllText(path, blok);
            }
            catch
            {
                // dosya yazilamazsa yoksay
            }
        }

        private static void EnsureDbVersionTable(SqlConnection con)
        {
            const string sql = @"
IF OBJECT_ID('dbo.DB_VERSION','U') IS NULL
CREATE TABLE dbo.DB_VERSION(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  VERSION_NO INT NOT NULL,
  SCRIPT_NAME NVARCHAR(400) NOT NULL,
  APPLIED_ON DATETIME NOT NULL DEFAULT(GETDATE())
);";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandTimeout = 60;
                cmd.ExecuteNonQuery();
            }
        }

        private static bool IsMigrationApplied(SqlConnection con, int versionNo)
        {
            if (con == null)
                return false;
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM dbo.DB_VERSION WHERE VERSION_NO = @V;", con))
            {
                cmd.Parameters.Add("@V", SqlDbType.Int).Value = versionNo;
                object o = cmd.ExecuteScalar();
                return o != null && o != DBNull.Value && Convert.ToInt32(o) > 0;
            }
        }

        private static string GetMigration20260202Sql()
        {
            return @"
IF OBJECT_ID('dbo.PERSONEL','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.PERSONEL','EV_ADRESI') IS NULL ALTER TABLE dbo.PERSONEL ADD EV_ADRESI NVARCHAR(400) NULL;
  IF COL_LENGTH('dbo.PERSONEL','GSM_1') IS NULL ALTER TABLE dbo.PERSONEL ADD GSM_1 NVARCHAR(40) NULL;
  IF COL_LENGTH('dbo.PERSONEL','KIM_BABA_ADI') IS NULL ALTER TABLE dbo.PERSONEL ADD KIM_BABA_ADI NVARCHAR(200) NULL;
  IF COL_LENGTH('dbo.PERSONEL','KIM_ANA_ADI') IS NULL ALTER TABLE dbo.PERSONEL ADD KIM_ANA_ADI NVARCHAR(200) NULL;
  IF COL_LENGTH('dbo.PERSONEL','KIM_DOGUM_YERI') IS NULL ALTER TABLE dbo.PERSONEL ADD KIM_DOGUM_YERI NVARCHAR(200) NULL;
END

IF OBJECT_ID('dbo.KURSIYER','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.KURSIYER','CINSIYET') IS NULL ALTER TABLE dbo.KURSIYER ADD CINSIYET VARCHAR(20) NULL;
  IF COL_LENGTH('dbo.KURSIYER','TAHSILI') IS NULL ALTER TABLE dbo.KURSIYER ADD TAHSILI NVARCHAR(100) NULL;
  IF COL_LENGTH('dbo.KURSIYER','EV_IL') IS NULL ALTER TABLE dbo.KURSIYER ADD EV_IL NVARCHAR(50) NULL;
  IF COL_LENGTH('dbo.KURSIYER','EV_ILCE') IS NULL ALTER TABLE dbo.KURSIYER ADD EV_ILCE NVARCHAR(100) NULL;
  IF COL_LENGTH('dbo.KURSIYER','IS_ADRESI') IS NULL ALTER TABLE dbo.KURSIYER ADD IS_ADRESI NVARCHAR(500) NULL;
  IF COL_LENGTH('dbo.KURSIYER','IS_TELEFON_1') IS NULL ALTER TABLE dbo.KURSIYER ADD IS_TELEFON_1 NVARCHAR(40) NULL;
  IF COL_LENGTH('dbo.KURSIYER','KALANBORC') IS NULL ALTER TABLE dbo.KURSIYER ADD KALANBORC MONEY NULL;
  IF COL_LENGTH('dbo.KURSIYER','TOPLAM_BORC') IS NULL ALTER TABLE dbo.KURSIYER ADD TOPLAM_BORC MONEY NULL;
  IF COL_LENGTH('dbo.KURSIYER','TOPLAM_ODENEN') IS NULL ALTER TABLE dbo.KURSIYER ADD TOPLAM_ODENEN MONEY NULL;
END

IF OBJECT_ID('dbo.KURSIYERLER','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.KURSIYERLER','CINSIYET') IS NULL ALTER TABLE dbo.KURSIYERLER ADD CINSIYET VARCHAR(20) NULL;
  IF COL_LENGTH('dbo.KURSIYERLER','TAHSILI') IS NULL ALTER TABLE dbo.KURSIYERLER ADD TAHSILI NVARCHAR(100) NULL;
  IF COL_LENGTH('dbo.KURSIYERLER','IS_ADRESI') IS NULL ALTER TABLE dbo.KURSIYERLER ADD IS_ADRESI NVARCHAR(500) NULL;
  IF COL_LENGTH('dbo.KURSIYERLER','IS_TELEFON_1') IS NULL ALTER TABLE dbo.KURSIYERLER ADD IS_TELEFON_1 NVARCHAR(40) NULL;
END

IF OBJECT_ID('dbo.KursBilgiParam','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.KursBilgiParam','KURUM_KODU') IS NULL ALTER TABLE dbo.KursBilgiParam ADD KURUM_KODU VARCHAR(50) NULL;
  IF COL_LENGTH('dbo.KursBilgiParam','MUDUR_ADI') IS NULL ALTER TABLE dbo.KursBilgiParam ADD MUDUR_ADI VARCHAR(200) NULL;
  IF COL_LENGTH('dbo.KursBilgiParam','KURUCU_ADI') IS NULL ALTER TABLE dbo.KursBilgiParam ADD KURUCU_ADI VARCHAR(200) NULL;
  IF COL_LENGTH('dbo.KursBilgiParam','KURS_ADI_KISA') IS NULL ALTER TABLE dbo.KursBilgiParam ADD KURS_ADI_KISA VARCHAR(100) NULL;
  IF COL_LENGTH('dbo.KursBilgiParam','SOZLESME_BANKA_HESAPNO') IS NULL ALTER TABLE dbo.KursBilgiParam ADD SOZLESME_BANKA_HESAPNO VARCHAR(400) NULL;
  IF COL_LENGTH('dbo.KursBilgiParam','PK') IS NULL ALTER TABLE dbo.KursBilgiParam ADD PK VARCHAR(10) NULL;
  IF COL_LENGTH('dbo.KursBilgiParam','WEB') IS NULL ALTER TABLE dbo.KursBilgiParam ADD WEB VARCHAR(200) NULL;
  IF COL_LENGTH('dbo.KursBilgiParam','E_POSTA') IS NULL ALTER TABLE dbo.KursBilgiParam ADD E_POSTA VARCHAR(150) NULL;
END

IF OBJECT_ID('dbo.KursBilgiParam','U') IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.KursBilgiParam)
BEGIN
  IF COL_LENGTH('dbo.KursBilgiParam','KURS_ADI_KISA') IS NOT NULL AND COL_LENGTH('dbo.KursBilgiParam','MUSTERI_NO') IS NOT NULL AND COL_LENGTH('dbo.KursBilgiParam','KURUM_KODU') IS NOT NULL
    INSERT INTO dbo.KursBilgiParam (KURS_ADI, KURUM_KODU, MUDUR_ADI, ADRES, IL, ILCE, TELEFON, GSM, KURS_ADI_KISA, MUSTERI_NO)
    VALUES (N'KOLERA MTSK', N'1234', N'', N'', N'', N'', N'', N'', N'KOLERA', N'1234');
  ELSE IF COL_LENGTH('dbo.KursBilgiParam','KURS_ADI_KISA') IS NOT NULL AND COL_LENGTH('dbo.KursBilgiParam','MUSTERI_NO') IS NOT NULL
    INSERT INTO dbo.KursBilgiParam (KURS_ADI, MUDUR_ADI, ADRES, IL, ILCE, TELEFON, GSM, KURS_ADI_KISA, MUSTERI_NO)
    VALUES (N'KOLERA MTSK', N'', N'', N'', N'', N'', N'', N'KOLERA', N'1234');
  ELSE IF COL_LENGTH('dbo.KursBilgiParam','KURS_ADI_KISA') IS NOT NULL
    INSERT INTO dbo.KursBilgiParam (KURS_ADI, MUDUR_ADI, ADRES, IL, ILCE, TELEFON, GSM, KURS_ADI_KISA)
    VALUES (N'KOLERA MTSK', N'', N'', N'', N'', N'', N'', N'KOLERA');
  ELSE IF COL_LENGTH('dbo.KursBilgiParam','MUSTERI_NO') IS NOT NULL
    INSERT INTO dbo.KursBilgiParam (KURS_ADI, ADRES, ILCE, IL, TELEFON, GSM, MUSTERI_NO)
    VALUES (N'KOLERA MTSK', N'', N'', N'', N'', N'', N'1234');
  ELSE
    INSERT INTO dbo.KursBilgiParam (KURS_ADI, ADRES, ILCE, IL, TELEFON, GSM)
    VALUES (N'KOLERA MTSK', N'', N'', N'', N'', N'');
END

IF OBJECT_ID('dbo.RAPOR_TANIMLARI','U') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','RAPOR_YOLU') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','RAPOR_GRUBU') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','SIRA_NO') IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM dbo.RAPOR_TANIMLARI
    WHERE RAPOR_GRUBU = N'PERSONEL FORMLARI' AND RAPOR_ADI = N'PERSONEL MURACAAT FORMU')
BEGIN
  DECLARE @siraNo INT;
  SELECT @siraNo = ISNULL(MAX(SIRA_NO), 0) + 1 FROM dbo.RAPOR_TANIMLARI;
  INSERT INTO dbo.RAPOR_TANIMLARI (RAPOR_GRUBU, RAPOR_ADI, RAPOR_YOLU, SIRA_NO, AKTIF, RENK, SABLON_BINARY, OLUSTURMA_TARIHI, GUNCELLEME_TARIHI)
  VALUES (N'PERSONEL FORMLARI', N'PERSONEL MURACAAT FORMU', N'C:\Raporlar\PersonelMuracaatFormu.frx', @siraNo, 1, 0, NULL, GETDATE(), GETDATE());
END

IF OBJECT_ID('dbo.RAPOR_TANIMLARI','U') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','RAPOR_YOLU') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','RAPOR_GRUBU') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','SIRA_NO') IS NOT NULL
BEGIN
  UPDATE dbo.RAPOR_TANIMLARI
  SET RAPOR_ADI = N'RANDEVU MEHMET',
      RAPOR_YOLU = N'C:\Raporlar\DireksiyonEgitimiSinavTakipSonucListesi.frx',
      GUNCELLEME_TARIHI = GETDATE()
  WHERE RAPOR_GRUBU = N'SINAV LISTESI - DIREKSIYON'
    AND RAPOR_ADI = N'DIREKSIYON EGITIMI SINAV TAKIP VE SONUC LISTESI';
END

IF OBJECT_ID('dbo.RAPOR_TANIMLARI','U') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','RAPOR_YOLU') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','RAPOR_GRUBU') IS NOT NULL
  AND COL_LENGTH('dbo.RAPOR_TANIMLARI','SIRA_NO') IS NOT NULL
  AND NOT EXISTS (
    SELECT 1 FROM dbo.RAPOR_TANIMLARI
    WHERE RAPOR_GRUBU = N'SINAV LISTESI - DIREKSIYON'
      AND RAPOR_ADI = N'RANDEVU MEHMET')
BEGIN
  DECLARE @siraNo2 INT;
  SELECT @siraNo2 = ISNULL(MAX(SIRA_NO), 0) + 1 FROM dbo.RAPOR_TANIMLARI;
  INSERT INTO dbo.RAPOR_TANIMLARI (RAPOR_GRUBU, RAPOR_ADI, RAPOR_YOLU, SIRA_NO, AKTIF, RENK, SABLON_BINARY, OLUSTURMA_TARIHI, GUNCELLEME_TARIHI)
  VALUES (N'SINAV LISTESI - DIREKSIYON', N'RANDEVU MEHMET', N'C:\Raporlar\DireksiyonEgitimiSinavTakipSonucListesi.frx', @siraNo2, 1, 0, NULL, GETDATE(), GETDATE());
END
";
        }

        /// <summary>
        /// COL_LENGTH NVARCHAR icin bayt cinsindendir (n karakter = 2n bayt).
        /// </summary>
        private static string GetMigration20260502KursiyerAdresSql()
        {
            return @"
IF OBJECT_ID('dbo.KURSIYER','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.KURSIYER','EV_ADRESI') IS NULL
    ALTER TABLE dbo.KURSIYER ADD EV_ADRESI NVARCHAR(500) NULL;
  ELSE IF COL_LENGTH('dbo.KURSIYER','EV_ADRESI') < 1000
    ALTER TABLE dbo.KURSIYER ALTER COLUMN EV_ADRESI NVARCHAR(500) NULL;

  IF COL_LENGTH('dbo.KURSIYER','EV_IL') IS NULL
    ALTER TABLE dbo.KURSIYER ADD EV_IL NVARCHAR(50) NULL;
  ELSE IF COL_LENGTH('dbo.KURSIYER','EV_IL') < 100
    ALTER TABLE dbo.KURSIYER ALTER COLUMN EV_IL NVARCHAR(50) NULL;

  IF COL_LENGTH('dbo.KURSIYER','EV_ILCE') IS NULL
    ALTER TABLE dbo.KURSIYER ADD EV_ILCE NVARCHAR(100) NULL;
  ELSE IF COL_LENGTH('dbo.KURSIYER','EV_ILCE') < 200
    ALTER TABLE dbo.KURSIYER ALTER COLUMN EV_ILCE NVARCHAR(100) NULL;

  IF COL_LENGTH('dbo.KURSIYER','IS_ADRESI') IS NULL
    ALTER TABLE dbo.KURSIYER ADD IS_ADRESI NVARCHAR(500) NULL;
  ELSE IF COL_LENGTH('dbo.KURSIYER','IS_ADRESI') < 1000
    ALTER TABLE dbo.KURSIYER ALTER COLUMN IS_ADRESI NVARCHAR(500) NULL;
END

IF OBJECT_ID('dbo.KURSIYERLER','U') IS NOT NULL
BEGIN
  IF COL_LENGTH('dbo.KURSIYERLER','IS_ADRESI') IS NULL
    ALTER TABLE dbo.KURSIYERLER ADD IS_ADRESI NVARCHAR(500) NULL;
  ELSE   IF COL_LENGTH('dbo.KURSIYERLER','IS_ADRESI') < 1000
    ALTER TABLE dbo.KURSIYERLER ALTER COLUMN IS_ADRESI NVARCHAR(500) NULL;
END
";
        }

        private static string GetMigration20260503SmsSablonlariSql()
        {
            return @"
IF OBJECT_ID('dbo.SMSSABLONLARI','U') IS NULL
CREATE TABLE dbo.SMSSABLONLARI(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  SABLON_ADI NVARCHAR(200) NOT NULL,
  SABLON_METNI NVARCHAR(MAX) NOT NULL,
  SIRA_NO INT NOT NULL CONSTRAINT DF_SMSSABLONLARI_SIRA DEFAULT(0),
  AKTIF BIT NOT NULL CONSTRAINT DF_SMSSABLONLARI_AKTIF DEFAULT(1),
  OLUSTURMA_TARIHI DATETIME2(7) NOT NULL CONSTRAINT DF_SMSSABLONLARI_OLUSTUR DEFAULT(SYSUTCDATETIME()),
  GUNCELLEME_TARIHI DATETIME2(7) NULL
);

IF OBJECT_ID('dbo.SMSSABLONLARI','U') IS NOT NULL
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
END
";
        }

        private static string GetMigration20260504KoleraIstampaSql()
        {
            return @"
IF OBJECT_ID('dbo.KOLERA_ISTAMPA','U') IS NULL
CREATE TABLE dbo.KOLERA_ISTAMPA(
  ID INT IDENTITY(1,1) PRIMARY KEY,
  ALAN_KODU NVARCHAR(50) NOT NULL,
  ALAN_ADI NVARCHAR(100) NOT NULL,
  RESIM VARBINARY(MAX) NULL,
  ACIKLAMA NVARCHAR(250) NULL,
  GUNCELLEME_TARIHI DATETIME NULL
);

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
";
        }
    }
}
