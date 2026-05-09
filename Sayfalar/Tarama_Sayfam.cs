using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using WIA;
using Kolera_Mtsk.Services;
using System.Runtime.InteropServices;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Tarama_Sayfam : Form
    {
        private byte[] _mevcutResim;
        private TaramaTipi _taramaTipi = TaramaTipi.KursiyerResmi;
        private bool _isCropping;
        private Point _cropStart;
        private Rectangle _cropRect;
        private readonly string _connectionString;
        private readonly KoleraIstampaRepository _istampaRepo;
        private ComboBox _cmbIstampaAlan;
        private Button _btnIstampaKaydet;
        private Button _btnIstampaUygula;
        private Button _btnIstampaOzellikler;
        private ComboBox _cmbIstampaSecim;
        private bool _istampaModuAktif;
        private byte[] _aktifIstampaResim;
        private Cursor _normalCursor;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        // 🔹 Ana forma taranan resmi göndermek için event
        public event Action<byte[]> TaramaTamamlandi;

        

        // WinForms Designer formu parametresiz olusturur.
        public Tarama_Sayfam()
            : this(null, TaramaTipi.KursiyerResmi, string.Empty)
        {
        }

        public Tarama_Sayfam(byte[] mevcutResim = null, TaramaTipi tip = TaramaTipi.KursiyerResmi, string connectionString = "")
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            _connectionString = connectionString ?? string.Empty;
            _istampaRepo = new KoleraIstampaRepository(_connectionString);
            _mevcutResim = mevcutResim;
            _taramaTipi = tip;

            
            Btn_Tara.Click += Btn_Tara_Click;
            Btn_Dosya.Click += Btn_Dosya_Click;
            TrackBarBrightness.Scroll += TrackBarBrightness_Scroll;
            ListBox1.DrawMode = DrawMode.OwnerDrawFixed;
            ListBox1.DrawItem += ListBox1_DrawItem;
            RESIM_TARAMA.MouseDown += RESIM_TARAMA_MouseDown;
            RESIM_TARAMA.MouseMove += RESIM_TARAMA_MouseMove;
            RESIM_TARAMA.MouseUp += RESIM_TARAMA_MouseUp;
            RESIM_TARAMA.MouseClick += RESIM_TARAMA_MouseClick;
            RESIM_TARAMA.Paint += RESIM_TARAMA_Paint;
            Btn_Uygun_Yap.Click += Btn_Uygun_Yap_Click;
            Btn_Parlak.Click += Btn_Parlak_Click;
            Load += Tarama_Sayfam_Load;

            TrackBarBrightness.Minimum = -100;
            TrackBarBrightness.Maximum = 100;
            TrackBarBrightness.TickFrequency = 10;
            TrackBarBrightness.Value = 0;
            EnsureIstampaControls();
            EnsureStampControlsOnEditPanel();
        }

        public enum TaramaTipi
        {
            KursiyerResmi,
            Evrak,
            Imza
        }

        // 🔹 Tarama işlemi (WIA)
        private void Btn_Tara_Click(object sender, EventArgs e)
        {
            string tempFile = null;
            try
            {
                WIA.CommonDialog dialog = new WIA.CommonDialog();
                ImageFile image = dialog.ShowAcquireImage(
                    WiaDeviceType.ScannerDeviceType,
                    WiaImageIntent.ColorIntent,
                    WiaImageBias.MaximizeQuality,
                    FormatID.wiaFormatJPEG,
                    false,
                    true,
                    false
                );

                if (image != null)
                {
                    tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
                    image.SaveFile(tempFile);

                    byte[] tarananResim = File.ReadAllBytes(tempFile);
                    DisplayImage(tarananResim);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tarama sırasında hata oluştu:\n" + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(tempFile) && File.Exists(tempFile))
                        File.Delete(tempFile);
                }
                catch
                {
                }
            }
        }

        // 🔹 Dosyadan açma
        private void Btn_Dosya_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "JPEG Dosyaları|*.jpg;*.jpeg|PNG Dosyaları|*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    byte[] tarananResim = File.ReadAllBytes(ofd.FileName);
                    DisplayImage(tarananResim);
                }
            }
        }

        // 🔹 Kaydet (ana forma gönderme, dosya dialogu yok)
        private void Btn_Save_Click(object sender, EventArgs e)
        {
            {
                if (RESIM_TARAMA.Image == null)
                {
                    MessageBox.Show("Kaydedilecek resim yok!", "Uyarı",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    byte[] data;
                    int hedefW;
                    int hedefH;
                    int minKb;
                    int maxKb;
                    GetTargetRules(out hedefW, out hedefH, out minKb, out maxKb);

                    if (ShouldPreserveTransparency())
                    {
                        // Imza/Kase gibi damgalarda PNG alpha kanali korunur.
                        data = ResizeCurrentImage(hedefW, hedefH, true);
                    }
                    else
                    {
                        data = GetCurrentImageJpegBytes();
                        data = ResizeImage(data, hedefW, hedefH);
                        data = CompressToTargetKb(data, minKb, maxKb);
                    }

                    if (data == null || data.Length == 0)
                    {
                        MessageBox.Show("Resim hedef boyuta uygun hale getirilemedi.", "Uyari",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _mevcutResim = data;

                    TaramaTamamlandi?.Invoke(_mevcutResim);

                    MessageBox.Show("Resim kaydedildi.",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resim kaydedilemedi: " + ex.Message,
                        "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private byte[] ResizeImage(byte[] data, int width, int height)
        {
            try
            {
                using (var ms = new MemoryStream(data))
                using (var original = new Bitmap(ms))
                {
                    using (Bitmap yeni = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(yeni))
                        {
                            g.Clear(ShouldPreserveTransparency() ? Color.Transparent : Color.White);
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.DrawImage(original, 0, 0, width, height);
                        }

                        using (var msOut = new MemoryStream())
                        {
                            yeni.Save(msOut, ShouldPreserveTransparency() ? ImageFormat.Png : ImageFormat.Jpeg);
                            return msOut.ToArray();
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Resim işlenemedi.", "Hata");
                return null;
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        private byte[] CompressToTargetKb(byte[] source, int minKb, int maxKb)
        {
            if (source == null || source.Length == 0)
                return null;

            var jpegEncoder = GetEncoder(ImageFormat.Jpeg);
            if (jpegEncoder == null)
                return source;

            byte[] best = source;
            for (long quality = 95; quality >= 30; quality -= 5)
            {
                using (var inMs = new MemoryStream(source))
                using (var bmp = new Bitmap(inMs))
                using (var outMs = new MemoryStream())
                using (var enc = new EncoderParameters(1))
                {
                    enc.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                    bmp.Save(outMs, jpegEncoder, enc);
                    var candidate = outMs.ToArray();
                    var kb = candidate.Length / 1024;

                    best = candidate;
                    if (kb >= minKb && kb <= maxKb)
                        return candidate;
                }
            }

            var finalKb = best.Length / 1024;
            return finalKb <= maxKb ? best : null;
        }


        // 🔹 Resmi PictureBox'ta göster ve _mevcutResim'i güncelle
        private void DisplayImage(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var img = Image.FromStream(ms))
            {
                if (RESIM_TARAMA.Image != null)
                    RESIM_TARAMA.Image.Dispose();

                RESIM_TARAMA.Image = new Bitmap(img);
            }

            RESIM_TARAMA.SizeMode = PictureBoxSizeMode.Zoom;

            _mevcutResim = data;
            UpdateImageStatusList();
        }

        private void Tarama_Sayfam_Load(object sender, EventArgs e)
        {
            InitializeUiByTaramaTipi();

            if (_mevcutResim != null && _mevcutResim.Length > 0)
                DisplayImage(_mevcutResim);
            else
                UpdateImageStatusList();
        }

        private void InitializeUiByTaramaTipi()
        {
            if (_cmbIstampaAlan != null)
            {
                _cmbIstampaAlan.Visible = false;
                _btnIstampaKaydet.Visible = false;
            }

            switch (_taramaTipi)
            {
                case TaramaTipi.KursiyerResmi:
                    Text = "Tarama - Kursiyer Resmi";
                    Label4.Text = "Hedef profil: Kursiyer";
                    break;
                case TaramaTipi.Evrak:
                    Text = "Tarama - Evrak";
                    Label4.Text = "Hedef profil: Evrak";
                    break;
                case TaramaTipi.Imza:
                    Text = "Tarama - Imza";
                    Label4.Text = "Hedef profil: Imza";
                    if (_cmbIstampaAlan != null)
                    {
                        _cmbIstampaAlan.Visible = true;
                        _btnIstampaKaydet.Visible = true;
                        YukleIstampaAlanlari();
                    }
                    break;
            }
        }

        private void EnsureIstampaControls()
        {
            if (Grp_1 == null)
                return;

            _cmbIstampaAlan = new ComboBox
            {
                Name = "Cmb_IstampaAlan",
                Left = 27,
                Top = 476,
                Width = 170,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Visible = false
            };

            _btnIstampaKaydet = new Button
            {
                Name = "Btn_IstampaKaydet",
                Text = "Istampaya Kaydet",
                Left = 27,
                Top = 506,
                Width = 170,
                Height = 34,
                Visible = false
            };
            _btnIstampaKaydet.Click += Btn_IstampaKaydet_Click;

            Grp_1.Controls.Add(_cmbIstampaAlan);
            Grp_1.Controls.Add(_btnIstampaKaydet);
            _cmbIstampaAlan.BringToFront();
            _btnIstampaKaydet.BringToFront();
        }

        private void EnsureStampControlsOnEditPanel()
        {
            if (Grp_2 == null)
                return;

            _btnIstampaUygula = new Button
            {
                Name = "Btn_IstampaUygula",
                Text = "ISTAMPA",
                Left = 19,
                Top = 110,
                Width = 90,
                Height = 25
            };
            _btnIstampaUygula.Click += Btn_IstampaUygula_Click;

            _cmbIstampaSecim = new ComboBox
            {
                Name = "Cmb_IstampaSecim",
                Left = 114,
                Top = 112,
                Width = 126,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cmbIstampaSecim.Items.Add("IMZA");
            _cmbIstampaSecim.Items.Add("MUHUR");
            _cmbIstampaSecim.Items.Add("KASE");
            _cmbIstampaSecim.Items.Add("ASLI_GIBIDIR");
            _cmbIstampaSecim.Items.Add("INCELENDI");
            _cmbIstampaSecim.SelectedIndex = 0;

            _btnIstampaOzellikler = new Button
            {
                Name = "Btn_IstampaOzellikler",
                Text = "Ozellikler",
                Left = 19,
                Top = 142,
                Width = 221,
                Height = 25
            };
            _btnIstampaOzellikler.Click += Btn_IstampaOzellikler_Click;

            Grp_2.Controls.Add(_btnIstampaUygula);
            Grp_2.Controls.Add(_cmbIstampaSecim);
            Grp_2.Controls.Add(_btnIstampaOzellikler);
            _btnIstampaUygula.BringToFront();
            _cmbIstampaSecim.BringToFront();
            _btnIstampaOzellikler.BringToFront();
        }

        private void Btn_IstampaUygula_Click(object sender, EventArgs e)
        {
            string kod = Convert.ToString(_cmbIstampaSecim?.SelectedItem) ?? string.Empty;
            byte[] stamp = _istampaRepo.GetImageByCode(kod);
            if (stamp == null || stamp.Length == 0)
            {
                MessageBox.Show("Secili istampa icin kayitli resim bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _aktifIstampaResim = stamp;
            _istampaModuAktif = true;
            _normalCursor = Cursor;
            Cursor = CreateStampCursor(stamp) ?? Cursors.Cross;
            RESIM_TARAMA.Cursor = Cursor;
        }

        private void Btn_IstampaOzellikler_Click(object sender, EventArgs e)
        {
            using (var frm = new KoleraIstampaForm(_connectionString))
            {
                frm.ShowDialog(this);
            }
        }

        private void RESIM_TARAMA_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_istampaModuAktif || _aktifIstampaResim == null || _aktifIstampaResim.Length == 0)
                return;
            if (RESIM_TARAMA.Image == null)
                return;

            using (var stampMs = new MemoryStream(_aktifIstampaResim))
            using (var stampImg = Image.FromStream(stampMs))
            {
                var baseBmp = new Bitmap(RESIM_TARAMA.Image);
                using (var g = Graphics.FromImage(baseBmp))
                {
                    int hedefX = (int)Math.Round((double)e.X * baseBmp.Width / Math.Max(1, RESIM_TARAMA.Width));
                    int hedefY = (int)Math.Round((double)e.Y * baseBmp.Height / Math.Max(1, RESIM_TARAMA.Height));
                    int stampW = Math.Max(30, baseBmp.Width / 6);
                    int stampH = (int)Math.Round((double)stampImg.Height * stampW / Math.Max(1, stampImg.Width));
                    hedefX -= stampW / 2;
                    hedefY -= stampH / 2;
                    g.DrawImage(stampImg, new Rectangle(hedefX, hedefY, stampW, stampH));
                }

                RESIM_TARAMA.Image = baseBmp;
                using (var ms = new MemoryStream())
                {
                    baseBmp.Save(ms, ShouldPreserveTransparency() ? ImageFormat.Png : ImageFormat.Jpeg);
                    _mevcutResim = ms.ToArray();
                }
                UpdateImageStatusList();
            }

            _istampaModuAktif = false;
            _aktifIstampaResim = null;
            Cursor = _normalCursor ?? Cursors.Default;
            RESIM_TARAMA.Cursor = Cursors.Default;
        }

        private Cursor CreateStampCursor(byte[] stampBytes)
        {
            try
            {
                using (var ms = new MemoryStream(stampBytes))
                using (var img = Image.FromStream(ms))
                using (var bmp = new Bitmap(32, 32))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(img, new Rectangle(0, 0, 32, 32));
                    IntPtr hIcon = bmp.GetHicon();
                    try
                    {
                        return new Cursor(hIcon);
                    }
                    finally
                    {
                        DestroyIcon(hIcon);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private void YukleIstampaAlanlari()
        {
            var dt = _istampaRepo.GetAll();
            _cmbIstampaAlan.DataSource = dt;
            _cmbIstampaAlan.DisplayMember = "ALAN_ADI";
            _cmbIstampaAlan.ValueMember = "ALAN_KODU";
            if (_cmbIstampaAlan.Items.Count > 0)
                _cmbIstampaAlan.SelectedIndex = 0;
        }

        private void Btn_IstampaKaydet_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                MessageBox.Show("Veritabani baglantisi bulunamadi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_aktifResimYok())
            {
                MessageBox.Show("Kaydedilecek resim yok.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string kod = Convert.ToString(_cmbIstampaAlan.SelectedValue) ?? string.Empty;
            string ad = Convert.ToString(_cmbIstampaAlan.Text) ?? string.Empty;
            if (!_istampaRepo.SaveImage(kod, ad, _mevcutResim, "Tarama ekranindan kaydedildi", out string hata))
            {
                MessageBox.Show(string.IsNullOrEmpty(hata) ? "Kayit basarisiz." : hata, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("KOLERA_ISTAMPA tablosuna kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool _aktifResimYok()
        {
            return _mevcutResim == null || _mevcutResim.Length == 0;
        }

        private void UpdateImageStatusList()
        {
            try
            {
                if (RESIM_TARAMA.Image == null)
                    return;

                byte[] currentBytes = GetCurrentImageJpegBytes();
                if (currentBytes == null || currentBytes.Length == 0)
                    return;

                int width = RESIM_TARAMA.Image.Width;
                int height = RESIM_TARAMA.Image.Height;
                double kb = Math.Round(currentBytes.Length / 1024d, 2);

                int hedefW;
                int hedefH;
                int minKb;
                int maxKb;
                GetTargetRules(out hedefW, out hedefH, out minKb, out maxKb);

                bool ebatUygun = width == hedefW && height == hedefH;
                bool kbUygun = kb >= minKb && kb <= maxKb;
                bool uygun = ebatUygun && kbUygun;

                ListBox1.Items.Clear();
                ListBox1.Items.Add("EBAT : " + width + " x " + height + " px");
                ListBox1.Items.Add("BOYUT: " + kb.ToString("0.##") + " KB");
                ListBox1.Items.Add("HEDEF: " + hedefW + "x" + hedefH + " / " + minKb + "-" + maxKb + " KB");
                ListBox1.Items.Add(uygun ? "UYGUNDUR" : "UYGUN DEĞİL");
                ListBox1.Refresh();
            }
            catch
            {
                // Bilgilendirme amaçlı alan; hata olursa akışı bozma.
            }
        }

        private void GetTargetRules(out int hedefW, out int hedefH, out int minKb, out int maxKb)
        {
            if (_taramaTipi == TaramaTipi.KursiyerResmi)
            {
                hedefW = 394;
                hedefH = 512;
                minKb = 10;
                maxKb = 99;
                return;
            }

            if (_taramaTipi == TaramaTipi.Imza)
            {
                hedefW = 472;
                hedefH = 472;
                minKb = 10;
                maxKb = 99;
                return;
            }

            hedefW = 1280;
            hedefH = 1729;
            minKb = 10;
            maxKb = 97;
        }

        private byte[] GetCurrentImageJpegBytes()
        {
            if (RESIM_TARAMA.Image == null)
                return null;

            using (var bmp = new Bitmap(RESIM_TARAMA.Image))
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private byte[] ResizeCurrentImage(int width, int height, bool asPng)
        {
            if (RESIM_TARAMA.Image == null)
                return null;

            using (var original = new Bitmap(RESIM_TARAMA.Image))
            using (var yeni = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(yeni))
            using (var ms = new MemoryStream())
            {
                g.Clear(asPng ? Color.Transparent : Color.White);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, width, height);
                yeni.Save(ms, asPng ? ImageFormat.Png : ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        private bool ShouldPreserveTransparency()
        {
            return _taramaTipi == TaramaTipi.Imza;
        }

        private void ListBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0 || e.Index >= ListBox1.Items.Count)
                return;

            string text = Convert.ToString(ListBox1.Items[e.Index]) ?? string.Empty;
            Color color = e.ForeColor;
            string upper = text.ToUpperInvariant();
            if (upper.Contains("UYGUNDUR"))
                color = Color.Green;
            else if (upper.Contains("UYGUN DEĞİL") || upper.Contains("UYGUN DEGIL"))
                color = Color.Red;

            TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, color, TextFormatFlags.Left);
            e.DrawFocusRectangle();
        }

        private void RESIM_TARAMA_MouseDown(object sender, MouseEventArgs e)
        {
            if (RESIM_TARAMA.Image == null || e.Button != MouseButtons.Left)
                return;

            _isCropping = true;
            _cropStart = e.Location;
            _cropRect = new Rectangle(e.Location, Size.Empty);
        }

        private void RESIM_TARAMA_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isCropping)
                return;

            _cropRect = GetNormalizedRectangle(_cropStart, e.Location);
            RESIM_TARAMA.Invalidate();
        }

        private void RESIM_TARAMA_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isCropping)
                return;

            _isCropping = false;
            _cropRect = GetNormalizedRectangle(_cropStart, e.Location);
            ApplyCrop();
            RESIM_TARAMA.Invalidate();
        }

        private void RESIM_TARAMA_Paint(object sender, PaintEventArgs e)
        {
            if (_isCropping && _cropRect.Width > 1 && _cropRect.Height > 1)
            {
                using (var pen = new Pen(Color.LimeGreen, 2))
                {
                    e.Graphics.DrawRectangle(pen, _cropRect);
                }
            }
        }

        private static Rectangle GetNormalizedRectangle(Point p1, Point p2)
        {
            int x = Math.Min(p1.X, p2.X);
            int y = Math.Min(p1.Y, p2.Y);
            int w = Math.Abs(p1.X - p2.X);
            int h = Math.Abs(p1.Y - p2.Y);
            return new Rectangle(x, y, w, h);
        }

        private void ApplyCrop()
        {
            if (RESIM_TARAMA.Image == null || _cropRect.Width < 2 || _cropRect.Height < 2)
                return;

            var displayRect = GetImageDisplayRectangle(RESIM_TARAMA);
            var realRect = Rectangle.Intersect(_cropRect, displayRect);
            if (realRect.Width < 2 || realRect.Height < 2)
                return;

            using (var src = new Bitmap(RESIM_TARAMA.Image))
            {
                float scaleX = (float)src.Width / displayRect.Width;
                float scaleY = (float)src.Height / displayRect.Height;

                var srcRect = new Rectangle(
                    (int)((realRect.X - displayRect.X) * scaleX),
                    (int)((realRect.Y - displayRect.Y) * scaleY),
                    Math.Max(1, (int)(realRect.Width * scaleX)),
                    Math.Max(1, (int)(realRect.Height * scaleY))
                );
                srcRect.Intersect(new Rectangle(0, 0, src.Width, src.Height));
                if (srcRect.Width < 2 || srcRect.Height < 2)
                    return;

                using (var cropped = src.Clone(srcRect, src.PixelFormat))
                using (var ms = new MemoryStream())
                {
                    cropped.Save(ms, ShouldPreserveTransparency() ? ImageFormat.Png : ImageFormat.Jpeg);
                    var bytes = ms.ToArray();
                    DisplayImage(bytes);
                }
            }
        }

        private static Rectangle GetImageDisplayRectangle(PictureBox pb)
        {
            if (pb.Image == null)
                return Rectangle.Empty;

            if (pb.SizeMode != PictureBoxSizeMode.Zoom)
                return pb.ClientRectangle;

            int imgW = pb.Image.Width;
            int imgH = pb.Image.Height;
            int boxW = pb.ClientSize.Width;
            int boxH = pb.ClientSize.Height;

            float ratio = Math.Min((float)boxW / imgW, (float)boxH / imgH);
            int drawW = (int)(imgW * ratio);
            int drawH = (int)(imgH * ratio);
            int x = (boxW - drawW) / 2;
            int y = (boxH - drawH) / 2;
            return new Rectangle(x, y, drawW, drawH);
        }

        // 🔹 Döndürme
        private void Btn_Sol_Click(object sender, EventArgs e)
        {
            if (RESIM_TARAMA.Image == null) return;
            RESIM_TARAMA.Image.RotateFlip(RotateFlipType.Rotate270FlipNone);
            RESIM_TARAMA.Refresh();
            UpdateImageStatusList();
        }

        private void Bnt_Sag_Click(object sender, EventArgs e)
        {
            if (RESIM_TARAMA.Image == null) return;
            RESIM_TARAMA.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
            RESIM_TARAMA.Refresh();
            UpdateImageStatusList();
        }

        // 🔹 Parlaklık
        private void ApplyBrightness(float brightness)
        {
            if (_mevcutResim == null || _mevcutResim.Length == 0) return;

            using (var ms = new MemoryStream(_mevcutResim))
            using (Bitmap original = new Bitmap(ms))
            {
                Bitmap temp = new Bitmap(original.Width, original.Height);

                float b = brightness;
                ColorMatrix cm = new ColorMatrix(new float[][]
                {
            new float[]{1,0,0,0,0},
            new float[]{0,1,0,0,0},
            new float[]{0,0,1,0,0},
            new float[]{0,0,0,1,0},
            new float[]{b,b,b,0,1}
                });

                using (Graphics g = Graphics.FromImage(temp))
                using (ImageAttributes ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(cm);
                    g.DrawImage(original,
                        new Rectangle(0, 0, temp.Width, temp.Height),
                        0, 0, original.Width, original.Height,
                        GraphicsUnit.Pixel, ia);
                }

                if (RESIM_TARAMA.Image != null)
                    RESIM_TARAMA.Image.Dispose();

                RESIM_TARAMA.Image = temp;
                UpdateImageStatusList();
            }
        }

        private void TrackBarBrightness_Scroll(object sender, EventArgs e)
        {
            float value = TrackBarBrightness.Value / 100f; // -1..1
            ApplyBrightness(value);
        }

        private void Btn_Parlak_Click(object sender, EventArgs e)
        {
            TrackBarBrightness.Value = 0;
            ApplyBrightness(0f);
        }

        private void Btn_Uygun_Yap_Click(object sender, EventArgs e)
        {
            if (RESIM_TARAMA.Image == null)
            {
                MessageBox.Show("Duzenlenecek resim yok!", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                byte[] bytes = GetCurrentImageJpegBytes();
                if (bytes == null || bytes.Length == 0)
                    return;

                int hedefW;
                int hedefH;
                int minKb;
                int maxKb;
                GetTargetRules(out hedefW, out hedefH, out minKb, out maxKb);

                var resized = ResizeImage(bytes, hedefW, hedefH);
                var compressed = CompressToTargetKb(resized, minKb, maxKb);
                if (compressed == null || compressed.Length == 0)
                {
                    MessageBox.Show("Resim hedef kurallara getirilemedi.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                TrackBarBrightness.Value = 0;
                DisplayImage(compressed);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Uygun hale getirme sirasinda hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
