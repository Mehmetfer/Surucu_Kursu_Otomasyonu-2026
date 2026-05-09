 
using Kolera.Donem.Services;
using Kolera.Esinav.HazirlaSon;
using Kolera.Mebbis.Services;
using Kolera_Mtsk.Sayfalar;
using Kolera_Mtsk.Services;

using Kolera_MTSK.Login;
using Kolera_MTSK.LoginL;
using System;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


namespace Kolera_Mtsk
{
    public partial class Kolera_Login : Form
    {
        private ServerAyarModel _serverAyar;
        private KullaniciService _kullaniciService;
        private LisansModel _lisans;
        private readonly IKursiyerPrintService _printService = new KursiyerPrintService();
        private SohbetForm _sohbetForm;
        private Button _btnSohbetLogin;
        private LisansCalismaModu _lisansModu = LisansCalismaModu.Demo;

        private bool _tamEkranAktif;
        private FormBorderStyle _tamEkranOncekiKenar;
        private Rectangle _tamEkranOncekiNormalBounds;
        private bool _tamEkranOncekiMaximized;
        private DockStyle _anaPanelOncekiDock;
        private Rectangle _anaPanelOncekiBounds;


        public Kolera_Login() : this(new ServerAyarModel())
        {
        }

        public Kolera_Login(ServerAyarModel serverAyar)
        {
            InitializeComponent();

            _serverAyar = serverAyar ?? throw new ArgumentNullException(nameof(serverAyar));
            _kullaniciService = new KullaniciService(_serverAyar.ConnectionString);

            this.Load += Kolera_Login_Load;
            Ana_Panel.Enabled = false;
            LoginPanel.BringToFront();
            LoginPanel.Visible = true;
            LoginPanel.Enabled = true;
            Menumain.Enabled = false;
            SideBar.Visible = false;
            Btn_Exit.Click += Btn_Exit_Click;
            buttonExit.Click += Btn_Exit_Click;
            Yardim_Button.Click += Yardim_Button_Click;
            pictureBoxLogo.Click += pictureBoxLogo_Click;
           
            Yedek_Button.Click += Yedek_Button_Click;
            Ayarlar_Button.Click += Ayarlar_Button_Click;
           
            SohbetMenusunuKaldir();
            SohbetLoginButonuHazirla();
            Move += (s, e) => KonumlaSohbet();
            Resize += (s, e) =>
            {
                if (_tamEkranAktif)
                    UyarlaAnaPanelTamEkranIcin();
                KonumlaSohbet();
            };
            SizeChanged += (s, e) =>
            {
                if (_tamEkranAktif)
                    UyarlaAnaPanelTamEkranIcin();
                KonumlaSohbet();
            };
        }

        private async void Kolera_Login_Load(object sender, EventArgs e)
        {
            GeciciLisansHelper.Sifirla();

            bool baglantiBasarili = false;
            bool lisansInternetVar = false;

            try
            {
                // Once constructor'dan gelen ayari kullan.
                // Yoksa once SQL xml, olmazsa LOCAL xml dene.
                if (_serverAyar == null || string.IsNullOrWhiteSpace(_serverAyar.ConnectionString))
                {
                    string appDataDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Kolera_MTSK");
                    string sqlXmlPath = File.Exists(Path.Combine(appDataDir, "baglantisql.xml"))
                        ? Path.Combine(appDataDir, "baglantisql.xml")
                        : Path.Combine(Application.StartupPath, "Baglantisql.xml");
                    string localXmlPath = File.Exists(Path.Combine(appDataDir, "baglanti.xml"))
                        ? Path.Combine(appDataDir, "baglanti.xml")
                        : Path.Combine(Application.StartupPath, "baglanti.xml");
                    string xmlPath = null;

                    if (File.Exists(sqlXmlPath))
                    {
                        xmlPath = sqlXmlPath;
                    }
                    else if (File.Exists(localXmlPath))
                    {
                        xmlPath = localXmlPath;
                    }
                    else
                    {
                        MessageBox.Show("Baglantisql.xml veya baglanti.xml bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    XmlBaglantiService xmlService = new XmlBaglantiService();
                    _serverAyar = xmlService.GetServerAyar(xmlPath);
                }

                // SQL bağlantısını test et
                using (var conn = new System.Data.SqlClient.SqlConnection(_serverAyar.ConnectionString))
                {
                    conn.Open();
                }

                // Kullanıcı servislerini başlat
                _kullaniciService = new KullaniciService(_serverAyar.ConnectionString);
                DatabaseSchemaMigration.ApplyIfNeeded(_serverAyar.ConnectionString);
                baglantiBasarili = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("SQL bağlantısı kurulamadı: " + ex.Message, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Burada uygulamayı kapatmıyoruz
                baglantiBasarili = false;
            }

            // Lisans kontrolü ve kullanıcıları yükleme
            try
            {
                if (baglantiBasarili)
                {
                    var lisService = new LisansService(_serverAyar.ConnectionString);
                    _lisans = await lisService.GetLisansFromWebAsync();
                    var webTextLicense = await TryGetWebTextLicenseAsync();
                    if (webTextLicense != null)
                        _lisans = webTextLicense;
                    lisansInternetVar = await CheckLicenseEndpointOnlineAsync();
                    _lisansModu = ResolveLisansModu(_lisans);
                    LisansPolitikasi.SetMode(_lisansModu);
                    LisansDurum.Demo = _lisansModu != LisansCalismaModu.Active;
                }
                else
                {
                    lisansInternetVar = false;
                    _lisansModu = LisansCalismaModu.Demo;
                    LisansPolitikasi.SetMode(_lisansModu);
                    LisansDurum.Demo = true; // Bağlantı yoksa demo modda başlat
                }
            }
            catch
            {
                lisansInternetVar = false;
                _lisansModu = LisansCalismaModu.Demo;
                LisansPolitikasi.SetMode(_lisansModu);
                LisansDurum.Demo = true;
                MessageBox.Show("Lisans alınamadı, demo sürüm başlatıldı.", "Lisans Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            LisansPolitikasi.RegisterLicenseCheck(lisansInternetVar);

            LisansDurumunuGoster();

            if (baglantiBasarili)
            {
                try
                {
                    if (_lisans != null)
                        await LisansDbStartupSync.ApplyIfPossibleAsync(_serverAyar.ConnectionString, _lisans);
                }
                catch
                {
                    // Lisans tablo guncellemesi basarisiz olsa giris devam eder.
                }

                try
                {
                    await KullaniciAdlariniYukle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kullanıcılar yüklenemedi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            VersionService.Save();

            if (_lisans != null && !VersionService.VersionGecerliMi(_lisans.Versiyon))
            {
                MessageBox.Show(
                    $"Program güncel değil\nGerekli sürüm: {_lisans.Versiyon}\nSizin sürüm: {VersionService.GetVersion()}",
                    "Güncelleme Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void LisansDurumunuGoster()
        {
            if (GeciciLisansHelper.GeciciAktif)
            {
                LblLisansDurum.Text = "Geçici Lisans Aktif";
                LblLisansDurum.ForeColor = Color.Green;
                LisansDurum.Demo = false;
                _lisansModu = LisansCalismaModu.Active;
                LisansPolitikasi.SetMode(_lisansModu);
                Mebbis_Button.Enabled = true;
                AppendLicenseCheckInfo();
                return;
            }

            if (_lisans == null)
            {
                LblLisansDurum.Text = "Lisans alınamadı | DEMO";
                LblLisansDurum.ForeColor = Color.Red;
                LisansDurum.Demo = true;
                _lisansModu = LisansCalismaModu.Demo;
                LisansPolitikasi.SetMode(_lisansModu);
                Mebbis_Button.Enabled = false;
                AppendLicenseCheckInfo();
                return;
            }

            _lisansModu = ResolveLisansModu(_lisans);
            LisansPolitikasi.SetMode(_lisansModu);

            if (_lisansModu == LisansCalismaModu.Active)
            {
                LblLisansDurum.Text = $"Lisans No: {_lisans.LisansNo} | Bitiş: {_lisans.ValidUntil:dd.MM.yyyy} | Aktif";
                LblLisansDurum.ForeColor = Color.Green;
                LisansDurum.Demo = false;
                Mebbis_Button.Enabled = true;
            }
            else if (_lisansModu == LisansCalismaModu.Passive)
            {
                LblLisansDurum.Text = $"Lisans No: {_lisans.LisansNo} | Bitiş: {_lisans.ValidUntil:dd.MM.yyyy} | Passive";
                LblLisansDurum.ForeColor = Color.DarkRed;
                LisansDurum.Demo = true;
                Mebbis_Button.Enabled = false;
            }
            else if (string.Equals(_lisans.Durum, "Bekleyen", StringComparison.OrdinalIgnoreCase))
            {
                LblLisansDurum.Text = $"Lisans No: {_lisans.LisansNo} | Bitiş: {_lisans.ValidUntil:dd.MM.yyyy} | Beklemede (onay)";
                LblLisansDurum.ForeColor = Color.DarkOrange;
                LisansDurum.Demo = true;
                Mebbis_Button.Enabled = false;
            }
            else
            {
                LblLisansDurum.Text = $"Lisans süresi dolmuş veya demo | Bitiş: {_lisans.ValidUntil:dd.MM.yyyy}";
                LblLisansDurum.ForeColor = Color.Orange;
                LisansDurum.Demo = true;
                Mebbis_Button.Enabled = false;
            }

            AppendLicenseCheckInfo();
        }

        private async Task KullaniciAdlariniYukle()
        {
            try
            {
                ComboBox_KullaniciAdi.Items.Clear();
                var liste = await _kullaniciService.GetKullaniciAdlariAsync();
                foreach (string k in liste)
                    ComboBox_KullaniciAdi.Items.Add(k);

                if (ComboBox_KullaniciAdi.Items.Count > 0)
                    ComboBox_KullaniciAdi.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kullanıcılar yüklenemedi:\n" + ex.Message, "HATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnGiris_Click(object sender, EventArgs e)
        {
            string kullanici = ComboBox_KullaniciAdi.SelectedItem?.ToString();
            string parola = Txt_Parola.Text;

            if (string.IsNullOrWhiteSpace(kullanici))
            {
                MessageBox.Show("Kullanıcı adı boş olamaz.");
                return;
            }

            if (string.IsNullOrWhiteSpace(parola) && kullanici.ToUpper() != "ADMİN")
            {
                MessageBox.Show("Parola boş olamaz.");
                return;
            }

            bool ok = false;
            try
            {
                // 🔹 Burayı async SP tabanlı DLL metoduna göre çağırıyoruz
                ok = await _kullaniciService.IsValidKullaniciAsync(kullanici, parola);
                if (!ok)
                {
                    // Eski veritabanlarinda bazi kullanicilar sifreyi duz metin tutuyor.
                    // SP dogrulamasi basarisizsa duz metin fallback ile tekrar dene.
                    ok = await IsValidKullaniciPlainFallbackAsync(kullanici, parola);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Giriş sırasında hata oluştu:\n" + ex.Message, "HATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!ok)
            {
                MessageBox.Show("Hatalı kullanıcı adı veya parola.");
                return;
            }

            if (LisansDurum.Demo)
            {
                Mebbis_Button.Enabled = !LisansDurum.Demo || GeciciLisansHelper.GeciciAktif;
            }

            LoginPanel.Visible = false;
            Ana_Panel.Enabled = true;
            Menumain.Enabled = true;
            AppSession.CurrentUserName = kullanici;

            PanelIcineFormAc(new Ana_Sayfam());
        }

        private async Task<bool> IsValidKullaniciPlainFallbackAsync(string kullanici, string parola)
        {
            if (string.IsNullOrWhiteSpace(_serverAyar?.ConnectionString))
                return false;

            try
            {
                using (var conn = new SqlConnection(_serverAyar.ConnectionString))
                using (var cmd = new SqlCommand(@"
SELECT COUNT(1)
FROM KULLANICI
WHERE KULLANICI_ADI = @kullanici
  AND ISNULL(KULLANICI_SIFRE, '') = @parola;", conn))
                {
                    cmd.Parameters.AddWithValue("@kullanici", kullanici ?? string.Empty);
                    cmd.Parameters.AddWithValue("@parola", parola ?? string.Empty);
                    await conn.OpenAsync();
                    var sonuc = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(sonuc) > 0;
                }
            }
            catch
            {
                return false;
            }
        }


        private void Btn_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }

        private void Btn_Tamekran_Click(object sender, EventArgs e)
        {
            if (_tamEkranAktif)
            {
                FormBorderStyle = _tamEkranOncekiKenar;
                if (_tamEkranOncekiMaximized)
                {
                    WindowState = FormWindowState.Normal;
                    Bounds = _tamEkranOncekiNormalBounds;
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    WindowState = FormWindowState.Normal;
                    Bounds = _tamEkranOncekiNormalBounds;
                }

                _tamEkranAktif = false;
                if (Btn_Tamekran != null)
                    Btn_Tamekran.Text = "Tam ekran";

                if (Ana_Panel != null)
                {
                    Ana_Panel.SuspendLayout();
                    Ana_Panel.Dock = _anaPanelOncekiDock;
                    Ana_Panel.Bounds = _anaPanelOncekiBounds;
                    Ana_Panel.ResumeLayout(true);
                }
            }
            else
            {
                if (Ana_Panel != null)
                {
                    _anaPanelOncekiDock = Ana_Panel.Dock;
                    _anaPanelOncekiBounds = Ana_Panel.Bounds;
                }

                _tamEkranOncekiKenar = FormBorderStyle;
                _tamEkranOncekiMaximized = WindowState == FormWindowState.Maximized;
                _tamEkranOncekiNormalBounds = _tamEkranOncekiMaximized ? RestoreBounds : Bounds;

                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Normal;
                Bounds = Screen.FromControl(this).Bounds;

                _tamEkranAktif = true;
                if (Btn_Tamekran != null)
                    Btn_Tamekran.Text = "Küçült";

                UyarlaAnaPanelTamEkranIcin();
            }

            KonumlaSohbet();
        }

        /// <summary>
        /// Tam ekranda Ana_Panel sabit kalmasin; icindeki Dock=Fill formlar da buyusun.
        /// </summary>
        private void UyarlaAnaPanelTamEkranIcin()
        {
            if (!_tamEkranAktif || Ana_Panel == null || Content == null || Menumain == null || panelHead == null)
                return;

            int stripTop = Menumain.Bottom;
            if (Yukari_Panel != null && Yukari_Panel.Visible)
                stripTop = Math.Max(stripTop, Yukari_Panel.Bottom);

            int w = Content.ClientSize.Width;
            int h = Content.ClientSize.Height - stripTop - panelHead.Height;
            if (w < 200) w = 200;
            if (h < 120) h = 120;

            Ana_Panel.SuspendLayout();
            Ana_Panel.Dock = DockStyle.None;
            Ana_Panel.Location = new Point(0, stripTop);
            Ana_Panel.Size = new Size(w, h);
            Ana_Panel.ResumeLayout(true);
            Ana_Panel.PerformLayout();
        }

        // Designer event fix
        private void button1_Click(object sender, EventArgs e) => Btn_Exit_Click(sender, e);
        private void ComboBox_KullaniciAdi_SelectedIndexChanged(object sender, EventArgs e) { }

        private void Kursiyer_Ekle_Click(object sender, EventArgs e)
        {
            if (_serverAyar == null || string.IsNullOrWhiteSpace(_serverAyar.ConnectionString))
            {
                MessageBox.Show("Veritabanı bağlantısı yok!");
                return;
            }
            PanelIcineFormAc(new Kursiyer_Sayfam(_serverAyar.ConnectionString));
        }

        private void Mebbis_Button_Click(object sender, EventArgs e)
        {
            if (LisansDurum.Demo)
            {
                MessageBox.Show("Demo sürümde bu modül kapalıdır.");
                return;
            }

            PanelIcineFormAc(new Mebbis_Sayfam(_serverAyar.ConnectionString));
        }

        private void Arama_Button_Click(object sender, EventArgs e)
        {
            PanelIcineFormAc(new Arama_Sayfam(_serverAyar.ConnectionString, Ana_Panel));
        }

        private void Araclar_Button_Click(object sender, EventArgs e)
        {
            PanelIcineFormAc(new Araclar_Sayfam(_serverAyar.ConnectionString));
        }

        private void Raporlar_Button_Click(object sender, EventArgs e)
        {
            PanelIcineFormAc(new Raporlar(_serverAyar.ConnectionString));
        }

        private void Donem_Grup_Click(object sender, EventArgs e)
        {
            PanelIcineFormAc(new Donem_Grup_Sayfam(_serverAyar.ConnectionString));
        }

        private void Peronsel_Button_Click(object sender, EventArgs e)
        {
            PanelIcineFormAc(new Form_Personel(_serverAyar.ConnectionString));
        }

        private void DireksiyonSinavi_Click(object sender, EventArgs e)
        {
            PanelIcineFormAc(new Direksiyon_Sinav_Hazirla(_serverAyar.ConnectionString));
        }

        private void Anasayfa_Click(object sender, EventArgs e)
        {
            AnaSayfayaDon();
        }

        private void AnaSayfayaDon()
        {
            TumPopupVeHariciFormlariKapat();

            foreach (Control control in Ana_Panel.Controls)
            {
                if (control is Form oldForm)
                    oldForm.Close();
            }

            Ana_Panel.Controls.Clear();
            PanelIcineFormAc(new Ana_Sayfam());
            Ana_Panel.BringToFront();
        }

        private void TumPopupVeHariciFormlariKapat()
        {
            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
            {
                Form form = Application.OpenForms[i];
                if (form == null || form == this)
                    continue;

                if (form.Modal || form.Owner == this || form.TopMost || form.ShowInTaskbar)
                    form.Close();
            }
        }

        private void PanelIcineFormAc(Form frm)
        {
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;

            Ana_Panel.Controls.Clear();
            Ana_Panel.Controls.Add(frm);
            frm.Show();
            if (_tamEkranAktif)
                UyarlaAnaPanelTamEkranIcin();
        }

        private async void PictureBox3_Click(object sender, EventArgs e)
        {
            using (var frm = new DbSecimForm())
            {
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    string appDataDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Kolera_MTSK");
                    string xmlPath = frm.Secim == "LOCAL"
                        ? (File.Exists(Path.Combine(appDataDir, "baglanti.xml"))
                            ? Path.Combine(appDataDir, "baglanti.xml")
                            : Path.Combine(Application.StartupPath, "baglanti.xml"))
                        : (File.Exists(Path.Combine(appDataDir, "baglantisql.xml"))
                            ? Path.Combine(appDataDir, "baglantisql.xml")
                            : Path.Combine(Application.StartupPath, "Baglantisql.xml"));

                    try
                    {
                        XmlBaglantiService xmlService = new XmlBaglantiService();
                        _serverAyar = xmlService.GetServerAyar(xmlPath);
                        _kullaniciService = new KullaniciService(_serverAyar.ConnectionString);

                        MessageBox.Show(
                            $"Seçilen DB: {frm.Secim}\nSunucu: {_serverAyar.Sunucu}\nDB: {_serverAyar.VeritabaniAdi}",
                            "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // 🔹 Kullanıcıları otomatik yükle
                        await KullaniciAdlariniYukle();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("XML okunamadı veya bağlantı oluşturulamadı:\n" + ex.Message,
                            "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void Esinavlar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_serverAyar?.ConnectionString))
            {
                MessageBox.Show("Veritabanı bağlantısı yok!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ana panel içine form açma
            var esinavForm = new Kolera_Mtsk.Sayfalar.ESINAV_HAZIRLA(_serverAyar.ConnectionString);
            PanelIcineFormAc(esinavForm);
        }

        private void Yardim_Button_Click(object sender, EventArgs e)
        {
            string mebbisKullanici = null;
            string mebbisSifre = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(_serverAyar?.ConnectionString))
                {
                    MebbisCredentialResolver.TryResolve(_serverAyar.ConnectionString, AppSession.CurrentUserName, out mebbisKullanici, out mebbisSifre);
                }
            }
            catch
            {
                // Mebbis_Sayfam kaynagindan alinamazsa mevcut login degerlerine fallback.
            }

            if (string.IsNullOrWhiteSpace(mebbisKullanici))
                mebbisKullanici = ComboBox_KullaniciAdi.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(mebbisSifre))
                mebbisSifre = Txt_Parola.Text;

            try
            {
                PanelIcineFormAc(new YardimMerkeziForm(mebbisKullanici, mebbisSifre, PanelIcineFormAc, _serverAyar?.ConnectionString, SidebariAktifEt, _lisans));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Yardim merkezi acilirken hata olustu:\n" + ex.Message,
                    "Yardim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SidebariAktifEt()
        {
            SideBar.Visible = true;
            SideBar.Enabled = true;
            SideBar.BringToFront();
        }

        private void pictureBoxLogo_Click(object sender, EventArgs e)
        {
            SideBar.Visible = false;
            SideBar.Enabled = false;
        }

        private async Task<LisansModel> TryGetWebTextLicenseAsync()
        {
            if (LisansPolitikasi.IsWebLicenseCallSuppressed)
                return null;

            try
            {
                string payload = (ConfigurationManager.AppSettings["WebLicensePayload"] ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(payload))
                {
                    string url = (ConfigurationManager.AppSettings["WebLicenseUrl"] ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(url))
                        url = "http://mehmetfer.com.tr/lisans.php?token=KOLERA2026";
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        string lisansNo = await GetCurrentLicenseNoForRequestAsync();
                        string musteriNo = await GetMusteriNoAsync();
                        string trackedUrl = AppendLicenseTrackingParams(url, lisansNo, musteriNo);
                        using (var wc = new WebClient())
                            payload = await wc.DownloadStringTaskAsync(trackedUrl);
                    }
                }

                if (string.IsNullOrWhiteSpace(payload))
                    return null;

                var entries = ParseWebLicenseEntries(payload);
                if (entries.Count == 0)
                    return null;

                string kurumAdi = await GetKursAdiAsync();
                string localLisansNo = await GetCurrentLicenseNoForRequestAsync();
                string localMusteriNo = await GetMusteriNoAsync();
                string normKurum = string.IsNullOrWhiteSpace(kurumAdi) ? string.Empty : Normalize(kurumAdi);
                string normLisansNo = (localLisansNo ?? string.Empty).Trim();
                string normMusteriNo = (localMusteriNo ?? string.Empty).Trim();

                WebLicenseEntry secilen = null;

                // 1) En guvenilir eslesme: lisans no
                if (!string.IsNullOrWhiteSpace(normLisansNo))
                    secilen = entries.Find(x => string.Equals((x.LisansNo ?? string.Empty).Trim(), normLisansNo, StringComparison.OrdinalIgnoreCase));

                // 2) Musteri no
                if (secilen == null && !string.IsNullOrWhiteSpace(normMusteriNo))
                    secilen = entries.Find(x => string.Equals((x.MusteriNo ?? string.Empty).Trim(), normMusteriNo, StringComparison.OrdinalIgnoreCase));

                // 3) Kurum adi fallback
                if (secilen == null && !string.IsNullOrWhiteSpace(normKurum))
                {
                    foreach (var e in entries)
                    {
                        string normFirma = Normalize(e.FirmaAdi);
                        if (normFirma == normKurum || normKurum.Contains(normFirma) || normFirma.Contains(normKurum))
                        {
                            secilen = e;
                            break;
                        }
                    }
                }

                if (secilen == null)
                    return null;

                var m = new LisansModel();
                m.MusteriNo = secilen.MusteriNo;
                m.LisansKurum = (secilen.FirmaAdi ?? string.Empty).Trim();
                m.LisansNo = secilen.LisansNo;
                m.ValidUntil = secilen.BitisTarihi;
                m.Durum = NormalizeLicenseStatus(secilen.Durum);
                m.Versiyon = string.IsNullOrWhiteSpace(secilen.Surum) ? VersionService.GetVersion() : secilen.Surum;
                return m;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> GetKursAdiAsync()
        {
            if (string.IsNullOrWhiteSpace(_serverAyar?.ConnectionString))
                return null;
            try
            {
                string tableName = ResolveKursBilgiTableName(_serverAyar.ConnectionString);
                if (string.IsNullOrWhiteSpace(tableName))
                    return null;
                using (var conn = new SqlConnection(_serverAyar.ConnectionString))
                using (var cmd = new SqlCommand("SELECT TOP 1 ISNULL(KURS_ADI,'') FROM [" + tableName + "]", conn))
                {
                    await conn.OpenAsync();
                    var val = await cmd.ExecuteScalarAsync();
                    return val == null ? null : val.ToString().Trim();
                }
            }
            catch
            {
                return null;
            }
        }

        private System.Collections.Generic.List<WebLicenseEntry> ParseWebLicenseEntries(string payload)
        {
            var list = new System.Collections.Generic.List<WebLicenseEntry>();
            if (string.IsNullOrWhiteSpace(payload)) return list;

            var lines = payload.Replace("\r", "").Split('\n');
            foreach (var raw in lines)
            {
                var line = (raw ?? string.Empty).Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                var p = line.Split('|');
                if (p.Length < 6) continue;

                DateTime bitis;
                if (!DateTime.TryParseExact((p[2] ?? "").Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out bitis))
                    bitis = DateTime.MinValue;

                list.Add(new WebLicenseEntry
                {
                    FirmaAdi = (p[0] ?? "").Trim(),
                    LisansNo = (p[1] ?? "").Trim(),
                    BitisTarihi = bitis,
                    UrunAdi = (p[3] ?? "").Trim(),
                    Surum = (p[4] ?? "").Trim(),
                    Durum = (p[5] ?? "").Trim(),
                    MusteriNo = p.Length >= 7 ? (p[6] ?? "").Trim() : string.Empty
                });

                if (list.Count >= 5) break;
            }

            return list;
        }

        private string Normalize(string t)
        {
            if (string.IsNullOrWhiteSpace(t)) return string.Empty;
            t = t.ToUpper(new CultureInfo("tr-TR")).Trim();
            t = t.Replace("ÖZEL ", "").Replace(" MOTORLU TAŞIT SÜRÜCÜLERİ KURSU", "").Replace(" MOTORLU TASIT SURUCULERI KURSU", "");
            return t.Replace("  ", " ").Trim();
        }

        private LisansCalismaModu ResolveLisansModu(LisansModel lisans)
        {
            if (lisans == null)
                return LisansCalismaModu.Demo;

            var status = NormalizeLicenseStatus(lisans.Durum);
            if (string.Equals(status, "Aktif", StringComparison.OrdinalIgnoreCase))
                return LisansCalismaModu.Active;
            if (string.Equals(status, "Passive", StringComparison.OrdinalIgnoreCase))
                return LisansCalismaModu.Passive;
            return LisansCalismaModu.Demo;
        }

        private string NormalizeLicenseStatus(string rawStatus)
        {
            var value = (rawStatus ?? string.Empty).Trim().ToLowerInvariant();
            if (value == "aktif" || value == "active")
                return "Aktif";
            if (value == "passive" || value == "pasive" || value == "pasif")
                return "Passive";
            if (value == "bekleyen" || value == "pending" || value == "waiting" || value == "onay bekliyor")
                return "Bekleyen";
            return "Demo";
        }

        private async Task<bool> CheckLicenseEndpointOnlineAsync()
        {
            if (LisansPolitikasi.IsWebLicenseCallSuppressed)
                return false;

            try
            {
                string lisansNo = await GetCurrentLicenseNoForRequestAsync();
                string musteriNo = await GetMusteriNoAsync();
                string url = AppendLicenseTrackingParams("http://mehmetfer.com.tr/lisans.php?token=KOLERA2026", lisansNo, musteriNo);
                using (var wc = new WebClient())
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    var text = await wc.DownloadStringTaskAsync(url);
                    return !string.IsNullOrWhiteSpace(text);
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> GetMusteriNoAsync()
        {
            if (string.IsNullOrWhiteSpace(_serverAyar?.ConnectionString))
                return string.Empty;
            try
            {
                string tableName = ResolveKursBilgiTableName(_serverAyar.ConnectionString);
                if (string.IsNullOrWhiteSpace(tableName))
                    return string.Empty;
                using (var conn = new SqlConnection(_serverAyar.ConnectionString))
                using (var cmd = new SqlCommand("SELECT TOP 1 ISNULL(MUSTERI_NO,'') FROM [" + tableName + "]", conn))
                {
                    await conn.OpenAsync();
                    var val = await cmd.ExecuteScalarAsync();
                    return val == null ? string.Empty : val.ToString().Trim();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> GetCurrentLicenseNoForRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(_serverAyar?.ConnectionString))
                return string.IsNullOrWhiteSpace(_lisans?.LisansNo) ? string.Empty : _lisans.LisansNo.Trim();
            try
            {
                using (var conn = new SqlConnection(_serverAyar.ConnectionString))
                using (var cmd = new SqlCommand("SELECT TOP 1 ISNULL(LISANS_NO,'') FROM APP_LOCAL_LISANS ORDER BY ID DESC", conn))
                {
                    await conn.OpenAsync();
                    var val = await cmd.ExecuteScalarAsync();
                    var localNo = val == null ? string.Empty : val.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(localNo))
                        return localNo;
                }
            }
            catch
            {
            }
            if (!string.IsNullOrWhiteSpace(_lisans?.LisansNo))
                return _lisans.LisansNo.Trim();
            return string.Empty;
        }

        private static string ResolveKursBilgiTableName(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return null;

            const string sql = @"
SELECT TOP 1 TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND UPPER(TABLE_NAME) IN ('KURSBILGIPARAM')
ORDER BY TABLE_NAME;";

            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var o = cmd.ExecuteScalar();
                return o == null || o == DBNull.Value ? null : Convert.ToString(o);
            }
        }

        private string AppendLicenseTrackingParams(string baseUrl, string lisansNo, string musteriNo)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return baseUrl;

            string sep = baseUrl.Contains("?") ? "&" : "?";
            return baseUrl + sep
                + "lisans_no=" + Uri.EscapeDataString((lisansNo ?? string.Empty).Trim())
                + "&musteri_no=" + Uri.EscapeDataString((musteriNo ?? string.Empty).Trim());
        }

        private void AppendLicenseCheckInfo()
        {
            var info = LisansPolitikasi.GetLastLicenseCheckText();
            var offline = LisansPolitikasi.IsInternetAvailableForLicense
                ? string.Empty
                : " | Offline Kayit Hakki: " + LisansPolitikasi.OfflineRemainingWriteCount;
            LblLisansDurum.Text = LblLisansDurum.Text + Environment.NewLine + info + offline;
        }

        private class WebLicenseEntry
        {
            public string FirmaAdi { get; set; }
            public string LisansNo { get; set; }
            public DateTime BitisTarihi { get; set; }
            public string UrunAdi { get; set; }
            public string Surum { get; set; }
            public string Durum { get; set; }
            public string MusteriNo { get; set; }
        }

        private void Btn_Lisans_Click(object sender, EventArgs e)
        {
            using (var frm = new LisansDetayForm(_serverAyar?.ConnectionString, _lisans))
            {
                frm.ShowDialog(this);
            }
        }

        private void Yedek_Button_Click(object sender, EventArgs e)
        {
            using (var frm = new YedekTanimlariForm(_serverAyar?.ConnectionString))
            {
                frm.ShowDialog(this);
            }
        }

        private void Ayarlar_Button_Click(object sender, EventArgs e)
        {
            using (var frm = new ParametrelerForm(_serverAyar?.ConnectionString))
            {
                frm.ShowDialog(this);
            }
        }

        private void Btn_Parametre_Click(object sender, EventArgs e)
        {
            using (var frm = new ParametrelerForm(_serverAyar?.ConnectionString))
            {
                frm.ShowDialog(this);
            }
        }

        private void SohbetMenusunuKaldir()
        {
            var mevcut = Menumain.Items["Btn_Sohbet"] as ToolStripItem;
            if (mevcut != null)
            {
                Menumain.Items.Remove(mevcut);
            }
        }

        private void SohbetLoginButonuHazirla()
        {
            if (_btnSohbetLogin != null) return;

            _btnSohbetLogin = new Button
            {
                Name = "BtnSohbetFloating",
                Text = "Sohbet",
                Size = new Size(110, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 21, 41),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = true
            };
            _btnSohbetLogin.FlatAppearance.BorderSize = 0;
            _btnSohbetLogin.Location = new Point(panelHead.Width - _btnSohbetLogin.Width - 8, 10);
            _btnSohbetLogin.Click += Btn_Sohbet_Click;
            panelHead.Controls.Add(_btnSohbetLogin);
            _btnSohbetLogin.BringToFront();

            panelHead.Resize += (s, e) =>
            {
                if (_btnSohbetLogin == null) return;
                _btnSohbetLogin.Location = new Point(panelHead.Width - _btnSohbetLogin.Width - 8, 10);
                _btnSohbetLogin.BringToFront();
            };
        }

        private void Btn_Sohbet_Click(object sender, EventArgs e)
        {
            var kullanici = (ComboBox_KullaniciAdi.SelectedItem?.ToString() ?? Kullanici_txt.Text ?? "Kullanici").Trim();
            if (_sohbetForm == null || _sohbetForm.IsDisposed)
            {
                _sohbetForm = new SohbetForm(kullanici);
                _sohbetForm.Show(this);
                KonumlaSohbet();
                return;
            }

            KonumlaSohbet();
            _sohbetForm.Show();
            _sohbetForm.BringToFront();
            _sohbetForm.Focus();
        }

        private void KonumlaSohbet()
        {
            if (_sohbetForm == null || _sohbetForm.IsDisposed) return;
            _sohbetForm.KonumlaSagAlt(this);
        }


    }
}





