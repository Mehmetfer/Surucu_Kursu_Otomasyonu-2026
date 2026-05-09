using Kolera.Arama;
using Kolera.Arama.Services;
using Kolera.Donem;
using Kolera.Evrak.Services;
using Kolera.Mebbis.Models;
using Kolera.Mebbis.Services;
using Kolera.SINAVLAR.Services;
using Kolera_Kursiyer;
using Kolera_Kursiyer.Services;
using Mebbis_Aktar;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using FastReport.Export.PdfSimple;
using static Kolera_Mtsk.Sayfalar.Tarama_Sayfam;
using Kolera_Mtsk.Services;



 





namespace Kolera_Mtsk.Sayfalar
{
    public partial class KursiyerDetay_Sayfam : Form
    {
        private const string KimlikDogrulamaServisUrl = "https://tckimlik.nvi.gov.tr/Service/KPSPublic.asmx";
        private const string KimlikDogrulamaServisUrlV2 = "https://tckimlik.nvi.gov.tr/Service/KPSPublicV2.asmx";
        private readonly AramaService _aramaService;
        private readonly DonemService _donemService;
        private readonly ESinavService _eSinavService;
        private readonly DireksiyonSinavService _direksiyonService;
        private readonly MebbisService _mebbisService;
        private readonly KursiyerEvrakService _evrakService;
        private readonly KursiyerService _kursiyerService;
        private readonly int _kursiyerId;
        private readonly string _cs;
        private byte[] _webcamImageBytes;
        private Arama_Model _model;
        private Button _btnRaporAl;
        private MebbisWebForm _mebbisForm;
        private Button _btnOdemeKart;
        public KursiyerDetay_Sayfam()
        {
            InitializeComponent();
        }

        public KursiyerDetay_Sayfam(string cs, int kursiyerId)
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _cs = cs;
            _kursiyerId = kursiyerId;
            _mebbisService = new MebbisService(cs);
            _evrakService = new KursiyerEvrakService(cs);
            _aramaService = new AramaService(cs);
            _kursiyerService = new KursiyerService(cs);
            _donemService = new DonemService(cs);
            _direksiyonService = new DireksiyonSinavService(cs); // burada başlatıldı
            _eSinavService = new ESinavService(cs);             // burada başlatıldı

            Load += KursiyerDetay_Sayfam_Load;
            Activated += KursiyerDetay_Sayfam_Activated;
            
            Btn_Kaydet.Click += Btn_Kaydet_Click;
            Btn_Sil.Click += Btn_Sil_Click;
            Btn_EkleResim.Click += Btn_EkleResim_Click;
            Btn_Webresimekle.Click += Btn_Webresimekle_Click;
            Btn_Tarama.Click += Btn_Tarama_Click;
            Btn_Mebbis_Aktar.Click += Btn_Mebbis_Aktar_Click;
            Dgv_Direksiyon_Liste.RowPrePaint += Dgv_Direksiyon_Liste_RowPrePaint;
            Btn_SMS_Gonder.Click += Btn_SMS_Gonder_Click;
            Btn_TC_Dogrula.Click += Btn_TC_Dogrula_Click;
            Tnk_TC_NO.MaxLength = 11;
            Tnk_TC_NO.Validating += Tnk_TC_NO_Validating;
            Tnk_cinsiyet.SelectedIndexChanged += Tnk_cinsiyet_SelectedIndexChanged;
            KeyPreview = true;
            KeyDown += KursiyerDetay_Sayfam_KeyDown;
            ApplyUppercaseInputs(this);
            OdemeKartButonuHazirla();

            RaporAlButonuOlustur();

        }
        private void RaporAlButonuOlustur()
        {
            if (Btn_Rapor_Al != null)
            {
                Btn_Rapor_Al.Click -= Btn_RaporAl_Click;
                Btn_Rapor_Al.Click += Btn_RaporAl_Click;
                return;
            }

            _btnRaporAl = new Button
            {
                Name = "Btn_RaporAl",
                Text = "Rapor Al",
                Size = new Size(76, 75),
                Location = new Point(629, 548),
                TextAlign = ContentAlignment.BottomCenter,
                ImageAlign = ContentAlignment.TopCenter,
                UseVisualStyleBackColor = true
            };
            _btnRaporAl.Click += Btn_RaporAl_Click;
            Controls.Add(_btnRaporAl);
            _btnRaporAl.BringToFront();
        }

        private void OdemeKartButonuHazirla()
        {
            var mevcut = Controls.Find("Btn_Odeme_Kart", true).FirstOrDefault() as Button;
            if (mevcut != null)
            {
                mevcut.Click -= Btn_Odeme_Kart_Click;
                mevcut.Click += Btn_Odeme_Kart_Click;
                return;
            }

            _btnOdemeKart = new Button
            {
                Name = "Btn_Odeme_Kart",
                Text = "Odeme Kart",
                Size = new Size(86, 75),
                Location = new Point(711, 548),
                TextAlign = ContentAlignment.BottomCenter,
                ImageAlign = ContentAlignment.TopCenter,
                UseVisualStyleBackColor = true
            };
            _btnOdemeKart.Click += Btn_Odeme_Kart_Click;
            Controls.Add(_btnOdemeKart);
            _btnOdemeKart.BringToFront();
        }

        private async void Btn_Odeme_Kart_Click(object sender, EventArgs e)
        {
            int kursiyerId = _model != null && _model.ID > 0 ? _model.ID : _kursiyerId;
            if (kursiyerId <= 0)
            {
                MessageBox.Show("Once kursiyer seciniz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string adSoyad = _model != null
                ? ((_model.ADI ?? "") + " " + (_model.SOYADI ?? "")).Trim()
                : ((Tnk_ADI.Text ?? string.Empty) + " " + (Tnk_SOYADI.Text ?? string.Empty)).Trim();
            string tcNo = _model != null
                ? (_model.TC_NO ?? string.Empty)
                : (Tnk_TC_NO.Text ?? string.Empty);

            using (var frm = new Kursiyer_Odeme_Karti(_cs, kursiyerId, adSoyad, tcNo))
            {
                frm.ShowDialog(this);
            }

            await LoadOdemeBilgiGridAsync();
        }

        private void Btn_RaporAl_Click(object sender, EventArgs e)
        {
            if (_model == null || _model.ID <= 0)
            {
                MessageBox.Show("Once kursiyer kaydini seciniz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string seciliAdSoyad = ((_model.ADI ?? "") + " " + (_model.SOYADI ?? "")).Trim();
            using (var secimFormu = new RaporDetay(_cs, "KURSIYER", _model.ID, "KURSIYER", seciliAdSoyad))
            {
                secimFormu.ShowDialog(this);
            }
        }
        private async void KursiyerDetay_Sayfam_Load(object sender, EventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            HazirlaESinavGrid();
            HazirlaDireksiyonGrid();

            await ComboboxYukle();

            if (_kursiyerId > 0)
                await KursiyerYukle();
            else
                _model = new Arama_Model();

            await ESinavlariYukle();
            await DireksiyonSinavlariYukle();
            await LoadOdemeBilgiGridAsync();

             
            Dgv_esinav_Liste.CellFormatting += Dgv_esinav_Liste_CellFormatting;
            if (_model != null && _model.RESIM != null && _model.RESIM.Length > 0)
                ResmiGoster(_model.RESIM);

            await LoadCinsiyetAsync();
            CinsiyetArkaPlaniniUygula();
            await LoadWebcamImageAsync();
        }

        private async void KursiyerDetay_Sayfam_Activated(object sender, EventArgs e)
        {
            await LoadOdemeBilgiGridAsync();
        }

        // ------------------ COMBOBOX ------------------
        private async Task ComboboxYukle()
        {
            Cmb_SINIFI.Items.AddRange(new object[] { "A", "A1", "A2", "B", "B1", "C", "D", "E", "F", "G" });
            Cmb_ONCEKI_SINIFI.Items.AddRange(new object[] { "", "A", "A1", "A2", "B", "B1", "C", "D", "E", "F", "G" });

            Cmb_DURUM.DisplayMember = "Text";
            Cmb_DURUM.ValueMember = "Value";
            Cmb_DURUM.Items.Add(new { Text = "Aktif", Value = 1 });
            Cmb_DURUM.Items.Add(new { Text = "Pasif", Value = 0 });
            Cmb_DURUM.SelectedIndex = 0;

            var donemler = await _donemService.GetDonemlerAsync();
            Cmb_DONEM.DisplayMember = "DonemAdi";
            Cmb_DONEM.ValueMember = "ID";
            Cmb_DONEM.DataSource = donemler;
        }

        // ------------------ KURSIYER YÜKLE ------------------
        private async Task KursiyerYukle()
        {
            try
            {
                _model = await _aramaService.GetKursiyerByIdAsync(_kursiyerId);
            }
            catch
            {
                _model = null;
            }

            if (_model == null)
                _model = await GetKursiyerByIdDirectAsync(_kursiyerId);

            if (_model == null) return;

            Tnk_ADI.Text = _model.ADI ?? "";
            Tnk_SOYADI.Text = _model.SOYADI ?? "";
            Tnk_TC_NO.Text = _model.TC_NO ?? "";
            Tnk_KIM_KAYIT_NO.Text = _model.KIM_KAYIT_NO ?? "";
            Tnk_GSM_1.Text = _model.GSM_1 ?? "";
            Tnk_GSM_2.Text = _model.GSM_2 ?? "";
            Tnk_EV_TELEFON.Text = _model.EV_TELEFON ?? "";
            Tnk_KIM_BABA_ADI.Text = _model.KIM_BABA_ADI ?? "";
            Tnk_KIM_ANA_ADI.Text = _model.KIM_ANA_ADI ?? "";
            Tnk_KIM_DOGUM_YERI.Text = _model.KIM_DOGUM_YERI ?? "";
            Tnk_EV_ADRESI.Text = _model.EV_ADRESI ?? "";
            Tnk_ADAY_NO.Text = _model.ADAY_NO.ToString();
            Tnk_ONCEKI_BELGE.Text = _model.ONCE_SERT_BELGESAYI ?? "";
            Tnk_Referans.Text = _model.SARI_NOTLAR ?? "";

            if (_model.DOGUM_TARIHI.HasValue)
                Dtp_DOGUM_TARIHI.Value = _model.DOGUM_TARIHI.Value;

            if (_model.KAYIT_TARIHI.HasValue)
                Dtp_KAYIT_TARIHI.Value = _model.KAYIT_TARIHI.Value;

            Cmb_SINIFI.SelectedItem = _model.SERTIFIKA_SINIFI;
            Cmb_ONCEKI_SINIFI.SelectedItem = _model.ONCE_SERT_SINIFI;
            Cmb_DURUM.SelectedValue = _model.KURSIYER_DURUMU;

            if (_model.ID_GRUP_KARTI > 0)
                Cmb_DONEM.SelectedValue = _model.ID_GRUP_KARTI;

            if (_model.RESIM != null && _model.RESIM.Length > 0)
                ResmiGoster(_model.RESIM);

            await LoadCinsiyetAsync();
        }

        private async Task<Arama_Model> GetKursiyerByIdDirectAsync(int kursiyerId)
        {
            const string sql = @"
SELECT TOP 1
    k.ID,
    ISNULL(k.ADI, '') AS ADI,
    ISNULL(k.SOYADI, '') AS SOYADI,
    ISNULL(k.TC_NO, '') AS TC_NO,
    ISNULL(k.KIMLIK_KAYIT_NO, '') AS KIM_KAYIT_NO,
    ISNULL(k.GSM_1, '') AS GSM_1,
    ISNULL(k.GSM_2, '') AS GSM_2,
    ISNULL(k.EV_TELEFON, '') AS EV_TELEFON,
    ISNULL(k.KIMLIK_BABA_ADI, '') AS KIM_BABA_ADI,
    ISNULL(k.KIM_ANA_ADI, '') AS KIM_ANA_ADI,
    ISNULL(k.KIMLIK_DOGUM_YERI, '') AS KIM_DOGUM_YERI,
    ISNULL(k.EV_ADRESI, '') AS EV_ADRESI,
    ISNULL(k.ADAY_NO, 0) AS ADAY_NO,
    ISNULL(k.ON_NOTLAR, '') AS SARI_NOTLAR,
    k.DOGUM_TARIHI,
    k.KAYIT_TARIHI,
    k.RESIM,
    ISNULL(k.ID_GRUP_KARTI, 0) AS ID_GRUP_KARTI,
    ISNULL(k.SERTIFIKA_SINIFI, '') AS SERTIFIKA_SINIFI,
    ISNULL(k.ONCE_SERT_SINIFI, '') AS ONCE_SERT_SINIFI,
    ISNULL(k.ONCE_SERT_BELGESAYI, '') AS ONCE_SERT_BELGESAYI,
    ISNULL(k.KURSIYER_DURUMU, 1) AS KURSIYER_DURUMU
FROM KURSIYER k
WHERE k.ID = @ID;";

            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ID", kursiyerId);
                await con.OpenAsync();

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (!dr.Read())
                        return null;

                    return new Arama_Model
                    {
                        ID = dr["ID"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID"]),
                        ADI = dr["ADI"] == DBNull.Value ? string.Empty : dr["ADI"].ToString(),
                        SOYADI = dr["SOYADI"] == DBNull.Value ? string.Empty : dr["SOYADI"].ToString(),
                        TC_NO = dr["TC_NO"] == DBNull.Value ? string.Empty : dr["TC_NO"].ToString(),
                        KIM_KAYIT_NO = dr["KIM_KAYIT_NO"] == DBNull.Value ? string.Empty : dr["KIM_KAYIT_NO"].ToString(),
                        GSM_1 = dr["GSM_1"] == DBNull.Value ? string.Empty : dr["GSM_1"].ToString(),
                        GSM_2 = dr["GSM_2"] == DBNull.Value ? string.Empty : dr["GSM_2"].ToString(),
                        EV_TELEFON = dr["EV_TELEFON"] == DBNull.Value ? string.Empty : dr["EV_TELEFON"].ToString(),
                        KIM_BABA_ADI = dr["KIM_BABA_ADI"] == DBNull.Value ? string.Empty : dr["KIM_BABA_ADI"].ToString(),
                        KIM_ANA_ADI = dr["KIM_ANA_ADI"] == DBNull.Value ? string.Empty : dr["KIM_ANA_ADI"].ToString(),
                        KIM_DOGUM_YERI = dr["KIM_DOGUM_YERI"] == DBNull.Value ? string.Empty : dr["KIM_DOGUM_YERI"].ToString(),
                        EV_ADRESI = dr["EV_ADRESI"] == DBNull.Value ? string.Empty : dr["EV_ADRESI"].ToString(),
                        ADAY_NO = dr["ADAY_NO"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ADAY_NO"]),
                        SARI_NOTLAR = dr["SARI_NOTLAR"] == DBNull.Value ? string.Empty : dr["SARI_NOTLAR"].ToString(),
                        DOGUM_TARIHI = dr["DOGUM_TARIHI"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["DOGUM_TARIHI"]),
                        KAYIT_TARIHI = dr["KAYIT_TARIHI"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["KAYIT_TARIHI"]),
                        RESIM = dr["RESIM"] == DBNull.Value ? null : (byte[])dr["RESIM"],
                        ID_GRUP_KARTI = dr["ID_GRUP_KARTI"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ID_GRUP_KARTI"]),
                        SERTIFIKA_SINIFI = dr["SERTIFIKA_SINIFI"] == DBNull.Value ? string.Empty : dr["SERTIFIKA_SINIFI"].ToString(),
                        ONCE_SERT_SINIFI = dr["ONCE_SERT_SINIFI"] == DBNull.Value ? string.Empty : dr["ONCE_SERT_SINIFI"].ToString(),
                        ONCE_SERT_BELGESAYI = dr["ONCE_SERT_BELGESAYI"] == DBNull.Value ? string.Empty : dr["ONCE_SERT_BELGESAYI"].ToString(),
                        KURSIYER_DURUMU = dr["KURSIYER_DURUMU"] == DBNull.Value ? 1 : Convert.ToInt32(dr["KURSIYER_DURUMU"])
                    };
                }
            }
        }

        // ------------------ KAYDET ------------------
        private async void Btn_Kaydet_Click(object sender, EventArgs e)
        {
            try
            {
                bool isNew = _model == null || _model.ID <= 0;

                if (_model == null)
                    _model = new Arama_Model();

                _model.ADI = Tnk_ADI.Text;
                _model.SOYADI = Tnk_SOYADI.Text;
                _model.KIM_KAYIT_NO = Tnk_KIM_KAYIT_NO.Text;
                _model.GSM_1 = Tnk_GSM_1.Text;
                _model.GSM_2 = Tnk_GSM_2.Text;
                _model.EV_TELEFON = Tnk_EV_TELEFON.Text;
                _model.KIM_BABA_ADI = Tnk_KIM_BABA_ADI.Text;
                _model.KIM_ANA_ADI = Tnk_KIM_ANA_ADI.Text;
                _model.KIM_DOGUM_YERI = Tnk_KIM_DOGUM_YERI.Text;
                _model.EV_ADRESI = Tnk_EV_ADRESI.Text;
                _model.ADAY_NO = string.IsNullOrWhiteSpace(Tnk_ADAY_NO.Text) ? 0 : Convert.ToInt32(Tnk_ADAY_NO.Text);
                _model.SARI_NOTLAR = Tnk_Referans.Text;
                _model.DOGUM_TARIHI = Dtp_DOGUM_TARIHI.Value;
                _model.KAYIT_TARIHI = Dtp_KAYIT_TARIHI.Value;
                _model.ID_GRUP_KARTI = Convert.ToInt32(Cmb_DONEM.SelectedValue);
                _model.SERTIFIKA_SINIFI = Cmb_SINIFI.SelectedItem != null ? Cmb_SINIFI.SelectedItem.ToString() : null;
                _model.ONCE_SERT_SINIFI = Cmb_ONCEKI_SINIFI.SelectedItem != null ? Cmb_ONCEKI_SINIFI.SelectedItem.ToString() : null;
                _model.ONCE_SERT_BELGESAYI = Tnk_ONCEKI_BELGE.Text;
                _model.KURSIYER_DURUMU = Convert.ToInt32(Cmb_DURUM.SelectedValue);

                if (IsUnderEighteen(_model.DOGUM_TARIHI ?? Dtp_DOGUM_TARIHI.Value, DateTime.Today))
                {
                    MessageBox.Show(
                        "Aday yaşı 18'den küçüktür.",
                        "Yaş Uyarısı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                if (TcKimlikValidator.TryExplainProblem(Tnk_TC_NO.Text, out string tcUyari))
                {
                    MessageBox.Show(tcUyari, "TC Kimlik No", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Tnk_TC_NO.Focus();
                    return;
                }

                _model.TC_NO = TcKimlikValidator.NormalizeForDb(Tnk_TC_NO.Text) ?? string.Empty;

                if (Tnk_RESIM_Kursiyer.Image != null)
                {
                    using (var ms = new MemoryStream())
                    using (var bmp = new Bitmap(Tnk_RESIM_Kursiyer.Image))
                    {
                        bmp.Save(ms, ImageFormat.Jpeg);
                        byte[] data = ResmiStandartBoyutaGetir(ms.ToArray());
                        if (data != null)
                            _model.RESIM = data;
                    }
                }

                var kursiyerModel = new Kursiyer_Model
                {
                    ID = _model.ID,
                    ADI = _model.ADI,
                    SOYADI = _model.SOYADI,
                    TC_NO = _model.TC_NO,
                    KIM_KAYIT_NO = _model.KIM_KAYIT_NO,
                    GSM_1 = _model.GSM_1,
                    GSM_2 = _model.GSM_2,
                    EV_TELEFON = _model.EV_TELEFON,
                    KIM_BABA_ADI = _model.KIM_BABA_ADI,
                    KIM_ANA_ADI = _model.KIM_ANA_ADI,
                    KIM_DOGUM_YERI = _model.KIM_DOGUM_YERI,
                    EV_ADRESI = _model.EV_ADRESI,
                    ADAY_NO = _model.ADAY_NO,
                    SARI_NOTLAR = _model.SARI_NOTLAR,
                    DOGUM_TARIHI = _model.DOGUM_TARIHI ?? DateTime.Today,
                    KAYIT_TARIHI = _model.KAYIT_TARIHI ?? DateTime.Today,
                    RESIM = _model.RESIM,
                    ID_GRUP_KARTI = _model.ID_GRUP_KARTI,
                    SERTIFIKA_SINIFI = _model.SERTIFIKA_SINIFI,
                    ONCE_SERT_SINIFI = _model.ONCE_SERT_SINIFI,
                    ONCE_SERT_BELGESAYI = _model.ONCE_SERT_BELGESAYI,
                    KURSIYER_DURUMU = _model.KURSIYER_DURUMU
                };

                int kursiyerId = await SaveKursiyerDirectAsync(kursiyerModel);
                bool sonuc = kursiyerId > 0;
                if (sonuc)
                {
                    _model.ID = kursiyerId;
                    await SaveCinsiyetAsync(kursiyerId);
                    await SaveWebcamImageIfAnyAsync(kursiyerId);
                    string islem = isNew ? "Aday eklendi" : "Aday guncellendi";
                    AppLogService.Write(
                        _cs,
                        "INFO",
                        "ADAY",
                        islem + ". ID=" + kursiyerId + ", AdSoyad=" + ((_model.ADI ?? "") + " " + (_model.SOYADI ?? "")).Trim());
                }

                MessageBox.Show(
                    sonuc ? "Kayıt başarılı" : "Kayıt başarısız",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    sonuc ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata");
            }
        }

        private void Btn_Webresimekle_Click(object sender, EventArgs e)
        {
            using (var frm = new WebCamResimForm())
            {
                if (frm.ShowDialog(this) != DialogResult.OK && (frm.ProcessedImageBytes == null || frm.ProcessedImageBytes.Length == 0))
                    return;

                _webcamImageBytes = frm.ProcessedImageBytes;
                if (_webcamImageBytes == null || _webcamImageBytes.Length == 0)
                    return;

                using (var ms = new MemoryStream(_webcamImageBytes))
                using (var img = Image.FromStream(ms))
                {
                    Tnk_Webcamre.Image = new Bitmap(img);
                    Tnk_Webcamre.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        // ------------------ RESİM ------------------
        private void Btn_EkleResim_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Resimler (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            byte[] data = File.ReadAllBytes(ofd.FileName);
            data = ResmiStandartBoyutaGetir(data);

            if (data == null)
                return;

            _model.RESIM = data;
            ResmiGoster(data);
        }




        private void HazirlaESinavGrid()
        {
            Dgv_esinav_Liste.AutoGenerateColumns = false;
            Dgv_esinav_Liste.AllowUserToAddRows = false;
            Dgv_esinav_Liste.AllowUserToDeleteRows = false;
            Dgv_esinav_Liste.AllowUserToResizeRows = false;
            Dgv_esinav_Liste.MultiSelect = false;
            Dgv_esinav_Liste.ReadOnly = true;
            Dgv_esinav_Liste.RowHeadersVisible = false;
            Dgv_esinav_Liste.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dgv_esinav_Liste.EnableHeadersVisualStyles = false;
            Dgv_esinav_Liste.BackgroundColor = Color.White;
            Dgv_esinav_Liste.BorderStyle = BorderStyle.None;
            Dgv_esinav_Liste.GridColor = Color.FromArgb(220, 224, 230);
            Dgv_esinav_Liste.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 255);
            Dgv_esinav_Liste.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            Dgv_esinav_Liste.DefaultCellStyle.SelectionForeColor = Color.White;
            Dgv_esinav_Liste.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            Dgv_esinav_Liste.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dgv_esinav_Liste.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Dgv_esinav_Liste.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            Dgv_esinav_Liste.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Dgv_esinav_Liste.ColumnHeadersHeight = 32;
            Dgv_esinav_Liste.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Tasarimda sutun tanimlandiysa Clear/Add yapma; aksi halde her acilista tasarim silinir.
            if (Dgv_esinav_Liste.Columns.Count > 0)
                return;

            Dgv_esinav_Liste.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ESINAV_TARIHI",
                DataPropertyName = "ESINAV_TARIHI",
                HeaderText = "E-Sınav Tarihi",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            Dgv_esinav_Liste.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TEO_NOT",
                DataPropertyName = "TEO_NOT",
                HeaderText = "Teo Not",
                Width = 60,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            Dgv_esinav_Liste.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TEO_HAK",
                DataPropertyName = "TEO_HAK",
                HeaderText = "Hak",
                Width = 50,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            Dgv_esinav_Liste.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TEO_DURUM",
                DataPropertyName = "TEO_DURUM",
                HeaderText = "Durum",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

        }


        private async void Btn_Sil_Click(object sender, EventArgs e)
        {
            if (_model == null || _model.ID == 0)
                return;

            int silinecekId = _model.ID;
            string silinecekAdSoyad = ((_model.ADI ?? "") + " " + (_model.SOYADI ?? "")).Trim();

            if (MessageBox.Show("Silmek istiyor musunuz?", "Onay",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            await _kursiyerService.DeleteKursiyerAsync(_model.ID);
            bool sonuc = true;

            if (sonuc)
            {
                AppLogService.Write(
                    _cs,
                    "WARN",
                    "ADAY",
                    "Aday silindi. ID=" + silinecekId + ", AdSoyad=" + silinecekAdSoyad);
                MessageBox.Show("Kayıt silindi", "Bilgi");
                Close();
            }
            else
            {
                MessageBox.Show("Silme işlemi başarısız", "Hata");
            }
        }

        private void Btn_Tarama_Click(object sender, EventArgs e)
        {
            var taramaForm = new Tarama_Sayfam(
     _model != null ? _model.RESIM : null,
     TaramaTipi.KursiyerResmi,
     _cs);

            taramaForm.TaramaTamamlandi += delegate (byte[] data)
            {
                data = ResmiStandartBoyutaGetir(data);
                if (data == null)
                    return;

                _model.RESIM = data;
                ResmiGoster(data);
            };

            taramaForm.ShowDialog();
        }

        private void ResmiGoster(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var img = Image.FromStream(ms))
            {
                Tnk_RESIM_Kursiyer.Image = new Bitmap(img);
            }
            Tnk_RESIM_Kursiyer.SizeMode = PictureBoxSizeMode.Zoom;
        }
        private void HazirlaDireksiyonGrid()
        {
            Dgv_Direksiyon_Liste.AutoGenerateColumns = false;
            Dgv_Direksiyon_Liste.AllowUserToAddRows = Dgv_esinav_Liste.AllowUserToAddRows;
            Dgv_Direksiyon_Liste.AllowUserToDeleteRows = Dgv_esinav_Liste.AllowUserToDeleteRows;
            Dgv_Direksiyon_Liste.AllowUserToResizeRows = Dgv_esinav_Liste.AllowUserToResizeRows;
            Dgv_Direksiyon_Liste.MultiSelect = Dgv_esinav_Liste.MultiSelect;
            Dgv_Direksiyon_Liste.ReadOnly = Dgv_esinav_Liste.ReadOnly;
            Dgv_Direksiyon_Liste.RowHeadersVisible = Dgv_esinav_Liste.RowHeadersVisible;
            Dgv_Direksiyon_Liste.SelectionMode = Dgv_esinav_Liste.SelectionMode;
            Dgv_Direksiyon_Liste.EnableHeadersVisualStyles = Dgv_esinav_Liste.EnableHeadersVisualStyles;
            Dgv_Direksiyon_Liste.BackgroundColor = Dgv_esinav_Liste.BackgroundColor;
            Dgv_Direksiyon_Liste.BorderStyle = Dgv_esinav_Liste.BorderStyle;
            Dgv_Direksiyon_Liste.GridColor = Dgv_esinav_Liste.GridColor;
            Dgv_Direksiyon_Liste.AlternatingRowsDefaultCellStyle = Dgv_esinav_Liste.AlternatingRowsDefaultCellStyle;
            Dgv_Direksiyon_Liste.DefaultCellStyle.SelectionBackColor = Dgv_esinav_Liste.DefaultCellStyle.SelectionBackColor;
            Dgv_Direksiyon_Liste.DefaultCellStyle.SelectionForeColor = Dgv_esinav_Liste.DefaultCellStyle.SelectionForeColor;
            Dgv_Direksiyon_Liste.ColumnHeadersDefaultCellStyle = Dgv_esinav_Liste.ColumnHeadersDefaultCellStyle;
            Dgv_Direksiyon_Liste.DefaultCellStyle.Font = Dgv_esinav_Liste.DefaultCellStyle.Font;
            Dgv_Direksiyon_Liste.AutoSizeColumnsMode = Dgv_esinav_Liste.AutoSizeColumnsMode;
            Dgv_Direksiyon_Liste.ColumnHeadersHeight = Dgv_esinav_Liste.ColumnHeadersHeight;
            Dgv_Direksiyon_Liste.ColumnHeadersHeightSizeMode = Dgv_esinav_Liste.ColumnHeadersHeightSizeMode;

            if (Dgv_Direksiyon_Liste.Columns.Count > 0)
                return;

            Dgv_Direksiyon_Liste.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SINAV_TARIHI",
                DataPropertyName = "SINAV_TARIHI",
                HeaderText = "Sınav Tarihi",
                Width = 120
            });

            Dgv_Direksiyon_Liste.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DIR_HAK",
                DataPropertyName = "DIR_HAK",
                HeaderText = "Hak",
                Width = 60
            });

            Dgv_Direksiyon_Liste.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DIR_DURUM",
                DataPropertyName = "DIR_DURUM",
                HeaderText = "Durum",
                Width = 100
            });

        }
         
        private byte[] ResmiStandartBoyutaGetir(byte[] data)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                using (var original = new Bitmap(ms))
                {
                    Bitmap yeni = new Bitmap(394, 512);

                    using (Graphics g = Graphics.FromImage(yeni))
                    {
                        g.Clear(Color.White);
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawImage(original, 0, 0, 394, 512);
                    }

                    using (var msOut = new MemoryStream())
                    {
                        yeni.Save(msOut, ImageFormat.Jpeg);
                        return msOut.ToArray();
                    }
                }
            }
            catch
            {
                MessageBox.Show("Resim işlenemedi.", "Hata");
                return null;
            }
        }

        private void Btn_Evrak_Click(object sender, EventArgs e)
        {

            if (_model == null || _model.ID == 0)
            {
                MessageBox.Show("Kursiyer bilgisi alınamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Arama_Model -> Kursiyer_Model dönüşümü
            var kursiyerModel = new Kursiyer_Model
            {
                ID = _model.ID,
                ADI = _model.ADI,
                SOYADI = _model.SOYADI,
                RESIM = _model.RESIM,
                TC_NO = _model.TC_NO,
                KIM_KAYIT_NO = _model.KIM_KAYIT_NO
                // Diğer alanlar gerekirse eklenebilir
            };

            // KursiyerEvrakService oluştur
            var evrakService = new KursiyerEvrakService(_cs);

            // Evraklar_Sayfam formunu aç
            using (var evrakForm = new Evraklar_Sayfam(kursiyerModel, evrakService, _cs))
            {
                evrakForm.ShowDialog();
            }


        }
        private void Dgv_esinav_Liste_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (Dgv_esinav_Liste.Columns[e.ColumnIndex].Name != "TEO_DURUM")
                return;

            string durum = e.Value?.ToString()?.ToUpper();

            if (durum == "GEÇTİ")
            {
                Dgv_esinav_Liste.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                Dgv_esinav_Liste.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
            }
            else if (durum == "KALDI")
            {
                Dgv_esinav_Liste.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                Dgv_esinav_Liste.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
            }
            else if (durum == "GİRMEDİ" || durum == "GIRMEDI")
            {
                Dgv_esinav_Liste.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                Dgv_esinav_Liste.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.Black;
            }
        }
        private async Task DireksiyonSinavlariYukle()
        {
            try
            {
                var list = await _direksiyonService.GetDireksiyonSinavlariAsync(_kursiyerId);

                if (list != null)
                    Dgv_Direksiyon_Liste.DataSource = list;
            }
            catch (Exception ex)
            {
                try
                {
                    Dgv_Direksiyon_Liste.DataSource = await GetDireksiyonSinavlariDirectAsync(_kursiyerId);
                }
                catch
                {
                    MessageBox.Show("Direksiyon sınavları yüklenemedi\n" + ex.Message);
                }
            }
        }

        private async Task<DataTable> GetDireksiyonSinavlariDirectAsync(int kursiyerId)
        {
            const string sql = @"
SELECT
    d.ID,
    st.SINAV_TARIHI,
    ISNULL(d.DIR_HAK,0) AS DIR_HAK,
    ISNULL(d.DIR_DURUM,'GIRMEDI') AS DIR_DURUM,
    ISNULL(d.RANDEVU_SAATI,'') AS RANDEVU_SAATI
FROM SINAV_LISTE_DIREKSIYON d
INNER JOIN SINAV_TARIHLERI st ON st.ID = d.ID_SINAV_TARIHI
WHERE d.ID_KURSIYER = @ID_KURSIYER
ORDER BY st.SINAV_TARIHI DESC;";

            var dt = new DataTable();
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                await con.OpenAsync();
                da.Fill(dt);
            }
            return dt;
        }
        private void Dgv_Direksiyon_Liste_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = Dgv_Direksiyon_Liste.Rows[e.RowIndex];
            if (row.Cells["DIR_DURUM"].Value == null) return;

            string durum = row.Cells["DIR_DURUM"].Value.ToString().ToUpperInvariant();

            // Tüm olası durumlar
            switch (durum)
            {
                case string s when s.Contains("GEÇ"):
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    break;

                case string s when s.Contains("KAL"):
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    break;

                case string s when s.Contains("GİR") || durum.Contains("GIR"):
                    row.DefaultCellStyle.BackColor = Color.Khaki;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    break;

                default:
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.DefaultCellStyle.ForeColor = Color.Black;
                    break;
            }
        }
        // E-Sınavları da aynı SP ile yükleyebiliriz, filtreleme sadece tipi ile yapılır
        private async Task ESinavlariYukle()
        {
            try
            {
                var dt = await _eSinavService.GetESinavAsync(_kursiyerId);
                ESinavKolonEslesmeleriniUygula(dt);
                Dgv_esinav_Liste.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("E-Sınav yüklenemedi\n" + ex.Message);
            }
        }

        private void ESinavKolonEslesmeleriniUygula(DataTable dt)
        {
            if (dt == null)
                return;

            string tarihKolonu = IlkVarOlanKolon(dt, new[] { "ESINAV_TARIHI", "E_SINAV_TARIHI", "SINAV_TARIHI" });
            string notKolonu = IlkVarOlanKolon(dt, new[] { "TEO_NOT", "NOT", "PUAN" });
            string hakKolonu = IlkVarOlanKolon(dt, new[] { "TEO_HAK", "HAK" });
            string durumKolonu = IlkVarOlanKolon(dt, new[] { "TEO_DURUM", "DURUM", "SONUC" });

            if (string.IsNullOrWhiteSpace(durumKolonu))
            {
                if (!dt.Columns.Contains("TEO_DURUM"))
                    dt.Columns.Add("TEO_DURUM", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    string notText = notKolonu != null ? Convert.ToString(row[notKolonu]) : string.Empty;
                    int notDegeri;
                    if (int.TryParse(notText, out notDegeri))
                        row["TEO_DURUM"] = notDegeri >= 70 ? "GEÇTİ" : "KALDI";
                    else
                        row["TEO_DURUM"] = "GİRMEDİ";
                }

                durumKolonu = "TEO_DURUM";
            }

            var colTarih = Dgv_esinav_Liste.Columns["ESINAV_TARIHI"];
            if (colTarih != null && !string.IsNullOrWhiteSpace(tarihKolonu))
                colTarih.DataPropertyName = tarihKolonu;

            var colNot = Dgv_esinav_Liste.Columns["TEO_NOT"];
            if (colNot != null && !string.IsNullOrWhiteSpace(notKolonu))
                colNot.DataPropertyName = notKolonu;

            var colHak = Dgv_esinav_Liste.Columns["TEO_HAK"];
            if (colHak != null && !string.IsNullOrWhiteSpace(hakKolonu))
                colHak.DataPropertyName = hakKolonu;

            var colDurum = Dgv_esinav_Liste.Columns["TEO_DURUM"];
            if (colDurum != null && !string.IsNullOrWhiteSpace(durumKolonu))
                colDurum.DataPropertyName = durumKolonu;
        }

        private static string IlkVarOlanKolon(DataTable dt, IEnumerable<string> adaylar)
        {
            if (dt == null || adaylar == null)
                return null;

            foreach (var aday in adaylar)
            {
                if (!string.IsNullOrWhiteSpace(aday) && dt.Columns.Contains(aday))
                    return aday;
            }

            return null;
        }
        private void Btn_Mebbis_Aktar_Click(object sender, EventArgs e)
        {
            if (_model == null || _model.ID == 0)
            {
                MessageBox.Show("Seçili kursiyer yok!");
                return;
            }

            byte[] resim = _model.RESIM;
            var evrak = _evrakService.GetKursiyerEvrak(_model.ID);

            string mebbisKullanici;
            string mebbisSifre;
            bool bulundu = MebbisCredentialResolver.TryResolve(_cs, AppSession.CurrentUserName, out mebbisKullanici, out mebbisSifre);
            if (!bulundu || string.IsNullOrWhiteSpace(mebbisKullanici))
            {
                MessageBox.Show("MEBBİS kullanıcı bilgisi yok!");
                return;
            }

            var kursiyer = new MebbisKursiyerModel
            {
                ID_Kursiyer = _model.ID,
                ADI = _model.ADI,
                SOYADI = _model.SOYADI,
                SERTIFIKA_SINIFI = _model.SERTIFIKA_SINIFI,
                ONCE_SERT_SINIFI = _model.ONCE_SERT_SINIFI,
                Foto = resim
            };

            if (_mebbisForm == null || _mebbisForm.IsDisposed)
            {
                _mebbisForm = new MebbisWebForm(
                    kullaniciAdi: mebbisKullanici,
                    sifre: mebbisSifre,
                    kursiyer: kursiyer,
                    resim: resim,
                    evrak: evrak,
                    connectionString: _cs);
                _mebbisForm.Show(this);
                return;
            }

            _mebbisForm.KursiyerYukle(kursiyer, resim, evrak);
            if (!_mebbisForm.Visible)
                _mebbisForm.Show(this);
            else
                _mebbisForm.BringToFront();
        }

        private async void Btn_Aday_Kopyala_Click(object sender, EventArgs e)
        {
            if (_model == null || _model.ID <= 0)
            {
                MessageBox.Show("Kopyalama için kayıtlı bir kursiyer seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var onay = MessageBox.Show(
                "Kursiyer ve evrak bilgileri yeni aday kaydı olarak kopyalansın mı?\nSertifika Sınıfı boş bırakılacaktır.",
                "Aday Kopyala",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (onay != DialogResult.Yes)
                return;

            try
            {
                Arama_Model kaynak = null;
                try
                {
                    kaynak = await _aramaService.GetKursiyerByIdAsync(_model.ID);
                }
                catch
                {
                    // Servis eski kolon adlarini bekliyorsa direkt SQL fallback ile devam et.
                    kaynak = null;
                }

                if (kaynak == null)
                    kaynak = await GetKursiyerByIdDirectAsync(_model.ID);
                if (kaynak == null)
                    kaynak = _model;

                var yeniKursiyer = new Kursiyer_Model
                {
                    ID = 0,
                    ADI = kaynak.ADI,
                    SOYADI = kaynak.SOYADI,
                    TC_NO = kaynak.TC_NO,
                    KIM_KAYIT_NO = kaynak.KIM_KAYIT_NO,
                    GSM_1 = kaynak.GSM_1,
                    GSM_2 = kaynak.GSM_2,
                    EV_TELEFON = kaynak.EV_TELEFON,
                    KIM_BABA_ADI = kaynak.KIM_BABA_ADI,
                    KIM_ANA_ADI = kaynak.KIM_ANA_ADI,
                    KIM_DOGUM_YERI = kaynak.KIM_DOGUM_YERI,
                    EV_ADRESI = kaynak.EV_ADRESI,
                    ADAY_NO = kaynak.ADAY_NO,
                    SARI_NOTLAR = kaynak.SARI_NOTLAR,
                    DOGUM_TARIHI = kaynak.DOGUM_TARIHI ?? DateTime.Today,
                    KAYIT_TARIHI = kaynak.KAYIT_TARIHI ?? DateTime.Today,
                    RESIM = kaynak.RESIM,
                    ID_GRUP_KARTI = kaynak.ID_GRUP_KARTI,
                    SERTIFIKA_SINIFI = null, // Istek: hedef sinif bos kalsin
                    ONCE_SERT_SINIFI = kaynak.ONCE_SERT_SINIFI,
                    ONCE_SERT_BELGESAYI = kaynak.ONCE_SERT_BELGESAYI,
                    KURSIYER_DURUMU = kaynak.KURSIYER_DURUMU
                };

                int yeniKursiyerId = await InsertAdayKopyasiDirectAsync(yeniKursiyer);
                if (yeniKursiyerId <= 0)
                    throw new Exception("Yeni kursiyer kaydı oluşturulamadı.");

                await TryCopyEvrakForAdayAsync(kaynak.ID, yeniKursiyerId);

                MessageBox.Show("Aday kopyalandı. Sertifika Sınıfı boş bırakıldı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                AcKopyalananAdayDetayFormu(yeniKursiyerId);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Aday kopyalama sırasında hata oluştu.\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AcKopyalananAdayDetayFormu(int yeniKursiyerId)
        {
            var yeniDetayForm = new KursiyerDetay_Sayfam(_cs, yeniKursiyerId);

            // Mevcut form bir panel icinde gomulu acildiysa yeni formu da ayni panelde ac.
            if (!TopLevel && Parent != null)
            {
                var host = Parent;
                host.Controls.Clear();

                yeniDetayForm.TopLevel = false;
                yeniDetayForm.FormBorderStyle = FormBorderStyle.None;
                yeniDetayForm.Dock = DockStyle.Fill;

                host.Controls.Add(yeniDetayForm);
                yeniDetayForm.Show();
                yeniDetayForm.BringToFront();
                yeniDetayForm.Focus();
                return;
            }

            // Normal pencere modunda aciliyorsa one getir.
            FormWorkspaceLayoutHelper.ApplyWorkingAreaMaximized(yeniDetayForm);
            yeniDetayForm.Show();
            yeniDetayForm.BringToFront();
            yeniDetayForm.Activate();
            Close();
        }

        private async Task<int> InsertAdayKopyasiDirectAsync(Kursiyer_Model model)
        {
            const string sql = @"
INSERT INTO KURSIYER
(
    ADI, SOYADI, TC_NO, KIMLIK_KAYIT_NO, GSM_1, GSM_2, EV_TELEFON,
    KIMLIK_BABA_ADI, KIM_ANA_ADI, KIMLIK_DOGUM_YERI, EV_ADRESI, ADAY_NO,
    ON_NOTLAR, DOGUM_TARIHI, KAYIT_TARIHI, RESIM, ID_GRUP_KARTI,
    SERTIFIKA_SINIFI, ONCE_SERT_SINIFI, ONCE_SERT_BELGESAYI, KURSIYER_DURUMU
)
OUTPUT INSERTED.ID
VALUES
(
    @ADI, @SOYADI, @TC_NO, @KIMLIK_KAYIT_NO, @GSM_1, @GSM_2, @EV_TELEFON,
    @KIMLIK_BABA_ADI, @KIM_ANA_ADI, @KIMLIK_DOGUM_YERI, @EV_ADRESI, @ADAY_NO,
    @ON_NOTLAR, @DOGUM_TARIHI, @KAYIT_TARIHI, @RESIM, @ID_GRUP_KARTI,
    @SERTIFIKA_SINIFI, @ONCE_SERT_SINIFI, @ONCE_SERT_BELGESAYI, @KURSIYER_DURUMU
);";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ADI", (object)model.ADI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SOYADI", (object)model.SOYADI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TC_NO", (object)model.TC_NO ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KIMLIK_KAYIT_NO", (object)model.KIM_KAYIT_NO ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GSM_1", (object)model.GSM_1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GSM_2", (object)model.GSM_2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EV_TELEFON", (object)model.EV_TELEFON ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KIMLIK_BABA_ADI", (object)model.KIM_BABA_ADI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KIM_ANA_ADI", (object)model.KIM_ANA_ADI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KIMLIK_DOGUM_YERI", (object)model.KIM_DOGUM_YERI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EV_ADRESI", (object)model.EV_ADRESI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ADAY_NO", (object)model.ADAY_NO ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ON_NOTLAR", (object)model.SARI_NOTLAR ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DOGUM_TARIHI", (object)model.DOGUM_TARIHI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KAYIT_TARIHI", (object)model.KAYIT_TARIHI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RESIM", (object)model.RESIM ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_GRUP_KARTI", (object)model.ID_GRUP_KARTI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SERTIFIKA_SINIFI", DBNull.Value); // istek: bos
                cmd.Parameters.AddWithValue("@ONCE_SERT_SINIFI", (object)model.ONCE_SERT_SINIFI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ONCE_SERT_BELGESAYI", (object)model.ONCE_SERT_BELGESAYI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KURSIYER_DURUMU", (object)model.KURSIYER_DURUMU ?? DBNull.Value);

                await conn.OpenAsync();
                object idObj = await cmd.ExecuteScalarAsync();
                int yeniId;
                return int.TryParse(Convert.ToString(idObj), out yeniId) ? yeniId : 0;
            }
        }

        private async Task TryCopyEvrakForAdayAsync(int kaynakKursiyerId, int hedefKursiyerId)
        {
            if (kaynakKursiyerId <= 0 || hedefKursiyerId <= 0)
                return;

            try
            {
                var evrakService = new KursiyerEvrakService(_cs);
                var kaynakEvrak = evrakService.GetKursiyerEvrak(kaynakKursiyerId);
                if (kaynakEvrak != null)
                {
                    kaynakEvrak.ID_Kursiyer = hedefKursiyerId;
                    evrakService.UpsertKursiyerEvrak(kaynakEvrak);
                    return;
                }
            }
            catch
            {
                // Servis eski kolonlari bekliyorsa direct SQL fallback ile devam edilir.
            }

            await CopyKursiyerEvrakDirectAsync(kaynakKursiyerId, hedefKursiyerId);
        }

        private async Task CopyKursiyerEvrakDirectAsync(int kaynakKursiyerId, int hedefKursiyerId)
        {
            using (var conn = new SqlConnection(_cs))
            {
                await conn.OpenAsync();
                var cols = await GetTableColumnsAsync(conn, "KURSIYER_EVRAK");
                if (cols.Count == 0)
                    return;

                var map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    { "EKSIK_OGRNIM_BEL", new[] { "EKSIK_OGRNIM_BEL", "EKSK_OGRNIM_BEL" } },
                    { "EKSIK_SAGLIK", new[] { "EKSIK_SAGLIK", "EKSK_SAGLIK" } },
                    { "EKSIK_SAVCILIK", new[] { "EKSIK_SAVCILIK", "EKSK_SAVCILIK" } },
                    { "EKSIK_SOZLESME", new[] { "EKSIK_SOZLESME", "EKSK_SOZLESME" } },
                    { "EKSIK_IMZA", new[] { "EKSIK_IMZA", "EKSK_IMZA" } },
                    { "EKSIK_WEPCAM", new[] { "EKSIK_WEPCAM", "EKSK_WEPCAM" } },
                    { "OGRNM_BEL_TURU", new[] { "OGRNM_BEL_TURU", "OGR_BEL_TURU" } },
                    { "OGRNM_BEL_VEREN_KURUM", new[] { "OGRNM_BEL_VEREN_KURUM", "OGR_BEL_VEREN_KURUM" } },
                    { "OGRNM_BEL_TARIHI", new[] { "OGRNM_BEL_TARIHI", "OGR_BEL_TARIHI" } },
                    { "OGRNM_BEL_SAYISI", new[] { "OGRNM_BEL_SAYISI", "OGR_BEL_SAYISI" } },
                    { "SAG_RAP_VEREN_KURUM", new[] { "SAG_RAP_VEREN_KURUM", "SAG_RAPOR_VEREN_KURUM" } },
                    { "SAG_RAP_TARIHI", new[] { "SAG_RAP_TARIHI", "SAG_RAPOR_TARIHI" } },
                    { "SAG_RAP_BELGENO", new[] { "SAG_RAP_BELGENO", "SAG_RAPOR_BELGENO" } },
                    { "CriminalNo", new[] { "CriminalNo", "SAVCILIK_BEL_NO" } },
                    { "RES_OGRNIM_BEL", new[] { "RES_OGRNIM_BEL", "IMG_OGRNIM_BEL" } },
                    { "RES_SAGLIK", new[] { "RES_SAGLIK", "IMG_SAGLIK" } },
                    { "RES_SAVCILIK", new[] { "RES_SAVCILIK", "IMG_SAVCILIK" } },
                    { "RES_SOZLESME_ON", new[] { "RES_SOZLESME_ON", "IMG_SOZLESME_ON" } },
                    { "RES_SOZLESME_ARKA", new[] { "RES_SOZLESME_ARKA", "IMG_SOZLESME_ARKA" } },
                    { "RES_IMZA", new[] { "RES_IMZA", "IMG_IMZA" } }
                };

                var copiedValues = await ReadEvrakSourceValuesAsync(conn, cols, map, kaynakKursiyerId);
                if (copiedValues.Count == 0)
                    return;

                await UpsertEvrakValuesAsync(conn, cols, copiedValues, hedefKursiyerId);
            }
        }

        private async Task<Dictionary<string, object>> ReadEvrakSourceValuesAsync(
            SqlConnection conn,
            HashSet<string> cols,
            Dictionary<string, string[]> map,
            int kaynakKursiyerId)
        {
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var selectParts = new List<string>();

            foreach (var kv in map)
            {
                if (!cols.Contains(kv.Key))
                    continue;

                string sourceCol = FirstExistingColumn(cols, kv.Value);
                if (string.IsNullOrWhiteSpace(sourceCol))
                    continue;

                selectParts.Add("[" + sourceCol + "] AS [" + kv.Key + "]");
            }

            if (selectParts.Count == 0)
                return result;

            string selectSql = "SELECT TOP 1 " + string.Join(", ", selectParts) +
                               " FROM [KURSIYER_EVRAK] WHERE [ID_KURSIYER]=@ID";

            using (var cmd = new SqlCommand(selectSql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", kaynakKursiyerId);
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    if (!await r.ReadAsync())
                        return result;

                    for (int i = 0; i < r.FieldCount; i++)
                        result[r.GetName(i)] = r.IsDBNull(i) ? DBNull.Value : r.GetValue(i);
                }
            }

            return result;
        }

        private async Task UpsertEvrakValuesAsync(
            SqlConnection conn,
            HashSet<string> cols,
            Dictionary<string, object> copiedValues,
            int hedefKursiyerId)
        {
            if (!cols.Contains("ID_KURSIYER") || copiedValues.Count == 0)
                return;

            var targetCols = copiedValues.Keys
                .Where(cols.Contains)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (targetCols.Count == 0)
                return;

            string updateSet = string.Join(", ", targetCols.Select(c => "[" + c + "]=@" + c));
            string insertCols = "[ID_KURSIYER], " + string.Join(", ", targetCols.Select(c => "[" + c + "]"));
            string insertVals = "@ID_KURSIYER, " + string.Join(", ", targetCols.Select(c => "@" + c));

            string sql = @"
UPDATE [KURSIYER_EVRAK]
   SET " + updateSet + @"
 WHERE [ID_KURSIYER]=@ID_KURSIYER;
IF @@ROWCOUNT = 0
BEGIN
    INSERT INTO [KURSIYER_EVRAK] (" + insertCols + @")
    VALUES (" + insertVals + @");
END;";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID_KURSIYER", hedefKursiyerId);
                foreach (string col in targetCols)
                    cmd.Parameters.AddWithValue("@" + col, copiedValues[col] ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task<HashSet<string>> GetTableColumnsAsync(SqlConnection conn, string tableName)
        {
            var cols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            const string sql = @"
SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@TABLE;";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@TABLE", tableName);
                using (var r = await cmd.ExecuteReaderAsync())
                {
                    while (await r.ReadAsync())
                    {
                        string col = Convert.ToString(r["COLUMN_NAME"]);
                        if (!string.IsNullOrWhiteSpace(col))
                            cols.Add(col);
                    }
                }
            }

            return cols;
        }

        private static string FirstExistingColumn(HashSet<string> cols, IEnumerable<string> candidates)
        {
            foreach (var c in candidates ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(c) && cols.Contains(c))
                    return c;
            }
            return null;
        }

        private void HazirlaOdemeBilgiGrid()
        {
            if (Dvg_Odeme_Bilgi == null)
                return;

            Dvg_Odeme_Bilgi.AutoGenerateColumns = false;
            Dvg_Odeme_Bilgi.Columns.Clear();
            Dvg_Odeme_Bilgi.AllowUserToAddRows = false;
            Dvg_Odeme_Bilgi.AllowUserToDeleteRows = false;
            Dvg_Odeme_Bilgi.AllowUserToResizeRows = false;
            Dvg_Odeme_Bilgi.MultiSelect = false;
            Dvg_Odeme_Bilgi.ReadOnly = true;
            Dvg_Odeme_Bilgi.RowHeadersVisible = false;
            Dvg_Odeme_Bilgi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dvg_Odeme_Bilgi.EnableHeadersVisualStyles = false;
            Dvg_Odeme_Bilgi.BackgroundColor = Color.White;
            Dvg_Odeme_Bilgi.BorderStyle = BorderStyle.None;
            Dvg_Odeme_Bilgi.GridColor = Color.FromArgb(220, 224, 230);
            Dvg_Odeme_Bilgi.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 255);
            Dvg_Odeme_Bilgi.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            Dvg_Odeme_Bilgi.DefaultCellStyle.SelectionForeColor = Color.White;
            Dvg_Odeme_Bilgi.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            Dvg_Odeme_Bilgi.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dvg_Odeme_Bilgi.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Dvg_Odeme_Bilgi.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            Dvg_Odeme_Bilgi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Dvg_Odeme_Bilgi.ColumnHeadersHeight = 32;
            Dvg_Odeme_Bilgi.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            Dvg_Odeme_Bilgi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MAKBUZNO",
                DataPropertyName = "MAKBUZNO",
                HeaderText = "Makbuz No",
                FillWeight = 90,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            Dvg_Odeme_Bilgi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TARIH",
                DataPropertyName = "TARIH",
                HeaderText = "Tarih",
                FillWeight = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            Dvg_Odeme_Bilgi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ODENEN",
                DataPropertyName = "ODENEN",
                HeaderText = "Odenen",
                FillWeight = 110,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            if (Lbl_ToplamBorc != null)
                Lbl_ToplamBorc.BringToFront();
            if (Lbl_KalanBorc != null)
                Lbl_KalanBorc.BringToFront();

        }

        private async Task LoadOdemeBilgiGridAsync()
        {
            if (Lbl_ToplamBorc != null)
                Lbl_ToplamBorc.Text = "TOPLAM BORC: 0,00 TL";
            if (Lbl_KalanBorc != null)
                Lbl_KalanBorc.Text = "KALAN BORC: 0,00 TL";

            if (Dvg_Odeme_Bilgi == null || string.IsNullOrWhiteSpace(_cs))
                return;

            HazirlaOdemeBilgiGrid();
            if (groupBox1 != null)
                groupBox1.Text = "Odeme Bilgisi";
            if (Lbl_ToplamBorc != null)
            {
                Lbl_ToplamBorc.ForeColor = Color.DarkRed;
                Lbl_ToplamBorc.Text = "TOPLAM BORC: 0,00 TL";
            }
            if (Lbl_KalanBorc != null)
            {
                Lbl_KalanBorc.ForeColor = Color.DarkRed;
                Lbl_KalanBorc.Text = "KALAN BORC: 0,00 TL";
            }
            var dt = new DataTable();
            dt.Columns.Add("MAKBUZNO", typeof(string));
            dt.Columns.Add("TARIH", typeof(string));
            dt.Columns.Add("ODENEN", typeof(string));

            int kursiyerId = _model != null && _model.ID > 0 ? _model.ID : _kursiyerId;
            if (kursiyerId <= 0)
            {
                Dvg_Odeme_Bilgi.DataSource = dt;
                return;
            }

            try
            {
                using (var conn = new SqlConnection(_cs))
                {
                    await conn.OpenAsync();

                    decimal toplamBorc = 0m;
                    decimal kalanBorc = 0m;
                    decimal hareketToplam = 0m;
                    decimal hareketKalan = 0m;

                    var kursiyerCols = await GetTableColumnsAsync(conn, "KURSIYER");
                    string colToplamBorc = FirstExistingColumn(kursiyerCols, new[] { "TOPLAM_BORC", "TOPLAMBORC" });
                    string colKalanBorc = FirstExistingColumn(kursiyerCols, new[] { "KALANBORC", "KALAN_BORC", "KALAN" });
                    if (!string.IsNullOrWhiteSpace(colToplamBorc) || !string.IsNullOrWhiteSpace(colKalanBorc))
                    {
                        string sqlOzet = "SELECT " +
                                         (!string.IsNullOrWhiteSpace(colToplamBorc) ? "[" + colToplamBorc + "]" : "NULL") + " AS TOPLAM_BORC, " +
                                         (!string.IsNullOrWhiteSpace(colKalanBorc) ? "[" + colKalanBorc + "]" : "NULL") + " AS KALAN_BORC " +
                                         "FROM KURSIYER WHERE ID=@ID";
                        using (var cmdOzet = new SqlCommand(sqlOzet, conn))
                        {
                            cmdOzet.Parameters.AddWithValue("@ID", kursiyerId);
                            using (var r = await cmdOzet.ExecuteReaderAsync())
                            {
                                if (await r.ReadAsync())
                                {
                                    toplamBorc = ToDecimalSafe(r["TOPLAM_BORC"]);
                                    kalanBorc = ToDecimalSafe(r["KALAN_BORC"]);
                                }
                            }
                        }
                    }

                    var odemeCols = await GetTableColumnsAsync(conn, "KURSIYER_ODEME_HAREKET");
                    string kursiyerKolonu = FirstExistingColumn(odemeCols, new[] { "KURSIYER_ID", "ID_KURSIYER" });
                    if (!string.IsNullOrWhiteSpace(kursiyerKolonu))
                    {
                        string makbuzNoKolonu = FirstExistingColumn(odemeCols, new[] { "MAKBUZNO", "MAKBUZ_NO", "MAKBUZNOU" });
                        string orderBy = odemeCols.Contains("ID") ? "[ID] DESC" : (odemeCols.Contains("TARIH") ? "[TARIH] DESC" : string.Empty);
                        string sql = "SELECT TOP 10 " +
                                     (!string.IsNullOrWhiteSpace(makbuzNoKolonu) ? "[" + makbuzNoKolonu + "] AS [MAKBUZNO]" : "NULL AS [MAKBUZNO]") + ", " +
                                     (odemeCols.Contains("TARIH") ? "[TARIH]" : "NULL AS [TARIH]") + ", " +
                                     (odemeCols.Contains("TOPLAM") ? "[TOPLAM]" : "NULL AS [TOPLAM]") + ", " +
                                     (odemeCols.Contains("ODENEN") ? "[ODENEN]" : "NULL AS [ODENEN]") + ", " +
                                     (odemeCols.Contains("KALAN") ? "[KALAN]" : "NULL AS [KALAN]") +
                                     " FROM [dbo].[KURSIYER_ODEME_HAREKET] WHERE [" + kursiyerKolonu + "]=@ID" +
                                     (string.IsNullOrWhiteSpace(orderBy) ? string.Empty : " ORDER BY " + orderBy);

                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", kursiyerId);
                            using (var r = await cmd.ExecuteReaderAsync())
                            {
                                while (await r.ReadAsync())
                                {
                                    string makbuzNo = r["MAKBUZNO"] == DBNull.Value ? string.Empty : Convert.ToString(r["MAKBUZNO"]);
                                    string tarih = r["TARIH"] == DBNull.Value ? string.Empty : Convert.ToString(r["TARIH"]);
                                    decimal satirToplam = ToDecimalSafe(r["TOPLAM"]);
                                    decimal satirOdenen = ToDecimalSafe(r["ODENEN"]);
                                    decimal satirKalan = ToDecimalSafe(r["KALAN"]);
                                    string odenen = satirOdenen.ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
                                    dt.Rows.Add(makbuzNo, tarih, odenen);

                                    if (hareketToplam <= 0m && satirToplam > 0m)
                                        hareketToplam = satirToplam;
                                    if (hareketKalan <= 0m && satirKalan >= 0m)
                                        hareketKalan = satirKalan;
                                }
                            }
                        }

                        string ozetSql = "SELECT TOP 1 " +
                                         (odemeCols.Contains("TOPLAM") ? "[TOPLAM]" : "NULL AS [TOPLAM]") + ", " +
                                         (odemeCols.Contains("KALAN") ? "[KALAN]" : "NULL AS [KALAN]") +
                                         " FROM [dbo].[KURSIYER_ODEME_HAREKET] WHERE [" + kursiyerKolonu + "]=@ID " +
                                         (odemeCols.Contains("ID") ? "ORDER BY [ID] DESC" : string.Empty);
                        using (var cmdOzet2 = new SqlCommand(ozetSql, conn))
                        {
                            cmdOzet2.Parameters.AddWithValue("@ID", kursiyerId);
                            using (var r2 = await cmdOzet2.ExecuteReaderAsync())
                            {
                                if (await r2.ReadAsync())
                                {
                                    hareketToplam = r2["TOPLAM"] == DBNull.Value ? 0m : Convert.ToDecimal(r2["TOPLAM"]);
                                    hareketKalan = r2["KALAN"] == DBNull.Value ? 0m : Convert.ToDecimal(r2["KALAN"]);
                                }
                            }
                        }
                    }

                    // Toplam borc, odeme kartindaki txtToplamBorc ile bire bir olmasi icin
                    // once KURSIYER kaydindan gelir; sadece bos/0 ise hareket tablosuna dus.
                    if (toplamBorc <= 0m && hareketToplam > 0m)
                        toplamBorc = hareketToplam;
                    if (kalanBorc <= 0m && hareketKalan >= 0m)
                        kalanBorc = hareketKalan;
                    if (kalanBorc <= 0m && toplamBorc > 0m)
                        kalanBorc = toplamBorc;

                    if (Lbl_KalanBorc != null)
                        Lbl_KalanBorc.Text = "KALAN BORC: " + FormatTl(kalanBorc);
                    if (Lbl_ToplamBorc != null)
                        Lbl_ToplamBorc.Text = "TOPLAM BORC: " + FormatTl(toplamBorc);
                }
            }
            catch
            {
                // Odeme ozeti/acik satirlar yuklenemiyorsa sessiz gec.
            }

            if (groupBox1 != null && string.IsNullOrWhiteSpace(groupBox1.Text))
                groupBox1.Text = "Odeme Bilgisi";
            Dvg_Odeme_Bilgi.DataSource = dt;
        }

        private static string FormatTl(decimal tutar)
        {
            var tr = CultureInfo.GetCultureInfo("tr-TR");
            return tutar.ToString("#,##0.00", tr) + " TL";
        }

        private static decimal ToDecimalSafe(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0m;

            if (value is decimal dec)
                return dec;

            string text = Convert.ToString(value) ?? string.Empty;
            text = text.Replace("TL", "").Replace("₺", "").Trim();

            decimal parsed;
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out parsed))
                return parsed;
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
                return parsed;
            return 0m;
        }

        private async Task<int> SaveKursiyerDirectAsync(Kursiyer_Model model)
        {
            bool isNew = model == null || model.ID <= 0;
            string sql = isNew
                ? @"
INSERT INTO KURSIYER
(
    ADI, SOYADI, TC_NO, KIMLIK_KAYIT_NO, GSM_1, GSM_2, EV_TELEFON,
    KIMLIK_BABA_ADI, KIM_ANA_ADI, KIMLIK_DOGUM_YERI, EV_ADRESI, ADAY_NO,
    ON_NOTLAR, DOGUM_TARIHI, KAYIT_TARIHI, RESIM, ID_GRUP_KARTI,
    SERTIFIKA_SINIFI, ONCE_SERT_SINIFI, ONCE_SERT_BELGESAYI, KURSIYER_DURUMU
)
OUTPUT INSERTED.ID
VALUES
(
    @ADI, @SOYADI, @TC_NO, @KIMLIK_KAYIT_NO, @GSM_1, @GSM_2, @EV_TELEFON,
    @KIMLIK_BABA_ADI, @KIM_ANA_ADI, @KIMLIK_DOGUM_YERI, @EV_ADRESI, @ADAY_NO,
    @ON_NOTLAR, @DOGUM_TARIHI, @KAYIT_TARIHI, @RESIM, @ID_GRUP_KARTI,
    @SERTIFIKA_SINIFI, @ONCE_SERT_SINIFI, @ONCE_SERT_BELGESAYI, @KURSIYER_DURUMU
);"
                : @"
UPDATE KURSIYER
SET
    ADI=@ADI,
    SOYADI=@SOYADI,
    TC_NO=@TC_NO,
    KIMLIK_KAYIT_NO=@KIMLIK_KAYIT_NO,
    GSM_1=@GSM_1,
    GSM_2=@GSM_2,
    EV_TELEFON=@EV_TELEFON,
    KIMLIK_BABA_ADI=@KIMLIK_BABA_ADI,
    KIM_ANA_ADI=@KIM_ANA_ADI,
    KIMLIK_DOGUM_YERI=@KIMLIK_DOGUM_YERI,
    EV_ADRESI=@EV_ADRESI,
    ADAY_NO=@ADAY_NO,
    ON_NOTLAR=@ON_NOTLAR,
    DOGUM_TARIHI=@DOGUM_TARIHI,
    KAYIT_TARIHI=@KAYIT_TARIHI,
    RESIM=@RESIM,
    ID_GRUP_KARTI=@ID_GRUP_KARTI,
    SERTIFIKA_SINIFI=@SERTIFIKA_SINIFI,
    ONCE_SERT_SINIFI=@ONCE_SERT_SINIFI,
    ONCE_SERT_BELGESAYI=@ONCE_SERT_BELGESAYI,
    KURSIYER_DURUMU=@KURSIYER_DURUMU
WHERE ID=@ID;
SELECT @ID;";

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ADI", (object)model.ADI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SOYADI", (object)model.SOYADI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@TC_NO", (object)model.TC_NO ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KIMLIK_KAYIT_NO", (object)model.KIM_KAYIT_NO ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GSM_1", (object)model.GSM_1 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GSM_2", (object)model.GSM_2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EV_TELEFON", (object)model.EV_TELEFON ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KIMLIK_BABA_ADI", (object)model.KIM_BABA_ADI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KIM_ANA_ADI", (object)model.KIM_ANA_ADI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KIMLIK_DOGUM_YERI", (object)model.KIM_DOGUM_YERI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EV_ADRESI", (object)model.EV_ADRESI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ADAY_NO", (object)model.ADAY_NO ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ON_NOTLAR", (object)model.SARI_NOTLAR ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DOGUM_TARIHI", (object)model.DOGUM_TARIHI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KAYIT_TARIHI", (object)model.KAYIT_TARIHI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RESIM", (object)model.RESIM ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID_GRUP_KARTI", (object)model.ID_GRUP_KARTI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SERTIFIKA_SINIFI", (object)model.SERTIFIKA_SINIFI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ONCE_SERT_SINIFI", (object)model.ONCE_SERT_SINIFI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ONCE_SERT_BELGESAYI", (object)model.ONCE_SERT_BELGESAYI ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@KURSIYER_DURUMU", (object)model.KURSIYER_DURUMU ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ID", (object)model.ID ?? DBNull.Value);

                await conn.OpenAsync();
                object idObj = await cmd.ExecuteScalarAsync();
                int id;
                return int.TryParse(Convert.ToString(idObj), out id) ? id : 0;
            }
        }

        private async Task LoadWebcamImageAsync()
        {
            if (_model == null || _model.ID <= 0 || string.IsNullOrWhiteSpace(_cs))
                return;

            const string sql = "SELECT RESIM_WEBCAM FROM KURSIYER WHERE ID=@ID";
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", _model.ID);
                await conn.OpenAsync();
                object o = await cmd.ExecuteScalarAsync();
                if (o == null || o == DBNull.Value)
                    return;

                _webcamImageBytes = o as byte[];
                if (_webcamImageBytes == null || _webcamImageBytes.Length == 0)
                    return;

                using (var ms = new MemoryStream(_webcamImageBytes))
                using (var img = Image.FromStream(ms))
                {
                    Tnk_Webcamre.Image = new Bitmap(img);
                    Tnk_Webcamre.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        private async Task SaveWebcamImageIfAnyAsync(int kursiyerId)
        {
            if (kursiyerId <= 0 || _webcamImageBytes == null || _webcamImageBytes.Length == 0)
                return;

            const string sql = "UPDATE KURSIYER SET RESIM_WEBCAM=@RESIM_WEBCAM WHERE ID=@ID";
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", kursiyerId);
                var p = cmd.Parameters.Add("@RESIM_WEBCAM", SqlDbType.VarBinary, -1);
                p.Value = _webcamImageBytes;
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task LoadCinsiyetAsync()
        {
            if (_model == null || _model.ID <= 0 || string.IsNullOrWhiteSpace(_cs))
                return;

            const string sql = "SELECT CINSIYET FROM KURSIYER WHERE ID=@ID";
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", _model.ID);
                await conn.OpenAsync();
                object o = await cmd.ExecuteScalarAsync();
                if (o == null || o == DBNull.Value)
                    return;
                Tnk_cinsiyet.Text = Convert.ToString(o);
                CinsiyetArkaPlaniniUygula();
            }
        }

        private void Tnk_cinsiyet_SelectedIndexChanged(object sender, EventArgs e)
        {
            CinsiyetArkaPlaniniUygula();
        }

        private void CinsiyetArkaPlaniniUygula()
        {
            string cinsiyet = (Tnk_cinsiyet.Text ?? string.Empty).Trim().ToUpperInvariant();
            Tnk_cinsiyet.BackColor = cinsiyet == "KADIN" ? Color.LightPink : SystemColors.Window;
            BackColor = cinsiyet == "KADIN" ? Color.MistyRose : SystemColors.GradientInactiveCaption;
        }

        private async Task SaveCinsiyetAsync(int kursiyerId)
        {
            if (kursiyerId <= 0 || string.IsNullOrWhiteSpace(_cs))
                return;

            const string sql = "UPDATE KURSIYER SET CINSIYET=@CINSIYET WHERE ID=@ID";
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", kursiyerId);
                cmd.Parameters.AddWithValue("@CINSIYET", string.IsNullOrWhiteSpace(Tnk_cinsiyet.Text) ? (object)DBNull.Value : Tnk_cinsiyet.Text.Trim());
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private void Btn_SMS_Gonder_Click(object sender, EventArgs e)
        {
            if (_model == null)
            {
                MessageBox.Show("Kursiyer bulunamadı!");
                return;
            }

            string gsm = (_model.GSM_1 ?? "").Trim();

            if (string.IsNullOrWhiteSpace(gsm))
            {
                MessageBox.Show("Kursiyerin GSM numarası yok!");
                return;
            }

            string adSoyad = $"{_model.ADI ?? ""} {_model.SOYADI ?? ""}".Trim();
            string kursAdi = KursAdiRaporIcinOku();

            var onizleme = new SmsSablonOnizlemeVerisi
            {
                AdSoyad = adSoyad,
                Telefon = gsm,
                KursAdi = kursAdi,
                Tarih = DateTime.Today,
                Saat = DateTime.Now.ToString("HH:mm")
            };

            const string varsayilanSablon =
                "SAYIN [AD SOYAD]; DIREKSIYON SINAV BILGILERI ICIN [KURS ADI] KURSUMUZU ARAYINIZ. Tel: [TELEFON]";

            string mesaj;
            using (var sablonForm = new SmsSablonDuzenleForm(varsayilanSablon, string.Empty, onizleme, _cs))
            {
                if (sablonForm.ShowDialog(this) != DialogResult.OK)
                    return;
                mesaj = (sablonForm.TemplateText ?? string.Empty).Trim();
            }

            if (string.IsNullOrWhiteSpace(mesaj))
            {
                MessageBox.Show("SMS metni bos olamaz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var frm = new Frm_SMS_Gonder();
            frm.KursiyerYukle(gsm, adSoyad, mesaj);
            frm.Show();
        }

        private string KursAdiRaporIcinOku()
        {
            if (string.IsNullOrWhiteSpace(_cs))
                return string.Empty;
            try
            {
                var k = KursRaporKursTablosu.Olustur(_cs);
                if (k.Rows.Count > 0 && k.Columns.Contains("KURS_ADI"))
                    return Convert.ToString(k.Rows[0]["KURS_ADI"]).Trim();
            }
            catch
            {
                // yoksay
            }
            return string.Empty;
        }

        private void Tnk_TC_NO_Validating(object sender, CancelEventArgs e)
        {
            if (TcKimlikValidator.TryExplainProblem(Tnk_TC_NO.Text, out string uyari))
            {
                MessageBox.Show(uyari, "TC Kimlik No", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = false;
            }
        }

        private void KursiyerDetay_Sayfam_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            if (ActiveControl is TextBox tb && tb.Multiline)
                return;

            e.SuppressKeyPress = true;
            e.Handled = true;
            SelectNextControl(ActiveControl, true, true, true, true);
        }

        private static void ApplyUppercaseInputs(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c is TextBox t)
                    t.CharacterCasing = CharacterCasing.Upper;

                if (c.HasChildren)
                    ApplyUppercaseInputs(c);
            }
        }

        private static bool IsUnderEighteen(DateTime dogumTarihi, DateTime referansTarih)
        {
            DateTime onSekiz = dogumTarihi.Date.AddYears(18);
            return referansTarih.Date < onSekiz;
        }

        private async void Btn_TC_Dogrula_Click(object sender, EventArgs e)
        {
            string tcRaw = Tnk_TC_NO.Text ?? string.Empty;
            string tcText = new string(tcRaw.Where(char.IsDigit).ToArray());

            if (TcKimlikValidator.TryExplainProblem(tcRaw, out string tcUyari))
            {
                MessageBox.Show(tcUyari, "TC Kimlik No", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!ulong.TryParse(tcText, out ulong tcNo))
            {
                MessageBox.Show("TC Kimlik No gecersiz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tr = new CultureInfo("tr-TR");
            var ad = (Tnk_ADI.Text ?? string.Empty).Trim().ToUpper(tr);
            var soyad = (Tnk_SOYADI.Text ?? string.Empty).Trim().ToUpper(tr);

            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad))
            {
                MessageBox.Show("Ad ve Soyad bos olamaz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int dogumYili = Dtp_DOGUM_TARIHI.Value.Year;

            try
            {
                bool dogrulandi = await TcKimlikDogrulaAsync(tcNo, ad, soyad, dogumYili);
                if (dogrulandi)
                {
                    MessageBox.Show("Kimlik dogrulama BASARILI.", "Sonuc", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Kimlik dogrulama BASARISIZ.\nLutfen Ad, Soyad ve Dogum Yili bilgilerinin MERNIS kaydi ile birebir ayni oldugunu kontrol edin.",
                        "Sonuc",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kimlik dogrulama servisine baglanilamadi.\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<bool> TcKimlikDogrulaAsync(ulong tcKimlikNo, string ad, string soyad, int dogumYili)
        {
            XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace wsNs = "http://tckimlik.nvi.gov.tr/WS";
            const string soapAction = "http://tckimlik.nvi.gov.tr/WS/TCKimlikNoDogrula";

            var requestDoc = new XDocument(
                new XElement(soapNs + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soap", soapNs),
                    new XAttribute(XNamespace.Xmlns + "ws", wsNs),
                    new XElement(soapNs + "Body",
                        new XElement(wsNs + "TCKimlikNoDogrula",
                            new XElement(wsNs + "TCKimlikNo", tcKimlikNo),
                            new XElement(wsNs + "Ad", ad),
                            new XElement(wsNs + "Soyad", soyad),
                            new XElement(wsNs + "DogumYili", dogumYili)
                        )
                    )
                )
            );

            // NVI servisi TLS 1.2 gerektirebildigi icin baglanti protokollerini acikca ayarliyoruz.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
            ServicePointManager.Expect100Continue = false;

            string soapXml = requestDoc.Declaration == null
                ? "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + requestDoc
                : requestDoc.ToString();

            string[] endpoints = { KimlikDogrulamaServisUrl, KimlikDogrulamaServisUrlV2 };
            Exception lastException = null;

            foreach (string endpoint in endpoints)
            {
                // 1) SOAP 1.1
                try
                {
                    using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                    using (var content = new StringContent(soapXml, System.Text.Encoding.UTF8, "text/xml"))
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Kolera-MTSK/1.0");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("SOAPAction", soapAction);
                        content.Headers.Remove("Content-Type");
                        content.Headers.TryAddWithoutValidation("Content-Type", "text/xml; charset=utf-8");

                        using (var response = await client.PostAsync(endpoint, content))
                        {
                            string responseText = await response.Content.ReadAsStringAsync();
                            return TcDogrulamaSonucunuAyikla(responseText, wsNs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                // 2) SOAP 1.2
                try
                {
                    using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                    using (var content = new StringContent(soapXml, System.Text.Encoding.UTF8))
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Kolera-MTSK/1.0");
                        content.Headers.Remove("Content-Type");
                        content.Headers.TryAddWithoutValidation(
                            "Content-Type",
                            "application/soap+xml; charset=utf-8; action=\"" + soapAction + "\"");

                        using (var response = await client.PostAsync(endpoint, content))
                        {
                            string responseText = await response.Content.ReadAsStringAsync();
                            return TcDogrulamaSonucunuAyikla(responseText, wsNs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw new Exception("Kimlik doğrulama servisi ile iletişim kurulamadı.", lastException);
        }

        private static bool TcDogrulamaSonucunuAyikla(string responseText, XNamespace wsNs)
        {
            if (string.IsNullOrWhiteSpace(responseText))
                return false;

            // XML parser'in kabul etmedigi HTML entity'lerini normalize et.
            string normalized = responseText.Replace("&nbsp;", " ");

            try
            {
                var responseDoc = XDocument.Parse(normalized);
                var faultString = responseDoc
                    .Descendants()
                    .FirstOrDefault(x => x.Name.LocalName.Equals("faultstring", StringComparison.OrdinalIgnoreCase))
                    ?.Value;
                if (!string.IsNullOrWhiteSpace(faultString))
                    throw new Exception("Kimlik doğrulama servisi hata döndürdü: " + faultString);

                var resultElement = responseDoc.Descendants(wsNs + "TCKimlikNoDogrulaResult").FirstOrDefault();
                if (resultElement != null)
                    return string.Equals(resultElement.Value, "true", StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Kimlik doğrulama servisi hata döndürdü:", StringComparison.Ordinal))
                    throw;
                // XML parse edilemezse asagidaki metin tabanli yonteme dusecek.
            }

            var match = Regex.Match(
                normalized,
                @"<\s*(?:\w+:)?TCKimlikNoDogrulaResult\s*>\s*(true|false)\s*<\s*/\s*(?:\w+:)?TCKimlikNoDogrulaResult\s*>",
                RegexOptions.IgnoreCase);

            if (match.Success)
                return string.Equals(match.Groups[1].Value, "true", StringComparison.OrdinalIgnoreCase);

            string preview = normalized.Length > 260 ? normalized.Substring(0, 260) + "..." : normalized;
            throw new Exception("Kimlik doğrulama servisi tanınmayan bir yanıt döndürdü: " + preview);
        }

    }

}


