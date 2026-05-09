using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar.EkProgramlar
{
    public partial class AylikTakvimForm : Form
    {
        private readonly List<TakvimDers> _dersler;
        private DateTime _ay;
        private readonly ToolTip _toolTip = new ToolTip();

        public AylikTakvimForm(List<TakvimDers> dersler, DateTime ay)
        {
            InitializeComponent();
            _dersler = dersler ?? new List<TakvimDers>();
            _ay = new DateTime(ay.Year, ay.Month, 1);
            btnOncekiAy.Click += (s, e) => { _ay = _ay.AddMonths(-1); Yenile(); };
            btnSonrakiAy.Click += (s, e) => { _ay = _ay.AddMonths(1); Yenile(); };
            Load += (s, e) => Yenile();
        }

        private void Yenile()
        {
            panelTakvim.Controls.Clear();
            lblAy.Text = _ay.ToString("MMMM yyyy", new CultureInfo("tr-TR"));

            var aylik = System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.Where(_dersler, x => x.Baslangic != DateTime.MinValue && x.Baslangic.Month == _ay.Month && x.Baslangic.Year == _ay.Year));
            int gunSayisi = DateTime.DaysInMonth(_ay.Year, _ay.Month);

            int cellW = 54;
            int cellH = 36;
            int leftW = 70;
            int topH = 40;

            var root = new Panel { Location = new Point(0, 0), Size = new Size(leftW + gunSayisi * cellW + 2, topH + 17 * cellH + 2), BackColor = Color.White };
            panelTakvim.Controls.Add(root);

            for (int g = 1; g <= gunSayisi; g++)
            {
                var d = new DateTime(_ay.Year, _ay.Month, g);
                var h = new Label
                {
                    Text = $"{g}\n{GunKisaAd(d.DayOfWeek)}",
                    Location = new Point(leftW + (g - 1) * cellW, 0),
                    Size = new Size(cellW, topH),
                    BorderStyle = BorderStyle.FixedSingle,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday ? Color.LightBlue : Color.Gainsboro
                };
                root.Controls.Add(h);
            }

            for (int s = 7; s <= 23; s++)
            {
                int y = topH + (s - 7) * cellH;
                root.Controls.Add(new Label
                {
                    Text = s.ToString("00") + ":00",
                    Location = new Point(0, y),
                    Size = new Size(leftW, cellH),
                    BorderStyle = BorderStyle.FixedSingle,
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = Color.WhiteSmoke
                });

                for (int g = 1; g <= gunSayisi; g++)
                {
                    DateTime day = new DateTime(_ay.Year, _ay.Month, g);
                    var ders = System.Linq.Enumerable.ToList(
                        System.Linq.Enumerable.Where(aylik, x => x.Baslangic.Date == day.Date && x.Baslangic.Hour == s));
                    var p = new Panel
                    {
                        Location = new Point(leftW + (g - 1) * cellW, y),
                        Size = new Size(cellW, cellH),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = System.Linq.Enumerable.Any(ders) ? Color.FromArgb(220, 235, 255) : Color.White
                    };
                    if (System.Linq.Enumerable.Any(ders))
                    {
                        var l = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Text = ders.Count == 1 ? Kisa(ders[0].AdayAdSoyad) : ders.Count + " ders", Font = new Font("Segoe UI", 7F) };
                        p.Controls.Add(l);
                        _toolTip.SetToolTip(p, string.Join("\n\n", System.Linq.Enumerable.Select(ders, x => $"{x.DersSaati} | {x.DersTuru} | {x.AdayAdSoyad} | {x.DersiVeren}")));
                    }
                    root.Controls.Add(p);
                }
            }
        }

        private string Kisa(string t) => string.IsNullOrWhiteSpace(t) ? "Ders" : (t.Length > 14 ? t.Substring(0, 11) + "..." : t);
        private string GunKisaAd(DayOfWeek d) => d == DayOfWeek.Monday ? "Pzt" : d == DayOfWeek.Tuesday ? "Sal" : d == DayOfWeek.Wednesday ? "Car" : d == DayOfWeek.Thursday ? "Per" : d == DayOfWeek.Friday ? "Cum" : d == DayOfWeek.Saturday ? "Cmt" : "Paz";
    }
}
