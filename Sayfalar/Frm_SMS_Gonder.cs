using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kolera_Mtsk.Services;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Frm_SMS_Gonder : Form
    {
        private WebView2 webView21;
        private bool bulkSayfaAcildi;
        private bool yonlendirmeYapiliyor;
        private bool customModHazirlandi;

        public Frm_SMS_Gonder()
        {
            InitializeComponent();
            OlusturWebView2();

            Load += Frm_SMS_Gonder_Load;
            Btn_Gonder.Click += Btn_Gonder_Click;
            Btn_PaneleGec.Click += Btn_PaneleGec_Click;
            FormClosed += (_, __) => TemizleWebView2();
        }

        private void TemizleWebView2()
        {
            if (webView21 == null)
                return;
            try
            {
                panelBrowser.Controls.Remove(webView21);
                webView21.Dispose();
            }
            catch
            {
                /* yoksay */
            }
            webView21 = null;
        }

        private static bool TasarimZamani =>
            LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private void OlusturWebView2()
        {
            if (TasarimZamani)
                return;

            webView21 = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = System.Drawing.Color.White,
                ZoomFactor = 1D
            };
            panelBrowser.Controls.Add(webView21);
        }

        private async void Frm_SMS_Gonder_Load(object sender, EventArgs e)
        {
            FormWorkspaceLayoutHelper.ApplyWorkingAreaMaximized(this);
            if (TasarimZamani || webView21 == null)
                return;

            try
            {
                await webView21.EnsureCoreWebView2Async(null);
                webView21.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                webView21.Source = new Uri("https://sms.myasist.ai/login");
                Lbl_Durum.Text = "Giriş sayfası yükleniyor...";
            }
            catch (Exception ex)
            {
                Lbl_Durum.Text = "WebView2 başlatılamadı.";
                MessageBox.Show(
                    "SMS ekranı için Microsoft Edge WebView2 Runtime kurulu olmalıdır.\r\n\r\n"
                    + "İndirme: https://developer.microsoft.com/microsoft-edge/webview2/\r\n\r\n"
                    + ex.Message,
                    "WebView2",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        public void KursiyerYukle(string gsm, string adSoyad, string mesaj)
        {
            Txt_Gsm.Text = gsm ?? string.Empty;
            Txt_Mesaj.Text = mesaj ?? string.Empty;
        }

        private async void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (webView21?.CoreWebView2 == null || !e.IsSuccess)
                return;

            try
            {
                string url = webView21.Source?.ToString() ?? string.Empty;

                if (url.Contains("/panel/sms/bulk"))
                {
                    if (!customModHazirlandi)
                    {
                        customModHazirlandi = true;
                        await CustomModunuHazirlaAsync();
                    }
                    return;
                }

                if (bulkSayfaAcildi || yonlendirmeYapiliyor)
                    return;

                string jsKontrol = @"
(function() {
    var username = document.getElementById('username');
    var password = document.getElementById('password');

    var activationInput =
        document.querySelector('input[name*=activation]') ||
        document.querySelector('input[id*=activation]') ||
        document.querySelector('input[name*=code]') ||
        document.querySelector('input[id*=code]') ||
        document.querySelector('input[name*=dogrulama]') ||
        document.querySelector('input[id*=dogrulama]');

    var panelHazir =
        document.querySelector('.user-menu') ||
        document.querySelector('a[href*=""/panel/sms/bulk""]') ||
        document.querySelector('a[href*=""/panel""]');

    return JSON.stringify({
        loginVar: !!username || !!password,
        activationVar: !!activationInput,
        panelHazir: !!panelHazir
    });
})();
";

                string sonuc = await webView21.CoreWebView2.ExecuteScriptAsync(jsKontrol);

                bool loginVar = sonuc.Contains("\"loginVar\":true");
                bool activationVar = sonuc.Contains("\"activationVar\":true");
                bool panelHazir = sonuc.Contains("\"panelHazir\":true");

                if (loginVar)
                {
                    Lbl_Durum.Text = "Kullanıcı adı ve şifre girişi bekleniyor...";
                    return;
                }

                if (activationVar)
                {
                    Lbl_Durum.Text = "Aktivasyon kodu girişi bekleniyor...";
                    return;
                }

                if (panelHazir)
                {
                    yonlendirmeYapiliyor = true;
                    Lbl_Durum.Text = "Toplu SMS sayfasına geçiliyor...";
                    webView21.CoreWebView2.Navigate("https://sms.myasist.ai/panel/sms/bulk");
                    bulkSayfaAcildi = true;
                }
            }
            catch (Exception ex)
            {
                Lbl_Durum.Text = "Sayfa kontrol hatası";
                MessageBox.Show("Hata: " + ex.Message, "SMS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task CustomModunuHazirlaAsync()
        {
            if (webView21?.CoreWebView2 == null)
                return;

            await Task.Delay(1200);

            string js = @"
(function() {
    var radio = document.querySelector('input[name=recipient_type][value=custom]');
    if (radio) {
        radio.checked = true;
        radio.dispatchEvent(new Event('change', { bubbles: true }));
    }

    if (typeof toggleRecipientType === 'function') {
        toggleRecipientType();
    }

    var input = document.getElementById('customFileInput');
    if (input) {
        input.style.display = 'block';
        input.style.opacity = '1';
        input.style.height = '40px';
    }
})();
";

            await webView21.CoreWebView2.ExecuteScriptAsync(js);
            Lbl_Durum.Text = "Farklı mesaj modu hazır (dosya seçin)";
        }

        private void Btn_PaneleGec_Click(object sender, EventArgs e)
        {
            try
            {
                if (webView21?.CoreWebView2 == null)
                {
                    MessageBox.Show("Tarayıcı henüz hazır değil. WebView2 kurulumunu kontrol edin.", "SMS",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                yonlendirmeYapiliyor = true;
                bulkSayfaAcildi = true;
                Lbl_Durum.Text = "Toplu SMS sayfasına gidiliyor...";
                webView21.CoreWebView2.Navigate("https://sms.myasist.ai/panel/sms/bulk");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SMS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void Btn_Gonder_Click(object sender, EventArgs e)
        {
            try
            {
                if (webView21?.CoreWebView2 == null)
                {
                    MessageBox.Show("Tarayıcı henüz hazır değil.");
                    return;
                }

                string url = webView21.Source?.ToString() ?? string.Empty;

                if (!url.Contains("/panel/sms/bulk"))
                {
                    MessageBox.Show("Henüz toplu SMS sayfası hazır değil.");
                    return;
                }

                string js = @"
(function() {
    var btn = document.querySelector('button[onclick*=showPreview]');
    if (btn) {
        btn.click();
        return true;
    }
    return false;
})();
";

                string sonuc1 = await webView21.CoreWebView2.ExecuteScriptAsync(js);

                if (!sonuc1.Contains("true"))
                {
                    MessageBox.Show("Önizleme butonu bulunamadı. Önce dosyayı seçip sütunları eşleştirin.");
                    return;
                }

                await Task.Delay(1500);

                string js2 = @"
(function() {
    var btn2 = document.querySelector('button[type=submit]');
    if (btn2) {
        btn2.click();
        return true;
    }
    return false;
})();
";

                string sonuc2 = await webView21.CoreWebView2.ExecuteScriptAsync(js2);

                if (sonuc2.Contains("true"))
                    Lbl_Durum.Text = "SMS gönderildi";
                else
                    MessageBox.Show("Gönder butonu bulunamadı.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
    }
}
