using Kolera.Mebbis;
using Kolera.Mebbis.Engine;
using Kolera.Mebbis.Models;
using Kolera.Evrak.Models;
using Kolera_Mtsk.Mebbis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EngineMebbisEngine = Kolera.Mebbis.Engine.MebbisEngine;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class MebbisWebForm : Form
    {
        private EngineMebbisEngine _engine;
        private MebbisAutomationEngine _automation;

        private readonly string _kullaniciAdi;
        private readonly string _sifre;
        private readonly string _connectionString;
        private Color _defaultMainMenuBackColor;
        private Color _defaultMainMenuForeColor;

        private MebbisKursiyerModel _kursiyer;
        private KursiyerEvrak_Model _evrak;
        private byte[] _resim;

        private DateTime _lastLoginAttempt = DateTime.MinValue;
        private DateTime _suppressAutoLoginUntilUtc = DateTime.MinValue;
        private List<Button> _actionButtons;
        private Button _btnCanliKamera;
        private WebCamResimForm _webCamResimForm;
        private readonly HashSet<string> _onaylananEvraklar = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // WinForms Designer formu parametresiz olusturur.
        public MebbisWebForm()
            : this(null, null, null, null, null)
        {
        }

        public MebbisWebForm(
            string kullaniciAdi,
            string sifre,
            MebbisKursiyerModel kursiyer,
            byte[] resim,
            KursiyerEvrak_Model evrak = null,
            string connectionString = null)
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _kullaniciAdi = kullaniciAdi;
            _sifre = sifre;
            _kursiyer = kursiyer;
            _resim = resim;
            _evrak = evrak;
            _connectionString = connectionString ?? string.Empty;

            Load += FormLoad;
            FormClosing += (s, e) => { e.Cancel = true; Hide(); };
        }

        // ================= LOAD =================
        private void FormLoad(object sender, EventArgs e)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.DocumentCompleted += DocumentCompleted;

            _engine = new EngineMebbisEngine(webBrowser1);
            _automation = new MebbisAutomationEngine(webBrowser1);
            _automation.SequenceCompleted += OnAutomationSequenceCompleted;
            WireButtons();
            Btn_HizliAktar.Enabled = false;
            Btn_HizliAktar.Visible = false;
            Btn2_Menu2.Enabled = false;
            Btn2_Menu2.Visible = false;

            webBrowser1.Navigate("https://mebbis.meb.gov.tr/default.aspx?NoSession");

            KursiyerYukle(_kursiyer, _resim);
            CacheMainMenuDefaultColors();
        }

        // ================= LOGIN CONTROL =================
        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.Document == null) return;
            if (e.Url != webBrowser1.Url) return;
            if (DateTime.UtcNow < _suppressAutoLoginUntilUtc)
                return;

            var url = webBrowser1.Url?.ToString() ?? string.Empty;
            if (url.IndexOf("default.aspx", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Oturum düşerse yeniden login dene (rate-limit ile).
                if ((DateTime.Now - _lastLoginAttempt).TotalSeconds < 3)
                    return;

                _lastLoginAttempt = DateTime.Now;
                Login();
            }

            if (url.IndexOf("/SKT/skt02002.aspx", StringComparison.OrdinalIgnoreCase) >= 0)
                TryApplyVideoPreviewToCurrentPage();
        }

        private void WireButtons()
        {
            CacheActionButtons();
            // Ana menüler hangi sayfada olursa olsun önce aday kayıt ekranına gider.
            Btn_IslemiKes.Click += (s, e) => StopCurrentAutomation();

            Btn_OzelMTSK.Click += (s, e) =>
            {
                ActivateMainMenu(Btn_OzelMTSK);
                ActivateSubMenus(Btn_Menu1, Btn_Menu2);
                // Kullanici istegi: bu menude sadece aday alt aksiyonlari acilsin.
                Btn_Menu2.Enabled = false;
            };

            Btn_Resim.Click += (s, e) =>
            {
                Btn2_Menu2.Visible = true;
                Btn2_Menu2.Enabled = true;
                ActivateMainMenu(Btn_Resim);
                ActivateSubMenus(Btn2_Menu1, Btn2_Menu2);
            };

            Btn_OgrnBilgileri.Click += (s, e) =>
            {
                ActivateMainMenu(Btn_OgrnBilgileri);
                ActivateSubMenus(Btn3_Menu1, Btn3_Menu2);
                RunSequenceSafe(
                    new MebbisDonemSecIslem(webBrowser1, _kursiyer),
                    new MebbisAdayKimlikGetirIslem(webBrowser1, _kursiyer, _evrak),
                    new MebbisGotoUrlIslem(webBrowser1, _kursiyer, "/SKT/skt02003.aspx"),
                    new MebbisFotoAdaySecIslem(webBrowser1, _kursiyer),
                    new MebbisOgrBilgiIslem(webBrowser1, _kursiyer, _evrak, GetPreferredOgrenimPhotoBytes()));
            };
            Btn_Saglik.Click += (s, e) =>
            {
                ActivateMainMenu(Btn_Saglik);
                ActivateSubMenus(Btn4_Menu1, Btn4_Menu2);
                StartSaglikFlow(false);
            };
            Btn_SABIKA.Click += (s, e) =>
            {
                ActivateMainMenu(Btn_SABIKA);
                ActivateSubMenus(Btn5_Menu1, Btn5_Menu2);
                StartSabikaFlow(false);
            };
            Btn_imzasi.Click += (s, e) =>
            {
                ActivateMainMenu(Btn_imzasi);
                ActivateSubMenus(Btn6_Menu1);
                StartImzaFlow(false);
            };
            Btn_Sozlesme.Click += (s, e) =>
            {
                ActivateMainMenu(Btn_Sozlesme);
                ActivateSubMenus(Btn7_Menu1, Btn7_Menu2);
                StartSozlesmeFlow(false, false);
            };
            Btn_adres.Click += (s, e) =>
            {
                ActivateMainMenu(Btn_adres);
                ActivateSubMenus(Btn8_Menu1);
                NavigateToAdayKayit();
            };

            Btn_Menu1.Click += (s, e) => NavigateToAdayKayitFromDirectUrlWithDonemAndKimlikGetir();
            Btn_Menu2.Click += (s, e) => NavigateToAdayKayit();

            Btn2_Menu1.Click += (s, e) =>
                RunResimHazirlikFlow();

            Btn2_Menu2.Click += (s, e) =>
            {
                if (_webCamResimForm == null || _webCamResimForm.IsDisposed)
                {
                    _webCamResimForm = new WebCamResimForm(_connectionString, _kursiyer?.ID_Kursiyer ?? 0, _kursiyer?.Foto ?? _resim);
                    _webCamResimForm.StartPosition = FormStartPosition.Manual;
                    _webCamResimForm.LivePreviewUpdated += WebCamResimForm_LivePreviewUpdated;
                    _webCamResimForm.ImageCommitted += WebCamResimForm_ImageCommitted;
                    _webCamResimForm.FormClosed += WebCamResimForm_FormClosed;
                }

                PositionWebCamResimForm();
                _webCamResimForm.SelectVirtualSourceDefault();

                if (!_webCamResimForm.Visible)
                    _webCamResimForm.Show(this);
                else
                    _webCamResimForm.BringToFront();

                _webCamResimForm.Activate();
            };

            Btn3_Menu1.Click += (s, e) => StartOgrenimFlow(false);
            Btn3_Menu2.Click += (s, e) => StartOgrenimFlow(true);

            Btn4_Menu1.Click += (s, e) =>
                StartSaglikFlow(false);

            Btn4_Menu2.Click += (s, e) => StartSaglikFlow(true);

            Btn5_Menu1.Click += (s, e) =>
                StartSabikaFlow(false);

            Btn5_Menu2.Click += (s, e) => StartSabikaFlow(true);

            Btn6_Menu1.Click += (s, e) => StartImzaFlow(true);

            Btn7_Menu1.Click += (s, e) => StartSozlesmeFlow(false, true);

            Btn7_Menu2.Click += (s, e) => StartSozlesmeFlow(true, true);

            Btn8_Menu1.Click += (s, e) =>
            {
                if (!EnsureAdresOnay())
                    return;
                RunEvrakFlow("/SKT/SKT02012.aspx", new MebbisAdresIslem(webBrowser1, _kursiyer, _evrak));
            };

            Btn_fatura.Click += (s, e) =>
            {
                if (!EnsureFaturaOnay())
                    return;
                RunEvrakFlow("/SKT/skt02013.aspx", new MebbisFaturaIslem(webBrowser1, _kursiyer, _evrak));
            };

            // Btn_Ogrenim_Aktar bu formda bazı sürümlerde bulunmayabilir.
        }

        private void EnsureCanliKameraButton()
        {
            if (_btnCanliKamera != null)
                return;

            _btnCanliKamera = new Button
            {
                Name = "Btn_CanliKamera",
                Text = "Canli Kamera",
                Width = 120,
                Height = 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(Math.Max(10, ClientSize.Width - 140), 8)
            };
            _btnCanliKamera.Click += (s, e) =>
            {
                using (var frm = new MebbisCameraWebViewForm(
                    _kullaniciAdi,
                    _sifre,
                    "https://mebbis.meb.gov.tr/SKT/skt02002.aspx",
                    GetPreferredPhotoBytes()))
                    frm.ShowDialog(this);
            };

            Controls.Add(_btnCanliKamera);
            _btnCanliKamera.BringToFront();
        }

        private void NavigateToAdayKayit()
        {
            RunSequenceSafe(new MebbisOzelMtskIslem(webBrowser1, _kursiyer));
        }

        private void NavigateToAdayKayitWithDonem()
        {
            RunSequenceSafe(
                new MebbisOzelMtskIslem(webBrowser1, _kursiyer),
                new MebbisDonemSecIslem(webBrowser1, _kursiyer));
        }

        private void NavigateToAdayKayitWithDonemAndKimlikGetir()
        {
            RunSequenceSafe(
                new MebbisOzelMtskIslem(webBrowser1, _kursiyer),
                new MebbisDonemSecIslem(webBrowser1, _kursiyer),
                new MebbisAdayKimlikGetirIslem(webBrowser1, _kursiyer, _evrak));
        }

        private void NavigateToAdayKayitFromDirectUrlWithDonemAndKimlikGetir()
        {
            // Kullanıcı isteği: Btn_Menu1 menü adımlarına takılmadan doğrudan aday dönem ekranına gitsin.
            var current = webBrowser1.Url;
            var baseUrl = (current != null && !string.IsNullOrWhiteSpace(current.Host))
                ? $"{current.Scheme}://{current.Host}"
                : "https://mebbis.meb.gov.tr";

            webBrowser1.Navigate($"{baseUrl}/SKT/skt02001.aspx");

            RunSequenceSafe(
                new MebbisDonemSecIslem(webBrowser1, _kursiyer),
                new MebbisAdayKimlikGetirIslem(webBrowser1, _kursiyer, _evrak));
        }

        private void NavigateToAdayFotografFromDirectUrlWithDonemAndAdaySec()
        {
            var current = webBrowser1.Url;
            var baseUrl = (current != null && !string.IsNullOrWhiteSpace(current.Host))
                ? $"{current.Scheme}://{current.Host}"
                : "https://mebbis.meb.gov.tr";

            webBrowser1.Navigate($"{baseUrl}/SKT/skt02002.aspx");

            RunSequenceSafe(
                new MebbisFotoAdaySecIslem(webBrowser1, _kursiyer));
        }


        private void ActivateSubMenus(params Button[] activeButtons)
        {
            var allSubMenus = new[]
            {
                Btn_Menu1, Btn_Menu2,
                Btn2_Menu1, Btn2_Menu2,
                Btn3_Menu1, Btn3_Menu2,
                Btn4_Menu1, Btn4_Menu2,
                Btn5_Menu1, Btn5_Menu2,
                Btn6_Menu1,
                Btn7_Menu1, Btn7_Menu2,
                Btn8_Menu1
            };

            foreach (var btn in allSubMenus)
            {
                if (btn == null) continue;
                btn.Visible = false;
                btn.Enabled = false;
            }

            if (activeButtons == null || activeButtons.Length == 0)
                return;

            foreach (var btn in activeButtons)
            {
                if (btn == null) continue;
                btn.Visible = true;
                btn.Enabled = true;
            }
        }

        private void ActivateMainMenu(Button activeMain)
        {
            var allMainMenus = new[]
            {
                Btn_OzelMTSK, Btn_Resim, Btn_OgrnBilgileri, Btn_Saglik,
                Btn_SABIKA, Btn_imzasi, Btn_Sozlesme, Btn_adres
            };

            foreach (var btn in allMainMenus)
            {
                if (btn == null) continue;
                btn.Enabled = true; // Menuler arasi gecis icin tiklanabilir kalmali.
                bool isActive = (btn == activeMain);
                btn.BackColor = isActive ? Color.FromArgb(46, 125, 50) : _defaultMainMenuBackColor;
                btn.ForeColor = isActive ? Color.White : _defaultMainMenuForeColor;
                btn.Font = new Font(btn.Font, isActive ? FontStyle.Bold : FontStyle.Regular);
            }
        }

        private void CacheMainMenuDefaultColors()
        {
            var first = new[]
            {
                Btn_OzelMTSK, Btn_Resim, Btn_OgrnBilgileri, Btn_Saglik,
                Btn_SABIKA, Btn_imzasi, Btn_Sozlesme, Btn_adres
            }.FirstOrDefault(b => b != null);

            if (first == null)
                return;

            _defaultMainMenuBackColor = first.BackColor;
            _defaultMainMenuForeColor = first.ForeColor;
        }

        private void StartOgrenimFlow(bool openPreview)
        {
            if (!EnsureOgrenimOnay())
                return;

            var ogrBelgeResim = GetPreferredOgrenimPhotoBytes();
            if (openPreview && ogrBelgeResim != null && ogrBelgeResim.Length > 0)
            {
                using (var onizleme = new MebbisResimOnizlemeForm(
                    ogrBelgeResim,
                    "Ogrenim Belgesi Onizleme",
                    "Yuklenecek ogrenim belgesi resmini kontrol edin. Dogruysa 'Aktar' secin.",
                    "Ogrenim belgesi resmi bulunamadi."))
                {
                    if (onizleme.ShowDialog(this) != DialogResult.OK)
                        return;
                }
            }

            RunEvrakFlow(
                "/SKT/skt02003.aspx",
                new MebbisFotoAdaySecIslem(webBrowser1, _kursiyer),
                new MebbisOgrBilgiIslem(webBrowser1, _kursiyer, _evrak, ogrBelgeResim));
        }

        private void StartSaglikFlow(bool openPreview)
        {
            if (!EnsureSaglikOnay())
                return;

            var saglikResim = GetPreferredSaglikPhotoBytes();
            if (openPreview && saglikResim != null && saglikResim.Length > 0)
            {
                using (var onizleme = new MebbisResimOnizlemeForm(
                    saglikResim,
                    "Saglik Raporu Onizleme",
                    "Yuklenecek saglik raporu resmini kontrol edin. Dogruysa 'Aktar' secin.",
                    "Saglik raporu resmi bulunamadi."))
                {
                    if (onizleme.ShowDialog(this) != DialogResult.OK)
                        return;
                }
            }

            RunEvrakFlow("/SKT/skt02004.aspx", new MebbisSaglikIslem(webBrowser1, _kursiyer, _evrak));
        }

        private void StartSabikaFlow(bool openPreview)
        {
            if (!EnsureSabikaOnay())
                return;

            var sabikaResim = GetPreferredSabikaPhotoBytes();
            if (openPreview && sabikaResim != null && sabikaResim.Length > 0)
            {
                using (var onizleme = new MebbisResimOnizlemeForm(
                    sabikaResim,
                    "Sabika Belgesi Onizleme",
                    "Yuklenecek sabika belgesi resmini kontrol edin. Dogruysa 'Aktar' secin.",
                    "Sabika belgesi resmi bulunamadi."))
                {
                    if (onizleme.ShowDialog(this) != DialogResult.OK)
                        return;
                }
            }

            RunEvrakFlow("/SKT/skt02005.aspx", new MebbisSabikaIslem(webBrowser1, _kursiyer, _evrak));
        }

        private void StartImzaFlow(bool openPreview)
        {
            if (!EnsureImzaOnay())
                return;

            var imzaResim = GetPreferredImzaPhotoBytes();
            if (openPreview && imzaResim != null && imzaResim.Length > 0)
            {
                using (var onizleme = new MebbisResimOnizlemeForm(
                    imzaResim,
                    "Imza Resmi Onizleme",
                    "Yuklenecek imza resmini kontrol edin. Dogruysa 'Aktar' secin.",
                    "Imza resmi bulunamadi."))
                {
                    if (onizleme.ShowDialog(this) != DialogResult.OK)
                        return;
                }
            }

            RunEvrakFlow("/SKT/SKT02010.aspx", new MebbisImzaIslem(webBrowser1, _kursiyer, _evrak));
        }

        private void StartSozlesmeFlow(bool arkaYuz, bool openPreview)
        {
            if (!EnsureSozlesmeOnay(arkaYuz))
                return;

            var sozlesmeResim = arkaYuz ? GetPreferredSozlesmeArkaPhotoBytes() : GetPreferredSozlesmeOnPhotoBytes();
            if (openPreview && sozlesmeResim != null && sozlesmeResim.Length > 0)
            {
                var title = arkaYuz ? "Sozlesme Arka Onizleme" : "Sozlesme On Onizleme";
                var info = arkaYuz
                    ? "Yuklenecek sozlesme arka resmini kontrol edin. Dogruysa 'Aktar' secin."
                    : "Yuklenecek sozlesme on resmini kontrol edin. Dogruysa 'Aktar' secin.";
                var empty = arkaYuz ? "Sozlesme arka resmi bulunamadi." : "Sozlesme on resmi bulunamadi.";

                using (var onizleme = new MebbisResimOnizlemeForm(sozlesmeResim, title, info, empty))
                {
                    if (onizleme.ShowDialog(this) != DialogResult.OK)
                        return;
                }
            }

            RunEvrakFlow("/SKT/skt02011.aspx", new MebbisSozlesmeIslem(webBrowser1, _kursiyer, _evrak, sozlesmeResim, arkaYuz));
        }

        private void StartHizliAktarFlow()
        {
            // Tek tik hazirlik: aday alani acilir ve aday bilgisi getirilir.
            // Resim aktarimi sadece RESIM ISLEMLERI altindan yapilir.
            RunSequenceSafe(
                new MebbisOzelMtskIslem(webBrowser1, _kursiyer),
                new MebbisDonemSecIslem(webBrowser1, _kursiyer),
                new MebbisAdayKimlikGetirIslem(webBrowser1, _kursiyer, _evrak));
        }

        private void RunResimHazirlikFlow()
        {
            RunSequenceSafe(
                new MebbisOzelMtskIslem(webBrowser1, _kursiyer),
                new MebbisGotoUrlIslem(webBrowser1, _kursiyer, "/SKT/skt02002.aspx"),
                new MebbisFotoAdaySecIslem(webBrowser1, _kursiyer));
        }

        private void RunEvrakFlow(string relativeUrl, params IMebbisIslem[] ekIslemler)
        {
            var sequence = new List<IMebbisIslem>
            {
                new MebbisGotoUrlIslem(webBrowser1, _kursiyer, relativeUrl),
                new MebbisDonemSecIslem(webBrowser1, _kursiyer),
                new MebbisAdayKimlikGetirIslem(webBrowser1, _kursiyer, _evrak)
            };

            if (ekIslemler != null)
            {
                foreach (var islem in ekIslemler)
                {
                    if (islem != null)
                        sequence.Add(islem);
                }
            }

            RunSequenceSafe(sequence.ToArray());
        }

        private bool EnsureEvrakOnay(string evrakAdi)
        {
            return EnsureOnayOnce(evrakAdi ?? "Evrak", () =>
            {
                using (var onayForm = new MebbisSaglikOnayForm(_kursiyer, evrakAdi, GetEvrakPreviewImage(evrakAdi)))
                    return onayForm.ShowDialog(this) == DialogResult.OK;
            });
        }

        private bool EnsureSaglikOnay()
        {
            return EnsureOnayOnce("Saglik", () =>
            {
                using (var form = new MebbisSaglikOnayForm(_kursiyer, "Saglik", GetPreferredSaglikPhotoBytes()))
                    return form.ShowDialog(this) == DialogResult.OK;
            });
        }

        private bool EnsureSabikaOnay()
        {
            return EnsureOnayOnce("Sabika", () =>
            {
                using (var form = new MebbisSabikaOnayForm(_kursiyer, GetPreferredSabikaPhotoBytes()))
                    return form.ShowDialog(this) == DialogResult.OK;
            });
        }

        private bool EnsureOgrenimOnay()
        {
            return EnsureOnayOnce("Ogrenim", () =>
            {
                using (var form = new MebbisOgrenimOnayForm(_kursiyer, GetPreferredOgrenimPhotoBytes()))
                    return form.ShowDialog(this) == DialogResult.OK;
            });
        }

        private bool EnsureImzaOnay()
        {
            return EnsureOnayOnce("Imza", () =>
            {
                using (var form = new MebbisImzaOnayForm(_kursiyer, GetPreferredImzaPreviewBytes()))
                    return form.ShowDialog(this) == DialogResult.OK;
            });
        }

        private bool EnsureAdresOnay()
        {
            return EnsureOnayOnce("Adres", () =>
            {
                using (var form = new MebbisAdresOnayForm(_kursiyer, GetPreferredPhotoBytes()))
                    return form.ShowDialog(this) == DialogResult.OK;
            });
        }

        private bool EnsureFaturaOnay()
        {
            return EnsureOnayOnce("Fatura", () =>
            {
                using (var form = new MebbisFaturaOnayForm(_kursiyer, GetPreferredPhotoBytes()))
                    return form.ShowDialog(this) == DialogResult.OK;
            });
        }

        private bool EnsureSozlesmeOnay(bool arkaYuz)
        {
            if (arkaYuz)
            {
                return EnsureOnayOnce("Sozlesme Arka", () =>
                {
                    using (var form = new MebbisSozlesmeArkaOnayForm(_kursiyer, GetPreferredSozlesmeArkaPhotoBytes()))
                        return form.ShowDialog(this) == DialogResult.OK;
                });
            }

            return EnsureOnayOnce("Sozlesme On", () =>
            {
                using (var onForm = new MebbisSozlesmeOnOnayForm(_kursiyer, GetPreferredSozlesmeOnPhotoBytes()))
                    return onForm.ShowDialog(this) == DialogResult.OK;
            });
        }

        private bool EnsureOnayOnce(string evrakTipi, Func<bool> onayAc)
        {
            var onayAnahtari = BuildOnayKey(evrakTipi);
            if (_onaylananEvraklar.Contains(onayAnahtari))
                return true;

            if (onayAc == null)
                return false;

            var kabul = onayAc();
            if (kabul)
                _onaylananEvraklar.Add(onayAnahtari);

            return kabul;
        }

        private string BuildOnayKey(string evrakTipi)
        {
            var tcNo = (_kursiyer?.TC_NO ?? string.Empty).Trim();
            var kursiyerId = _kursiyer?.ID_Kursiyer ?? 0;
            var kimlik = !string.IsNullOrWhiteSpace(tcNo) ? tcNo : ("ID:" + kursiyerId);
            return kimlik + "|" + (evrakTipi ?? string.Empty).Trim();
        }

        private byte[] GetEvrakPreviewImage(string evrakAdi)
        {
            var key = (evrakAdi ?? string.Empty).ToLowerInvariant();
            if (key.Contains("saglik"))
                return GetPreferredSaglikPhotoBytes();
            if (key.Contains("sabika"))
                return GetPreferredSabikaPhotoBytes();
            if (key.Contains("imza"))
                return GetPreferredImzaPhotoBytes();
            if (key.Contains("sozlesme arka"))
                return GetPreferredSozlesmeArkaPhotoBytes();
            if (key.Contains("sozlesme"))
                return GetPreferredSozlesmeOnPhotoBytes();
            if (key.Contains("ogrenim"))
                return GetPreferredOgrenimPhotoBytes();

            return GetPreferredPhotoBytes();
        }

        private void Btn_Ogrenim_Aktar_Click(object sender, EventArgs e)
        {
            RunSequenceSafe(
                new MebbisOzelMtskIslem(webBrowser1, _kursiyer),
                new MebbisDonemSecIslem(webBrowser1, _kursiyer),
                new MebbisOgrBilgiIslem(webBrowser1, _kursiyer, _evrak));
        }

        private void CacheActionButtons()
        {
            _actionButtons = new List<Button>
            {
                Btn_OzelMTSK, Btn_Resim, Btn_OgrnBilgileri, Btn_Saglik,
                Btn_SABIKA, Btn_imzasi, Btn_Sozlesme, Btn_adres, Btn_fatura,
                Btn_HizliAktar,
                Btn_Menu1, Btn_Menu2, Btn2_Menu1, Btn2_Menu2, Btn3_Menu1, Btn3_Menu2,
                Btn4_Menu1, Btn4_Menu2, Btn5_Menu1, Btn5_Menu2, Btn6_Menu1, Btn7_Menu1,
                Btn7_Menu2, Btn8_Menu1
            };
        }

        private void SetActionButtonsEnabled(bool enabled)
        {
            if (_actionButtons == null) return;
            foreach (var button in _actionButtons)
            {
                if (button != null)
                    button.Enabled = enabled;
            }
        }

        private void OnAutomationSequenceCompleted()
        {
            if (IsDisposed) return;
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => SetActionButtonsEnabled(true)));
                return;
            }
            SetActionButtonsEnabled(true);
        }

        private void RunSequenceSafe(params IMebbisIslem[] islemler)
        {
            if (_automation == null)
                return;

            if (_automation.IsRunning)
            {
                MessageBox.Show(this, "Islem devam ediyor. Lutfen bitmesini bekleyin.", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetActionButtonsEnabled(false);
            if (Btn_IslemiKes != null)
                Btn_IslemiKes.Enabled = true;
            _automation.RunSequence(islemler);
        }

        private void StopCurrentAutomation()
        {
            if (_automation == null)
                return;

            _automation.Stop();
            SetActionButtonsEnabled(true);
            if (Btn_IslemiKes != null)
                Btn_IslemiKes.Enabled = true;

            MessageBox.Show(this, "Calisan islem durduruldu.", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ================= LOGIN (FIXED) =================
        private void Login()
        {
            var doc = webBrowser1.Document;
            if (doc == null) return;

            HtmlElement userBox = TryFindMebbisLoginUserInput(doc);
            HtmlElement passBox = TryFindMebbisLoginPasswordInput(doc);

            if (userBox == null || passBox == null)
            {
                foreach (HtmlElement el in doc.GetElementsByTagName("input"))
                {
                    string type = (el.GetAttribute("type") ?? "").ToLowerInvariant();
                    if (type == "text" && userBox == null)
                        userBox = el;
                    if (type == "password")
                        passBox = el;
                }
            }

            if (userBox != null)
                userBox.SetAttribute("value", _kullaniciAdi);

            if (passBox != null)
                passBox.SetAttribute("value", _sifre);

            foreach (HtmlElement el in doc.GetElementsByTagName("input"))
            {
                string type = (el.GetAttribute("type") ?? "").ToLowerInvariant();

                if (type == "submit" || type == "button")
                {
                    el.InvokeMember("click");
                    _suppressAutoLoginUntilUtc = DateTime.UtcNow.AddSeconds(12);
                    break;
                }
            }
        }

        private static HtmlElement TryFindMebbisLoginUserInput(HtmlDocument doc)
        {
            foreach (string id in new[]
                     {
                         "txtKullaniciAdi", "txtKullanici", "txtUserName", "tbKullanici", "KullaniciAdi",
                         "ctl00_ContentPlaceHolder1_txtKullanici", "ctl00$ContentPlaceHolder1$txtKullanici"
                     })
            {
                var el = doc.GetElementById(id);
                if (el != null)
                    return el;
            }

            return null;
        }

        private static HtmlElement TryFindMebbisLoginPasswordInput(HtmlDocument doc)
        {
            foreach (string id in new[]
                     {
                         "txtSifre", "txtParola", "txtPassword", "Parola", "Sifre",
                         "ctl00_ContentPlaceHolder1_txtSifre", "ctl00$ContentPlaceHolder1$txtSifre"
                     })
            {
                var el = doc.GetElementById(id);
                if (el != null)
                    return el;
            }

            return null;
        }

        // ================= KURSIYER LOAD =================
        public void KursiyerYukle(MebbisKursiyerModel kursiyer, byte[] resim)
        {
            _kursiyer = kursiyer;
            _resim = resim;

            if (_kursiyer == null) return;

            KursiyerAdi.Text = $"{_kursiyer.ADI} {_kursiyer.SOYADI}";
            Ehliyetsinif.Text = _kursiyer.SERTIFIKA_SINIFI;
            Onceki_Ehliyet.Text = _kursiyer.ONCE_SERT_SINIFI;
            Donem_Adi.Text = _kursiyer.DONEM_ADI;

            SetImage(_resim ?? _kursiyer.Foto);
        }

        public void KursiyerYukle(MebbisKursiyerModel kursiyer, byte[] resim, KursiyerEvrak_Model evrak)
        {
            _evrak = evrak;
            KursiyerYukle(kursiyer, resim);
        }

        // ================= IMAGE SAFE =================
        private void SetImage(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                Tnk_RESIM_Kursiyer.Image = null;
                return;
            }

            try
            {
                using (var ms = new MemoryStream(data))
                {
                    Tnk_RESIM_Kursiyer.Image = Image.FromStream(ms);
                    Tnk_RESIM_Kursiyer.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
            catch
            {
                Tnk_RESIM_Kursiyer.Image = null;
            }
        }

        private void TryApplyVideoPreviewToCurrentPage()
        {
            var url = webBrowser1?.Url?.ToString() ?? string.Empty;
            if (url.IndexOf("/SKT/skt02002.aspx", StringComparison.OrdinalIgnoreCase) < 0)
                return;

            var bytes = GetPreferredPhotoBytes();
            if (bytes == null || bytes.Length == 0)
                return;

            ApplyVideoPreviewToPage(bytes);
        }

        private void WebCamResimForm_LivePreviewUpdated(byte[] frameBytes)
        {
            if (IsDisposed || frameBytes == null || frameBytes.Length == 0)
                return;

            _resim = frameBytes;
            if (_kursiyer != null)
                _kursiyer.Foto = frameBytes;
            SetImage(frameBytes);
            TryApplyVideoPreviewToCurrentPage();
        }

        private void WebCamResimForm_ImageCommitted(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return;

            _resim = imageBytes;
            if (_kursiyer != null)
                _kursiyer.Foto = imageBytes;
            SetImage(imageBytes);
            TryApplyVideoPreviewToCurrentPage();

            RunSequenceSafe(
                new MebbisOzelMtskIslem(webBrowser1, _kursiyer),
                new MebbisDonemSecIslem(webBrowser1, _kursiyer),
                new MebbisAdayKimlikGetirIslem(webBrowser1, _kursiyer, _evrak),
                new MebbisGotoUrlIslem(webBrowser1, _kursiyer, "/SKT/skt02002.aspx"),
                new MebbisFotoAdaySecIslem(webBrowser1, _kursiyer),
                new MebbisResimIslem(webBrowser1, _kursiyer, imageBytes, null));
        }

        private void WebCamResimForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_webCamResimForm == null)
                return;

            _webCamResimForm.LivePreviewUpdated -= WebCamResimForm_LivePreviewUpdated;
            _webCamResimForm.ImageCommitted -= WebCamResimForm_ImageCommitted;
            _webCamResimForm.FormClosed -= WebCamResimForm_FormClosed;
            _webCamResimForm = null;
        }

        private void PositionWebCamResimForm()
        {
            if (_webCamResimForm == null)
                return;

            var area = Screen.FromControl(this).WorkingArea;
            int desiredX = this.Right + 8;
            int desiredY = this.Top;

            if (desiredX + _webCamResimForm.Width > area.Right)
                desiredX = this.Left - _webCamResimForm.Width - 8;

            if (desiredX < area.Left)
                desiredX = Math.Max(area.Left, area.Left + ((area.Width - _webCamResimForm.Width) / 2));

            if (desiredY + _webCamResimForm.Height > area.Bottom)
                desiredY = Math.Max(area.Top, area.Bottom - _webCamResimForm.Height);

            desiredY = Math.Max(area.Top, desiredY);
            _webCamResimForm.Location = new Point(desiredX, desiredY);
        }

        private void ApplyVideoPreviewToPage(byte[] imageBytes)
        {
            if (webBrowser1?.Document == null || imageBytes == null || imageBytes.Length == 0)
                return;

            string dataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes);
            string script = @"
(function(){
  var dataUrl = '" + dataUrl + @"';
  function applyDoc(doc){
    if(!doc) return;
    var list = [];
    var byId = doc.getElementById('video');
    if (byId) list.push(byId);
    var vids = doc.getElementsByTagName('video');
    for (var i=0;i<vids.length;i++) list.push(vids[i]);

    for (var j=0;j<list.length;j++){
      var v = list[j];
      if(!v) continue;
      try { v.setAttribute('poster', dataUrl); } catch(e){}
      try { v.style.backgroundImage = 'url(' + dataUrl + ')'; } catch(e){}
      try { v.style.backgroundSize = 'cover'; } catch(e){}
      try { v.style.backgroundPosition = 'center center'; } catch(e){}
      try { v.style.backgroundRepeat = 'no-repeat'; } catch(e){}

      var p = v.parentNode;
      if(!p) continue;
      if(!p.style.position || p.style.position === 'static') p.style.position = 'relative';
      var ov = doc.getElementById('__koleraVideoOverlay');
      if(!ov){
        ov = doc.createElement('img');
        ov.id = '__koleraVideoOverlay';
        ov.style.position = 'absolute';
        ov.style.left = '0';
        ov.style.top = '0';
        ov.style.width = '100%';
        ov.style.height = '100%';
        ov.style.objectFit = 'cover';
        ov.style.pointerEvents = 'none';
        ov.style.zIndex = '9999';
        p.appendChild(ov);
      }
      ov.src = dataUrl;
    }
  }
  applyDoc(document);
  var ifr = document.getElementsByTagName('iframe');
  for (var k=0;k<ifr.length;k++){
    try { applyDoc(ifr[k].contentWindow.document); } catch(ex){}
  }
})();";

            try
            {
                webBrowser1.Document.InvokeScript("eval", new object[] { script });
            }
            catch
            {
                // ignore injection errors
            }
        }

        private byte[] GetPreferredPhotoBytes()
        {
            var data = _resim ?? _kursiyer?.Foto;
            if (data != null && data.Length > 0)
                return data;

            if (Tnk_RESIM_Kursiyer?.Image == null)
                return null;

            try
            {
                using (var ms = new MemoryStream())
                using (var bmp = new Bitmap(Tnk_RESIM_Kursiyer.Image))
                {
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return ms.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        private byte[] GetPreferredOgrenimPhotoBytes()
        {
            return (_evrak?.ImgOgrBel != null && _evrak.ImgOgrBel.Length > 0) ? _evrak.ImgOgrBel : null;
        }

        private byte[] GetPreferredSaglikPhotoBytes()
        {
            if (_evrak?.ImgSaglik != null && _evrak.ImgSaglik.Length > 0)
                return _evrak.ImgSaglik;

            return GetPreferredPhotoBytes();
        }

        private byte[] GetPreferredSabikaPhotoBytes()
        {
            if (_evrak?.ImgSavcilik != null && _evrak.ImgSavcilik.Length > 0)
                return _evrak.ImgSavcilik;

            return GetPreferredPhotoBytes();
        }

        private byte[] GetPreferredImzaPhotoBytes()
        {
            return (_evrak?.ImgImza != null && _evrak.ImgImza.Length > 0) ? _evrak.ImgImza : null;
        }

        private byte[] GetPreferredImzaPreviewBytes()
        {
            var imza = GetPreferredImzaPhotoBytes();
            if (imza != null && imza.Length > 0)
                return imza;

            // Imza resmi yoksa onay ekrani bos kalmasin.
            return GetPreferredPhotoBytes();
        }

        private byte[] GetPreferredSozlesmeOnPhotoBytes()
        {
            return (_evrak?.ImgSozlesme_On != null && _evrak.ImgSozlesme_On.Length > 0) ? _evrak.ImgSozlesme_On : null;
        }

        private byte[] GetPreferredSozlesmeArkaPhotoBytes()
        {
            return (_evrak?.ImgSozlesme_Arka != null && _evrak.ImgSozlesme_Arka.Length > 0) ? _evrak.ImgSozlesme_Arka : null;
        }
    }
}