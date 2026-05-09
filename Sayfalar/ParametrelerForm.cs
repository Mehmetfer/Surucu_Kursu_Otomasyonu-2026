using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Windows.Forms;
using Kolera_Mtsk.Services;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class ParametrelerForm : Form
    {
        private readonly string _connectionString;
        private DataTable _kullaniciTable;
        private KullaniciTanimlariForm _kullaniciTanimlariForm;
        private SonHaliSertifikaUcretForm _sonHaliSertifikaUcretForm;
        private Panel _pnlLogGridHost;
        private DataGridView _dgvLogKayitlari;
        private TabPage _tabParametreTabloYonetimi;
        private Panel _pnlParamHeader;
        private SplitContainer _splitParam;
        private ListBox _lstParamTablolar;
        private TabControl _tabParamTablePages;
        private Button _btnParamKaydet;
        private Button _btnParamYenile;
        private Button _btnParamSil;
        private Button _btnParamYeni;
        private TabPage _tabIstampa;
        private ComboBox _cmbIstampaAlan;
        private TextBox _txtIstampaAciklama;
        private PictureBox _picIstampa;
        private Label _lblIstampaBoyut;
        private Button _btnIstampaTara;
        private Button _btnIstampaDosya;
        private Button _btnIstampaKaydet;
        private byte[] _aktifIstampaResim;
        private readonly KoleraIstampaRepository _istampaRepo;
        private readonly System.Collections.Generic.Dictionary<string, DataTable> _paramDataTables =
            new System.Collections.Generic.Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Collections.Generic.Dictionary<string, SqlDataAdapter> _paramAdapters =
            new System.Collections.Generic.Dictionary<string, SqlDataAdapter>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Collections.Generic.Dictionary<string, DataGridView> _paramGridler =
            new System.Collections.Generic.Dictionary<string, DataGridView>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Collections.Generic.Dictionary<string, string> _paramGosterimAdlari =
            new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "app_parametreler", "Durum" },
                { "durum_param", "Arac Parametre1" },
                { "param_direksiyon", "Direksiyon" }
            };

        public ParametrelerForm() : this(string.Empty)
        {
        }

        public ParametrelerForm(string connectionString)
        {
            _connectionString = connectionString ?? string.Empty;
            _istampaRepo = new KoleraIstampaRepository(_connectionString);
            InitializeComponent();
            Shown += ParametrelerForm_Shown;
            tabMain.SelectedIndexChanged += tabMain_SelectedIndexChanged;
            btnLogListe.Click += btnLogListe_Click;
            btnLogTemizle.Click += btnLogTemizle_Click;
            btnLogExcel.Click += btnLogExcel_Click;
            btnBugunDoganlarYenile.Click += btnBugunDoganlarYenile_Click;
            btnBugunDoganlarSmsGonder.Click += btnBugunDoganlarSmsGonder_Click;
            KurSonHaliSertifikaGomulu();
            ParametreTabloYonetimSekmesiniHazirla();
            IstampaSekmesiniHazirla();
        }

        private void IstampaSekmesiniHazirla()
        {
            _tabIstampa = new TabPage
            {
                Text = "Istampa",
                BackColor = System.Drawing.Color.FromArgb(245, 247, 250),
                Padding = new Padding(10)
            };

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = System.Drawing.Color.White,
                Padding = new Padding(12)
            };
            var lblAlan = new Label { Text = "Alan", Left = 10, Top = 15, Width = 70 };
            _cmbIstampaAlan = new ComboBox
            {
                Left = 90,
                Top = 12,
                Width = 260,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbIstampaAlan.SelectedIndexChanged += (s, e) => IstampaSeciliAlaniYukle();

            var lblAciklama = new Label { Text = "Aciklama", Left = 10, Top = 50, Width = 70 };
            _txtIstampaAciklama = new TextBox { Left = 90, Top = 47, Width = 430 };

            _btnIstampaTara = new Button { Text = "Tarayici", Left = 540, Top = 10, Width = 120, Height = 28 };
            _btnIstampaDosya = new Button { Text = "Dosyadan Al", Left = 540, Top = 44, Width = 120, Height = 28 };
            _btnIstampaKaydet = new Button { Text = "Veritabanina Kaydet", Left = 670, Top = 10, Width = 170, Height = 62 };
            _btnIstampaTara.Click += (s, e) => IstampaTarat();
            _btnIstampaDosya.Click += (s, e) => IstampaDosyadanAl();
            _btnIstampaKaydet.Click += (s, e) => IstampaKaydet();

            _lblIstampaBoyut = new Label
            {
                Left = 90,
                Top = 79,
                Width = 430,
                Height = 20,
                ForeColor = System.Drawing.Color.DimGray,
                Text = "Resim yok"
            };

            pnlTop.Controls.Add(lblAlan);
            pnlTop.Controls.Add(_cmbIstampaAlan);
            pnlTop.Controls.Add(lblAciklama);
            pnlTop.Controls.Add(_txtIstampaAciklama);
            pnlTop.Controls.Add(_btnIstampaTara);
            pnlTop.Controls.Add(_btnIstampaDosya);
            pnlTop.Controls.Add(_btnIstampaKaydet);
            pnlTop.Controls.Add(_lblIstampaBoyut);

            _picIstampa = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = System.Drawing.Color.WhiteSmoke,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            _tabIstampa.Controls.Add(_picIstampa);
            _tabIstampa.Controls.Add(pnlTop);
            tabMain.TabPages.Add(_tabIstampa);
        }

        private void IstampaAlanlariniYukle()
        {
            if (_cmbIstampaAlan == null)
                return;

            DataTable dt = _istampaRepo.GetAll();
            _cmbIstampaAlan.DataSource = dt;
            _cmbIstampaAlan.DisplayMember = "ALAN_ADI";
            _cmbIstampaAlan.ValueMember = "ALAN_KODU";
            if (_cmbIstampaAlan.Items.Count > 0)
                _cmbIstampaAlan.SelectedIndex = 0;
        }

        private void IstampaSeciliAlaniYukle()
        {
            string kod = Convert.ToString(_cmbIstampaAlan?.SelectedValue) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kod))
                return;

            var dt = _cmbIstampaAlan?.DataSource as DataTable;
            if (!KoleraIstampaRepository.TryFindStampRow(dt, kod, out DataRow row))
                return;
            if (row.RowState == DataRowState.Detached)
                return;

            _txtIstampaAciklama.Text = Convert.ToString(KoleraIstampaRepository.GetColumnValue(row, "ACIKLAMA")) ?? string.Empty;
            object res = KoleraIstampaRepository.GetColumnValue(row, "RESIM");
            _aktifIstampaResim = (res == null || res == DBNull.Value) ? null : res as byte[];
            IstampaResmiGoster(_aktifIstampaResim);
        }

        private void IstampaTarat()
        {
            using (var frm = new Tarama_Sayfam(_aktifIstampaResim, Tarama_Sayfam.TaramaTipi.Imza, _connectionString))
            {
                frm.TaramaTamamlandi += bytes =>
                {
                    _aktifIstampaResim = bytes;
                    IstampaResmiGoster(_aktifIstampaResim);
                };
                frm.ShowDialog(this);
            }
        }

        private void IstampaDosyadanAl()
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Resim Dosyalari|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;
                _aktifIstampaResim = File.ReadAllBytes(ofd.FileName);
                IstampaResmiGoster(_aktifIstampaResim);
            }
        }

        private void IstampaKaydet()
        {
            string kod = Convert.ToString(_cmbIstampaAlan?.SelectedValue) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kod))
            {
                MessageBox.Show("Gecerli bir istampa alani seciniz.", "Uyari",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ad = Convert.ToString(_cmbIstampaAlan?.Text) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ad) && KoleraIstampaRepository.GetDefaultDefinitions().TryGetValue(kod, out string varsayilanAd))
                ad = varsayilanAd;

            if (_aktifIstampaResim == null || _aktifIstampaResim.Length == 0)
            {
                MessageBox.Show("Once tarama veya dosyadan bir istampa resmi yukleyin.", "Uyari",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_istampaRepo.SaveImage(kod, ad, _aktifIstampaResim, _txtIstampaAciklama.Text, out string hata))
            {
                MessageBox.Show(string.IsNullOrEmpty(hata) ? "Kayit basarisiz." : hata, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Istampa KOLERA_ISTAMPA tablosuna kaydedildi.", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            IstampaAlanlariniYukle();
        }

        private void IstampaResmiGoster(byte[] data)
        {
            if (_picIstampa == null)
                return;

            try
            {
                if (_picIstampa.Image != null)
                {
                    var old = _picIstampa.Image;
                    _picIstampa.Image = null;
                    old.Dispose();
                }

                if (data == null || data.Length == 0)
                {
                    _lblIstampaBoyut.Text = "Resim yok";
                    return;
                }

                using (var ms = new MemoryStream(data))
                using (var img = System.Drawing.Image.FromStream(ms))
                    _picIstampa.Image = new System.Drawing.Bitmap(img);

                _lblIstampaBoyut.Text = "Boyut: " + (data.Length / 1024d).ToString("0.##") + " KB";
            }
            catch
            {
                _lblIstampaBoyut.Text = "Resim okunamadi";
            }
        }

        private void ParametrelerForm_Shown(object sender, EventArgs e)
        {
            FormWorkspaceLayoutHelper.ApplyWorkingAreaMaximized(this);
            KurumsalTasarimiUygula();
            ParametreTablolariniYukle();
            YukleParametreler();
            IstampaAlanlariniYukle();
            TryGosterKullaniciTanimlariForm();
            if (tabMain.SelectedTab == tabBugunDoganlar)
                YukleBugunDoganlar();
        }

        private void ParametreTabloYonetimSekmesiniHazirla()
        {
            _tabParametreTabloYonetimi = new TabPage
            {
                Text = "Parametre Tablo Yonetimi",
                BackColor = System.Drawing.Color.FromArgb(245, 247, 250),
                Padding = new Padding(10)
            };

            _pnlParamHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = System.Drawing.Color.FromArgb(194, 34, 45),
                Padding = new Padding(14, 10, 14, 10)
            };
            var lblHeaderTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold),
                Text = "SISTEM PARAMETRELERI",
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            var lblHeaderSub = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = System.Drawing.Color.FromArgb(255, 235, 235),
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                Text = "Kolera MTSK sisteminde kullanilan tum parametre tanimlari bu ekrandan yonetilir.",
                TextAlign = System.Drawing.ContentAlignment.TopLeft
            };
            _pnlParamHeader.Controls.Add(lblHeaderSub);
            _pnlParamHeader.Controls.Add(lblHeaderTitle);

            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 44,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 6, 0, 0)
            };
            _btnParamKaydet = new Button { Text = "Kaydet", Width = 95, Height = 30, FlatStyle = FlatStyle.Flat };
            _btnParamYenile = new Button { Text = "Yenile", Width = 95, Height = 30, FlatStyle = FlatStyle.Flat };
            _btnParamSil = new Button { Text = "Sil", Width = 95, Height = 30, FlatStyle = FlatStyle.Flat };
            _btnParamYeni = new Button { Text = "Yeni Kayit", Width = 110, Height = 30, FlatStyle = FlatStyle.Flat };
            _btnParamKaydet.Click += (s, e) => DegisiklikleriKaydet();
            _btnParamYenile.Click += (s, e) =>
            {
                if (_tabParamTablePages.SelectedTab != null)
                    TabloyuYukle(Convert.ToString(_tabParamTablePages.SelectedTab.Tag));
            };
            _btnParamSil.Click += (s, e) =>
            {
                var g = AktifParamGrid();
                if (g == null || g.CurrentRow == null || g.CurrentRow.IsNewRow)
                    return;
                g.Rows.Remove(g.CurrentRow);
            };
            _btnParamYeni.Click += (s, e) =>
            {
                var g = AktifParamGrid();
                if (g == null) return;
                var dt = g.DataSource as DataTable;
                if (dt == null) return;
                dt.Rows.Add(dt.NewRow());
            };
            pnlButtons.Controls.Add(_btnParamKaydet);
            pnlButtons.Controls.Add(_btnParamYenile);
            pnlButtons.Controls.Add(_btnParamSil);
            pnlButtons.Controls.Add(_btnParamYeni);

            _splitParam = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 250,
                FixedPanel = FixedPanel.Panel1
            };

            _lstParamTablolar = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle
            };
            _lstParamTablolar.SelectedIndexChanged += (s, e) =>
            {
                var secilen = GosterimdenTabloAdi(Convert.ToString(_lstParamTablolar.SelectedItem));
                if (string.IsNullOrWhiteSpace(secilen))
                    return;

                foreach (TabPage tp in _tabParamTablePages.TabPages)
                {
                    if (string.Equals(Convert.ToString(tp.Tag), secilen, StringComparison.OrdinalIgnoreCase))
                    {
                        _tabParamTablePages.SelectedTab = tp;
                        TabloyuYukle(secilen);
                        return;
                    }
                }
            };
            _splitParam.Panel1.Controls.Add(_lstParamTablolar);

            _tabParamTablePages = new TabControl
            {
                Dock = DockStyle.Fill
            };
            _tabParamTablePages.SelectedIndexChanged += (s, e) =>
            {
                if (_tabParamTablePages.SelectedTab != null)
                    TabloyuYukle(Convert.ToString(_tabParamTablePages.SelectedTab.Tag));
            };
            _splitParam.Panel2.Controls.Add(_tabParamTablePages);

            _tabParametreTabloYonetimi.Controls.Add(_splitParam);
            _tabParametreTabloYonetimi.Controls.Add(pnlButtons);
            _tabParametreTabloYonetimi.Controls.Add(_pnlParamHeader);
            tabMain.TabPages.Add(_tabParametreTabloYonetimi);
        }

        private DataGridView AktifParamGrid()
        {
            if (_tabParamTablePages == null || _tabParamTablePages.SelectedTab == null)
                return null;
            var tabloAdi = Convert.ToString(_tabParamTablePages.SelectedTab.Tag);
            if (string.IsNullOrWhiteSpace(tabloAdi))
                return null;
            return _paramGridler.ContainsKey(tabloAdi) ? _paramGridler[tabloAdi] : null;
        }

        private void KurumsalTasarimiUygula()
        {
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.BackColor = System.Drawing.Color.FromArgb(240, 242, 245);

            if (_tabParametreTabloYonetimi != null)
                _tabParametreTabloYonetimi.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);

            foreach (TabPage tp in tabMain.TabPages)
                tp.BackColor = System.Drawing.Color.FromArgb(245, 247, 250);

            tabMain.Appearance = TabAppearance.Normal;

            mainLayout.BackColor = System.Drawing.Color.White;

            btnKaydet.FlatStyle = FlatStyle.Flat;
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnKaydet.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
            btnKaydet.ForeColor = System.Drawing.Color.White;

            btnYenile.FlatStyle = FlatStyle.Flat;
            btnYenile.FlatAppearance.BorderSize = 0;
            btnYenile.BackColor = System.Drawing.Color.FromArgb(69, 90, 100);
            btnYenile.ForeColor = System.Drawing.Color.White;

            if (_btnParamKaydet != null)
            {
                _btnParamKaydet.FlatAppearance.BorderSize = 0;
                _btnParamKaydet.BackColor = System.Drawing.Color.FromArgb(46, 125, 50);
                _btnParamKaydet.ForeColor = System.Drawing.Color.White;
            }
            if (_btnParamYenile != null)
            {
                _btnParamYenile.FlatAppearance.BorderSize = 0;
                _btnParamYenile.BackColor = System.Drawing.Color.FromArgb(69, 90, 100);
                _btnParamYenile.ForeColor = System.Drawing.Color.White;
            }
            if (_btnParamSil != null)
            {
                _btnParamSil.FlatAppearance.BorderSize = 0;
                _btnParamSil.BackColor = System.Drawing.Color.FromArgb(198, 40, 40);
                _btnParamSil.ForeColor = System.Drawing.Color.White;
            }
            if (_btnParamYeni != null)
            {
                _btnParamYeni.FlatAppearance.BorderSize = 0;
                _btnParamYeni.BackColor = System.Drawing.Color.FromArgb(25, 118, 210);
                _btnParamYeni.ForeColor = System.Drawing.Color.White;
            }

            GridDuzenle();
        }

        private void ParametreTablolariniYukle()
        {
            if (_lstParamTablolar == null || _tabParamTablePages == null)
                return;

            _lstParamTablolar.Items.Clear();
            _tabParamTablePages.TabPages.Clear();
            _paramDataTables.Clear();
            _paramAdapters.Clear();
            _paramGridler.Clear();

            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = @"
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND (
        TABLE_NAME LIKE 'Param%'
        OR TABLE_NAME LIKE '%Param'
        OR TABLE_NAME LIKE '%Parametre%'
      )
ORDER BY TABLE_NAME;";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var tabloAdi = Convert.ToString(r["TABLE_NAME"]);
                            if (!GuvenliTabloAdiMi(tabloAdi))
                                continue;
                            if (!_paramGosterimAdlari.ContainsKey(tabloAdi))
                                continue;

                            var gosterimAdi = TabloGosterimAdi(tabloAdi);
                            _lstParamTablolar.Items.Add(gosterimAdi);

                            var tp = new TabPage(gosterimAdi) { BackColor = System.Drawing.Color.White, Tag = tabloAdi };
                            var grid = new DataGridView
                            {
                                Dock = DockStyle.Fill,
                                AllowUserToAddRows = true,
                                AllowUserToDeleteRows = true,
                                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                                RowHeadersVisible = false,
                                BackgroundColor = System.Drawing.Color.White,
                                BorderStyle = BorderStyle.None
                            };
                            tp.Controls.Add(grid);
                            _tabParamTablePages.TabPages.Add(tp);
                            _paramGridler[tabloAdi] = grid;
                        }
                    }
                }

                if (_lstParamTablolar.Items.Count > 0)
                {
                    _lstParamTablolar.SelectedIndex = 0;
                    TabloyuYukle(GosterimdenTabloAdi(Convert.ToString(_lstParamTablolar.Items[0])));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Parametre tablolari yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TabloyuYukle(string tabloAdi)
        {
            if (string.IsNullOrWhiteSpace(tabloAdi))
                return;
            if (!GuvenliTabloAdiMi(tabloAdi))
            {
                MessageBox.Show("Gecersiz tablo adi tespit edildi.", "Guvenlik", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!_paramGridler.ContainsKey(tabloAdi))
                return;

            try
            {
                var sql = "SELECT * FROM " + KoseliTabloAdi(tabloAdi);
                var dt = new DataTable();
                var da = new SqlDataAdapter(sql, _connectionString);
                da.Fill(dt);
                _paramDataTables[tabloAdi] = dt;
                _paramAdapters[tabloAdi] = da;
                _paramGridler[tabloAdi].DataSource = dt;
                GridDuzenle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tablo yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DegisiklikleriKaydet()
        {
            var seciliTab = _tabParamTablePages == null ? null : _tabParamTablePages.SelectedTab;
            if (seciliTab == null)
                return;

            var tabloAdi = Convert.ToString(seciliTab.Tag);
            if (string.IsNullOrWhiteSpace(tabloAdi))
                return;
            if (!_paramDataTables.ContainsKey(tabloAdi) || !_paramAdapters.ContainsKey(tabloAdi))
                return;

            try
            {
                var dt = _paramDataTables[tabloAdi];
                var da = _paramAdapters[tabloAdi];

                using (var builder = new SqlCommandBuilder(da))
                {
                    da.InsertCommand = builder.GetInsertCommand();
                    da.UpdateCommand = builder.GetUpdateCommand();
                    da.DeleteCommand = builder.GetDeleteCommand();
                    int etkilenen = da.Update(dt);
                    MessageBox.Show(etkilenen + " kayıt güncellendi", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                TabloyuYukle(tabloAdi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydetme sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GridDuzenle()
        {
            foreach (var kv in _paramGridler)
            {
                var g = kv.Value;
                if (g == null) continue;

                g.EnableHeadersVisualStyles = false;
                g.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(194, 34, 45);
                g.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
                g.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
                g.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9F);
                g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                foreach (DataGridViewColumn c in g.Columns)
                {
                    c.HeaderText = (c.HeaderText ?? c.Name).Replace("_", " ");
                    if (string.Equals(c.Name, "ID", StringComparison.OrdinalIgnoreCase))
                    {
                        c.ReadOnly = true;
                        c.DefaultCellStyle.BackColor = System.Drawing.Color.Gainsboro;
                    }
                }
            }
        }

        private bool GuvenliTabloAdiMi(string tabloAdi)
        {
            if (string.IsNullOrWhiteSpace(tabloAdi))
                return false;
            return Regex.IsMatch(tabloAdi, "^[A-Za-z0-9_]+$");
        }

        private string TabloGosterimAdi(string tabloAdi)
        {
            if (string.IsNullOrWhiteSpace(tabloAdi))
                return string.Empty;
            return _paramGosterimAdlari.ContainsKey(tabloAdi) ? _paramGosterimAdlari[tabloAdi] : tabloAdi;
        }

        private string GosterimdenTabloAdi(string gosterimAdi)
        {
            if (string.IsNullOrWhiteSpace(gosterimAdi))
                return string.Empty;

            var match = _paramGosterimAdlari.FirstOrDefault(x =>
                string.Equals(x.Value, gosterimAdi, StringComparison.OrdinalIgnoreCase));
            return string.IsNullOrWhiteSpace(match.Key) ? gosterimAdi : match.Key;
        }

        private string KoseliTabloAdi(string tabloAdi)
        {
            return "[" + tabloAdi + "]";
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabMain.SelectedTab == tabBugunDoganlar)
                YukleBugunDoganlar();
        }

        private void btnYenile_Click(object sender, EventArgs e)
        {
            YukleParametreler();
        }

        private void btnBugunDoganlarYenile_Click(object sender, EventArgs e)
        {
            YukleBugunDoganlar();
        }

        private void btnBugunDoganlarSmsGonder_Click(object sender, EventArgs e)
        {
            if (dgvBugunDoganlar.DataSource == null || dgvBugunDoganlar.Rows.Count == 0)
            {
                MessageBox.Show("SMS gondermek icin once bugun doganlar listesi dolu olmali.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var gsmListesi = dgvBugunDoganlar.Rows
                .Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .Select(r => Convert.ToString(r.Cells["GSM_1"]?.Value))
                .Select(NormalizeTelefon)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            if (gsmListesi.Count == 0)
            {
                MessageBox.Show("Listede gecerli telefon numarasi bulunamadi.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var mesaj =
                "Sayin kursiyerimiz, dogum gununuzu kutlar; saglikli ve mutlu yillar dileriz. Kolera MTSK";

            using (var frm = new Frm_SMS_Gonder())
            {
                frm.KursiyerYukle(string.Join(",", gsmListesi), "BUGUN DOGANLAR", mesaj);
                frm.ShowDialog(this);
            }
        }

        private void btnKaydet_Click(object sender, EventArgs e)
        {
            KaydetParametreler();
        }

        private void YukleBugunDoganlar()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                lblBugunDoganlarBilgi.Text = "Veritabani baglantisi bulunamadi.";
                dgvBugunDoganlar.DataSource = null;
                return;
            }

            var sqlList = new[]
            {
                @"
SELECT
    ID,
    ISNULL(ADAY_NO,'') AS ADAY_NO,
    ISNULL(ADI,'') AS ADI,
    ISNULL(SOYADI,'') AS SOYADI,
    DOGUM_TARIHI,
    ISNULL(SERTIFIKA_SINIFI,'') AS SERTIFIKA_SINIFI,
    ISNULL(TC_NO,'') AS TC_NO,
    ISNULL(GSM_1,'') AS GSM_1
FROM KURSIYER
WHERE DAY(DOGUM_TARIHI)=DAY(GETDATE())
  AND MONTH(DOGUM_TARIHI)=MONTH(GETDATE())
  AND ISNULL(KURSIYER_DURUMU,1) NOT IN(6)
ORDER BY ADAY_NO DESC",
                @"
SELECT
    ID,
    ISNULL(ADAY_NO,'') AS ADAY_NO,
    ISNULL(ADI,'') AS ADI,
    ISNULL(SOYADI,'') AS SOYADI,
    DOGUM_TARIHI,
    ISNULL(SERTIFIKA_SINIFI,'') AS SERTIFIKA_SINIFI,
    ISNULL(TC_NO,'') AS TC_NO,
    ISNULL(GSM_1,'') AS GSM_1
FROM KURSIYERLER
WHERE DAY(DOGUM_TARIHI)=DAY(GETDATE())
  AND MONTH(DOGUM_TARIHI)=MONTH(GETDATE())
ORDER BY ADAY_NO DESC"
            };

            try
            {
                DataTable dt = null;
                string sonHata = null;

                foreach (var sql in sqlList)
                {
                    try
                    {
                        using (var conn = new SqlConnection(_connectionString))
                        using (var da = new SqlDataAdapter(sql, conn))
                        {
                            dt = new DataTable();
                            da.Fill(dt);
                            break;
                        }
                    }
                    catch (Exception exTry)
                    {
                        sonHata = exTry.Message;
                        dt = null;
                    }
                }

                if (dt == null)
                    throw new Exception(sonHata ?? "Bugun doganlar icin uygun tablo bulunamadi.");

                dgvBugunDoganlar.DataSource = dt;
                if (dgvBugunDoganlar.Columns.Contains("ID"))
                    dgvBugunDoganlar.Columns["ID"].Visible = false;
                if (dgvBugunDoganlar.Columns.Contains("DOGUM_TARIHI"))
                    dgvBugunDoganlar.Columns["DOGUM_TARIHI"].DefaultCellStyle.Format = "dd.MM.yyyy";

                lblBugunDoganlarBilgi.Text = "Toplam kayit: " + dgvBugunDoganlar.Rows.Count;
            }
            catch (Exception ex)
            {
                lblBugunDoganlarBilgi.Text = "Yukleme hatasi: " + ex.Message;
                dgvBugunDoganlar.DataSource = null;
            }
        }

        private static string NormalizeTelefon(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var digits = new string(input.Where(char.IsDigit).ToArray());
            if (digits.Length == 0)
                return string.Empty;

            if (digits.Length == 10)
                return "90" + digits;
            if (digits.Length == 11 && digits.StartsWith("0"))
                return "9" + digits;
            if (digits.Length == 12 && digits.StartsWith("90"))
                return digits;

            return digits;
        }

        private void YukleParametreler()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabani baglantisi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string kursBilgiTable = ResolveKursBilgiTableName();
            if (string.IsNullOrWhiteSpace(kursBilgiTable))
            {
                MessageBox.Show("KursBilgiParam tablosu bulunamadi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string sql = @"
SELECT TOP (1)
    ID,
    ISNULL(KURS_ADI,'') AS KURS_ADI,
    ISNULL(KURUM_KODU,'') AS KURUM_KODU,
    ISNULL(ADRES,'') AS ADRES,
    ISNULL(ILCE,'') AS ILCE,
    ISNULL(IL,'') AS IL,
    ISNULL(TELEFON,'') AS TELEFON,
    ISNULL(GSM,'') AS GSM,
    ISNULL(PK,'') AS PK,
    ISNULL(WEB,'') AS WEB,
    ISNULL(E_POSTA,'') AS E_POSTA,
    ISNULL(KURUCU_ADI,'') AS KURUCU_ADI,
    ISNULL(MUDUR_ADI,'') AS MUDUR_ADI,
    ISNULL(MUSTERI_NO,'') AS MUSTERI_NO,
    ISNULL(KURS_ADI_KISA,'') AS KURS_ADI_KISA,
    ISNULL(SOZLESME_BANKA_HESAPNO,'') AS SOZLESME_BANKA_HESAPNO
FROM [" + kursBilgiTable + @"]
ORDER BY ID DESC";

            try
            {
                EnsureKurumKoduColumn(kursBilgiTable);
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            if (EnsureKursBilgiDefaultRow(kursBilgiTable))
                            {
                                // Bos tablolarda ilk acilista form tamamen bos kalmasin.
                                YukleParametreler();
                                return;
                            }

                            TemizleParametreFormu();
                            return;
                        }

                        txtId.Text = reader["ID"].ToString();
                        txtKursAdi.Text = reader["KURS_ADI"].ToString();
                        txtAdres.Text = reader["ADRES"].ToString();
                        txtIlce.Text = reader["ILCE"].ToString();
                        txtIl.Text = reader["IL"].ToString();
                        txtTelefon.Text = reader["TELEFON"].ToString();
                        txtGsm.Text = reader["GSM"].ToString();
                        txtPk.Text = reader["PK"].ToString();
                        txtWeb.Text = reader["WEB"].ToString();
                        txtEposta.Text = reader["E_POSTA"].ToString();
                        txtKurucuAdi.Text = reader["KURUCU_ADI"].ToString();
                        txtMudurAdi.Text = reader["MUDUR_ADI"].ToString();
                        txtKurumKodu.Text = reader["KURUM_KODU"].ToString();
                        txtMusteriNo.Text = reader["MUSTERI_NO"].ToString();
                        txtKursAdiKisa.Text = reader["KURS_ADI_KISA"].ToString();
                        txtSozlesmeBankaHesapNo.Text = reader["SOZLESME_BANKA_HESAPNO"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Parametreler yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TemizleParametreFormu()
        {
            txtId.Text = string.Empty;
            txtKursAdi.Text = string.Empty;
            txtAdres.Text = string.Empty;
            txtIlce.Text = string.Empty;
            txtIl.Text = string.Empty;
            txtTelefon.Text = string.Empty;
            txtGsm.Text = string.Empty;
            txtPk.Text = string.Empty;
            txtWeb.Text = string.Empty;
            txtEposta.Text = string.Empty;
            txtKurucuAdi.Text = string.Empty;
            txtMudurAdi.Text = string.Empty;
            txtKurumKodu.Text = string.Empty;
            txtMusteriNo.Text = string.Empty;
            txtKursAdiKisa.Text = string.Empty;
            txtSozlesmeBankaHesapNo.Text = string.Empty;
        }

        private bool EnsureKursBilgiDefaultRow(string kursBilgiTable)
        {
            if (string.IsNullOrWhiteSpace(_connectionString) || string.IsNullOrWhiteSpace(kursBilgiTable))
                return false;

            string sql = @"
INSERT INTO [" + kursBilgiTable + @"]
(
    KURS_ADI, KURUM_KODU, ADRES, ILCE, IL, TELEFON, GSM, PK, WEB, E_POSTA,
    KURUCU_ADI, MUDUR_ADI, MUSTERI_NO, KURS_ADI_KISA, SOZLESME_BANKA_HESAPNO
)
VALUES
(
    '', '', '', '', '', '', '', '', '', '',
    '', '', '', '', ''
)";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void EnsureKurumKoduColumn(string kursBilgiTable)
        {
            if (string.IsNullOrWhiteSpace(_connectionString) || string.IsNullOrWhiteSpace(kursBilgiTable))
                return;

            string sql = @"IF COL_LENGTH('dbo." + kursBilgiTable + @"','KURUM_KODU') IS NULL
ALTER TABLE dbo." + kursBilgiTable + " ADD KURUM_KODU VARCHAR(50) NULL;";
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
            }
        }

        private void KaydetParametreler()
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabani baglantisi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string kursBilgiTable = ResolveKursBilgiTableName();
            if (string.IsNullOrWhiteSpace(kursBilgiTable))
            {
                MessageBox.Show("KursBilgiParam tablosu bulunamadi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string insertSql = @"
INSERT INTO [" + kursBilgiTable + @"]
(
    KURS_ADI, KURUM_KODU, ADRES, ILCE, IL, TELEFON, GSM, PK, WEB, E_POSTA,
    KURUCU_ADI, MUDUR_ADI, MUSTERI_NO, KURS_ADI_KISA, SOZLESME_BANKA_HESAPNO
)
VALUES
(
    @KURS_ADI, @KURUM_KODU, @ADRES, @ILCE, @IL, @TELEFON, @GSM, @PK, @WEB, @E_POSTA,
    @KURUCU_ADI, @MUDUR_ADI, @MUSTERI_NO, @KURS_ADI_KISA, @SOZLESME_BANKA_HESAPNO
)";

            string updateSql = @"
UPDATE [" + kursBilgiTable + @"]
SET
    KURS_ADI = @KURS_ADI,
    KURUM_KODU = @KURUM_KODU,
    ADRES = @ADRES,
    ILCE = @ILCE,
    IL = @IL,
    TELEFON = @TELEFON,
    GSM = @GSM,
    PK = @PK,
    WEB = @WEB,
    E_POSTA = @E_POSTA,
    KURUCU_ADI = @KURUCU_ADI,
    MUDUR_ADI = @MUDUR_ADI,
    MUSTERI_NO = @MUSTERI_NO,
    KURS_ADI_KISA = @KURS_ADI_KISA,
    SOZLESME_BANKA_HESAPNO = @SOZLESME_BANKA_HESAPNO
WHERE ID = @ID";

            try
            {
                EnsureKurumKoduColumn(kursBilgiTable);
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;

                    int id;
                    if (int.TryParse(txtId.Text, out id))
                    {
                        cmd.CommandText = updateSql;
                        cmd.Parameters.AddWithValue("@ID", id);
                    }
                    else
                    {
                        cmd.CommandText = insertSql;
                    }

                    cmd.Parameters.AddWithValue("@KURS_ADI", txtKursAdi.Text.Trim());
                    cmd.Parameters.AddWithValue("@KURUM_KODU", txtKurumKodu.Text.Trim());
                    cmd.Parameters.AddWithValue("@ADRES", txtAdres.Text.Trim());
                    cmd.Parameters.AddWithValue("@ILCE", txtIlce.Text.Trim());
                    cmd.Parameters.AddWithValue("@IL", txtIl.Text.Trim());
                    cmd.Parameters.AddWithValue("@TELEFON", txtTelefon.Text.Trim());
                    cmd.Parameters.AddWithValue("@GSM", txtGsm.Text.Trim());
                    cmd.Parameters.AddWithValue("@PK", txtPk.Text.Trim());
                    cmd.Parameters.AddWithValue("@WEB", txtWeb.Text.Trim());
                    cmd.Parameters.AddWithValue("@E_POSTA", txtEposta.Text.Trim());
                    cmd.Parameters.AddWithValue("@KURUCU_ADI", txtKurucuAdi.Text.Trim());
                    cmd.Parameters.AddWithValue("@MUDUR_ADI", txtMudurAdi.Text.Trim());
                    cmd.Parameters.AddWithValue("@MUSTERI_NO", txtMusteriNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@KURS_ADI_KISA", txtKursAdiKisa.Text.Trim());
                    cmd.Parameters.AddWithValue("@SOZLESME_BANKA_HESAPNO", txtSozlesmeBankaHesapNo.Text.Trim());

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                LisansPolitikasi.RegisterSuccessfulWrite();
                MessageBox.Show("Parametreler kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                YukleParametreler();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kaydetme hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ResolveKursBilgiTableName()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return null;

            const string sql = @"
SELECT TOP 1 TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND UPPER(TABLE_NAME) IN ('KURSBILGIPARAM')
ORDER BY TABLE_NAME;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var o = cmd.ExecuteScalar();
                var name = o == null || o == DBNull.Value ? null : Convert.ToString(o);
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            if (TryCreateKursBilgiParamTable())
                return "KursBilgiParam";

            return null;
        }

        private string ResolveGenelParamTableName()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return null;

            const string sql = @"
SELECT TOP 1 TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND UPPER(TABLE_NAME) IN ('GENELPARAM')
ORDER BY TABLE_NAME;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var o = cmd.ExecuteScalar();
                var name = o == null || o == DBNull.Value ? null : Convert.ToString(o);
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            if (TryCreateGenelParamTable())
                return "GenelParam";

            return null;
        }

        private bool TryCreateKursBilgiParamTable()
        {
            const string sql = @"
IF OBJECT_ID('dbo.KursBilgiParam','U') IS NULL
BEGIN
    CREATE TABLE dbo.KursBilgiParam(
      ID INT IDENTITY(1,1) PRIMARY KEY,
      KURS_ADI VARCHAR(200) NULL,
      KURUM_KODU VARCHAR(50) NULL,
      ADRES VARCHAR(300) NULL,
      ILCE VARCHAR(120) NULL,
      IL VARCHAR(120) NULL,
      TELEFON VARCHAR(50) NULL,
      GSM VARCHAR(50) NULL,
      PK VARCHAR(10) NULL,
      WEB VARCHAR(150) NULL,
      E_POSTA VARCHAR(150) NULL,
      KURUCU_ADI VARCHAR(120) NULL,
      MUDUR_ADI VARCHAR(120) NULL,
      MUSTERI_NO VARCHAR(120) NULL,
      KURS_ADI_KISA VARCHAR(120) NULL,
      SOZLESME_BANKA_HESAPNO VARCHAR(80) NULL
    );
END;";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TryCreateGenelParamTable()
        {
            const string sql = @"
IF OBJECT_ID('dbo.GenelParam','U') IS NULL
BEGIN
    CREATE TABLE dbo.GenelParam(
      ID INT IDENTITY(1,1) PRIMARY KEY,
      MEBBIS_KUL_ADI_1 NVARCHAR(100) NULL,
      MEBBIS_KUL_SIF_1 NVARCHAR(100) NULL,
      MEBBIS_KUL_YET_1 NVARCHAR(100) NULL,
      MEBBIS_KUL_ADI_2 NVARCHAR(100) NULL,
      MEBBIS_KUL_SIF_2 NVARCHAR(100) NULL,
      MEBBIS_KUL_YET_2 NVARCHAR(100) NULL,
      MEBBIS_KUL_ADI_3 NVARCHAR(100) NULL,
      MEBBIS_KUL_SIF_3 NVARCHAR(100) NULL,
      MEBBIS_KUL_YET_3 NVARCHAR(100) NULL,
      MEBBIS_KUL_ADI_4 NVARCHAR(100) NULL,
      MEBBIS_KUL_SIF_4 NVARCHAR(100) NULL,
      MEBBIS_KUL_YET_4 NVARCHAR(100) NULL,
      MEBBIS_KUL_ADI_5 NVARCHAR(100) NULL,
      MEBBIS_KUL_SIF_5 NVARCHAR(100) NULL,
      MEBBIS_KUL_YET_5 NVARCHAR(100) NULL
    );
END;";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void btnKullaniciTanimlari_Click(object sender, EventArgs e)
        {
            TryGosterKullaniciTanimlariForm();
        }

        private void btnLogKayitlari_Click(object sender, EventArgs e)
        {
            pnlKullaniciTanimlari.Visible = false;
            pnlLogKayitlari.Visible = true;
            pnlMebbisSifre.Visible = false;
            EnsureLogGridInitialized();
            EnsureAppLogTable();
            DeleteOlderThanOneMonthLogs();
            YukleLogKayitlari();
            LogKaydet("INFO", "PARAMETRELER", "Log kayitlari ekrani acildi.");
        }

        private void btnMebbisSifreTanimlama_Click(object sender, EventArgs e)
        {
            pnlKullaniciTanimlari.Visible = false;
            pnlLogKayitlari.Visible = false;
            pnlMebbisSifre.Visible = true;
            pnlMebbisSifre.BringToFront();
            YukleMebbisYetkiKullanicilari();
            YukleMebbisBilgileri();
        }

        private void EnsureLogGridInitialized()
        {
            if (_dgvLogKayitlari != null)
                return;

            _pnlLogGridHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 6, 0, 0)
            };

            _dgvLogKayitlari = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _pnlLogGridHost.Controls.Add(_dgvLogKayitlari);
            pnlLogKayitlari.Controls.Add(_pnlLogGridHost);
            flowLogButtons.BringToFront();
        }

        private bool EnsureAppLogTable()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return false;

            const string sql = @"
IF OBJECT_ID('dbo.APP_LOG_KAYITLARI','U') IS NULL
BEGIN
    CREATE TABLE dbo.APP_LOG_KAYITLARI
    (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        LOG_TARIHI DATETIME NOT NULL DEFAULT(GETDATE()),
        LOG_SEVIYE VARCHAR(20) NOT NULL,
        MODUL VARCHAR(100) NULL,
        KULLANICI_ADI VARCHAR(100) NULL,
        ACIKLAMA NVARCHAR(1000) NULL
    );
END;";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void LogKaydet(string seviye, string modul, string aciklama)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;
            if (!EnsureAppLogTable())
                return;

            const string sql = @"
INSERT INTO APP_LOG_KAYITLARI(LOG_TARIHI, LOG_SEVIYE, MODUL, KULLANICI_ADI, ACIKLAMA)
VALUES(GETDATE(), @SEVIYE, @MODUL, @KULLANICI, @ACIKLAMA);";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SEVIYE", (seviye ?? "INFO").Trim().ToUpperInvariant());
                    cmd.Parameters.AddWithValue("@MODUL", (object)(modul ?? string.Empty).Trim());
                    cmd.Parameters.AddWithValue("@KULLANICI", (object)(AppSession.CurrentUserName ?? string.Empty).Trim());
                    cmd.Parameters.AddWithValue("@ACIKLAMA", (object)(aciklama ?? string.Empty).Trim());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Log yazimi basarisiz olsa da uygulama akisi kesilmesin.
            }
        }

        private int DeleteOlderThanOneMonthLogs()
        {
            if (string.IsNullOrWhiteSpace(_connectionString) || !EnsureAppLogTable())
                return 0;

            const string sql = "DELETE FROM APP_LOG_KAYITLARI WHERE LOG_TARIHI < DATEADD(MONTH, -1, GETDATE())";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return 0;
            }
        }

        private int DeleteLastOneMonthLogs()
        {
            if (string.IsNullOrWhiteSpace(_connectionString) || !EnsureAppLogTable())
                return 0;

            const string sql = "DELETE FROM APP_LOG_KAYITLARI WHERE LOG_TARIHI >= DATEADD(MONTH, -1, GETDATE())";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                return 0;
            }
        }

        private bool IsCurrentUserAdmin()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return false;

            string aktifKullanici = (AppSession.CurrentUserName ?? string.Empty).Trim();
            if (aktifKullanici.Length == 0)
                return false;

            const string sql = @"
SELECT TOP (1) ISNULL(YETKI,'')
FROM KULLANICI
WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@KULLANICI_ADI)
ORDER BY ID DESC;";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@KULLANICI_ADI", aktifKullanici);
                    conn.Open();
                    var roleObj = cmd.ExecuteScalar();
                    string role = roleObj == null || roleObj == DBNull.Value ? string.Empty : Convert.ToString(roleObj).Trim();
                    return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }

        private void YukleLogKayitlari()
        {
            if (_dgvLogKayitlari == null || string.IsNullOrWhiteSpace(_connectionString))
                return;

            if (!EnsureAppLogTable())
                return;

            const string sql = @"
SELECT TOP (1000)
    ID,
    LOG_TARIHI,
    LOG_SEVIYE,
    ISNULL(MODUL,'') AS MODUL,
    ISNULL(KULLANICI_ADI,'') AS KULLANICI_ADI,
    ISNULL(ACIKLAMA,'') AS ACIKLAMA
FROM APP_LOG_KAYITLARI
ORDER BY LOG_TARIHI DESC, ID DESC;";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    _dgvLogKayitlari.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Log kayitlari yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogListe_Click(object sender, EventArgs e)
        {
            EnsureLogGridInitialized();
            EnsureAppLogTable();
            DeleteOlderThanOneMonthLogs();
            YukleLogKayitlari();
            LogKaydet("INFO", "PARAMETRELER", "Log listesi yenilendi.");
        }

        private void btnLogTemizle_Click(object sender, EventArgs e)
        {
            if (!IsCurrentUserAdmin())
            {
                MessageBox.Show("Bu islem sadece ADMIN yetkisiyle yapilabilir.", "Yetki", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int silinen = DeleteLastOneMonthLogs();
            YukleLogKayitlari();
            LogKaydet("WARN", "PARAMETRELER", "ADMIN tarafindan son 1 ay loglari temizlendi. Silinen: " + silinen);
            MessageBox.Show(silinen + " adet son 1 aya ait log kaydi silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnLogExcel_Click(object sender, EventArgs e)
        {
            if (_dgvLogKayitlari == null || _dgvLogKayitlari.DataSource == null)
            {
                MessageBox.Show("Disa aktarmak icin once log listesi yuklenmelidir.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var dt = _dgvLogKayitlari.DataSource as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Disa aktarilacak log kaydi bulunamadi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filePath = Path.Combine(desktopPath, "log_kayitlari_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
                var sb = new StringBuilder();
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(";");
                    sb.Append(dt.Columns[i].ColumnName);
                }
                sb.AppendLine();

                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        if (i > 0) sb.Append(";");
                        string cell = Convert.ToString(row[i] ?? string.Empty).Replace(";", ",");
                        sb.Append(cell);
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                LogKaydet("INFO", "PARAMETRELER", "Loglar masaustune CSV olarak aktarıldı. Dosya: " + filePath);
                MessageBox.Show("Log kayitlari masaustune aktarildi.\n" + filePath, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Disa aktarma hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KurSonHaliSertifikaGomulu()
        {
            if (_sonHaliSertifikaUcretForm != null && !_sonHaliSertifikaUcretForm.IsDisposed)
                return;

            _sonHaliSertifikaUcretForm = new SonHaliSertifikaUcretForm(_connectionString)
            {
                TopLevel = false,
                FormBorderStyle = FormBorderStyle.None,
                Dock = DockStyle.Fill,
                MinimumSize = new System.Drawing.Size(0, 0)
            };
            _sonHaliSertifikaUcretForm.KapatGomuluDavran = () =>
            {
                if (tabMain.TabCount > 0)
                    tabMain.SelectedIndex = 0;
                return true;
            };
            pnlSonHaliSertifikaHost.Controls.Add(_sonHaliSertifikaUcretForm);
            _sonHaliSertifikaUcretForm.Show();
        }

        private void GosterKullaniciTanimlari()
        {
            pnlKullaniciTanimlari.Visible = true;
            pnlLogKayitlari.Visible = false;
            pnlMebbisSifre.Visible = false;
            YukleKullaniciListesi();
            YukleSabitYetkiler();
        }

        private void GosterKullaniciTanimlariForm()
        {
            pnlKullaniciTanimlari.Visible = true;
            pnlLogKayitlari.Visible = false;
            pnlMebbisSifre.Visible = false;
            pnlKullaniciTanimlari.BringToFront();

            if (_kullaniciTanimlariForm == null || _kullaniciTanimlariForm.IsDisposed)
            {
                _kullaniciTanimlariForm = new KullaniciTanimlariForm(_connectionString);
                _kullaniciTanimlariForm.TopLevel = false;
                _kullaniciTanimlariForm.FormBorderStyle = FormBorderStyle.None;
                _kullaniciTanimlariForm.Dock = DockStyle.Fill;
            }

            pnlKullaniciTanimlari.Controls.Clear();
            pnlKullaniciTanimlari.Controls.Add(_kullaniciTanimlariForm);
            _kullaniciTanimlariForm.Show();
            _kullaniciTanimlariForm.BringToFront();
        }

        private void TryGosterKullaniciTanimlariForm()
        {
            try
            {
                GosterKullaniciTanimlariForm();
            }
            catch (Exception ex)
            {
                // Alt form acilamazsa ekranin tamamen bos kalmasini engelle.
                GosterKullaniciTanimlari();
                MessageBox.Show(
                    "Kullanici tanimlari alt formu acilamadi. Varsayilan panel gosterildi.\n" + ex.Message,
                    "Uyari",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void YukleKullaniciListesi()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabani baglantisi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            const string sql = "SELECT TOP (1000) ID, KULLANICI_ADI, KULLANICI_SIFRE, KAYIT_TARIHI, YETKI FROM KULLANICI";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var da = new SqlDataAdapter(sql, conn))
                {
                    _kullaniciTable = new DataTable();
                    da.Fill(_kullaniciTable);
                    dgvKullanicilar.DataSource = _kullaniciTable;
                    GuncelleSeciliKullaniciBaslik();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanici listesi yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void YukleSabitYetkiler()
        {
            if (dgvYetkiler.Rows.Count > 0)
                return;

            string[] satirlar =
            {
                "YARDIM",
                "PARAMETRELER",
                "RAPORLAR",
                "TAKIP/SMS/PERSONEL",
                "FINANS",
                "SINAV ISLEMLERI",
                "DERS PROGRAMI ISLEMLERI",
                "KURSIYER/GRUP ISLEMLERI",
                "KURSIYER KARTI",
                "RAPOR EKRANI",
                "DIGER YETKILER"
            };

            foreach (var satir in satirlar)
                dgvYetkiler.Rows.Add(satir, true);
        }

        private void dgvKullanicilar_SelectionChanged(object sender, EventArgs e)
        {
            GuncelleSeciliKullaniciBaslik();
        }

        private void GuncelleSeciliKullaniciBaslik()
        {
            if (dgvKullanicilar.CurrentRow == null)
            {
                lblSecilenKullanici.Text = "Secili kullanici yok";
                return;
            }

            var row = dgvKullanicilar.CurrentRow;
            string ad = DegerGetir(row, "KULLANICI_ADI");
            if (string.IsNullOrWhiteSpace(ad) && row.Cells.Count > 0)
                ad = Convert.ToString(row.Cells[0].Value);

            lblSecilenKullanici.Text = string.IsNullOrWhiteSpace(ad)
                ? "Secili kullanici yok"
                : ad.ToUpperInvariant() + " - KULLANICI YETKILERI";
        }

        private static string DegerGetir(DataGridViewRow row, string kolonAdi)
        {
            if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(kolonAdi))
                return string.Empty;
            var val = row.Cells[kolonAdi].Value;
            return val == null ? string.Empty : val.ToString().Trim();
        }

        private void YukleMebbisBilgileri()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            string genelTable = ResolveGenelParamTableName();
            if (string.IsNullOrWhiteSpace(genelTable))
                return;

            string sql = @"
SELECT TOP (1)
    ISNULL(MEBBIS_KUL_ADI_1,'') AS MEBBIS_KUL_ADI_1,
    ISNULL(MEBBIS_KUL_SIF_1,'') AS MEBBIS_KUL_SIF_1,
    ISNULL(MEBBIS_KUL_YET_1,'') AS MEBBIS_KUL_YET_1,
    ISNULL(MEBBIS_KUL_ADI_2,'') AS MEBBIS_KUL_ADI_2,
    ISNULL(MEBBIS_KUL_SIF_2,'') AS MEBBIS_KUL_SIF_2,
    ISNULL(MEBBIS_KUL_YET_2,'') AS MEBBIS_KUL_YET_2,
    ISNULL(MEBBIS_KUL_ADI_3,'') AS MEBBIS_KUL_ADI_3,
    ISNULL(MEBBIS_KUL_SIF_3,'') AS MEBBIS_KUL_SIF_3,
    ISNULL(MEBBIS_KUL_YET_3,'') AS MEBBIS_KUL_YET_3
FROM [" + genelTable + @"]
ORDER BY ID DESC";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            if (EnsureGenelParamDefaultRow(genelTable))
                            {
                                // Bos GenelParam tablosunda ilk kaydi olusturup tekrar oku.
                                YukleMebbisBilgileri();
                            }
                            return;
                        }

                        txtMebbisKulAdi1.Text = reader["MEBBIS_KUL_ADI_1"].ToString();
                        txtMebbisSifre1.Text = reader["MEBBIS_KUL_SIF_1"].ToString();
                        SetComboTextSafe(cmbMebbisYetki1, reader["MEBBIS_KUL_YET_1"].ToString());

                        txtMebbisKulAdi2.Text = reader["MEBBIS_KUL_ADI_2"].ToString();
                        txtMebbisSifre2.Text = reader["MEBBIS_KUL_SIF_2"].ToString();
                        SetComboTextSafe(cmbMebbisYetki2, reader["MEBBIS_KUL_YET_2"].ToString());

                        txtMebbisKulAdi3.Text = reader["MEBBIS_KUL_ADI_3"].ToString();
                        txtMebbisSifre3.Text = reader["MEBBIS_KUL_SIF_3"].ToString();
                        SetComboTextSafe(cmbMebbisYetki3, reader["MEBBIS_KUL_YET_3"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mebbis bilgileri yuklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool EnsureGenelParamDefaultRow(string genelTable)
        {
            if (string.IsNullOrWhiteSpace(_connectionString) || string.IsNullOrWhiteSpace(genelTable))
                return false;

            string sql = @"
INSERT INTO [" + genelTable + @"]
(
    MEBBIS_KUL_ADI_1, MEBBIS_KUL_SIF_1, MEBBIS_KUL_YET_1,
    MEBBIS_KUL_ADI_2, MEBBIS_KUL_SIF_2, MEBBIS_KUL_YET_2,
    MEBBIS_KUL_ADI_3, MEBBIS_KUL_SIF_3, MEBBIS_KUL_YET_3,
    MEBBIS_KUL_ADI_4, MEBBIS_KUL_SIF_4, MEBBIS_KUL_YET_4,
    MEBBIS_KUL_ADI_5, MEBBIS_KUL_SIF_5, MEBBIS_KUL_YET_5
)
VALUES
(
    '', '', '',
    '', '', '',
    '', '', '',
    '', '', '',
    '', '', ''
)";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }

        private void btnMebbisKaydet_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabani baglantisi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string genelTable = ResolveGenelParamTableName();
            if (string.IsNullOrWhiteSpace(genelTable))
            {
                MessageBox.Show("GenelParam tablosu bulunamadi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string sql = @"
IF EXISTS (SELECT 1 FROM [" + genelTable + @"])
BEGIN
    UPDATE [" + genelTable + @"]
    SET
        MEBBIS_KUL_ADI_1 = @ADI1,
        MEBBIS_KUL_SIF_1 = @SIF1,
        MEBBIS_KUL_YET_1 = @YET1,
        MEBBIS_KUL_ADI_2 = @ADI2,
        MEBBIS_KUL_SIF_2 = @SIF2,
        MEBBIS_KUL_YET_2 = @YET2,
        MEBBIS_KUL_ADI_3 = @ADI3,
        MEBBIS_KUL_SIF_3 = @SIF3,
        MEBBIS_KUL_YET_3 = @YET3
    WHERE ID = (SELECT TOP (1) ID FROM [" + genelTable + @"] ORDER BY ID DESC);
END
ELSE
BEGIN
    INSERT INTO [" + genelTable + @"]
    (
        MEBBIS_KUL_ADI_1, MEBBIS_KUL_SIF_1, MEBBIS_KUL_YET_1,
        MEBBIS_KUL_ADI_2, MEBBIS_KUL_SIF_2, MEBBIS_KUL_YET_2,
        MEBBIS_KUL_ADI_3, MEBBIS_KUL_SIF_3, MEBBIS_KUL_YET_3,
        MEBBIS_KUL_ADI_4, MEBBIS_KUL_SIF_4, MEBBIS_KUL_YET_4,
        MEBBIS_KUL_ADI_5, MEBBIS_KUL_SIF_5, MEBBIS_KUL_YET_5
    )
    VALUES
    (
        @ADI1, @SIF1, @YET1,
        @ADI2, @SIF2, @YET2,
        @ADI3, @SIF3, @YET3,
        '', '', '',
        '', '', ''
    );
END";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ADI1", txtMebbisKulAdi1.Text.Trim());
                    cmd.Parameters.AddWithValue("@SIF1", txtMebbisSifre1.Text.Trim());
                    cmd.Parameters.AddWithValue("@YET1", cmbMebbisYetki1.Text.Trim());
                    cmd.Parameters.AddWithValue("@ADI2", txtMebbisKulAdi2.Text.Trim());
                    cmd.Parameters.AddWithValue("@SIF2", txtMebbisSifre2.Text.Trim());
                    cmd.Parameters.AddWithValue("@YET2", cmbMebbisYetki2.Text.Trim());
                    cmd.Parameters.AddWithValue("@ADI3", txtMebbisKulAdi3.Text.Trim());
                    cmd.Parameters.AddWithValue("@SIF3", txtMebbisSifre3.Text.Trim());
                    cmd.Parameters.AddWithValue("@YET3", cmbMebbisYetki3.Text.Trim());

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                LisansPolitikasi.RegisterSuccessfulWrite();
                MessageBox.Show("Mebbis sifre bilgileri kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                YukleMebbisBilgileri();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mebbis kaydetme hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void YukleMebbisYetkiKullanicilari()
        {
            cmbMebbisYetki1.Items.Clear();
            cmbMebbisYetki2.Items.Clear();
            cmbMebbisYetki3.Items.Clear();

            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = "SELECT TOP (1000) KULLANICI_ADI FROM KULLANICI";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string kullaniciAdi = ReaderKolonDegeri(reader, "KULLANICI_ADI");

                            if (string.IsNullOrWhiteSpace(kullaniciAdi))
                                continue;

                            cmbMebbisYetki1.Items.Add(kullaniciAdi);
                            cmbMebbisYetki2.Items.Add(kullaniciAdi);
                            cmbMebbisYetki3.Items.Add(kullaniciAdi);
                        }
                    }
                }
            }
            catch
            {
                // Kullanici listesi okunamazsa combo bos kalabilir, form yine acilsin.
            }
        }

        private static string ReaderKolonDegeri(SqlDataReader reader, string kolonAdi)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (!string.Equals(reader.GetName(i), kolonAdi, StringComparison.OrdinalIgnoreCase))
                    continue;
                var val = reader.GetValue(i);
                return val == null ? string.Empty : val.ToString().Trim();
            }
            return string.Empty;
        }

        private static void SetComboTextSafe(ComboBox combo, string text)
        {
            var value = (text ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                combo.SelectedIndex = -1;
                combo.Text = string.Empty;
                return;
            }

            if (combo.Items.IndexOf(value) < 0)
                combo.Items.Add(value);

            combo.SelectedItem = value;
        }

        private void btnSifreGoster1_Click(object sender, EventArgs e)
        {
            txtMebbisSifre1.UseSystemPasswordChar = !txtMebbisSifre1.UseSystemPasswordChar;
            btnSifreGoster1.Text = txtMebbisSifre1.UseSystemPasswordChar ? "Goster" : "Gizle";
        }

        private void btnSifreGoster2_Click(object sender, EventArgs e)
        {
            txtMebbisSifre2.UseSystemPasswordChar = !txtMebbisSifre2.UseSystemPasswordChar;
            btnSifreGoster2.Text = txtMebbisSifre2.UseSystemPasswordChar ? "Goster" : "Gizle";
        }

        private void btnSifreGoster3_Click(object sender, EventArgs e)
        {
            txtMebbisSifre3.UseSystemPasswordChar = !txtMebbisSifre3.UseSystemPasswordChar;
            btnSifreGoster3.Text = txtMebbisSifre3.UseSystemPasswordChar ? "Goster" : "Gizle";
        }
    }
}
