using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class IstatistikForm : Form
    {
        private readonly string _connectionString;

        private sealed class StatRow
        {
            public string YasGrubu { get; set; }
            public int KayitliToplam { get; set; }
            public int KayitliErkek { get; set; }
            public int KayitliKadin { get; set; }
            public int IlkokulErkek { get; set; }
            public int IlkokulKadin { get; set; }
            public int IlkogretimErkek { get; set; }
            public int IlkogretimKadin { get; set; }
            public int GenelLiseErkek { get; set; }
            public int GenelLiseKadin { get; set; }
            public int MeslekLiseErkek { get; set; }
            public int MeslekLiseKadin { get; set; }
            public int FakulteErkek { get; set; }
            public int FakulteKadin { get; set; }
            public int ToplamOgrenim { get; set; }
        }

        public IstatistikForm() : this(string.Empty)
        {
        }

        public IstatistikForm(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? string.Empty;
            Load += IstatistikForm_Load;
            btnHazirla.Click += btnHazirla_Click;
        }

        private void IstatistikForm_Load(object sender, EventArgs e)
        {
            dtBaslangic.Value = DateTime.Today.AddYears(-1);
            dtBitis.Value = DateTime.Today;
            YukleSertifikaSiniflari();
            GridHazirla();
        }

        private void YukleSertifikaSiniflari()
        {
            cmbSertifika.Items.Clear();
            cmbSertifika.Items.Add("TUMU");
            cmbSertifika.SelectedIndex = 0;

            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            const string sql = @"SELECT DISTINCT ISNULL(SERTIFIKA_SINIFI,'') AS SINIF
FROM KURSIYER
WHERE ISNULL(SERTIFIKA_SINIFI,'') <> ''
ORDER BY SINIF";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string s = Convert.ToString(r["SINIF"]).Trim();
                            if (!string.IsNullOrWhiteSpace(s))
                                cmbSertifika.Items.Add(s);
                        }
                    }
                }
            }
            catch
            {
                // Filter listesi bos kalabilir; hesaplama yine TUMU ile calisir.
            }
        }

        private void GridHazirla()
        {
            dgvIstatistik.AutoGenerateColumns = false;
            dgvIstatistik.Columns.Clear();
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "YasGrubu", HeaderText = "Yas Grubu", DataPropertyName = "YasGrubu", Width = 90 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "KayitliToplam", HeaderText = "Kayitli Toplam", DataPropertyName = "KayitliToplam", Width = 85 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "KayitliErkek", HeaderText = "Kayitli Erkek", DataPropertyName = "KayitliErkek", Width = 85 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "KayitliKadin", HeaderText = "Kayitli Kadin", DataPropertyName = "KayitliKadin", Width = 85 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "IlkokulErkek", HeaderText = "Ilkokul Erkek", DataPropertyName = "IlkokulErkek", Width = 85 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "IlkokulKadin", HeaderText = "Ilkokul Kadin", DataPropertyName = "IlkokulKadin", Width = 85 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "IlkogretimErkek", HeaderText = "Ilkogretim Erkek", DataPropertyName = "IlkogretimErkek", Width = 95 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "IlkogretimKadin", HeaderText = "Ilkogretim Kadin", DataPropertyName = "IlkogretimKadin", Width = 95 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "GenelLiseErkek", HeaderText = "Genel Lise Erkek", DataPropertyName = "GenelLiseErkek", Width = 95 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "GenelLiseKadin", HeaderText = "Genel Lise Kadin", DataPropertyName = "GenelLiseKadin", Width = 95 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "MeslekLiseErkek", HeaderText = "Meslek Lise Erkek", DataPropertyName = "MeslekLiseErkek", Width = 100 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "MeslekLiseKadin", HeaderText = "Meslek Lise Kadin", DataPropertyName = "MeslekLiseKadin", Width = 100 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "FakulteErkek", HeaderText = "Fakulte Erkek", DataPropertyName = "FakulteErkek", Width = 90 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "FakulteKadin", HeaderText = "Fakulte Kadin", DataPropertyName = "FakulteKadin", Width = 90 });
            dgvIstatistik.Columns.Add(new DataGridViewTextBoxColumn { Name = "ToplamOgrenim", HeaderText = "Toplam", DataPropertyName = "ToplamOgrenim", Width = 80 });
        }

        private async void btnHazirla_Click(object sender, EventArgs e)
        {
            if (dtBitis.Value.Date < dtBaslangic.Value.Date)
            {
                MessageBox.Show("Bitis tarihi, baslangic tarihinden kucuk olamaz.");
                return;
            }

            string sertifika = cmbSertifika.SelectedItem == null ? "TUMU" : cmbSertifika.SelectedItem.ToString();
            btnHazirla.Enabled = false;
            Cursor oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                var kayitlar = await System.Threading.Tasks.Task.Run(() =>
                    GetKursiyerRows(dtBaslangic.Value.Date, dtBitis.Value.Date, sertifika));
                var result = await System.Threading.Tasks.Task.Run(() =>
                    BuildStats(kayitlar, dtBitis.Value.Date));

                dgvIstatistik.DataSource = result;

                if (kayitlar.Count == 0)
                {
                    MessageBox.Show(
                        "Seçilen tarih/sınıf filtresinde kayıt bulunamadı.\n" +
                        "Tarih aralığını genişletip tekrar deneyin.",
                        "İstatistik",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "İstatistik hazırlanırken hata oluştu:\n" + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Cursor.Current = oldCursor;
                btnHazirla.Enabled = true;
            }
        }

        private sealed class KursiyerRow
        {
            public DateTime? DogumTarihi { get; set; }
            public string Tahsil { get; set; }
            public string Cinsiyet { get; set; }
        }

        private List<KursiyerRow> GetKursiyerRows(DateTime baslangic, DateTime bitis, string sertifikaSinifi)
        {
            List<KursiyerRow> list = new List<KursiyerRow>();
            if (string.IsNullOrWhiteSpace(_connectionString))
                return list;

            string[] sqlAdaylari =
            {
                @"SELECT DOGUM_TARIHI, ISNULL(TAHSILI,'') AS TAHSILI, ISNULL(CINSIYET,'') AS CINSIYET
FROM KURSIYER
WHERE CAST(ISNULL(KAYIT_TARIHI, GETDATE()) AS date) BETWEEN @BAS AND @BIT
  AND (@SINIF = 'TUMU' OR ISNULL(SERTIFIKA_SINIFI,'') = @SINIF)",
                @"SELECT DOGUM_TARIHI, ISNULL(TAHSILI,'') AS TAHSILI, ISNULL(CINSIYET,'') AS CINSIYET
FROM KURSIYERLER
WHERE CAST(ISNULL(KAYIT_TARIHI, GETDATE()) AS date) BETWEEN @BAS AND @BIT
  AND (@SINIF = 'TUMU' OR ISNULL(SERTIFIKA_SINIFI,'') = @SINIF)"
            };

            foreach (var sql in sqlAdaylari)
            {
                try
                {
                    list.Clear();
                    using (var conn = new SqlConnection(_connectionString))
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BAS", baslangic);
                        cmd.Parameters.AddWithValue("@BIT", bitis);
                        cmd.Parameters.AddWithValue("@SINIF", sertifikaSinifi ?? "TUMU");
                        conn.Open();
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                DateTime dt;
                                DateTime? dogum = DateTime.TryParse(Convert.ToString(r["DOGUM_TARIHI"]), out dt) ? (DateTime?)dt.Date : null;
                                list.Add(new KursiyerRow
                                {
                                    DogumTarihi = dogum,
                                    Tahsil = Convert.ToString(r["TAHSILI"]),
                                    Cinsiyet = Convert.ToString(r["CINSIYET"])
                                });
                            }
                        }
                    }
                    return list;
                }
                catch
                {
                    // Siradaki tablo adayi denenir.
                }
            }

            MessageBox.Show("Istatistik verisi okunamadi: kursiyer tablosu bulunamadi veya erisim hatasi.");
            return list;
        }

        private static List<StatRow> BuildStats(List<KursiyerRow> rows, DateTime referansTarih)
        {
            var gruplar = new List<StatRow>
            {
                new StatRow { YasGrubu = "16-17" },
                new StatRow { YasGrubu = "18-22" },
                new StatRow { YasGrubu = "23-44" },
                new StatRow { YasGrubu = "45+" }
            };

            foreach (var r in rows)
            {
                int age = GetAge(r.DogumTarihi, referansTarih);
                StatRow g = GetAgeGroup(gruplar, age);
                if (g == null) continue;
                g.KayitliToplam++;
                bool erkek = IsMale(r.Cinsiyet);
                if (erkek) g.KayitliErkek++;
                else g.KayitliKadin++;

                string tahsil = NormalizeTahsil(r.Tahsil);
                if (tahsil == "ILKOKUL")
                {
                    if (erkek) g.IlkokulErkek++; else g.IlkokulKadin++;
                }
                else if (tahsil == "ILKOGRETIM")
                {
                    if (erkek) g.IlkogretimErkek++; else g.IlkogretimKadin++;
                }
                else if (tahsil == "MESLEK_LISE")
                {
                    if (erkek) g.MeslekLiseErkek++; else g.MeslekLiseKadin++;
                }
                else if (tahsil == "FAKULTE")
                {
                    if (erkek) g.FakulteErkek++; else g.FakulteKadin++;
                }
                else
                {
                    if (erkek) g.GenelLiseErkek++; else g.GenelLiseKadin++;
                }
            }

            foreach (var g in gruplar)
                g.ToplamOgrenim =
                    g.IlkokulErkek + g.IlkokulKadin +
                    g.IlkogretimErkek + g.IlkogretimKadin +
                    g.GenelLiseErkek + g.GenelLiseKadin +
                    g.MeslekLiseErkek + g.MeslekLiseKadin +
                    g.FakulteErkek + g.FakulteKadin;

            // Cinsiyet yogunluguna gore siralama:
            // 1) Kayitli erkek fazla olan grup ustte
            // 2) Esitlikte kayitli kadin fazla olan grup ustte
            // 3) Tekrar esitlikte toplam kayit fazla olan grup ustte
            // 4) Tam esitlikte yas grubu dogal sirasi korunur
            var yasSirasi = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "16-17", 1 },
                { "18-22", 2 },
                { "23-44", 3 },
                { "45+", 4 }
            };
            gruplar = gruplar
                .OrderByDescending(x => x.KayitliErkek)
                .ThenByDescending(x => x.KayitliKadin)
                .ThenByDescending(x => x.KayitliToplam)
                .ThenBy(x => yasSirasi.ContainsKey(x.YasGrubu) ? yasSirasi[x.YasGrubu] : int.MaxValue)
                .ToList();

            gruplar.Add(new StatRow
            {
                YasGrubu = "TOPLAM",
                KayitliToplam = gruplar.Sum(x => x.KayitliToplam),
                KayitliErkek = gruplar.Sum(x => x.KayitliErkek),
                KayitliKadin = gruplar.Sum(x => x.KayitliKadin),
                IlkokulErkek = gruplar.Sum(x => x.IlkokulErkek),
                IlkokulKadin = gruplar.Sum(x => x.IlkokulKadin),
                IlkogretimErkek = gruplar.Sum(x => x.IlkogretimErkek),
                IlkogretimKadin = gruplar.Sum(x => x.IlkogretimKadin),
                GenelLiseErkek = gruplar.Sum(x => x.GenelLiseErkek),
                GenelLiseKadin = gruplar.Sum(x => x.GenelLiseKadin),
                MeslekLiseErkek = gruplar.Sum(x => x.MeslekLiseErkek),
                MeslekLiseKadin = gruplar.Sum(x => x.MeslekLiseKadin),
                FakulteErkek = gruplar.Sum(x => x.FakulteErkek),
                FakulteKadin = gruplar.Sum(x => x.FakulteKadin),
                ToplamOgrenim = gruplar.Sum(x => x.ToplamOgrenim)
            });

            return gruplar;
        }

        private static int GetAge(DateTime? dogum, DateTime refDate)
        {
            if (!dogum.HasValue) return -1;
            int age = refDate.Year - dogum.Value.Year;
            if (refDate < dogum.Value.AddYears(age)) age--;
            return age;
        }

        private static string NormalizeTahsil(string raw)
        {
            string t = (raw ?? string.Empty).Trim().ToUpperInvariant();
            if (t.Length == 0) return string.Empty;
            if (t.Contains("ILKOKUL")) return "ILKOKUL";
            if (t.Contains("ILKOGRET") || t.Contains("ORTAOKUL")) return "ILKOGRETIM";
            if (t.Contains("MESLEK")) return "MESLEK_LISE";
            if (t.Contains("FAKULTE") || t.Contains("YUKSEKOKUL") || t.Contains("Y.O")) return "FAKULTE";
            if (t.Contains("LISE")) return "GENEL_LISE";
            return string.Empty;
        }

        private static bool IsMale(string cinsiyet)
        {
            string c = (cinsiyet ?? string.Empty).Trim().ToUpperInvariant();
            return c == "E" || c == "ERKEK" || c == "M";
        }

        private static StatRow GetAgeGroup(List<StatRow> gruplar, int age)
        {
            if (age >= 16 && age <= 17) return gruplar[0];
            if (age >= 18 && age <= 22) return gruplar[1];
            if (age >= 23 && age <= 44) return gruplar[2];
            if (age >= 45) return gruplar[3];
            return null;
        }
    }
}
