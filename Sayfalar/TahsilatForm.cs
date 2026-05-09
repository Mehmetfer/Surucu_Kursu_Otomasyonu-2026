using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public class TahsilatForm : Form
    {
        private readonly string _adSoyad;
        private readonly string _tcNo;
        private readonly decimal _kalanBorc;
        private readonly int _makbuzNo;

        private DateTimePicker dtpOdemeTarihi;
        private TextBox txtOdenecekTutar;
        private TextBox txtAciklama;
        private Button btnKaydet;
        private Button btnMakbuzYazdir;
        private Button btnKapat;
        private PrintDocument printDocument;
        private readonly bool _readOnly;

        public TahsilatSonuc Sonuc { get; private set; }

        public TahsilatForm(string adSoyad, string tcNo, decimal kalanBorc, string aciklama, int makbuzNo, DateTime? odemeTarihi = null, decimal? odenecekTutar = null, bool readOnly = false)
        {
            _adSoyad = adSoyad ?? string.Empty;
            _tcNo = tcNo ?? string.Empty;
            _kalanBorc = kalanBorc;
            _makbuzNo = makbuzNo;
            _readOnly = readOnly;

            InitializeUi();
            txtAciklama.Text = aciklama ?? string.Empty;
            dtpOdemeTarihi.Value = odemeTarihi ?? DateTime.Today;
            txtOdenecekTutar.Text = (odenecekTutar ?? kalanBorc).ToString("N2", CultureInfo.GetCultureInfo("tr-TR"));
            if (_readOnly)
            {
                dtpOdemeTarihi.Enabled = false;
                txtOdenecekTutar.ReadOnly = true;
                txtAciklama.ReadOnly = true;
                btnKaydet.Enabled = false;
            }
        }

        private void InitializeUi()
        {
            Text = "Tahsilat";
            StartPosition = FormStartPosition.CenterParent;
            Width = 620;
            Height = 390;
            MinimumSize = new Size(620, 390);
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9F);

            var lblBaslik = new Label
            {
                Dock = DockStyle.Top,
                Height = 42,
                Text = "Tahsilat Bilgileri",
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(35, 55, 86),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };

            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(12),
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            panel.RowCount = 7;
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

            panel.Controls.Add(new Label { Text = "Kursiyer", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 0);
            panel.Controls.Add(new Label { Text = _adSoyad, Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 0);

            panel.Controls.Add(new Label { Text = "TC Kimlik", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 1);
            panel.Controls.Add(new Label { Text = string.IsNullOrWhiteSpace(_tcNo) ? "-" : _tcNo, Anchor = AnchorStyles.Left, AutoSize = true }, 1, 1);

            panel.Controls.Add(new Label { Text = "Makbuz No", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 2);
            panel.Controls.Add(new Label { Text = _makbuzNo.ToString(CultureInfo.InvariantCulture), Anchor = AnchorStyles.Left, AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 2);

            panel.Controls.Add(new Label { Text = "Ödeme Tarihi", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 3);
            dtpOdemeTarihi = new DateTimePicker { Anchor = AnchorStyles.Left | AnchorStyles.Right, Format = DateTimePickerFormat.Short };
            panel.Controls.Add(dtpOdemeTarihi, 1, 3);

            panel.Controls.Add(new Label { Text = "Ödenecek Tutar", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 4);
            txtOdenecekTutar = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, TextAlign = HorizontalAlignment.Right };
            panel.Controls.Add(txtOdenecekTutar, 1, 4);

            panel.Controls.Add(new Label { Text = "Açıklama", Anchor = AnchorStyles.Right, AutoSize = true }, 0, 5);
            txtAciklama = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom, Multiline = true, ScrollBars = ScrollBars.Vertical };
            panel.Controls.Add(txtAciklama, 1, 5);

            var pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor = Color.FromArgb(236, 239, 243)
            };
            btnKaydet = new Button { Width = 100, Height = 34, Text = "Kaydet", BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnKaydet.FlatAppearance.BorderSize = 0;
            btnMakbuzYazdir = new Button { Width = 140, Height = 34, Text = "Makbuz Yazdır", BackColor = Color.FromArgb(46, 125, 50), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnMakbuzYazdir.FlatAppearance.BorderSize = 0;
            btnKapat = new Button { Width = 100, Height = 34, Text = "Kapat", BackColor = Color.FromArgb(52, 73, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnKapat.FlatAppearance.BorderSize = 0;
            pnlButtons.Controls.Add(btnKaydet);
            pnlButtons.Controls.Add(btnMakbuzYazdir);
            pnlButtons.Controls.Add(btnKapat);
            panel.SetColumnSpan(pnlButtons, 2);
            panel.Controls.Add(pnlButtons, 0, 6);

            Controls.Add(panel);
            Controls.Add(lblBaslik);

            printDocument = new PrintDocument();
            printDocument.DefaultPageSettings.Landscape = false;
            printDocument.PrintPage += PrintDocument_PrintPage;

            btnKaydet.Click += BtnKaydet_Click;
            btnMakbuzYazdir.Click += BtnMakbuzYazdir_Click;
            btnKapat.Click += (s, e) => Close();
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            decimal odenen = ParseMoney(txtOdenecekTutar.Text);
            if (odenen <= 0m)
            {
                MessageBox.Show("Ödenecek tutar 0'dan büyük olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOdenecekTutar.Focus();
                return;
            }
            Sonuc = new TahsilatSonuc
            {
                MakbuzNo = _makbuzNo,
                OdemeTarihi = dtpOdemeTarihi.Value.Date,
                OdenecekTutar = odenen,
                Aciklama = (txtAciklama.Text ?? string.Empty).Trim()
            };
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnMakbuzYazdir_Click(object sender, EventArgs e)
        {
            decimal odenen = ParseMoney(txtOdenecekTutar.Text);
            if (odenen <= 0m)
            {
                MessageBox.Show("Ödenecek tutar 0'dan büyük olmalıdır.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOdenecekTutar.Focus();
                return;
            }

            using (var preview = new PrintPreviewDialog())
            {
                preview.Document = printDocument;
                preview.Width = 1000;
                preview.Height = 700;
                preview.ShowDialog(this);
            }

            if (!_readOnly)
                BtnKaydet_Click(sender, e);
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var tr = CultureInfo.GetCultureInfo("tr-TR");

            var page = e.MarginBounds;
            var makbuzRect = new Rectangle(page.Left + 40, page.Top + 20, page.Width - 80, 330);

            using (var shadow = new SolidBrush(Color.FromArgb(30, 0, 0, 0)))
                g.FillRectangle(shadow, makbuzRect.X + 6, makbuzRect.Y + 6, makbuzRect.Width, makbuzRect.Height);
            using (var bg = new SolidBrush(Color.White))
                g.FillRectangle(bg, makbuzRect);
            using (var border = new Pen(Color.FromArgb(80, 80, 80), 2))
                g.DrawRectangle(border, makbuzRect);

            var titleFont = new Font("Segoe UI", 14, FontStyle.Bold);
            var textFont = new Font("Segoe UI", 10, FontStyle.Regular);
            var small = new Font("Segoe UI", 9, FontStyle.Regular);

            g.DrawString("TAHSİLAT MAKBUZU", titleFont, Brushes.Black, makbuzRect.Right - 260, makbuzRect.Y + 18);
            g.DrawString("No: " + _makbuzNo.ToString(CultureInfo.InvariantCulture), small, Brushes.Black, makbuzRect.Right - 200, makbuzRect.Y + 52);
            g.DrawString("Tarih: " + dtpOdemeTarihi.Value.ToString("dd.MM.yyyy"), small, Brushes.Black, makbuzRect.Right - 200, makbuzRect.Y + 72);

            int y = makbuzRect.Y + 110;
            g.DrawString("Sayın: " + _adSoyad, textFont, Brushes.Black, makbuzRect.X + 22, y);
            y += 26;
            g.DrawString("TC No: " + (string.IsNullOrWhiteSpace(_tcNo) ? "-" : _tcNo), textFont, Brushes.Black, makbuzRect.X + 22, y);
            y += 26;
            g.DrawString("Açıklama: " + (txtAciklama.Text ?? string.Empty), textFont, Brushes.Black, makbuzRect.X + 22, y);
            y += 26;

            decimal tutar = ParseMoney(txtOdenecekTutar.Text);
            g.DrawString("Ödenecek Tutar: " + tutar.ToString("N2", tr) + " TL", new Font("Segoe UI", 11, FontStyle.Bold), Brushes.Black, makbuzRect.X + 22, y);

            int tableY = makbuzRect.Y + 245;
            int col1 = makbuzRect.X + 22;
            int col2 = col1 + 220;
            int col3 = col2 + 140;
            int col4 = col3 + 120;
            g.DrawString("Banka / Şube", small, Brushes.Black, col1, tableY);
            g.DrawString("Hesap No", small, Brushes.Black, col2, tableY);
            g.DrawString("Vade", small, Brushes.Black, col3, tableY);
            g.DrawString("Tutar", small, Brushes.Black, col4, tableY);
            using (var line = new Pen(Color.Gray, 1))
            {
                for (int i = 0; i < 4; i++)
                    g.DrawLine(line, makbuzRect.X + 20, tableY + 20 + (i * 18), makbuzRect.Right - 20, tableY + 20 + (i * 18));
            }

            g.DrawString("İmza: ____________________", small, Brushes.Black, makbuzRect.Right - 220, makbuzRect.Bottom - 28);
            g.DrawString("Kalan Borç: " + _kalanBorc.ToString("N2", tr) + " TL", small, Brushes.Black, makbuzRect.X + 22, makbuzRect.Bottom - 28);
        }

        private static decimal ParseMoney(string text)
        {
            var tr = CultureInfo.GetCultureInfo("tr-TR");
            decimal d;
            var t = (text ?? "0").Trim();
            if (decimal.TryParse(t, NumberStyles.Any, tr, out d))
                return d;
            if (decimal.TryParse(t.Replace(".", ","), NumberStyles.Any, tr, out d))
                return d;
            return 0m;
        }

        public class TahsilatSonuc
        {
            public int MakbuzNo { get; set; }
            public DateTime OdemeTarihi { get; set; }
            public decimal OdenecekTutar { get; set; }
            public string Aciklama { get; set; }
        }
    }
}
