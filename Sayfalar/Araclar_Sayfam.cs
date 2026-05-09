using Kolera.Araclar.Services;
using Kolera.Araclar.Services.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Araclar_Sayfam : Form
    {
        private readonly AracService _aracService;
        private readonly string _connectionString;
        private int _seciliID = 0;
        private readonly Dictionary<string, TextBox> _detayTextMap = new Dictionary<string, TextBox>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, TextBox> _bilgiTextMap = new Dictionary<string, TextBox>(StringComparer.OrdinalIgnoreCase);
        private DataTable _gridKaynak;

        public Araclar_Sayfam() : this(string.Empty)
        {
        }

        public Araclar_Sayfam(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? string.Empty;

            // Designer acilirken runtime baglanti/servis kodlari calismasin.
            if (IsDesignerMode())
                return;

            _aracService = new AracService(connectionString);

            Load += Araclar_Sayfam_Load;
            txtDetayAracPlaka.TextChanged += Tnk_Plaka_TextChanged;
            Btn_Yeniekle.Click += Btn_Yeniekle_Click;
            Btn_Save.Click += Btn_Save_Click;
           
           

            Dvg_Araclar.CellClick += Dvg_Araclar_CellClick;
            Dvg_Araclar.RowPrePaint += Dvg_Araclar_RowPrePaint;
            Dvg_Araclar.DataBindingComplete += Dvg_Araclar_DataBindingComplete;
        }

        #region LOAD
        private async void Araclar_Sayfam_Load(object sender, EventArgs e)
        {
            if (IsDesignerMode())
                return;

            GridAyarla();
            ComboBoxlariDoldur();
            EkDetayAlanlariniHazirla();
            EkBilgiAlanlariniHazirla();
            if (txtArama != null)
                txtArama.TextChanged += (s, args) => GridFiltrele();
            await PlakasizAraclariTemizleAsync();
            await AraclariYukle();
        }
        #endregion

        #region GRID
       
        private void Tnk_Plaka_TextChanged(object sender, EventArgs e)
        {
            label_Plaka.Text = (txtDetayAracPlaka.Text ?? string.Empty).ToUpper();
        }
        private async Task AraclariYukle()
        {
            try
            {
                DataTable dt = await GetAraclarSafeAsync();

                if (!dt.Columns.Contains("AKT_DURUM"))
                {
                    dt.Columns.Add("AKT_DURUM", typeof(string));
                    foreach (DataRow row in dt.Rows)
                    {
                        int akt = 0;
                        if (dt.Columns.Contains("AKT") && row["AKT"] != DBNull.Value)
                            akt = Convert.ToInt32(row["AKT"]);

                        row["AKT_DURUM"] = akt == 1 ? "Aktif" : "Pasif";
                    }
                }

                EnsureDisplayColumns(dt);

                DataView dv = dt.DefaultView;
                if (dt.Columns.Contains("AKT") && dt.Columns.Contains("ARAC_PLAKA"))
                    dv.Sort = "AKT DESC, ARAC_PLAKA ASC";
                else if (dt.Columns.Contains("ARAC_PLAKA"))
                    dv.Sort = "ARAC_PLAKA ASC";

                _gridKaynak = dv.ToTable();
                GridFiltrele();
                Dvg_Araclar.ClearSelection();
            }
            catch (Exception ex)
            {
                Dvg_Araclar.DataSource = new DataTable();
                MessageBox.Show($"Araç listesi yüklenemedi.\n{ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task PlakasizAraclariTemizleAsync()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            int silinenToplam = await Task.Run(() =>
            {
                int toplam = 0;
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    string tableName = ResolveAracTableName(con);
                    if (string.IsNullOrWhiteSpace(tableName))
                        return 0;

                    var cols = GetTableColumns(con, tableName);
                    string plakaKolonu = cols.Contains("ARAC_PLAKA")
                        ? "ARAC_PLAKA"
                        : (cols.Contains("PLAKA") ? "PLAKA" : string.Empty);

                    if (string.IsNullOrWhiteSpace(plakaKolonu))
                        return 0;

                    string sql = "DELETE FROM " + tableName + " WHERE ISNULL(LTRIM(RTRIM([" + plakaKolonu + "])), '') = '';";
                    using (var cmd = new SqlCommand(sql, con))
                        toplam += cmd.ExecuteNonQuery();
                }
                return toplam;
            });

            if (silinenToplam > 0)
            {
                MessageBox.Show(
                    silinenToplam + " adet plakasiz arac kaydi otomatik silindi.",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private async Task<DataTable> GetAraclarSafeAsync()
        {
            try
            {
                DataTable dt = await _aracService.GetAraclarAsync();
                if (!BeklenenKolonlarVar(dt))
                    return await GetAraclarDirectAsync();

                // SP eski/uyumsuz sürümlerde boş dönebiliyor; doğrudan SQL ile tekrar dene.
                if (dt.Rows.Count == 0)
                {
                    DataTable direct = await GetAraclarDirectAsync();
                    if (direct != null && direct.Rows.Count > 0)
                        return direct;
                }

                return dt;
            }
            catch
            {
                return await GetAraclarDirectAsync();
            }
        }

        private Task<DataTable> GetAraclarDirectAsync()
        {
            return Task.Run(() =>
            {
                var dt = new DataTable();
                if (string.IsNullOrWhiteSpace(_connectionString))
                    return dt;

                string[] sqlAdaylari =
                {
                    @"SELECT ID, ARAC_TIPI, ARAC_PLAKA, ARAC_ACIKLAMASI, DURUMU, KULLANIM, MARKASI, RENGI, VITES_TURU, YAKIT_TIPI, MODEL, MODEL_YILI, ARAC_TESCIL_TAR, MUHAYENE_TAR, ISNULL(AKT,1) AS AKT
                      FROM AracParam
                      ORDER BY ISNULL(AKT,1) DESC, ARAC_PLAKA ASC",
                    @"SELECT ID, ARAC_TIPI, ARAC_PLAKA, ARAC_ACIKLAMASI, DURUMU, KULLANIM, MARKASI, RENGI, VITES_TURU, YAKIT_TIPI, MODEL, MODEL_YILI, ARAC_TESCIL_TAR, MUHAYENE_TAR, ISNULL(AKT,1) AS AKT
                      FROM dbo.AracParam
                      ORDER BY ISNULL(AKT,1) DESC, ARAC_PLAKA ASC",
                    @"SELECT ID, ARAC_TIPI, ARAC_PLAKA, ARAC_ACIKLAMASI, DURUMU, KULLANIM, MARKASI, RENGI, VITES_TURU, YAKIT_TIPI, MODEL, MODEL_YILI, ARAC_TESCIL_TAR, HIZ_BAS_TAR, MUHAYENE_TAR, SIGORTA_BAS_TAR, SIGORTA_BEL_NO, KASKO_BAS_TAR, KASKO_BIT_TAR, KASKO_ISL_BEDELI, SIGORTA_BIT_TAR, ISNULL(AKT,1) AS AKT
                      FROM AracParam
                      ORDER BY ISNULL(AKT,1) DESC, ARAC_PLAKA ASC",
                    @"SELECT ID, ARAC_TIPI, ARAC_PLAKA, ARAC_ACIKLAMASI, DURUMU, KULLANIM, MARKASI, RENGI, VITES_TURU, YAKIT_TIPI, MODEL, MODEL_YILI, ARAC_TESCIL_TAR, HIZ_BAS_TAR, MUHAYENE_TAR, SIGORTA_BAS_TAR, SIGORTA_BEL_NO, KASKO_BAS_TAR, KASKO_BIT_TAR, KASKO_ISL_BEDELI, SIGORTA_BIT_TAR, ISNULL(AKT,1) AS AKT
                      FROM dbo.AracParam
                      ORDER BY ISNULL(AKT,1) DESC, ARAC_PLAKA ASC",
                    @"SELECT ID, ARAC_TIPI, ARAC_PLAKA, DURUMU, MARKASI, RENGI, VITES_TURU, MODEL, MUHAYENE_TAR, ISNULL(AKT,1) AS AKT
                      FROM AracParam
                      ORDER BY ISNULL(AKT,1) DESC, ARAC_PLAKA ASC",
                    @"SELECT ID, ARAC_TIPI, ARAC_PLAKA, DURUMU, MARKASI, RENGI, VITES_TURU, MODEL, MUHAYENE_TAR, ISNULL(AKT,1) AS AKT
                      FROM dbo.AracParam
                      ORDER BY ISNULL(AKT,1) DESC, ARAC_PLAKA ASC",
                    @"SELECT ID, ARAC_TIPI, ARAC_PLAKA, DURUMU, MARKASI, RENGI, VITES_TURU, MODEL, MUHAYENE_TAR, 1 AS AKT
                      FROM PARAM_ARAC_TANIMLARI
                      ORDER BY ARAC_PLAKA ASC",
                    @"SELECT ID, ARAC_TIPI, ARAC_PLAKA, DURUMU, MARKASI, RENGI, VITES_TURU, MODEL, MUHAYENE_TAR, 1 AS AKT
                      FROM dbo.PARAM_ARAC_TANIMLARI
                      ORDER BY ARAC_PLAKA ASC",
                    @"SELECT ID, ARAC_TIPI, ISNULL(PLAKA,'') AS ARAC_PLAKA, DURUMU, MARKASI, RENGI, VITES_TURU, MODEL, MUHAYENE_TAR, ISNULL(AKT,1) AS AKT
                      FROM AracParam
                      ORDER BY ISNULL(AKT,1) DESC, PLAKA ASC"
                };

                foreach (string sql in sqlAdaylari)
                {
                    try
                    {
                        using (var con = new SqlConnection(_connectionString))
                        using (var cmd = new SqlCommand(sql, con))
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var tmp = new DataTable();
                            con.Open();
                            da.Fill(tmp);
                            if (!BeklenenKolonlarVar(tmp))
                                continue;
                            return tmp;
                        }
                    }
                    catch
                    {
                        // Sonraki SQL adayı denensin.
                    }
                }

                return dt;
            });
        }


        private void Dvg_Araclar_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = Dvg_Araclar.Rows[e.RowIndex];
            if (!Dvg_Araclar.Columns.Contains("AKT") || row.Cells["AKT"].Value == DBNull.Value) return;

            int akt = Convert.ToInt32(row.Cells["AKT"].Value);
            row.DefaultCellStyle.BackColor = akt == 1 ? Color.LightGreen : Color.LightCoral;
        }


        private void Dvg_Araclar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = Dvg_Araclar.Rows[e.RowIndex];

            _seciliID = row.Cells["ID"].Value != DBNull.Value ? Convert.ToInt32(row.Cells["ID"].Value) : 0;
            Cmb_Turu.Text = row.Cells["ARAC_TIPI"].Value?.ToString() ?? "";
            Cmb_Vites.Text = row.Cells["VITES_TURU"].Value?.ToString() ?? "";
            Cmb_Aracdurum.Text = row.Cells["DURUMU"].Value?.ToString() ?? "";

            SetDetayText("ARAC_TIPI", GetCellString(row, "ARAC_TIPI"));
            SetDetayText("ARAC_PLAKA", GetCellString(row, "ARAC_PLAKA"));
            SetDetayText("ARAC_ACIKLAMASI", GetCellString(row, "ARAC_ACIKLAMASI"));
            SetDetayText("DURUMU", GetCellString(row, "DURUMU"));
            SetDetayText("KULLANIM", GetCellString(row, "KULLANIM"));
            SetDetayText("MARKASI", GetCellString(row, "MARKASI"));
            SetDetayText("RENGI", GetCellString(row, "RENGI"));
            SetDetayText("VITES_TURU", GetCellString(row, "VITES_TURU"));
            SetDetayText("YAKIT_TIPI", GetCellString(row, "YAKIT_TIPI"));
            SetDetayText("MODEL", GetCellString(row, "MODEL"));
            SetDetayText("MODEL_YILI", GetCellString(row, "MODEL_YILI"));
            SetDetayText("ARAC_TESCIL_TAR", GetCellDateText(row, "ARAC_TESCIL_TAR"));

            SetBilgiText("HIZ_BAS_TAR", GetCellDateText(row, "HIZ_BAS_TAR"));
            SetBilgiText("MUHAYENE_TAR", GetCellDateText(row, "MUHAYENE_TAR"));
            SetBilgiText("SIGORTA_BAS_TAR", GetCellDateText(row, "SIGORTA_BAS_TAR"));
            SetBilgiText("SIGORTA_BEL_NO", GetCellString(row, "SIGORTA_BEL_NO"));
            SetBilgiText("KASKO_BAS_TAR", GetCellDateText(row, "KASKO_BAS_TAR"));
            SetBilgiText("KASKO_BIT_TAR", GetCellDateText(row, "KASKO_BIT_TAR"));
            SetBilgiText("KASKO_ISL_BEDELI", GetCellString(row, "KASKO_ISL_BEDELI"));
            SetBilgiText("SIGORTA_BIT_TAR", GetCellDateText(row, "SIGORTA_BIT_TAR"));
        }
        #endregion

        #region COMBO
        private void ComboBoxlariDoldur()
        {
            Cmb_Aracdurum.Items.Clear();
            Cmb_Turu.Items.Clear();
            Cmb_Vites.Items.Clear();
            Cmb_Aracdurum.Items.AddRange(new[] { "Aktif", "Pasif", "Bakımda" });
            Cmb_Turu.Items.AddRange(new[] { "Otomobil", "Kamyon", "Minibüs", "Motosiklet" });
            Cmb_Vites.Items.AddRange(new[] { "Manuel", "Otomatik", "Yarı Otomatik" });
        }
        #endregion

        private void GridFiltrele()
        {
            if (_gridKaynak == null)
                return;

            string ara = (txtArama?.Text ?? string.Empty).Trim();
            IEnumerable<DataRow> rows = _gridKaynak.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ara))
            {
                string q = ara.ToUpperInvariant();
                rows = rows.Where(r =>
                    (r.Table.Columns.Contains("ARAC_PLAKA") ? Convert.ToString(r["ARAC_PLAKA"]) : string.Empty).ToUpperInvariant().Contains(q) ||
                    (r.Table.Columns.Contains("ARAC_TIPI") ? Convert.ToString(r["ARAC_TIPI"]) : string.Empty).ToUpperInvariant().Contains(q) ||
                    (r.Table.Columns.Contains("MARKASI") ? Convert.ToString(r["MARKASI"]) : string.Empty).ToUpperInvariant().Contains(q) ||
                    (r.Table.Columns.Contains("MODEL") ? Convert.ToString(r["MODEL"]) : string.Empty).ToUpperInvariant().Contains(q) ||
                    (r.Table.Columns.Contains("DURUMU") ? Convert.ToString(r["DURUMU"]) : string.Empty).ToUpperInvariant().Contains(q));
            }

            Dvg_Araclar.DataSource = rows.Any() ? rows.CopyToDataTable() : _gridKaynak.Clone();
        }

        #region BUTTONS
        private void Btn_Yeniekle_Click(object sender, EventArgs e) => Temizle();

        private async void Btn_Save_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDetayAracPlaka.Text))
            {
                MessageBox.Show("Plaka boş olamaz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Arac_Model arac = new Arac_Model
            {
                ID = _seciliID,
                ARAC_PLAKA = (txtDetayAracPlaka.Text ?? string.Empty).Trim(),
                ARAC_TIPI = (Cmb_Turu.Text ?? string.Empty).Trim(),
                DURUMU = (Cmb_Aracdurum.Text ?? string.Empty).Trim(),
                RENGI = (txtDetayRengi.Text ?? string.Empty).Trim(),
                VITES_TURU = (Cmb_Vites.Text ?? string.Empty).Trim(),
                MODEL = (txtDetayModel.Text ?? string.Empty).Trim(),
                MUHAYENE_TAR = ParseNullableDate(txtBilgiMuayeneTar.Text),
                AKT = Cmb_Aracdurum.Text == "Pasif" ? 0 : 1
            };

            var kayitData = new AracKayitData
            {
                AracAciklamasi = (txtDetayAracAciklama.Text ?? string.Empty).Trim(),
                Kullanim = (txtDetayKullanim.Text ?? string.Empty).Trim(),
                Markasi = (txtDetayMarkasi.Text ?? string.Empty).Trim(),
                YakitTipi = (txtDetayYakitTipi.Text ?? string.Empty).Trim(),
                ModelYili = (txtDetayModelYili.Text ?? string.Empty).Trim(),
                AracTescilTar = ParseNullableDate(txtDetayAracTescilTar.Text),
                HizBasTar = ParseNullableDate(txtBilgiHizBasTar.Text),
                SigortaBasTar = ParseNullableDate(txtBilgiSigortaBasTar.Text),
                SigortaBelNo = (txtBilgiSigortaBelNo.Text ?? string.Empty).Trim(),
                KaskoBasTar = ParseNullableDate(txtBilgiKaskoBasTar.Text),
                KaskoBitTar = ParseNullableDate(txtBilgiKaskoBitTar.Text),
                KaskoIslBedeli = (txtBilgiKaskoIslBedeli.Text ?? string.Empty).Trim(),
                SigortaBitTar = ParseNullableDate(txtBilgiSigortaBitTar.Text)
            };

            try
            {
                if (_seciliID == 0)
                    await AddAracSafeAsync(arac, kayitData);
                else
                    await UpdateAracSafeAsync(arac, kayitData);

                await AraclariYukle();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

      

        private async void Btn_PasifYap_Click(object sender, EventArgs e)
        {
            if (_seciliID == 0) return;

            var onay = MessageBox.Show("Seçilen araç pasif yapılacak. Devam etmek istiyor musunuz?",
                "Pasif Yap Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (onay != DialogResult.Yes) return;

            try
            {
                await PasifYapSafeAsync(_seciliID);
                await AraclariYukle();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
        private void GridAyarla()
        {
            Dvg_Araclar.AutoGenerateColumns = true;
            Dvg_Araclar.ReadOnly = true;
            Dvg_Araclar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dvg_Araclar.MultiSelect = false;
            Dvg_Araclar.AllowUserToAddRows = false;
            Dvg_Araclar.AllowUserToDeleteRows = false;
            Dvg_Araclar.RowHeadersVisible = false;

            // 🔴 Eski sayfa düzeni
            Dvg_Araclar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            Dvg_Araclar.AllowUserToResizeColumns = false;
            Dvg_Araclar.ColumnHeadersHeight = 40;
            Dvg_Araclar.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            Dvg_Araclar.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private void Dvg_Araclar_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Kolon gizleme ve başlık ayarlama
            Gizle("ID");
            Gizle("AKT");

            Baslik("DURUMU", "Durumu", 100);
            Baslik("ARAC_PLAKA", "Plaka", 120);
            Baslik("ARAC_TIPI", "Araç Tipi", 140);
            Baslik("MODEL", "Model", 110);
            Baslik("RENGI", "Renk", 90);
            Baslik("VITES_TURU", "Vites", 110);

            if (Dvg_Araclar.Columns.Contains("MUHAYENE_TAR"))
            {
                var col = Dvg_Araclar.Columns["MUHAYENE_TAR"];
                col.HeaderText = "Muayene";
                col.Width = 120;
                col.DefaultCellStyle.Format = "dd.MM.yyyy";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // 🔒 Sort ikonlarını kapat
            foreach (DataGridViewColumn col in Dvg_Araclar.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void Gizle(string col)
        {
            if (Dvg_Araclar.Columns.Contains(col))
                Dvg_Araclar.Columns[col].Visible = false;
        }

        private void Baslik(string col, string text, int width)
        {
            if (Dvg_Araclar.Columns.Contains(col))
            {
                Dvg_Araclar.Columns[col].HeaderText = text;
                Dvg_Araclar.Columns[col].Width = width;
            }
        }
        #region HELPERS
        private void Temizle()
        {
            _seciliID = 0;
            txtDetayAracPlaka.Clear();
            txtDetayModel.Clear();
            txtDetayRengi.Clear();
            txtBilgiMuayeneTar.Clear();
            Cmb_Turu.Text = "";
            Cmb_Vites.Text = "";
            Cmb_Aracdurum.Text = "";
            foreach (var tb in _detayTextMap.Values)
                tb.Clear();
            foreach (var tb in _bilgiTextMap.Values)
                tb.Clear();
        }

        #endregion

        private async void Btn_Sil_Click(object sender, EventArgs e)
        {
            if (_seciliID == 0) return;

            var onay = MessageBox.Show(
                "Seçilen araç pasif yapılacak (silinmeyecek). Devam etmek istiyor musunuz?",
                "Pasif Yap Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (onay != DialogResult.Yes) return;

            try
            {
                await PasifYapSafeAsync(_seciliID);
                await AraclariYukle();
                Temizle();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task AddAracSafeAsync(Arac_Model arac, AracKayitData kayitData)
        {
            try
            {
                await _aracService.AddAracAsync(arac);
                await UpdateAracEkAlanlariByPlakaDirectAsync(arac.ARAC_PLAKA, kayitData);
            }
            catch
            {
                await AddAracDirectAsync(arac, kayitData);
            }
        }

        private async Task UpdateAracSafeAsync(Arac_Model arac, AracKayitData kayitData)
        {
            try
            {
                await _aracService.UpdateAracAsync(arac);
                await UpdateAracEkAlanlariByIdDirectAsync(arac.ID, kayitData);
            }
            catch
            {
                await UpdateAracDirectAsync(arac, kayitData);
            }
        }

        private async Task PasifYapSafeAsync(int id)
        {
            try
            {
                await _aracService.PasifYapAsync(id);
            }
            catch
            {
                await PasifYapDirectAsync(id);
            }
        }

        private Task AddAracDirectAsync(Arac_Model arac, AracKayitData kayitData)
        {
            return Task.Run(() =>
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    string tableName = ResolveAracTableName(con);
                    if (string.IsNullOrEmpty(tableName))
                        throw new InvalidOperationException("Arac tablosu bulunamadi.");

                    var cols = GetTableColumns(con, tableName);
                    var insertCols = new List<string>();
                    var insertVals = new List<string>();
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "ARAC_TIPI", arac.ARAC_TIPI);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "ARAC_PLAKA", arac.ARAC_PLAKA);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "ARAC_ACIKLAMASI", kayitData.AracAciklamasi);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "DURUMU", arac.DURUMU);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "KULLANIM", kayitData.Kullanim);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "MARKASI", kayitData.Markasi);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "RENGI", arac.RENGI);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "VITES_TURU", arac.VITES_TURU);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "YAKIT_TIPI", kayitData.YakitTipi);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "MODEL", arac.MODEL);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "MODEL_YILI", kayitData.ModelYili);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "ARAC_TESCIL_TAR", kayitData.AracTescilTar);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "HIZ_BAS_TAR", kayitData.HizBasTar);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "MUHAYENE_TAR", arac.MUHAYENE_TAR);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "SIGORTA_BAS_TAR", kayitData.SigortaBasTar);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "SIGORTA_BEL_NO", kayitData.SigortaBelNo);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "KASKO_BAS_TAR", kayitData.KaskoBasTar);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "KASKO_BIT_TAR", kayitData.KaskoBitTar);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "KASKO_ISL_BEDELI", kayitData.KaskoIslBedeli);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "SIGORTA_BIT_TAR", kayitData.SigortaBitTar);
                        AddIfColumnExists(cmd, cols, insertCols, insertVals, "AKT", arac.AKT);

                        if (insertCols.Count == 0)
                            throw new InvalidOperationException("Kaydedilecek kolon bulunamadi.");

                        cmd.CommandText = "INSERT INTO " + tableName + " (" + string.Join(",", insertCols) + ") VALUES (" + string.Join(",", insertVals) + ");";
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        private Task UpdateAracDirectAsync(Arac_Model arac, AracKayitData kayitData)
        {
            return Task.Run(() =>
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    string tableName = ResolveAracTableName(con);
                    if (string.IsNullOrEmpty(tableName))
                        throw new InvalidOperationException("Arac tablosu bulunamadi.");

                    var cols = GetTableColumns(con, tableName);
                    if (!cols.Contains("ID"))
                        throw new InvalidOperationException("Guncelleme icin ID kolonu bulunamadi.");

                    var sets = new List<string>();
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        AddIfColumnExists(cmd, cols, sets, "ARAC_TIPI", arac.ARAC_TIPI);
                        AddIfColumnExists(cmd, cols, sets, "ARAC_PLAKA", arac.ARAC_PLAKA);
                        AddIfColumnExists(cmd, cols, sets, "ARAC_ACIKLAMASI", kayitData.AracAciklamasi);
                        AddIfColumnExists(cmd, cols, sets, "DURUMU", arac.DURUMU);
                        AddIfColumnExists(cmd, cols, sets, "KULLANIM", kayitData.Kullanim);
                        AddIfColumnExists(cmd, cols, sets, "MARKASI", kayitData.Markasi);
                        AddIfColumnExists(cmd, cols, sets, "RENGI", arac.RENGI);
                        AddIfColumnExists(cmd, cols, sets, "VITES_TURU", arac.VITES_TURU);
                        AddIfColumnExists(cmd, cols, sets, "YAKIT_TIPI", kayitData.YakitTipi);
                        AddIfColumnExists(cmd, cols, sets, "MODEL", arac.MODEL);
                        AddIfColumnExists(cmd, cols, sets, "MODEL_YILI", kayitData.ModelYili);
                        AddIfColumnExists(cmd, cols, sets, "ARAC_TESCIL_TAR", kayitData.AracTescilTar);
                        AddIfColumnExists(cmd, cols, sets, "HIZ_BAS_TAR", kayitData.HizBasTar);
                        AddIfColumnExists(cmd, cols, sets, "MUHAYENE_TAR", arac.MUHAYENE_TAR);
                        AddIfColumnExists(cmd, cols, sets, "SIGORTA_BAS_TAR", kayitData.SigortaBasTar);
                        AddIfColumnExists(cmd, cols, sets, "SIGORTA_BEL_NO", kayitData.SigortaBelNo);
                        AddIfColumnExists(cmd, cols, sets, "KASKO_BAS_TAR", kayitData.KaskoBasTar);
                        AddIfColumnExists(cmd, cols, sets, "KASKO_BIT_TAR", kayitData.KaskoBitTar);
                        AddIfColumnExists(cmd, cols, sets, "KASKO_ISL_BEDELI", kayitData.KaskoIslBedeli);
                        AddIfColumnExists(cmd, cols, sets, "SIGORTA_BIT_TAR", kayitData.SigortaBitTar);
                        AddIfColumnExists(cmd, cols, sets, "AKT", arac.AKT);
                        cmd.Parameters.AddWithValue("@ID", arac.ID);

                        if (sets.Count == 0)
                            throw new InvalidOperationException("Guncellenecek kolon bulunamadi.");

                        cmd.CommandText = "UPDATE " + tableName + " SET " + string.Join(",", sets) + " WHERE ID=@ID;";
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        private Task PasifYapDirectAsync(int id)
        {
            return Task.Run(() =>
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    string tableName = ResolveAracTableName(con);
                    if (string.IsNullOrEmpty(tableName))
                        throw new InvalidOperationException("Arac tablosu bulunamadi.");
                    using (var cmd = new SqlCommand("UPDATE " + tableName + " SET AKT=0, DURUMU='Pasif' WHERE ID=@ID;", con))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        private static bool BeklenenKolonlarVar(DataTable dt)
        {
            if (dt == null)
                return false;

            return dt.Columns.Contains("ID")
                   && dt.Columns.Contains("ARAC_PLAKA");
        }

        private static void EnsureDisplayColumns(DataTable dt)
        {
            string[] required =
            {
                "ARAC_TIPI","ARAC_PLAKA","ARAC_ACIKLAMASI","DURUMU","KULLANIM",
                "MARKASI","RENGI","VITES_TURU","YAKIT_TIPI","MODEL","MODEL_YILI","ARAC_TESCIL_TAR",
                "HIZ_BAS_TAR","MUHAYENE_TAR","SIGORTA_BAS_TAR","SIGORTA_BEL_NO",
                "KASKO_BAS_TAR","KASKO_BIT_TAR","KASKO_ISL_BEDELI","SIGORTA_BIT_TAR"
            };

            foreach (var col in required)
            {
                if (!dt.Columns.Contains(col))
                    dt.Columns.Add(col, typeof(string));
            }
        }

        private void EkDetayAlanlariniHazirla()
        {
            if (_detayTextMap.Count > 0 || Tab_Arac == null)
                return;

            _detayTextMap["ARAC_PLAKA"] = txtDetayAracPlaka;
            _detayTextMap["ARAC_ACIKLAMASI"] = txtDetayAracAciklama;
            _detayTextMap["DURUMU"] = txtDetayDurumu;
            _detayTextMap["KULLANIM"] = txtDetayKullanim;
            _detayTextMap["MARKASI"] = txtDetayMarkasi;
            _detayTextMap["RENGI"] = txtDetayRengi;
            _detayTextMap["YAKIT_TIPI"] = txtDetayYakitTipi;
            _detayTextMap["MODEL"] = txtDetayModel;
            _detayTextMap["MODEL_YILI"] = txtDetayModelYili;
            _detayTextMap["ARAC_TESCIL_TAR"] = txtDetayAracTescilTar;

        }

        private void SetDetayText(string columnName, string value)
        {
            TextBox tb;
            if (_detayTextMap.TryGetValue(columnName, out tb))
                tb.Text = value ?? string.Empty;
        }

        private void EkBilgiAlanlariniHazirla()
        {
            if (_bilgiTextMap.Count > 0 || Tab_Bilgi == null)
                return;

            _bilgiTextMap["HIZ_BAS_TAR"] = txtBilgiHizBasTar;
            _bilgiTextMap["MUHAYENE_TAR"] = txtBilgiMuayeneTar;
            _bilgiTextMap["SIGORTA_BAS_TAR"] = txtBilgiSigortaBasTar;
            _bilgiTextMap["SIGORTA_BEL_NO"] = txtBilgiSigortaBelNo;
            _bilgiTextMap["KASKO_BAS_TAR"] = txtBilgiKaskoBasTar;
            _bilgiTextMap["KASKO_BIT_TAR"] = txtBilgiKaskoBitTar;
            _bilgiTextMap["KASKO_ISL_BEDELI"] = txtBilgiKaskoIslBedeli;
            _bilgiTextMap["SIGORTA_BIT_TAR"] = txtBilgiSigortaBitTar;
        }

        private void SetBilgiText(string columnName, string value)
        {
            TextBox tb;
            if (_bilgiTextMap.TryGetValue(columnName, out tb))
                tb.Text = value ?? string.Empty;
        }

        private static string GetCellString(DataGridViewRow row, string col)
        {
            if (!row.DataGridView.Columns.Contains(col))
                return string.Empty;
            if (row.Cells[col].Value == null || row.Cells[col].Value == DBNull.Value)
                return string.Empty;
            return Convert.ToString(row.Cells[col].Value);
        }

        private static string GetCellDateText(DataGridViewRow row, string col)
        {
            if (!row.DataGridView.Columns.Contains(col))
                return string.Empty;
            if (row.Cells[col].Value == null || row.Cells[col].Value == DBNull.Value)
                return string.Empty;
            DateTime dt;
            return DateTime.TryParse(Convert.ToString(row.Cells[col].Value), out dt) ? dt.ToString("dd.MM.yyyy") : Convert.ToString(row.Cells[col].Value);
        }

        private bool IsDesignerMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime
                   || (Site != null && Site.DesignMode)
                   || DesignMode;
        }

        private static DateTime? ParseNullableDate(string text)
        {
            string raw = (text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw))
                return null;
            DateTime dt;
            return DateTime.TryParse(raw, out dt) ? (DateTime?)dt.Date : null;
        }

        private static string ResolveAracTableName(SqlConnection con)
        {
            string[] adaylar = { "dbo.AracParam", "AracParam", "dbo.PARAM_ARAC_TANIMLARI", "PARAM_ARAC_TANIMLARI" };
            foreach (var t in adaylar)
            {
                using (var cmd = new SqlCommand("SELECT OBJECT_ID(@n)", con))
                {
                    cmd.Parameters.AddWithValue("@n", t);
                    object o = cmd.ExecuteScalar();
                    if (o != null && o != DBNull.Value && Convert.ToInt32(o) > 0)
                        return t.StartsWith("dbo.", StringComparison.OrdinalIgnoreCase) ? "[dbo].[" + t.Substring(4) + "]" : "[" + t + "]";
                }
            }
            return null;
        }

        private static HashSet<string> GetTableColumns(SqlConnection con, string quotedTableName)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new SqlCommand("SELECT TOP 0 * FROM " + quotedTableName, con))
            using (var r = cmd.ExecuteReader())
            {
                var schema = r.GetSchemaTable();
                foreach (DataRow row in schema.Rows)
                    set.Add(Convert.ToString(row["ColumnName"]));
            }
            return set;
        }

        private static void AddIfColumnExists(SqlCommand cmd, HashSet<string> cols, List<string> setClauses, string colName, object value)
        {
            if (!cols.Contains(colName))
                return;
            string p = "@P_" + colName;
            setClauses.Add("[" + colName + "]=" + p);
            cmd.Parameters.AddWithValue(p, value ?? DBNull.Value);
        }

        private static void AddIfColumnExists(SqlCommand cmd, HashSet<string> cols, List<string> insertCols, List<string> insertVals, string colName, object value)
        {
            if (!cols.Contains(colName))
                return;
            string p = "@P_" + colName;
            insertCols.Add("[" + colName + "]");
            insertVals.Add(p);
            cmd.Parameters.AddWithValue(p, value ?? DBNull.Value);
        }

        private Task UpdateAracEkAlanlariByIdDirectAsync(int id, AracKayitData kayitData)
        {
            if (id <= 0)
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    string tableName = ResolveAracTableName(con);
                    if (string.IsNullOrEmpty(tableName))
                        return;

                    var cols = GetTableColumns(con, tableName);
                    var sets = new List<string>();
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        AddIfColumnExists(cmd, cols, sets, "ARAC_ACIKLAMASI", kayitData.AracAciklamasi);
                        AddIfColumnExists(cmd, cols, sets, "KULLANIM", kayitData.Kullanim);
                        AddIfColumnExists(cmd, cols, sets, "MARKASI", kayitData.Markasi);
                        AddIfColumnExists(cmd, cols, sets, "YAKIT_TIPI", kayitData.YakitTipi);
                        AddIfColumnExists(cmd, cols, sets, "MODEL_YILI", kayitData.ModelYili);
                        AddIfColumnExists(cmd, cols, sets, "ARAC_TESCIL_TAR", kayitData.AracTescilTar);
                        AddIfColumnExists(cmd, cols, sets, "HIZ_BAS_TAR", kayitData.HizBasTar);
                        AddIfColumnExists(cmd, cols, sets, "SIGORTA_BAS_TAR", kayitData.SigortaBasTar);
                        AddIfColumnExists(cmd, cols, sets, "SIGORTA_BEL_NO", kayitData.SigortaBelNo);
                        AddIfColumnExists(cmd, cols, sets, "KASKO_BAS_TAR", kayitData.KaskoBasTar);
                        AddIfColumnExists(cmd, cols, sets, "KASKO_BIT_TAR", kayitData.KaskoBitTar);
                        AddIfColumnExists(cmd, cols, sets, "KASKO_ISL_BEDELI", kayitData.KaskoIslBedeli);
                        AddIfColumnExists(cmd, cols, sets, "SIGORTA_BIT_TAR", kayitData.SigortaBitTar);
                        if (sets.Count == 0)
                            return;

                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.CommandText = "UPDATE " + tableName + " SET " + string.Join(",", sets) + " WHERE ID=@ID;";
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        private Task UpdateAracEkAlanlariByPlakaDirectAsync(string plaka, AracKayitData kayitData)
        {
            string p = (plaka ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(p))
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                using (var con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    string tableName = ResolveAracTableName(con);
                    if (string.IsNullOrEmpty(tableName))
                        return;

                    int id = 0;
                    using (var idCmd = new SqlCommand("SELECT TOP 1 ID FROM " + tableName + " WHERE UPPER(LTRIM(RTRIM(ARAC_PLAKA)))=UPPER(@P) ORDER BY ID DESC;", con))
                    {
                        idCmd.Parameters.AddWithValue("@P", p);
                        object o = idCmd.ExecuteScalar();
                        if (o != null && o != DBNull.Value)
                            id = Convert.ToInt32(o);
                    }

                    if (id > 0)
                        UpdateAracEkAlanlariByIdDirectAsync(id, kayitData).GetAwaiter().GetResult();
                }
            });
        }

        private sealed class AracKayitData
        {
            public string AracAciklamasi { get; set; }
            public string Kullanim { get; set; }
            public string Markasi { get; set; }
            public string YakitTipi { get; set; }
            public string ModelYili { get; set; }
            public DateTime? AracTescilTar { get; set; }
            public DateTime? HizBasTar { get; set; }
            public DateTime? SigortaBasTar { get; set; }
            public string SigortaBelNo { get; set; }
            public DateTime? KaskoBasTar { get; set; }
            public DateTime? KaskoBitTar { get; set; }
            public string KaskoIslBedeli { get; set; }
            public DateTime? SigortaBitTar { get; set; }
        }
    }
    }
    
