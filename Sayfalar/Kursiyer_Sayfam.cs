using Kolera.Donem;
using Kolera_Kursiyer;
using Kolera_Kursiyer.Services;
using Kolera_Mtsk.Services;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Kursiyer_Sayfam : Form
    {
        private readonly KursiyerService _kursiyerServis;
        private readonly DonemService _donemServis;
        private readonly string _connectionString;
        private byte[] _webcamImageBytes;

        public Kursiyer_Sayfam() : this(string.Empty)
        {
        }

        public Kursiyer_Sayfam(string connectionString)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
            _kursiyerServis = new KursiyerService(connectionString);
            _donemServis = new DonemService(connectionString);

            Tnk_DOGUM_TARIHI.ShowCheckBox = false;
            Tnk_KAYIT_TARIHI.ShowCheckBox = false;

            Tnk_KAYIT_TARIHI.Value = DateTime.Now;

            Load += Kursiyer_Sayfam_Load;
            Btn_Webresimekle.Click += Btn_Webresimekle_Click;
            Btn_Rapor_Al.Click += Btn_Rapor_Al_Click;
            Tnk_TC_NO.MaxLength = 11;
            Tnk_TC_NO.Validating += Tnk_TC_NO_Validating;
            KeyPreview = true;
            KeyDown += Kursiyer_Sayfam_KeyDown;
            ApplyUppercaseInputs(this);
        }

        private void Tnk_TC_NO_Validating(object sender, CancelEventArgs e)
        {
            if (TcKimlikValidator.TryExplainProblem(Tnk_TC_NO.Text, out string uyari))
            {
                MessageBox.Show(uyari, "TC Kimlik No", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = false;
            }
        }

        private void Kursiyer_Sayfam_KeyDown(object sender, KeyEventArgs e)
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

        private async void Kursiyer_Sayfam_Load(object sender, EventArgs e)
        {
            try
            {
                await SayfaYukleAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Yükleme Hatası",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SayfaYukleAsync()
        {
            // DÖNEM
            var donemler = await _donemServis.GetDonemlerAsync();
            Cmb_DONEM.DataSource = donemler;
            Cmb_DONEM.DisplayMember = "DonemAdi";
            Cmb_DONEM.ValueMember = "ID";
            Cmb_DONEM.SelectedIndex = -1;

            // SINIF
            var siniflar = await _donemServis.GetSertifikaSiniflariAsync();
            Cmb_SINIFI.Items.Clear();
            foreach (var item in siniflar)
                Cmb_SINIFI.Items.Add(item);

            // ÖNCEKİ SINIF
            var oncekiler = await _donemServis.GetOncekiSertifikaSiniflariAsync();
            Cmb_ONCEKI_SINIFI.Items.Clear();
            Cmb_ONCEKI_SINIFI.Items.Add("");
            foreach (var item in oncekiler)
                Cmb_ONCEKI_SINIFI.Items.Add(item);
        }

        private void Btn_ResimYukle_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Resimler|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                    {
                        Tnk_RESIM_Kursiyer.Image = Image.FromStream(fs);
                    }
                }
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

        private byte[] ImageToBytes(Image img)
        {
            if (img == null) return null;

            using (var ms = new MemoryStream())
            using (var bmp = new Bitmap(img))
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        // ================= KAYDET (INSERT veya UPDATE) =================
        private async void Btn_Kaydet_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Tnk_ADI.Text))
                {
                    MessageBox.Show("Ad zorunludur");
                    return;
                }

                if (Cmb_DONEM.SelectedValue == null)
                {
                    MessageBox.Show("Dönem seçiniz");
                    return;
                }

                if (Cmb_SINIFI.SelectedItem == null)
                {
                    MessageBox.Show("Sertifika sınıfı seçiniz");
                    return;
                }

                int donemId = int.TryParse(Cmb_DONEM.SelectedValue.ToString(), out int tmpId) ? tmpId : 0;
                if (donemId <= 0)
                {
                    MessageBox.Show("Dönem bilgisi hatalı");
                    return;
                }

                int adayNo = int.TryParse(Tnk_ADAY_NO.Text, out int tmpAdayNo) ? tmpAdayNo : 1;
                if (adayNo <= 0) adayNo = 1;

                if (TcKimlikValidator.TryExplainProblem(Tnk_TC_NO.Text, out string tcUyari))
                {
                    MessageBox.Show(tcUyari, "TC Kimlik No", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Tnk_TC_NO.Focus();
                    return;
                }

                if (IsUnderEighteen(Tnk_DOGUM_TARIHI.Value, DateTime.Today))
                {
                    MessageBox.Show(
                        "Aday yaşı 18'den küçüktür.",
                        "Yaş Uyarısı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                var model = new Kursiyer_Model
                {
                    ID = 0, // Yeni kayıt için 0, güncelleme için formdan ID setlenebilir
                    ADI = Tnk_ADI.Text.Trim(),
                    SOYADI = Tnk_SOYADI.Text.Trim(),
                    TC_NO = string.IsNullOrWhiteSpace(Tnk_TC_NO.Text) ? null : Tnk_TC_NO.Text.Trim(),
                    KIM_KAYIT_NO = string.IsNullOrWhiteSpace(Tnk_Kimlik_No.Text) ? null : Tnk_Kimlik_No.Text.Trim(),
                    ID_GRUP_KARTI = donemId,
                    SERTIFIKA_SINIFI = Cmb_SINIFI.SelectedItem.ToString(),
                    ONCE_SERT_SINIFI = Cmb_ONCEKI_SINIFI.SelectedIndex <= 0
                        ? null
                        : Cmb_ONCEKI_SINIFI.SelectedItem.ToString(),
                    ONCE_SERT_BELGESAYI = string.IsNullOrWhiteSpace(Tnk_Ehliyet_No.Text)
                        ? null
                        : Tnk_Ehliyet_No.Text.Trim(),
                    SARI_NOTLAR = string.IsNullOrWhiteSpace(Tnk_Referans.Text) ? null : Tnk_Referans.Text.Trim(),
                    ADAY_NO = adayNo,
                    KURSIYER_DURUMU = 1,
                    DOGUM_TARIHI = Tnk_DOGUM_TARIHI.Value,
                    KAYIT_TARIHI = Tnk_KAYIT_TARIHI.Value,
                    GSM_1 = string.IsNullOrWhiteSpace(Tnk_GSM_1.Text) ? null : Tnk_GSM_1.Text.Trim(),
                    GSM_2 = string.IsNullOrWhiteSpace(Tnk_GSM_2.Text) ? null : Tnk_GSM_2.Text.Trim(),
                    EV_TELEFON = string.IsNullOrWhiteSpace(TextBox10.Text) ? null : TextBox10.Text.Trim(),
                    EV_ADRESI = string.IsNullOrWhiteSpace(Tnk_Adres.Text) ? null : Tnk_Adres.Text.Trim(),
                    KIM_BABA_ADI = string.IsNullOrWhiteSpace(Tnk_KIM_BABA_ADI.Text) ? null : Tnk_KIM_BABA_ADI.Text.Trim(),
                    KIM_ANA_ADI = string.IsNullOrWhiteSpace(Tnk_KIM_ANA_ADI.Text) ? null : Tnk_KIM_ANA_ADI.Text.Trim(),
                    KIM_DOGUM_YERI = string.IsNullOrWhiteSpace(Tnk_KIM_DOGUM_YERI.Text) ? null : Tnk_KIM_DOGUM_YERI.Text.Trim(),
                    RESIM = ImageToBytes(Tnk_RESIM_Kursiyer.Image)
                };

                // 🔹 Tek SP üzerinden insert veya update
                SanitizeKursiyerModelForInsert(model);
                int kursiyerId = await SaveKursiyerSafeAsync(model);
                await SaveKursiyerEkAlanlariDirectAsync(
                    kursiyerId,
                    model,
                    string.IsNullOrWhiteSpace(Tnk_Referans.Text) ? null : Tnk_Referans.Text.Trim(),
                    string.IsNullOrWhiteSpace(Tnk_cinsiyet.Text) ? null : Tnk_cinsiyet.Text.Trim());
                await SaveWebcamImageIfAnyAsync(kursiyerId);
                AppLogService.Write(
                    _connectionString,
                    "INFO",
                    "ADAY",
                    "Aday eklendi. ID=" + kursiyerId + ", AdSoyad=" + (model.ADI + " " + model.SOYADI).Trim());

                MessageBox.Show("Kursiyer başarıyla kaydedildi. ID: " + kursiyerId,
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                Close();
            }
            catch (SqlException ex) when (ex.Number == 8152)
            {
                MessageBox.Show(
                    "Veritabanina yazilan bir metin sutun genisliginden uzun (SQL 8152).\n\n"
                    + "TC kimlik 11 hane olmali; ad/soyad ve diger alanlar kisaltilmis olsa da hedef veritabaninda "
                    + "KURSIYER sutunlari eski kurulumda dar olabilir.\n\n"
                    + ex.Message,
                    "Kayit Hatasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),
                    "Kayıt Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SanitizeKursiyerModelForInsert(Kursiyer_Model model)
        {
            if (model == null)
                return;

            model.ADI = TruncateUnicode(model.ADI, 200);
            model.SOYADI = TruncateUnicode(model.SOYADI, 200);
            model.TC_NO = TcKimlikValidator.NormalizeForDb(model.TC_NO);
            model.KIM_KAYIT_NO = TruncateUnicode(model.KIM_KAYIT_NO, 100);
            model.SERTIFIKA_SINIFI = TruncateUnicode(model.SERTIFIKA_SINIFI, 100);
            model.ONCE_SERT_SINIFI = TruncateUnicode(model.ONCE_SERT_SINIFI, 100);
            model.ONCE_SERT_BELGESAYI = TruncateUnicode(model.ONCE_SERT_BELGESAYI, 100);
            model.SARI_NOTLAR = TruncateUnicode(model.SARI_NOTLAR, 1000);
            model.GSM_1 = TruncateUnicode(model.GSM_1, 40);
            model.GSM_2 = TruncateUnicode(model.GSM_2, 40);
            model.EV_TELEFON = TruncateUnicode(model.EV_TELEFON, 40);
            model.EV_ADRESI = TruncateUnicode(model.EV_ADRESI, 500);
            model.KIM_BABA_ADI = TruncateUnicode(model.KIM_BABA_ADI, 200);
            model.KIM_ANA_ADI = TruncateUnicode(model.KIM_ANA_ADI, 200);
            model.KIM_DOGUM_YERI = TruncateUnicode(model.KIM_DOGUM_YERI, 200);
        }

        private static string TruncateUnicode(string s, int maxChars)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            return s.Length <= maxChars ? s : s.Substring(0, maxChars);
        }


        private async Task<int> SaveKursiyerSafeAsync(Kursiyer_Model model)
        {
            try
            {
                return await _kursiyerServis.SaveKursiyerAsync(model);
            }
            catch
            {
                return await SaveKursiyerDirectAsync(model);
            }
        }

        private Task<int> SaveKursiyerDirectAsync(Kursiyer_Model model)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(_connectionString))
                    throw new InvalidOperationException("Veritabani baglantisi yok.");

                string tableName = ResolveKursiyerTableName(_connectionString);
                if (string.IsNullOrWhiteSpace(tableName))
                    throw new InvalidOperationException("Kursiyer tablosu bulunamadi.");

                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    var columns = GetColumnSet(con, tableName);

                    var insertCols = new List<string>();
                    var insertParams = new List<string>();

                    Action<string, object, SqlCommand> addParam = (col, value, cmd) =>
                    {
                        if (!columns.Contains(col)) return;
                        insertCols.Add(col);
                        insertParams.Add("@" + col);
                        if (col == "RESIM")
                        {
                            var pResim = cmd.Parameters.Add("@RESIM", SqlDbType.VarBinary, -1);
                            pResim.Value = value ?? DBNull.Value;
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@" + col, value ?? DBNull.Value);
                        }
                    };

                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        addParam("ADI", (model.ADI ?? string.Empty).Trim(), cmd);
                        addParam("SOYADI", (model.SOYADI ?? string.Empty).Trim(), cmd);
                        addParam("TC_NO", (object)model.TC_NO ?? DBNull.Value, cmd);
                        if (columns.Contains("KIMLIK_KAYIT_NO"))
                            addParam("KIMLIK_KAYIT_NO", (object)model.KIM_KAYIT_NO ?? DBNull.Value, cmd);
                        else if (columns.Contains("KIM_KAYIT_NO"))
                            addParam("KIM_KAYIT_NO", (object)model.KIM_KAYIT_NO ?? DBNull.Value, cmd);
                        addParam("ID_GRUP_KARTI", model.ID_GRUP_KARTI > 0 ? (object)model.ID_GRUP_KARTI : DBNull.Value, cmd);
                        addParam("SERTIFIKA_SINIFI", (object)model.SERTIFIKA_SINIFI ?? DBNull.Value, cmd);
                        addParam("ONCE_SERT_SINIFI", (object)model.ONCE_SERT_SINIFI ?? DBNull.Value, cmd);
                        addParam("ONCE_SERT_BELGESAYI", (object)model.ONCE_SERT_BELGESAYI ?? DBNull.Value, cmd);
                        addParam("ON_NOTLAR", (object)model.SARI_NOTLAR ?? DBNull.Value, cmd);
                        addParam("ADAY_NO", model.ADAY_NO > 0 ? (object)model.ADAY_NO : DBNull.Value, cmd);
                        addParam("KURSIYER_DURUMU", model.KURSIYER_DURUMU, cmd);
                        addParam("DOGUM_TARIHI", model.DOGUM_TARIHI == DateTime.MinValue ? (object)DBNull.Value : model.DOGUM_TARIHI, cmd);
                        addParam("KAYIT_TARIHI", model.KAYIT_TARIHI == DateTime.MinValue ? (object)DBNull.Value : model.KAYIT_TARIHI, cmd);
                        addParam("GSM_1", (object)model.GSM_1 ?? DBNull.Value, cmd);
                        addParam("GSM_2", (object)model.GSM_2 ?? DBNull.Value, cmd);
                        if (columns.Contains("EV_TELEFON"))
                            addParam("EV_TELEFON", (object)model.EV_TELEFON ?? DBNull.Value, cmd);
                        else if (columns.Contains("IS_TELEFON_1"))
                            addParam("IS_TELEFON_1", (object)model.EV_TELEFON ?? DBNull.Value, cmd);
                        addParam("EV_ADRESI", (object)model.EV_ADRESI ?? DBNull.Value, cmd);
                        if (columns.Contains("KIMLIK_BABA_ADI"))
                            addParam("KIMLIK_BABA_ADI", (object)model.KIM_BABA_ADI ?? DBNull.Value, cmd);
                        else if (columns.Contains("KIM_BABA_ADI"))
                            addParam("KIM_BABA_ADI", (object)model.KIM_BABA_ADI ?? DBNull.Value, cmd);
                        addParam("KIM_ANA_ADI", (object)model.KIM_ANA_ADI ?? DBNull.Value, cmd);
                        if (columns.Contains("KIMLIK_DOGUM_YERI"))
                            addParam("KIMLIK_DOGUM_YERI", (object)model.KIM_DOGUM_YERI ?? DBNull.Value, cmd);
                        else if (columns.Contains("KIM_DOGUM_YERI"))
                            addParam("KIM_DOGUM_YERI", (object)model.KIM_DOGUM_YERI ?? DBNull.Value, cmd);
                        addParam("RESIM", model.RESIM, cmd);

                        if (insertCols.Count == 0)
                            throw new InvalidOperationException("Kursiyer tablosunda yazilabilir kolon bulunamadi.");

                        cmd.CommandText = "INSERT INTO [" + tableName + "] (" + string.Join(",", insertCols) + ") VALUES (" + string.Join(",", insertParams) + "); SELECT CAST(SCOPE_IDENTITY() AS int);";
                        object idObj = cmd.ExecuteScalar();
                        int newId;
                        return int.TryParse(Convert.ToString(idObj), out newId) ? newId : 0;
                    }
                }
            });
        }

        private Task SaveKursiyerEkAlanlariDirectAsync(int kursiyerId, Kursiyer_Model model, string referans, string cinsiyet)
        {
            return Task.Run(() =>
            {
                if (kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString) || model == null)
                    return;

                string tableName = ResolveKursiyerTableName(_connectionString);
                if (string.IsNullOrWhiteSpace(tableName))
                    return;

                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    var columns = GetColumnSet(con, tableName);
                    var sets = new List<string>();
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        AddSetIfExists(columns, sets, cmd, "GSM_1", model.GSM_1);
                        AddSetIfExists(columns, sets, cmd, "GSM_2", model.GSM_2);
                        AddSetIfExists(columns, sets, cmd, "ON_NOTLAR", referans);
                        AddSetIfExists(columns, sets, cmd, "CINSIYET", cinsiyet);
                        if (columns.Contains("EV_TELEFON"))
                            AddSetIfExists(columns, sets, cmd, "EV_TELEFON", model.EV_TELEFON);
                        else if (columns.Contains("IS_TELEFON_1"))
                            AddSetIfExists(columns, sets, cmd, "IS_TELEFON_1", model.EV_TELEFON);
                        AddSetIfExists(columns, sets, cmd, "EV_ADRESI", model.EV_ADRESI);
                        if (columns.Contains("KIMLIK_BABA_ADI"))
                            AddSetIfExists(columns, sets, cmd, "KIMLIK_BABA_ADI", model.KIM_BABA_ADI);
                        else if (columns.Contains("KIM_BABA_ADI"))
                            AddSetIfExists(columns, sets, cmd, "KIM_BABA_ADI", model.KIM_BABA_ADI);
                        AddSetIfExists(columns, sets, cmd, "KIM_ANA_ADI", model.KIM_ANA_ADI);
                        if (columns.Contains("KIMLIK_DOGUM_YERI"))
                            AddSetIfExists(columns, sets, cmd, "KIMLIK_DOGUM_YERI", model.KIM_DOGUM_YERI);
                        else if (columns.Contains("KIM_DOGUM_YERI"))
                            AddSetIfExists(columns, sets, cmd, "KIM_DOGUM_YERI", model.KIM_DOGUM_YERI);

                        if (sets.Count == 0)
                            return;

                        cmd.Parameters.AddWithValue("@ID", kursiyerId);
                        cmd.CommandText = "UPDATE [" + tableName + "] SET " + string.Join(",", sets) + " WHERE ID=@ID;";
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        private async Task SaveWebcamImageIfAnyAsync(int kursiyerId)
        {
            if (kursiyerId <= 0 || _webcamImageBytes == null || _webcamImageBytes.Length == 0)
                return;

            const string sql = "UPDATE KURSIYER SET RESIM_WEBCAM=@RESIM_WEBCAM WHERE ID=@ID";
            using (var con = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@ID", kursiyerId);
                var p = cmd.Parameters.Add("@RESIM_WEBCAM", SqlDbType.VarBinary, -1);
                p.Value = _webcamImageBytes;
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static string ResolveKursiyerTableName(string connectionString)
        {
            string[] adaylar = { "KURSIYER", "KURSIYERLER", "KURSİYER" };
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                foreach (string t in adaylar)
                {
                    using (var cmd = new SqlCommand("SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME=@t", con))
                    {
                        cmd.Parameters.AddWithValue("@t", t);
                        object o = cmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value)
                            return Convert.ToString(o);
                    }
                }
            }
            return null;
        }

        private static HashSet<string> GetColumnSet(SqlConnection con, string tableName)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t", con))
            {
                cmd.Parameters.AddWithValue("@t", tableName);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        set.Add(Convert.ToString(r["COLUMN_NAME"]));
                }
            }
            return set;
        }

        private static void AddSetIfExists(HashSet<string> columns, List<string> sets, SqlCommand cmd, string col, object value)
        {
            if (!columns.Contains(col))
                return;
            string p = "@P_" + col;
            sets.Add(col + "=" + p);
            cmd.Parameters.AddWithValue(p, value ?? DBNull.Value);
        }

        private static bool IsUnderEighteen(DateTime dogumTarihi, DateTime referansTarih)
        {
            DateTime onSekiz = dogumTarihi.Date.AddYears(18);
            return referansTarih.Date < onSekiz;
        }

        private void Btn_Rapor_Al_Click(object sender, EventArgs e)
        {
            string seciliAdSoyad = ((Tnk_ADI.Text ?? string.Empty) + " " + (Tnk_SOYADI.Text ?? string.Empty)).Trim();
            using (var raporDetay = new RaporDetay(_connectionString, "KURSIYER", 0, "KURSIYER", seciliAdSoyad))
            {
                raporDetay.ShowDialog(this);
            }
        }
    }
}