using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public class SohbetForm : Form
    {
        private readonly string _displayName;
        private readonly string _sessionId;

        private readonly ListBox _lstMesajlar = new ListBox();
        private readonly TextBox _txtMesaj = new TextBox();
        private readonly Button _btnGonder = new Button();
        private readonly Label _lblBaslik = new Label();
        private readonly Timer _timer = new Timer();

        private long _lastSeenId;

        private static readonly object _lockObj = new object();
        private static long _messageId;
        private static readonly List<SohbetMesaj> _mesajlar = new List<SohbetMesaj>();

        public SohbetForm(string kullaniciAdi)
        {
            _sessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _displayName = string.IsNullOrWhiteSpace(kullaniciAdi)
                ? "Kullanici#" + _sessionId
                : kullaniciAdi.Trim() + "#" + _sessionId;

            InitializeUi();
            Load += SohbetForm_Load;
            FormClosed += (s, e) => _timer.Stop();
        }

        private void InitializeUi()
        {
            Text = "Sohbet";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            ShowInTaskbar = false;
            TopMost = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(360, 300);

            _lblBaslik.Text = "Genel Sohbet";
            _lblBaslik.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _lblBaslik.AutoSize = true;
            _lblBaslik.Location = new Point(10, 10);

            _lstMesajlar.Location = new Point(10, 32);
            _lstMesajlar.Size = new Size(330, 186);
            _lstMesajlar.HorizontalScrollbar = true;

            _txtMesaj.Location = new Point(10, 227);
            _txtMesaj.Size = new Size(248, 23);
            _txtMesaj.KeyDown += TxtMesaj_KeyDown;

            _btnGonder.Text = "Gonder";
            _btnGonder.Location = new Point(266, 226);
            _btnGonder.Size = new Size(74, 25);
            _btnGonder.Click += async (s, e) => await MesajGonderAsync();

            Controls.Add(_lblBaslik);
            Controls.Add(_lstMesajlar);
            Controls.Add(_txtMesaj);
            Controls.Add(_btnGonder);
        }

        private async void SohbetForm_Load(object sender, EventArgs e)
        {
            KonumlaSagAlt(Owner as Form);
            await BaslatAsync();
        }

        public void KonumlaSagAlt(Form parentForm)
        {
            Rectangle alan;
            if (parentForm != null && !parentForm.IsDisposed)
            {
                alan = parentForm.Bounds;
            }
            else
            {
                alan = Screen.PrimaryScreen.WorkingArea;
            }

            Left = alan.Right - Width - 10;
            Top = alan.Bottom - Height - 10;
        }

        private async System.Threading.Tasks.Task BaslatAsync()
        {
            _timer.Interval = 1000;
            _timer.Tick += async (s, e) => await MesajlariYukleAsync();
            _timer.Start();
            await MesajlariYukleAsync();
        }

        private async System.Threading.Tasks.Task MesajGonderAsync()
        {
            var text = (_txtMesaj.Text ?? string.Empty).Trim();
            if (text.Length == 0) return;

            lock (_lockObj)
            {
                _messageId++;
                _mesajlar.Add(new SohbetMesaj
                {
                    Id = _messageId,
                    SenderDisplay = _displayName,
                    SenderSession = _sessionId,
                    MessageText = text,
                    CreatedAt = DateTime.Now
                });
            }

            _txtMesaj.Clear();
            await MesajlariYukleAsync();
        }

        private async System.Threading.Tasks.Task MesajlariYukleAsync()
        {
            List<SohbetMesaj> yeniler;
            lock (_lockObj)
            {
                yeniler = System.Linq.Enumerable.ToList(
                    System.Linq.Enumerable.Where(_mesajlar, m => m.Id > _lastSeenId));
            }

            foreach (var m in yeniler)
            {
                _lastSeenId = m.Id;
                var at = m.CreatedAt.ToString("HH:mm");
                _lstMesajlar.Items.Add("[" + at + "] " + m.SenderDisplay + ": " + m.MessageText);
            }

            if (_lstMesajlar.Items.Count > 0)
                _lstMesajlar.TopIndex = _lstMesajlar.Items.Count - 1;

            await System.Threading.Tasks.Task.CompletedTask;
        }

        private class SohbetMesaj
        {
            public long Id { get; set; }
            public string SenderDisplay { get; set; }
            public string SenderSession { get; set; }
            public string MessageText { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private async void TxtMesaj_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            await MesajGonderAsync();
        }
    }
}
