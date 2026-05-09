using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net;
using System.Text;
using System.Globalization;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class LisansDetayForm : Form
    {
        private readonly string _connectionString;
        private readonly object _lisans;
        private const string LisansAdminSifre = "Kolera@2026";
        private const string WebLisansUrl = "http://mehmetfer.com.tr/lisans.php?token=KOLERA2026";

        public LisansDetayForm(string connectionString, object lisans)
        {
            InitializeComponent();
            _connectionString = connectionString;
            _lisans = lisans;
            Load += LisansDetayForm_Load;
        }

        private void LisansDetayForm_Load(object sender, EventArgs e)
        {
            KurumsalTasarimiUygula();
            ApplyLisansData();
            ApplyKurumData();
            KalanGunHesapla();
            ButonDurumlariniGuncelle();
            LogYaz("Lisans ekranı açıldı.");
        }

        private void ApplyLisansData()
        {
            var localNo = GetLocalLisansNo();
            if (_lisans == null)
            {
                lblLisansNoValue.Text = string.IsNullOrWhiteSpace(localNo) ? "-" : localNo;
                lblDurumValue.Text = "DEMO";
                lblDurumValue.BackColor = Color.Orange;
                lblBitisValue.Text = "-";
                if (lblDurumRozet != null)
                {
                    lblDurumRozet.Text = "DEMO";
                    lblDurumRozet.BackColor = LisansDurumRengi("DEMO");
                }
                return;
            }

            var lisansNo = GetProp<string>(_lisans, "LisansNo");
            var durum = GetProp<string>(_lisans, "Durum");
            var validUntil = GetProp<DateTime>(_lisans, "ValidUntil");

            lblLisansNoValue.Text = !string.IsNullOrWhiteSpace(localNo)
                ? localNo
                : (string.IsNullOrWhiteSpace(lisansNo) ? "-" : lisansNo);
            lblDurumValue.Text = string.IsNullOrWhiteSpace(durum) ? "-" : durum.ToUpperInvariant();
            lblDurumValue.BackColor = LisansDurumRengi(durum);
            lblBitisValue.Text = validUntil == DateTime.MinValue ? "-" : validUntil.ToString("dd.MM.yyyy");
            if (lblDurumRozet != null)
            {
                lblDurumRozet.Text = lblDurumValue.Text;
                lblDurumRozet.BackColor = LisansDurumRengi(lblDurumValue.Text);
            }
        }

        private void ApplyKurumData()
        {
            lblKurumKoduValue.Text = "-";
            lblKurumAdiValue.Text = "-";
            lblMusteriNoValue.Text = "-";

            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            try
            {
                string tableName = ResolveKursBilgiTableName();
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    ApplyFallbackKurumData();
                    return;
                }
                using (var conn = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter("SELECT TOP 1 * FROM [" + tableName + "] ORDER BY ID DESC", conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        ApplyFallbackKurumData();
                        return;
                    }
                    var row = dt.Rows[0];

                    lblKurumKoduValue.Text = ReadValue(row, "KURUM_KODU", "KOD", "KURUMKODU");
                    lblKurumAdiValue.Text = ReadValue(row, "KURS_ADI", "KURUM_ADI", "ADI");
                    lblMusteriNoValue.Text = ReadValue(row, "MUSTERI_NO", "MUSTERI", "MUSTERINO");
                }
            }
            catch
            {
                // Ekrani acik tut, DB sorunu olsa da tasarim gorunsun.
            }

            ApplyFallbackKurumData();

            // Web lisans servisinden firma/musteri bilgisi gelirse onu tercih et.
            var webInfo = TryGetWebLicenseInfo(lblKurumAdiValue.Text, lblLisansNoValue.Text);
            if (webInfo != null)
            {
                if (!string.IsNullOrWhiteSpace(webInfo.FirmaAdi))
                    lblKurumAdiValue.Text = webInfo.FirmaAdi;
                if (!string.IsNullOrWhiteSpace(webInfo.MusteriNo))
                    lblMusteriNoValue.Text = webInfo.MusteriNo;

                SaveKursBilgiIdentity(lblKurumAdiValue.Text, webInfo.MusteriNo);
            }
        }

        private void ApplyFallbackKurumData()
        {
            // Lisans nesnesinden gelen kurum adini ekranda son fallback olarak kullan.
            var lisansKurum = GetProp<string>(_lisans, "LisansKurum");
            SetIfMissing(lblKurumAdiValue, lisansKurum);

            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Yerel lisans cache tablosunda kurum adi/musteri no tutulabiliyor.
                    if (TableExists(conn, "APP_LOCAL_LISANS"))
                    {
                        using (var cmd = new SqlCommand(@"SELECT TOP 1
ISNULL(KURUM_ADI,'') AS KURUM_ADI,
ISNULL(MUSTERI_NO,'') AS MUSTERI_NO
FROM dbo.APP_LOCAL_LISANS
ORDER BY ID DESC;", conn))
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                SetIfMissing(lblKurumAdiValue, Convert.ToString(r["KURUM_ADI"]));
                                SetIfMissing(lblMusteriNoValue, Convert.ToString(r["MUSTERI_NO"]));
                            }
                        }
                    }

                    // Kurum kodu lisans ekraninda sadece KursBilgiParam.KURUM_KODU alanindan okunur.
                }
            }
            catch
            {
                // Fallback bilgiler okunamazsa mevcut degerler korunur.
            }
        }

        private static bool IsMissingValue(string text)
        {
            return string.IsNullOrWhiteSpace(text) || text.Trim() == "-";
        }

        private static void SetIfMissing(Label target, string value)
        {
            if (target == null || !IsMissingValue(target.Text))
                return;

            var trimmed = (value ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
                target.Text = trimmed;
        }

        private static bool TableExists(SqlConnection conn, string tableName)
        {
            using (var cmd = new SqlCommand(@"
SELECT CASE WHEN EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@t AND TABLE_TYPE='BASE TABLE'
) THEN 1 ELSE 0 END;", conn))
            {
                cmd.Parameters.AddWithValue("@t", tableName);
                var o = cmd.ExecuteScalar();
                return o != null && o != DBNull.Value && Convert.ToInt32(o) == 1;
            }
        }

        private void SaveKursBilgiIdentity(string firmaAdi, string musteriNo)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            string firma = (firmaAdi ?? string.Empty).Trim();
            string musteri = (musteriNo ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(firma) && string.IsNullOrWhiteSpace(musteri))
                return;

            try
            {
                string kursBilgiTable = ResolveKursBilgiTableName();
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (!string.IsNullOrWhiteSpace(kursBilgiTable))
                    {
                        using (var alter = new SqlCommand(@"IF COL_LENGTH('dbo." + kursBilgiTable + @"','KURUM_KODU') IS NULL
    ALTER TABLE dbo." + kursBilgiTable + " ADD KURUM_KODU VARCHAR(50) NULL;", conn))
                        {
                            alter.ExecuteNonQuery();
                        }

                        string sql = @"
UPDATE [" + kursBilgiTable + @"]
SET
  KURS_ADI = CASE WHEN LEN(ISNULL(@firma, N'')) > 0 THEN @firma ELSE KURS_ADI END,
  MUSTERI_NO = CASE WHEN LEN(ISNULL(@musteri, N'')) > 0 THEN @musteri ELSE MUSTERI_NO END
WHERE ID = (SELECT TOP 1 ID FROM [" + kursBilgiTable + @"] ORDER BY ID DESC);";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@firma", firma);
                            cmd.Parameters.AddWithValue("@musteri", musteri);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    if (TableExists(conn, "APP_LOCAL_LISANS"))
                    {
                        using (var cmd = new SqlCommand(@"UPDATE dbo.APP_LOCAL_LISANS
SET KURUM_ADI = CASE WHEN LEN(ISNULL(@firma, N'')) > 0 THEN @firma ELSE KURUM_ADI END,
    MUSTERI_NO = CASE WHEN LEN(ISNULL(@musteri, N'')) > 0 THEN @musteri ELSE MUSTERI_NO END,
    UPDATED_AT = GETDATE()
WHERE ID = (SELECT TOP 1 ID FROM dbo.APP_LOCAL_LISANS ORDER BY ID DESC);", conn))
                        {
                            cmd.Parameters.AddWithValue("@firma", firma);
                            cmd.Parameters.AddWithValue("@musteri", musteri);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch
            {
                // Lisans ekrani acik kalsin; yazma hatasi sessizce gecilir.
            }
        }

        private string ResolveKursBilgiTableName()
        {
            const string sql = @"
SELECT TOP 1 TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND UPPER(TABLE_NAME) IN ('KURSBILGIPARAM')
ORDER BY TABLE_NAME;";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    var o = cmd.ExecuteScalar();
                    return o == null || o == DBNull.Value ? null : Convert.ToString(o);
                }
            }
            catch
            {
                return null;
            }
        }

        private string ReadValue(DataRow row, params string[] columnCandidates)
        {
            foreach (var c in columnCandidates)
            {
                foreach (DataColumn dc in row.Table.Columns)
                {
                    if (!string.Equals(dc.ColumnName, c, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var v = row[dc];
                    var text = v == null || v == DBNull.Value ? string.Empty : v.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                        return text;
                }
            }
            return "-";
        }

        private T GetProp<T>(object source, string propName)
        {
            if (source == null) return default(T);
            try
            {
                var pi = source.GetType().GetProperty(propName);
                if (pi == null) return default(T);
                var raw = pi.GetValue(source, null);
                if (raw == null || raw == DBNull.Value) return default(T);
                if (raw is T casted) return casted;
                return (T)Convert.ChangeType(raw, typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        private void btnKapat_Click(object sender, EventArgs e)
        {
            LogYaz("Lisans ekranı kapatıldı.");
            Close();
        }

        private void btnGeciciDegistir_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            var pw = (txtAdminSifre.Text ?? string.Empty).Trim();
            var yeniNo = (txtYeniLisansNo.Text ?? string.Empty).Trim();

            if (pw != LisansAdminSifre)
            {
                MessageBox.Show("Admin sifresi hatali.");
                return;
            }

            if (string.IsNullOrWhiteSpace(yeniNo))
            {
                MessageBox.Show("Yeni lisans numarasini giriniz.");
                return;
            }

            var sonuc = SaveLocalLisansNo(yeniNo);
            if (!sonuc)
            {
                MessageBox.Show("Yerel veritabani guncellenemedi.");
                LogYaz("Yerel lisans güncellemesi başarısız.");
                return;
            }

            lblLisansNoValue.Text = yeniNo;
            RefreshMusteriNoFromWeb(yeniNo);

            var kurumAdi = (lblKurumAdiValue.Text ?? string.Empty).Trim();
            var musteriNo = (lblMusteriNoValue.Text ?? string.Empty).Trim();
            var bitisTarihi = ParseBitisDate(lblBitisValue.Text);
            SaveLocalLisansDetay(yeniNo, kurumAdi, bitisTarihi, musteriNo);

            MessageBox.Show("Lisans numarasi programin yerel veritabaninda guncellendi.");
            LogYaz("Yerel lisans başarıyla güncellendi: " + yeniNo);
            txtAdminSifre.Clear();
            txtYeniLisansNo.Clear();
            KalanGunHesapla();
            ButonDurumlariniGuncelle();
        }

        private void btnWebPanelAc_Click(object sender, EventArgs e)
        {
            try
            {
                var web = TryGetWebLicenseInfo(lblKurumAdiValue.Text, lblLisansNoValue.Text);
                if (web != null && (!string.IsNullOrWhiteSpace(web.FirmaAdi) || !string.IsNullOrWhiteSpace(web.MusteriNo)))
                {
                    if (!string.IsNullOrWhiteSpace(web.FirmaAdi))
                        lblKurumAdiValue.Text = web.FirmaAdi;
                    if (!string.IsNullOrWhiteSpace(web.MusteriNo))
                        lblMusteriNoValue.Text = web.MusteriNo;
                    SaveKursBilgiIdentity(lblKurumAdiValue.Text, web.MusteriNo);

                    LogYaz("Web lisans kontrolü başarılı. Firma: " + (web.FirmaAdi ?? "-") + " | Müşteri No: " + (web.MusteriNo ?? "-"));
                }
                else
                {
                    LogYaz("Web lisans kontrolü yapıldı, yeni firma/müşteri bilgisi bulunamadı.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Web lisans kontrolünde hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogYaz("Web lisans kontrolü hatası.");
            }
        }

        private string GetLocalLisansNo()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return null;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    EnsureLocalTable(conn);
                    using (var cmd = new SqlCommand("SELECT TOP 1 ISNULL(LISANS_NO,'') FROM APP_LOCAL_LISANS ORDER BY ID DESC", conn))
                    {
                        var v = cmd.ExecuteScalar();
                        var s = v == null || v == DBNull.Value ? string.Empty : v.ToString().Trim();
                        return string.IsNullOrWhiteSpace(s) ? null : s;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private bool SaveLocalLisansNo(string lisansNo)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (string.IsNullOrWhiteSpace(_connectionString))
                return false;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    EnsureLocalTable(conn);
                    using (var del = new SqlCommand("DELETE FROM APP_LOCAL_LISANS", conn))
                        del.ExecuteNonQuery();
                    using (var ins = new SqlCommand("INSERT INTO APP_LOCAL_LISANS (LISANS_NO, UPDATED_AT) VALUES (@no, GETDATE())", conn))
                    {
                        ins.Parameters.AddWithValue("@no", lisansNo);
                        ins.ExecuteNonQuery();
                    }
                }
                LisansPolitikasi.RegisterSuccessfulWrite();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SaveLocalLisansDetay(string lisansNo, string kurumAdi, DateTime? bitisTarihi, string musteriNo)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (string.IsNullOrWhiteSpace(_connectionString))
                return false;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    EnsureLocalTable(conn);
                    EnsureLocalDetailColumns(conn);

                    using (var upd = new SqlCommand(@"UPDATE APP_LOCAL_LISANS
SET KURUM_ADI = @kurumAdi,
    BITIS_TARIHI = @bitis,
    MUSTERI_NO = @musteriNo,
    UPDATED_AT = GETDATE()
WHERE LISANS_NO = @no", conn))
                    {
                        upd.Parameters.AddWithValue("@no", lisansNo ?? string.Empty);
                        upd.Parameters.AddWithValue("@kurumAdi", string.IsNullOrWhiteSpace(kurumAdi) ? (object)DBNull.Value : kurumAdi);
                        upd.Parameters.AddWithValue("@musteriNo", string.IsNullOrWhiteSpace(musteriNo) ? (object)DBNull.Value : musteriNo);
                        upd.Parameters.AddWithValue("@bitis", bitisTarihi.HasValue ? (object)bitisTarihi.Value : DBNull.Value);
                        upd.ExecuteNonQuery();
                    }
                }
                LisansPolitikasi.RegisterSuccessfulWrite();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void EnsureLocalTable(SqlConnection conn)
        {
            var sql = @"
IF OBJECT_ID('dbo.APP_LOCAL_LISANS', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.APP_LOCAL_LISANS(
        ID INT IDENTITY(1,1) PRIMARY KEY,
        LISANS_NO NVARCHAR(100) NOT NULL,
        KURUM_ADI NVARCHAR(200) NULL,
        BITIS_TARIHI DATETIME NULL,
        MUSTERI_NO NVARCHAR(100) NULL,
        UPDATED_AT DATETIME NOT NULL DEFAULT(GETDATE())
    );
END";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private void EnsureLocalDetailColumns(SqlConnection conn)
        {
            var sql = @"
IF COL_LENGTH('dbo.APP_LOCAL_LISANS', 'KURUM_ADI') IS NULL
    ALTER TABLE dbo.APP_LOCAL_LISANS ADD KURUM_ADI NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.APP_LOCAL_LISANS', 'BITIS_TARIHI') IS NULL
    ALTER TABLE dbo.APP_LOCAL_LISANS ADD BITIS_TARIHI DATETIME NULL;
IF COL_LENGTH('dbo.APP_LOCAL_LISANS', 'MUSTERI_NO') IS NULL
    ALTER TABLE dbo.APP_LOCAL_LISANS ADD MUSTERI_NO NVARCHAR(100) NULL;";
            using (var cmd = new SqlCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private WebLicenseInfo TryGetWebLicenseInfo(string kurumAdi, string lisansNo)
        {
            if (LisansPolitikasi.IsWebLicenseCallSuppressed)
                return null;

            try
            {
                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    string musteriNo = lblMusteriNoValue == null ? string.Empty : (lblMusteriNoValue.Text ?? string.Empty).Trim();
                    var trackedUrl = WebLisansUrl
                        + (WebLisansUrl.Contains("?") ? "&" : "?")
                        + "lisans_no=" + Uri.EscapeDataString((lisansNo ?? string.Empty).Trim())
                        + "&musteri_no=" + Uri.EscapeDataString(musteriNo);
                    var text = wc.DownloadString(trackedUrl);
                    if (string.IsNullOrWhiteSpace(text)) return null;

                    string normKurum = Normalize(kurumAdi);
                    string normLisans = (lisansNo ?? string.Empty).Trim();

                    var lines = text.Replace("\r", "").Split('\n');
                    foreach (var raw in lines)
                    {
                        var line = (raw ?? string.Empty).Trim();
                        if (line.Length == 0 || line.StartsWith("#")) continue;

                        var p = line.Split('|');
                        if (p.Length < 6) continue;

                        var firma = Normalize(p[0]);
                        var ln = (p[1] ?? string.Empty).Trim();
                        if (!(firma == normKurum || firma.Contains(normKurum) || normKurum.Contains(firma) || ln == normLisans))
                            continue;

                        // Beklenen yeni format: Firma|LisansNo|Bitis|Urun|Surum|Durum|MusteriNo
                        string firmaAdi = (p[0] ?? string.Empty).Trim();
                        if (p.Length >= 7)
                        {
                            var m = (p[6] ?? string.Empty).Trim();
                            if (string.IsNullOrWhiteSpace(m) && string.IsNullOrWhiteSpace(firmaAdi))
                                return null;
                            return new WebLicenseInfo
                            {
                                FirmaAdi = string.IsNullOrWhiteSpace(firmaAdi) ? null : firmaAdi,
                                MusteriNo = string.IsNullOrWhiteSpace(m) ? null : m
                            };
                        }
                        if (string.IsNullOrWhiteSpace(firmaAdi))
                            return null;
                        return new WebLicenseInfo { FirmaAdi = firmaAdi, MusteriNo = null };
                    }
                }
            }
            catch
            {
                // Web okunamazsa mevcut yerel degeri koru.
            }
            return null;
        }

        private void RefreshMusteriNoFromWeb(string lisansNo)
        {
            var kurumAdi = lblKurumAdiValue == null ? string.Empty : (lblKurumAdiValue.Text ?? string.Empty).Trim();
            var webInfo = TryGetWebLicenseInfo(kurumAdi, lisansNo);
            if (webInfo == null)
                return;

            if (!string.IsNullOrWhiteSpace(webInfo.FirmaAdi))
                lblKurumAdiValue.Text = webInfo.FirmaAdi;
            if (!string.IsNullOrWhiteSpace(webInfo.MusteriNo))
                lblMusteriNoValue.Text = webInfo.MusteriNo;
        }

        private class WebLicenseInfo
        {
            public string FirmaAdi { get; set; }
            public string MusteriNo { get; set; }
        }

        private DateTime? ParseBitisDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            if (DateTime.TryParseExact(text.Trim(), "dd.MM.yyyy", new System.Globalization.CultureInfo("tr-TR"),
                System.Globalization.DateTimeStyles.None, out var dt))
                return dt;
            return null;
        }

        private string Normalize(string t)
        {
            if (string.IsNullOrWhiteSpace(t)) return string.Empty;
            t = t.ToUpper(new System.Globalization.CultureInfo("tr-TR")).Trim();
            t = t.Replace("ÖZEL ", "").Replace(" MOTORLU TAŞIT SÜRÜCÜLERİ KURSU", "").Replace(" MOTORLU TASIT SURUCULERI KURSU", "");
            while (t.Contains("  ")) t = t.Replace("  ", " ");
            return t.Trim();
        }

        private void KurumsalTasarimiUygula()
        {
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);
            pnlHeader.BackColor = Color.FromArgb(192, 0, 0);
            pnlMain.BackColor = BackColor;
            pnlSummaryCards.BackColor = BackColor;
            pnlKurumBilgileri.BackColor = Color.White;
            pnlLisansIslemleri.BackColor = Color.White;
            pnlLog.BackColor = Color.White;
            cardLisansNo.BackColor = Color.White;
            cardDurum.BackColor = Color.White;
            cardBitis.BackColor = Color.White;

            lblHeaderTitle.ForeColor = Color.White;
            lblHeaderTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            lblHeaderSub.ForeColor = Color.FromArgb(255, 228, 228);
            lblHeaderSub.Font = new Font("Segoe UI", 9.5F);

            lblLisansNoValue.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            lblDurumValue.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            lblBitisValue.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);

            lblDurumRozet.ForeColor = Color.White;
            lblDurumRozet.BackColor = LisansDurumRengi(lblDurumValue.Text);
            lblDurumRozet.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);

            StilButon(btnGeciciDegistir, Color.FromArgb(46, 125, 50));
            StilButon(btnWebPanelAc, Color.FromArgb(52, 73, 94));
            StilButon(btnKapat, Color.FromArgb(192, 0, 0));

            rtbLog.ReadOnly = true;
            rtbLog.Font = new Font("Segoe UI", 9F);
        }

        private void LogYaz(string mesaj)
        {
            if (rtbLog == null)
                return;
            rtbLog.AppendText("[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + mesaj + Environment.NewLine);
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.ScrollToCaret();
        }

        private void ButonDurumlariniGuncelle()
        {
            btnWebPanelAc.Enabled = !string.IsNullOrWhiteSpace(lblLisansNoValue.Text) && lblLisansNoValue.Text != "-";
        }

        private void KalanGunHesapla()
        {
            if (lblKalanGunValue == null)
                return;

            DateTime bitis;
            if (!DateTime.TryParseExact((lblBitisValue.Text ?? string.Empty).Trim(), "dd.MM.yyyy", new CultureInfo("tr-TR"), DateTimeStyles.None, out bitis))
            {
                lblKalanGunValue.Text = "Kalan Gün: -";
                lblKalanGunValue.ForeColor = Color.FromArgb(52, 73, 94);
                return;
            }

            int kalan = (int)Math.Floor((bitis.Date - DateTime.Today).TotalDays);
            if (kalan < 0)
            {
                lblKalanGunValue.Text = "Kalan Gün: Süresi Geçmiş";
                lblKalanGunValue.ForeColor = Color.FromArgb(192, 0, 0);
                LogYaz("Lisans süresi geçmiş görünüyor.");
            }
            else if (kalan < 30)
            {
                lblKalanGunValue.Text = "Kalan Gün: " + kalan + " (Uyarı)";
                lblKalanGunValue.ForeColor = Color.FromArgb(230, 126, 34);
            }
            else
            {
                lblKalanGunValue.Text = "Kalan Gün: " + kalan;
                lblKalanGunValue.ForeColor = Color.FromArgb(46, 125, 50);
            }
        }

        private Color LisansDurumRengi(string durum)
        {
            var d = (durum ?? string.Empty).Trim().ToUpperInvariant();
            if (d == "AKTIF" || d == "AKTİF")
                return Color.FromArgb(46, 125, 50);
            if (d == "DEMO")
                return Color.FromArgb(230, 126, 34);
            return Color.FromArgb(192, 0, 0);
        }

        private static void StilButon(Button btn, Color color)
        {
            if (btn == null) return;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btn.Margin = new Padding(0, 6, 0, 0);
        }
    }
}
