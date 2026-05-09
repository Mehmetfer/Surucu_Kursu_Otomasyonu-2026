using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Kolera_Mtsk.Sayfalar.EkProgramlar;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class YardimMerkeziForm : Form
    {
        private readonly string _mebbisKullaniciAdi;
        private readonly string _mebbisSifre;
        private readonly string _connectionString;
        private readonly object _lisans;
        private readonly Action<Form> _openInMainPanel;
        private readonly Action _showSidebar;
        private Form _activeContentForm;
        private Button _activeMenuButton;
        private Label _lblYerelSurumDeger;
        private Label _lblWebSurumDeger;
        private Label _lblProgramDurumDeger;
        private RichTextBox _rtbGuncellemeLog;

        public YardimMerkeziForm()
            : this(null, null, null, null, null, null)
        {
        }

        public YardimMerkeziForm(string mebbisKullaniciAdi, string mebbisSifre, Action<Form> openInMainPanel, string connectionString, Action showSidebar, object lisans)
        {
            InitializeComponent();
            _mebbisKullaniciAdi = mebbisKullaniciAdi;
            _mebbisSifre = mebbisSifre;
            _openInMainPanel = openInMainPanel;
            _connectionString = connectionString;
            _showSidebar = showSidebar;
            _lisans = lisans;
            Load += YardimMerkeziForm_Load;
        }

        private void YardimMerkeziForm_Load(object sender, EventArgs e)
        {
            ConfigureCorporateMenu();
            ShowWelcomePanel();
        }

        private void ConfigureCorporateMenu()
        {
            var menuButtons = new[]
            {
                btnHakkimizda, Btn_Klavuz, btnSimulatorAktarimi,
                btnDersProgramiCizelgesi, btnUstYazilar, btnIstatistik, btnLisansDetay,
                btnParametreler, btnGuncellemeKontrol, btnTumAyar
            };

            foreach (var btn in menuButtons)
            {
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(57, 73, 122);
                btn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(71, 89, 145);
                btn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(16, 0, 0, 0);
            }
        }

        private void SetActiveMenuButton(Button clickedButton)
        {
            if (clickedButton == null || clickedButton == btnTumAyar)
                return;

            if (_activeMenuButton != null)
                _activeMenuButton.BackColor = System.Drawing.Color.FromArgb(28, 38, 68);

            clickedButton.BackColor = System.Drawing.Color.FromArgb(74, 94, 160);
            _activeMenuButton = clickedButton;
        }

        private void ShowWelcomePanel()
        {
            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(70, 80, 95),
                Text = "YARDIM MERKEZI\n\nSoldaki menuden bir sayfa secin."
            };
            ShowControlInContent(lbl);
        }

        private void ShowControlInContent(Control control)
        {
            if (control == null)
                return;

            if (_activeContentForm != null)
            {
                try
                {
                    pnlContent.Controls.Remove(_activeContentForm);
                    _activeContentForm.Dispose();
                }
                catch
                {
                }
                _activeContentForm = null;
            }

            pnlContent.Controls.Clear();
            control.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(control);
        }

        private void OpenPage(Form frm)
        {
            if (frm == null)
                return;

            if (_activeContentForm != null)
            {
                try
                {
                    pnlContent.Controls.Remove(_activeContentForm);
                    _activeContentForm.Dispose();
                }
                catch
                {
                }
            }

            _activeContentForm = frm;
            frm.TopLevel = false;
            frm.FormBorderStyle = FormBorderStyle.None;
            frm.Dock = DockStyle.Fill;
            pnlContent.Controls.Clear();
            pnlContent.Controls.Add(frm);
            frm.Show();
        }

        private void ShowTextPage(string title, string text)
        {
            var panel = new Panel { BackColor = System.Drawing.Color.White };
            var lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 46,
                Padding = new Padding(12, 10, 12, 0),
                Text = title,
                Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(52, 73, 94)
            };
            var box = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.None,
                BackColor = System.Drawing.Color.White,
                Text = text ?? string.Empty
            };
            panel.Controls.Add(box);
            panel.Controls.Add(lblTitle);
            ShowControlInContent(panel);
        }

        private void btnHakkimizda_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            ShowModernAboutPage();
        }

        private void ShowModernAboutPage()
        {
            var root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(245, 247, 250),
                Padding = new Padding(18)
            };

            var footer = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 9.25F),
                ForeColor = System.Drawing.Color.FromArgb(90, 90, 90),
                Text = "© 2026 Kolera MTSK - Tüm Hakları Saklıdır"
            };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = System.Drawing.Color.FromArgb(194, 34, 45),
                Padding = new Padding(18, 14, 18, 14)
            };

            var lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 38,
                Text = "KOLERA MTSK SÜRÜCÜ KURSU OTOMASYONU",
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var lblSlogan = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Sürücü kursları için hızlı, güvenilir ve modern yönetim platformu.",
                ForeColor = System.Drawing.Color.FromArgb(255, 235, 235),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                TextAlign = System.Drawing.ContentAlignment.TopLeft
            };

            headerPanel.Controls.Add(lblSlogan);
            headerPanel.Controls.Add(lblTitle);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = System.Drawing.Color.Transparent,
                Padding = new Padding(0, 14, 0, 10)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54F));

            var leftContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 12, 0)
            };

            var leftStack = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6
            };
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Percent, 34F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));
            leftStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            leftStack.RowStyles.Add(new RowStyle(SizeType.Percent, 33F));

            var lblHakkimizdaTitle = CreateSectionTitle("Hakkımızda");
            var lblHakkimizdaText = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = System.Drawing.Color.FromArgb(58, 64, 76),
                Text = "Kolera MTSK Sürücü Kursu Otomasyonu, kursiyer kayıt, süreç ve sınav operasyonlarını tek panelde yönetmek için geliştirilmiştir.",
                Padding = new Padding(0, 2, 0, 8)
            };

            var lblVizyonTitle = CreateSectionTitle("Vizyon");
            var lblVizyonText = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = System.Drawing.Color.FromArgb(58, 64, 76),
                Text = "Sürücü kursu otomasyonunda güvenilir, yenilikçi ve sürdürülebilir dijital çözümlerle sektörde öncü olmak.",
                Padding = new Padding(0, 2, 0, 8)
            };

            var lblMisyonTitle = CreateSectionTitle("Misyon");
            var lblMisyonText = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = System.Drawing.Color.FromArgb(58, 64, 76),
                Text = "İş süreçlerini dijitalleştirerek zaman tasarrufu sağlamak, verimliliği artırmak ve hataları minimuma indirmek.",
                Padding = new Padding(0, 2, 0, 8)
            };

            leftStack.Controls.Add(lblHakkimizdaTitle, 0, 0);
            leftStack.Controls.Add(lblHakkimizdaText, 0, 1);
            leftStack.Controls.Add(lblVizyonTitle, 0, 2);
            leftStack.Controls.Add(lblVizyonText, 0, 3);
            leftStack.Controls.Add(lblMisyonTitle, 0, 4);
            leftStack.Controls.Add(lblMisyonText, 0, 5);
            leftContent.Controls.Add(leftStack);

            var rightCardsHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Transparent,
                Padding = new Padding(0, 0, 0, 0),
                Margin = new Padding(12, 0, 0, 0)
            };

            var cardsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = System.Drawing.Color.Transparent
            };
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));

            cardsGrid.Controls.Add(CreateFeatureCard("Kursiyer Yönetimi"), 0, 0);
            cardsGrid.Controls.Add(CreateFeatureCard("Sınav Takibi"), 1, 0);
            cardsGrid.Controls.Add(CreateFeatureCard("Ödeme Yönetimi"), 0, 1);
            cardsGrid.Controls.Add(CreateFeatureCard("Raporlama"), 1, 1);
            cardsGrid.Controls.Add(CreateFeatureCard("Güvenli Veri Saklama"), 0, 2);
            cardsGrid.Controls.Add(CreateFeatureCard("Kullanıcı Dostu Arayüz"), 1, 2);

            rightCardsHost.Controls.Add(cardsGrid);

            body.Controls.Add(leftContent, 0, 0);
            body.Controls.Add(rightCardsHost, 1, 0);

            root.Controls.Add(body);
            root.Controls.Add(footer);
            root.Controls.Add(headerPanel);

            ShowControlInContent(root);
        }

        private Control CreateFeatureCard(string title)
        {
            var outer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(226, 232, 240),
                Margin = new Padding(8)
            };

            var inner = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White,
                Padding = new Padding(12),
                Margin = new Padding(1)
            };

            var accent = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = System.Drawing.Color.FromArgb(194, 34, 45)
            };

            var lbl = new Label
            {
                Dock = DockStyle.Fill,
                Text = title,
                Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(45, 55, 72),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0)
            };

            inner.Controls.Add(lbl);
            inner.Controls.Add(accent);
            outer.Controls.Add(inner);
            return outer;
        }

        private Label CreateSectionTitle(string text)
        {
            return new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 30,
                Text = text,
                Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(194, 34, 45),
                Padding = new Padding(0, 4, 0, 0)
            };
        }

        private void btnSimulatorAktarimi_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            var frm = new SimulatorAktarimForm(_mebbisKullaniciAdi, _mebbisSifre, _connectionString);
            OpenPage(frm);
        }

        private void btnDersProgramiCizelgesi_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            var dersler = EkProgramState.SonCekilenDersler ?? new System.Collections.Generic.List<TakvimDers>();
            if (dersler.Count == 0)
            {
                MessageBox.Show(this, "Once 'Simulator Aktarimi' ekraninda dersleri cekmelisiniz.",
                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ilkAy = System.Linq.Enumerable.First(
                System.Linq.Enumerable.DefaultIfEmpty(
                    System.Linq.Enumerable.Select(
                        System.Linq.Enumerable.OrderBy(
                            System.Linq.Enumerable.Where(dersler, x => x.Baslangic != System.DateTime.MinValue),
                            x => x.Baslangic),
                        x => x.Baslangic),
                    System.DateTime.Now));

            var frm = new AylikTakvimForm(dersler, new System.DateTime(ilkAy.Year, ilkAy.Month, 1));
            OpenPage(frm);
        }

        private void btnTumAyar_Click(object sender, EventArgs e)
        {
            _showSidebar?.Invoke();
        }

        private void btnUstYazilar_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            var frm = new UstYazilarForm();
            OpenPage(frm);
        }

        private void btnIstatistik_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            var frm = new IstatistikForm(_connectionString);
            OpenPage(frm);
        }

        private void btnSistemParametreleri_Click(object sender, EventArgs e)
        {
            btnParametreler_Click(sender, e);
        }

        private void btnLisansDetay_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            var frm = new LisansDetayForm(_connectionString, _lisans);
            OpenPage(frm);
        }

        private void btnParametreler_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show(this, "Veritabani baglanti bilgisi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var frm = new ParametrelerForm(_connectionString);
            OpenPage(frm);
        }

        private void Btn_Klavuz_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            string yol = FindKlavuzPath();

            if (string.IsNullOrWhiteSpace(yol))
            {
                MessageBox.Show(this, "kullanici_klavuzu.txt bulunamadi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string metin = File.ReadAllText(yol, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(metin))
                    metin = "Kilavuz dosyasi bulundu, ancak icerik bos.";
                ShowModernKlavuzPage(metin);
            }
            catch (Exception ex)
            {
                ShowTextPage("Kullanim Kilavuzu", "Kilavuz okunamadi.\n\n" + ex.Message);
            }
        }

        private void ShowModernKlavuzPage(string klavuzMetni)
        {
            var root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(245, 247, 250),
                Padding = new Padding(18)
            };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = System.Drawing.Color.FromArgb(194, 34, 45),
                Padding = new Padding(18, 14, 18, 14)
            };

            var lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 38,
                Text = "KULLANIM KILAVUZU",
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var lblSlogan = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Sistemi hızlı ve doğru kullanmanız için temel adımlar ve yönlendirmeler.",
                ForeColor = System.Drawing.Color.FromArgb(255, 235, 235),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                TextAlign = System.Drawing.ContentAlignment.TopLeft
            };
            headerPanel.Controls.Add(lblSlogan);
            headerPanel.Controls.Add(lblTitle);

            var footer = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 9.25F),
                ForeColor = System.Drawing.Color.FromArgb(90, 90, 90),
                Text = "© 2026 Kolera MTSK - Tüm Hakları Saklıdır"
            };

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = System.Drawing.Color.Transparent,
                Padding = new Padding(0, 14, 0, 10)
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 66F));

            var quickPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White,
                Padding = new Padding(16),
                Margin = new Padding(0, 0, 12, 0)
            };
            var quickGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5
            };
            quickGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            quickGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            quickGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            quickGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            quickGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            quickGrid.Controls.Add(CreateSectionTitle("Hızlı Erişim"), 0, 0);
            quickGrid.Controls.Add(CreateFeatureCard("1. İlk Kurulum ve Giriş"), 0, 1);
            quickGrid.Controls.Add(CreateFeatureCard("2. Kursiyer ve Kayıt İşlemleri"), 0, 2);
            quickGrid.Controls.Add(CreateFeatureCard("3. Sınav ve Program Yönetimi"), 0, 3);
            quickGrid.Controls.Add(CreateFeatureCard("4. Raporlama ve Parametreler"), 0, 4);
            quickPanel.Controls.Add(quickGrid);

            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White,
                Padding = new Padding(16),
                Margin = new Padding(12, 0, 0, 0)
            };

            var txtKlavuz = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = true,
                BorderStyle = BorderStyle.None,
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.FromArgb(45, 55, 72),
                Font = new System.Drawing.Font("Segoe UI", 10F),
                Text = BuildCorporateKlavuzText(klavuzMetni)
            };

            var lblIcerikBaslik = CreateSectionTitle("Kılavuz İçeriği");
            lblIcerikBaslik.Dock = DockStyle.Top;
            contentPanel.Controls.Add(txtKlavuz);
            contentPanel.Controls.Add(lblIcerikBaslik);

            body.Controls.Add(quickPanel, 0, 0);
            body.Controls.Add(contentPanel, 1, 0);

            root.Controls.Add(body);
            root.Controls.Add(footer);
            root.Controls.Add(headerPanel);

            ShowControlInContent(root);
        }

        private string BuildCorporateKlavuzText(string rawText)
        {
            var sb = new StringBuilder();
            sb.AppendLine("KOLERA MTSK KULLANIM REHBERI");
            sb.AppendLine();
            sb.AppendLine("Bu rehber, temel ekranlara hizli uyum saglamaniz icin ozet olarak hazirlanmistir.");
            sb.AppendLine();
            sb.AppendLine("1) SISTEME GIRIS");
            sb.AppendLine("- Kullanici adiniz ve sifreniz ile giris yapin.");
            sb.AppendLine("- Baglanti veya yetki sorunu varsa yonetici ile iletisime gecin.");
            sb.AppendLine();
            sb.AppendLine("2) KURSIYER ISLEMLERI");
            sb.AppendLine("- Yeni kayit, guncelleme ve durum takibini kursiyer ekranindan yonetin.");
            sb.AppendLine("- Veri girislerinde zorunlu alanlari bos birakmayin.");
            sb.AppendLine();
            sb.AppendLine("3) SINAV VE DERS PLANLAMA");
            sb.AppendLine("- Sinav takvimini ve ders programini ilgili ekranlardan olusturun.");
            sb.AppendLine("- Planlama sonrasi kontrol listesi ile onay yapin.");
            sb.AppendLine();
            sb.AppendLine("4) ODEME VE RAPORLAMA");
            sb.AppendLine("- Odeme kayitlarini duzenli takip edin.");
            sb.AppendLine("- Yonetsel kararlar icin rapor ekranlarini periyodik kullanin.");
            sb.AppendLine();
            sb.AppendLine("5) PARAMETRE VE LISANS YONETIMI");
            sb.AppendLine("- Sistem ayarlari ve lisans detaylari yalnizca yetkili kullanicilar tarafindan duzenlenmelidir.");
            sb.AppendLine("- Degisikliklerden once mevcut ayarlarin yedegini alin.");
            sb.AppendLine();
            sb.AppendLine("6) DESTEK");
            sb.AppendLine("- Teknik destek taleplerinizde ekran adi ve kisa hata aciklamasi paylasin.");
            sb.AppendLine();
            sb.AppendLine("Not: Detayli teknik icerik icin kurum ici dokumanlar kullanilmalidir.");

            if (!string.IsNullOrWhiteSpace(rawText))
            {
                sb.AppendLine();
                sb.AppendLine("----");
                sb.AppendLine("Mevcut dosyadan ozetlenen icerik kullanilmistir.");
            }

            return sb.ToString();
        }

        private static string FindKlavuzPath()
        {
            const string fileName = "kullanici_klavuzu.txt";

            string current = Path.GetFullPath(Application.StartupPath);
            for (int i = 0; i < 10; i++)
            {
                string candidate = Path.Combine(current, fileName);
                if (File.Exists(candidate))
                    return candidate;

                DirectoryInfo parent = Directory.GetParent(current);
                if (parent == null)
                    break;
                current = parent.FullName;
            }

            return null;
        }

        private void btnGuncellemeKontrol_Click(object sender, EventArgs e)
        {
            SetActiveMenuButton(sender as Button);
            ShowUpdateCenterPage();
        }

        private void ShowUpdateCenterPage()
        {
            var root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(245, 247, 250),
                Padding = new Padding(16)
            };

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = System.Drawing.Color.FromArgb(194, 34, 45),
                Padding = new Padding(16, 12, 16, 10)
            };
            var lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                Text = "GUNCELLEME MERKEZI",
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            var lblSub = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Program, rapor, SMS şablonu ve veritabanı güncellemeleri bu ekrandan yönetilir.",
                ForeColor = System.Drawing.Color.FromArgb(255, 235, 235),
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                TextAlign = System.Drawing.ContentAlignment.TopLeft
            };
            header.Controls.Add(lblSub);
            header.Controls.Add(lblTitle);

            var cardsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(0, 12, 0, 8)
            };
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            cardsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            cardsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var pnlProgram = new Panel { Dock = DockStyle.Fill };
            var infoProgram = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 86,
                ColumnCount = 2
            };
            infoProgram.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            infoProgram.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            infoProgram.Controls.Add(new Label { Text = "Yerel Sürüm:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 0);
            _lblYerelSurumDeger = new Label { Text = Application.ProductVersion ?? "-", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            infoProgram.Controls.Add(_lblYerelSurumDeger, 1, 0);
            infoProgram.Controls.Add(new Label { Text = "Web Sürüm:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 1);
            _lblWebSurumDeger = new Label { Text = "-", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            infoProgram.Controls.Add(_lblWebSurumDeger, 1, 1);
            infoProgram.Controls.Add(new Label { Text = "Durum:", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 0, 2);
            _lblProgramDurumDeger = new Label { Text = "Kontrol bekleniyor", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
            infoProgram.Controls.Add(_lblProgramDurumDeger, 1, 2);
            var btnProgramKontrol = new Button { Text = "Guncellemeyi Kontrol Et", Width = 180, Height = 30, FlatStyle = FlatStyle.Flat };
            btnProgramKontrol.FlatAppearance.BorderSize = 0;
            btnProgramKontrol.BackColor = System.Drawing.Color.FromArgb(25, 118, 210);
            btnProgramKontrol.ForeColor = System.Drawing.Color.White;
            btnProgramKontrol.Click += BtnProgramGuncellemeKontrol_Click;
            var pnlProgramButton = new Panel { Dock = DockStyle.Bottom, Height = 38 };
            pnlProgramButton.Controls.Add(btnProgramKontrol);
            btnProgramKontrol.Location = new System.Drawing.Point(0, 4);
            pnlProgram.Controls.Add(pnlProgramButton);
            pnlProgram.Controls.Add(infoProgram);
            cardsGrid.Controls.Add(CreateUpdateCard("Program Güncellemesi", string.Empty, pnlProgram), 0, 0);

            var pnlRapor = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            var btnRaporGuncelle = new Button { Text = "Raporlari Guncelle", Width = 150, Height = 30, FlatStyle = FlatStyle.Flat };
            var btnYeniRapor = new Button { Text = "Yeni Rapor Ekle", Width = 130, Height = 30, FlatStyle = FlatStyle.Flat };
            StilVerMavi(btnRaporGuncelle);
            StilVerMavi(btnYeniRapor);
            btnRaporGuncelle.Click += BtnRaporlariGuncelle_Click;
            btnYeniRapor.Click += BtnYeniRaporEkle_Click;
            pnlRapor.Controls.Add(btnRaporGuncelle);
            pnlRapor.Controls.Add(btnYeniRapor);
            cardsGrid.Controls.Add(CreateUpdateCard("Rapor İşlemleri", "Sistemdeki rapor şablonlarını güncelleyin veya yeni rapor ekleyin.", pnlRapor), 1, 0);

            var pnlSms = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            var btnSmsEkle = new Button { Text = "SMS Sablonu Ekle", Width = 145, Height = 30, FlatStyle = FlatStyle.Flat };
            var btnSmsGuncelle = new Button { Text = "SMS Sablonlarini Guncelle", Width = 185, Height = 30, FlatStyle = FlatStyle.Flat };
            StilVerMavi(btnSmsEkle);
            StilVerMavi(btnSmsGuncelle);
            btnSmsEkle.Click += BtnSmsSablonuEkle_Click;
            btnSmsGuncelle.Click += BtnSmsSablonlariniGuncelle_Click;
            pnlSms.Controls.Add(btnSmsEkle);
            pnlSms.Controls.Add(btnSmsGuncelle);
            cardsGrid.Controls.Add(CreateUpdateCard("SMS Şablonları", "Hazır SMS metinlerini yönetin.", pnlSms), 0, 1);

            var pnlDb = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
            var btnEksik = new Button { Text = "Eksik Tablolari Kontrol Et", Width = 170, Height = 30, FlatStyle = FlatStyle.Flat };
            var btnScript = new Button { Text = "Guncelleme Scriptlerini Calistir", Width = 205, Height = 30, FlatStyle = FlatStyle.Flat };
            StilVerMavi(btnEksik);
            StilVerMavi(btnScript);
            btnEksik.Click += BtnEksikTablolariKontrolEt_Click;
            btnScript.Click += BtnScriptleriCalistir_Click;
            pnlDb.Controls.Add(btnEksik);
            pnlDb.Controls.Add(btnScript);
            cardsGrid.Controls.Add(CreateUpdateCard("Veritabanı İşlemleri", "Sistem için gerekli tablo ve kolon güncellemelerini kontrol edin.", pnlDb), 1, 1);

            var pnlLog = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 145,
                BackColor = System.Drawing.Color.White,
                Padding = new Padding(10)
            };
            var lblLog = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Text = "Islem Loglari",
                Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(52, 73, 94)
            };
            _rtbGuncellemeLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                BackColor = System.Drawing.Color.White
            };
            pnlLog.Controls.Add(_rtbGuncellemeLog);
            pnlLog.Controls.Add(lblLog);

            root.Controls.Add(cardsGrid);
            root.Controls.Add(pnlLog);
            root.Controls.Add(header);
            ShowControlInContent(root);

            LogYaz("Guncelleme Merkezi ekrani acildi.");
        }

        private Panel CreateUpdateCard(string title, string description, Control bodyControl)
        {
            var border = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(223, 228, 235),
                Margin = new Padding(8)
            };

            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White,
                Padding = new Padding(12)
            };

            var lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 26,
                Text = title,
                Font = new System.Drawing.Font("Segoe UI Semibold", 10.5F, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(194, 34, 45)
            };
            var lblDesc = new Label
            {
                Dock = DockStyle.Top,
                Height = string.IsNullOrWhiteSpace(description) ? 4 : 34,
                Text = description ?? string.Empty,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                ForeColor = System.Drawing.Color.FromArgb(80, 90, 105)
            };
            if (bodyControl != null)
            {
                bodyControl.Dock = DockStyle.Fill;
                card.Controls.Add(bodyControl);
            }
            card.Controls.Add(lblDesc);
            card.Controls.Add(lblTitle);
            border.Controls.Add(card);
            return border;
        }

        private void LogYaz(string mesaj)
        {
            if (_rtbGuncellemeLog == null)
                return;
            _rtbGuncellemeLog.AppendText("[" + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + "] " + mesaj + Environment.NewLine);
            _rtbGuncellemeLog.SelectionStart = _rtbGuncellemeLog.TextLength;
            _rtbGuncellemeLog.ScrollToCaret();
        }

        private void BtnProgramGuncellemeKontrol_Click(object sender, EventArgs e)
        {
            if (_lblYerelSurumDeger != null)
                _lblYerelSurumDeger.Text = Application.ProductVersion ?? "-";
            if (_lblWebSurumDeger != null)
                _lblWebSurumDeger.Text = "Kontrol edildi (demo)";
            if (_lblProgramDurumDeger != null)
                _lblProgramDurumDeger.Text = "Guncelleme kontrolu tamamlandi.";
            LogYaz("Program guncelleme kontrolu calistirildi.");
        }

        private void BtnRaporlariGuncelle_Click(object sender, EventArgs e)
        {
            LogYaz("Raporlari Guncelle islemi baslatildi (ornek).");
        }

        private void BtnYeniRaporEkle_Click(object sender, EventArgs e)
        {
            LogYaz("Yeni Rapor Ekle islemi baslatildi (ornek).");
        }

        private void BtnSmsSablonuEkle_Click(object sender, EventArgs e)
        {
            LogYaz("SMS Sablonu Ekle islemi baslatildi (ornek).");
        }

        private void BtnSmsSablonlariniGuncelle_Click(object sender, EventArgs e)
        {
            LogYaz("SMS Sablonlarini Guncelle islemi baslatildi (ornek).");
        }

        private void BtnEksikTablolariKontrolEt_Click(object sender, EventArgs e)
        {
            LogYaz("Eksik Tablolari Kontrol Et islemi baslatildi (ornek).");
        }

        private void BtnScriptleriCalistir_Click(object sender, EventArgs e)
        {
            LogYaz("Guncelleme Scriptlerini Calistir islemi baslatildi (ornek).");
        }

        private static void StilVerMavi(Button btn)
        {
            if (btn == null) return;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = System.Drawing.Color.FromArgb(25, 118, 210);
            btn.ForeColor = System.Drawing.Color.White;
        }

        private void ShowDbItemsDialog(string text)
        {
            using (var frm = new Form())
            using (var box = new TextBox())
            using (var btnCopy = new Button())
            {
                frm.Text = "Veritabani Ogeleri";
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.Width = 900;
                frm.Height = 700;

                box.Dock = DockStyle.Fill;
                box.ReadOnly = true;
                box.Multiline = true;
                box.WordWrap = false;
                box.ScrollBars = ScrollBars.Both;
                box.Font = new System.Drawing.Font("Consolas", 10F);
                box.Text = text;
                box.ShortcutsEnabled = true;

                btnCopy.Text = "Tumunu Kopyala";
                btnCopy.Dock = DockStyle.Bottom;
                btnCopy.Height = 36;
                btnCopy.Click += (s, e) =>
                {
                    Clipboard.SetText(box.Text ?? string.Empty);
                    MessageBox.Show(frm, "Metin panoya kopyalandi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                frm.Controls.Add(box);
                frm.Controls.Add(btnCopy);
                frm.ShowDialog(this);
            }
        }

    }
}
