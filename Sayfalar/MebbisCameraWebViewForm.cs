using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Kolera_Mtsk.Sayfalar
{
    public sealed class MebbisCameraWebViewForm : Form
    {
        private readonly string _kullaniciAdi;
        private readonly string _sifre;
        private readonly string _targetUrl;
        private readonly byte[] _sourceImageBytes;

        private WebView2 _webView;
        private ComboBox _cmbCameras;
        private Button _btnKameraListele;
        private Button _btnKameraUygula;
        private Label _lblCamStatus;
        private bool _targetNavigated;

        public MebbisCameraWebViewForm(string kullaniciAdi, string sifre, string targetUrl, byte[] sourceImageBytes = null)
        {
            _kullaniciAdi = kullaniciAdi ?? string.Empty;
            _sifre = sifre ?? string.Empty;
            _targetUrl = string.IsNullOrWhiteSpace(targetUrl) ? "https://mebbis.meb.gov.tr/SKT/skt02002.aspx" : targetUrl;
            _sourceImageBytes = sourceImageBytes;

            InitializeUi();
            Load += async (s, e) => await InitializeBrowserAsync();
        }

        private void InitializeUi()
        {
            Text = "MEBBIS Kamera (WebView2)";
            StartPosition = FormStartPosition.CenterParent;
            Width = 1200;
            Height = 800;

            var info = new Label
            {
                Dock = DockStyle.Top,
                Height = 38,
                Padding = new Padding(10, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Bu ekran WebView2 ile kamera iznini otomatik verir. Video alani burada gorunmelidir."
            };

            var topBar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 42,
                Padding = new Padding(10, 6, 10, 6),
                FlowDirection = FlowDirection.LeftToRight
            };

            _cmbCameras = new ComboBox
            {
                Width = 520,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _btnKameraListele = new Button
            {
                Width = 120,
                Height = 28,
                Text = "Kameralari Yenile"
            };
            _btnKameraUygula = new Button
            {
                Width = 140,
                Height = 28,
                Text = "Secili Kamerayi Ac"
            };
            _lblCamStatus = new Label
            {
                Width = 320,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Text = "Kamera listesi hazir degil."
            };
            _btnKameraListele.Click += async (s, e) => await RefreshCameraListAsync();
            _btnKameraUygula.Click += async (s, e) => await ApplySelectedCameraAsync();

            topBar.Controls.Add(_cmbCameras);
            topBar.Controls.Add(_btnKameraListele);
            topBar.Controls.Add(_btnKameraUygula);
            topBar.Controls.Add(_lblCamStatus);

            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            Controls.Add(_webView);
            Controls.Add(topBar);
            Controls.Add(info);
        }

        private async Task InitializeBrowserAsync()
        {
            await _webView.EnsureCoreWebView2Async();

            _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
            _webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
            _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            _webView.CoreWebView2.Settings.IsScriptEnabled = true;

            string imageDataUrl = BuildImageDataUrl(_sourceImageBytes);
            await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
                "window.__koleraUserImageDataUrl = '" + EscapeJs(imageDataUrl) + "';");

            await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
(() => {
  const g = window;
  if (g.__koleraVirtualCamInstalled) return;
  g.__koleraVirtualCamInstalled = true;

  g.__koleraVirtualCam = {
    stream: null,
    canvas: null,
    timer: null,
    enabled: true,
    image: null,
    imageLoaded: false
  };

  function createVirtualStream() {
    if (g.__koleraVirtualCam.stream) return g.__koleraVirtualCam.stream;

    const canvas = document.createElement('canvas');
    canvas.width = 640;
    canvas.height = 480;
    const ctx = canvas.getContext('2d');
    const state = g.__koleraVirtualCam;
    state.canvas = canvas;

    if (!state.image && typeof g.__koleraUserImageDataUrl === 'string' && g.__koleraUserImageDataUrl.indexOf('data:image') === 0) {
      const img = new Image();
      img.onload = () => { state.imageLoaded = true; };
      img.onerror = () => { state.imageLoaded = false; };
      img.src = g.__koleraUserImageDataUrl;
      state.image = img;
    }

    function drawFrame() {
      const w = canvas.width, h = canvas.height;
      const t = new Date();
      const ms = t.getMilliseconds();

      if (state.image && state.imageLoaded) {
        // Kullanicidan gelen resmi webcam akisi olarak arkaplanda goster.
        const iw = state.image.width || 1;
        const ih = state.image.height || 1;
        const scale = Math.max(w / iw, h / ih);
        const dw = iw * scale;
        const dh = ih * scale;
        const dx = (w - dw) / 2;
        const dy = (h - dh) / 2;
        ctx.drawImage(state.image, dx, dy, dw, dh);
      } else {
        const grad = ctx.createLinearGradient(0, 0, w, h);
        grad.addColorStop(0, '#0b5cff');
        grad.addColorStop(1, '#12b886');
        ctx.fillStyle = grad;
        ctx.fillRect(0, 0, w, h);
      }

      // Hareketli bir ""canli yayin"" hissi vermesi icin dalga/cizgi
      ctx.strokeStyle = 'rgba(255,255,255,0.55)';
      ctx.lineWidth = 3;
      ctx.beginPath();
      for (let x = 0; x <= w; x += 8) {
        const y = h * 0.5 + Math.sin((x + ms) * 0.02) * 22;
        if (x === 0) ctx.moveTo(x, y);
        else ctx.lineTo(x, y);
      }
      ctx.stroke();

      // Hareketli beyaz nokta
      const dotX = (ms / 1000) * w;
      const dotY = h * 0.5 + Math.sin((dotX + ms) * 0.02) * 22;
      ctx.beginPath();
      ctx.fillStyle = '#ffffff';
      ctx.arc(dotX, dotY, 8, 0, Math.PI * 2);
      ctx.fill();

      // Alt bilgi bandi
      ctx.fillStyle = 'rgba(0,0,0,0.35)';
      ctx.fillRect(0, h - 96, w, 96);

      ctx.fillStyle = '#ffffff';
      ctx.font = 'bold 34px Segoe UI, Arial';
      ctx.fillText('KOLERA SANAL KAMERA', 20, 54);

      ctx.font = '20px Segoe UI, Arial';
      ctx.fillText('MEBBIS video alani test akis', 20, 84);

      ctx.font = 'bold 24px Consolas, monospace';
      const stamp = t.toLocaleDateString('tr-TR') + ' ' + t.toLocaleTimeString('tr-TR');
      ctx.fillText(stamp, 20, h - 34);
    }

    drawFrame();
    state.timer = setInterval(drawFrame, 500);
    state.stream = canvas.captureStream(25);
    return state.stream;
  }

  function stopVirtual() {
    const state = g.__koleraVirtualCam;
    if (state.timer) {
      clearInterval(state.timer);
      state.timer = null;
    }
    if (state.stream) {
      state.stream.getTracks().forEach(t => t.stop());
      state.stream = null;
    }
  }

  g.__koleraVirtualCamStart = () => createVirtualStream();
  g.__koleraVirtualCamStop = () => stopVirtual();

  if (!navigator.mediaDevices) navigator.mediaDevices = {};
  const originalEnumerateDevices = navigator.mediaDevices.enumerateDevices
    ? navigator.mediaDevices.enumerateDevices.bind(navigator.mediaDevices)
    : null;

  // Kural: video talebinde daima Kolera sanal kamerayi dondur.
  navigator.mediaDevices.getUserMedia = async function(constraints) {
    const videoRequested = !!(constraints && (constraints.video === true || typeof constraints.video === 'object'));
    if (videoRequested) return createVirtualStream();
    throw new Error('Sadece video akis desteklenir.');
  };

  navigator.mediaDevices.enumerateDevices = async function(){
    let base = [];
    try{
      if (originalEnumerateDevices) base = await originalEnumerateDevices();
    }catch(_){}
    const virtual = {
      deviceId: 'kolera-virtual-camera',
      kind: 'videoinput',
      label: 'Kolera Sanal Kamera',
      groupId: 'kolera'
    };
    const rest = (base || []).filter(d => !(d && d.kind === 'videoinput' && d.deviceId === virtual.deviceId));
    return [virtual].concat(rest);
  };

  // Legacy API uyumlulugu (eski web kodlari icin).
  const legacy = function(constraints, success, fail){
    navigator.mediaDevices.getUserMedia(constraints)
      .then(stream => { if (typeof success === 'function') success(stream); })
      .catch(err => { if (typeof fail === 'function') fail(err); });
  };
  navigator.getUserMedia = legacy;
  navigator.webkitGetUserMedia = legacy;
  navigator.mozGetUserMedia = legacy;
})();");

            _webView.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
            _webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            _webView.CoreWebView2.Navigate("https://mebbis.meb.gov.tr/default.aspx?NoSession");
        }

        private void CoreWebView2_PermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs e)
        {
            if (e.PermissionKind == CoreWebView2PermissionKind.Camera ||
                e.PermissionKind == CoreWebView2PermissionKind.Microphone)
            {
                e.State = CoreWebView2PermissionState.Allow;
                return;
            }

            e.State = CoreWebView2PermissionState.Default;
        }

        private async void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess || _webView.CoreWebView2 == null)
                return;

            string url = (_webView.Source?.ToString() ?? string.Empty).ToLowerInvariant();

            if (url.Contains("default.aspx"))
            {
                await TryAutoLoginAsync();
                return;
            }

            if (!_targetNavigated && url.Contains("mebbis.meb.gov.tr"))
            {
                _targetNavigated = true;
                _webView.CoreWebView2.Navigate(_targetUrl);
                return;
            }

            if (url.Contains("/skt/skt02002.aspx"))
            {
                await RefreshCameraListAsync();
                await EnsureVirtualCameraBoundToVideoAsync();
            }
        }

        private async Task EnsureVirtualCameraBoundToVideoAsync()
        {
            if (_webView.CoreWebView2 == null)
                return;

            const string js = @"
(async function(){
  try{
    const video = document.getElementById('video');
    if(!video) return JSON.stringify({ok:false, reason:'video#video yok'});

    if(video.srcObject && video.srcObject.getTracks){
      video.srcObject.getTracks().forEach(t => t.stop());
    }

    const stream = await window.__koleraVirtualCamStart();
    if(!stream) return JSON.stringify({ok:false, reason:'sanal stream olusmadi'});

    video.srcObject = stream;
    await video.play();
    return JSON.stringify({ok:true, reason:'sanal kamera baglandi'});
  }catch(e){
    return JSON.stringify({ok:false, reason:(e && e.message ? e.message : String(e))});
  }
})();";

            try
            {
                string raw = await _webView.CoreWebView2.ExecuteScriptAsync(js);
                string text = DecodeWebViewJsonString(raw);
                var payload = ParseBindPayload(text);
                _lblCamStatus.Text = payload.ok
                    ? "Sanal kamera aktif."
                    : "Sanal kamera baglanamadi: " + payload.reason;

                if (payload.ok)
                {
                    await _webView.CoreWebView2.ExecuteScriptAsync("setTimeout(async()=>{try{const v=document.getElementById('video');if(v){v.srcObject=await window.__koleraVirtualCamStart();await v.play();}}catch(_){ }}, 900);");
                    await _webView.CoreWebView2.ExecuteScriptAsync("setTimeout(async()=>{try{const v=document.getElementById('video');if(v){v.srcObject=await window.__koleraVirtualCamStart();await v.play();}}catch(_){ }}, 1900);");
                }
            }
            catch (Exception ex)
            {
                _lblCamStatus.Text = "Sanal kamera hatasi: " + ex.Message;
            }
        }

        private async Task RefreshCameraListAsync()
        {
            if (_webView.CoreWebView2 == null)
                return;

            const string js = @"
(async function() {
  try{
    if(!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices){
      return JSON.stringify({ ok:false, error:'mediaDevices API yok', cameras:[] });
    }

    // Etiketlerin dolu gelmesi icin once izin iste.
    let tempStream = null;
    try{
      tempStream = await navigator.mediaDevices.getUserMedia({ video:true, audio:false });
    }catch(err){
      // Izin verilmezse yine enumerateDevices deneyelim.
    }

    const devices = await navigator.mediaDevices.enumerateDevices();
    const cams = devices.filter(d => d.kind === 'videoinput')
      .map(d => ({ id: d.deviceId || '', label: d.label || 'Kamera' }));

    if(tempStream){
      tempStream.getTracks().forEach(t => t.stop());
    }

    return JSON.stringify({ ok:true, cameras:cams });
  }catch(e){
    return JSON.stringify({ ok:false, error:(e && e.message ? e.message : String(e)), cameras:[] });
  }
})();";

            try
            {
                string raw = await _webView.CoreWebView2.ExecuteScriptAsync(js);
                string jsonText = DecodeWebViewJsonString(raw);
                var payload = ParseCameraPayload(jsonText);
                var cams = payload.cameras ?? new List<CameraDevice>();

                _cmbCameras.BeginUpdate();
                _cmbCameras.Items.Clear();
                foreach (var cam in cams)
                    _cmbCameras.Items.Add(cam);
                _cmbCameras.EndUpdate();

                if (_cmbCameras.Items.Count > 0)
                {
                    _cmbCameras.SelectedIndex = 0;
                    _lblCamStatus.Text = "Kamera bulundu: " + _cmbCameras.Items.Count;
                }
                else
                {
                    _lblCamStatus.Text = "Kamera bulunamadi. Izin veya surucu kontrol edin.";
                    if (!string.IsNullOrWhiteSpace(payload.error))
                        _lblCamStatus.Text = "Kamera listesi bos: " + payload.error;
                }
            }
            catch (Exception ex)
            {
                // Kamera listesi okunamazsa form akisi bozulmasin.
                _lblCamStatus.Text = "Kamera listesi alinirken hata: " + ex.Message;
            }
        }

        private async Task ApplySelectedCameraAsync()
        {
            if (_webView.CoreWebView2 == null)
                return;

            var selected = _cmbCameras.SelectedItem as CameraDevice;
            if (selected == null || string.IsNullOrWhiteSpace(selected.Id))
            {
                MessageBox.Show(this, "Lutfen bir kamera secin.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string deviceId = EscapeJs(selected.Id);
            string script = @"
(async function() {
  const video = document.getElementById('video');
  if(!video){
    alert('Video alani bulunamadi (#video).');
    return;
  }
  try{
    if(video.srcObject && video.srcObject.getTracks){
      video.srcObject.getTracks().forEach(t => t.stop());
    }
    let stream;
    if('" + deviceId + @"' === 'kolera-virtual-camera'){
      stream = await window.__koleraVirtualCamStart();
    }else{
      stream = await navigator.mediaDevices.getUserMedia({
        video: { deviceId: { exact: '" + deviceId + @"' } },
        audio: false
      });
    }
    video.srcObject = stream;
    await video.play();
  }catch(e){
    alert('Secilen kamera acilamadi: ' + (e && e.message ? e.message : e));
  }
})();";

            try
            {
                await _webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Kamera uygulanamadi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task TryAutoLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(_kullaniciAdi) || string.IsNullOrWhiteSpace(_sifre))
                return;

            string script = @"
(function(){
  const inputs = Array.from(document.querySelectorAll('input'));
  const user = inputs.find(i => (i.type || '').toLowerCase() === 'text');
  const pass = inputs.find(i => (i.type || '').toLowerCase() === 'password');
  if(user) user.value = '" + EscapeJs(_kullaniciAdi) + @"';
  if(pass) pass.value = '" + EscapeJs(_sifre) + @"';
  const btn = inputs.find(i => {
    const t = (i.type || '').toLowerCase();
    return t === 'submit' || t === 'button';
  });
  if(btn) btn.click();
})();";

            try
            {
                await _webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch
            {
                // Login script calismazsa kullanici manuel devam edebilir.
            }
        }

        private static string EscapeJs(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("'", "\\'");
        }

        private static string BuildImageDataUrl(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;
            return "data:image/jpeg;base64," + Convert.ToBase64String(data);
        }

        private static string DecodeWebViewJsonString(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "[]";

            try
            {
                return JsonSerializer.Deserialize<string>(raw) ?? "[]";
            }
            catch
            {
                return "[]";
            }
        }

        private sealed class CameraDevice
        {
            public string id { get; set; }
            public string label { get; set; }
            public string Id => id ?? string.Empty;
            public string Label => string.IsNullOrWhiteSpace(label) ? "Kamera" : label;
            public override string ToString() => Label;
        }

        private sealed class CameraListPayload
        {
            public bool ok { get; set; }
            public string error { get; set; }
            public List<CameraDevice> cameras { get; set; }
        }

        private sealed class BindPayload
        {
            public bool ok { get; set; }
            public string reason { get; set; }
        }

        private static CameraListPayload ParseCameraPayload(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                return new CameraListPayload { ok = false, error = "Bos yanit", cameras = new List<CameraDevice>() };

            string t = jsonText.Trim();

            // Eski/alternatif scriptler direkt dizi donebilir.
            if (t.StartsWith("["))
            {
                var list = JsonSerializer.Deserialize<List<CameraDevice>>(t) ?? new List<CameraDevice>();
                return new CameraListPayload { ok = true, cameras = list };
            }

            if (t.StartsWith("{"))
            {
                return JsonSerializer.Deserialize<CameraListPayload>(t)
                       ?? new CameraListPayload { ok = false, error = "Payload parse edilemedi", cameras = new List<CameraDevice>() };
            }

            return new CameraListPayload { ok = false, error = "Beklenmeyen yanit formati", cameras = new List<CameraDevice>() };
        }

        private static BindPayload ParseBindPayload(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                return new BindPayload { ok = false, reason = "Bos yanit" };
            try
            {
                return JsonSerializer.Deserialize<BindPayload>(jsonText) ?? new BindPayload { ok = false, reason = "Parse hatasi" };
            }
            catch
            {
                return new BindPayload { ok = false, reason = "Beklenmeyen yanit" };
            }
        }
    }
}
