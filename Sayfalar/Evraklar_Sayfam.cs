using Kolera.Evrak.Models;
using Kolera.Evrak.Services;
using Kolera_Kursiyer;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using static Kolera_Mtsk.Sayfalar.Tarama_Sayfam;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Evraklar_Sayfam : Form
    {
        private readonly Kursiyer_Model _kursiyerModel;
        private readonly KursiyerEvrakService _evrakService;
        private readonly string _connectionString;

        private byte[] _imgOgr = null;
        private byte[] _imgSaglik = null;
        private byte[] _imgSavcilik = null;
        private byte[] _imgImza = null;
        private byte[] _imgSozlesme_On = null;
        private byte[] _imgSozlesme_Arka = null;

        // WinForms Designer formu parametresiz olusturur.
        public Evraklar_Sayfam()
            : this(null, null)
        {
        }

        public Evraklar_Sayfam(Kursiyer_Model kursiyer, KursiyerEvrakService evrakService, string connectionString = null)
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _kursiyerModel = kursiyer;
            _evrakService = evrakService;
            _connectionString = connectionString;

            this.Load += Evraklar_Sayfam_Load;

            // 🔹 Buton olayları
            Btn_1.Click += (s, e) => LoadImageToPictureBox(_imgOgr, Pic_Belgem);
            Btn_2.Click += (s, e) => LoadImageToPictureBox(_imgSaglik, Pic_Belgem);
            Btn_3.Click += (s, e) => LoadImageToPictureBox(_imgSavcilik, Pic_Belgem);
            Btn_4.Click += (s, e) => LoadImageToPictureBox(_imgImza, Pic_Belgem);
            Btn_Soz_On.Click += (s, e) => LoadImageToPictureBox(_imgSozlesme_On, Pic_Belgem);
            Btn_Soz_Arka.Click += (s, e) => LoadImageToPictureBox(_imgSozlesme_Arka, Pic_Belgem);

            // 🔹 Tarama butonları
            Btn_Tara_Ogrenim.Click += (s, e) => OpenTarama(1);
            Btn_Tara_Saglik.Click += (s, e) => OpenTarama(2);
            Btn_Tara_Savcilik.Click += (s, e) => OpenTarama(3);
            Btn_Tara_Imza.Click += (s, e) => OpenTarama(4);
            Btn_Tara_SozlesmeOn.Click += (s, e) => OpenTarama(5);
            Btn_Tara_SozlesmeArka.Click += (s, e) => OpenTarama(6);

            // 🔹 Kaydet
            Btn_Kaydet1.Click += Btn_Kaydet1_Click;
        }

        private void Evraklar_Sayfam_Load(object sender, EventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            if (_kursiyerModel == null)
            {
                MessageBox.Show("Kursiyer bilgisi alınamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            // 🔹 Ad-Soyad
            Lbl_Adsoyad.Text = (_kursiyerModel.ADI + " " + _kursiyerModel.SOYADI).ToUpper();

            // 🔹 Profil resmi
            if (_kursiyerModel.RESIM != null && _kursiyerModel.RESIM.Length > 0)
            {
                using (var ms = new MemoryStream(_kursiyerModel.RESIM))
                {
                    Tnk_RESIM_Kursiyer.Image = Image.FromStream(ms);
                }
            }

            // 🔹 Kursiyer evrakları
            KursiyerEvrak_Model evrak = null;
            try
            {
                evrak = GetKursiyerEvrakDirect(_kursiyerModel.ID);
            }
            catch
            {
                evrak = null;
            }

            if (evrak == null)
            {
                try
                {
                    evrak = _evrakService.GetKursiyerEvrak(_kursiyerModel.ID);
                }
                catch
                {
                    evrak = null;
                }
            }

            if (evrak == null) return;

            // 🔹 Text alanlar
            Cmb_OGR_BEL_TURU.Text = evrak.OgrBelgeTuru ?? "";
            Txt_OGR_BEL_VEREN_KURUM.Text = evrak.OgrBelgeVerenKurum ?? "";
            Txt_OGR_BEL_SAYISI.Text = evrak.OgrBelgeSayisi ?? "";

            Txt_SAG_RAPOR_BELGENO.Text = evrak.SaglikBelgeNo ?? "";
            Txt_SAG_RAPOR_VEREN_KURUM.Text = evrak.SaglikBelverenKurum ?? "";
            Txt_SAG_RAPOR_REFERANS.Text = evrak.SaglikBelReferans ?? "";

            Txt_SAVCILIK_BEL_NO.Text = evrak.SavcilikBelgeNo ?? "";
            Cmb_SAVCILIK_BEL_VEREN_KURUM.Text = evrak.SavcilikBelgeVerenKurum ?? "";

            // 🔹 Resimler
            _imgOgr = evrak.ImgOgrBel;
            _imgSaglik = evrak.ImgSaglik;
            _imgSavcilik = evrak.ImgSavcilik;
            _imgImza = evrak.ImgImza;
            _imgSozlesme_On = evrak.ImgSozlesme_On;
            _imgSozlesme_Arka = evrak.ImgSozlesme_Arka;

            // 🔹 Checkbox'ları görsel varlığına göre doldur
            Chk_1.Checked = HasImage(_imgOgr);
            Chk_2.Checked = HasImage(_imgSaglik);
            Chk_3.Checked = HasImage(_imgSavcilik);
            Chk_4.Checked = HasImage(_imgSozlesme_On) || HasImage(_imgSozlesme_Arka);
            Chk_6.Checked = HasImage(_imgImza);

            // 🔹 Tarihler
            SetDate(Tnk_OGRNM_BELGE_TARIHI, evrak.OgrBelgeTarihi);
            SetDate(Tnk_SAG_RAPOR_TARIHI, evrak.SaglikBelgeTarihi);
            SetDate(Tnk_SAVCILIK_BEL_TARIHI, evrak.SavcilikBelgeTarihi);
            
        }

        private void SetDate(DateTimePicker picker, DateTime? value)
        {
            picker.ShowCheckBox = true;
            if (value.HasValue && value.Value >= picker.MinDate && value.Value <= picker.MaxDate)
            {
                picker.Value = value.Value;
                picker.Checked = true;
            }
            else
            {
                picker.Value = DateTime.Today;
                picker.Checked = false;
            }
        }

        private void LoadImageToPictureBox(byte[] data, PictureBox box)
        {
            if (data != null && data.Length > 0)
            {
                using (var ms = new MemoryStream(data))
                {
                    box.Image = Image.FromStream(ms);
                }
            }
            else
            {
                box.Image = null;
            }
        }

        private static bool HasImage(byte[] data)
        {
            return data != null && data.Length > 0;
        }

        private KursiyerEvrak_Model GetKursiyerEvrakDirect(int kursiyerId)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return null;

            var dt = new DataTable();
            string tableName;
            string idColumnName;

            if (!TryResolveEvrakTableAndIdColumn(out tableName, out idColumnName))
                return null;

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT TOP 1 * FROM [" + tableName + "] WHERE [" + idColumnName + "]=@ID", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@ID", kursiyerId);
                con.Open();
                da.Fill(dt);
            }

            if (dt.Rows.Count == 0)
                return null;

            var row = dt.Rows[0];
            return new KursiyerEvrak_Model
            {
                ID_Kursiyer = kursiyerId,
                EkskOgrBel = GetBool(row, "EKSIK_OGRNIM_BEL", "EKSK_OGRNIM_BEL"),
                EkskSaglik = GetBool(row, "EKSIK_SAGLIK", "EKSK_SAGLIK"),
                EkskSavcilik = GetBool(row, "EKSIK_SAVCILIK", "EKSK_SAVCILIK"),
                EkskSozlesme = GetBool(row, "EKSIK_SOZLESME", "EKSK_SOZLESME"),
                EkskImza = GetBool(row, "EKSIK_IMZA", "EKSK_IMZA"),
                EkskWepcam = GetBool(row, "EKSIK_WEPCAM", "EKSK_WEPCAM"),

                OgrBelgeTuru = GetString(row, "OGRNM_BEL_TURU", "OGR_BEL_TURU"),
                OgrBelgeVerenKurum = GetString(row, "OGRNM_BEL_VEREN_KURUM", "OGR_BEL_VEREN_KURUM"),
                OgrBelgeSayisi = GetString(row, "OGRNM_BEL_SAYISI", "OGR_BEL_SAYISI"),
                OgrBelgeTarihi = GetDate(row, "OGRNM_BEL_TARIHI", "OGR_BEL_TARIHI"),

                SaglikBelverenKurum = GetString(row, "SAG_RAP_VEREN_KURUM", "SAG_RAPOR_VEREN_KURUM"),
                SaglikBelgeNo = GetString(row, "SAG_RAP_BELGENO", "SAG_RAPOR_BELGENO"),
                SaglikBelgeTarihi = GetDate(row, "SAG_RAP_TARIHI", "SAG_RAPOR_TARIHI"),
                SaglikBelReferans = GetString(row, "SAG_RAPOR_REFERANS"),
                SaglikBelHeskodu = GetString(row, "SAG_RAPOR_HESKODU"),

                SavcilikBelgeNo = GetString(row, "CriminalNo", "SAVCILIK_BEL_NO"),
                SavcilikBelgeVerenKurum = GetString(row, "SAVCILIK_BEL_VEREN_KURUM"),
                SavcilikBelgeTarihi = GetDate(row, "SAVCILIK_BEL_TARIHI"),

                OzurDurumu = GetString(row, "OZUR_DURUMU"),
                YabanciDil = GetString(row, "YABANCI_DIL"),
                FaturaNo = GetString(row, "FATURA_NO"),
                FaturaTarihi = GetDate(row, "FATURA_TARIHI"),

                ImgOgrBel = GetBytes(row, "RES_OGRNIM_BEL", "IMG_OGRNIM_BEL"),
                ImgSaglik = GetBytes(row, "RES_SAGLIK", "IMG_SAGLIK"),
                ImgSavcilik = GetBytes(row, "RES_SAVCILIK", "IMG_SAVCILIK"),
                ImgSozlesme_On = GetBytes(row, "RES_SOZLESME_ON", "IMG_SOZLESME_ON"),
                ImgSozlesme_Arka = GetBytes(row, "RES_SOZLESME_ARKA", "IMG_SOZLESME_ARKA"),
                ImgImza = GetBytes(row, "RES_IMZA", "IMG_IMZA")
            };
        }

        private bool TryResolveEvrakTableAndIdColumn(out string tableName, out string idColumnName)
        {
            tableName = null;
            idColumnName = null;

            const string sql = @"
SELECT TABLE_NAME, COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE
    UPPER(TABLE_NAME) LIKE '%EVRAK%'
    AND UPPER(COLUMN_NAME) IN ('ID_KURSIYER', 'IDKURSIYER', 'KURSIYER_ID', 'ID_KURSIYERLER')
ORDER BY
    CASE WHEN UPPER(TABLE_NAME) = 'KURSIYER_EVRAK' THEN 0 ELSE 1 END,
    TABLE_NAME;";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                        return false;

                    tableName = dr["TABLE_NAME"] == DBNull.Value ? null : dr["TABLE_NAME"].ToString();
                    idColumnName = dr["COLUMN_NAME"] == DBNull.Value ? null : dr["COLUMN_NAME"].ToString();
                }
            }

            return !string.IsNullOrWhiteSpace(tableName) && !string.IsNullOrWhiteSpace(idColumnName);
        }

        private static string ResolveColumnName(DataRow row, params string[] candidates)
        {
            return candidates.FirstOrDefault(c => row.Table.Columns.Contains(c));
        }

        private static string GetString(DataRow row, params string[] candidates)
        {
            var col = ResolveColumnName(row, candidates);
            if (string.IsNullOrWhiteSpace(col) || row[col] == DBNull.Value)
                return null;
            return Convert.ToString(row[col]);
        }

        private static bool GetBool(DataRow row, params string[] candidates)
        {
            var col = ResolveColumnName(row, candidates);
            if (string.IsNullOrWhiteSpace(col) || row[col] == DBNull.Value)
                return false;

            object raw = row[col];
            if (raw is bool)
                return (bool)raw;

            var text = Convert.ToString(raw)?.Trim();
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (text == "1")
                return true;
            if (text == "0")
                return false;

            bool parsedBool;
            if (bool.TryParse(text, out parsedBool))
                return parsedBool;

            int parsedInt;
            if (int.TryParse(text, out parsedInt))
                return parsedInt != 0;

            return false;
        }

        private static DateTime? GetDate(DataRow row, params string[] candidates)
        {
            var col = ResolveColumnName(row, candidates);
            if (string.IsNullOrWhiteSpace(col) || row[col] == DBNull.Value)
                return null;
            return Convert.ToDateTime(row[col]);
        }

        private static byte[] GetBytes(DataRow row, params string[] candidates)
        {
            var col = ResolveColumnName(row, candidates);
            if (string.IsNullOrWhiteSpace(col) || row[col] == DBNull.Value)
                return null;
            return row[col] as byte[];
        }

        private void UpsertKursiyerEvrakDirect(KursiyerEvrak_Model evrak)
        {
            if (evrak == null)
                return;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _evrakService.UpsertKursiyerEvrak(evrak);
                return;
            }

            const string sql = @"
IF EXISTS (SELECT 1 FROM KURSIYER_EVRAK WHERE ID_KURSIYER=@ID_KURSIYER)
BEGIN
    UPDATE KURSIYER_EVRAK
    SET
        EKSIK_OGRNIM_BEL=@EKSIK_OGRNIM_BEL,
        EKSIK_SAGLIK=@EKSIK_SAGLIK,
        EKSIK_SAVCILIK=@EKSIK_SAVCILIK,
        EKSIK_SOZLESME=@EKSIK_SOZLESME,
        EKSIK_IMZA=@EKSIK_IMZA,
        EKSIK_WEPCAM=@EKSIK_WEPCAM,
        OGRNM_BEL_TURU=@OGRNM_BEL_TURU,
        OGRNM_BEL_VEREN_KURUM=@OGRNM_BEL_VEREN_KURUM,
        OGRNM_BEL_TARIHI=@OGRNM_BEL_TARIHI,
        OGRNM_BEL_SAYISI=@OGRNM_BEL_SAYISI,
        SAG_RAP_VEREN_KURUM=@SAG_RAP_VEREN_KURUM,
        SAG_RAP_TARIHI=@SAG_RAP_TARIHI,
        SAG_RAP_BELGENO=@SAG_RAP_BELGENO,
        SAG_RAPOR_REFERANS=@SAG_RAPOR_REFERANS,
        SAG_RAPOR_HESKODU=@SAG_RAPOR_HESKODU,
        CriminalNo=@CriminalNo,
        SAVCILIK_BEL_VEREN_KURUM=@SAVCILIK_BEL_VEREN_KURUM,
        SAVCILIK_BEL_TARIHI=@SAVCILIK_BEL_TARIHI,
        OZUR_DURUMU=@OZUR_DURUMU,
        YABANCI_DIL=@YABANCI_DIL,
        RES_OGRNIM_BEL=@RES_OGRNIM_BEL,
        RES_SAGLIK=@RES_SAGLIK,
        RES_SAVCILIK=@RES_SAVCILIK,
        RES_SOZLESME_ON=@RES_SOZLESME_ON,
        RES_SOZLESME_ARKA=@RES_SOZLESME_ARKA,
        RES_IMZA=@RES_IMZA
    WHERE ID_KURSIYER=@ID_KURSIYER;
END
ELSE
BEGIN
    INSERT INTO KURSIYER_EVRAK
    (
        ID_KURSIYER, EKSIK_OGRNIM_BEL, EKSIK_SAGLIK, EKSIK_SAVCILIK, EKSIK_SOZLESME, EKSIK_IMZA, EKSIK_WEPCAM,
        OGRNM_BEL_TURU, OGRNM_BEL_VEREN_KURUM, OGRNM_BEL_TARIHI, OGRNM_BEL_SAYISI,
        SAG_RAP_VEREN_KURUM, SAG_RAP_TARIHI, SAG_RAP_BELGENO, SAG_RAPOR_REFERANS, SAG_RAPOR_HESKODU,
        CriminalNo, SAVCILIK_BEL_VEREN_KURUM, SAVCILIK_BEL_TARIHI, OZUR_DURUMU, YABANCI_DIL,
        RES_OGRNIM_BEL, RES_SAGLIK, RES_SAVCILIK, RES_SOZLESME_ON, RES_SOZLESME_ARKA, RES_IMZA
    )
    VALUES
    (
        @ID_KURSIYER, @EKSIK_OGRNIM_BEL, @EKSIK_SAGLIK, @EKSIK_SAVCILIK, @EKSIK_SOZLESME, @EKSIK_IMZA, @EKSIK_WEPCAM,
        @OGRNM_BEL_TURU, @OGRNM_BEL_VEREN_KURUM, @OGRNM_BEL_TARIHI, @OGRNM_BEL_SAYISI,
        @SAG_RAP_VEREN_KURUM, @SAG_RAP_TARIHI, @SAG_RAP_BELGENO, @SAG_RAPOR_REFERANS, @SAG_RAPOR_HESKODU,
        @CriminalNo, @SAVCILIK_BEL_VEREN_KURUM, @SAVCILIK_BEL_TARIHI, @OZUR_DURUMU, @YABANCI_DIL,
        @RES_OGRNIM_BEL, @RES_SAGLIK, @RES_SAVCILIK, @RES_SOZLESME_ON, @RES_SOZLESME_ARKA, @RES_IMZA
    );
END;";

            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@ID_KURSIYER", SqlDbType.Int).Value = evrak.ID_Kursiyer;
                cmd.Parameters.Add("@EKSIK_OGRNIM_BEL", SqlDbType.NVarChar, 1).Value = evrak.EkskOgrBel ? "1" : "0";
                cmd.Parameters.Add("@EKSIK_SAGLIK", SqlDbType.NVarChar, 1).Value = evrak.EkskSaglik ? "1" : "0";
                cmd.Parameters.Add("@EKSIK_SAVCILIK", SqlDbType.NVarChar, 1).Value = evrak.EkskSavcilik ? "1" : "0";
                cmd.Parameters.Add("@EKSIK_SOZLESME", SqlDbType.NVarChar, 1).Value = evrak.EkskSozlesme ? "1" : "0";
                cmd.Parameters.Add("@EKSIK_IMZA", SqlDbType.NVarChar, 1).Value = evrak.EkskImza ? "1" : "0";
                cmd.Parameters.Add("@EKSIK_WEPCAM", SqlDbType.NVarChar, 1).Value = evrak.EkskWepcam ? "1" : "0";
                cmd.Parameters.Add("@OGRNM_BEL_TURU", SqlDbType.NVarChar, 100).Value = (object)evrak.OgrBelgeTuru ?? DBNull.Value;
                cmd.Parameters.Add("@OGRNM_BEL_VEREN_KURUM", SqlDbType.NVarChar, 150).Value = (object)evrak.OgrBelgeVerenKurum ?? DBNull.Value;
                cmd.Parameters.Add("@OGRNM_BEL_TARIHI", SqlDbType.DateTime).Value = (object)evrak.OgrBelgeTarihi ?? DBNull.Value;
                cmd.Parameters.Add("@OGRNM_BEL_SAYISI", SqlDbType.NVarChar, 100).Value = (object)evrak.OgrBelgeSayisi ?? DBNull.Value;
                cmd.Parameters.Add("@SAG_RAP_VEREN_KURUM", SqlDbType.NVarChar, 150).Value = (object)evrak.SaglikBelverenKurum ?? DBNull.Value;
                cmd.Parameters.Add("@SAG_RAP_TARIHI", SqlDbType.DateTime).Value = (object)evrak.SaglikBelgeTarihi ?? DBNull.Value;
                cmd.Parameters.Add("@SAG_RAP_BELGENO", SqlDbType.NVarChar, 100).Value = (object)evrak.SaglikBelgeNo ?? DBNull.Value;
                cmd.Parameters.Add("@SAG_RAPOR_REFERANS", SqlDbType.NVarChar, 100).Value = (object)evrak.SaglikBelReferans ?? DBNull.Value;
                cmd.Parameters.Add("@SAG_RAPOR_HESKODU", SqlDbType.NVarChar, 100).Value = (object)evrak.SaglikBelHeskodu ?? DBNull.Value;
                cmd.Parameters.Add("@CriminalNo", SqlDbType.NVarChar, 100).Value = (object)evrak.SavcilikBelgeNo ?? DBNull.Value;
                cmd.Parameters.Add("@SAVCILIK_BEL_VEREN_KURUM", SqlDbType.NVarChar, 150).Value = (object)evrak.SavcilikBelgeVerenKurum ?? DBNull.Value;
                cmd.Parameters.Add("@SAVCILIK_BEL_TARIHI", SqlDbType.DateTime).Value = (object)evrak.SavcilikBelgeTarihi ?? DBNull.Value;
                cmd.Parameters.Add("@OZUR_DURUMU", SqlDbType.NVarChar, 100).Value = (object)evrak.OzurDurumu ?? DBNull.Value;
                cmd.Parameters.Add("@YABANCI_DIL", SqlDbType.NVarChar, 100).Value = (object)evrak.YabanciDil ?? DBNull.Value;
                cmd.Parameters.Add("@RES_OGRNIM_BEL", SqlDbType.VarBinary, -1).Value = (object)evrak.ImgOgrBel ?? DBNull.Value;
                cmd.Parameters.Add("@RES_SAGLIK", SqlDbType.VarBinary, -1).Value = (object)evrak.ImgSaglik ?? DBNull.Value;
                cmd.Parameters.Add("@RES_SAVCILIK", SqlDbType.VarBinary, -1).Value = (object)evrak.ImgSavcilik ?? DBNull.Value;
                cmd.Parameters.Add("@RES_SOZLESME_ON", SqlDbType.VarBinary, -1).Value = (object)evrak.ImgSozlesme_On ?? DBNull.Value;
                cmd.Parameters.Add("@RES_SOZLESME_ARKA", SqlDbType.VarBinary, -1).Value = (object)evrak.ImgSozlesme_Arka ?? DBNull.Value;
                cmd.Parameters.Add("@RES_IMZA", SqlDbType.VarBinary, -1).Value = (object)evrak.ImgImza ?? DBNull.Value;

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }



       
            private void Btn_Kaydet1_Click(object sender, EventArgs e)
        {
            try
            {
                // Kursiyer evrak modeli oluştur
                var evrak = new KursiyerEvrak_Model
                {
                    ID_Kursiyer = _kursiyerModel.ID,

                    // Boolean alanlar → resim var mı kontrolü ile
                    EkskOgrBel = _imgOgr != null && _imgOgr.Length > 0,
                    EkskSaglik = _imgSaglik != null && _imgSaglik.Length > 0,
                    EkskSavcilik = _imgSavcilik != null && _imgSavcilik.Length > 0,
                    EkskSozlesme = _imgSozlesme_On != null && _imgSozlesme_On.Length > 0,
                    EkskImza = _imgImza != null && _imgImza.Length > 0,
                    EkskWepcam = false, // Formda yok

                    // String alanlar
                    OgrBelgeTuru = string.IsNullOrWhiteSpace(Cmb_OGR_BEL_TURU.Text) ? null : Cmb_OGR_BEL_TURU.Text,
                    OgrBelgeVerenKurum = string.IsNullOrWhiteSpace(Txt_OGR_BEL_VEREN_KURUM.Text) ? null : Txt_OGR_BEL_VEREN_KURUM.Text,
                    OgrBelgeSayisi = string.IsNullOrWhiteSpace(Txt_OGR_BEL_SAYISI.Text) ? null : Txt_OGR_BEL_SAYISI.Text,

                    SaglikBelgeNo = string.IsNullOrWhiteSpace(Txt_SAG_RAPOR_BELGENO.Text) ? null : Txt_SAG_RAPOR_BELGENO.Text,
                    SaglikBelverenKurum = string.IsNullOrWhiteSpace(Txt_SAG_RAPOR_VEREN_KURUM.Text) ? null : Txt_SAG_RAPOR_VEREN_KURUM.Text,
                    SaglikBelReferans = string.IsNullOrWhiteSpace(Txt_SAG_RAPOR_REFERANS.Text) ? null : Txt_SAG_RAPOR_REFERANS.Text,
                    SaglikBelHeskodu = null,

                    SavcilikBelgeNo = string.IsNullOrWhiteSpace(Txt_SAVCILIK_BEL_NO.Text) ? null : Txt_SAVCILIK_BEL_NO.Text,
                    SavcilikBelgeVerenKurum = string.IsNullOrWhiteSpace(Cmb_SAVCILIK_BEL_VEREN_KURUM.Text) ? null : Cmb_SAVCILIK_BEL_VEREN_KURUM.Text,

                    OzurDurumu = string.IsNullOrWhiteSpace(Cmb_OZUR_DURUMU.Text) ? null : Cmb_OZUR_DURUMU.Text,
                    YabanciDil = string.IsNullOrWhiteSpace(CMD_Dil.Text) ? null : CMD_Dil.Text,
                    FaturaNo = null,

                    // Tarihler
                    OgrBelgeTarihi = Tnk_OGRNM_BELGE_TARIHI.Checked ? (DateTime?)Tnk_OGRNM_BELGE_TARIHI.Value : null,
                    SaglikBelgeTarihi = Tnk_SAG_RAPOR_TARIHI.Checked ? (DateTime?)Tnk_SAG_RAPOR_TARIHI.Value : null,
                    SavcilikBelgeTarihi = Tnk_SAVCILIK_BEL_TARIHI.Checked ? (DateTime?)Tnk_SAVCILIK_BEL_TARIHI.Value : null,
                    FaturaTarihi = null,
                    FaturaTutari = null,

                    // Resimler
                    ImgOgrBel = _imgOgr,
                    ImgSaglik = _imgSaglik,
                    ImgSavcilik = _imgSavcilik,
                    ImgImza = _imgImza,
                    ImgSozlesme_On = _imgSozlesme_On,
                    ImgSozlesme_Arka = _imgSozlesme_Arka
                };

                // SP ad uyuşmazlığından etkilenmemesi için doğrudan kaydet
                UpsertKursiyerEvrakDirect(evrak);

                MessageBox.Show("Evrak bilgileri kaydedildi ✅", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // 🔹 Tarama işlemi
        private void OpenTarama(int tip)
        {
            byte[] mevcutResim = null;
            var taramaTipi = TaramaTipi.KursiyerResmi;

            switch (tip)
            {
                case 1: mevcutResim = _imgOgr; taramaTipi = TaramaTipi.Evrak; break;
                case 2: mevcutResim = _imgSaglik; taramaTipi = TaramaTipi.Evrak; break;
                case 3: mevcutResim = _imgSavcilik; taramaTipi = TaramaTipi.Evrak; break;
                case 4: mevcutResim = _imgImza; taramaTipi = TaramaTipi.Imza; break;
                case 5: mevcutResim = _imgSozlesme_On; taramaTipi = TaramaTipi.Evrak; break;
                case 6: mevcutResim = _imgSozlesme_Arka; taramaTipi = TaramaTipi.Evrak; break;
            }

            using (var taramaForm = new Tarama_Sayfam(mevcutResim, taramaTipi, _connectionString)) // 🔹 Mevcut resmi gönder
            {
                taramaForm.TaramaTamamlandi += (byte[] data) =>
                {
                    switch (tip)
                    {
                        case 1: _imgOgr = data; break;
                        case 2: _imgSaglik = data; break;
                        case 3: _imgSavcilik = data; break;
                        case 4: _imgImza = data; break;
                        case 5: _imgSozlesme_On = data; break;
                        case 6: _imgSozlesme_Arka = data; break;
                    }

                    LoadImageToPictureBox(data, Pic_Belgem);
                };

                taramaForm.ShowDialog();
            }
        }

        private void Btn_Tara_Ogrenim_Click(object sender, EventArgs e)
        {

        }
    }
}
