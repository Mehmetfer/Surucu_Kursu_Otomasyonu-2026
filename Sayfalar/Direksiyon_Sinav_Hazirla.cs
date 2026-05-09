using Kolera.Direksiyonsinavi.Hazirla;
using Kolera.Direksiyonsinavi.Hazirla.Interfaces;
using Kolera.Mebbis.Services;
using Kolera_Mtsk.Services;
using FastReport;
using FastReport.Export.PdfSimple;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using static Kolera_Mtsk.Sayfalar.Arama_Sayfam;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Direksiyon_Sinav_Hazirla : Form
    {
        private readonly DireksiyonSinavHazirlaService _service;
        private readonly MebbisService _mebbisService;
        private readonly string _connectionString;
        private bool yukleniyor;
        private string _mebbisKullaniciAdi;
        private string _mebbisSifre;
        private DateTime _lastMebbisLoginAttempt = DateTime.MinValue;
        private List<DireksiyonSinavModel> _currentListe = new List<DireksiyonSinavModel>();
        private readonly Dictionary<int, string> _personelMap = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _aracMap = new Dictionary<int, string>();
        private string _lastSortColumn = string.Empty;
        private bool _lastSortAsc = true;
        private int _seciliSinavTarihiId = 0;
        private Label _lblAktifFiltreBilgi;
        private sealed class MebbisDireksiyonSonuc
        {
            public string TcNo { get; set; }
            public DateTime SinavTarihi { get; set; }
            public string PuanDurumu { get; set; }
            public string OnayDurumu { get; set; }
            public string SinavSonucu { get; set; }
        }
        private sealed class MebbisRandevuBilgi
        {
            public string TcNo { get; set; }
            public string AracPlaka { get; set; }
            public string UstaOgretici { get; set; }
            public DateTime SinavTarihi { get; set; }
            public string SinavSaati { get; set; }
        }
        private sealed class RandevuAktarimKaydi
        {
            public int KayitId { get; set; }
            public string TcNo { get; set; }
            public string AdSoyad { get; set; }
            public int? AracId { get; set; }
            public int? PersonelId { get; set; }
            public string Saat { get; set; }
            public DateTime SinavTarihi { get; set; }
        }

        public Direksiyon_Sinav_Hazirla() : this(string.Empty)
        {
        }

        public Direksiyon_Sinav_Hazirla(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? string.Empty;
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                _service = null;
                _mebbisService = null;
                return;
            }
            _service = new DireksiyonSinavHazirlaService(connectionString);
            _mebbisService = new MebbisService(connectionString);
            Load += Direksiyon_Sinav_Hazirla_Load;
            Btn_EKLE.Click += Btn_Ekle_Click;
            Btn_ADAYSIL.Click += Btn_Adaysil_Click;
            Btn_Durum_Cek.Click += Btn_Durum_Cek_Click;
            Btn_Yeni.Click += Btn_Yeni_Click;
            Btn_TarihSil.Click += Btn_TarihSil_Click;
            Btn_SMS_Hazirla.Click += Btn_SMS_Hazirla_Click;
            Btn_Rapor_Al.Click += Btn_Rapor_Al_Click;
            button1.Click += Btn_SinavTarihiKaydet_Click;
            Dvg_Sinavlar.SelectionChanged += Dvg_Sinavlar_SelectionChanged;
            TryBindRandevuCekButton();
            Btn_Meb_Randevu_Ac.Click += Btn_Meb_Randevu_Ac_Click;
            TryBindRandevuAktarButton();
        }

        private void Direksiyon_Sinav_Hazirla_Load(object sender, EventArgs e)
        {
            GridAyarla();
            HazirlaMebbisTab();
            HazirlaRandevuTab();
            HazirlaSinavOlusturTab();
            Combo_Sinavlar.DropDownStyle = ComboBoxStyle.DropDownList;
            Combo_Sinavlar.DisplayMember = "SINAV_TARIHI";
            Combo_Sinavlar.ValueMember = "ID";
            Combo_Sinavlar.Format -= Combo_Sinavlar_Format;
            Combo_Sinavlar.Format += Combo_Sinavlar_Format;

            Combo_Sinavlar.DataSource = GetSinavTarihleri();
            Combo_Sinavlar.SelectedIndex = -1;
            Combo_Sinavlar.SelectedIndexChanged += Combo_Sinavlar_SelectedIndexChanged;
            Combo_Sinavlar.DropDown -= Combo_Sinavlar_DropDown;
            Combo_Sinavlar.DropDown += Combo_Sinavlar_DropDown;
            EnsureAktifFiltreBilgiLabel();

            // Form acildiginda secili sinav olmazsa grid bos kaliyor.
            // Ilk kaydi otomatik secip listeyi hemen yukle.
            if (Combo_Sinavlar.Items.Count > 0)
                Combo_Sinavlar.SelectedIndex = 0;
            else
                _ = RefreshGridAsync();

            Dgv_Listesi.CurrentCellDirtyStateChanged += (s, ev) =>
            {
                if (Dgv_Listesi.IsCurrentCellDirty)
                    Dgv_Listesi.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            Dgv_Listesi.CellValueChanged += async (s, ev) => await Grid_CellValueChanged(ev);
            Dgv_Listesi.DataError += (s, ev) => ev.ThrowException = false;
            Dgv_Listesi.ColumnHeaderMouseClick -= Dgv_Listesi_ColumnHeaderMouseClick;
            Dgv_Listesi.ColumnHeaderMouseClick += Dgv_Listesi_ColumnHeaderMouseClick;
        }

        private async void Combo_Sinavlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await RefreshGridAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sınav listesi yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private void Btn_Rapor_Al_Click(object sender, EventArgs e)
        {
            int sinavId = 0;
            string baslik = string.Empty;
            DateTime? seciliTarih = GetSelectedSinavDate();
            sinavId = ResolveAktifSinavIdForRapor(seciliTarih);

            if (seciliTarih.HasValue)
                baslik = seciliTarih.Value.ToString("dd.MM.yyyy");
            else
                baslik = (Combo_Sinavlar.Text ?? string.Empty).Trim();

            if (sinavId <= 0)
            {
                MessageBox.Show("Rapor için geçerli sınav seçilemedi. Lütfen sınav tarihini tekrar seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(baslik))
                baslik = "SINAV TARIHI: " + baslik;
            else
                baslik = "SINAV LISTESI";

            using (var raporDetay = new RaporDetay(_connectionString, "SINAV LISTESI", sinavId, "SINAV", baslik))
            {
                raporDetay.ShowDialog(this);
            }
        }

        private bool TryShowRandevuMehmetDirectReport(DateTime? seciliTarih)
        {
            try
            {
                string frxPath = ResolveRandevuMehmetFrxPath();
                if (string.IsNullOrWhiteSpace(frxPath) || !File.Exists(frxPath))
                    return false;

                DataTable kursiyer = BuildRandevuMehmetKursiyerTablo();
                if (kursiyer.Rows.Count == 0)
                    return false;

                DataTable kurs = KursRaporKursTablosu.Olustur(_connectionString);
                if (kurs == null)
                    kurs = new DataTable("KURS");
                if (!kurs.Columns.Contains("KURS_ADI"))
                    kurs.Columns.Add("KURS_ADI", typeof(string));
                if (!kurs.Columns.Contains("RAPOR_TARIHI"))
                    kurs.Columns.Add("RAPOR_TARIHI", typeof(DateTime));
                if (!kurs.Columns.Contains("RAPOR_SAATI"))
                    kurs.Columns.Add("RAPOR_SAATI", typeof(string));
                if (kurs.Rows.Count == 0)
                    kurs.Rows.Add(kurs.NewRow());

                kurs.Rows[0]["RAPOR_TARIHI"] = (object)(seciliTarih ?? DateTime.Today);
                kurs.Rows[0]["RAPOR_SAATI"] = DateTime.Now.ToString("HH:mm:ss");

                using (var report = new Report())
                {
                    report.Load(frxPath);
                    report.RegisterData(kurs, "KURS");
                    report.RegisterData(kursiyer, "KURSIYER");

                    var dsKurs = report.GetDataSource("KURS");
                    if (dsKurs != null) dsKurs.Enabled = true;
                    var dsKursiyer = report.GetDataSource("KURSIYER");
                    if (dsKursiyer != null) dsKursiyer.Enabled = true;

                    report.Prepare();
                    string pdfPath = Path.Combine(Path.GetTempPath(), "randevu_mehmet_" + DateTime.Now.ToString("yyyyMMdd_HHmmssfff") + ".pdf");
                    using (var pdf = new PDFSimpleExport())
                    {
                        report.Export(pdf, pdfPath);
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = pdfPath,
                        UseShellExecute = true
                    });
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private string ResolveRandevuMehmetFrxPath()
        {
            string[] dosyalar =
            {
                "KULLANILANDireksiyonEgitimiSinavTakipSonucListesi.frx",
                "DireksiyonEgitimiSinavTakipSonucListesi.frx"
            };
            string[] klasorler =
            {
                Path.Combine(Application.StartupPath, "Raporlar"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "Raporlar"),
                @"C:\Raporlar"
            };

            foreach (string klasor in klasorler)
            {
                foreach (string dosya in dosyalar)
                {
                    string yol = Path.Combine(klasor ?? string.Empty, dosya);
                    if (!string.IsNullOrWhiteSpace(yol) && File.Exists(yol))
                        return yol;
                }
            }

            return string.Empty;
        }

        private DataTable BuildRandevuMehmetKursiyerTablo()
        {
            var dt = new DataTable("KURSIYER");
            dt.Columns.Add("TC_NO", typeof(string));
            dt.Columns.Add("ADI", typeof(string));
            dt.Columns.Add("SOYADI", typeof(string));
            dt.Columns.Add("SERTIFIKA_SINIFI", typeof(string));
            dt.Columns.Add("SINAV_SAATI", typeof(string));
            dt.Columns.Add("TEO_DURUM", typeof(string));
            var eklenenAnahtarlar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var eklenenKursiyerIdler = new HashSet<int>();
            bool gridSatiriVar = Dgv_Listesi != null && Dgv_Listesi.Rows.Cast<DataGridViewRow>().Any(r => r != null && !r.IsNewRow);
            Dictionary<int, string> tcMapFromDb = new Dictionary<int, string>();

            if (gridSatiriVar && Dgv_Listesi != null)
            {
                var ids = Dgv_Listesi.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r != null && !r.IsNewRow)
                    .Select(r => r.DataBoundItem as DireksiyonSinavModel)
                    .Where(m => m != null && m.ID_KURSIYER > 0)
                    .Select(m => m.ID_KURSIYER)
                    .Distinct()
                    .ToList();

                if (ids.Count > 0)
                    tcMapFromDb = GetKursiyerTcMap(ids);
            }

            // Oncelik: ekranda gorunen DataGrid satirlari.
            if (Dgv_Listesi != null && Dgv_Listesi.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in Dgv_Listesi.Rows)
                {
                    if (row == null || row.IsNewRow)
                        continue;

                    var model = row.DataBoundItem as DireksiyonSinavModel;
                    int kursiyerId = model == null ? 0 : model.ID_KURSIYER;
                    if (kursiyerId > 0 && eklenenKursiyerIdler.Contains(kursiyerId))
                        continue;

                    string tc = GridHucreDegeri(row, "TC_NO", "TcNo", "RandevuTcNo");
                    string ad = GridHucreDegeri(row, "ADI", "Ad");
                    string soyad = GridHucreDegeri(row, "SOYADI", "Soyad");
                    string adSoyad = GridHucreDegeri(row, "ADSOYAD", "RandevuAdiSoyadi", "AdSoyad");

                    if (model != null)
                    {
                        // Grid kolon isimleri ortamdan ortama degisebildigi icin once modele bak.
                        if (string.IsNullOrWhiteSpace(tc))
                            tc = GetPropValue(model, "TC_NO", "TcNo", "RandevuTcNo");
                        if (string.IsNullOrWhiteSpace(adSoyad))
                            adSoyad = GetPropValue(model, "RandevuAdiSoyadi", "AdiSoyadi", "AdSoyad");
                        if (string.IsNullOrWhiteSpace(ad))
                            ad = GetPropValue(model, "ADI", "Ad");
                        if (string.IsNullOrWhiteSpace(soyad))
                            soyad = GetPropValue(model, "SOYADI", "Soyad");
                    }

                    if (string.IsNullOrWhiteSpace(ad) && !string.IsNullOrWhiteSpace(adSoyad))
                    {
                        int idx = adSoyad.IndexOf(' ');
                        if (idx > 0)
                        {
                            ad = adSoyad.Substring(0, idx).Trim();
                            soyad = adSoyad.Substring(idx + 1).Trim();
                        }
                        else
                        {
                            ad = adSoyad.Trim();
                        }
                    }

                    string sinif = GridHucreDegeri(row, "SERTIFIKA_SINIFI", "Sinif");
                    string saat = GridHucreDegeri(row, "RANDEVU_SAATI", "Saat", "SINAV_SAATI");
                    string durum = GridHucreDegeri(row, "DIR_DURUM", "TeoDurum");
                    if (model != null)
                    {
                        if (string.IsNullOrWhiteSpace(sinif))
                            sinif = GetPropValue(model, "SERTIFIKA_SINIFI", "Sinif");
                        if (string.IsNullOrWhiteSpace(saat))
                            saat = GetPropValue(model, "RANDEVU_SAATI", "Saat", "SINAV_SAATI");
                        if (string.IsNullOrWhiteSpace(durum))
                            durum = GetPropValue(model, "DIR_DURUM", "TeoDurum");
                    }

                    // Gridde TC kolonu yoksa DB'den ID_KURSIYER ile tamamla.
                    if (string.IsNullOrWhiteSpace(tc) && kursiyerId > 0)
                    {
                        string tcDb;
                        if (tcMapFromDb.TryGetValue(kursiyerId, out tcDb))
                            tc = tcDb;
                    }

                    if (string.IsNullOrWhiteSpace(ad) && string.IsNullOrWhiteSpace(soyad) && string.IsNullOrWhiteSpace(tc))
                        continue;

                    EkleRandevuRaporSatiri(dt, eklenenAnahtarlar, tc, ad, soyad, sinif, saat, durum);
                    if (kursiyerId > 0)
                        eklenenKursiyerIdler.Add(kursiyerId);
                }
            }

            // Gridde satir varsa rapor yalnizca gridden uretilir.
            // Bu sayede _currentListe/DB fallback kaynaklarindan gelen tekrarlar tamamen engellenir.
            if (gridSatiriVar)
                return DistinctRandevuRaporTablosu(dt);

            // _currentListe de eklenir (gridden gelmeyenler tamamlanir).
            foreach (var item in (_currentListe ?? new List<DireksiyonSinavModel>()).Where(x => x != null))
            {
                string tc = GetPropValue(item, "TC_NO", "TcNo", "RandevuTcNo");
                string adSoyad = (GetPropValue(item, "RandevuAdiSoyadi", "AdiSoyadi", "AdSoyad") ?? string.Empty).Trim();
                string ad = adSoyad;
                string soyad = string.Empty;
                int idx = adSoyad.IndexOf(' ');
                if (idx > 0)
                {
                    ad = adSoyad.Substring(0, idx).Trim();
                    soyad = adSoyad.Substring(idx + 1).Trim();
                }

                string sinif = GetPropValue(item, "SERTIFIKA_SINIFI", "Sinif");
                string saat = GetPropValue(item, "Saat", "RANDEVU_SAATI");
                string durum = GetPropValue(item, "DIR_DURUM", "TeoDurum");
                EkleRandevuRaporSatiri(dt, eklenenAnahtarlar, tc, ad, soyad, sinif, saat, durum);
            }

            // Son fallback: secili tarihte DB'deki tum direksiyon kayitlarini da ekle.
            DateTime? seciliTarih = GetSelectedSinavDate();
            if (seciliTarih.HasValue)
            {
                var dbListe = GetDireksiyonListeByDateDirectAsync(seciliTarih.Value.Date).GetAwaiter().GetResult();
                foreach (var item in (dbListe ?? new List<DireksiyonSinavModel>()).Where(x => x != null))
                {
                    string tc = GetPropValue(item, "TC_NO", "TcNo", "RandevuTcNo");
                    string adSoyad = (GetPropValue(item, "RandevuAdiSoyadi", "AdiSoyadi", "AdSoyad") ?? string.Empty).Trim();
                    string ad = adSoyad;
                    string soyad = string.Empty;
                    int idx = adSoyad.IndexOf(' ');
                    if (idx > 0)
                    {
                        ad = adSoyad.Substring(0, idx).Trim();
                        soyad = adSoyad.Substring(idx + 1).Trim();
                    }

                    string sinif = GetPropValue(item, "SERTIFIKA_SINIFI", "Sinif");
                    string saat = GetPropValue(item, "Saat", "RANDEVU_SAATI");
                    string durum = GetPropValue(item, "DIR_DURUM", "TeoDurum");
                    EkleRandevuRaporSatiri(dt, eklenenAnahtarlar, tc, ad, soyad, sinif, saat, durum);
                }
            }

            return DistinctRandevuRaporTablosu(dt);
        }

        private static DataTable DistinctRandevuRaporTablosu(DataTable source)
        {
            if (source == null || source.Rows.Count <= 1)
                return source;

            var result = source.Clone();
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in source.Rows)
            {
                string tc = NormalizeDigits(Convert.ToString(row["TC_NO"]));
                string ad = (Convert.ToString(row["ADI"]) ?? string.Empty).Trim();
                string soyad = (Convert.ToString(row["SOYADI"]) ?? string.Empty).Trim();
                string saat = (Convert.ToString(row["SINAV_SAATI"]) ?? string.Empty).Trim();

                // TC varsa yalnızca TC'ye göre tekilleştir; yoksa ad/soyad/saat kullan.
                string key = tc.Length == 11
                    ? "TC|" + tc
                    : "ADSOYAD|" + ad + "|" + soyad + "|" + saat;

                if (keys.Contains(key))
                    continue;

                keys.Add(key);
                result.ImportRow(row);
            }

            return result;
        }

        private static void EkleRandevuRaporSatiri(
            DataTable dt,
            HashSet<string> anahtarSeti,
            string tc,
            string ad,
            string soyad,
            string sinif,
            string saat,
            string durum)
        {
            if (dt == null || anahtarSeti == null)
                return;

            string tcNorm = (tc ?? string.Empty).Trim();
            string adNorm = (ad ?? string.Empty).Trim();
            string soyadNorm = (soyad ?? string.Empty).Trim();
            // Tekrari azaltmak icin once TC'ye gore tekillestir; TC bos ise ad-soyad+saat'e dus.
            string key = !string.IsNullOrWhiteSpace(tcNorm)
                ? "TC|" + tcNorm
                : "ADSOYAD|" + adNorm + "|" + soyadNorm + "|" + (saat ?? string.Empty).Trim();
            if (anahtarSeti.Contains(key))
                return;

            anahtarSeti.Add(key);
            dt.Rows.Add(tcNorm, adNorm, soyadNorm, (sinif ?? string.Empty).Trim(), (saat ?? string.Empty).Trim(), (durum ?? string.Empty).Trim());
        }

        private static string GridHucreDegeri(DataGridViewRow row, params string[] adayKolonlar)
        {
            if (row == null || adayKolonlar == null)
                return string.Empty;

            foreach (string kolon in adayKolonlar)
            {
                if (string.IsNullOrWhiteSpace(kolon))
                    continue;

                if (!row.DataGridView.Columns.Contains(kolon))
                    continue;

                object v = row.Cells[kolon].Value;
                string s = Convert.ToString(v) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(s))
                    return s.Trim();
            }

            return string.Empty;
        }

        private void EnsureCurrentListeKayitlariRaporIcin(int sinavId, DateTime? seciliTarih)
        {
            if (_currentListe == null || _currentListe.Count == 0)
                return;

            foreach (var item in _currentListe.Where(x => x != null && x.ID_KURSIYER > 0))
                EnsureDireksiyonKaydiForRaporDirect(sinavId, item.ID_KURSIYER, seciliTarih);
        }

        private void EnsureDireksiyonKaydiForRaporDirect(int sinavId, int kursiyerId, DateTime? seciliTarih)
        {
            if (kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return;

            var adayIds = new List<int>();
            if (sinavId > 0)
                adayIds.Add(sinavId);

            if (seciliTarih.HasValue)
            {
                foreach (int altId in GetAlternativeSinavIdsByDate(seciliTarih.Value.Date, sinavId))
                {
                    if (altId > 0 && !adayIds.Contains(altId))
                        adayIds.Add(altId);
                }
            }

            foreach (int id in adayIds)
            {
                try
                {
                    EnsureDireksiyonKaydiByIdDirect(id, kursiyerId);
                }
                catch
                {
                    // rapor akisinda hata vermeden devam et
                }
            }
        }

        private void EnsureDireksiyonKaydiByIdDirect(int sinavId, int kursiyerId)
        {
            if (sinavId <= 0 || kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = @"
IF NOT EXISTS (
    SELECT 1
    FROM SINAV_LISTE_DIREKSIYON
    WHERE ID_SINAV_TARIHI = @ID_SINAV_TARIHI
      AND ID_KURSIYER = @ID_KURSIYER
)
BEGIN
    INSERT INTO SINAV_LISTE_DIREKSIYON (ID_SINAV_TARIHI, ID_KURSIYER, DIR_HAK, DIR_DURUM)
    VALUES (@ID_SINAV_TARIHI, @ID_KURSIYER, 0, 'GIRMEDI');
END";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavId);
                cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureRandevuMehmetRaporKaydi()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = @"
IF OBJECT_ID('dbo.RAPOR_TANIMLARI','U') IS NULL
    RETURN;

UPDATE dbo.RAPOR_TANIMLARI
SET RAPOR_ADI = N'RANDEVU MEHMET',
    RAPOR_YOLU = N'C:\Raporlar\DireksiyonEgitimiSinavTakipSonucListesi.frx',
    GUNCELLEME_TARIHI = GETDATE()
WHERE RAPOR_GRUBU = N'SINAV LISTESI - DIREKSIYON'
  AND (
      RAPOR_ADI = N'DIREKSIYON EGITIMI SINAV TAKIP VE SONUC LISTESI'
      OR RAPOR_YOLU LIKE N'%DireksiyonEgitimiSinavTakipSonucListesi.frx'
  );

IF NOT EXISTS (
    SELECT 1
    FROM dbo.RAPOR_TANIMLARI
    WHERE RAPOR_GRUBU = N'SINAV LISTESI - DIREKSIYON'
      AND RAPOR_ADI = N'RANDEVU MEHMET'
)
BEGIN
    DECLARE @SIRA INT;
    SELECT @SIRA = ISNULL(MAX(SIRA_NO), 0) + 1 FROM dbo.RAPOR_TANIMLARI;

    INSERT INTO dbo.RAPOR_TANIMLARI
    (
        RAPOR_GRUBU, RAPOR_ADI, RAPOR_YOLU, SIRA_NO, AKTIF, RENK, SABLON_BINARY, OLUSTURMA_TARIHI, GUNCELLEME_TARIHI
    )
    VALUES
    (
        N'SINAV LISTESI - DIREKSIYON',
        N'RANDEVU MEHMET',
        N'C:\Raporlar\DireksiyonEgitimiSinavTakipSonucListesi.frx',
        @SIRA, 1, 0, NULL, GETDATE(), GETDATE()
    );
END";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Rapor listesi acilmasin diye burada hata yansitilmaz.
            }
        }

        private int ResolveAktifSinavIdForRapor(DateTime? seciliTarih)
        {
            int sinavId;
            if (TryGetSelectedSinavId(out sinavId) && sinavId > 0)
                return sinavId;

            var first = _currentListe == null ? null : _currentListe.FirstOrDefault(x => x != null);
            if (first != null)
            {
                sinavId = ToInt(GetPropValue(first, "ID_SINAV_TARIHI", "SinavId", "IDSinavTarihi"));
                if (sinavId > 0)
                    return sinavId;
            }

            if (seciliTarih.HasValue)
            {
                var alternatifler = GetAlternativeSinavIdsByDate(seciliTarih.Value.Date, 0);
                if (alternatifler != null && alternatifler.Count > 0)
                    return alternatifler[0];
            }

            return 0;
        }

        private void GridAyarla()
        {
            Dgv_Listesi.AllowUserToAddRows = false;
            Dgv_Listesi.RowHeadersVisible = false;
            Dgv_Listesi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Dgv_Listesi.RowTemplate.Height = 32;
            Dgv_Listesi.EnableHeadersVisualStyles = false;
            Dgv_Listesi.ColumnHeadersDefaultCellStyle.BackColor = Color.DimGray;
            Dgv_Listesi.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dgv_Listesi.CellFormatting -= Dgv_Listesi_CellFormatting;
            Dgv_Listesi.CellFormatting += Dgv_Listesi_CellFormatting;
            Dgv_Listesi.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            Dgv_Listesi.ColumnHeadersHeight = 34;
        }

        private void EnsureAktifFiltreBilgiLabel()
        {
            if (_lblAktifFiltreBilgi != null)
                return;

            _lblAktifFiltreBilgi = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                Text = "Not: Personelde sadece 'Devam' durumundakiler, araçlarda sadece 'Aktif' olanlar listelenir.",
                Name = "LblAktifFiltreBilgi",
                Location = new Point(620, 15)
            };

            if (Tab_Direk != null)
                Tab_Direk.Controls.Add(_lblAktifFiltreBilgi);
        }

        private void KolonlariDuzenle()
        {
            foreach (DataGridViewColumn c in Dgv_Listesi.Columns)
                c.Visible = false;

            EnsureSiraNoColumn();

            DataGridViewColumn tcCol = FindColumn("TC_NO", "TcNo", "TC", "TCKimlikNo", "RandevuTcNo");
            if (tcCol == null)
            {
                tcCol = new DataGridViewTextBoxColumn
                {
                    Name = "TC_NO",
                    HeaderText = "TC NO",
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.Programmatic
                };
                Dgv_Listesi.Columns.Add(tcCol);
            }
            DataGridViewColumn adSoyadCol = FindColumn("RandevuAdiSoyadi", "AdiSoyadi", "AdSoyad", "KursiyerAdi");
            DataGridViewColumn donemCol = FindColumn("Donem", "DONEM_ADI", "Donemi");
            if (donemCol == null)
            {
                donemCol = new DataGridViewTextBoxColumn
                {
                    Name = "Donemi",
                    HeaderText = "DÖNEMİ",
                    ReadOnly = true,
                    SortMode = DataGridViewColumnSortMode.Programmatic
                };
                Dgv_Listesi.Columns.Add(donemCol);
            }
            DataGridViewColumn dirHakCol = FindColumn("DirHak", "Hak", "DIRHAK");
            DataGridViewColumn sinavTarihiCol = FindColumn("SinavTarihi", "SINAV_TARIHI", "E_SinavTarihi", "Tarih");
            DataGridViewColumn dirDurumCol = FindColumn("DirDurum", "DIR_DURUM", "Durum");

            if (Dgv_Listesi.Columns.Contains("SiraNo")) Dgv_Listesi.Columns["SiraNo"].Visible = true;
            if (tcCol != null) { tcCol.Visible = true; tcCol.HeaderText = "TC NO"; }
            if (adSoyadCol != null) { adSoyadCol.Visible = true; adSoyadCol.HeaderText = "ADI SOYADI"; }
            if (donemCol != null) donemCol.HeaderText = "DONEMİ";
            if (donemCol != null) donemCol.Visible = true;
            if (dirHakCol != null) { dirHakCol.Visible = true; dirHakCol.HeaderText = "DIRHAK"; }
            if (sinavTarihiCol != null) { sinavTarihiCol.Visible = true; sinavTarihiCol.HeaderText = "SINAV TARİHİ"; }
            if (dirDurumCol != null) { dirDurumCol.Visible = true; dirDurumCol.HeaderText = "DIR DURUM"; }
            if (Dgv_Listesi.Columns.Contains("SiraNo")) Dgv_Listesi.Columns["SiraNo"].Width = 55;
            if (tcCol != null) tcCol.Width = 90;
            if (donemCol != null) donemCol.Width = 120;
            if (dirHakCol != null) dirHakCol.Width = 60;
            if (dirDurumCol != null) dirDurumCol.Width = 85;

            int index = 0;
            SetDisplayIndexIfExists("SiraNo", ref index);
            SetDisplayIndexIfExists(tcCol, ref index);
            SetDisplayIndexIfExists(adSoyadCol, ref index);
            SetDisplayIndexIfExists(donemCol, ref index);
            SetDisplayIndexIfExists(dirHakCol, ref index);
            SetDisplayIndexIfExists("PersonelId", ref index);
            SetDisplayIndexIfExists("AracId", ref index);
            SetDisplayIndexIfExists(sinavTarihiCol, ref index);
            SetDisplayIndexIfExists("Saat", ref index);
            SetDisplayIndexIfExists(dirDurumCol, ref index);

            // Gridde TC kolonu yoksa, ID_KURSIYER uzerinden DB'den getirip gorunur kolona yaz.
            if (tcCol != null && Dgv_Listesi.Rows.Count > 0)
            {
                var ids = Dgv_Listesi.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r != null && !r.IsNewRow)
                    .Select(r => r.DataBoundItem as DireksiyonSinavModel)
                    .Where(m => m != null && m.ID_KURSIYER > 0)
                    .Select(m => m.ID_KURSIYER)
                    .Distinct()
                    .ToList();

                var tcMap = ids.Count > 0 ? GetKursiyerTcMap(ids) : new Dictionary<int, string>();
                foreach (DataGridViewRow row in Dgv_Listesi.Rows)
                {
                    if (row == null || row.IsNewRow)
                        continue;

                    var model = row.DataBoundItem as DireksiyonSinavModel;
                    if (model == null)
                        continue;

                    string tc = GetPropValue(model, "TC_NO", "TcNo", "RandevuTcNo");
                    if (string.IsNullOrWhiteSpace(tc) && model.ID_KURSIYER > 0)
                    {
                        string tcDb;
                        if (tcMap.TryGetValue(model.ID_KURSIYER, out tcDb))
                            tc = tcDb;
                    }

                    row.Cells[tcCol.Name].Value = (tc ?? string.Empty).Trim();
                }
            }

            if (donemCol != null && Dgv_Listesi.Rows.Count > 0)
            {
                var ids = Dgv_Listesi.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r != null && !r.IsNewRow)
                    .Select(r => r.DataBoundItem as DireksiyonSinavModel)
                    .Where(m => m != null && m.ID_KURSIYER > 0)
                    .Select(m => m.ID_KURSIYER)
                    .Distinct()
                    .ToList();
                var donemMap = ids.Count > 0 ? GetKursiyerDonemMap(ids) : new Dictionary<int, string>();

                foreach (DataGridViewRow row in Dgv_Listesi.Rows)
                {
                    if (row == null || row.IsNewRow)
                        continue;

                    var model = row.DataBoundItem as DireksiyonSinavModel;
                    string donem = model == null ? string.Empty : GetPropValue(model, "Donem", "DONEM_ADI", "Donemi");
                    if (string.IsNullOrWhiteSpace(donem) && model != null && model.ID_KURSIYER > 0)
                    {
                        string donemDb;
                        if (donemMap.TryGetValue(model.ID_KURSIYER, out donemDb))
                            donem = donemDb;
                    }

                    row.Cells[donemCol.Name].Value = (donem ?? string.Empty).Trim();
                }
            }
        }

        private void ComboKolonlariEkle()
        {
            Sil("PersonelId");
            Sil("AracId");
            Sil("Saat");
            DataTable aktifPersoneller = GetAktifPersoneller();
            DataTable araclar = GetAraclarSafe();
            DataTable saatler = GetSaatlerSafe();

            // Personel Combo
            var personelKolon = new DataGridViewComboBoxColumn
            {
                Name = "PersonelId",
                HeaderText = "PERSONEL",
                DataPropertyName = "PersonelId",
                DataSource = aktifPersoneller,
                DisplayMember = "PERSONEL",
                ValueMember = "ID",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Standard,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };
            Dgv_Listesi.Columns.Add(personelKolon);

            // Araç Combo
            var aracKolon = new DataGridViewComboBoxColumn
            {
                Name = "AracId",
                HeaderText = "ARAÇ",
                DataPropertyName = "AracId",
                DataSource = araclar,
                DisplayMember = "ARAC_PLAKA",
                ValueMember = "ID",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Standard,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };
            Dgv_Listesi.Columns.Add(aracKolon);

            // Saat Combo
            var saatKolon = new DataGridViewComboBoxColumn
            {
                Name = "Saat",
                HeaderText = "SAAT",
                DataPropertyName = "Saat",
                DataSource = saatler,
                DisplayMember = "SAAT",
                ValueMember = "SAAT",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Standard,
                SortMode = DataGridViewColumnSortMode.Programmatic
            };
            Dgv_Listesi.Columns.Add(saatKolon);

            FillLookupMap(_personelMap, aktifPersoneller, "ID", "PERSONEL");
            FillLookupMap(_aracMap, araclar, "ID", "ARAC_PLAKA");

            // Sadece istenen kolonlar kalsin.
            if (Dgv_Listesi.Columns.Contains("PersonelId")) Dgv_Listesi.Columns["PersonelId"].Visible = true;
            if (Dgv_Listesi.Columns.Contains("AracId")) Dgv_Listesi.Columns["AracId"].Visible = true;
            if (Dgv_Listesi.Columns.Contains("Saat")) Dgv_Listesi.Columns["Saat"].Visible = true;
        }

        private async Task Grid_CellValueChanged(DataGridViewCellEventArgs e)
        {
            if (yukleniyor || e.RowIndex < 0) return;
            if (!LisansPolitikasi.IsWriteAllowed) return;

            try
            {
                var model = Dgv_Listesi.Rows[e.RowIndex].DataBoundItem as DireksiyonSinavModel;
                if (model == null) return;

                SyncModelForRandevuUpdate(model);

                int sonuc = 0;
                try
                {
                    sonuc = await _service.GuncelleAsync(model);
                }
                catch
                {
                    sonuc = 0;
                }

                if (sonuc == 0 && UpdateDireksiyonRandevuDirect(model))
                    sonuc = 1;

                if (sonuc == 0)
                    MessageBox.Show("Güncelleme yapılamadı.");
                else
                    LisansPolitikasi.RegisterSuccessfulWrite();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Güncelleme sırasında hata oluştu: " + ex.Message);
            }
        }

        private void SyncModelForRandevuUpdate(DireksiyonSinavModel model)
        {
            if (model == null)
                return;

            int personelId = ToInt(GetPropValue(model, "PersonelId", "ID_PERSONEL"));
            int aracId = ToInt(GetPropValue(model, "AracId", "ID_ARAC"));
            string saat = GetPropValue(model, "Saat", "RANDEVU_SAATI");

            SetIntProp(model, "PersonelId", personelId);
            SetIntProp(model, "ID_PERSONEL", personelId);
            SetIntProp(model, "AracId", aracId);
            SetIntProp(model, "ID_ARAC", aracId);
            SetStringProp(model, "Saat", saat ?? string.Empty);
            SetStringProp(model, "RANDEVU_SAATI", saat ?? string.Empty);
        }

        private bool UpdateDireksiyonRandevuDirect(DireksiyonSinavModel model)
        {
            if (!LisansPolitikasi.IsWriteAllowed || model == null || string.IsNullOrWhiteSpace(_connectionString))
                return false;

            int personelId = ToInt(GetPropValue(model, "PersonelId", "ID_PERSONEL"));
            int aracId = ToInt(GetPropValue(model, "AracId", "ID_ARAC"));
            string saat = (GetPropValue(model, "Saat", "RANDEVU_SAATI") ?? string.Empty).Trim();
            int kayitId = ToInt(GetPropValue(model, "ID"));

            try
            {
                if (kayitId > 0)
                {
                    const string sqlById = @"UPDATE SINAV_LISTE_DIREKSIYON
SET ID_PERSONEL=@PERSONEL_ID,
    ID_ARAC=@ARAC_ID,
    RANDEVU_SAATI=@SAAT
WHERE ID=@ID";
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sqlById, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", kayitId);
                        cmd.Parameters.AddWithValue("@PERSONEL_ID", personelId > 0 ? (object)personelId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ARAC_ID", aracId > 0 ? (object)aracId : DBNull.Value);
                        cmd.Parameters.AddWithValue("@SAAT", string.IsNullOrWhiteSpace(saat) ? (object)DBNull.Value : saat);
                        conn.Open();
                        if (cmd.ExecuteNonQuery() > 0)
                            return true;
                    }
                }

                int sinavId;
                if (!TryGetSelectedSinavId(out sinavId))
                    return false;
                if (model.ID_KURSIYER <= 0)
                    return false;

                const string sqlByPair = @"UPDATE SINAV_LISTE_DIREKSIYON
SET ID_PERSONEL=@PERSONEL_ID,
    ID_ARAC=@ARAC_ID,
    RANDEVU_SAATI=@SAAT
WHERE ID_SINAV_TARIHI=@SINAV_ID
  AND ID_KURSIYER=@KURSIYER_ID";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sqlByPair, conn))
                {
                    cmd.Parameters.AddWithValue("@SINAV_ID", sinavId);
                    cmd.Parameters.AddWithValue("@KURSIYER_ID", model.ID_KURSIYER);
                    cmd.Parameters.AddWithValue("@PERSONEL_ID", personelId > 0 ? (object)personelId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ARAC_ID", aracId > 0 ? (object)aracId : DBNull.Value);
                    cmd.Parameters.AddWithValue("@SAAT", string.IsNullOrWhiteSpace(saat) ? (object)DBNull.Value : saat);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void Goster(string kolon, string baslik)
        {
            if (Dgv_Listesi.Columns.Contains(kolon))
            {
                Dgv_Listesi.Columns[kolon].Visible = true;
                Dgv_Listesi.Columns[kolon].HeaderText = baslik;
            }
        }

        private void Sil(string kolon)
        {
            if (Dgv_Listesi.Columns.Contains(kolon))
                Dgv_Listesi.Columns.Remove(kolon);
        }

        private DataTable GetSinavTarihleri()
        {
            DataTable fromYonetim = BuildComboSourceFromYonetimData();
            if (fromYonetim.Rows.Count > 0)
                return fromYonetim;

            DataTable serviceTable = _service.GetSinavTarihleri();
            DataTable dbTable = GetSinavTarihleriFromDb();
            DataTable merged = MergeSinavTarihleri(serviceTable, dbTable);
            return DistinctSinavTarihleri(merged);
        }

        private DataTable BuildComboSourceFromYonetimData()
        {
            DataTable result = new DataTable();
            result.Columns.Add("ID", typeof(int));
            result.Columns.Add("SINAV_TARIHI", typeof(string));

            DataTable src = GetSinavTarihleriYonetimData();
            if (src == null || src.Rows.Count == 0 || !src.Columns.Contains("ID") || !src.Columns.Contains("SINAV_TARIHI"))
                return result;

            foreach (DataRow row in src.Rows)
            {
                int id = ToInt(row["ID"]);
                if (id <= 0)
                    continue;

                DateTime dt;
                if (!DateTime.TryParse(Convert.ToString(row["SINAV_TARIHI"]), out dt))
                    continue;

                DataRow nr = result.NewRow();
                nr["ID"] = id;
                nr["SINAV_TARIHI"] = dt.ToString("dd.MM.yyyy");
                result.Rows.Add(nr);
            }

            return result;
        }

        private DataTable GetSinavTarihleriFromDb()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("SINAV_TARIHI", typeof(string));

            if (string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            const string sql = @"
SELECT
    st.ID,
    CONVERT(varchar(10), CAST(st.SINAV_TARIHI AS date), 104) AS SINAV_TARIHI
FROM SINAV_TARIHLERI st
WHERE st.SINAV_TARIHI IS NOT NULL
  AND st.SINAV_TARIHI > '1900-01-01'
  AND (
      UPPER(ISNULL(st.SINAV_TURU,'')) LIKE '%DIREK%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE N'%DİREK%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE '%DIR%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE N'%DİR%'
      OR EXISTS (SELECT 1 FROM SINAV_LISTE_DIREKSIYON d WHERE d.ID_SINAV_TARIHI = st.ID)
  )
ORDER BY st.SINAV_TARIHI DESC, st.ID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            catch
            {
                // Servis verisi fallback olarak kullanılacak.
            }

            return dt;
        }

        private static DataTable MergeSinavTarihleri(DataTable primary, DataTable secondary)
        {
            DataTable result = new DataTable();
            result.Columns.Add("ID", typeof(int));
            result.Columns.Add("SINAV_TARIHI", typeof(string));

            Action<DataTable> append = (src) =>
            {
                if (src == null || src.Rows.Count == 0) return;
                bool hasId = src.Columns.Contains("ID");
                string dateCol = src.Columns.Contains("SINAV_TARIHI")
                    ? "SINAV_TARIHI"
                    : (src.Columns.Contains("SINAV_TARIHI_TEXT") ? "SINAV_TARIHI_TEXT" : null);
                if (!hasId || string.IsNullOrWhiteSpace(dateCol)) return;

                foreach (DataRow row in src.Rows)
                {
                    int id;
                    if (!int.TryParse(Convert.ToString(row["ID"]), out id) || id <= 0) continue;
                    string tarih = Convert.ToString(row[dateCol]);
                    if (string.IsNullOrWhiteSpace(tarih)) continue;

                    DataRow nr = result.NewRow();
                    nr["ID"] = id;
                    nr["SINAV_TARIHI"] = tarih;
                    result.Rows.Add(nr);
                }
            };

            append(primary);
            append(secondary);
            return result;
        }

        private static DataTable DistinctSinavTarihleri(DataTable source)
        {
            if (source == null || source.Rows.Count == 0)
                return source ?? new DataTable();

            bool hasId = source.Columns.Contains("ID");
            string textCol = source.Columns.Contains("SINAV_TARIHI")
                ? "SINAV_TARIHI"
                : source.Columns.Cast<DataColumn>().Select(c => c.ColumnName).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(textCol))
                return source;

            DataTable result = source.Clone();
            Dictionary<string, DataRow> secilen = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in source.Rows)
            {
                string text = Convert.ToString(row[textCol]).Trim();
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                DateTime dt;
                if (DateTime.TryParse(text, out dt))
                    text = dt.ToString("dd.MM.yyyy");

                if (!secilen.ContainsKey(text))
                {
                    secilen[text] = row;
                    continue;
                }

                if (hasId)
                {
                    int mevcutId = ToInt(secilen[text]["ID"]);
                    int yeniId = ToInt(row["ID"]);
                    if (yeniId > mevcutId)
                        secilen[text] = row;
                }
            }

            foreach (DataRow row in secilen.Values)
                result.ImportRow(row);

            return result;
        }

        private static int ToInt(object value)
        {
            int n;
            return int.TryParse(Convert.ToString(value), out n) ? n : 0;
        }

        private static List<DireksiyonSinavModel> NormalizeListeForGrid(IEnumerable<DireksiyonSinavModel> kaynak)
        {
            if (kaynak == null)
                return new List<DireksiyonSinavModel>();

            return kaynak
                .Where(x => x != null)
                .GroupBy(x =>
                {
                    if (x.ID_KURSIYER > 0)
                    {
                        string t = x.SINAV_TARIHI == DateTime.MinValue
                            ? string.Empty
                            : x.SINAV_TARIHI.Date.ToString("yyyyMMdd");
                        return "K:" + x.ID_KURSIYER + "|T:" + t;
                    }

                    if (x.ID > 0)
                        return "ID:" + x.ID;

                    string tc = NormalizeText(GetPropValue(x, "TC_NO", "TcNo", "RandevuTcNo"));
                    string ad = NormalizeText(GetPropValue(x, "RandevuAdiSoyadi", "AdiSoyadi", "AdSoyad"));
                    string tarih = NormalizeText(GetPropValue(x, "SinavTarihi", "SINAV_TARIHI", "E_SinavTarihi", "Tarih"));
                    string saat = NormalizeText(GetPropValue(x, "Saat"));
                    return "ROW:" + tc + "|" + ad + "|" + tarih + "|" + saat;
                })
                .Select(g => g.OrderByDescending(x => x.ID).First())
                .ToList();
        }

        private static string GetPropValue(object obj, params string[] names)
        {
            if (obj == null || names == null) return string.Empty;
            Type t = obj.GetType();
            foreach (string n in names)
            {
                var p = t.GetProperty(n);
                if (p == null) continue;
                object val = p.GetValue(obj, null);
                string s = Convert.ToString(val);
                if (!string.IsNullOrWhiteSpace(s))
                    return s;
            }
            return string.Empty;
        }

        private static string NormalizeText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
        }

        private void EnsureSiraNoColumn()
        {
            if (Dgv_Listesi.Columns.Contains("SiraNo"))
                return;

            DataGridViewTextBoxColumn siraNo = new DataGridViewTextBoxColumn
            {
                Name = "SiraNo",
                HeaderText = "Sıra No",
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable,
                Width = 70
            };
            Dgv_Listesi.Columns.Insert(0, siraNo);
        }

        private void Dgv_Listesi_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string colName = Dgv_Listesi.Columns[e.ColumnIndex].Name;
            if (colName == "SiraNo")
            {
                e.Value = (e.RowIndex + 1).ToString();
                e.FormattingApplied = true;
                return;
            }

            if (colName != "DirDurum" && colName != "DIR_DURUM" && colName != "Durum")
                return;

            string durum = (Convert.ToString(e.Value) ?? string.Empty).Trim().ToUpperInvariant();
            if (durum.Contains("GEÇTİ") || durum.Contains("GECTI") || durum.Contains("BAŞARILI") || durum.Contains("BASARILI"))
            {
                e.CellStyle.BackColor = Color.FromArgb(198, 239, 206);
                e.CellStyle.ForeColor = Color.FromArgb(0, 97, 0);
                e.CellStyle.Font = new Font(Dgv_Listesi.Font, FontStyle.Bold);
            }
            else if (durum.Contains("KALDI") || durum.Contains("BAŞARISIZ") || durum.Contains("BASARISIZ"))
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 199, 206);
                e.CellStyle.ForeColor = Color.FromArgb(156, 0, 6);
                e.CellStyle.Font = new Font(Dgv_Listesi.Font, FontStyle.Bold);
            }
            else
            {
                e.CellStyle.BackColor = Color.FromArgb(255, 235, 156);
                e.CellStyle.ForeColor = Color.FromArgb(156, 101, 0);
                e.CellStyle.Font = new Font(Dgv_Listesi.Font, FontStyle.Bold);
            }
        }

        private void Dgv_Listesi_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || _currentListe == null || _currentListe.Count == 0) return;

            string columnName = Dgv_Listesi.Columns[e.ColumnIndex].Name;
            bool asc = _lastSortColumn == columnName ? !_lastSortAsc : true;
            _lastSortColumn = columnName;
            _lastSortAsc = asc;

            IEnumerable<DireksiyonSinavModel> sorted = asc
                ? _currentListe.OrderBy(x => GetSortKey(x, columnName))
                : _currentListe.OrderByDescending(x => GetSortKey(x, columnName));

            _currentListe = sorted.ToList();
            Dgv_Listesi.DataSource = null;
            Dgv_Listesi.DataSource = _currentListe;
            KolonlariDuzenle();
            ComboKolonlariEkle();
        }

        private string GetSortKey(DireksiyonSinavModel model, string columnName)
        {
            if (model == null) return string.Empty;
            if (string.Equals(columnName, "PersonelId", StringComparison.OrdinalIgnoreCase))
            {
                int id = ToInt(GetPropValue(model, "PersonelId"));
                string ad;
                return _personelMap.TryGetValue(id, out ad) ? ad : string.Empty;
            }
            if (string.Equals(columnName, "AracId", StringComparison.OrdinalIgnoreCase))
            {
                int id = ToInt(GetPropValue(model, "AracId"));
                string plaka;
                return _aracMap.TryGetValue(id, out plaka) ? plaka : string.Empty;
            }

            if (string.Equals(columnName, "SiraNo", StringComparison.OrdinalIgnoreCase))
                return ToInt(GetPropValue(model, "ID")).ToString("D10");

            return NormalizeText(GetPropValue(model, columnName, "DirDurum", "RandevuAdiSoyadi", "TC_NO"));
        }

        private static void FillLookupMap(Dictionary<int, string> map, object source, string keyCol, string textCol)
        {
            map.Clear();
            DataTable dt = source as DataTable;
            if (dt == null || !dt.Columns.Contains(keyCol) || !dt.Columns.Contains(textCol))
                return;

            foreach (DataRow row in dt.Rows)
            {
                int id;
                if (!int.TryParse(Convert.ToString(row[keyCol]), out id) || id <= 0) continue;
                string text = Convert.ToString(row[textCol]) ?? string.Empty;
                if (!map.ContainsKey(id))
                    map.Add(id, text);
            }
        }

        private DataTable GetPersonellerSafe()
        {
            try
            {
                DataTable dt = _service.GetPersoneller();
                if (dt != null)
                    return dt;
            }
            catch
            {
            }

            DataTable empty = new DataTable();
            empty.Columns.Add("ID", typeof(int));
            empty.Columns.Add("PERSONEL", typeof(string));
            return empty;
        }

        private DataTable GetAraclarSafe()
        {
            try
            {
                DataTable dt = _service.GetAraclar();
                if (dt != null && dt.Columns.Contains("ID") && dt.Columns.Contains("ARAC_PLAKA"))
                {
                    DataTable filtered = dt.Clone();
                    string aktCol = dt.Columns.Contains("AKT") ? "AKT" : string.Empty;
                    string durumCol = dt.Columns.Contains("DURUMU") ? "DURUMU" : string.Empty;

                    foreach (DataRow row in dt.Rows)
                    {
                        string plaka = Convert.ToString(row["ARAC_PLAKA"]) ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(plaka))
                            continue;

                        bool aktif = true;
                        if (!string.IsNullOrWhiteSpace(aktCol))
                            aktif = ToInt(row[aktCol]) == 1;
                        else if (!string.IsNullOrWhiteSpace(durumCol))
                        {
                            string durum = (Convert.ToString(row[durumCol]) ?? string.Empty).Trim().ToUpperInvariant();
                            aktif = durum.Contains("AKTIF") || durum.Contains("AKTİF") || durum == "1" || durum == "TRUE";
                        }

                        if (aktif)
                            filtered.ImportRow(row);
                    }

                    return filtered;
                }
            }
            catch
            {
            }

            DataTable fromDb = GetAraclarFromDb();
            if (fromDb.Rows.Count > 0)
                return fromDb;

            DataTable empty = new DataTable();
            empty.Columns.Add("ID", typeof(int));
            empty.Columns.Add("ARAC_PLAKA", typeof(string));
            return empty;
        }

        private DataTable GetAraclarFromDb()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("ARAC_PLAKA", typeof(string));

            if (string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            string[] sqlAdaylari =
            {
                @"SELECT ID, ISNULL(ARAC_PLAKA,'') AS ARAC_PLAKA
                  FROM AracParam
                  WHERE ISNULL(AKT,1)=1
                     OR UPPER(ISNULL(DURUMU,'')) LIKE N'%AKTIF%'
                     OR UPPER(ISNULL(DURUMU,'')) LIKE N'%AKTİF%'
                  ORDER BY ARAC_PLAKA ASC",
                @"SELECT ID, ISNULL(ARAC_PLAKA,'') AS ARAC_PLAKA
                  FROM dbo.AracParam
                  WHERE ISNULL(AKT,1)=1
                     OR UPPER(ISNULL(DURUMU,'')) LIKE N'%AKTIF%'
                     OR UPPER(ISNULL(DURUMU,'')) LIKE N'%AKTİF%'
                  ORDER BY ARAC_PLAKA ASC",
                @"SELECT ID, ISNULL(ARAC_PLAKA,'') AS ARAC_PLAKA
                  FROM AracParam
                  WHERE ISNULL(ARAC_PLAKA,'') <> ''
                  ORDER BY ISNULL(AKT,1) DESC, ARAC_PLAKA ASC",
                @"SELECT ID, ISNULL(ARAC_PLAKA,'') AS ARAC_PLAKA
                  FROM PARAM_ARAC_TANIMLARI
                  WHERE ISNULL(ARAC_PLAKA,'') <> ''
                    AND (
                        UPPER(ISNULL(DURUMU,'')) LIKE N'%AKTIF%'
                        OR UPPER(ISNULL(DURUMU,'')) LIKE N'%AKTİF%'
                        OR UPPER(ISNULL(DURUMU,'')) LIKE N'%DEVAM%'
                        OR ISNULL(AKT,1)=1
                    )
                  ORDER BY ARAC_PLAKA ASC",
                @"SELECT ID, ISNULL(ARAC_PLAKA,'') AS ARAC_PLAKA
                  FROM dbo.PARAM_ARAC_TANIMLARI
                  WHERE (
                        UPPER(ISNULL(DURUMU,'')) LIKE N'%AKTIF%'
                        OR UPPER(ISNULL(DURUMU,'')) LIKE N'%AKTİF%'
                        OR UPPER(ISNULL(DURUMU,'')) LIKE N'%DEVAM%'
                        OR ISNULL(AKT,1)=1
                  )
                  ORDER BY ARAC_PLAKA ASC",
                @"SELECT ID, ISNULL(PLAKA,'') AS ARAC_PLAKA
                  FROM AracParam
                  ORDER BY PLAKA ASC"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        DataTable tmp = new DataTable();
                        da.Fill(tmp);
                        if (!tmp.Columns.Contains("ID") || !tmp.Columns.Contains("ARAC_PLAKA"))
                            continue;

                        foreach (DataRow row in tmp.Rows)
                        {
                            int id = ToInt(row["ID"]);
                            string plaka = Convert.ToString(row["ARAC_PLAKA"]) ?? string.Empty;
                            if (id <= 0 || string.IsNullOrWhiteSpace(plaka))
                                continue;
                            DataRow nr = dt.NewRow();
                            nr["ID"] = id;
                            nr["ARAC_PLAKA"] = plaka.Trim();
                            dt.Rows.Add(nr);
                        }
                    }

                    if (dt.Rows.Count > 0)
                        return dt;
                }
                catch
                {
                    // sonraki SQL adayi
                }
            }

            return dt;
        }

        private DataTable GetSaatlerSafe()
        {
            try
            {
                DataTable dt = _service.GetSaatler();
                if (dt != null)
                    return dt;
            }
            catch
            {
            }

            DataTable empty = new DataTable();
            empty.Columns.Add("SAAT", typeof(string));
            return empty;
        }

        private DataGridViewColumn FindColumn(params string[] candidates)
        {
            foreach (string name in candidates)
            {
                if (Dgv_Listesi.Columns.Contains(name))
                    return Dgv_Listesi.Columns[name];
            }
            return null;
        }

        private void SetDisplayIndexIfExists(string columnName, ref int nextIndex)
        {
            if (!Dgv_Listesi.Columns.Contains(columnName))
                return;

            DataGridViewColumn col = Dgv_Listesi.Columns[columnName];
            col.DisplayIndex = nextIndex++;
        }

        private static void SetDisplayIndexIfExists(DataGridViewColumn col, ref int nextIndex)
        {
            if (col == null)
                return;

            col.DisplayIndex = nextIndex++;
        }

        #region Mebbis
        private void HazirlaRandevuTab()
        {
            try
            {
                Combo_Arac_Sec.DropDownStyle = ComboBoxStyle.DropDownList;
                Combo_Personel_Sec.DropDownStyle = ComboBoxStyle.DropDownList;
                RefreshRandevuSecimCombolari();
            }
            catch
            {
                // Randevu altyapısı; tablo/kolon uyuşmazlıklarında sessiz geç.
            }
        }

        private void RefreshRandevuSecimCombolari()
        {
            DataTable personelKaynak = GetAktifPersoneller();
            DataTable aracKaynak = GetAraclarSafe();

            Dictionary<int, string> personelAdMap = new Dictionary<int, string>();
            Dictionary<int, string> aracAdMap = new Dictionary<int, string>();
            FillLookupMap(personelAdMap, personelKaynak, "ID", "PERSONEL");
            FillLookupMap(aracAdMap, aracKaynak, "ID", "ARAC_PLAKA");

            DataTable personelSecim = new DataTable();
            personelSecim.Columns.Add("ID", typeof(int));
            personelSecim.Columns.Add("TEXT", typeof(string));

            DataTable aracSecim = new DataTable();
            aracSecim.Columns.Add("ID", typeof(int));
            aracSecim.Columns.Add("TEXT", typeof(string));

            DateTime? seciliTarih = GetSelectedSinavDate();
            IEnumerable<DireksiyonSinavModel> kaynak = _currentListe ?? new List<DireksiyonSinavModel>();
            if (seciliTarih.HasValue)
                kaynak = kaynak.Where(x => x != null && x.SINAV_TARIHI.Date == seciliTarih.Value.Date);

            foreach (int pid in kaynak.Select(x => ToInt(x == null ? null : x.PersonelId)).Where(x => x > 0).Distinct().OrderBy(x => x))
            {
                DataRow nr = personelSecim.NewRow();
                nr["ID"] = pid;
                string ad;
                nr["TEXT"] = personelAdMap.TryGetValue(pid, out ad) ? ad : ("Personel #" + pid);
                personelSecim.Rows.Add(nr);
            }

            foreach (int aid in kaynak.Select(x => ToInt(x == null ? null : x.AracId)).Where(x => x > 0).Distinct().OrderBy(x => x))
            {
                DataRow nr = aracSecim.NewRow();
                nr["ID"] = aid;
                string plaka;
                nr["TEXT"] = aracAdMap.TryGetValue(aid, out plaka) ? plaka : ("Araç #" + aid);
                aracSecim.Rows.Add(nr);
            }

            Combo_Personel_Sec.DataSource = personelSecim;
            Combo_Personel_Sec.DisplayMember = "TEXT";
            Combo_Personel_Sec.ValueMember = "ID";
            Combo_Personel_Sec.SelectedIndex = personelSecim.Rows.Count > 0 ? 0 : -1;

            Combo_Arac_Sec.DataSource = aracSecim;
            Combo_Arac_Sec.DisplayMember = "TEXT";
            Combo_Arac_Sec.ValueMember = "ID";
            Combo_Arac_Sec.SelectedIndex = aracSecim.Rows.Count > 0 ? 0 : -1;
        }

        private DataTable GetAktifPersoneller()
        {
            DataTable dt = GetAktifPersonellerFromDb();
            if (dt != null && dt.Rows.Count > 0)
                return dt;

            dt = GetPersonellerSafe();
            if (dt == null)
                return new DataTable();
            if (!dt.Columns.Contains("ID") || !dt.Columns.Contains("PERSONEL"))
                return dt;
            HashSet<int> ayrilanIdSet = GetAyrilanPersonelIdSetFromDb();

            string[] durumKolonlari = { "PERSONEL_DURUMU", "DURUM", "AKT_DURUM", "AKTIF", "AKT" };
            string durumKolonu = durumKolonlari.FirstOrDefault(c => dt.Columns.Contains(c));
            DataTable filtered = dt.Clone();
            foreach (DataRow row in dt.Rows)
            {
                int pid = ToInt(row["ID"]);
                if (pid > 0 && ayrilanIdSet.Contains(pid))
                    continue;

                if (!string.IsNullOrWhiteSpace(durumKolonu))
                {
                    string raw = Convert.ToString(row[durumKolonu]) ?? string.Empty;
                    string t = raw.Trim().ToUpperInvariant();
                    bool devam = t.Contains("DEVAM");
                    if (!devam)
                        continue;
                }
                filtered.ImportRow(row);
            }
            return filtered;
        }

        private DataTable GetAktifPersonellerFromDb()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("PERSONEL", typeof(string));
            if (string.IsNullOrWhiteSpace(_connectionString))
                return dt;

            const string sql = @"
;WITH p AS
(
    SELECT
        ID,
        ISNULL(TC_NO,'') AS TC_NO,
        LTRIM(RTRIM(ISNULL(ADI,''))) + ' ' + LTRIM(RTRIM(ISNULL(SOYADI,''))) AS PERSONEL,
        UPPER(LTRIM(RTRIM(ISNULL(PERSONEL_DURUMU,'')))) AS DURUM
    FROM dbo.PERSONEL
),
active_only AS
(
    SELECT *
    FROM p
    WHERE DURUM LIKE N'%DEVAM%'
),
pick AS
(
    SELECT *,
           ROW_NUMBER() OVER (PARTITION BY NULLIF(TC_NO,'') ORDER BY ID DESC) AS rn
    FROM active_only
)
SELECT ID, PERSONEL
FROM pick
WHERE rn = 1
ORDER BY PERSONEL";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    DataTable tmp = new DataTable();
                    da.Fill(tmp);
                    if (!tmp.Columns.Contains("ID") || !tmp.Columns.Contains("PERSONEL"))
                        return dt;

                    foreach (DataRow row in tmp.Rows)
                    {
                        int id = ToInt(row["ID"]);
                        string personel = Convert.ToString(row["PERSONEL"]) ?? string.Empty;
                        if (id <= 0 || string.IsNullOrWhiteSpace(personel))
                            continue;
                        DataRow nr = dt.NewRow();
                        nr["ID"] = id;
                        nr["PERSONEL"] = personel.Trim();
                        dt.Rows.Add(nr);
                    }
                }
            }
            catch
            {
                // DB sorgusu başarısızsa servis fallback devreye girer.
            }

            return dt;
        }

        private HashSet<int> GetAyrilanPersonelIdSetFromDb()
        {
            HashSet<int> set = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(_connectionString))
                return set;

            const string sql = @"
SELECT ID
FROM dbo.PERSONEL
WHERE UPPER(ISNULL(PERSONEL_DURUMU,'')) LIKE N'%AYRILDI%'
   OR UPPER(ISNULL(PERSONEL_DURUMU,'')) LIKE N'%PASIF%'
   OR UPPER(ISNULL(PERSONEL_DURUMU,'')) LIKE N'%PASİF%'";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int id = ToInt(r["ID"]);
                            if (id > 0)
                                set.Add(id);
                        }
                    }
                }
            }
            catch
            {
                // Servis içi filtre fallback olarak devam eder.
            }
            return set;
        }

        private void HazirlaMebbisTab()
        {
            Web_Mebbis.ScriptErrorsSuppressed = true;
            Web_Mebbis.DocumentCompleted -= Web_Mebbis_DocumentCompleted;
            Web_Mebbis.DocumentCompleted += Web_Mebbis_DocumentCompleted;
            Web_Randevu.ScriptErrorsSuppressed = true;
            Web_Randevu.DocumentCompleted -= Web_Randevu_DocumentCompleted;
            Web_Randevu.DocumentCompleted += Web_Randevu_DocumentCompleted;

            Btn_Mebbis_Ac.Click -= Btn_Mebbis_Ac_Click;
            Btn_Mebbis_Ac.Click += Btn_Mebbis_Ac_Click;
        }

        private async void Btn_Randevular_Aktar_Click(object sender, EventArgs e)
        {
            int seciliAracId;
            if (!TryGetSelectedComboId(Combo_Arac_Sec, out seciliAracId))
            {
                MessageBox.Show("Önce araç seçiniz.");
                return;
            }

            DateTime? seciliTarih = GetSelectedSinavDate();
            if (!seciliTarih.HasValue)
            {
                MessageBox.Show("Önce sınav tarihi seçiniz.");
                return;
            }

            if (_currentListe == null || _currentListe.Count == 0)
                await RefreshGridAsync();

            List<RandevuAktarimKaydi> aktarimListesi = BuildRandevuAktarimListesi(seciliAracId, seciliTarih.Value.Date);
            if (aktarimListesi.Count == 0)
            {
                MessageBox.Show("Seçilen araç ve tarihe uygun aktarılacak kayıt bulunamadı.");
                return;
            }

            // HTML yapısı geldikten sonra bu metot içinde form alanlarına yazıp submit edeceğiz.
            int aktarilan = await MebbiseRandevuAktarAsync(aktarimListesi);
            MessageBox.Show("Altyapı hazır. Aktarım kuyruğuna alınan kayıt: " + aktarilan);
        }

        private async void Btn_Randv_Gonder_Click(object sender, EventArgs e)
        {
            await Task.Yield();
            Btn_Randevular_Aktar_Click(sender, e);
        }

        private List<RandevuAktarimKaydi> BuildRandevuAktarimListesi(int aracId, DateTime sinavTarihi)
        {
            Dictionary<int, string> tcMap = GetKursiyerTcMap((_currentListe ?? new List<DireksiyonSinavModel>())
                .Where(x => x != null)
                .Select(x => x.ID_KURSIYER)
                .Distinct()
                .ToList());

            List<RandevuAktarimKaydi> list = new List<RandevuAktarimKaydi>();
            foreach (DireksiyonSinavModel item in _currentListe.Where(x => x != null))
            {
                if (item.SINAV_TARIHI.Date != sinavTarihi)
                    continue;
                if (ToInt(item.AracId) != aracId)
                    continue;

                string tc;
                tcMap.TryGetValue(item.ID_KURSIYER, out tc);
                tc = NormalizeDigits(tc);

                list.Add(new RandevuAktarimKaydi
                {
                    KayitId = item.ID,
                    TcNo = tc,
                    AdSoyad = item.RandevuAdiSoyadi ?? string.Empty,
                    AracId = item.AracId,
                    PersonelId = item.PersonelId,
                    Saat = item.Saat ?? string.Empty,
                    SinavTarihi = item.SINAV_TARIHI.Date
                });
            }
            return list;
        }

        private Task<int> MebbiseRandevuAktarAsync(List<RandevuAktarimKaydi> aktarimListesi)
        {
            return MebbiseRandevuAktarCoreAsync(aktarimListesi);
        }

        private async Task<int> MebbiseRandevuAktarCoreAsync(List<RandevuAktarimKaydi> liste)
        {
            if (liste == null || liste.Count == 0)
                return 0;

            if (Web_Randevu == null || Web_Randevu.Document == null)
            {
                MessageBox.Show("MEBBİS randevu sayfası açık değil.");
                return 0;
            }

            HtmlDocument doc = Web_Randevu.Document;
            if (doc.GetElementById("FRM_SKT01005") == null)
            {
                MessageBox.Show("Randevu formu bulunamadı. SKT01005 sayfasını açınız.");
                return 0;
            }

            HtmlElement dgListe = doc.GetElementById("dgListele") ?? doc.GetElementById("dgListele1");
            if (dgListe == null)
            {
                MessageBox.Show("Aday listesi bulunamadı. Önce MEBBİS'te sınav tarihini seçip Listele yapınız.");
                return 0;
            }

            string seciliAracText = Convert.ToString(Combo_Arac_Sec.Text ?? string.Empty).Trim();
            string seciliPersonelText = Convert.ToString(Combo_Personel_Sec.Text ?? string.Empty).Trim();

            int aktarilan = 0;
            foreach (RandevuAktarimKaydi kayit in liste)
            {
                string tc = NormalizeDigits(kayit.TcNo);
                if (string.IsNullOrWhiteSpace(tc))
                    continue;

                HtmlElement adaySatiri;
                bool bulundu = MebbisAdaySatiriniSec(doc, tc, out adaySatiri);
                if (!bulundu || adaySatiri == null)
                    continue;

                string satirAracText = ResolveLookupTextById(kayit.AracId, _aracMap, seciliAracText);
                string satirPersonelText = ResolveLookupTextById(kayit.PersonelId, _personelMap, seciliPersonelText);

                await Task.Delay(300);
                MebbisAlanlariDoldur(doc, adaySatiri, kayit, satirAracText, satirPersonelText);
                aktarilan++;
            }

            return aktarilan;
        }

        private static string ResolveLookupTextById(int? id, Dictionary<int, string> lookupMap, string fallback)
        {
            int key = id ?? 0;
            if (key > 0 && lookupMap != null)
            {
                string value;
                if (lookupMap.TryGetValue(key, out value) && !string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }
            return (fallback ?? string.Empty).Trim();
        }

        private bool MebbisAdaySatiriniSec(HtmlDocument doc, string tcNo, out HtmlElement adaySatiri)
        {
            adaySatiri = null;
            if (doc == null || string.IsNullOrWhiteSpace(tcNo))
                return false;

            string normalizedTc = NormalizeDigits(tcNo);
            HtmlElement table = doc.GetElementById("dgListele") ?? doc.GetElementById("dgListele1");
            if (table == null)
                return false;

            foreach (HtmlElement tr in table.GetElementsByTagName("tr"))
            {
                bool tcEslesti = false;
                foreach (HtmlElement td in tr.GetElementsByTagName("td"))
                {
                    string cellText = NormalizeDigits(td.InnerText ?? string.Empty);
                    if (cellText == normalizedTc)
                    {
                        tcEslesti = true;
                        break;
                    }
                }
                if (!tcEslesti)
                    continue;

                foreach (HtmlElement input in tr.GetElementsByTagName("input"))
                {
                    string type = (input.GetAttribute("type") ?? string.Empty).ToLowerInvariant();
                    if (type == "checkbox" || type == "radio")
                    {
                        input.SetAttribute("checked", "true");
                        input.InvokeMember("click");
                        adaySatiri = tr;
                        return true;
                    }
                }
                tr.InvokeMember("click");
                adaySatiri = tr;
                return true;
            }

            return false;
        }

        private void MebbisAlanlariDoldur(HtmlDocument doc, HtmlElement adaySatiri, RandevuAktarimKaydi kayit, string seciliAracText, string seciliPersonelText)
        {
            if (doc == null || kayit == null)
                return;

            string tc = NormalizeDigits(kayit.TcNo);
            SetInputValueIfExists(doc, new[] { "txtTC", "txtTc", "txtTcNo", "txtTCKN", "txtKimlikNo" }, tc);
            SetInputValueIfExists(doc, new[] { "txtAdSoyad", "txtAdiSoyadi", "txtAdayAdSoyad" }, kayit.AdSoyad ?? string.Empty);
            SetInputValueIfExists(doc, new[] { "txtSaat", "txtSinavSaati", "txtRandevuSaati" }, kayit.Saat ?? string.Empty);

            DateTime? tarih = kayit.SinavTarihi == DateTime.MinValue ? (DateTime?)null : kayit.SinavTarihi.Date;
            if (tarih.HasValue)
            {
                string tarihText = tarih.Value.ToString("dd.MM.yyyy");
                SetInputValueIfExists(doc, new[] { "txtSinavTarihi", "txtTarih", "txtRandevuTarihi" }, tarihText);
            }

            // SKT01005 satırlarında id'ler dinamik gelir: dgListele_ctlXX_cmbAracPlaka / cmbUstaOgretici
            if (kayit.AracId.HasValue && kayit.AracId.Value > 0)
                SetRowSelectValueByOptionValue(adaySatiri, "cmbAracPlaka", Convert.ToString(kayit.AracId.Value));
            SetRowSelectValueByText(adaySatiri: adaySatiri, idContains: "cmbAracPlaka", targetText: seciliAracText);
            SetRowSelectValueByText(adaySatiri: adaySatiri, idContains: "cmbUstaOgretici", targetText: seciliPersonelText);
            SetRowSelectValueByText(adaySatiri: adaySatiri, idContains: "cmbSinavSaati", targetText: kayit.Saat ?? string.Empty);

            // Fallback: bazı sürümlerde satır dışı/basit id kullanımı olabilir.
            SetSelectValueByText(doc, new[] { "ddlArac", "cmbArac", "drpArac", "ARAC", "ARAC_ID" }, seciliAracText);
            SetSelectValueByText(doc, new[] { "ddlPersonel", "cmbPersonel", "drpPersonel", "USTA_OGRETICI", "PERSONEL_ID" }, seciliPersonelText);
            SetSelectValueByText(doc, new[] { "ddlSinavSaati", "cmbSinavSaati", "drpSinavSaati", "SINAV_SAATI" }, kayit.Saat ?? string.Empty);
        }

        private static void SetInputValueIfExists(HtmlDocument doc, string[] idCandidates, string value)
        {
            if (doc == null || idCandidates == null || idCandidates.Length == 0)
                return;

            foreach (string id in idCandidates)
            {
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                HtmlElement el = doc.GetElementById(id);
                if (el == null)
                    continue;
                el.SetAttribute("value", value ?? string.Empty);
                return;
            }
        }

        private static void SetSelectValueByText(HtmlDocument doc, string[] idCandidates, string targetText)
        {
            if (doc == null || idCandidates == null || idCandidates.Length == 0 || string.IsNullOrWhiteSpace(targetText))
                return;

            string hedef = NormalizeLookupText(targetText);
            foreach (string id in idCandidates)
            {
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                HtmlElement select = doc.GetElementById(id);
                if (select == null || !string.Equals(select.TagName, "SELECT", StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (HtmlElement option in select.GetElementsByTagName("option"))
                {
                    string optText = NormalizeLookupText(option.InnerText ?? option.GetAttribute("value"));
                    if (!string.Equals(optText, hedef, StringComparison.OrdinalIgnoreCase))
                        continue;
                    string val = option.GetAttribute("value");
                    if (!string.IsNullOrWhiteSpace(val))
                        select.SetAttribute("value", val);
                    option.SetAttribute("selected", "selected");
                    select.InvokeMember("onchange");
                    return;
                }
            }
        }

        private static void SetRowSelectValueByText(HtmlElement adaySatiri, string idContains, string targetText)
        {
            if (adaySatiri == null || string.IsNullOrWhiteSpace(idContains) || string.IsNullOrWhiteSpace(targetText))
                return;

            string hedef = NormalizeLookupText(targetText);
            string hedefNoSpace = Regex.Replace(hedef, @"\s+", string.Empty);
            foreach (HtmlElement select in adaySatiri.GetElementsByTagName("select"))
            {
                string id = select.GetAttribute("id") ?? string.Empty;
                string name = select.GetAttribute("name") ?? string.Empty;
                bool alanEslesiyor =
                    id.IndexOf(idContains, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf(idContains, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!alanEslesiyor)
                    continue;

                HtmlElement bestOption = null;
                int bestScore = int.MinValue;
                foreach (HtmlElement option in select.GetElementsByTagName("option"))
                {
                    string raw = option.InnerText ?? option.GetAttribute("value") ?? string.Empty;
                    string optText = NormalizeLookupText(raw);
                    string optNoSpace = Regex.Replace(optText, @"\s+", string.Empty);
                    string optAlphaNum = Regex.Replace(optNoSpace, @"[^A-Z0-9]", string.Empty);
                    string hedefAlphaNum = Regex.Replace(hedefNoSpace, @"[^A-Z0-9]", string.Empty);

                    int score = -1;
                    if (string.Equals(optText, hedef, StringComparison.OrdinalIgnoreCase))
                        score = 100;
                    else if (optText.Contains(hedef) || hedef.Contains(optText))
                        score = 90;
                    else if (!string.IsNullOrWhiteSpace(hedefAlphaNum) &&
                             !string.IsNullOrWhiteSpace(optAlphaNum) &&
                             (optAlphaNum.Contains(hedefAlphaNum) || hedefAlphaNum.Contains(optAlphaNum)))
                        score = 80;
                    else
                    {
                        // Personel seçenekleri çoğu ekranda "İzin No:xxxx AD SOYAD" formatında gelir.
                        string optNameOnly = Regex.Replace(optText, @"^IZIN\s*NO[:\s]*\d+\s*", string.Empty).Trim();
                        if (!string.IsNullOrWhiteSpace(optNameOnly) &&
                            (optNameOnly.Contains(hedef) || hedef.Contains(optNameOnly)))
                            score = 70;
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestOption = option;
                    }
                }

                if (bestOption != null && bestScore >= 70)
                {
                    string val = bestOption.GetAttribute("value");
                    if (!string.IsNullOrWhiteSpace(val))
                        select.SetAttribute("value", val);
                    bestOption.SetAttribute("selected", "selected");
                    select.InvokeMember("onchange");
                    return;
                }
            }
        }

        private static void SetRowSelectValueByOptionValue(HtmlElement adaySatiri, string idContains, string targetValue)
        {
            if (adaySatiri == null || string.IsNullOrWhiteSpace(idContains) || string.IsNullOrWhiteSpace(targetValue))
                return;

            string hedef = (targetValue ?? string.Empty).Trim();
            foreach (HtmlElement select in adaySatiri.GetElementsByTagName("select"))
            {
                string id = select.GetAttribute("id") ?? string.Empty;
                string name = select.GetAttribute("name") ?? string.Empty;
                bool alanEslesiyor =
                    id.IndexOf(idContains, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf(idContains, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!alanEslesiyor)
                    continue;

                foreach (HtmlElement option in select.GetElementsByTagName("option"))
                {
                    string val = (option.GetAttribute("value") ?? string.Empty).Trim();
                    if (!string.Equals(val, hedef, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!string.IsNullOrWhiteSpace(val))
                        select.SetAttribute("value", val);
                    option.SetAttribute("selected", "selected");
                    select.InvokeMember("onchange");
                    return;
                }
            }
        }

        private static bool TryGetSelectedComboId(ComboBox combo, out int id)
        {
            id = 0;
            if (combo == null || combo.SelectedValue == null)
                return false;
            return int.TryParse(Convert.ToString(combo.SelectedValue), out id) && id > 0;
        }

        private void Btn_Mebbis_Ac_Click(object sender, EventArgs e)
        {
            MebbisAc();
        }

        private void MebbisAc()
        {
            try
            {
                string mebbisKullanici;
                string mebbisSifre;
                bool bulundu = MebbisCredentialResolver.TryResolve(_connectionString, AppSession.CurrentUserName, out mebbisKullanici, out mebbisSifre);
                if (!bulundu || string.IsNullOrWhiteSpace(mebbisKullanici))
                {
                    MessageBox.Show("MEBBİS kullanıcı bilgisi bulunamadı.");
                    return;
                }

                _mebbisKullaniciAdi = mebbisKullanici;
                _mebbisSifre = mebbisSifre;
                Web_Mebbis.Navigate("https://mebbis.meb.gov.tr/default.aspx?NoSession");
            }
            catch (Exception ex)
            {
                MessageBox.Show("MEBBİS açılırken hata oluştu: " + ex.Message);
            }
        }

        private void Btn_Meb_Randevu_Ac_Click(object sender, EventArgs e)
        {
            RandevuMebbisAc();
        }

        private void RandevuMebbisAc()
        {
            try
            {
                string aktifUrl = Web_Mebbis?.Url == null ? string.Empty : Web_Mebbis.Url.ToString();
                bool anaMebbisAcik = !string.IsNullOrWhiteSpace(aktifUrl)
                    && aktifUrl.IndexOf("mebbis.meb.gov.tr", StringComparison.OrdinalIgnoreCase) >= 0
                    && aktifUrl.IndexOf("default.aspx", StringComparison.OrdinalIgnoreCase) < 0;

                if (anaMebbisAcik)
                {
                    Web_Randevu.Navigate("https://mebbis.meb.gov.tr/SKT/skt01005.aspx");
                    return;
                }

                string mebbisKullanici;
                string mebbisSifre;
                bool bulundu = MebbisCredentialResolver.TryResolve(_connectionString, AppSession.CurrentUserName, out mebbisKullanici, out mebbisSifre);
                if (!bulundu || string.IsNullOrWhiteSpace(mebbisKullanici))
                {
                    MessageBox.Show("MEBBİS kullanıcı bilgisi bulunamadı.");
                    return;
                }

                _mebbisKullaniciAdi = mebbisKullanici;
                _mebbisSifre = mebbisSifre;
                Web_Randevu.Navigate("https://mebbis.meb.gov.tr/default.aspx?NoSession");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Randevu MEBBİS açılırken hata oluştu: " + ex.Message);
            }
        }

        private void Web_Mebbis_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (Web_Mebbis.Document == null) return;
            if (e.Url != Web_Mebbis.Url) return;
            if (string.IsNullOrWhiteSpace(_mebbisKullaniciAdi) || string.IsNullOrWhiteSpace(_mebbisSifre)) return;

            string url = Web_Mebbis.Url == null ? string.Empty : Web_Mebbis.Url.ToString();
            if (url.IndexOf("default.aspx", StringComparison.OrdinalIgnoreCase) < 0)
                return;

            if ((DateTime.Now - _lastMebbisLoginAttempt).TotalSeconds < 3)
                return;

            _lastMebbisLoginAttempt = DateTime.Now;
            MebbisAutoLogin();
        }

        private void Web_Randevu_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (Web_Randevu.Document == null) return;
            if (e.Url != Web_Randevu.Url) return;

            string url = Web_Randevu.Url == null ? string.Empty : Web_Randevu.Url.ToString();
            if (string.IsNullOrWhiteSpace(_mebbisKullaniciAdi) || string.IsNullOrWhiteSpace(_mebbisSifre))
                return;
            if (url.IndexOf("default.aspx", StringComparison.OrdinalIgnoreCase) < 0)
                return;
            if ((DateTime.Now - _lastMebbisLoginAttempt).TotalSeconds < 3)
                return;

            _lastMebbisLoginAttempt = DateTime.Now;
            MebbisAutoLogin(Web_Randevu);
        }

        private void MebbisAutoLogin()
        {
            MebbisAutoLogin(Web_Mebbis);
        }

        private void MebbisAutoLogin(WebBrowser browser)
        {
            HtmlDocument doc = browser == null ? null : browser.Document;
            if (doc == null) return;

            HtmlElement userBox = null;
            HtmlElement passBox = null;
            foreach (HtmlElement el in doc.GetElementsByTagName("input"))
            {
                string type = (el.GetAttribute("type") ?? string.Empty).ToLowerInvariant();
                if (type == "text" && userBox == null)
                    userBox = el;
                if (type == "password" && passBox == null)
                    passBox = el;
            }

            if (userBox != null)
                userBox.SetAttribute("value", _mebbisKullaniciAdi);
            if (passBox != null)
                passBox.SetAttribute("value", _mebbisSifre);

            foreach (HtmlElement el in doc.GetElementsByTagName("input"))
            {
                string type = (el.GetAttribute("type") ?? string.Empty).ToLowerInvariant();
                if (type == "submit" || type == "button")
                {
                    el.InvokeMember("click");
                    break;
                }
            }
        }
        #endregion

        private async void Btn_Ekle_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            int sinavId;
            if (!TryGetSelectedSinavId(out sinavId))
            {
                MessageBox.Show("Önce geçerli bir sınav tarihi seçiniz.");
                return;
            }

            bool secimIslendi = false;
            var arama = new Arama_Sayfam(_connectionString, null) { Mod = AramaModu.SecimYap };

            Func<int, Task> adayEkle = async (kursiyerId) =>
            {
                if (kursiyerId <= 0)
                    return;

                if (_currentListe != null && _currentListe.Any(x => x != null && x.ID_KURSIYER == kursiyerId))
                {
                    MessageBox.Show("Bu aday seçili sınav listesinde zaten var.");
                    return;
                }

                bool eklendi = false;

                // 1) Servis (varsa) dene.
                try
                {
                    int sonuc = await _service.EkleAsync(sinavId, kursiyerId);
                    eklendi = sonuc > 0;
                }
                catch
                {
                    eklendi = false;
                }

                // 2) Servis basarisizsa silmedeki gibi dogrudan SQL fallback.
                if (!eklendi)
                    eklendi = TryInsertDireksiyonKaydiWithAlternatives(sinavId, kursiyerId, GetSelectedSinavDate());

                if (!eklendi)
                {
                    MessageBox.Show("Aday ekleme işlemi başarısız veya kayıt zaten mevcut.");
                    return;
                }

                LisansPolitikasi.RegisterSuccessfulWrite();
                await RefreshGridAsync();
            };

            arama.KursiyerSecildi += (kursiyerId) =>
            {
                secimIslendi = true;
                _ = adayEkle(kursiyerId);
            };

            arama.ShowDialog();

            // Bazi ortamlarda CellDoubleClick event'i her zaman tetiklenmeyebiliyor.
            // Formdan secilen ID property ile geldiyse yine de eklemeyi garanti et.
            if (!secimIslendi && arama.SecilenKursiyerId > 0)
            {
                try
                {
                    await adayEkle(arama.SecilenKursiyerId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Aday ekleme sırasında hata oluştu: " + ex.Message);
                }
            }
        }
       
        private async void Btn_Adaysil_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            if (Dgv_Listesi.CurrentRow == null) return;

            var model = Dgv_Listesi.CurrentRow.DataBoundItem as DireksiyonSinavModel;
            if (model == null) return;

            var onay = MessageBox.Show("Seçili adayı silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (onay != DialogResult.Yes) return;

            bool silindi = false;
            try
            {
                int sonuc = await _service.SilAsync(model.ID);
                silindi = sonuc > 0;
            }
            catch
            {
                silindi = false;
            }

            if (!silindi)
            {
                int seciliSinavId;
                TryGetSelectedSinavId(out seciliSinavId);
                silindi = TryDeleteDireksiyonKaydiWithFallback(model.ID, model.ID_KURSIYER, seciliSinavId, GetSelectedSinavDate());
            }

            if (!silindi)
            {
                MessageBox.Show("Silme işlemi başarısız.");
                return;
            }

            LisansPolitikasi.RegisterSuccessfulWrite();
            await RefreshGridAsync();
        }

        private bool TryDeleteDireksiyonKaydiWithFallback(int kayitId, int kursiyerId, int sinavId, DateTime? seciliTarih)
        {
            if (!LisansPolitikasi.IsWriteAllowed || string.IsNullOrWhiteSpace(_connectionString))
                return false;

            // 1) ID ile direkt silmeyi dene.
            if (kayitId > 0 && ExecuteDeleteSql("DELETE FROM SINAV_LISTE_DIREKSIYON WHERE ID=@ID", cmd =>
            {
                cmd.Parameters.AddWithValue("@ID", kayitId);
            }))
            {
                return true;
            }

            // 2) ID bilinmiyorsa ya da etkilenmediyse kursiyer+sınav üzerinden dene.
            var sinavIds = new List<int>();
            if (sinavId > 0) sinavIds.Add(sinavId);
            if (seciliTarih.HasValue)
            {
                int byDate = FindDireksiyonSinavTarihiIdByDate(seciliTarih.Value.Date);
                if (byDate > 0 && !sinavIds.Contains(byDate))
                    sinavIds.Add(byDate);
                foreach (int alt in GetAlternativeSinavIdsByDate(seciliTarih.Value.Date, sinavId))
                    if (alt > 0 && !sinavIds.Contains(alt))
                        sinavIds.Add(alt);
            }

            foreach (int id in sinavIds)
            {
                if (id <= 0 || kursiyerId <= 0)
                    continue;

                bool ok = ExecuteDeleteSql(
                    "DELETE FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@ID_SINAV_TARIHI AND ID_KURSIYER=@ID_KURSIYER",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", id);
                        cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                    });
                if (ok)
                    return true;
            }

            return false;
        }

        private bool ExecuteDeleteSql(string sql, Action<SqlCommand> bind)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    bind?.Invoke(cmd);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private async void Btn_Durum_Cek_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            if (Web_Mebbis?.Document == null)
            {
                MessageBox.Show("Önce MEBBİS sayfasını açıp sonuç tablosunu görüntüleyiniz.");
                return;
            }

            int sinavId;
            if (!TryGetSelectedSinavId(out sinavId))
            {
                MessageBox.Show("Önce geçerli sınav tarihi seçiniz.");
                return;
            }

            DateTime? seciliTarih = GetSelectedSinavDate();
            if (!seciliTarih.HasValue)
            {
                MessageBox.Show("Seçili sınav tarihi okunamadı.");
                return;
            }

            DateTime hedefTarih = seciliTarih.Value.Date;
            Dictionary<string, MebbisDireksiyonSonuc> mebbisMap = GetMebbisDireksiyonSonucMap(hedefTarih);
            if (mebbisMap.Count == 0)
            {
                MessageBox.Show("MEBBİS tablosunda seçili tarihe ait sonuç bulunamadı.");
                return;
            }

            Dictionary<int, string> tcMap = BuildTcMapForCurrentListe();
            HashSet<string> listedeOlanTc = new HashSet<string>();
            int guncellenen = 0;
            int bulunamayan = 0;

            foreach (DireksiyonSinavModel item in _currentListe)
            {
                string tc;
                if (!tcMap.TryGetValue(item.ID_KURSIYER, out tc))
                {
                    bulunamayan++;
                    continue;
                }

                tc = NormalizeDigits(tc);
                if (string.IsNullOrWhiteSpace(tc))
                {
                    bulunamayan++;
                    continue;
                }
                listedeOlanTc.Add(tc);

                string dateTcKey = BuildDateTcKey(hedefTarih, tc);
                MebbisDireksiyonSonuc sonuc;
                if (!mebbisMap.TryGetValue(dateTcKey, out sonuc))
                {
                    bulunamayan++;
                    continue;
                }

                string dirDurum = NormalizeDirDurum(sonuc.SinavSonucu);
                if (string.IsNullOrWhiteSpace(dirDurum) || dirDurum == "Girmedi")
                    dirDurum = NormalizeDirDurum(sonuc.PuanDurumu);
                if (!UpdateDireksiyonDurum(item.ID, dirDurum))
                {
                    bulunamayan++;
                    continue;
                }

                item.DIR_DURUM = dirDurum;
                guncellenen++;
            }

            List<MebbisDireksiyonSonuc> olmayanlar = mebbisMap.Values
                .Where(x => x != null && x.SinavTarihi.Date == hedefTarih)
                .Where(x => !listedeOlanTc.Contains(x.TcNo))
                .ToList();
            int yeniEklenen = 0;
            int eslesmeyenTc = 0;

            if (olmayanlar.Count > 0)
            {
                DialogResult onay = MessageBox.Show(
                    "MEBBİS'te listede olmayan " + olmayanlar.Count + " aday var. Sınav listesine eklensin mi?",
                    "Eksik Adaylar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (onay == DialogResult.Yes)
                {
                    foreach (MebbisDireksiyonSonuc sonuc in olmayanlar)
                    {
                        int kursiyerId = GetKursiyerIdByTc(sonuc.TcNo);
                        if (kursiyerId <= 0)
                        {
                            eslesmeyenTc++;
                            continue;
                        }

                        if (!InsertDireksiyonKaydi(sinavId, kursiyerId))
                            continue;

                        string dirDurum = NormalizeDirDurum(sonuc.SinavSonucu);
                        if (string.IsNullOrWhiteSpace(dirDurum) || dirDurum == "Girmedi")
                            dirDurum = NormalizeDirDurum(sonuc.PuanDurumu);
                        UpdateDireksiyonDurumByPair(sinavId, kursiyerId, dirDurum);
                        yeniEklenen++;
                    }
                }
            }

            await RefreshGridAsync();
            MessageBox.Show(
                "MEBBİS durum çekme tamamlandı.\n" +
                "Güncellenen: " + guncellenen + "\n" +
                "Bulunamayan: " + bulunamayan + "\n" +
                "Yeni Eklenen: " + yeniEklenen + "\n" +
                "TC Eşleşmeyen: " + eslesmeyenTc);
        }

        private void Btn_SMS_Hazirla_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentListe == null || _currentListe.Count == 0)
                {
                    MessageBox.Show("SMS listesi olusturmak icin once listeyi getiriniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DateTime tarih = GetSelectedSinavDate() ?? DateTime.Today;
                string masaustu = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string dosyaAdi = tarih.ToString("dd.MM.yyyy") + "_smsatilacak.xlsx";
                string dosyaYolu = Path.Combine(masaustu, dosyaAdi);

                string sablon = LoadSmsTemplate();
                var smsOnizleme = BuildDireksiyonSmsOnizlemeVerisi(tarih);
                using (var sablonForm = new SmsSablonDuzenleForm(sablon, BuildSmsPreviewText(sablon, tarih), smsOnizleme, _connectionString))
                {
                    if (sablonForm.ShowDialog(this) != DialogResult.OK)
                        return;
                    sablon = sablonForm.TemplateText;
                }
                SaveSmsTemplate(sablon);

                ExportSmsToXlsx(dosyaYolu, tarih, sablon);
                MessageBox.Show("SMS excel dosyasi olusturuldu:\n" + dosyaYolu, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start(new ProcessStartInfo
                {
                    FileName = dosyaYolu,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("SMS excel olusturma hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportSmsToXlsx(string filePath, DateTime sinavTarihi, string templateText)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            List<int> kursiyerIds = _currentListe
                .Where(x => x != null && x.ID_KURSIYER > 0)
                .Select(x => x.ID_KURSIYER)
                .Distinct()
                .ToList();
            Dictionary<int, string> gsmMap = GetKursiyerGsmMap(kursiyerIds);
            string kursAdi;
            string telefon;
            GetSmsKurumBilgi(out kursAdi, out telefon);
            List<Tuple<string, string>> smsRows = BuildSmsRows(gsmMap, kursAdi, telefon, sinavTarihi, templateText);
            if (smsRows.Count == 0)
                throw new InvalidOperationException("SMS icin uygun telefon bulunamadi.");

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite))
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create, false, Encoding.UTF8))
            {
                WriteZipText(archive, "[Content_Types].xml",
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
  <Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
</Types>");

                WriteZipText(archive, "_rels/.rels",
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>");

                WriteZipText(archive, "xl/workbook.xml",
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">
  <sheets>
    <sheet name=""SMS"" sheetId=""1"" r:id=""rId1""/>
  </sheets>
</workbook>");

                WriteZipText(archive, "xl/_rels/workbook.xml.rels",
@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
</Relationships>");

                string worksheetXml = BuildSmsWorksheetXml(smsRows);
                WriteZipText(archive, "xl/worksheets/sheet1.xml", worksheetXml);
            }
        }

        private string BuildSmsWorksheetXml(List<Tuple<string, string>> smsRows)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>");
            sb.Append(@"<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main""><sheetData>");

            // Header row: Telefon + Mesaj
            sb.Append(@"<row r=""1"">");
            sb.Append(@"<c r=""A1"" t=""inlineStr""><is><t>Telefon</t></is></c>");
            sb.Append(@"<c r=""B1"" t=""inlineStr""><is><t>Mesaj</t></is></c>");
            sb.Append("</row>");

            int rowIndex = 2;
            foreach (var row in smsRows)
            {
                sb.Append(@"<row r=""" + rowIndex + @""">");
                sb.Append(@"<c r=""A" + rowIndex + @""" t=""inlineStr""><is><t xml:space=""preserve"">" + EscapeXml(row.Item1) + "</t></is></c>");
                sb.Append(@"<c r=""B" + rowIndex + @""" t=""inlineStr""><is><t xml:space=""preserve"">" + EscapeXml(row.Item2) + "</t></is></c>");
                sb.Append("</row>");
                rowIndex++;
            }

            sb.Append("</sheetData></worksheet>");
            return sb.ToString();
        }

        private static void WriteZipText(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(true)))
            {
                writer.Write(content);
            }
        }

        private static string ExcelColName(int index)
        {
            string col = string.Empty;
            while (index > 0)
            {
                int rem = (index - 1) % 26;
                col = (char)('A' + rem) + col;
                index = (index - 1) / 26;
            }
            return col;
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        private List<Tuple<string, string>> BuildSmsRows(Dictionary<int, string> gsmMap, string kursAdi, string telefon, DateTime sinavTarihi, string templateText)
        {
            List<Tuple<string, string>> rows = new List<Tuple<string, string>>();
            foreach (var item in _currentListe.Where(x => x != null))
            {
                string gsm;
                if (!gsmMap.TryGetValue(item.ID_KURSIYER, out gsm))
                    continue;

                gsm = NormalizeDigits(gsm);
                if (gsm.Length < 10)
                    continue;

                string mesaj = BuildSmsText(templateText, item, kursAdi, telefon, sinavTarihi);

                rows.Add(Tuple.Create(gsm, mesaj));
            }
            return rows;
        }

        private string BuildSmsPreviewText(string templateText, DateTime sinavTarihi)
        {
            var first = _currentListe == null ? null : _currentListe.FirstOrDefault(x => x != null);
            if (first == null)
                return string.Empty;
            string kursAdi;
            string telefon;
            GetSmsKurumBilgi(out kursAdi, out telefon);
            return BuildSmsText(templateText, first, kursAdi, telefon, sinavTarihi);
        }

        private SmsSablonOnizlemeVerisi BuildDireksiyonSmsOnizlemeVerisi(DateTime sinavTarihi)
        {
            var first = _currentListe == null ? null : _currentListe.FirstOrDefault(x => x != null);
            string kursAdi;
            string telefon;
            GetSmsKurumBilgi(out kursAdi, out telefon);
            return new SmsSablonOnizlemeVerisi
            {
                AdSoyad = (first == null ? string.Empty : (first.RandevuAdiSoyadi ?? string.Empty).Trim().ToUpperInvariant()),
                Telefon = (telefon ?? string.Empty).Trim(),
                KursAdi = (kursAdi ?? string.Empty).Trim(),
                Tarih = sinavTarihi.Date,
                Saat = first == null ? string.Empty : NormalizeSmsSaat(first.Saat)
            };
        }

        private string BuildSmsText(string templateText, DireksiyonSinavModel item, string kursAdi, string telefon, DateTime sinavTarihi)
        {
            string sablon = string.IsNullOrWhiteSpace(templateText) ? DefaultSmsTemplate : templateText;
            string adSoyad = (item == null ? string.Empty : (item.RandevuAdiSoyadi ?? string.Empty)).Trim().ToUpperInvariant();
            string saat = NormalizeSmsSaat(item == null ? string.Empty : item.Saat);

            return sablon
                .Replace("[AD SOYAD]", adSoyad)
                .Replace("[TARIH]", sinavTarihi.ToString("dd.MM.yyyy"))
                .Replace("[SAAT]", saat)
                .Replace("[KURS ADI]", (kursAdi ?? string.Empty).Trim())
                .Replace("[TELEFON]", (telefon ?? string.Empty).Trim());
        }

        private void GetSmsKurumBilgi(out string kursAdi, out string telefon)
        {
            kursAdi = "METRO SURUCU KURSLARI";
            telefon = string.Empty;
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            string tableName = ResolveKursBilgiTableName();
            if (string.IsNullOrWhiteSpace(tableName))
                return;
            string sql = "SELECT TOP (1) ISNULL(KURS_ADI,'') AS KURS_ADI, ISNULL(TELEFON,'') AS TELEFON FROM [" + tableName + "]";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return;
                        kursAdi = Convert.ToString(reader["KURS_ADI"]).Trim().ToUpperInvariant();
                        telefon = NormalizeDigits(Convert.ToString(reader["TELEFON"]));
                        if (string.IsNullOrWhiteSpace(kursAdi))
                            kursAdi = "METRO SURUCU KURSLARI";
                    }
                }
            }
            catch
            {
                // varsayilan degerler.
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

        private Dictionary<int, string> GetKursiyerGsmMap(List<int> ids)
        {
            Dictionary<int, string> map = new Dictionary<int, string>();
            if (ids == null || ids.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return map;

            string idList = string.Join(",", ids.Where(x => x > 0).Distinct());
            if (string.IsNullOrWhiteSpace(idList))
                return map;

            string[] sqlAdaylari =
            {
                "SELECT ID, ISNULL(GSM_1,'') AS GSM FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(GSM_1,'') AS GSM FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(GSM_1,'') AS GSM FROM dbo.KURSİYER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(GSM,'') AS GSM FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(GSM,'') AS GSM FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                int id = ToInt(r["ID"]);
                                string gsm = Convert.ToString(r["GSM"]);
                                if (id > 0 && !string.IsNullOrWhiteSpace(gsm) && !map.ContainsKey(id))
                                    map.Add(id, gsm);
                            }
                        }
                    }
                    if (map.Count > 0) return map;
                }
                catch
                {
                    // Sonraki SQL adayi.
                }
            }
            return map;
        }

        private static string NormalizeSmsSaat(string rawSaat)
        {
            string text = (rawSaat ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text))
                return "00:00";

            Match m = Regex.Match(text, @"\b\d{1,2}:\d{2}\b");
            if (!m.Success)
                return "00:00";

            string[] parts = m.Value.Split(':');
            int hour;
            int minute;
            if (!int.TryParse(parts[0], out hour) || !int.TryParse(parts[1], out minute))
                return "00:00";
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59)
                return "00:00";

            return hour.ToString("00") + ":" + minute.ToString("00");
        }

        private const string DefaultSmsTemplate =
"SAYIN [AD SOYAD]; [TARIH] YARIN SAAT:[SAAT] DIREKSIYON SINAVINIZ VARDIR. SINAV SAATINDEN YARIM SAAT ONCE DIREKSIYON EGITIM PISTINDE KIMLIGINIZLE BULUNMANIZ ONEMLE RICA OLUNUR. [KURS ADI] [TELEFON]";

        private string GetSmsTemplatePath()
        {
            return Path.Combine(Application.StartupPath, "sms_sablon.txt");
        }

        private string LoadSmsTemplate()
        {
            string path = GetSmsTemplatePath();
            if (!File.Exists(path))
                return DefaultSmsTemplate;
            string text = File.ReadAllText(path, Encoding.UTF8).Trim();
            return string.IsNullOrWhiteSpace(text) ? DefaultSmsTemplate : text;
        }

        private void SaveSmsTemplate(string templateText)
        {
            string path = GetSmsTemplatePath();
            string text = string.IsNullOrWhiteSpace(templateText) ? DefaultSmsTemplate : templateText.Trim();
            File.WriteAllText(path, text, Encoding.UTF8);
        }

        private void TryBindRandevuCekButton()
        {
            Button btn = FindControlRecursive(this, "Btn_Randevular_Cek") as Button;
            if (btn == null)
                btn = FindControlRecursive(this, "Btn_Cek") as Button;
            if (btn == null)
                return;

            btn.Click -= Btn_Randevular_Cek_Click;
            btn.Click += Btn_Randevular_Cek_Click;
        }

        private void TryBindRandevuAktarButton()
        {
            Button btn = FindControlRecursive(this, "Btn_Randevular_Aktar") as Button;
            if (btn == null)
                btn = FindControlRecursive(this, "Btn_Aktar") as Button;
            if (btn == null)
                return;

            btn.Click -= Btn_Randevular_Aktar_Click;
            btn.Click += Btn_Randevular_Aktar_Click;
        }

        private static Control FindControlRecursive(Control root, string name)
        {
            if (root == null || string.IsNullOrWhiteSpace(name))
                return null;
            foreach (Control child in root.Controls)
            {
                if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                    return child;
                Control found = FindControlRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private async void Btn_Randevular_Cek_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            HtmlDocument kaynakDoc = null;
            if (Web_Randevu?.Document != null && GetMebbisRandevuTable(Web_Randevu.Document) != null)
                kaynakDoc = Web_Randevu.Document;
            else if (Web_Mebbis?.Document != null && GetMebbisRandevuTable(Web_Mebbis.Document) != null)
                kaynakDoc = Web_Mebbis.Document;

            if (kaynakDoc == null)
            {
                MessageBox.Show("Önce MEBBİS'e girip randevu listesini açınız (SKT01005 / dgListele).");
                return;
            }

            Dictionary<string, MebbisRandevuBilgi> randevuMap = GetMebbisRandevuMap(kaynakDoc);
            if (randevuMap.Count == 0)
            {
                MessageBox.Show("MEBBİS randevu tablosunda eşleşecek kayıt bulunamadı.");
                return;
            }

            DataTable personeller = GetPersonellerSafe();
            DataTable araclar = GetAraclarSafe();
            DataTable saatler = GetSaatlerSafe();
            Dictionary<string, int> personelIdMap = BuildLookupIdMap(personeller, "PERSONEL");
            Dictionary<string, int> aracIdMap = BuildLookupIdMap(araclar, "ARAC_PLAKA");
            HashSet<string> saatSet = BuildSaatSet(saatler);

            Dictionary<int, string> tcMap = BuildTcMapForCurrentListe();
            int guncellenen = 0;
            int eslesenTcYok = 0;
            int personelEslesmedi = 0;
            int aracEslesmedi = 0;
            List<string> personelEslesmeyenDetay = new List<string>();
            List<string> aracEslesmeyenDetay = new List<string>();

            foreach (DireksiyonSinavModel item in _currentListe)
            {
                string tc;
                if (!tcMap.TryGetValue(item.ID_KURSIYER, out tc))
                {
                    eslesenTcYok++;
                    continue;
                }
                tc = NormalizeDigits(tc);

                MebbisRandevuBilgi bilgi;
                if (!randevuMap.TryGetValue(tc, out bilgi))
                {
                    eslesenTcYok++;
                    continue;
                }

                bool changed = false;
                int personelId;
                if (personelIdMap.TryGetValue(NormalizeLookupText(bilgi.UstaOgretici), out personelId) && personelId > 0)
                {
                    if (item.PersonelId != personelId)
                    {
                        item.PersonelId = personelId;
                        changed = true;
                    }
                }
                else
                {
                    personelEslesmedi++;
                    if (personelEslesmeyenDetay.Count < 20)
                        personelEslesmeyenDetay.Add(tc + " -> " + bilgi.UstaOgretici);
                }

                int aracId;
                if (aracIdMap.TryGetValue(NormalizeLookupText(bilgi.AracPlaka), out aracId) && aracId > 0)
                {
                    if (item.AracId != aracId)
                    {
                        item.AracId = aracId;
                        changed = true;
                    }
                }
                else
                {
                    aracEslesmedi++;
                    if (aracEslesmeyenDetay.Count < 20)
                        aracEslesmeyenDetay.Add(tc + " -> " + bilgi.AracPlaka);
                }

                string saat = NormalizeSaatText(bilgi.SinavSaati);
                if (!string.IsNullOrWhiteSpace(saat))
                {
                    if (saatSet.Count == 0 || SaatSetContains(saatSet, saat))
                    {
                        if (!string.Equals(item.Saat ?? string.Empty, saat, StringComparison.OrdinalIgnoreCase))
                        {
                            item.Saat = saat;
                            changed = true;
                        }
                    }
                }

                if (!changed)
                    continue;

                int sonuc = await _service.GuncelleAsync(item);
                if (sonuc > 0)
                {
                    LisansPolitikasi.RegisterSuccessfulWrite();
                    guncellenen++;
                }
            }

            await RefreshGridAsync();
            string rapor =
                "Randevu aktarımı tamamlandı.\n" +
                "Güncellenen: " + guncellenen + "\n" +
                "Listede eşleşmeyen TC: " + eslesenTcYok + "\n" +
                "Personel eşleşmeyen: " + personelEslesmedi + "\n" +
                "Araç eşleşmeyen: " + aracEslesmedi;

            if (personelEslesmeyenDetay.Count > 0)
                rapor += "\n\nPersonel Eşleşmeyenler (ilk " + personelEslesmeyenDetay.Count + "):\n- "
                    + string.Join("\n- ", personelEslesmeyenDetay);
            if (aracEslesmeyenDetay.Count > 0)
                rapor += "\n\nAraç Eşleşmeyenler (ilk " + aracEslesmeyenDetay.Count + "):\n- "
                    + string.Join("\n- ", aracEslesmeyenDetay);

            MessageBox.Show(rapor);
        }

        private Dictionary<int, string> GetTcMapFromDgvListesi()
        {
            Dictionary<int, string> map = new Dictionary<int, string>();
            if (Dgv_Listesi == null || Dgv_Listesi.Rows.Count == 0)
                return map;

            foreach (DataGridViewRow row in Dgv_Listesi.Rows)
            {
                if (row == null || row.IsNewRow)
                    continue;

                DireksiyonSinavModel model = row.DataBoundItem as DireksiyonSinavModel;
                if (model == null || model.ID_KURSIYER <= 0)
                    continue;

                string tc = string.Empty;
                if (Dgv_Listesi.Columns.Contains("TC_NO"))
                    tc = NormalizeDigits(Convert.ToString(row.Cells["TC_NO"]?.Value));

                if (string.IsNullOrWhiteSpace(tc))
                    tc = NormalizeDigits(GetPropValue(model, "TC_NO", "TcNo", "RandevuTcNo"));

                if (tc.Length == 11 && !map.ContainsKey(model.ID_KURSIYER))
                    map.Add(model.ID_KURSIYER, tc);
            }

            return map;
        }

        private Dictionary<string, MebbisRandevuBilgi> GetMebbisRandevuMap(HtmlDocument doc)
        {
            Dictionary<string, MebbisRandevuBilgi> map = new Dictionary<string, MebbisRandevuBilgi>();
            if (doc == null)
                return map;

            HtmlElement table = GetMebbisRandevuTable(doc);
            if (table == null)
                return map;

            foreach (HtmlElement tr in table.GetElementsByTagName("tr"))
            {
                List<string> cells = new List<string>();
                foreach (HtmlElement td in tr.GetElementsByTagName("td"))
                    cells.Add((td.InnerText ?? string.Empty).Trim());

                if (cells.Count == 0)
                    continue;

                string tc = ExtractTcFromCells(cells);
                if (tc.Length != 11)
                    continue;

                DateTime tarih;
                if (!TryExtractDateFromCells(cells, out tarih))
                    continue;

                int tarihIndex = -1;
                for (int i = 0; i < cells.Count; i++)
                {
                    if (Regex.IsMatch(cells[i] ?? string.Empty, @"\b\d{1,2}[./-]\d{1,2}[./-]\d{2,4}\b"))
                    {
                        tarihIndex = i;
                        break;
                    }
                }

                string arac = (cells.Count > 5 ? cells[5] : string.Empty) ?? string.Empty;
                string usta = (cells.Count > 6 ? cells[6] : string.Empty) ?? string.Empty;
                if (tarihIndex >= 2)
                {
                    arac = (cells[tarihIndex - 2] ?? string.Empty).Trim();
                    usta = (cells[tarihIndex - 1] ?? string.Empty).Trim();
                }

                string saatText = string.Empty;
                if (tarihIndex >= 0 && tarihIndex + 1 < cells.Count)
                {
                    string s1 = cells[tarihIndex + 1] ?? string.Empty;
                    string s2 = (tarihIndex + 2 < cells.Count) ? (cells[tarihIndex + 2] ?? string.Empty) : string.Empty;
                    saatText = (s1 + " " + s2).Trim();
                }
                if (string.IsNullOrWhiteSpace(saatText) && cells.Count > 8)
                    saatText = cells[8] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(saatText))
                    saatText = string.Join(" ", cells);

                string saat = ExtractPrimarySaat(saatText);

                MebbisRandevuBilgi bilgi = new MebbisRandevuBilgi
                {
                    TcNo = tc,
                    AracPlaka = arac,
                    UstaOgretici = usta,
                    SinavTarihi = tarih.Date,
                    SinavSaati = saat
                };
                map[tc] = bilgi;
            }

            return map;
        }

        private static HtmlElement GetMebbisRandevuTable(HtmlDocument doc)
        {
            if (doc == null)
                return null;
            return doc.GetElementById("dgListele") ?? doc.GetElementById("dgListele1");
        }

        private static Dictionary<string, int> BuildLookupIdMap(DataTable dt, string textColumn)
        {
            Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (dt == null || !dt.Columns.Contains("ID") || !dt.Columns.Contains(textColumn))
                return map;

            foreach (DataRow row in dt.Rows)
            {
                int id = ToInt(row["ID"]);
                if (id <= 0)
                    continue;
                string text = NormalizeLookupText(Convert.ToString(row[textColumn]));
                if (string.IsNullOrWhiteSpace(text))
                    continue;
                if (!map.ContainsKey(text))
                    map.Add(text, id);
            }
            return map;
        }

        private static HashSet<string> BuildSaatSet(DataTable saatler)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (saatler == null || !saatler.Columns.Contains("SAAT"))
                return set;

            foreach (DataRow row in saatler.Rows)
            {
                string saat = NormalizeSaatText(Convert.ToString(row["SAAT"]));
                if (!string.IsNullOrWhiteSpace(saat))
                    set.Add(saat);
            }
            return set;
        }

        private static bool SaatSetContains(HashSet<string> saatSet, string saat)
        {
            if (saatSet == null || saatSet.Count == 0)
                return true;

            string hedef = ExtractPrimarySaat(saat);
            foreach (string item in saatSet)
            {
                if (string.Equals(item ?? string.Empty, saat ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                    return true;

                string itemPrimary = ExtractPrimarySaat(item);
                if (!string.IsNullOrWhiteSpace(hedef) &&
                    string.Equals(itemPrimary, hedef, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string NormalizeLookupText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            string t = value.Trim().ToUpperInvariant();
            t = t.Replace("İ", "I").Replace("I\u0307", "I");
            t = t.Replace("Ğ", "G").Replace("Ü", "U").Replace("Ş", "S").Replace("Ö", "O").Replace("Ç", "C");
            while (t.Contains("  "))
                t = t.Replace("  ", " ");
            return t;
        }

        private static string NormalizeSaatText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            string t = value.Trim();
            while (t.Contains("  "))
                t = t.Replace("  ", " ");
            return t;
        }

        private static string ExtractPrimarySaat(string value)
        {
            string text = NormalizeSaatText(value);
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            Match m = Regex.Match(text, @"\b([01]?\d|2[0-3])[:.]([0-5]\d)\b");
            if (!m.Success)
                return text;
            return m.Groups[1].Value.PadLeft(2, '0') + ":" + m.Groups[2].Value;
        }

        private Dictionary<string, MebbisDireksiyonSonuc> GetMebbisDireksiyonSonucMap(DateTime? seciliTarih)
        {
            Dictionary<string, MebbisDireksiyonSonuc> map = new Dictionary<string, MebbisDireksiyonSonuc>();
            HtmlDocument doc = Web_Mebbis.Document;
            if (doc == null) return map;

            foreach (HtmlElement tr in doc.GetElementsByTagName("tr"))
            {
                List<string> cells = new List<string>();
                foreach (HtmlElement td in tr.GetElementsByTagName("td"))
                {
                    string val = (td.InnerText ?? string.Empty).Trim();
                    if (!string.IsNullOrWhiteSpace(val))
                        cells.Add(val);
                }
                if (cells.Count == 0) continue;

                string tc = ExtractTcFromCells(cells);
                if (string.IsNullOrWhiteSpace(tc)) continue;

                DateTime tarih;
                if (!TryExtractDateFromCells(cells, out tarih)) continue;
                if (seciliTarih.HasValue && tarih.Date != seciliTarih.Value.Date) continue;

                string sinavSonucu = ExtractDireksiyonDurumFromCells(cells);
                string puanDurumu = ExtractPuanFromCells(cells);
                string onayDurumu = string.Join(" ", cells);
                if (string.IsNullOrWhiteSpace(sinavSonucu) && string.IsNullOrWhiteSpace(puanDurumu))
                    continue;

                MebbisDireksiyonSonuc sonuc = new MebbisDireksiyonSonuc
                {
                    TcNo = tc,
                    SinavTarihi = tarih.Date,
                    PuanDurumu = puanDurumu,
                    OnayDurumu = onayDurumu,
                    SinavSonucu = sinavSonucu
                };
                map[BuildDateTcKey(tarih.Date, tc)] = sonuc;
            }

            return map;
        }

        private static string BuildDateTcKey(DateTime tarih, string tcNo)
        {
            string tc = NormalizeDigits(tcNo);
            return tarih.Date.ToString("yyyyMMdd") + "|" + tc;
        }

        private Dictionary<int, string> BuildTcMapForCurrentListe()
        {
            var map = new Dictionary<int, string>();
            if (_currentListe == null || _currentListe.Count == 0)
                return map;

            foreach (var item in _currentListe.Where(x => x != null && x.ID_KURSIYER > 0))
            {
                string tc = NormalizeDigits(GetPropValue(item, "TC_NO", "TcNo", "RandevuTcNo"));
                if (tc.Length == 11 && !map.ContainsKey(item.ID_KURSIYER))
                    map[item.ID_KURSIYER] = tc;
            }

            List<int> eksikIds = _currentListe
                .Where(x => x != null && x.ID_KURSIYER > 0 && !map.ContainsKey(x.ID_KURSIYER))
                .Select(x => x.ID_KURSIYER)
                .Distinct()
                .ToList();
            if (eksikIds.Count > 0)
            {
                Dictionary<int, string> dbMap = GetKursiyerTcMap(eksikIds);
                foreach (var kv in dbMap)
                {
                    string tc = NormalizeDigits(kv.Value);
                    if (kv.Key > 0 && tc.Length == 11 && !map.ContainsKey(kv.Key))
                        map[kv.Key] = tc;
                }
            }

            return map;
        }

        private static string ExtractTcFromCells(List<string> cells)
        {
            if (cells == null || cells.Count == 0)
                return string.Empty;

            foreach (string cell in cells)
            {
                Match m = Regex.Match(cell ?? string.Empty, @"\b\d{11}\b");
                if (m.Success)
                    return NormalizeDigits(m.Value);
            }

            return string.Empty;
        }

        private static bool TryExtractDateFromCells(List<string> cells, out DateTime tarih)
        {
            tarih = DateTime.MinValue;
            if (cells == null || cells.Count == 0)
                return false;

            foreach (string cell in cells)
            {
                Match m = Regex.Match(cell ?? string.Empty, @"\b\d{1,2}[./-]\d{1,2}[./-]\d{2,4}\b");
                if (!m.Success)
                    continue;

                if (DateTime.TryParse(m.Value, out tarih))
                    return true;
            }

            return false;
        }

        private static string ExtractPuanFromCells(List<string> cells)
        {
            if (cells == null || cells.Count == 0)
                return string.Empty;

            for (int i = cells.Count - 1; i >= 0; i--)
            {
                string token = (cells[i] ?? string.Empty).Trim();
                int n;
                if (Regex.IsMatch(token, @"^\d{1,3}$") && int.TryParse(token, out n) && n >= 0 && n <= 100)
                    return token;
            }
            return string.Empty;
        }

        private static string ExtractDireksiyonDurumFromCells(List<string> cells)
        {
            if (cells == null || cells.Count == 0)
                return string.Empty;

            for (int i = cells.Count - 1; i >= 0; i--)
            {
                string raw = (cells[i] ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                string norm = NormalizeDirDurum(raw);
                string upper = norm.ToUpperInvariant();
                if (upper.Contains("BAŞARILI") || upper.Contains("BASARILI") ||
                    upper.Contains("BAŞARISIZ") || upper.Contains("BASARISIZ") ||
                    upper.Contains("GIRMEDI") || upper.Contains("GİRMEDİ") ||
                    upper.Contains("GIRDI") || upper.Contains("GİRDİ") ||
                    upper.Contains("KALDI") || upper.Contains("GEÇTİ") || upper.Contains("GECTI"))
                    return norm;
            }

            return string.Empty;
        }

        private static string NormalizeDirDurum(string raw)
        {
            string t = (raw ?? string.Empty).Trim();
            string u = t.ToUpperInvariant();
            if (u.Contains("BAŞARILI") || u.Contains("BASARILI")) return "Başarılı";
            if (u.Contains("BAŞARISIZ") || u.Contains("BASARISIZ")) return "Başarısız";
            if (u.Contains("GİRMEDİ") || u.Contains("GIRMEDI")) return "Girmedi";
            if (u.Contains("GİRDİ") || u.Contains("GIRDI")) return "Girdi";
            return string.IsNullOrWhiteSpace(t) ? "Girmedi" : t;
        }

        private bool UpdateDireksiyonDurum(int kayitId, string dirDurum)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (kayitId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return false;

            const string sql = "UPDATE SINAV_LISTE_DIREKSIYON SET DIR_DURUM=@DIR_DURUM WHERE ID=@ID";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", kayitId);
                    cmd.Parameters.AddWithValue("@DIR_DURUM", string.IsNullOrWhiteSpace(dirDurum) ? (object)DBNull.Value : dirDurum);
                    conn.Open();
                    var ok = cmd.ExecuteNonQuery() > 0;
                    if (ok) LisansPolitikasi.RegisterSuccessfulWrite();
                    return ok;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool UpdateDireksiyonDurumByPair(int sinavId, int kursiyerId, string dirDurum)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (sinavId <= 0 || kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return false;

            const string sql = @"
UPDATE SINAV_LISTE_DIREKSIYON
SET DIR_DURUM=@DIR_DURUM
WHERE ID_SINAV_TARIHI=@ID_SINAV_TARIHI
  AND ID_KURSIYER=@ID_KURSIYER";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavId);
                    cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                    cmd.Parameters.AddWithValue("@DIR_DURUM", string.IsNullOrWhiteSpace(dirDurum) ? (object)DBNull.Value : dirDurum);
                    conn.Open();
                    var ok = cmd.ExecuteNonQuery() > 0;
                    if (ok) LisansPolitikasi.RegisterSuccessfulWrite();
                    return ok;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool InsertDireksiyonKaydi(int sinavId, int kursiyerId)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (sinavId <= 0 || kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return false;

            if (ExistsDireksiyonKaydi(sinavId, kursiyerId))
                return true;

            string[] sqlAdaylari =
            {
                @"IF NOT EXISTS (SELECT 1 FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@ID_SINAV_TARIHI AND ID_KURSIYER=@ID_KURSIYER)
                  INSERT INTO SINAV_LISTE_DIREKSIYON (ID_SINAV_TARIHI, ID_KURSIYER) VALUES (@ID_SINAV_TARIHI, @ID_KURSIYER)",
                @"IF NOT EXISTS (SELECT 1 FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@ID_SINAV_TARIHI AND ID_KURSIYER=@ID_KURSIYER)
                  INSERT INTO SINAV_LISTE_DIREKSIYON (ID_SINAV_TARIHI, ID_KURSIYER, DIR_HAK, DIR_DURUM) VALUES (@ID_SINAV_TARIHI, @ID_KURSIYER, 0, 'GIRMEDI')",
                @"IF NOT EXISTS (SELECT 1 FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@ID_SINAV_TARIHI AND ID_KURSIYER=@ID_KURSIYER)
                  INSERT INTO SINAV_LISTE_DIREKSIYON (ID_SINAV_TARIHI, ID_KURSIYER, DIR_HAK) VALUES (@ID_SINAV_TARIHI, @ID_KURSIYER, 0)"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavId);
                        cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                        conn.Open();
                        int affected = cmd.ExecuteNonQuery();
                        if (affected > 0 || ExistsDireksiyonKaydi(sinavId, kursiyerId))
                        {
                            LisansPolitikasi.RegisterSuccessfulWrite();
                            return true;
                        }
                    }
                }
                catch
                {
                    // sonraki SQL adayi
                }
            }
            return false;
        }

        private bool TryInsertDireksiyonKaydiWithAlternatives(int sinavId, int kursiyerId, DateTime? seciliTarih)
        {
            var adayIds = new List<int>();
            if (sinavId > 0)
                adayIds.Add(sinavId);

            if (seciliTarih.HasValue)
            {
                int byDateId = FindDireksiyonSinavTarihiIdByDate(seciliTarih.Value.Date);
                if (byDateId > 0 && !adayIds.Contains(byDateId))
                    adayIds.Add(byDateId);

                foreach (int altId in GetAlternativeSinavIdsByDate(seciliTarih.Value.Date, sinavId))
                {
                    if (altId > 0 && !adayIds.Contains(altId))
                        adayIds.Add(altId);
                }
            }

            foreach (int id in adayIds)
            {
                if (InsertDireksiyonKaydi(id, kursiyerId))
                    return true;
            }

            return false;
        }

        private bool ExistsDireksiyonKaydi(int sinavId, int kursiyerId)
        {
            if (sinavId <= 0 || kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                return false;

            const string sql = @"SELECT TOP 1 1 FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@ID_SINAV_TARIHI AND ID_KURSIYER=@ID_KURSIYER";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavId);
                    cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                    conn.Open();
                    object val = cmd.ExecuteScalar();
                    return val != null && val != DBNull.Value;
                }
            }
            catch
            {
                return false;
            }
        }

        private Dictionary<int, string> GetKursiyerTcMap(List<int> ids)
        {
            Dictionary<int, string> map = new Dictionary<int, string>();
            if (ids == null || ids.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return map;

            string idList = string.Join(",", ids.Where(x => x > 0).Distinct());
            if (string.IsNullOrWhiteSpace(idList))
                return map;

            string[] sqlAdaylari =
            {
                "SELECT ID, ISNULL(TC_NO,'') AS TC FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(TC_NO,'') AS TC FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(TC_NO,'') AS TC FROM dbo.KURSİYER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(TC,'') AS TC FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(TC,'') AS TC FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(TC,'') AS TC FROM dbo.KURSİYER WHERE ID IN (" + idList + ")"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                int id = ToInt(r["ID"]);
                                string tc = NormalizeDigits(Convert.ToString(r["TC"]));
                                if (id > 0 && !string.IsNullOrWhiteSpace(tc) && !map.ContainsKey(id))
                                    map.Add(id, tc);
                            }
                        }
                    }
                    if (map.Count > 0) return map;
                }
                catch
                {
                    // sonraki SQL adayı
                }
            }

            return map;
        }

        private void FillMissingAdSoyadFromDb(List<DireksiyonSinavModel> liste)
        {
            if (liste == null || liste.Count == 0)
                return;

            List<int> ids = liste
                .Where(x => x != null && x.ID_KURSIYER > 0)
                .Select(x => x.ID_KURSIYER)
                .Distinct()
                .ToList();
            if (ids.Count == 0)
                return;

            Dictionary<int, string> adMap = GetKursiyerAdSoyadMap(ids);
            if (adMap.Count == 0)
                return;

            foreach (var item in liste.Where(x => x != null && x.ID_KURSIYER > 0))
            {
                string mevcut = GetPropValue(item, "RandevuAdiSoyadi", "AdiSoyadi", "AdSoyad", "KursiyerAdi");
                if (!string.IsNullOrWhiteSpace(mevcut))
                    continue;

                string adSoyad;
                if (!adMap.TryGetValue(item.ID_KURSIYER, out adSoyad))
                    continue;

                adSoyad = (adSoyad ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(adSoyad))
                    continue;

                SetStringProp(item, "RandevuAdiSoyadi", adSoyad);
                SetStringProp(item, "AdiSoyadi", adSoyad);
                SetStringProp(item, "AdSoyad", adSoyad);
                SetStringProp(item, "KursiyerAdi", adSoyad);
            }
        }

        private Dictionary<int, string> GetKursiyerAdSoyadMap(List<int> ids)
        {
            Dictionary<int, string> map = new Dictionary<int, string>();
            if (ids == null || ids.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return map;

            string idList = string.Join(",", ids.Where(x => x > 0).Distinct());
            if (string.IsNullOrWhiteSpace(idList))
                return map;

            string[] sqlAdaylari =
            {
                "SELECT ID, LTRIM(RTRIM(ISNULL(ADI,'') + ' ' + ISNULL(SOYADI,''))) AS ADSOYAD FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, LTRIM(RTRIM(ISNULL(ADI,'') + ' ' + ISNULL(SOYADI,''))) AS ADSOYAD FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")",
                "SELECT ID, LTRIM(RTRIM(ISNULL(ADI,'') + ' ' + ISNULL(SOYADI,''))) AS ADSOYAD FROM dbo.KURSİYER WHERE ID IN (" + idList + ")",
                "SELECT ID, LTRIM(RTRIM(ISNULL(AD,'') + ' ' + ISNULL(SOYAD,''))) AS ADSOYAD FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, LTRIM(RTRIM(ISNULL(AD,'') + ' ' + ISNULL(SOYAD,''))) AS ADSOYAD FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")",
                "SELECT ID, LTRIM(RTRIM(ISNULL(AD,'') + ' ' + ISNULL(SOYAD,''))) AS ADSOYAD FROM dbo.KURSİYER WHERE ID IN (" + idList + ")"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                int id = ToInt(r["ID"]);
                                string adSoyad = (Convert.ToString(r["ADSOYAD"]) ?? string.Empty).Trim();
                                if (id > 0 && !string.IsNullOrWhiteSpace(adSoyad) && !map.ContainsKey(id))
                                    map.Add(id, adSoyad);
                            }
                        }
                    }

                    if (map.Count > 0)
                        return map;
                }
                catch
                {
                    // sonraki SQL adayı
                }
            }

            return map;
        }

        private Dictionary<int, string> GetKursiyerDonemMap(List<int> ids)
        {
            var map = new Dictionary<int, string>();
            if (ids == null || ids.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return map;

            string idList = string.Join(",", ids.Where(x => x > 0).Distinct());
            if (string.IsNullOrWhiteSpace(idList))
                return map;

            string[] sqlAdaylari =
            {
                @"SELECT k.ID, ISNULL(g.DONEM_ADI,'') AS DONEM
                  FROM dbo.KURSIYER k
                  LEFT JOIN dbo.GRUP_KARTI g ON g.ID = k.ID_GRUP_KARTI
                  WHERE k.ID IN (" + idList + ")",
                @"SELECT k.ID, ISNULL(g.DONEM_ADI,'') AS DONEM
                  FROM dbo.KURSIYERLER k
                  LEFT JOIN dbo.GRUP_KARTI g ON g.ID = k.ID_GRUP_KARTI
                  WHERE k.ID IN (" + idList + ")",
                @"SELECT k.ID, ISNULL(g.DONEM_ADI,'') AS DONEM
                  FROM dbo.KURSİYER k
                  LEFT JOIN dbo.GRUP_KARTI g ON g.ID = k.ID_GRUP_KARTI
                  WHERE k.ID IN (" + idList + ")"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                int id = ToInt(r["ID"]);
                                string donem = (Convert.ToString(r["DONEM"]) ?? string.Empty).Trim();
                                if (id > 0 && !string.IsNullOrWhiteSpace(donem) && !map.ContainsKey(id))
                                    map.Add(id, donem);
                            }
                        }
                    }

                    if (map.Count > 0)
                        return map;
                }
                catch
                {
                    // sonraki SQL adayi
                }
            }

            return map;
        }

        private int GetKursiyerIdByTc(string tcNo)
        {
            string tc = NormalizeDigits(tcNo);
            if (string.IsNullOrWhiteSpace(_connectionString) || tc.Length != 11)
                return 0;

            string[] sqlAdaylari =
            {
                "SELECT TOP 1 ID FROM dbo.KURSIYER WHERE REPLACE(REPLACE(ISNULL(TC_NO,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSIYERLER WHERE REPLACE(REPLACE(ISNULL(TC_NO,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSİYER WHERE REPLACE(REPLACE(ISNULL(TC_NO,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSIYER WHERE REPLACE(REPLACE(ISNULL(TC,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSIYERLER WHERE REPLACE(REPLACE(ISNULL(TC,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSİYER WHERE REPLACE(REPLACE(ISNULL(TC,''),' ',''),'-','') = @TC"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TC", tc);
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        int id;
                        if (int.TryParse(Convert.ToString(result), out id) && id > 0)
                            return id;
                    }
                }
                catch
                {
                    // sonraki SQL adayı
                }
            }

            return 0;
        }

        private static string NormalizeDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return new string(value.Where(char.IsDigit).ToArray());
        }

        private bool TryGetSelectedSinavId(out int sinavId)
        {
            sinavId = 0;
            if (Combo_Sinavlar.SelectedValue == null)
                return false;

            if (int.TryParse(Convert.ToString(Combo_Sinavlar.SelectedValue), out sinavId) && sinavId > 0)
                return true;

            DataRowView row = Combo_Sinavlar.SelectedItem as DataRowView;
            if (row == null)
                return false;

            if (row.Row.Table.Columns.Contains("ID"))
                return int.TryParse(Convert.ToString(row.Row["ID"]), out sinavId) && sinavId > 0;

            return false;
        }

        private async Task RefreshGridAsync()
        {
            int sinavId;
            if (!TryGetSelectedSinavId(out sinavId))
            {
                if (Combo_Sinavlar.Items.Count > 0 && Combo_Sinavlar.SelectedIndex < 0)
                    Combo_Sinavlar.SelectedIndex = 0;

                if (!TryGetSelectedSinavId(out sinavId))
                    return;
            }

            yukleniyor = true;
            try
            {
                DateTime? seciliTarih = GetSelectedSinavDate();
                var liste = await LoadListeBySinavIdFallbackAsync(sinavId);
                liste = UygunsaTariheGoreFiltrele(liste, seciliTarih);

                // Ayni tarihte birden fazla SINAV_TARIHLERI.ID olabiliyor.
                // Secili ID bos donerse ayni tarihteki alternatif ID'leri de dene.
                if ((liste == null || liste.Count == 0) && seciliTarih.HasValue)
                {
                    foreach (int altId in GetAlternativeSinavIdsByDate(seciliTarih.Value, sinavId))
                    {
                        liste = await LoadListeBySinavIdFallbackAsync(altId);
                        liste = UygunsaTariheGoreFiltrele(liste, seciliTarih);
                        if (liste != null && liste.Count > 0)
                            break;
                    }
                }

                // Bazi veritabanlarinda combodaki ID ile liste ID'si birebir eslesmeyebiliyor.
                // Son fallback: secili tarihe gore dogrudan liste cek.
                if ((liste == null || liste.Count == 0) && seciliTarih.HasValue)
                    liste = await GetDireksiyonListeByDateDirectAsync(seciliTarih.Value.Date);

                FillMissingAdSoyadFromDb(liste);
                liste = NormalizeListeForGrid(liste);
                _currentListe = liste;

                Dgv_Listesi.AutoGenerateColumns = true;
                Dgv_Listesi.Columns.Clear();
                Dgv_Listesi.DataSource = liste;

                KolonlariDuzenle();
                ComboKolonlariEkle();
                RefreshRandevuSecimCombolari();
            }
            finally
            {
                yukleniyor = false;
            }
        }

        private async Task<List<DireksiyonSinavModel>> LoadListeBySinavIdFallbackAsync(int sinavId)
        {
            if (sinavId <= 0)
                return new List<DireksiyonSinavModel>();

            List<DireksiyonSinavModel> liste = null;
            try
            {
                liste = await _service.ListeAsync(sinavId);
            }
            catch
            {
                liste = null;
            }

            if (liste == null || liste.Count == 0)
                liste = await GetDireksiyonListeDirectAsync(sinavId);

            return liste ?? new List<DireksiyonSinavModel>();
        }

        private List<int> GetAlternativeSinavIdsByDate(DateTime tarih, int exceptId)
        {
            var ids = new List<int>();
            if (string.IsNullOrWhiteSpace(_connectionString))
                return ids;

            const string sql = @"
SELECT st.ID
FROM SINAV_TARIHLERI st
WHERE st.SINAV_TARIHI IS NOT NULL
  AND CAST(st.SINAV_TARIHI AS date) = @TARIH
  AND st.ID <> @EXCEPT_ID
  AND (
      UPPER(ISNULL(st.SINAV_TURU,'')) LIKE '%DIREK%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE N'%DİREK%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE '%DIR%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE N'%DİR%'
      OR EXISTS (SELECT 1 FROM SINAV_LISTE_DIREKSIYON d WHERE d.ID_SINAV_TARIHI = st.ID)
  )
ORDER BY st.ID DESC;";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TARIH", tarih.Date);
                    cmd.Parameters.AddWithValue("@EXCEPT_ID", exceptId);
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int id = ToInt(r["ID"]);
                            if (id > 0 && !ids.Contains(id))
                                ids.Add(id);
                        }
                    }
                }
            }
            catch
            {
                // alternatif id bulunamazsa mevcut akis devam eder
            }

            return ids;
        }

        private List<DireksiyonSinavModel> UygunsaTariheGoreFiltrele(List<DireksiyonSinavModel> liste, DateTime? seciliTarih)
        {
            if (liste == null || !seciliTarih.HasValue)
                return liste ?? new List<DireksiyonSinavModel>();

            bool anlamliTarihVar = liste.Any(x => x != null && x.SINAV_TARIHI > new DateTime(1901, 1, 1));
            if (!anlamliTarihVar)
                return liste;

            var filtered = liste
                .Where(x => x != null && x.SINAV_TARIHI.Date == seciliTarih.Value.Date)
                .ToList();

            return filtered.Count > 0 ? filtered : liste;
        }

        private Task<List<DireksiyonSinavModel>> GetDireksiyonListeDirectAsync(int sinavId)
        {
            return Task.Run(() =>
            {
                var list = new List<DireksiyonSinavModel>();
                if (sinavId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
                    return list;

                const string sql = @"
SELECT
    d.ID,
    d.ID_KURSIYER,
    ISNULL(k.TC_NO, '') AS TC_NO,
    LTRIM(RTRIM(ISNULL(k.ADI,'') + ' ' + ISNULL(k.SOYADI,''))) AS ADSOYAD,
    ISNULL(g.DONEM_ADI, '') AS DONEM,
    ISNULL(d.DIR_HAK, 0) AS DIR_HAK,
    ISNULL(d.DIR_DURUM, 'GIRMEDI') AS DIR_DURUM,
    ISNULL(d.ID_PERSONEL, 0) AS ID_PERSONEL,
    ISNULL(d.ID_ARAC, 0) AS ID_ARAC,
    ISNULL(d.RANDEVU_SAATI, '') AS RANDEVU_SAATI,
    st.SINAV_TARIHI
FROM SINAV_LISTE_DIREKSIYON d
LEFT JOIN KURSIYER k ON k.ID = d.ID_KURSIYER
LEFT JOIN GRUP_KARTI g ON g.ID = k.ID_GRUP_KARTI
LEFT JOIN SINAV_TARIHLERI st ON st.ID = d.ID_SINAV_TARIHI
WHERE d.ID_SINAV_TARIHI = @SINAV_ID
ORDER BY d.ID;";

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SINAV_ID", sinavId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var item = new DireksiyonSinavModel
                            {
                                ID = ToInt(r["ID"]),
                                ID_KURSIYER = ToInt(r["ID_KURSIYER"]),
                                DIR_HAK = ToInt(r["DIR_HAK"]),
                                DIR_DURUM = Convert.ToString(r["DIR_DURUM"]) ?? string.Empty,
                                SINAV_TARIHI = r["SINAV_TARIHI"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["SINAV_TARIHI"])
                            };

                            SetStringProp(item, "TC_NO", Convert.ToString(r["TC_NO"]) ?? string.Empty);
                            SetStringProp(item, "RandevuAdiSoyadi", Convert.ToString(r["ADSOYAD"]) ?? string.Empty);
                            SetStringProp(item, "Donem", Convert.ToString(r["DONEM"]) ?? string.Empty);
                            SetIntProp(item, "PersonelId", ToInt(r["ID_PERSONEL"]));
                            SetIntProp(item, "AracId", ToInt(r["ID_ARAC"]));
                            SetStringProp(item, "Saat", Convert.ToString(r["RANDEVU_SAATI"]) ?? string.Empty);

                            list.Add(item);
                        }
                    }
                }

                return list;
            });
        }

        private Task<List<DireksiyonSinavModel>> GetDireksiyonListeByDateDirectAsync(DateTime tarih)
        {
            return Task.Run(() =>
            {
                var list = new List<DireksiyonSinavModel>();
                if (string.IsNullOrWhiteSpace(_connectionString))
                    return list;

                const string sql = @"
SELECT
    d.ID,
    d.ID_KURSIYER,
    ISNULL(k.TC_NO, '') AS TC_NO,
    LTRIM(RTRIM(ISNULL(k.ADI,'') + ' ' + ISNULL(k.SOYADI,''))) AS ADSOYAD,
    ISNULL(g.DONEM_ADI, '') AS DONEM,
    ISNULL(d.DIR_HAK, 0) AS DIR_HAK,
    ISNULL(d.DIR_DURUM, 'GIRMEDI') AS DIR_DURUM,
    ISNULL(d.ID_PERSONEL, 0) AS ID_PERSONEL,
    ISNULL(d.ID_ARAC, 0) AS ID_ARAC,
    ISNULL(d.RANDEVU_SAATI, '') AS RANDEVU_SAATI,
    st.SINAV_TARIHI
FROM SINAV_LISTE_DIREKSIYON d
LEFT JOIN KURSIYER k ON k.ID = d.ID_KURSIYER
LEFT JOIN GRUP_KARTI g ON g.ID = k.ID_GRUP_KARTI
LEFT JOIN SINAV_TARIHLERI st ON st.ID = d.ID_SINAV_TARIHI
WHERE st.SINAV_TARIHI IS NOT NULL
  AND CAST(st.SINAV_TARIHI AS date) = @TARIH
ORDER BY d.ID;";

                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TARIH", tarih.Date);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var item = new DireksiyonSinavModel
                            {
                                ID = ToInt(r["ID"]),
                                ID_KURSIYER = ToInt(r["ID_KURSIYER"]),
                                DIR_HAK = ToInt(r["DIR_HAK"]),
                                DIR_DURUM = Convert.ToString(r["DIR_DURUM"]) ?? string.Empty,
                                SINAV_TARIHI = r["SINAV_TARIHI"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(r["SINAV_TARIHI"])
                            };

                            SetStringProp(item, "TC_NO", Convert.ToString(r["TC_NO"]) ?? string.Empty);
                            SetStringProp(item, "RandevuAdiSoyadi", Convert.ToString(r["ADSOYAD"]) ?? string.Empty);
                            SetStringProp(item, "Donem", Convert.ToString(r["DONEM"]) ?? string.Empty);
                            SetIntProp(item, "PersonelId", ToInt(r["ID_PERSONEL"]));
                            SetIntProp(item, "AracId", ToInt(r["ID_ARAC"]));
                            SetStringProp(item, "Saat", Convert.ToString(r["RANDEVU_SAATI"]) ?? string.Empty);

                            list.Add(item);
                        }
                    }
                }

                return list;
            });
        }

        private static void SetStringProp(object obj, string propName, string value)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propName))
                return;
            var p = obj.GetType().GetProperty(propName);
            if (p == null || !p.CanWrite || p.PropertyType != typeof(string))
                return;
            p.SetValue(obj, value ?? string.Empty);
        }

        private static void SetIntProp(object obj, string propName, int value)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propName))
                return;
            var p = obj.GetType().GetProperty(propName);
            if (p == null || !p.CanWrite)
                return;
            if (p.PropertyType == typeof(int))
                p.SetValue(obj, value);
            else if (p.PropertyType == typeof(int?))
                p.SetValue(obj, (int?)value);
        }

        private DateTime? GetSelectedSinavDate()
        {
            DateTime dt;

            if (Combo_Sinavlar.SelectedItem != null)
            {
                DataRowView row = Combo_Sinavlar.SelectedItem as DataRowView;
                if (row != null && row.Row.Table.Columns.Contains("SINAV_TARIHI"))
                {
                    object raw = row.Row["SINAV_TARIHI"];
                    if (raw is DateTime)
                        return ((DateTime)raw).Date;

                    if (DateTime.TryParse(Convert.ToString(raw), out dt))
                        return dt.Date;
                }
            }

            // Format/locale farklarinda da secili metinden tarihi yakala.
            string text = (Combo_Sinavlar.Text ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                Match m = Regex.Match(text, @"\b\d{1,2}\.\d{1,2}\.\d{4}\b");
                if (m.Success && DateTime.TryParse(m.Value, out dt))
                    return dt.Date;
                if (DateTime.TryParse(text, out dt))
                    return dt.Date;
            }

            return null;
        }

        #region Sinav Olustur
        private void HazirlaSinavOlusturTab()
        {
            Cmb_SinavDurumu.Items.Clear();
            Cmb_SinavDurumu.Items.Add("Hazır");
            Cmb_SinavDurumu.Items.Add("Hazır Değil");
            Cmb_SinavDurumu.DropDownStyle = ComboBoxStyle.DropDownList;
            Cmb_SinavDurumu.SelectedIndex = 0;

            Dvg_Sinavlar.AllowUserToAddRows = false;
            Dvg_Sinavlar.AllowUserToDeleteRows = false;
            Dvg_Sinavlar.ReadOnly = true;
            Dvg_Sinavlar.MultiSelect = false;
            Dvg_Sinavlar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dvg_Sinavlar.RowHeadersVisible = false;
            Dvg_Sinavlar.AutoGenerateColumns = true;

            SinavTarihleriniYukle();
            SetSinavFormMode(true);
        }

        private void SinavTarihleriniYukle()
        {
            DataTable dt = GetSinavTarihleriYonetimData();
            if (dt == null || dt.Rows.Count == 0)
                dt = BuildSinavYonetimFromComboSource();
            Dvg_Sinavlar.DataSource = dt;

            if (Dvg_Sinavlar.Columns.Contains("ID"))
                Dvg_Sinavlar.Columns["ID"].Visible = false;
            if (Dvg_Sinavlar.Columns.Contains("SINAV_TARIHI"))
                Dvg_Sinavlar.Columns["SINAV_TARIHI"].HeaderText = "Sınav Tarihi";
            if (Dvg_Sinavlar.Columns.Contains("DURUM_TEXT"))
                Dvg_Sinavlar.Columns["DURUM_TEXT"].HeaderText = "Durum";
            if (Dvg_Sinavlar.Columns.Contains("ACIKLAMA"))
                Dvg_Sinavlar.Columns["ACIKLAMA"].HeaderText = "Açıklama";
        }

        private DataTable GetSinavTarihleriYonetimData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("SINAV_TARIHI", typeof(DateTime));
            dt.Columns.Add("DURUM_TEXT", typeof(string));
            dt.Columns.Add("ACIKLAMA", typeof(string));

            string[] sqlAdaylari =
            {
                @"SELECT ID, SINAV_TARIHI,
                         CASE
                           WHEN TRY_CONVERT(int, CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) = 1 THEN 'Hazır'
                           WHEN UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) IN (N'HAZIR', N'1', N'TRUE') THEN 'Hazır'
                           WHEN UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) LIKE N'%HAZIR%' 
                                AND UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) NOT LIKE N'%DEĞİL%' 
                                AND UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) NOT LIKE N'%DEGIL%' THEN 'Hazır'
                           ELSE 'Hazır Değil'
                         END AS DURUM_TEXT,
                         ISNULL(SINAV_ACIKLAMA,'') AS ACIKLAMA
                  FROM SINAV_TARIHLERI
                  WHERE ISNULL(SINAV_TURU,'') LIKE '%DIREK%'
                  ORDER BY SINAV_TARIHI DESC, ID DESC",
                @"SELECT ID, SINAV_TARIHI,
                         CASE
                           WHEN TRY_CONVERT(int, CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) = 1 THEN 'Hazır'
                           WHEN UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) IN (N'HAZIR', N'1', N'TRUE') THEN 'Hazır'
                           WHEN UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) LIKE N'%HAZIR%' 
                                AND UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) NOT LIKE N'%DEĞİL%' 
                                AND UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) NOT LIKE N'%DEGIL%' THEN 'Hazır'
                           ELSE 'Hazır Değil'
                         END AS DURUM_TEXT,
                         ISNULL(SINAV_ACIKLAMA,'') AS ACIKLAMA
                  FROM SINAV_TARIHLERI
                  WHERE ISNULL(SINAV_TURU,'') LIKE '%DİREK%'
                     OR ISNULL(SINAV_TURU,'') LIKE '%DIREKSIYON%'
                     OR ISNULL(SINAV_TURU,'') LIKE '%DİREKSİYON%'
                     OR ISNULL(SINAV_TURU,'') LIKE '%DIR%'
                     OR ISNULL(SINAV_TURU,'') LIKE '%DIR%'
                  ORDER BY SINAV_TARIHI DESC, ID DESC",
                @"SELECT ID, SINAV_TARIHI,
                         CASE
                           WHEN TRY_CONVERT(int, DURUM) = 1 THEN 'Hazır'
                           WHEN UPPER(CONVERT(varchar(50), ISNULL(DURUM,''))) LIKE N'%HAZIR%' AND UPPER(CONVERT(varchar(50), ISNULL(DURUM,''))) NOT LIKE N'%DEĞİL%' AND UPPER(CONVERT(varchar(50), ISNULL(DURUM,''))) NOT LIKE N'%DEGIL%' THEN 'Hazır'
                           ELSE 'Hazır Değil'
                         END AS DURUM_TEXT,
                         ISNULL(SINAV_ACIKLAMA,'') AS ACIKLAMA
                  FROM SINAV_TARIHLERI
                  WHERE ISNULL(SINAV_TURU,'') LIKE '%DIREK%'
                  ORDER BY SINAV_TARIHI DESC, ID DESC",
                @"SELECT ID, SINAV_TARIHI,
                         CASE
                           WHEN TRY_CONVERT(int, SINAV_DURUMU) = 1 THEN 'Hazır'
                           WHEN UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) LIKE N'%HAZIR%' AND UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) NOT LIKE N'%DEĞİL%' AND UPPER(CONVERT(varchar(50), ISNULL(SINAV_DURUMU,''))) NOT LIKE N'%DEGIL%' THEN 'Hazır'
                           ELSE 'Hazır Değil'
                         END AS DURUM_TEXT,
                         ISNULL(ACIKLAMA,'') AS ACIKLAMA
                  FROM SINAV_TARIHLERI
                  ORDER BY SINAV_TARIHI DESC, ID DESC",
                @"SELECT ID, SINAV_TARIHI,
                         CASE
                           WHEN TRY_CONVERT(int, DURUM) = 1 THEN 'Hazır'
                           WHEN UPPER(CONVERT(varchar(50), ISNULL(DURUM,''))) LIKE N'%HAZIR%' AND UPPER(CONVERT(varchar(50), ISNULL(DURUM,''))) NOT LIKE N'%DEĞİL%' AND UPPER(CONVERT(varchar(50), ISNULL(DURUM,''))) NOT LIKE N'%DEGIL%' THEN 'Hazır'
                           ELSE 'Hazır Değil'
                         END AS DURUM_TEXT,
                         ISNULL(SINAV_ACIKLAMA,'') AS ACIKLAMA
                  FROM SINAV_TARIHLERI
                  ORDER BY SINAV_TARIHI DESC, ID DESC",
                @"SELECT ID, SINAV_TARIHI,
                         CASE
                           WHEN TRY_CONVERT(int, SINAV_DURUMU) = 1 THEN 'Hazır'
                           ELSE 'Hazır Değil'
                         END AS DURUM_TEXT,
                         ISNULL(ACIKLAMA,'') AS ACIKLAMA
                  FROM SINAV_TARIHLERI
                  ORDER BY SINAV_TARIHI DESC, ID DESC"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        DataTable tmp = new DataTable();
                        da.Fill(tmp);
                        if (tmp.Rows.Count > 0)
                            return tmp;
                    }
                }
                catch
                {
                    // Sonraki SQL adayı denenir.
                }
            }

            return dt;
        }

        private DataTable BuildSinavYonetimFromComboSource()
        {
            DataTable result = new DataTable();
            result.Columns.Add("ID", typeof(int));
            result.Columns.Add("SINAV_TARIHI", typeof(DateTime));
            result.Columns.Add("DURUM_TEXT", typeof(string));
            result.Columns.Add("ACIKLAMA", typeof(string));

            DataTable combo = GetSinavTarihleri();
            if (combo == null || combo.Rows.Count == 0)
                return result;

            foreach (DataRow row in combo.Rows)
            {
                int id;
                if (!int.TryParse(Convert.ToString(row["ID"]), out id))
                    id = 0;

                DateTime tarih;
                string raw = Convert.ToString(row.Table.Columns.Contains("SINAV_TARIHI")
                    ? row["SINAV_TARIHI"]
                    : (row.Table.Columns.Contains("SINAV_TARIHI_TEXT") ? row["SINAV_TARIHI_TEXT"] : null));
                if (!DateTime.TryParse(raw, out tarih))
                    continue;

                DataRow nr = result.NewRow();
                nr["ID"] = id;
                nr["SINAV_TARIHI"] = tarih.Date;
                nr["DURUM_TEXT"] = "Hazır";
                nr["ACIKLAMA"] = string.Empty;
                result.Rows.Add(nr);
            }

            DataView view = result.DefaultView;
            view.Sort = "SINAV_TARIHI DESC, ID DESC";
            return view.ToTable();
        }

        private void Btn_Yeni_Click(object sender, EventArgs e)
        {
            Dvg_Sinavlar.ClearSelection();
            Dvg_Sinavlar.CurrentCell = null;
            _seciliSinavTarihiId = 0;
            DateTime secilenTarih = DateTime.Today;
            DateTime? secilen = PromptForDateSelection(DateTime.Today);
            if (secilen.HasValue)
                secilenTarih = secilen.Value.Date;

            Dtp_SinavTarihi.Value = secilenTarih;
            Cmb_SinavDurumu.SelectedIndex = 0;
            Txt_Aciklama.Text = string.Empty;
            SetSinavFormMode(true);
        }

        private void Dvg_Sinavlar_SelectionChanged(object sender, EventArgs e)
        {
            if (Dvg_Sinavlar.CurrentRow == null)
            {
                _seciliSinavTarihiId = 0;
                return;
            }
            DataGridViewRow row = Dvg_Sinavlar.CurrentRow;
            if (row.Cells["SINAV_TARIHI"] == null) return;
            _seciliSinavTarihiId = ToInt(row.Cells["ID"] == null ? null : row.Cells["ID"].Value);

            DateTime tarih;
            if (DateTime.TryParse(Convert.ToString(row.Cells["SINAV_TARIHI"].Value), out tarih))
                Dtp_SinavTarihi.Value = tarih;

            string durum = Convert.ToString(row.Cells["DURUM_TEXT"] == null ? null : row.Cells["DURUM_TEXT"].Value);
            Cmb_SinavDurumu.SelectedIndex = (durum ?? string.Empty).IndexOf("Hazır", StringComparison.OrdinalIgnoreCase) >= 0
                && (durum ?? string.Empty).IndexOf("Değil", StringComparison.OrdinalIgnoreCase) < 0
                ? 0
                : 1;

            Txt_Aciklama.Text = Convert.ToString(row.Cells["ACIKLAMA"] == null ? null : row.Cells["ACIKLAMA"].Value) ?? string.Empty;
            SetSinavFormMode(_seciliSinavTarihiId <= 0);
        }

        private void Btn_SinavTarihiKaydet_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            int id = GetSeciliSinavTarihiId();
            DateTime tarih = Dtp_SinavTarihi.Value.Date;
            if (tarih.Year < 2000)
            {
                MessageBox.Show("Lütfen geçerli bir sınav tarihi seçiniz.");
                return;
            }
            int durum = Cmb_SinavDurumu.SelectedIndex == 0 ? 1 : 0;
            string aciklama = (Txt_Aciklama.Text ?? string.Empty).Trim();
            string durumText = durum == 1 ? "Hazır" : "Hazır Değil";

            bool ok;
            int mevcutId = 0;
            if (id <= 0)
                mevcutId = FindDireksiyonSinavTarihiIdByDate(tarih);

            if (id > 0)
            {
                ok = UpdateSinavTarihi(id, tarih, durum, aciklama);
            }
            else if (mevcutId > 0)
            {
                ok = UpdateSinavTarihi(mevcutId, tarih, durum, aciklama);
                if (ok)
                    MessageBox.Show("Bu tarih zaten vardı, mevcut kayıt güncellendi.");
            }
            else
            {
                ok = InsertSinavTarihi(tarih, durum, aciklama);
            }
            if (!ok)
            {
                MessageBox.Show("Sınav tarihi kaydedilemedi.");
                return;
            }

            // Grid satırını anında güncelle (UI gecikmesini azaltır).
            if (Dvg_Sinavlar.CurrentRow != null && id > 0)
            {
                if (Dvg_Sinavlar.CurrentRow.Cells["SINAV_TARIHI"] != null)
                    Dvg_Sinavlar.CurrentRow.Cells["SINAV_TARIHI"].Value = tarih;
                if (Dvg_Sinavlar.CurrentRow.Cells["DURUM_TEXT"] != null)
                    Dvg_Sinavlar.CurrentRow.Cells["DURUM_TEXT"].Value = durumText;
                if (Dvg_Sinavlar.CurrentRow.Cells["ACIKLAMA"] != null)
                    Dvg_Sinavlar.CurrentRow.Cells["ACIKLAMA"].Value = aciklama;
            }

            SinavTarihleriniYukle();
            int tercihId = id > 0 ? id : mevcutId;
            ReloadComboSinavlar(tercihId > 0 ? (int?)tercihId : null, tarih);
            _seciliSinavTarihiId = 0;
            MessageBox.Show("Sınav tarihi güncellendi.");
        }

        private int FindDireksiyonSinavTarihiIdByDate(DateTime tarih)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return 0;

            const string sql = @"
SELECT TOP 1 st.ID
FROM SINAV_TARIHLERI st
WHERE st.SINAV_TARIHI IS NOT NULL
  AND CAST(st.SINAV_TARIHI AS date) = @TARIH
  AND (
      UPPER(ISNULL(st.SINAV_TURU,'')) LIKE '%DIREK%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE N'%DİREK%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE '%DIR%'
      OR UPPER(ISNULL(st.SINAV_TURU,'')) LIKE N'%DİR%'
  )
ORDER BY st.ID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TARIH", tarih.Date);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    int id;
                    return int.TryParse(Convert.ToString(result), out id) ? id : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private void Btn_TarihSil_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            int id = GetSeciliSinavTarihiId();
            if (id <= 0) return;

            string seciliTarih = Dtp_SinavTarihi.Value.ToString("dd.MM.yyyy");
            DialogResult onay = MessageBox.Show(
                seciliTarih + " tarihini silmek istediğinizden emin misiniz?",
                "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (onay != DialogResult.Yes) return;

            if (!DeleteSinavTarihi(id))
            {
                MessageBox.Show("Silme işlemi başarısız.");
                return;
            }

            SinavTarihleriniYukle();
            ReloadComboSinavlar();
        }

        private void SetSinavFormMode(bool yeniKayit)
        {
            button1.Text = yeniKayit ? "KAYDET (YENİ)" : "KAYDET (GÜNCELLE)";
        }

        private DateTime? PromptForDateSelection(DateTime defaultDate)
        {
            using (Form prompt = new Form())
            {
                prompt.Text = "Yeni Direksiyon Sınav Tarihi";
                prompt.Width = 360;
                prompt.Height = 170;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;

                Label lbl = new Label { Left = 12, Top = 12, Width = 320, Text = "Yeni sınav tarihini seçiniz:" };
                DateTimePicker dtp = new DateTimePicker
                {
                    Left = 12,
                    Top = 38,
                    Width = 320,
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "dd.MM.yyyy",
                    Value = defaultDate
                };
                Button ok = new Button { Text = "Tamam", Left = 176, Width = 75, Top = 74, DialogResult = DialogResult.OK };
                Button cancel = new Button { Text = "İptal", Left = 257, Width = 75, Top = 74, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(lbl);
                prompt.Controls.Add(dtp);
                prompt.Controls.Add(ok);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = ok;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog(this) == DialogResult.OK ? (DateTime?)dtp.Value.Date : null;
            }
        }

        private void ReloadComboSinavlar(int? preferredId = null, DateTime? preferredDate = null)
        {
            object selected = Combo_Sinavlar.SelectedValue;
            Combo_Sinavlar.SelectedIndexChanged -= Combo_Sinavlar_SelectedIndexChanged;
            Combo_Sinavlar.DataSource = null;
            DataTable src = GetSinavTarihleri();
            Combo_Sinavlar.DataSource = src.Copy();
            Combo_Sinavlar.DisplayMember = "SINAV_TARIHI";
            Combo_Sinavlar.ValueMember = "ID";
            Combo_Sinavlar.SelectedIndexChanged += Combo_Sinavlar_SelectedIndexChanged;

            if (preferredId.HasValue && preferredId.Value > 0)
                Combo_Sinavlar.SelectedValue = preferredId.Value;

            if (preferredDate.HasValue && Combo_Sinavlar.Items.Count > 0)
            {
                string key = preferredDate.Value.ToString("dd.MM.yyyy");
                for (int i = 0; i < Combo_Sinavlar.Items.Count; i++)
                {
                    DataRowView rv = Combo_Sinavlar.Items[i] as DataRowView;
                    if (rv == null) continue;
                    string val = Convert.ToString(rv["SINAV_TARIHI"]);
                    if (!string.IsNullOrWhiteSpace(val) && val.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                    {
                        Combo_Sinavlar.SelectedIndex = i;
                        break;
                    }
                }
            }

            if (selected != null)
            {
                int id;
                if (int.TryParse(Convert.ToString(selected), out id) && id > 0)
                    Combo_Sinavlar.SelectedValue = id;
            }
            if (Combo_Sinavlar.SelectedIndex < 0 && Combo_Sinavlar.Items.Count > 0)
                Combo_Sinavlar.SelectedIndex = 0;
            Combo_Sinavlar.Refresh();
        }

        private void Combo_Sinavlar_Format(object sender, ListControlConvertEventArgs e)
        {
            DataRowView row = e.ListItem as DataRowView;
            if (row == null) return;

            string text = Convert.ToString(row.Row["SINAV_TARIHI"]);
            DateTime dt;
            if (DateTime.TryParse(text, out dt))
                e.Value = dt.ToString("dd.MM.yyyy");
            else
                e.Value = text;
        }

        private void Combo_Sinavlar_DropDown(object sender, EventArgs e)
        {
            try
            {
                int id = GetSeciliSinavTarihiId();
                ReloadComboSinavlar(id > 0 ? (int?)id : null, null);
            }
            catch
            {
            }
        }

        private int GetSeciliSinavTarihiId()
        {
            return _seciliSinavTarihiId;
        }

        private bool InsertSinavTarihi(DateTime tarih, int durum, string aciklama)
        {
            string durumText = durum == 1 ? "HAZIR" : "HAZIR DEĞİL";
            string[] sqlAdaylari =
            {
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU, SINAV_DURUMU, SINAV_ACIKLAMA) VALUES (@TARIH, N'DİREKSİYON', @DURUM_TEXT, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU, SINAV_DURUMU, ACIKLAMA) VALUES (@TARIH, N'DİREKSİYON', @DURUM_TEXT, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU, SINAV_DURUMU, ACIKLAMA) VALUES (@TARIH, 'DIREKSIYON', @DURUM, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU, SINAV_DURUMU, ACIKLAMA) VALUES (@TARIH, N'DİREKSİYON', @DURUM_TEXT, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU, DURUM, ACIKLAMA) VALUES (@TARIH, 'DIREKSIYON', @DURUM, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU, SINAV_DURUMU, SINAV_ACIKLAMA) VALUES (@TARIH, 'DIREKSIYON', @DURUM, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU, ACIKLAMA) VALUES (@TARIH, 'DIREKSIYON', @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU) VALUES (@TARIH, 'DIREKSIYON')"
            };
            return ExecuteSinavTarihiYazma(sqlAdaylari, tarih, durum, aciklama, null, durumText);
        }

        private bool UpdateSinavTarihi(int id, DateTime tarih, int durum, string aciklama)
        {
            string durumText = durum == 1 ? "HAZIR" : "HAZIR DEĞİL";
            string[] sqlAdaylari =
            {
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM_TEXT, DURUM=@DURUM_TEXT, SINAV_ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM_TEXT, DURUM=@DURUM_TEXT, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM_TEXT, SINAV_ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM_TEXT, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM_TEXT, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, DURUM=@DURUM_TEXT, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, DURUM=@DURUM, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM, SINAV_ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH WHERE ID=@ID"
            };
            return ExecuteSinavTarihiYazma(sqlAdaylari, tarih, durum, aciklama, id, durumText);
        }

        private bool ExecuteSinavTarihiYazma(string[] sqlAdaylari, DateTime tarih, int durum, string aciklama, int? id = null, string durumText = null)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TARIH", tarih);
                        cmd.Parameters.AddWithValue("@DURUM", durum);
                        cmd.Parameters.AddWithValue("@DURUM_TEXT", string.IsNullOrWhiteSpace(durumText) ? (object)DBNull.Value : durumText);
                        cmd.Parameters.AddWithValue("@ACIKLAMA", string.IsNullOrWhiteSpace(aciklama) ? (object)DBNull.Value : aciklama);
                        if (id.HasValue)
                            cmd.Parameters.AddWithValue("@ID", id.Value);

                        conn.Open();
                        int affected = cmd.ExecuteNonQuery();
                        if ((id.HasValue && affected > 0) || (!id.HasValue && affected >= 0))
                            LisansPolitikasi.RegisterSuccessfulWrite();
                        if (!id.HasValue)
                            return affected >= 0;
                        return affected > 0;
                    }
                }
                catch
                {
                    // Sonraki SQL adayı denenir.
                }
            }

            return false;
        }

        private bool DeleteSinavTarihi(int id)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            const string sql = "DELETE FROM SINAV_TARIHLERI WHERE ID=@ID";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    var ok = cmd.ExecuteNonQuery() > 0;
                    if (ok) LisansPolitikasi.RegisterSuccessfulWrite();
                    return ok;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}