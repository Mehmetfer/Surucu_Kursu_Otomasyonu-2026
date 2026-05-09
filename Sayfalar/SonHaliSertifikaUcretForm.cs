using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class SonHaliSertifikaUcretForm : Form
    {
        private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

        private readonly string _connectionString;
        private SinifGridKaynak _sinifGridKaynak = SinifGridKaynak.Yok;
        private bool _hesaplamaIci;

        private enum SinifGridKaynak
        {
            Yok,
            SinifParam,
            SertifikaSinifParam,
            DigerTablo
        }

        /// <summary>
        /// Gömülü gösterimde true dönerse pencere kapanmaz (ör. üst sekmede kal).
        /// </summary>
        public Func<bool> KapatGomuluDavran { get; set; }

        public SonHaliSertifikaUcretForm()
            : this(null)
        {
        }

        public SonHaliSertifikaUcretForm(string connectionString)
        {
            _connectionString = connectionString ?? string.Empty;
            InitializeComponent();
            dgvSiniflar.SelectionChanged += DgvSiniflar_SelectionChanged;
            Load += SonHaliSertifikaUcretForm_Load;
            tsbYeni.Click += TsbYeni_Click;
            tsbKaydet.Click += TsbKaydet_Click;
            tsbSil.Click += TsbSil_Click;
            tsbHizliDuzenle.Click += TsbHizliDuzenle_Click;
            tsbSinifKopyala.Click += TsbSinifKopyala_Click;
            cmbTrafikCevre.SelectedIndexChanged += HesapAlanDegisti;
            cmbMotorAracTek.SelectedIndexChanged += HesapAlanDegisti;
            cmbIlkYardim.SelectedIndexChanged += HesapAlanDegisti;
            cmbTrafikAdabi.SelectedIndexChanged += HesapAlanDegisti;
            cmbSimulatorAlan.SelectedIndexChanged += HesapAlanDegisti;
            cmbDireksiyonDers.SelectedIndexChanged += HesapAlanDegisti;
            cmbDireksiyonDersToplam.SelectedIndexChanged += HesapAlanDegisti;
            txtTeoriSaatlikUcret.TextChanged += HesapAlanDegisti;
            txtDireksiyonSaatlikUcret.TextChanged += HesapAlanDegisti;
            txtDireksiyonDersUcreti.TextChanged += HesapAlanDegisti;
            tsbKapat.Click += (_, __) =>
            {
                if (KapatGomuluDavran != null && KapatGomuluDavran())
                    return;
                Close();
            };
        }

        private void SonHaliSertifikaUcretForm_Load(object sender, EventArgs e)
        {
            SetupToolbarGlyphs();
            PopulateSaatCombos();
            StyleGrid();
            YukleSinifListesi();
            grpSinavHarclari.BackColor = Color.FromArgb(255, 236, 210);
            grpTeoriDersleri.BackColor = Color.FromArgb(255, 248, 210);
            grpDireksiyonDersleri.BackColor = Color.FromArgb(210, 242, 255);
            grpTabanOzet.BackColor = Color.FromArgb(220, 255, 220);
            RecalculateAll();
        }

        private void SetupToolbarGlyphs()
        {
            const int s = 28;
            tsbYeni.Image = ToolbarGlyph.PlusOnGreen(s);
            tsbKaydet.Image = ToolbarGlyph.CheckOnGreen(s);
            tsbSil.Image = ToolbarGlyph.XOnRed(s);
            tsbHizliDuzenle.Image = ToolbarGlyph.EditNote(s);
            tsbSinifKopyala.Image = ToolbarGlyph.CopyDoc(s);
            tsbKapat.Image = ToolbarGlyph.XOnDarkRed(s);

            foreach (ToolStripItem item in toolStripMain.Items)
            {
                if (item is ToolStripButton b)
                {
                    b.ImageScaling = ToolStripItemImageScaling.None;
                    b.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    b.TextImageRelation = TextImageRelation.ImageAboveText;
                }
            }
        }

        private void PopulateSaatCombos()
        {
            var saatlar = Enumerable.Range(0, 51).Select(i => i.ToString()).ToArray();
            foreach (var c in new[]
                     {
                         cmbTrafikCevre, cmbMotorAracTek, cmbIlkYardim, cmbTrafikAdabi,
                         cmbSimulatorAlan, cmbDireksiyonDers, cmbDireksiyonDersToplam
                     })
            {
                c.Items.Clear();
                c.Items.AddRange(saatlar);
            }
        }

        private void StyleGrid()
        {
            dgvSiniflar.AutoGenerateColumns = true;
            dgvSiniflar.EnableHeadersVisualStyles = false;
            dgvSiniflar.BackgroundColor = Color.FromArgb(245, 247, 250);
            dgvSiniflar.BorderStyle = BorderStyle.None;
            dgvSiniflar.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvSiniflar.GridColor = Color.FromArgb(220, 224, 230);
            dgvSiniflar.RowHeadersVisible = false;
            dgvSiniflar.AllowUserToResizeRows = false;
            dgvSiniflar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSiniflar.MultiSelect = false;
            dgvSiniflar.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(232, 240, 252);
            dgvSiniflar.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 120, 215);
            dgvSiniflar.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvSiniflar.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 73, 94);
            dgvSiniflar.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvSiniflar.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvSiniflar.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgvSiniflar.RowTemplate.Height = 24;
        }

        /// <summary>
        /// Listeyi veritabanından doldurur (SinifParam / SertifikaSinifParam veya eski tablolar).
        /// </summary>
        public void YukleSinifListesi()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            var mappedQueries = new[]
            {
                Tuple.Create(SinifGridKaynak.SinifParam, @"SELECT TOP (2000)
    sp.ID,
    ISNULL(sp.SINIF_MEVCUT,'') AS Mevcut,
    ISNULL(sp.SINIF_YENI,'') AS Yeni,
    CAST(ISNULL(sp.SINIF_TABAN_FIYAT, ISNULL(sp.SINIF_KURS_UCRETI, 0)) AS money) AS TFiyat
FROM dbo.SinifParam sp
ORDER BY sp.ID"),
                Tuple.Create(SinifGridKaynak.SertifikaSinifParam, @"SELECT TOP (2000)
    p.ID,
    ISNULL(p.MEVCUT_SINIF,'') AS Mevcut,
    ISNULL(p.YENI_SINIF,'') AS Yeni,
    CAST(ISNULL(p.UCRET, 0) AS money) AS TFiyat
FROM dbo.SertifikaSinifParam p
ORDER BY p.ID")
            };

            foreach (var pair in mappedQueries)
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    using (var da = new SqlDataAdapter(pair.Item2, conn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        _sinifGridKaynak = pair.Item1;
                        dgvSiniflar.DataSource = dt;
                        ApplyMappedGridColumns();
                        if (dgvSiniflar.Rows.Count > 0)
                            dgvSiniflar.Rows[0].Selected = true;
                        return;
                    }
                }
                catch
                {
                    // Tablo veya kolon yoksa sırada ki sorguya geç.
                }
            }

            foreach (var table in new[] { "SERTIFIKA_SINIFLARI", "SINIFLAR" })
            {
                try
                {
                    using (var conn = new SqlConnection(_connectionString))
                    using (var da = new SqlDataAdapter("SELECT TOP (1000) * FROM dbo.[" + table + "]", conn))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        _sinifGridKaynak = SinifGridKaynak.DigerTablo;
                        dgvSiniflar.DataSource = dt;
                        if (dgvSiniflar.Rows.Count > 0)
                            dgvSiniflar.Rows[0].Selected = true;
                        return;
                    }
                }
                catch
                {
                }
            }

            _sinifGridKaynak = SinifGridKaynak.Yok;
        }

        private void ApplyMappedGridColumns()
        {
            if (dgvSiniflar.Columns["ID"] != null)
            {
                dgvSiniflar.Columns["ID"].Visible = false;
                dgvSiniflar.Columns["ID"].Width = 1;
            }
            if (dgvSiniflar.Columns["Mevcut"] != null)
                dgvSiniflar.Columns["Mevcut"].HeaderText = "MEVCUT";
            if (dgvSiniflar.Columns["Yeni"] != null)
                dgvSiniflar.Columns["Yeni"].HeaderText = "YENİ";
            if (dgvSiniflar.Columns["TFiyat"] != null)
            {
                dgvSiniflar.Columns["TFiyat"].HeaderText = "T.FİYAT";
                dgvSiniflar.Columns["TFiyat"].DefaultCellStyle.Format = "N2";
                dgvSiniflar.Columns["TFiyat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void DgvSiniflar_SelectionChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            var cur = dgvSiniflar.CurrentRow;
            if (cur == null || cur.IsNewRow || cur.Index < 0)
            {
                TemizleDetayAlanlari();
                return;
            }

            if (!(cur.DataBoundItem is DataRowView drv))
                return;

            var t = drv.Row.Table;
            if (!KolonVar(t, "ID") || drv["ID"] == DBNull.Value)
            {
                if (_sinifGridKaynak == SinifGridKaynak.DigerTablo)
                    DoldurDetayDigerTablodan(drv.Row);
                else
                    TemizleDetayAlanlari();
                return;
            }

            var id = Convert.ToInt32(drv["ID"], Tr);
            switch (_sinifGridKaynak)
            {
                case SinifGridKaynak.SinifParam:
                    DoldurSinifParamDetay(id);
                    break;
                case SinifGridKaynak.SertifikaSinifParam:
                    DoldurSertifikaSinifParamDetay(id);
                    break;
                case SinifGridKaynak.DigerTablo:
                    DoldurDetayDigerTablodan(drv.Row);
                    break;
                default:
                    TemizleDetayAlanlari();
                    break;
            }
        }

        private void DoldurSinifParamDetay(int id)
        {
            const string sql = "SELECT * FROM dbo.SinifParam WHERE ID = @ID";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count == 0)
                            TemizleDetayAlanlari();
                        else
                            ApplySinifParamRow(dt.Rows[0]);
                    }
                }
            }
            catch
            {
                TemizleDetayAlanlari();
            }
        }

        private void DoldurSertifikaSinifParamDetay(int id)
        {
            const string sql = "SELECT * FROM dbo.SertifikaSinifParam WHERE ID = @ID";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows.Count == 0)
                            TemizleDetayAlanlari();
                        else
                            ApplySertifikaSinifParamRow(dt.Rows[0]);
                    }
                }
            }
            catch
            {
                TemizleDetayAlanlari();
            }
        }

        private void ApplySinifParamRow(DataRow r)
        {
            EnsureComboMevcut(Metin(r, "SINIF_MEVCUT"));
            txtYeniSinif.Text = Metin(r, "SINIF_YENI");
            chkEOncesi.Checked = DbBool(r, "SERT_2016_ONCESI");
            chkENakil.Checked = DbBool(r, "E_SINAV_MUAF");
            chkA1.Checked = DbBool(r, "YUZ_YIRMI_BES_CC");
            txtYasSiniri.Text = Metin(r, "SINIF_YAS");
            txtKullandigiAraclar.Text = Metin(r, "SINIF_KUL_ARACLAR");
            txtKapsam.Text = Metin(r, "SINIF_KAPSAMI");
            txtDeneyimSarti.Text = Metin(r, "SINIF_DENEYIM");

            txtTeoriSinavHarci.Text = "0,00";
            txtDireksiyonSinavHarci.Text = "0,00";

            SetComboSaat(cmbTrafikCevre, Metin(r, "SINIF_TEORI_TRAFIK"));
            SetComboSaat(cmbMotorAracTek, Metin(r, "SINIF_TEORI_MOTOR"));
            SetComboSaat(cmbIlkYardim, Metin(r, "SINIF_TEORI_ILKYRDM"));
            SetComboSaat(cmbTrafikAdabi, Metin(r, "SINIF_TEORI_TRAFIK_ADABI"));
            txtTeoriDersToplam.Text = Metin(r, "SINIF_TEORI_TOP_SAAT");
            txtTeoriSaatlikUcret.Text = ParaMetin(r, "SINIF_TEORI_1SAAT_UCRETI");
            txtTeoriDersUcreti.Text = ParaMetin(r, "SINIF_TEORI_TOP_UCRETI");

            SetComboSaat(cmbSimulatorAlan, Metin(r, "SINIF_DRKS_SMLT_EGTM"));
            SetComboSaat(cmbDireksiyonDers, Metin(r, "SINIF_DRKS_SAAT"));
            SetComboSaat(cmbDireksiyonDersToplam, Metin(r, "SINIF_DRKS_TOP_SAAT"));
            txtDireksiyonSaatlikUcret.Text = ParaMetin(r, "SINIF_DRKS_1SAAT_UCRETI");
            txtDireksiyonDersUcreti.Text = ParaMetin(r, "SINIF_DRKS_TOP_UCRETI");
            txtTabanFiyat.Text = ParaMetin(r, "SINIF_TABAN_FIYAT");
            RecalculateAll();
        }

        private void ApplySertifikaSinifParamRow(DataRow r)
        {
            EnsureComboMevcut(Metin(r, "MEVCUT_SINIF"));
            txtYeniSinif.Text = Metin(r, "YENI_SINIF");
            chkEOncesi.Checked = false;
            chkENakil.Checked = false;
            chkA1.Checked = false;
            txtYasSiniri.Text = Metin(r, "YAS");
            txtKullandigiAraclar.Text = Metin(r, "KULLANDIGI_ARACLAR");
            txtKapsam.Text = Metin(r, "KAPSAMI");
            txtDeneyimSarti.Text = Metin(r, "DENEYIM");

            txtTeoriSinavHarci.Text = ParaMetin(r, "TEORI_HARC");
            txtDireksiyonSinavHarci.Text = ParaMetin(r, "DRKS_HARC");

            SetComboSaat(cmbTrafikCevre, Metin(r, "TRAFIK"));
            SetComboSaat(cmbMotorAracTek, Metin(r, "MOTOR"));
            SetComboSaat(cmbIlkYardim, Metin(r, "ILK_YARDIM"));
            SetComboSaat(cmbTrafikAdabi, string.Empty);
            txtTeoriDersToplam.Text = string.Empty;
            txtTeoriSaatlikUcret.Text = "0,00";
            txtTeoriDersUcreti.Text = "0,00";

            SetComboSaat(cmbSimulatorAlan, string.Empty);
            SetComboSaat(cmbDireksiyonDers, Metin(r, "DIREKSIYON"));
            SetComboSaat(cmbDireksiyonDersToplam, string.Empty);
            txtDireksiyonSaatlikUcret.Text = "0,00";
            txtDireksiyonDersUcreti.Text = "0,00";
            txtTabanFiyat.Text = ParaMetin(r, "UCRET");
            RecalculateAll();
        }

        private void DoldurDetayDigerTablodan(DataRow r)
        {
            if (KolonVar(r.Table, "SINIF_MEVCUT") || KolonVar(r.Table, "SINIF_YENI"))
            {
                ApplySinifParamRow(r);
                return;
            }
            if (KolonVar(r.Table, "MEVCUT_SINIF") || KolonVar(r.Table, "YENI_SINIF"))
            {
                ApplySertifikaSinifParamRow(r);
                return;
            }

            TemizleDetayAlanlari();
        }

        private void TemizleDetayAlanlari()
        {
            cmbMevcutSinif.SelectedIndex = -1;
            txtYeniSinif.Clear();
            chkEOncesi.Checked = chkENakil.Checked = chkA1.Checked = false;
            txtYasSiniri.Clear();
            txtKullandigiAraclar.Clear();
            txtKapsam.Clear();
            txtDeneyimSarti.Clear();
            txtTeoriSinavHarci.Text = txtDireksiyonSinavHarci.Text = "0,00";
            foreach (var c in new[]
                     {
                         cmbTrafikCevre, cmbMotorAracTek, cmbIlkYardim, cmbTrafikAdabi,
                         cmbSimulatorAlan, cmbDireksiyonDers, cmbDireksiyonDersToplam
                     })
                c.SelectedIndex = -1;
            txtTeoriDersToplam.Clear();
            txtTeoriSaatlikUcret.Text = txtTeoriDersUcreti.Text = "0,00";
            txtDireksiyonSaatlikUcret.Text = txtDireksiyonDersUcreti.Text = txtTabanFiyat.Text = "0,00";
            RecalculateAll();
        }

        private void EnsureComboMevcut(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                cmbMevcutSinif.SelectedIndex = -1;
                return;
            }

            var s = text.Trim();
            int idx = cmbMevcutSinif.FindStringExact(s);
            if (idx >= 0)
                cmbMevcutSinif.SelectedIndex = idx;
            else
            {
                cmbMevcutSinif.Items.Add(s);
                cmbMevcutSinif.SelectedItem = s;
            }
        }

        private void SetComboSaat(ComboBox c, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                c.SelectedIndex = -1;
                return;
            }

            var s = text.Trim();
            int idx = c.FindStringExact(s);
            if (idx >= 0)
                c.SelectedIndex = idx;
            else
            {
                c.Items.Add(s);
                c.SelectedItem = s;
            }
        }

        private static bool KolonVar(DataTable t, string name)
        {
            return t != null && t.Columns.Contains(name);
        }

        private static string Metin(DataRow r, string kolon)
        {
            if (r == null || !r.Table.Columns.Contains(kolon))
                return string.Empty;
            var v = r[kolon];
            return v == DBNull.Value || v == null ? string.Empty : v.ToString().Trim();
        }

        private static bool DbBool(DataRow r, string kolon)
        {
            if (r == null || !r.Table.Columns.Contains(kolon))
                return false;
            var v = r[kolon];
            if (v == DBNull.Value || v == null)
                return false;
            if (v is bool b)
                return b;
            if (bool.TryParse(v.ToString(), out var pb))
                return pb;
            if (int.TryParse(v.ToString(), NumberStyles.Integer, Tr, out var n))
                return n != 0;
            return false;
        }

        private static string ParaMetin(DataRow r, string kolon)
        {
            if (r == null || !r.Table.Columns.Contains(kolon))
                return "0,00";
            var v = r[kolon];
            if (v == DBNull.Value || v == null)
                return "0,00";
            try
            {
                return Convert.ToDecimal(v, Tr).ToString("N2", Tr);
            }
            catch
            {
                return "0,00";
            }
        }

        private void TsbYeni_Click(object sender, EventArgs e)
        {
            dgvSiniflar.ClearSelection();
            TemizleDetayAlanlari();
            cmbMevcutSinif.Focus();
        }

        private void TsbKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabanı bağlantısı bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_sinifGridKaynak == SinifGridKaynak.Yok || _sinifGridKaynak == SinifGridKaynak.DigerTablo)
            {
                MessageBox.Show("Bu görünümde kaydetme desteklenmiyor. Lütfen SinifParam veya SertifikaSinifParam kaynağını kullanın.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_sinifGridKaynak == SinifGridKaynak.SinifParam)
                    KaydetSinifParam();
                else if (_sinifGridKaynak == SinifGridKaynak.SertifikaSinifParam)
                    KaydetSertifikaSinifParam();

                YukleSinifListesi();
                MessageBox.Show("Kayıt tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kayıt hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TsbSil_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            int id;
            if (!TryGetSelectedId(out id))
            {
                MessageBox.Show("Silinecek bir satır seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_sinifGridKaynak == SinifGridKaynak.Yok || _sinifGridKaynak == SinifGridKaynak.DigerTablo)
            {
                MessageBox.Show("Bu görünümde silme desteklenmiyor.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Seçili sınıf kaydı silinsin mi?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            string table = _sinifGridKaynak == SinifGridKaynak.SinifParam ? "SinifParam" : "SertifikaSinifParam";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("DELETE FROM dbo." + table + " WHERE ID=@ID", conn))
                {
                    cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                YukleSinifListesi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Silme hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TsbHizliDuzenle_Click(object sender, EventArgs e)
        {
            if (dgvSiniflar.CurrentCell == null)
            {
                MessageBox.Show("Önce listeden bir satır seçiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            dgvSiniflar.Focus();
            dgvSiniflar.BeginEdit(true);
        }

        private void TsbSinifKopyala_Click(object sender, EventArgs e)
        {
            if (_sinifGridKaynak == SinifGridKaynak.Yok || _sinifGridKaynak == SinifGridKaynak.DigerTablo)
            {
                MessageBox.Show("Bu görünümde sınıf kopyalama desteklenmiyor.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int id;
            if (!TryGetSelectedId(out id))
            {
                MessageBox.Show("Kopyalanacak sınıfı seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string yeniSinif = (txtYeniSinif.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(yeniSinif))
            {
                MessageBox.Show("Kopya için Yeni Sınıf alanını doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtYeniSinif.Focus();
                return;
            }

            try
            {
                if (_sinifGridKaynak == SinifGridKaynak.SinifParam)
                {
                    const string sql = @"
INSERT INTO dbo.SinifParam
(SINIF_MEVCUT, SINIF_YENI, SINIF_YAS, SINIF_KUL_ARACLAR, SINIF_KAPSAMI, SINIF_DENEYIM,
 SERT_2016_ONCESI, E_SINAV_MUAF, YUZ_YIRMI_BES_CC, SINIF_TABAN_FIYAT)
SELECT SINIF_MEVCUT, @YENI, SINIF_YAS, SINIF_KUL_ARACLAR, SINIF_KAPSAMI, SINIF_DENEYIM,
       SERT_2016_ONCESI, E_SINAV_MUAF, YUZ_YIRMI_BES_CC, SINIF_TABAN_FIYAT
FROM dbo.SinifParam WHERE ID=@ID;";
                    using (var conn = new SqlConnection(_connectionString))
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@YENI", SqlDbType.NVarChar, 50).Value = yeniSinif;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    const string sql = @"
INSERT INTO dbo.SertifikaSinifParam
(MEVCUT_SINIF, YENI_SINIF, YAS, KULLANDIGI_ARACLAR, KAPSAMI, DENEYIM, UCRET)
SELECT MEVCUT_SINIF, @YENI, YAS, KULLANDIGI_ARACLAR, KAPSAMI, DENEYIM, UCRET
FROM dbo.SertifikaSinifParam WHERE ID=@ID;";
                    using (var conn = new SqlConnection(_connectionString))
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                        cmd.Parameters.Add("@YENI", SqlDbType.NVarChar, 50).Value = yeniSinif;
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                YukleSinifListesi();
                MessageBox.Show("Sınıf kopyalandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kopyalama hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KaydetSinifParam()
        {
            int id;
            bool updateMode = TryGetSelectedId(out id);
            const string updateSql = @"
UPDATE dbo.SinifParam SET
  SINIF_MEVCUT=@mevcut,
  SINIF_YENI=@yeni,
  SERT_2016_ONCESI=@eOncesi,
  E_SINAV_MUAF=@nakil,
  YUZ_YIRMI_BES_CC=@a1,
  SINIF_YAS=@yas,
  SINIF_KUL_ARACLAR=@arac,
  SINIF_KAPSAMI=@kapsam,
  SINIF_DENEYIM=@deneyim,
  SINIF_TEORI_TRAFIK=@teoriTrafik,
  SINIF_TEORI_MOTOR=@teoriMotor,
  SINIF_TEORI_ILKYRDM=@teoriIlkyardim,
  SINIF_TEORI_TRAFIK_ADABI=@teoriAdabi,
  SINIF_TEORI_TOP_SAAT=@teoriTopSaat,
  SINIF_TEORI_1SAAT_UCRETI=@teoriSaatlik,
  SINIF_TEORI_TOP_UCRETI=@teoriTopUcret,
  SINIF_DRKS_SMLT_EGTM=@simSaat,
  SINIF_DRKS_SAAT=@drksSaat,
  SINIF_DRKS_TOP_SAAT=@drksTopSaat,
  SINIF_DRKS_1SAAT_UCRETI=@drksSaatlik,
  SINIF_DRKS_TOP_UCRETI=@drksTopUcret,
  SINIF_KURS_UCRETI=@kursUcreti,
  SINIF_TABAN_FIYAT=@taban
WHERE ID=@id";
            const string insertSql = @"
INSERT INTO dbo.SinifParam
(SINIF_MEVCUT,SINIF_YENI,SERT_2016_ONCESI,E_SINAV_MUAF,YUZ_YIRMI_BES_CC,SINIF_YAS,SINIF_KUL_ARACLAR,SINIF_KAPSAMI,SINIF_DENEYIM,
 SINIF_TEORI_TRAFIK,SINIF_TEORI_MOTOR,SINIF_TEORI_ILKYRDM,SINIF_TEORI_TRAFIK_ADABI,SINIF_TEORI_TOP_SAAT,SINIF_TEORI_1SAAT_UCRETI,SINIF_TEORI_TOP_UCRETI,
 SINIF_DRKS_SMLT_EGTM,SINIF_DRKS_SAAT,SINIF_DRKS_TOP_SAAT,SINIF_DRKS_1SAAT_UCRETI,SINIF_DRKS_TOP_UCRETI,SINIF_KURS_UCRETI,SINIF_TABAN_FIYAT)
VALUES
(@mevcut,@yeni,@eOncesi,@nakil,@a1,@yas,@arac,@kapsam,@deneyim,
 @teoriTrafik,@teoriMotor,@teoriIlkyardim,@teoriAdabi,@teoriTopSaat,@teoriSaatlik,@teoriTopUcret,
 @simSaat,@drksSaat,@drksTopSaat,@drksSaatlik,@drksTopUcret,@kursUcreti,@taban)";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(updateMode ? updateSql : insertSql, conn))
            {
                cmd.Parameters.Add("@mevcut", SqlDbType.NVarChar, 50).Value = SeciliMetin(cmbMevcutSinif);
                cmd.Parameters.Add("@yeni", SqlDbType.NVarChar, 50).Value = (txtYeniSinif.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@eOncesi", SqlDbType.Bit).Value = chkEOncesi.Checked;
                cmd.Parameters.Add("@nakil", SqlDbType.Bit).Value = chkENakil.Checked;
                cmd.Parameters.Add("@a1", SqlDbType.Bit).Value = chkA1.Checked;
                cmd.Parameters.Add("@yas", SqlDbType.NVarChar, 20).Value = (txtYasSiniri.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@arac", SqlDbType.NVarChar, 300).Value = (txtKullandigiAraclar.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@kapsam", SqlDbType.NVarChar, 300).Value = (txtKapsam.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@deneyim", SqlDbType.NVarChar, 300).Value = (txtDeneyimSarti.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@teoriTrafik", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbTrafikCevre);
                cmd.Parameters.Add("@teoriMotor", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbMotorAracTek);
                cmd.Parameters.Add("@teoriIlkyardim", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbIlkYardim);
                cmd.Parameters.Add("@teoriAdabi", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbTrafikAdabi);
                cmd.Parameters.Add("@teoriTopSaat", SqlDbType.NVarChar, 10).Value = (txtTeoriDersToplam.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@teoriSaatlik", SqlDbType.Money).Value = ParseMoney(txtTeoriSaatlikUcret.Text);
                cmd.Parameters.Add("@teoriTopUcret", SqlDbType.Money).Value = ParseMoney(txtTeoriDersUcreti.Text);
                cmd.Parameters.Add("@simSaat", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbSimulatorAlan);
                cmd.Parameters.Add("@drksSaat", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbDireksiyonDers);
                cmd.Parameters.Add("@drksTopSaat", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbDireksiyonDersToplam);
                cmd.Parameters.Add("@drksSaatlik", SqlDbType.Money).Value = ParseMoney(txtDireksiyonSaatlikUcret.Text);
                cmd.Parameters.Add("@drksTopUcret", SqlDbType.Money).Value = ParseMoney(txtDireksiyonDersUcreti.Text);
                cmd.Parameters.Add("@kursUcreti", SqlDbType.Money).Value = ParseMoney(txtTabanFiyat.Text);
                cmd.Parameters.Add("@taban", SqlDbType.Money).Value = ParseMoney(txtTabanFiyat.Text);
                if (updateMode)
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void KaydetSertifikaSinifParam()
        {
            int id;
            bool updateMode = TryGetSelectedId(out id);
            const string updateSql = @"
UPDATE dbo.SertifikaSinifParam SET
  MEVCUT_SINIF=@mevcut,
  YENI_SINIF=@yeni,
  YAS=@yas,
  KULLANDIGI_ARACLAR=@arac,
  KAPSAMI=@kapsam,
  DENEYIM=@deneyim,
  TRAFIK=@trafik,
  MOTOR=@motor,
  ILK_YARDIM=@ilkYardim,
  DIREKSIYON=@direksiyon,
  TEORI_HARC=@teoriHarc,
  DRKS_HARC=@drksHarc,
  UCRET=@ucret
WHERE ID=@id";
            const string insertSql = @"
INSERT INTO dbo.SertifikaSinifParam
(MEVCUT_SINIF,YENI_SINIF,YAS,KULLANDIGI_ARACLAR,KAPSAMI,DENEYIM,TRAFIK,MOTOR,ILK_YARDIM,DIREKSIYON,TEORI_HARC,DRKS_HARC,UCRET)
VALUES
(@mevcut,@yeni,@yas,@arac,@kapsam,@deneyim,@trafik,@motor,@ilkYardim,@direksiyon,@teoriHarc,@drksHarc,@ucret)";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(updateMode ? updateSql : insertSql, conn))
            {
                cmd.Parameters.Add("@mevcut", SqlDbType.NVarChar, 50).Value = SeciliMetin(cmbMevcutSinif);
                cmd.Parameters.Add("@yeni", SqlDbType.NVarChar, 50).Value = (txtYeniSinif.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@yas", SqlDbType.NVarChar, 20).Value = (txtYasSiniri.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@arac", SqlDbType.NVarChar, 300).Value = (txtKullandigiAraclar.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@kapsam", SqlDbType.NVarChar, 300).Value = (txtKapsam.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@deneyim", SqlDbType.NVarChar, 300).Value = (txtDeneyimSarti.Text ?? string.Empty).Trim();
                cmd.Parameters.Add("@trafik", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbTrafikCevre);
                cmd.Parameters.Add("@motor", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbMotorAracTek);
                cmd.Parameters.Add("@ilkYardim", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbIlkYardim);
                cmd.Parameters.Add("@direksiyon", SqlDbType.NVarChar, 10).Value = SeciliMetin(cmbDireksiyonDers);
                cmd.Parameters.Add("@teoriHarc", SqlDbType.Money).Value = ParseMoney(txtTeoriSinavHarci.Text);
                cmd.Parameters.Add("@drksHarc", SqlDbType.Money).Value = ParseMoney(txtDireksiyonSinavHarci.Text);
                cmd.Parameters.Add("@ucret", SqlDbType.Money).Value = ParseMoney(txtTabanFiyat.Text);
                if (updateMode)
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private bool TryGetSelectedId(out int id)
        {
            id = 0;
            var cur = dgvSiniflar.CurrentRow;
            if (cur == null || cur.IsNewRow || cur.Index < 0 || !(cur.DataBoundItem is DataRowView drv))
                return false;
            if (!KolonVar(drv.Row.Table, "ID") || drv["ID"] == DBNull.Value)
                return false;
            return int.TryParse(Convert.ToString(drv["ID"], Tr), NumberStyles.Integer, Tr, out id);
        }

        private static string SeciliMetin(ComboBox combo)
        {
            var s = combo.SelectedItem == null ? combo.Text : combo.SelectedItem.ToString();
            return (s ?? string.Empty).Trim();
        }

        private static decimal ParseMoney(string text)
        {
            var s = (text ?? "0").Trim();
            if (string.IsNullOrWhiteSpace(s))
                return 0m;
            decimal value;
            if (decimal.TryParse(s, NumberStyles.Any, Tr, out value))
                return value;
            if (decimal.TryParse(s.Replace(".", ","), NumberStyles.Any, Tr, out value))
                return value;
            return 0m;
        }

        private void HesapAlanDegisti(object sender, EventArgs e)
        {
            RecalculateAll();
        }

        private void RecalculateAll()
        {
            if (_hesaplamaIci)
                return;

            try
            {
                _hesaplamaIci = true;

                // cmbTrafikCevre + cmbMotorAracTek + cmbIlkYardim + cmbTrafikAdabi = txtTeoriDersToplam
                int teoriToplamSaat = ParseIntCombo(cmbTrafikCevre)
                                     + ParseIntCombo(cmbMotorAracTek)
                                     + ParseIntCombo(cmbIlkYardim)
                                     + ParseIntCombo(cmbTrafikAdabi);
                txtTeoriDersToplam.Text = teoriToplamSaat.ToString(Tr);

                // cmbSimulatorAlan + cmbDireksiyonDers = cmbDireksiyonDersToplam
                int direksiyonToplamSaat = ParseIntCombo(cmbSimulatorAlan) + ParseIntCombo(cmbDireksiyonDers);
                string direksiyonToplamText = direksiyonToplamSaat.ToString(Tr);
                int idx = cmbDireksiyonDersToplam.FindStringExact(direksiyonToplamText);
                if (idx >= 0)
                    cmbDireksiyonDersToplam.SelectedIndex = idx;
                else
                {
                    cmbDireksiyonDersToplam.Items.Add(direksiyonToplamText);
                    cmbDireksiyonDersToplam.SelectedItem = direksiyonToplamText;
                }

                // cmbDireksiyonDersToplam * txtDireksiyonSaatlikUcret = txtDireksiyonDersUcreti
                decimal drToplam = ParseIntCombo(cmbDireksiyonDersToplam);
                decimal drSaatlik = ParseMoney(txtDireksiyonSaatlikUcret.Text);
                decimal drUcreti = drToplam * drSaatlik;
                txtDireksiyonDersUcreti.Text = drUcreti.ToString("N2", Tr);

                // txtTeoriDersUcreti = txtTeoriDersToplam * txtTeoriSaatlikUcret
                decimal teoriSaat = ParseMoney(txtTeoriDersToplam.Text);
                decimal teoriSaatlik = ParseMoney(txtTeoriSaatlikUcret.Text);
                decimal teoriDersUcreti = teoriSaat * teoriSaatlik;
                txtTeoriDersUcreti.Text = teoriDersUcreti.ToString("N2", Tr);

                // txtTabanFiyat = txtDireksiyonDersUcreti + txtTeoriDersUcreti
                decimal taban = ParseMoney(txtDireksiyonDersUcreti.Text) + ParseMoney(txtTeoriDersUcreti.Text);
                txtTabanFiyat.Text = taban.ToString("N2", Tr);
            }
            finally
            {
                _hesaplamaIci = false;
            }
        }

        private static int ParseIntCombo(ComboBox c)
        {
            if (c == null)
                return 0;

            var s = c.SelectedItem == null ? c.Text : c.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            int val;
            return int.TryParse(s.Trim(), NumberStyles.Integer, Tr, out val) ? val : 0;
        }

        private static class ToolbarGlyph
        {
            public static Bitmap PlusOnGreen(int size)
            {
                var bmp = new Bitmap(size, size);
                using (var g = Graphics.FromImage(bmp))
                using (var bg = new SolidBrush(Color.FromArgb(46, 125, 50)))
                using (var pen = new Pen(Color.White, Math.Max(2f, size / 12f)))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(bg, 1, 1, size - 3, size - 3);
                    float m = size * 0.32f;
                    float c = size / 2f;
                    g.DrawLine(pen, c, m, c, size - m);
                    g.DrawLine(pen, m, c, size - m, c);
                }
                return bmp;
            }

            public static Bitmap CheckOnGreen(int size)
            {
                var bmp = new Bitmap(size, size);
                using (var g = Graphics.FromImage(bmp))
                using (var bg = new SolidBrush(Color.FromArgb(56, 142, 60)))
                using (var pen = new Pen(Color.White, Math.Max(2f, size / 10f)))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(bg, 1, 1, size - 3, size - 3);
                    pen.StartCap = pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    float s = size / 8f;
                    g.DrawLine(pen, s * 2.2f, size * 0.48f, size * 0.38f, size * 0.72f);
                    g.DrawLine(pen, size * 0.38f, size * 0.72f, size - s * 1.2f, s * 2f);
                }
                return bmp;
            }

            public static Bitmap XOnRed(int size)
            {
                var bmp = new Bitmap(size, size);
                using (var g = Graphics.FromImage(bmp))
                using (var bg = new SolidBrush(Color.FromArgb(198, 40, 40)))
                using (var pen = new Pen(Color.White, Math.Max(2f, size / 10f)))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(bg, 1, 1, size - 3, size - 3);
                    float m = size * 0.28f;
                    g.DrawLine(pen, m, m, size - m, size - m);
                    g.DrawLine(pen, size - m, m, m, size - m);
                }
                return bmp;
            }

            public static Bitmap XOnDarkRed(int size)
            {
                var bmp = new Bitmap(size, size);
                using (var g = Graphics.FromImage(bmp))
                using (var bg = new SolidBrush(Color.FromArgb(130, 20, 20)))
                using (var pen = new Pen(Color.White, Math.Max(2.2f, size / 9f)))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(bg, 1, 1, size - 3, size - 3);
                    float m = size * 0.26f;
                    g.DrawLine(pen, m, m, size - m, size - m);
                    g.DrawLine(pen, size - m, m, m, size - m);
                }
                return bmp;
            }

            public static Bitmap EditNote(int size)
            {
                var bmp = new Bitmap(size, size);
                using (var g = Graphics.FromImage(bmp))
                using (var paper = new SolidBrush(Color.FromArgb(250, 250, 250)))
                using (var border = new Pen(Color.FromArgb(90, 90, 90), 1f))
                using (var pencil = new Pen(Color.FromArgb(33, 150, 243), Math.Max(1.5f, size / 16f)))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    float pad = size * 0.18f;
                    g.FillRectangle(paper, pad, pad, size - pad * 2, size - pad * 2);
                    g.DrawRectangle(border, pad, pad, size - pad * 2, size - pad * 2);
                    pencil.StartCap = pencil.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    g.DrawLine(pencil, size * 0.55f, pad * 0.9f, pad * 0.8f, size * 0.65f);
                }
                return bmp;
            }

            public static Bitmap CopyDoc(int size)
            {
                var bmp = new Bitmap(size, size);
                using (var g = Graphics.FromImage(bmp))
                using (var pen = new Pen(Color.FromArgb(55, 71, 79), 1.2f))
                using (var fill = new SolidBrush(Color.FromArgb(236, 239, 241)))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    float w = size * 0.42f;
                    float h = size * 0.52f;
                    float x1 = size * 0.22f;
                    float y1 = size * 0.18f;
                    g.FillRectangle(fill, x1, y1, w, h);
                    g.DrawRectangle(pen, x1, y1, w, h);
                    float x2 = size * 0.38f;
                    float y2 = size * 0.28f;
                    g.FillRectangle(Brushes.White, x2, y2, w, h);
                    g.DrawRectangle(pen, x2, y2, w, h);
                }
                return bmp;
            }
        }
    }
}
