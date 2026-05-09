using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class WebCamResimForm : Form
    {
        private enum ResizeCorner
        {
            None,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        private readonly string _connectionString;
        private readonly int _kursiyerId;
        private readonly byte[] _initialImageBytes;

        private byte[] _selectedImageBytes;
        private Image _previewImage;
        private bool _isDraggingImage;
        private bool _isCornerResizing;
        private ResizeCorner _activeCorner = ResizeCorner.None;
        private Point _lastMousePoint;
        private float _zoom = 1f;
        private float _panX;
        private float _panY;
        private readonly Timer _previewTimer = new Timer();
        private bool _previewDirty;
        private bool _webcamReady;
        private readonly Timer _liveCameraPushTimer = new Timer();
        private bool _isLivePushTickRunning;

        public event Action<byte[]> LivePreviewUpdated;
        public event Action<byte[]> ImageCommitted;

        public WebCamResimForm() : this(string.Empty, 0)
        {
        }

        public WebCamResimForm(string connectionString, int kursiyerId, byte[] initialImageBytes = null)
        {
            _connectionString = connectionString ?? string.Empty;
            _kursiyerId = kursiyerId;
            _initialImageBytes = initialImageBytes;

            InitializeComponent();
            WireEvents();
            UpdateUiState();
        }

        public byte[] ProcessedImageBytes { get; private set; }

        public void SelectVirtualSourceDefault()
        {
            if (_cmbCameraSource == null || _cmbCameraSource.Items.Count == 0)
                return;

            if (_cmbCameraSource.SelectedIndex != 0)
                _cmbCameraSource.SelectedIndex = 0;
            else
            {
                StopWebcam();
                StopLiveCameraPush();
            }
        }

        private void WireEvents()
        {
            _btnSec.Click += BtnSec_Click;
            _btnTemizle.Click += BtnTemizle_Click;
            _btnKaydet.Click += BtnKaydet_Click;
            _btnSifirla.Click += BtnSifirla_Click;
            _chkMirror.CheckedChanged += (s, e) => OnPreviewTransformChanged();
            _cmbCameraSource.SelectedIndexChanged += CmbCameraSource_SelectedIndexChanged;
            _btnWebcamAl.Click += BtnWebcamAl_Click;
            _dropPanel.DragEnter += DropPanel_DragEnter;
            _dropPanel.DragDrop += DropPanel_DragDrop;
            _picture.DragEnter += DropPanel_DragEnter;
            _picture.DragDrop += DropPanel_DragDrop;
            _picture.Paint += Picture_Paint;
            _picture.MouseDown += Picture_MouseDown;
            _picture.MouseMove += Picture_MouseMove;
            _picture.MouseUp += Picture_MouseUp;
            _picture.MouseWheel += Picture_MouseWheel;
            _previewTimer.Interval = 120;
            _previewTimer.Tick += PreviewTimer_Tick;
            _liveCameraPushTimer.Interval = 350;
            _liveCameraPushTimer.Tick += LiveCameraPushTimer_Tick;
        }

        private void DropPanel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void DropPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0)
                return;

            LoadImageFromPath(files[0]);
        }

        private void BtnSec_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Resim Dosyalari|*.jpg;*.jpeg;*.png;*.bmp;*.webp";
                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                LoadImageFromPath(ofd.FileName);
            }
        }

        private void BtnTemizle_Click(object sender, EventArgs e)
        {
            _selectedImageBytes = null;
            ProcessedImageBytes = null;
            DisposePreview();
            ResetTransform();
            UpdateUiState();
            PublishLivePreview(null);
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            if (_selectedImageBytes == null || _selectedImageBytes.Length == 0)
            {
                MessageBox.Show(this, "Once bir resim secmelisiniz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                ProcessedImageBytes = CreateWebcamStyleImage(
                    _selectedImageBytes,
                    _chkMirror.Checked,
                    _zoom,
                    _panX,
                    _panY);

                if (_kursiyerId > 0 && !string.IsNullOrWhiteSpace(_connectionString))
                {
                    SaveToDatabase(_kursiyerId, ProcessedImageBytes);
                    MessageBox.Show(this, "Webcam resmi kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, "Resim hazirlandi. Form disinda kullanabilirsiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ImageCommitted?.Invoke(ProcessedImageBytes);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Kaydetme hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSifirla_Click(object sender, EventArgs e)
        {
            ResetTransform();
            OnPreviewTransformChanged();
        }

        private async void BtnWebcamAl_Click(object sender, EventArgs e)
        {
            if (!_webcamReady || _webCamView?.CoreWebView2 == null)
                return;

            try
            {
                var raw = await _webCamView.CoreWebView2.ExecuteScriptAsync(@"
                    (function(){
                      var v = document.getElementById('cam');
                      if(!v || !v.videoWidth || !v.videoHeight) return '';
                      var c = document.createElement('canvas');
                      c.width = v.videoWidth;
                      c.height = v.videoHeight;
                      var ctx = c.getContext('2d');
                      ctx.drawImage(v,0,0,c.width,c.height);
                      return c.toDataURL('image/jpeg',0.92);
                    })();");

                var dataUrl = UnwrapJsString(raw);
                if (string.IsNullOrWhiteSpace(dataUrl) || dataUrl.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) < 0)
                    return;

                var base64 = dataUrl.Substring(dataUrl.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7);
                _selectedImageBytes = Convert.FromBase64String(base64);
                DisposePreview();
                using (var ms = new MemoryStream(_selectedImageBytes))
                using (var src = Image.FromStream(ms))
                    _previewImage = new Bitmap(src);

                ResetTransform();
                UpdateUiState();
                OnPreviewTransformChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "WebCam goruntusu alinamadi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveToDatabase(int kursiyerId, byte[] imageData)
        {
            const string sql = @"
UPDATE KURSIYER
SET RESIM_WEBCAM = @RESIM_WEBCAM
WHERE ID = @ID;";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@ID", kursiyerId);
                var p = cmd.Parameters.Add("@RESIM_WEBCAM", System.Data.SqlDbType.VarBinary, -1);
                p.Value = (object)imageData ?? DBNull.Value;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadImageFromPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            try
            {
                _selectedImageBytes = File.ReadAllBytes(path);
                DisposePreview();
                using (var ms = new MemoryStream(_selectedImageBytes))
                using (var src = Image.FromStream(ms))
                {
                    _previewImage = new Bitmap(src);
                }

                ResetTransform();
                UpdateUiState();
                _picture.Focus();
                OnPreviewTransformChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Resim okunamadi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static byte[] CreateWebcamStyleImage(byte[] input, bool mirror, float zoom, float panX, float panY)
        {
            // Webcam gorunumunde kullanmak icin sabit bir canvas boyutu.
            const int targetWidth = 640;
            const int targetHeight = 480;

            using (var ms = new MemoryStream(input))
            using (var src = Image.FromStream(ms))
            using (var canvas = new Bitmap(targetWidth, targetHeight, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(canvas))
            {
                DrawWebcamFrame(g, src, targetWidth, targetHeight, mirror, zoom, panX, panY);

                using (var outMs = new MemoryStream())
                {
                    canvas.Save(outMs, ImageFormat.Jpeg);
                    return outMs.ToArray();
                }
            }
        }

        private void Picture_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.Black);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;

            if (_previewImage == null)
                return;

            DrawWebcamFrame(g, _previewImage, _picture.Width, _picture.Height, _chkMirror.Checked, _zoom, _panX, _panY);
            DrawResizeHandles(g);
        }

        private void Picture_MouseDown(object sender, MouseEventArgs e)
        {
            if (_previewImage == null || e.Button != MouseButtons.Left)
                return;

            var imageRect = GetCurrentImageRect(_picture.Width, _picture.Height);
            _activeCorner = HitTestCorner(e.Location);
            _isCornerResizing = _activeCorner != ResizeCorner.None;
            _isDraggingImage = !_isCornerResizing && imageRect.Contains(e.Location);
            _lastMousePoint = e.Location;
            _picture.Focus();
        }

        private void Picture_MouseMove(object sender, MouseEventArgs e)
        {
            if (_previewImage == null)
            {
                _picture.Cursor = Cursors.Default;
                return;
            }

            if (!_isDraggingImage && !_isCornerResizing)
                SetHoverCursor(e.Location, GetCurrentImageRect(_picture.Width, _picture.Height));

            if (_isDraggingImage)
            {
                int dx = e.X - _lastMousePoint.X;
                int dy = e.Y - _lastMousePoint.Y;
                _panX += dx;
                _panY += dy;
                _lastMousePoint = e.Location;
                OnPreviewTransformChanged();
                return;
            }

            if (_isCornerResizing)
            {
                float delta = GetCornerResizeDelta(_activeCorner, e.Location, _lastMousePoint, _picture.Size);
                _zoom = Clamp(_zoom + delta, 0.2f, 6f);
                _lastMousePoint = e.Location;
                OnPreviewTransformChanged();
            }
        }

        private void Picture_MouseUp(object sender, MouseEventArgs e)
        {
            _isDraggingImage = false;
            _isCornerResizing = false;
            _activeCorner = ResizeCorner.None;
            SetHoverCursor(e.Location, GetCurrentImageRect(_picture.Width, _picture.Height));
        }

        private void Picture_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_previewImage == null)
                return;

            float step = e.Delta > 0 ? 0.12f : -0.12f;
            _zoom = Clamp(_zoom + step, 0.2f, 6f);
            OnPreviewTransformChanged();
        }

        private void DrawResizeHandles(Graphics g)
        {
            if (_previewImage == null)
                return;

            var r = GetCurrentImageRect(_picture.Width, _picture.Height);
            using (var pen = new Pen(Color.FromArgb(0, 120, 215), 2f))
                g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);

            const float handleR = 6f;
            var points = new[]
            {
                new PointF(r.Left, r.Top),
                new PointF(r.Right, r.Top),
                new PointF(r.Left, r.Bottom),
                new PointF(r.Right, r.Bottom)
            };

            using (var fill = new SolidBrush(Color.White))
            using (var border = new Pen(Color.FromArgb(0, 120, 215), 2f))
                foreach (var p in points)
                {
                    g.FillEllipse(fill, p.X - handleR, p.Y - handleR, handleR * 2f, handleR * 2f);
                    g.DrawEllipse(border, p.X - handleR, p.Y - handleR, handleR * 2f, handleR * 2f);
                }
        }

        private void SetHoverCursor(Point p, RectangleF imageRect)
        {
            switch (HitTestCorner(p))
            {
                case ResizeCorner.TopLeft:
                case ResizeCorner.BottomRight:
                    _picture.Cursor = Cursors.SizeNWSE;
                    break;
                case ResizeCorner.TopRight:
                case ResizeCorner.BottomLeft:
                    _picture.Cursor = Cursors.SizeNESW;
                    break;
                default:
                    _picture.Cursor = imageRect.Contains(p) ? Cursors.Hand : Cursors.Default;
                    break;
            }
        }

        private ResizeCorner HitTestCorner(Point p)
        {
            if (_previewImage == null)
                return ResizeCorner.None;

            var r = GetCurrentImageRect(_picture.Width, _picture.Height);
            const float hit = 14f;
            if (Distance(p, new PointF(r.Left, r.Top)) <= hit) return ResizeCorner.TopLeft;
            if (Distance(p, new PointF(r.Right, r.Top)) <= hit) return ResizeCorner.TopRight;
            if (Distance(p, new PointF(r.Left, r.Bottom)) <= hit) return ResizeCorner.BottomLeft;
            if (Distance(p, new PointF(r.Right, r.Bottom)) <= hit) return ResizeCorner.BottomRight;
            return ResizeCorner.None;
        }

        private static float GetCornerResizeDelta(ResizeCorner corner, Point current, Point previous, Size bounds)
        {
            var center = new PointF(bounds.Width / 2f, bounds.Height / 2f);
            float prevDist = Distance(center, previous);
            float currDist = Distance(center, current);
            float dir = (corner == ResizeCorner.TopLeft || corner == ResizeCorner.BottomLeft) ? -1f : 1f;
            float delta = ((currDist - prevDist) / Math.Max(100f, Math.Min(bounds.Width, bounds.Height))) * dir;
            return delta;
        }

        private static float Distance(PointF center, Point point)
        {
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;
            return (float)Math.Sqrt((dx * dx) + (dy * dy));
        }

        private static float Distance(Point point, PointF target)
        {
            float dx = point.X - target.X;
            float dy = point.Y - target.Y;
            return (float)Math.Sqrt((dx * dx) + (dy * dy));
        }

        private RectangleF GetCurrentImageRect(int targetWidth, int targetHeight)
        {
            if (_previewImage == null || targetWidth <= 0 || targetHeight <= 0)
                return RectangleF.Empty;

            float baseScale = Math.Min((float)targetWidth / _previewImage.Width, (float)targetHeight / _previewImage.Height);
            float finalScale = baseScale * Clamp(_zoom, 0.2f, 6f);
            float drawW = _previewImage.Width * finalScale;
            float drawH = _previewImage.Height * finalScale;
            float x = (targetWidth - drawW) / 2f + _panX;
            float y = (targetHeight - drawH) / 2f + _panY;
            return new RectangleF(x, y, drawW, drawH);
        }

        private static void DrawWebcamFrame(Graphics g, Image src, int targetWidth, int targetHeight, bool mirror, float zoom, float panX, float panY)
        {
            g.Clear(Color.Black);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // "contain" davranisi: resmin kendisiyle rahat calismak icin tam gorunur baslat.
            float baseScale = Math.Min((float)targetWidth / src.Width, (float)targetHeight / src.Height);
            float finalScale = baseScale * Clamp(zoom, 0.2f, 6f);
            float drawW = src.Width * finalScale;
            float drawH = src.Height * finalScale;
            float x = (targetWidth - drawW) / 2f + panX;
            float y = (targetHeight - drawH) / 2f + panY;

            if (mirror)
            {
                g.TranslateTransform(targetWidth, 0);
                g.ScaleTransform(-1, 1);
                g.DrawImage(src, targetWidth - x - drawW, y, drawW, drawH);
                g.ResetTransform();
            }
            else
            {
                g.DrawImage(src, x, y, drawW, drawH);
            }

        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private void ResetTransform()
        {
            _zoom = 1f;
            _panX = 0f;
            _panY = 0f;
        }

        private void OnPreviewTransformChanged()
        {
            _picture.Invalidate();
            QueueLivePreviewUpdate();
        }

        private void QueueLivePreviewUpdate()
        {
            _previewDirty = true;
            if (!_previewTimer.Enabled)
                _previewTimer.Start();
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            if (!_previewDirty)
            {
                _previewTimer.Stop();
                return;
            }

            _previewDirty = false;
            PublishLivePreview(BuildCurrentFrameBytes());
        }

        private byte[] BuildCurrentFrameBytes()
        {
            if (_previewImage == null)
                return null;

            const int targetWidth = 640;
            const int targetHeight = 480;
            using (var canvas = new Bitmap(targetWidth, targetHeight, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(canvas))
            using (var outMs = new MemoryStream())
            {
                DrawWebcamFrame(g, _previewImage, targetWidth, targetHeight, _chkMirror.Checked, _zoom, _panX, _panY);
                canvas.Save(outMs, ImageFormat.Jpeg);
                return outMs.ToArray();
            }
        }

        private void PublishLivePreview(byte[] frameBytes)
        {
            LivePreviewUpdated?.Invoke(frameBytes);
        }

        private void DisposePreview()
        {
            if (_previewImage != null)
            {
                _previewImage.Dispose();
                _previewImage = null;
            }
        }

        private void UpdateUiState()
        {
            bool hasImage = _selectedImageBytes != null && _selectedImageBytes.Length > 0;
            _btnTemizle.Enabled = hasImage;
            _btnKaydet.Enabled = hasImage;
            _btnSifirla.Enabled = hasImage;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _previewTimer.Stop();
            _previewTimer.Tick -= PreviewTimer_Tick;
            StopLiveCameraPush();
            _liveCameraPushTimer.Tick -= LiveCameraPushTimer_Tick;
            StopWebcam();
            DisposePreview();
            base.OnFormClosed(e);
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            LoadExistingWebcamImageFromDatabase();
            await InitializeWebcamAsync();
        }

        private void LoadExistingWebcamImageFromDatabase()
        {
            if (_kursiyerId <= 0 || string.IsNullOrWhiteSpace(_connectionString))
            {
                if (_initialImageBytes != null && _initialImageBytes.Length > 0)
                    LoadPreviewFromBytes(_initialImageBytes);
                return;
            }

            const string sql = @"
SELECT TOP 1 RESIM_WEBCAM
FROM KURSIYER
WHERE ID = @ID;";

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", _kursiyerId);
                    conn.Open();
                    var obj = cmd.ExecuteScalar();
                    var data = obj as byte[];
                    if (data != null && data.Length > 0)
                    {
                        LoadPreviewFromBytes(data);
                        return;
                    }
                }
            }
            catch
            {
                // DB okunamazsa form normal akisla devam etsin.
            }

            if (_initialImageBytes != null && _initialImageBytes.Length > 0)
                LoadPreviewFromBytes(_initialImageBytes);
        }

        private void LoadPreviewFromBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                return;

            _selectedImageBytes = data;
            DisposePreview();
            using (var ms = new MemoryStream(data))
            using (var src = Image.FromStream(ms))
                _previewImage = new Bitmap(src);

            ResetTransform();
            UpdateUiState();
            OnPreviewTransformChanged();
        }

        private async System.Threading.Tasks.Task InitializeWebcamAsync()
        {
            if (_webCamView == null || DesignMode)
                return;

            try
            {
                await _webCamView.EnsureCoreWebView2Async();
                _webCamView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _webCamView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _webCamView.CoreWebView2.PermissionRequested += CoreWebView2_PermissionRequested;
                _webCamView.NavigateToString(@"<!doctype html><html><body style='margin:0;background:#000;overflow:hidden;'>
<video id='cam' autoplay playsinline muted style='width:100%;height:100%;object-fit:cover;background:#000'></video>
</body></html>");
                await LoadCameraListAsync();
                _webcamReady = true;
            }
            catch
            {
                _webcamReady = false;
            }
        }

        private void CoreWebView2_PermissionRequested(object sender, CoreWebView2PermissionRequestedEventArgs e)
        {
            if (e.PermissionKind == CoreWebView2PermissionKind.Camera || e.PermissionKind == CoreWebView2PermissionKind.Microphone)
                e.State = CoreWebView2PermissionState.Allow;
        }

        private async System.Threading.Tasks.Task LoadCameraListAsync()
        {
            if (_webCamView?.CoreWebView2 == null)
                return;

            _cmbCameraSource.Items.Clear();
            _cmbCameraSource.Items.Add(new CameraItem { DeviceId = string.Empty, Label = "Sanal/WebCamResim (Dosya)" });

            var raw = await _webCamView.CoreWebView2.ExecuteScriptAsync(@"
                (async function(){
                    try{
                        await navigator.mediaDevices.getUserMedia({video:true,audio:false});
                        var list = await navigator.mediaDevices.enumerateDevices();
                        return JSON.stringify(list.filter(function(x){ return x.kind==='videoinput'; }).map(function(x){
                            return { deviceId:x.deviceId || '', label:x.label || 'Kamera' };
                        }));
                    }catch(e){ return '[]'; }
                })();");

            string json = UnwrapJsString(raw);
            if (string.IsNullOrWhiteSpace(json))
                json = "[]";

            try
            {
                var cams = JsonSerializer.Deserialize<CameraItem[]>(json);
                if (cams != null)
                {
                    foreach (var cam in cams)
                        _cmbCameraSource.Items.Add(cam);
                }
            }
            catch { }

            if (_cmbCameraSource.Items.Count > 0)
                _cmbCameraSource.SelectedIndex = 0;
        }

        private async void CmbCameraSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = _cmbCameraSource.SelectedItem as CameraItem;
            if (selected == null || _webCamView?.CoreWebView2 == null)
                return;

            if (string.IsNullOrWhiteSpace(selected.DeviceId))
            {
                StopWebcam();
                StopLiveCameraPush();
                return;
            }

            await StartWebcamAsync(selected.DeviceId);
            StartLiveCameraPush();
        }

        private async System.Threading.Tasks.Task StartWebcamAsync(string deviceId)
        {
            var escaped = EscapeJs(deviceId);
            await _webCamView.CoreWebView2.ExecuteScriptAsync(@"
                (async function(){
                  try{
                    if(window.__koleraStream){
                      window.__koleraStream.getTracks().forEach(function(t){ t.stop(); });
                    }
                    var stream = await navigator.mediaDevices.getUserMedia({ video: { deviceId: { exact: '" + escaped + @"' } }, audio:false });
                    window.__koleraStream = stream;
                    var v = document.getElementById('cam');
                    if(v){ v.srcObject = stream; await v.play().catch(function(){}); }
                  }catch(e){}
                })();");
        }

        private void StopWebcam()
        {
            try
            {
                _webCamView?.CoreWebView2?.ExecuteScriptAsync(@"
                    (function(){
                       if(window.__koleraStream){
                         window.__koleraStream.getTracks().forEach(function(t){ t.stop(); });
                         window.__koleraStream = null;
                       }
                       var v = document.getElementById('cam');
                       if(v) v.srcObject = null;
                    })();");
            }
            catch { }
        }

        private void StartLiveCameraPush()
        {
            if (!_liveCameraPushTimer.Enabled)
                _liveCameraPushTimer.Start();
        }

        private void StopLiveCameraPush()
        {
            if (_liveCameraPushTimer.Enabled)
                _liveCameraPushTimer.Stop();
        }

        private async void LiveCameraPushTimer_Tick(object sender, EventArgs e)
        {
            if (_isLivePushTickRunning || _webCamView?.CoreWebView2 == null)
                return;

            _isLivePushTickRunning = true;
            try
            {
                var raw = await _webCamView.CoreWebView2.ExecuteScriptAsync(@"
                    (function(){
                      try{
                        var v = document.getElementById('cam');
                        if(!v || !v.srcObject || !v.videoWidth || !v.videoHeight) return '';
                        var c = document.createElement('canvas');
                        c.width = 640;
                        c.height = 480;
                        var ctx = c.getContext('2d');
                        ctx.drawImage(v,0,0,c.width,c.height);
                        return c.toDataURL('image/jpeg',0.78);
                      }catch(e){ return ''; }
                    })();");

                var dataUrl = UnwrapJsString(raw);
                if (string.IsNullOrWhiteSpace(dataUrl) || dataUrl.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) < 0)
                    return;

                var base64 = dataUrl.Substring(dataUrl.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7);
                var frame = Convert.FromBase64String(base64);
                PublishLivePreview(frame);
            }
            catch
            {
                // ignore transient webcam frame errors
            }
            finally
            {
                _isLivePushTickRunning = false;
            }
        }

        private static string UnwrapJsString(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return string.Empty;
            try
            {
                return JsonSerializer.Deserialize<string>(raw) ?? string.Empty;
            }
            catch
            {
                return raw.Trim('"');
            }
        }

        private static string EscapeJs(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("'", "\\'");
        }

        private sealed class CameraItem
        {
            public string DeviceId { get; set; }
            public string Label { get; set; }
            public override string ToString() { return Label; }
        }
    }
}
