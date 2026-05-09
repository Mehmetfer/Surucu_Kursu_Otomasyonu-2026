using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Forms;
using Kolera.SINAVLAR.Models;
using Kolera.Esinav.HazirlaSon;
using Kolera.Mebbis.Services;
using Kolera_Mtsk.Services;
using static Kolera_Mtsk.Sayfalar.Arama_Sayfam;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class ESINAV_HAZIRLA : Form
    {
        private EsinavHazirlaService _service;
        private readonly MebbisService _mebbisService;
        private readonly string _connectionString;
        private Dictionary<int, KursiyerEkBilgi> _kursiyerEkBilgiMap = new Dictionary<int, KursiyerEkBilgi>();
        private string _mebbisKullaniciAdi;
        private string _mebbisSifre;
        private DateTime _lastMebbisLoginAttempt = DateTime.MinValue;
        private sealed class SinavComboItem
        {
            public int ID { get; set; }
            public string Text { get; set; }
        }
        private sealed class KursiyerEkBilgi
        {
            public string AdSoyad { get; set; }
            public string TcNo { get; set; }
            public string Donem { get; set; }
            public string IstenenSinif { get; set; }
        }
        private sealed class MebbisSonuc
        {
            public string TcNo { get; set; }
            public int? TeoNot { get; set; }
            public string TeoDurum { get; set; }
        }

        public ESINAV_HAZIRLA() : this(string.Empty)
        {
        }

        public ESINAV_HAZIRLA(string connectionString)
        {
            InitializeComponent();
            _connectionString = connectionString ?? string.Empty;
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                _service = null;
                _mebbisService = null;
                return;
            }
            _service = new EsinavHazirlaService(connectionString);
            _mebbisService = new MebbisService(connectionString);
            Load += ESINAV_HAZIRLA_Load;
            Dgv_Listesi.CellDoubleClick += Dgv_Listesi_CellDoubleClick;
            Btn_EKLE.Click += Btn_EKLE_Click;
            Btn_Yeni.Click += Btn_Yeni_Click;
            Btn_TarihSil.Click += Btn_TarihSil_Click;
            button1.Click += Btn_SinavTarihiKaydet_Click;
            Dvg_Sinavlar.SelectionChanged += Dvg_Sinavlar_SelectionChanged;
            Btn_Durum_Cek.Click += Btn_Durum_Cek_Click;
            Btn_SMS_Hazirla.Click += Btn_SMS_Hazirla_Click;
            Btn_Rapor_Al.Click += Btn_Rapor_Al_Click;
        }

        private void ESINAV_HAZIRLA_Load(object sender, EventArgs e)
        {
            GridAyarla();
            Combo_Sinavlar_Doldur();
            HazirlaSinavOlusturTab();
            HazirlaMebbisTab();
        }

        private void Btn_Rapor_Al_Click(object sender, EventArgs e)
        {
            int sinavId = ResolveSelectedSinavTarihiId();
            string baslik = string.Empty;

            if (Dvg_Sinavlar.CurrentRow != null && Dvg_Sinavlar.Columns.Contains("SINAV_TARIHI"))
                baslik = Convert.ToString(Dvg_Sinavlar.CurrentRow.Cells["SINAV_TARIHI"]?.Value) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baslik))
                baslik = (Combo_Sinavlar.Text ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(baslik))
                baslik = "SINAV TARIHI: " + baslik;
            else
                baslik = "SINAV LISTESI";

            using (var raporDetay = new RaporDetay(_connectionString, "SINAV LISTESI", sinavId, "SINAV", baslik))
            {
                raporDetay.ShowDialog(this);
            }
        }

        private static int SafeGetIntCell(DataGridViewRow row, string col)
        {
            if (row == null || row.DataGridView == null || !row.DataGridView.Columns.Contains(col))
                return 0;

            int n;
            return int.TryParse(Convert.ToString(row.Cells[col]?.Value), out n) ? n : 0;
        }

        #region Grid Ayarları
        private void GridAyarla()
        {
            Dgv_Listesi.AllowUserToAddRows = false;
            Dgv_Listesi.AllowUserToDeleteRows = false;
            Dgv_Listesi.ReadOnly = false;
            Dgv_Listesi.MultiSelect = false;
            Dgv_Listesi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dgv_Listesi.RowHeadersVisible = false;
            Dgv_Listesi.AutoGenerateColumns = false;
            Dgv_Listesi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Dgv_Listesi.RowTemplate.Height = 28;
            Dgv_Listesi.EnableHeadersVisualStyles = false;
            Dgv_Listesi.BorderStyle = BorderStyle.None;
            Dgv_Listesi.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            Dgv_Listesi.GridColor = Color.Gainsboro;

            Dgv_Listesi.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(64, 64, 64);
            Dgv_Listesi.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            Dgv_Listesi.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Dgv_Listesi.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            Dgv_Listesi.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            Dgv_Listesi.DefaultCellStyle.SelectionForeColor = Color.Black;
            Dgv_Listesi.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);

            Dgv_Listesi.Columns.Clear();

            // ID gizli
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ID",
                DataPropertyName = "ID",
                ReadOnly = true,
                Visible = false
            });

            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SiraNo",
                HeaderText = "Sıra No",
                FillWeight = 60
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TC_NO",
                HeaderText = "TC",
                DataPropertyName = "TC_NO",
                ReadOnly = true,
                FillWeight = 95
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "KursiyerAdi",
                HeaderText = "Adı Soyadı",
                DataPropertyName = "KursiyerAdi",
                ReadOnly = true,
                FillWeight = 170
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Donem",
                HeaderText = "Dönemi",
                ReadOnly = true,
                FillWeight = 110
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "IstenenSinif",
                HeaderText = "İstenilen Sınıf",
                ReadOnly = true,
                FillWeight = 95
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TeoHak",
                HeaderText = "TEO_HAK",
                DataPropertyName = "TeoHak",
                ReadOnly = true,
                FillWeight = 70
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TeoNot",
                HeaderText = "TEO_NOT",
                DataPropertyName = "TeoNot",
                ReadOnly = false,
                FillWeight = 80
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TeoDurum",
                HeaderText = "TEO_DURUM",
                DataPropertyName = "TeoDurum",
                ReadOnly = true,
                FillWeight = 95
            });

            // E_Sinav bilgileri
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "E_SinavTarihi",
                HeaderText = "E_Sınav Tarihi",
                DataPropertyName = "E_SinavTarihi",
                DefaultCellStyle = { Format = "dd.MM.yyyy" },
                ReadOnly = true,
                FillWeight = 110
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "E_SinavSaati",
                HeaderText = "E_Sınav Saati",
                DataPropertyName = "E_SinavSaati",
                ReadOnly = true,
                FillWeight = 95
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "E_SinavYeri",
                HeaderText = "E_Sınav Yeri",
                DataPropertyName = "E_SinavYeri",
                ReadOnly = true,
                FillWeight = 120
            });
            Dgv_Listesi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "E_SinavAciklama",
                HeaderText = "E_Sınav Açıklama",
                DataPropertyName = "E_SinavAciklama",
                ReadOnly = true,
                FillWeight = 140
            });

            Dgv_Listesi.CellFormatting -= Dgv_Listesi_CellFormatting;
            Dgv_Listesi.CellFormatting += Dgv_Listesi_CellFormatting;
            Dgv_Listesi.CellEndEdit -= Dgv_Listesi_CellEndEdit;
            Dgv_Listesi.CellEndEdit += Dgv_Listesi_CellEndEdit;
            Dgv_Listesi.DataError -= Dgv_Listesi_DataError;
            Dgv_Listesi.DataError += Dgv_Listesi_DataError;
        }
        #endregion

        #region Mebbis Tab
        private void HazirlaMebbisTab()
        {
            Web_Mebbis.ScriptErrorsSuppressed = true;
            Web_Mebbis.DocumentCompleted -= Web_Mebbis_DocumentCompleted;
            Web_Mebbis.DocumentCompleted += Web_Mebbis_DocumentCompleted;

            Btn_Mebbis_Ac.Click -= Btn_Mebbis_Ac_Click;
            Btn_Mebbis_Ac.Click += Btn_Mebbis_Ac_Click;
        }

        private void Btn_Mebbis_Ac_Click(object sender, EventArgs e)
        {
            MebbisAc();
        }

        private void MebbisAc()
        {
            try
            {
                string mebbisKullanici;
                string mebbisSifre;
                bool bulundu = MebbisCredentialResolver.TryResolve(_connectionString, AppSession.CurrentUserName, out mebbisKullanici, out mebbisSifre);
                if (!bulundu || string.IsNullOrWhiteSpace(mebbisKullanici))
                {
                    MessageBox.Show("MEBBİS kullanıcı bilgisi bulunamadı.");
                    return;
                }

                _mebbisKullaniciAdi = mebbisKullanici;
                _mebbisSifre = mebbisSifre;
                Web_Mebbis.Navigate("https://mebbis.meb.gov.tr/default.aspx?NoSession");
            }
            catch (Exception ex)
            {
                MessageBox.Show("MEBBİS açılırken hata oluştu: " + ex.Message);
            }
        }

        private void Web_Mebbis_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (Web_Mebbis.Document == null) return;
            if (e.Url != Web_Mebbis.Url) return;
            if (string.IsNullOrWhiteSpace(_mebbisKullaniciAdi) || string.IsNullOrWhiteSpace(_mebbisSifre)) return;

            string url = Web_Mebbis.Url == null ? string.Empty : Web_Mebbis.Url.ToString();
            if (url.IndexOf("default.aspx", StringComparison.OrdinalIgnoreCase) < 0)
                return;

            if ((DateTime.Now - _lastMebbisLoginAttempt).TotalSeconds < 3)
                return;

            _lastMebbisLoginAttempt = DateTime.Now;
            MebbisAutoLogin();
        }

        private void MebbisAutoLogin()
        {
            HtmlDocument doc = Web_Mebbis.Document;
            if (doc == null) return;

            HtmlElement userBox = null;
            HtmlElement passBox = null;
            foreach (HtmlElement el in doc.GetElementsByTagName("input"))
            {
                string type = (el.GetAttribute("type") ?? string.Empty).ToLowerInvariant();
                if (type == "text" && userBox == null)
                    userBox = el;
                if (type == "password" && passBox == null)
                    passBox = el;
            }

            if (userBox != null)
                userBox.SetAttribute("value", _mebbisKullaniciAdi);
            if (passBox != null)
                passBox.SetAttribute("value", _mebbisSifre);

            foreach (HtmlElement el in doc.GetElementsByTagName("input"))
            {
                string type = (el.GetAttribute("type") ?? string.Empty).ToLowerInvariant();
                if (type == "submit" || type == "button")
                {
                    el.InvokeMember("click");
                    break;
                }
            }
        }

        private void Btn_Durum_Cek_Click(object sender, EventArgs e)
        {
            if (Dgv_Listesi.DataSource == null || Dgv_Listesi.Rows.Count == 0)
            {
                MessageBox.Show("Önce sınav listesini doldurunuz.");
                return;
            }
            if (Web_Mebbis?.Document == null)
            {
                MessageBox.Show("MEBBİS sayfası açık değil. Önce MEBBİS'e giriş yapınız.");
                return;
            }

            int guncellenen = 0;
            int bulunamayan = 0;
            int yeniEklenen = 0;
            int eslesmeyenTc = 0;

            DateTime? seciliSinavTarihi = GetSeciliSinavTarihiDate();
            Dictionary<string, MebbisSonuc> mebbisMap = GetMebbisSonucMap(seciliSinavTarihi);
            if (mebbisMap.Count == 0)
            {
                MessageBox.Show("MEBBİS sayfasında seçili tarihe ait sonuç bulunamadı.");
                return;
            }

            HashSet<string> listedekiTcSet = new HashSet<string>();

            foreach (DataGridViewRow row in Dgv_Listesi.Rows)
            {
                EsinavModel item = row.DataBoundItem as EsinavModel;
                if (item == null || item.ID <= 0) continue;

                string tc = NormalizeDigits(item.TC_NO);
                if (string.IsNullOrWhiteSpace(tc))
                {
                    bulunamayan++;
                    continue;
                }
                listedekiTcSet.Add(tc);

                MebbisSonuc sonuc;
                if (!mebbisMap.TryGetValue(tc, out sonuc))
                {
                    bulunamayan++;
                    continue;
                }

                int? notDegeri = sonuc.TeoNot;
                string durum = sonuc.TeoDurum;

                if (!UpdateTeoSonuc(item.ID, notDegeri, durum))
                {
                    bulunamayan++;
                    continue;
                }

                item.TeoNot = notDegeri.HasValue ? notDegeri.Value.ToString() : string.Empty;
                item.TeoDurum = !string.IsNullOrWhiteSpace(durum)
                    ? durum
                    : (notDegeri.HasValue ? HesaplaDurum(item.TeoNot) : "GİRMEDİ");
                guncellenen++;
            }

            List<MebbisSonuc> olmayanlar = mebbisMap.Values
                .Where(x => !listedekiTcSet.Contains(x.TcNo))
                .ToList();

            if (olmayanlar.Count > 0)
            {
                DialogResult onay = MessageBox.Show(
                    "MEBBİS'te listede olmayan " + olmayanlar.Count + " aday var. Listeye eklensin mi?",
                    "Eksik Adaylar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (onay == DialogResult.Yes)
                {
                    int secilenTarihId = ResolveSelectedSinavTarihiId();
                    if (secilenTarihId > 0)
                    {
                        foreach (MebbisSonuc sonuc in olmayanlar)
                        {
                            int kursiyerId = GetKursiyerIdByTc(sonuc.TcNo);
                            if (kursiyerId <= 0)
                            {
                                eslesmeyenTc++;
                                continue;
                            }

                            if (!InsertEsinavKaydi(kursiyerId, secilenTarihId))
                                continue;

                            UpdateTeoSonucByKursiyerVeSinav(kursiyerId, secilenTarihId, sonuc.TeoNot, sonuc.TeoDurum);
                            yeniEklenen++;
                        }
                    }
                }
            }

            Combo_Sinavlar_SelectedIndexChanged(null, null);
            MessageBox.Show(
                "MEBBİS sonuç çekme tamamlandı.\n" +
                "Güncellenen: " + guncellenen + "\n" +
                "Bulunamayan/Hata: " + bulunamayan + "\n" +
                "Yeni Eklenen: " + yeniEklenen + "\n" +
                "TC Eşleşmeyen (eklenemedi): " + eslesmeyenTc,
                "Bilgi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private DateTime? GetSeciliSinavTarihiDate()
        {
            DataRowView selectedRow = Combo_Sinavlar.SelectedItem as DataRowView;
            if (selectedRow == null)
                return null;

            object dateValue = ReadRowField(selectedRow.Row, "SINAV_TARIHI_VALUE")
                               ?? ReadRowField(selectedRow.Row, "SINAV_TARIHI_TEXT")
                               ?? ReadRowField(selectedRow.Row, "SINAV_TARIHI");
            DateTime dt;
            return TryParseSinavDate(dateValue, out dt) ? dt.Date : (DateTime?)null;
        }

        private Dictionary<string, MebbisSonuc> GetMebbisSonucMap(DateTime? hedefSinavTarihi)
        {
            Dictionary<string, MebbisSonuc> map = new Dictionary<string, MebbisSonuc>();
            HtmlDocument doc = Web_Mebbis.Document;
            if (doc == null) return map;

            foreach (HtmlElement tr in doc.GetElementsByTagName("tr"))
            {
                List<string> hucreler = GetRowCells(tr);
                if (hucreler.Count == 0) continue;

                string tc = ExtractTcFromCells(hucreler);
                if (string.IsNullOrWhiteSpace(tc)) continue;
                if (!SatirTarihEslesiyor(string.Join(" ", hucreler), hedefSinavTarihi)) continue;

                int? notDegeri;
                string durum;
                if (!TryExtractSonucFromCells(hucreler, out notDegeri, out durum))
                    continue;

                map[tc] = new MebbisSonuc
                {
                    TcNo = tc,
                    TeoNot = notDegeri,
                    TeoDurum = durum
                };
            }

            if (map.Count > 0)
                return map;

            string body = doc.Body == null ? string.Empty : (doc.Body.InnerText ?? string.Empty);
            if (string.IsNullOrWhiteSpace(body))
                return map;

            const string pattern = @"(?m)(?<tc>\b\d{11}\b)\s+(?<ad>[A-ZÇĞİÖŞÜa-zçğıöşü\s]+?)\s+(?<donem>\d{6})\s+(?<sinif>[A-Za-z0-9]+)\s+(?<tarih>\d{1,2}[./-]\d{1,2}[./-]\d{2,4})(?:\s+\d{1,2}:\d{2}:\d{2})?\s+(?<puan>\d{1,3})(?:\s+(?<durum>GEÇTİ|GECTI|KALDI|GİRMEDİ|GIRMEDI|BAŞARILI|BASARILI|BAŞARISIZ|BASARISIZ))?";
            MatchCollection matches = Regex.Matches(body, pattern, RegexOptions.IgnoreCase);
            foreach (Match m in matches)
            {
                string tc = NormalizeDigits(m.Groups["tc"].Value);
                if (tc.Length != 11) continue;

                DateTime parsedDate;
                if (!DateTime.TryParse(m.Groups["tarih"].Value, out parsedDate))
                    continue;
                if (hedefSinavTarihi.HasValue && parsedDate.Date != hedefSinavTarihi.Value.Date)
                    continue;

                int puan;
                if (!int.TryParse(m.Groups["puan"].Value, out puan))
                    continue;

                string durum = m.Groups["durum"].Success ? m.Groups["durum"].Value : string.Empty;
                if (string.IsNullOrWhiteSpace(durum))
                    durum = puan >= 70 ? "GEÇTİ" : "KALDI";
                else
                    TryExtractDurumFromText(durum, out durum);

                map[tc] = new MebbisSonuc
                {
                    TcNo = tc,
                    TeoNot = puan,
                    TeoDurum = durum
                };
            }

            return map;
        }

        private bool TryReadMebbisSonucByTc(string tc, DateTime? hedefSinavTarihi, out int? teoNot, out string teoDurum)
        {
            teoNot = null;
            teoDurum = string.Empty;
            Dictionary<string, MebbisSonuc> map = GetMebbisSonucMap(hedefSinavTarihi);
            MebbisSonuc sonuc;
            if (!map.TryGetValue(NormalizeDigits(tc), out sonuc))
                return false;

            teoNot = sonuc.TeoNot;
            teoDurum = sonuc.TeoDurum;
            return teoNot.HasValue || !string.IsNullOrWhiteSpace(teoDurum);
        }

        private static List<string> GetRowCells(HtmlElement tr)
        {
            List<string> hucreler = new List<string>();
            if (tr == null) return hucreler;
            foreach (HtmlElement td in tr.GetElementsByTagName("td"))
            {
                string val = (td.InnerText ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(val))
                    hucreler.Add(val);
            }
            return hucreler;
        }

        private static bool TcHucreEslesiyor(List<string> hucreler, string tc)
        {
            if (hucreler == null || hucreler.Count == 0 || string.IsNullOrWhiteSpace(tc))
                return false;

            string normTc = NormalizeDigits(tc);
            for (int i = 0; i < hucreler.Count; i++)
            {
                string cellDigits = NormalizeDigits(hucreler[i]);
                if (cellDigits.Length == 11 && cellDigits == normTc)
                    return true;
            }

            return false;
        }

        private static string ExtractTcFromCells(List<string> hucreler)
        {
            if (hucreler == null || hucreler.Count == 0)
                return string.Empty;

            foreach (string hucre in hucreler)
            {
                Match m = Regex.Match(hucre, @"\b\d{11}\b");
                if (m.Success)
                    return NormalizeDigits(m.Value);
            }

            return string.Empty;
        }

        private static bool TryExtractSonucFromCells(List<string> hucreler, out int? teoNot, out string teoDurum)
        {
            teoNot = null;
            teoDurum = string.Empty;
            if (hucreler == null || hucreler.Count == 0) return false;

            // Durumu genelde son hücre taşıdığı için sondan tarıyoruz.
            for (int i = hucreler.Count - 1; i >= 0; i--)
            {
                if (TryExtractDurumFromText(hucreler[i], out teoDurum))
                    break;
            }

            // Puanı hücre bazlı alıyoruz; tarih/saat gibi değerlerle karışmaması için
            // sadece tek başına sayı olan hücreleri değerlendiriyoruz.
            for (int i = hucreler.Count - 1; i >= 0; i--)
            {
                int n;
                string token = hucreler[i].Trim();
                if (Regex.IsMatch(token, @"^\d{1,3}$") && int.TryParse(token, out n) && n >= 0 && n <= 100)
                {
                    teoNot = n;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(teoDurum) && teoNot.HasValue)
                teoDurum = teoNot.Value >= 70 ? "GEÇTİ" : "KALDI";

            return teoNot.HasValue || !string.IsNullOrWhiteSpace(teoDurum);
        }

        private static bool SatirTarihEslesiyor(string text, DateTime? hedefSinavTarihi)
        {
            if (!hedefSinavTarihi.HasValue) return true;
            if (string.IsNullOrWhiteSpace(text)) return false;

            DateTime hedef = hedefSinavTarihi.Value.Date;
            MatchCollection matches = Regex.Matches(text, @"\b\d{1,2}[./-]\d{1,2}[./-]\d{2,4}\b");
            foreach (Match m in matches)
            {
                DateTime parsed;
                if (DateTime.TryParse(m.Value, out parsed) && parsed.Date == hedef)
                    return true;
            }

            return false;
        }

        private static bool TryExtractDurumFromText(string text, out string teoDurum)
        {
            teoDurum = string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return false;

            string upper = text.ToUpperInvariant();
            if (upper.Contains("GEÇTİ") || upper.Contains("GECTI") || upper.Contains("BAŞARILI") || upper.Contains("BASARILI"))
                teoDurum = "GEÇTİ";
            else if (upper.Contains("KALDI") || upper.Contains("BAŞARISIZ") || upper.Contains("BASARISIZ"))
                teoDurum = "KALDI";
            else if (upper.Contains("GİRMEDİ") || upper.Contains("GIRMEDI"))
                teoDurum = "GİRMEDİ";

            return !string.IsNullOrWhiteSpace(teoDurum);
        }

        private static bool TryExtractSonucFromText(string text, out int? teoNot, out string teoDurum)
        {
            teoNot = null;
            teoDurum = string.Empty;
            if (string.IsNullOrWhiteSpace(text)) return false;

            TryExtractDurumFromText(text, out teoDurum);

            MatchCollection sayilar = Regex.Matches(text, @"\b\d{1,3}\b");
            foreach (Match m in sayilar)
            {
                int n;
                if (!int.TryParse(m.Value, out n)) continue;
                if (n >= 0 && n <= 100)
                {
                    teoNot = n;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(teoDurum) && teoNot.HasValue)
                teoDurum = teoNot.Value >= 70 ? "GEÇTİ" : "KALDI";

            return teoNot.HasValue || !string.IsNullOrWhiteSpace(teoDurum);
        }

        private bool UpdateTeoSonuc(int teorisnvId, int? teoNot, string teoDurum)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (string.IsNullOrWhiteSpace(_connectionString) || teorisnvId <= 0)
                return false;

            const string sql = @"
UPDATE SINAV_LISTE_TEORI
SET
    TEO_NOT = @TEO_NOT,
    TEO_DURUM = @TEO_DURUM
WHERE ID = @ID";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", teorisnvId);
                    cmd.Parameters.AddWithValue("@TEO_NOT", teoNot.HasValue ? (object)teoNot.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TEO_DURUM", string.IsNullOrWhiteSpace(teoDurum) ? (object)DBNull.Value : teoDurum);
                    conn.Open();
                    var ok = cmd.ExecuteNonQuery() > 0;
                    if (ok) LisansPolitikasi.RegisterSuccessfulWrite();
                    return ok;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizeDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            char[] arr = value.Where(char.IsDigit).ToArray();
            return new string(arr);
        }

        private void Btn_SMS_Hazirla_Click(object sender, EventArgs e)
        {
            try
            {
                List<EsinavModel> rows = Dgv_Listesi.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => !r.IsNewRow)
                    .Select(r => r.DataBoundItem as EsinavModel)
                    .Where(x => x != null)
                    .ToList();
                if (rows.Count == 0)
                {
                    MessageBox.Show("SMS listesi olusturmak icin once listeyi getiriniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DateTime tarih = DateTime.Today;
                DataRowView selectedRow = Combo_Sinavlar.SelectedItem as DataRowView;
                if (selectedRow != null && selectedRow.Row.Table.Columns.Contains("SINAV_TARIHI"))
                {
                    DateTime d;
                    if (DateTime.TryParse(Convert.ToString(selectedRow.Row["SINAV_TARIHI"]), out d))
                        tarih = d.Date;
                }

                string sablon = LoadEsinavSmsTemplate();
                var smsOnizleme = BuildEsinavSmsOnizlemeVerisi(rows.First(), tarih);
                using (var sablonForm = new SmsSablonDuzenleForm(sablon, BuildEsinavSmsPreviewText(sablon, rows.First(), tarih), smsOnizleme, _connectionString))
                {
                    if (sablonForm.ShowDialog(this) != DialogResult.OK)
                        return;
                    sablon = sablonForm.TemplateText;
                }
                SaveEsinavSmsTemplate(sablon);

                string masaustu = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string dosyaYolu = Path.Combine(masaustu, tarih.ToString("dd.MM.yyyy") + "_esinav_smsatilacak.xlsx");
                ExportEsinavSmsToXlsx(dosyaYolu, rows, tarih, sablon);
                Process.Start(new ProcessStartInfo { FileName = dosyaYolu, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("E-sinav SMS excel olusturma hatasi: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportEsinavSmsToXlsx(string filePath, List<EsinavModel> rows, DateTime sinavTarihi, string templateText)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            List<int> ids = rows.Where(x => x.ID_KURSIYER > 0).Select(x => x.ID_KURSIYER).Distinct().ToList();
            Dictionary<int, string> gsmMap = GetKursiyerGsmMap(ids);
            string kursAdi;
            string telefon;
            GetSmsKurumBilgi(out kursAdi, out telefon);

            List<Tuple<string, string>> smsRows = new List<Tuple<string, string>>();
            foreach (var item in rows)
            {
                string gsm;
                if (!gsmMap.TryGetValue(item.ID_KURSIYER, out gsm))
                    continue;
                gsm = NormalizeDigits(gsm);
                if (gsm.Length < 10)
                    continue;

                string adSoyad = item.KursiyerAdi ?? string.Empty;
                string saat = NormalizeSmsSaat(item.E_SinavSaati);
                string msg = (string.IsNullOrWhiteSpace(templateText) ? DefaultEsinavSmsTemplate : templateText)
                    .Replace("[AD SOYAD]", adSoyad.Trim().ToUpperInvariant())
                    .Replace("[TARIH]", sinavTarihi.ToString("dd.MM.yyyy"))
                    .Replace("[SAAT]", saat)
                    .Replace("[KURS ADI]", kursAdi)
                    .Replace("[TELEFON]", telefon);
                smsRows.Add(Tuple.Create(gsm, msg));
            }
            if (smsRows.Count == 0)
                throw new InvalidOperationException("SMS icin uygun GSM bulunamadi.");

            using (FileStream fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite))
            using (ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Create, false, Encoding.UTF8))
            {
                WriteZipText(archive, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
                WriteZipText(archive, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
                WriteZipText(archive, "xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"SMS\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
                WriteZipText(archive, "xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
                WriteZipText(archive, "xl/worksheets/sheet1.xml", BuildSmsWorksheetXml(smsRows));
            }
        }

        private static string BuildSmsWorksheetXml(List<Tuple<string, string>> smsRows)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
            sb.Append("<row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>Telefon</t></is></c><c r=\"B1\" t=\"inlineStr\"><is><t>Mesaj</t></is></c></row>");
            int rowIndex = 2;
            foreach (var row in smsRows)
            {
                sb.Append("<row r=\"" + rowIndex + "\">");
                sb.Append("<c r=\"A" + rowIndex + "\" t=\"inlineStr\"><is><t xml:space=\"preserve\">" + EscapeXml(row.Item1) + "</t></is></c>");
                sb.Append("<c r=\"B" + rowIndex + "\" t=\"inlineStr\"><is><t xml:space=\"preserve\">" + EscapeXml(row.Item2) + "</t></is></c>");
                sb.Append("</row>");
                rowIndex++;
            }
            sb.Append("</sheetData></worksheet>");
            return sb.ToString();
        }

        private static void WriteZipText(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(true)))
                writer.Write(content);
        }

        private static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        private Dictionary<int, string> GetKursiyerGsmMap(List<int> ids)
        {
            Dictionary<int, string> map = new Dictionary<int, string>();
            if (ids == null || ids.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return map;
            string idList = string.Join(",", ids.Where(x => x > 0).Distinct());
            if (string.IsNullOrWhiteSpace(idList))
                return map;

            string[] sqls =
            {
                "SELECT ID, ISNULL(GSM_1,'') AS GSM FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(GSM_1,'') AS GSM FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")",
                "SELECT ID, ISNULL(GSM,'') AS GSM FROM dbo.KURSIYER WHERE ID IN (" + idList + ")"
            };
            foreach (string sql in sqls)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                int id = SafeGetInt(r, "ID");
                                string gsm = SafeGetString(r, "GSM");
                                if (id > 0 && !string.IsNullOrWhiteSpace(gsm) && !map.ContainsKey(id))
                                    map.Add(id, gsm);
                            }
                        }
                    }
                    if (map.Count > 0) return map;
                }
                catch { }
            }
            return map;
        }

        private void GetSmsKurumBilgi(out string kursAdi, out string telefon)
        {
            kursAdi = "METRO SURUCU KURSLARI";
            telefon = string.Empty;
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;
            string tableName = ResolveKursBilgiTableName();
            if (string.IsNullOrWhiteSpace(tableName))
                return;
            string sql = "SELECT TOP (1) ISNULL(KURS_ADI,'') AS KURS_ADI, ISNULL(TELEFON,'') AS TELEFON FROM [" + tableName + "]";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) return;
                        kursAdi = (SafeGetString(r, "KURS_ADI") ?? string.Empty).Trim().ToUpperInvariant();
                        if (string.IsNullOrWhiteSpace(kursAdi))
                            kursAdi = "METRO SURUCU KURSLARI";
                        telefon = NormalizeDigits(SafeGetString(r, "TELEFON"));
                    }
                }
            }
            catch { }
        }

        private string ResolveKursBilgiTableName()
        {
            const string sql = @"
SELECT TOP 1 TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE'
  AND UPPER(TABLE_NAME) IN ('KURSBILGIPARAM')
ORDER BY TABLE_NAME;";
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    var o = cmd.ExecuteScalar();
                    return o == null || o == DBNull.Value ? null : Convert.ToString(o);
                }
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeSmsSaat(string rawSaat)
        {
            string text = (rawSaat ?? string.Empty).Trim();
            Match m = Regex.Match(text, @"\b\d{1,2}:\d{2}\b");
            if (!m.Success) return "00:00";
            string[] parts = m.Value.Split(':');
            int h, mn;
            if (!int.TryParse(parts[0], out h) || !int.TryParse(parts[1], out mn)) return "00:00";
            if (h < 0 || h > 23 || mn < 0 || mn > 59) return "00:00";
            return h.ToString("00") + ":" + mn.ToString("00");
        }

        private string BuildEsinavSmsPreviewText(string templateText, EsinavModel row, DateTime tarih)
        {
            string kursAdi, telefon;
            GetSmsKurumBilgi(out kursAdi, out telefon);
            return (string.IsNullOrWhiteSpace(templateText) ? DefaultEsinavSmsTemplate : templateText)
                .Replace("[AD SOYAD]", (row.KursiyerAdi ?? string.Empty).Trim().ToUpperInvariant())
                .Replace("[TARIH]", tarih.ToString("dd.MM.yyyy"))
                .Replace("[SAAT]", NormalizeSmsSaat(row.E_SinavSaati))
                .Replace("[KURS ADI]", kursAdi)
                .Replace("[TELEFON]", telefon);
        }

        private SmsSablonOnizlemeVerisi BuildEsinavSmsOnizlemeVerisi(EsinavModel row, DateTime tarih)
        {
            string kursAdi, telefon;
            GetSmsKurumBilgi(out kursAdi, out telefon);
            return new SmsSablonOnizlemeVerisi
            {
                AdSoyad = (row == null ? string.Empty : (row.KursiyerAdi ?? string.Empty).Trim().ToUpperInvariant()),
                Telefon = (telefon ?? string.Empty).Trim(),
                KursAdi = (kursAdi ?? string.Empty).Trim(),
                Tarih = tarih.Date,
                Saat = row == null ? string.Empty : NormalizeSmsSaat(row.E_SinavSaati)
            };
        }

        private const string DefaultEsinavSmsTemplate =
"SAYIN [AD SOYAD]; [TARIH] SAAT:[SAAT] E-SINAVINIZ VARDIR. SINAV SAATINDEN ONCE KIMLIGINIZLE HAZIR BULUNUNUZ. [KURS ADI] [TELEFON]";

        private string GetEsinavSmsTemplatePath()
        {
            return Path.Combine(Application.StartupPath, "esinav_sms_sablon.txt");
        }

        private string LoadEsinavSmsTemplate()
        {
            string p = GetEsinavSmsTemplatePath();
            if (!File.Exists(p))
                return DefaultEsinavSmsTemplate;
            string t = File.ReadAllText(p, Encoding.UTF8).Trim();
            return string.IsNullOrWhiteSpace(t) ? DefaultEsinavSmsTemplate : t;
        }

        private void SaveEsinavSmsTemplate(string templateText)
        {
            File.WriteAllText(GetEsinavSmsTemplatePath(), string.IsNullOrWhiteSpace(templateText) ? DefaultEsinavSmsTemplate : templateText.Trim(), Encoding.UTF8);
        }

        private int GetKursiyerIdByTc(string tcNo)
        {
            string tc = NormalizeDigits(tcNo);
            if (string.IsNullOrWhiteSpace(_connectionString) || tc.Length != 11)
                return 0;

            string[] sqlAdaylari =
            {
                "SELECT TOP 1 ID FROM dbo.KURSIYER WHERE REPLACE(REPLACE(ISNULL(TC_NO,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSIYERLER WHERE REPLACE(REPLACE(ISNULL(TC_NO,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSİYER WHERE REPLACE(REPLACE(ISNULL(TC_NO,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSIYER WHERE REPLACE(REPLACE(ISNULL(TC,''),' ',''),'-','') = @TC",
                "SELECT TOP 1 ID FROM dbo.KURSIYERLER WHERE REPLACE(REPLACE(ISNULL(TC,''),' ',''),'-','') = @TC"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TC", tc);
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        int id;
                        if (int.TryParse(Convert.ToString(result), out id) && id > 0)
                            return id;
                    }
                }
                catch
                {
                    // Bir sonraki aday sorguya geç.
                }
            }

            return 0;
        }

        private bool UpdateTeoSonucByKursiyerVeSinav(int kursiyerId, int sinavTarihiId, int? teoNot, string teoDurum)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (string.IsNullOrWhiteSpace(_connectionString) || kursiyerId <= 0 || sinavTarihiId <= 0)
                return false;

            const string sql = @"
UPDATE SINAV_LISTE_TEORI
SET
    TEO_NOT = @TEO_NOT,
    TEO_DURUM = @TEO_DURUM
WHERE ID_KURSIYER = @ID_KURSIYER
  AND ID_SINAV_TARIHI = @ID_SINAV_TARIHI";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                    cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavTarihiId);
                    cmd.Parameters.AddWithValue("@TEO_NOT", teoNot.HasValue ? (object)teoNot.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TEO_DURUM", string.IsNullOrWhiteSpace(teoDurum) ? (object)DBNull.Value : teoDurum);
                    conn.Open();
                    var ok = cmd.ExecuteNonQuery() > 0;
                    if (ok) LisansPolitikasi.RegisterSuccessfulWrite();
                    return ok;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Sinav Olustur
        private void HazirlaSinavOlusturTab()
        {
            Cmb_SinavDurumu.Items.Clear();
            Cmb_SinavDurumu.Items.Add("Hazır");
            Cmb_SinavDurumu.Items.Add("Hazır Değil");
            Cmb_SinavDurumu.DropDownStyle = ComboBoxStyle.DropDownList;
            Cmb_SinavDurumu.SelectedIndex = 0;

            Dvg_Sinavlar.AllowUserToAddRows = false;
            Dvg_Sinavlar.AllowUserToDeleteRows = false;
            Dvg_Sinavlar.ReadOnly = true;
            Dvg_Sinavlar.MultiSelect = false;
            Dvg_Sinavlar.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Dvg_Sinavlar.RowHeadersVisible = false;
            Dvg_Sinavlar.AutoGenerateColumns = true;

            SinavTarihleriniYukle();
        }

        private void SinavTarihleriniYukle()
        {
            DataTable dt = GetSinavTarihleriYonetimData();
            Dvg_Sinavlar.DataSource = dt;

            if (Dvg_Sinavlar.Columns.Contains("ID"))
                Dvg_Sinavlar.Columns["ID"].Visible = false;
            if (Dvg_Sinavlar.Columns.Contains("SINAV_TARIHI"))
                Dvg_Sinavlar.Columns["SINAV_TARIHI"].HeaderText = "Sınav Tarihi";
            if (Dvg_Sinavlar.Columns.Contains("DURUM_TEXT"))
                Dvg_Sinavlar.Columns["DURUM_TEXT"].HeaderText = "Durum";
            if (Dvg_Sinavlar.Columns.Contains("ACIKLAMA"))
                Dvg_Sinavlar.Columns["ACIKLAMA"].HeaderText = "Açıklama";
        }

        private DataTable GetSinavTarihleriYonetimData()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("SINAV_TARIHI", typeof(DateTime));
            dt.Columns.Add("DURUM_TEXT", typeof(string));
            dt.Columns.Add("ACIKLAMA", typeof(string));

            string[] sqlAdaylari =
            {
                @"SELECT ID, SINAV_TARIHI, CASE WHEN ISNULL(SINAV_DURUMU,0)=1 THEN 'Hazır' ELSE 'Hazır Değil' END AS DURUM_TEXT, ISNULL(ACIKLAMA,'') AS ACIKLAMA FROM SINAV_TARIHLERI ORDER BY SINAV_TARIHI DESC, ID DESC",
                @"SELECT ID, SINAV_TARIHI, CASE WHEN ISNULL(DURUM,0)=1 THEN 'Hazır' ELSE 'Hazır Değil' END AS DURUM_TEXT, ISNULL(ACIKLAMA,'') AS ACIKLAMA FROM SINAV_TARIHLERI ORDER BY SINAV_TARIHI DESC, ID DESC",
                @"SELECT ID, SINAV_TARIHI, CASE WHEN ISNULL(SINAV_DURUMU,0)=1 THEN 'Hazır' ELSE 'Hazır Değil' END AS DURUM_TEXT, ISNULL(SINAV_ACIKLAMA,'') AS ACIKLAMA FROM SINAV_TARIHLERI ORDER BY SINAV_TARIHI DESC, ID DESC",
                @"SELECT ID, SINAV_TARIHI, 'Hazır Değil' AS DURUM_TEXT, '' AS ACIKLAMA FROM SINAV_TARIHLERI ORDER BY SINAV_TARIHI DESC, ID DESC"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                    {
                        DataTable tmp = new DataTable();
                        da.Fill(tmp);
                        if (tmp.Rows.Count >= 0)
                            return tmp;
                    }
                }
                catch
                {
                    // Sonraki aday SQL denenecek.
                }
            }

            return dt;
        }

        private void Btn_Yeni_Click(object sender, EventArgs e)
        {
            Dtp_SinavTarihi.Value = DateTime.Today;
            Cmb_SinavDurumu.SelectedIndex = 0;
            Txt_Aciklama.Text = string.Empty;
            Dvg_Sinavlar.ClearSelection();
        }

        private void Dvg_Sinavlar_SelectionChanged(object sender, EventArgs e)
        {
            if (Dvg_Sinavlar.CurrentRow == null) return;
            DataGridViewRow row = Dvg_Sinavlar.CurrentRow;

            DateTime dt;
            if (DateTime.TryParse(Convert.ToString(row.Cells["SINAV_TARIHI"]?.Value), out dt))
                Dtp_SinavTarihi.Value = dt;

            string durumText = Convert.ToString(row.Cells["DURUM_TEXT"]?.Value);
            Cmb_SinavDurumu.SelectedIndex = string.Equals(durumText, "Hazır", StringComparison.OrdinalIgnoreCase) ? 0 : 1;

            Txt_Aciklama.Text = Convert.ToString(row.Cells["ACIKLAMA"]?.Value) ?? string.Empty;
        }

        private void Btn_SinavTarihiKaydet_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            int seciliId = GetSeciliSinavTarihiId();
            DateTime sinavTarihi = Dtp_SinavTarihi.Value.Date;
            int durum = Cmb_SinavDurumu.SelectedIndex == 0 ? 1 : 0;
            string aciklama = (Txt_Aciklama.Text ?? string.Empty).Trim();

            bool ok = seciliId > 0
                ? UpdateSinavTarihi(seciliId, sinavTarihi, durum, aciklama)
                : InsertSinavTarihi(sinavTarihi, durum, aciklama);

            if (!ok)
            {
                MessageBox.Show("Sınav tarihi kaydedilemedi.");
                return;
            }

            SinavTarihleriniYukle();
            Combo_Sinavlar_Doldur();
            MessageBox.Show("Sınav tarihi kaydedildi.");
        }

        private void Btn_TarihSil_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            int seciliId = GetSeciliSinavTarihiId();
            if (seciliId <= 0)
            {
                MessageBox.Show("Silmek için listeden bir sınav tarihi seçiniz.");
                return;
            }

            DialogResult onay = MessageBox.Show("Seçili sınav tarihini silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (onay != DialogResult.Yes) return;

            if (!DeleteSinavTarihi(seciliId))
            {
                MessageBox.Show("Sınav tarihi silinemedi. Bu tarihe bağlı kayıtlar olabilir.");
                return;
            }

            SinavTarihleriniYukle();
            Combo_Sinavlar_Doldur();
            Btn_Yeni_Click(null, null);
        }

        private int GetSeciliSinavTarihiId()
        {
            if (Dvg_Sinavlar.CurrentRow == null) return 0;
            object raw = Dvg_Sinavlar.CurrentRow.Cells["ID"]?.Value;
            int id;
            return int.TryParse(Convert.ToString(raw), out id) ? id : 0;
        }

        private bool InsertSinavTarihi(DateTime tarih, int durum, string aciklama)
        {
            string[] sqlAdaylari =
            {
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_DURUMU, ACIKLAMA) VALUES (@TARIH, @DURUM, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, DURUM, ACIKLAMA) VALUES (@TARIH, @DURUM, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_DURUMU, SINAV_ACIKLAMA) VALUES (@TARIH, @DURUM, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, ACIKLAMA) VALUES (@TARIH, @ACIKLAMA)",
                "INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI) VALUES (@TARIH)"
            };
            return ExecuteSinavTarihiYazma(sqlAdaylari, tarih, durum, aciklama);
        }

        private bool UpdateSinavTarihi(int id, DateTime tarih, int durum, string aciklama)
        {
            string[] sqlAdaylari =
            {
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, DURUM=@DURUM, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, SINAV_DURUMU=@DURUM, SINAV_ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH, ACIKLAMA=@ACIKLAMA WHERE ID=@ID",
                "UPDATE SINAV_TARIHLERI SET SINAV_TARIHI=@TARIH WHERE ID=@ID"
            };
            return ExecuteSinavTarihiYazma(sqlAdaylari, tarih, durum, aciklama, id);
        }

        private bool ExecuteSinavTarihiYazma(string[] sqlAdaylari, DateTime tarih, int durum, string aciklama, int? id = null)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return false;

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@TARIH", tarih);
                        cmd.Parameters.AddWithValue("@DURUM", durum);
                        cmd.Parameters.AddWithValue("@ACIKLAMA", string.IsNullOrWhiteSpace(aciklama) ? (object)DBNull.Value : aciklama);
                        if (id.HasValue)
                            cmd.Parameters.AddWithValue("@ID", id.Value);

                        conn.Open();
                        int affected = cmd.ExecuteNonQuery();
                    if ((id.HasValue && affected > 0) || (!id.HasValue && affected >= 0))
                        LisansPolitikasi.RegisterSuccessfulWrite();
                        if (!id.HasValue)
                            return affected >= 0;
                        return affected > 0;
                    }
                }
                catch
                {
                    // Kolon farklari icin siradaki SQL adayi denenir.
                }
            }

            return false;
        }

        private bool DeleteSinavTarihi(int id)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return false;

            const string sql = "DELETE FROM SINAV_TARIHLERI WHERE ID=@ID";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch
            {
                return false;
            }
        }
        #endregion

        private void Dgv_Listesi_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = Dgv_Listesi.Columns[e.ColumnIndex].Name;
            EsinavModel current = Dgv_Listesi.Rows[e.RowIndex].DataBoundItem as EsinavModel;

            if (colName == "SiraNo")
            {
                e.Value = (e.RowIndex + 1).ToString();
                e.FormattingApplied = true;
                return;
            }
            if (current != null && colName == "Donem")
            {
                KursiyerEkBilgi info;
                if (_kursiyerEkBilgiMap.TryGetValue(current.ID_KURSIYER, out info))
                    e.Value = info.Donem ?? string.Empty;
                else
                    e.Value = string.Empty;
                e.FormattingApplied = true;
                return;
            }
            if (current != null && colName == "IstenenSinif")
            {
                KursiyerEkBilgi info;
                if (_kursiyerEkBilgiMap.TryGetValue(current.ID_KURSIYER, out info))
                    e.Value = info.IstenenSinif ?? string.Empty;
                else
                    e.Value = string.Empty;
                e.FormattingApplied = true;
                return;
            }

            if (colName == "TeoDurum")
            {
                string durum = Convert.ToString(e.Value)?.Trim().ToUpperInvariant() ?? string.Empty;
                if (durum.Contains("BAŞARILI") || durum.Contains("BASARILI") || durum.Contains("GEÇTİ") || durum.Contains("GECTI"))
                {
                    e.CellStyle.BackColor = Color.FromArgb(198, 239, 206);
                    e.CellStyle.ForeColor = Color.FromArgb(0, 97, 0);
                    e.CellStyle.Font = new Font(Dgv_Listesi.Font, FontStyle.Bold);
                }
                else if (durum.Contains("BAŞARISIZ") || durum.Contains("BASARISIZ") || durum.Contains("KALDI"))
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 199, 206);
                    e.CellStyle.ForeColor = Color.FromArgb(156, 0, 6);
                    e.CellStyle.Font = new Font(Dgv_Listesi.Font, FontStyle.Bold);
                }
            }

            if (colName == "TeoNot")
            {
                int notDegeri;
                if (int.TryParse(Convert.ToString(e.Value), out notDegeri))
                {
                    if (notDegeri >= 70)
                    {
                        e.CellStyle.BackColor = Color.FromArgb(226, 239, 218);
                        e.CellStyle.ForeColor = Color.FromArgb(0, 97, 0);
                    }
                    else
                    {
                        e.CellStyle.BackColor = Color.FromArgb(255, 235, 156);
                        e.CellStyle.ForeColor = Color.FromArgb(156, 101, 0);
                    }
                }
            }
        }

        private void Dgv_Listesi_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void Dgv_Listesi_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            if (e.RowIndex < 0) return;
            if (Dgv_Listesi.Columns[e.ColumnIndex].Name != "TeoNot") return;

            DataGridViewRow row = Dgv_Listesi.Rows[e.RowIndex];
            EsinavModel item = row.DataBoundItem as EsinavModel;
            if (item == null || item.ID <= 0) return;

            string yeniNot = Convert.ToString(row.Cells["TeoNot"].Value)?.Trim();
            if (!string.IsNullOrWhiteSpace(yeniNot))
            {
                int parsed;
                if (!int.TryParse(yeniNot, out parsed) || parsed < 0 || parsed > 100)
                {
                    MessageBox.Show("TEO_NOT değeri 0-100 arasında sayı olmalıdır.");
                    row.Cells["TeoNot"].Value = item.TeoNot;
                    return;
                }
            }

            if (!UpdateTeoNot(item.ID, yeniNot))
            {
                MessageBox.Show("TEO_NOT güncellenemedi.");
                row.Cells["TeoNot"].Value = item.TeoNot;
                return;
            }

            item.TeoNot = yeniNot ?? string.Empty;
            item.TeoDurum = HesaplaDurum(item.TeoNot);
            row.Cells["TeoDurum"].Value = item.TeoDurum;
            Dgv_Listesi.InvalidateRow(e.RowIndex);
        }

        private bool UpdateTeoNot(int teorisnvId, string teoNot)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (string.IsNullOrWhiteSpace(_connectionString) || teorisnvId <= 0)
                return false;

            const string sql = @"
UPDATE SINAV_LISTE_TEORI
SET
    TEO_NOT = @TEO_NOT,
    TEO_DURUM = CASE
        WHEN @TEO_NOT IS NULL THEN 'GİRMEDİ'
        WHEN @TEO_NOT >= 70 THEN 'GEÇTİ'
        ELSE 'KALDI'
    END
WHERE ID = @ID";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID", teorisnvId);
                    if (string.IsNullOrWhiteSpace(teoNot))
                        cmd.Parameters.AddWithValue("@TEO_NOT", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@TEO_NOT", Convert.ToInt32(teoNot));

                    conn.Open();
                    var ok = cmd.ExecuteNonQuery() > 0;
                    if (ok) LisansPolitikasi.RegisterSuccessfulWrite();
                    return ok;
                }
            }
            catch
            {
                return false;
            }
        }

        #region ComboBox Doldurma
        private void Combo_Sinavlar_Doldur()
        {
            Combo_Sinavlar.SelectedIndexChanged -= Combo_Sinavlar_SelectedIndexChanged;
            Combo_Sinavlar.Format -= Combo_Sinavlar_Format;
            Combo_Sinavlar.DropDownStyle = ComboBoxStyle.DropDownList;
            Combo_Sinavlar.DrawMode = DrawMode.Normal;
            Combo_Sinavlar.FlatStyle = FlatStyle.Standard;
            Combo_Sinavlar.BackColor = Color.White;
            Combo_Sinavlar.ForeColor = Color.Black;
            Combo_Sinavlar.DataSource = null;

            DataTable sinavTarihleri = GetComboSinavTarihleriBirlesik();
            if (sinavTarihleri.Rows.Count == 0)
                sinavTarihleri = GetSinavTarihleriFromSp();
            if (!ContainsRecentSinavTarihi(sinavTarihleri))
                sinavTarihleri = GetSinavTarihleriFromTeori();
            if (sinavTarihleri.Rows.Count == 0)
                sinavTarihleri = GetSinavTarihleriDataTable();
            sinavTarihleri = DistinctSinavTarihleriByDate(sinavTarihleri);
            if (sinavTarihleri.Rows.Count > 0)
            {
                Combo_Sinavlar.DisplayMember = sinavTarihleri.Columns.Contains("SINAV_TARIHI_TEXT")
                    ? "SINAV_TARIHI_TEXT"
                    : "SINAV_TARIHI";
                Combo_Sinavlar.ValueMember = "ID";
                Combo_Sinavlar.DataSource = sinavTarihleri;
                Combo_Sinavlar.SelectedIndex = 0;
            }
            else
            {
                Combo_Sinavlar.DisplayMember = "Text";
                Combo_Sinavlar.ValueMember = "ID";
                Combo_Sinavlar.DataSource = BuildFallbackComboItems();
                if (Combo_Sinavlar.Items.Count > 0)
                    Combo_Sinavlar.SelectedIndex = 0;
            }

            Combo_Sinavlar.Format += Combo_Sinavlar_Format;
            Combo_Sinavlar.SelectedIndexChanged += Combo_Sinavlar_SelectedIndexChanged;
        }

        private static DataTable DistinctSinavTarihleriByDate(DataTable source)
        {
            if (source == null || source.Rows.Count == 0)
                return source ?? new DataTable();

            DataTable table = source.Clone();
            Dictionary<DateTime, DataRow> secilen = new Dictionary<DateTime, DataRow>();

            foreach (DataRow row in source.Rows)
            {
                DateTime dt;
                string raw = Convert.ToString(
                    ReadRowField(row, "SINAV_TARIHI_VALUE")
                    ?? ReadRowField(row, "SINAV_TARIHI_TEXT")
                    ?? ReadRowField(row, "SINAV_TARIHI")
                );
                if (!TryParseSinavDate(raw, out dt))
                    continue;

                DateTime key = dt.Date;
                if (!secilen.ContainsKey(key))
                {
                    secilen[key] = row;
                    continue;
                }

                int mevcutId = ToInt(ReadRowField(secilen[key], "ID"));
                int yeniId = ToInt(ReadRowField(row, "ID"));
                if (yeniId > mevcutId)
                    secilen[key] = row;
            }

            foreach (DataRow row in secilen.Values.OrderByDescending(r =>
            {
                DateTime d;
                return TryParseSinavDate(
                    Convert.ToString(ReadRowField(r, "SINAV_TARIHI_VALUE")
                        ?? ReadRowField(r, "SINAV_TARIHI_TEXT")
                        ?? ReadRowField(r, "SINAV_TARIHI")), out d)
                    ? d.Date
                    : DateTime.MinValue;
            }))
            {
                table.ImportRow(row);
            }

            return table;
        }

        private static int ToInt(object value)
        {
            int n;
            return int.TryParse(Convert.ToString(value), out n) ? n : 0;
        }

        private DataTable GetComboSinavTarihleriBirlesik()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("SINAV_TARIHI", typeof(string));
            table.Columns.Add("SINAV_TARIHI_TEXT", typeof(string));
            table.Columns.Add("SINAV_TARIHI_VALUE", typeof(DateTime));

            if (string.IsNullOrWhiteSpace(_connectionString))
                return table;

            const string sql = @"
SELECT
    st.ID AS ID,
    CONVERT(varchar(10), CAST(st.SINAV_TARIHI AS date), 104) + ' (ID:' + CAST(st.ID AS varchar(20)) + ')' AS SINAV_TARIHI,
    CONVERT(varchar(10), CAST(st.SINAV_TARIHI AS date), 104) AS SINAV_TARIHI_TEXT,
    CAST(st.SINAV_TARIHI AS date) AS SINAV_TARIHI_VALUE
FROM SINAV_TARIHLERI st
WHERE st.SINAV_TARIHI IS NOT NULL
  AND st.SINAV_TARIHI > '1900-01-01'

UNION

SELECT
    ISNULL(MAX(NULLIF(slt.ID_SINAV_TARIHI, 0)), 0) AS ID,
    CONVERT(varchar(10), CAST(slt.E_SINAV_TARIHI AS date), 104) + ' (ID:' +
        CAST(ISNULL(MAX(NULLIF(slt.ID_SINAV_TARIHI, 0)), 0) AS varchar(20)) + ')' AS SINAV_TARIHI,
    CONVERT(varchar(10), CAST(slt.E_SINAV_TARIHI AS date), 104) AS SINAV_TARIHI_TEXT,
    CAST(slt.E_SINAV_TARIHI AS date) AS SINAV_TARIHI_VALUE
FROM SINAV_LISTE_TEORI slt
WHERE slt.E_SINAV_TARIHI IS NOT NULL
  AND slt.E_SINAV_TARIHI > '1900-01-01'
GROUP BY CAST(slt.E_SINAV_TARIHI AS date)

ORDER BY SINAV_TARIHI_VALUE DESC, ID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(table);
                }
            }
            catch
            {
                // Hata olursa alt fallback akislari devam eder.
            }

            return table;
        }

        private static bool ContainsRecentSinavTarihi(DataTable table)
        {
            if (table == null || table.Rows.Count == 0)
                return false;

            int minimumYear = DateTime.Now.Year - 1;
            foreach (DataRow row in table.Rows)
            {
                DateTime dt;
                string raw = Convert.ToString(
                    ReadRowField(row, "SINAV_TARIHI_VALUE")
                    ?? ReadRowField(row, "SINAV_TARIHI")
                    ?? ReadRowField(row, "SINAV_TARIHI_TEXT")
                    ?? ReadRowField(row, "E_SINAV_TARIHI")
                    ?? ReadRowField(row, "ESINAV_TARIHI")
                );

                if (DateTime.TryParse(raw, out dt) && dt.Year >= minimumYear)
                    return true;
            }

            return false;
        }

        private void Combo_Sinavlar_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem is DataRowView)
            {
                DataRowView row = (DataRowView)e.ListItem;
                string text = Convert.ToString(
                    ReadRowField(row.Row, "SINAV_TARIHI_TEXT")
                    ?? ReadRowField(row.Row, "SINAV_TARIHI")
                );
                if (string.IsNullOrWhiteSpace(text))
                    text = "Sınav #" + Convert.ToString(ReadRowField(row.Row, "ID"));
                e.Value = text;
            }
            else if (e.ListItem is SinavComboItem)
            {
                SinavComboItem item = (SinavComboItem)e.ListItem;
                e.Value = string.IsNullOrWhiteSpace(item.Text) ? ("Sınav #" + item.ID) : item.Text;
            }
        }

        private DataTable GetSinavTarihleriFromTeori()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("SINAV_TARIHI", typeof(string));

            if (string.IsNullOrWhiteSpace(_connectionString))
                return table;

            const string sql = @"
SELECT
    slt.ID_SINAV_TARIHI AS ID,
    CONVERT(varchar(10), MAX(slt.E_SINAV_TARIHI), 104) + ' (ID:' + CAST(slt.ID_SINAV_TARIHI AS varchar(20)) + ')' AS SINAV_TARIHI
FROM SINAV_LISTE_TEORI slt
WHERE slt.ID_SINAV_TARIHI IS NOT NULL
  AND slt.ID_SINAV_TARIHI > 0
  AND slt.E_SINAV_TARIHI IS NOT NULL
  AND slt.E_SINAV_TARIHI > '1900-01-01'
GROUP BY slt.ID_SINAV_TARIHI
ORDER BY MAX(slt.E_SINAV_TARIHI) DESC, slt.ID_SINAV_TARIHI DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(table);
                }
            }
            catch
            {
                // Veritabaniya erisimde hata olursa mevcut fallback akisi devam eder.
            }

            return table;
        }

        private DataTable GetSinavTarihleriFromSinavTarihleri()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("SINAV_TARIHI", typeof(string));
            table.Columns.Add("SINAV_TARIHI_TEXT", typeof(string));

            if (string.IsNullOrWhiteSpace(_connectionString))
                return table;

            const string sql = @"
SELECT
    st.ID,
    CONVERT(varchar(10), st.SINAV_TARIHI, 104) + ' (ID:' + CAST(st.ID AS varchar(20)) + ')' AS SINAV_TARIHI,
    CONVERT(varchar(10), st.SINAV_TARIHI, 104) AS SINAV_TARIHI_TEXT
FROM SINAV_TARIHLERI st
WHERE st.SINAV_TARIHI IS NOT NULL
  AND st.SINAV_TARIHI > '1900-01-01'
  AND (
        EXISTS (
            SELECT 1
            FROM SINAV_LISTE_TEORI slt
            WHERE slt.ID_SINAV_TARIHI = st.ID
        )
        OR ISNULL(st.SINAV_TURU, '') LIKE '%TEOR%'
        OR ISNULL(st.SINAV_TURU, '') LIKE '%E-SINAV%'
      )
ORDER BY st.SINAV_TARIHI DESC, st.ID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(table);
                }
            }
            catch
            {
                // Veritabani hatasinda fallback akisi devam eder.
            }

            return table;
        }

        private DataTable GetSinavTarihleriFromEsinavKayitlari()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("SINAV_TARIHI", typeof(string));
            table.Columns.Add("SINAV_TARIHI_TEXT", typeof(string));
            table.Columns.Add("SINAV_TARIHI_VALUE", typeof(DateTime));

            if (string.IsNullOrWhiteSpace(_connectionString))
                return table;

            const string sql = @"
SELECT
    ISNULL(MAX(NULLIF(slt.ID_SINAV_TARIHI, 0)), 0) AS ID,
    CONVERT(varchar(10), CAST(slt.E_SINAV_TARIHI AS date), 104) + ' (ID:' +
        CAST(ISNULL(MAX(NULLIF(slt.ID_SINAV_TARIHI, 0)), 0) AS varchar(20)) + ')' AS SINAV_TARIHI,
    CONVERT(varchar(10), CAST(slt.E_SINAV_TARIHI AS date), 104) AS SINAV_TARIHI_TEXT,
    CAST(slt.E_SINAV_TARIHI AS date) AS SINAV_TARIHI_VALUE
FROM SINAV_LISTE_TEORI slt
WHERE slt.E_SINAV_TARIHI IS NOT NULL
  AND slt.E_SINAV_TARIHI > '1900-01-01'
GROUP BY CAST(slt.E_SINAV_TARIHI AS date)
ORDER BY CAST(slt.E_SINAV_TARIHI AS date) DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(table);
                }
            }
            catch
            {
                // Veritabani hatasinda fallback akisi devam eder.
            }

            return table;
        }

        private DataTable GetSinavTarihleriFromSp()
        {
            DataTable table = new DataTable();
            if (string.IsNullOrWhiteSpace(_connectionString))
                return table;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("SP_KOLERA_ESINAV_TARIHLERI_LISTELE", conn))
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    da.Fill(table);
                }
            }
            catch
            {
                // SP bulunamazsa ya da hata olursa fallback akisi devam eder.
            }

            return table;
        }

        private DataTable GetSinavTarihleriDataTable()
        {
            MethodInfo method = _service.GetType().GetMethod("GetSinavTarihleri", BindingFlags.Public | BindingFlags.Instance);
            if (method != null)
            {
                object result = method.Invoke(_service, null);
                if (result is DataTable)
                {
                    DataTable table = (DataTable)result;
                    if (!table.Columns.Contains("SINAV_TARIHI"))
                        table.Columns.Add("SINAV_TARIHI", typeof(string));
                    if (!table.Columns.Contains("ID"))
                        table.Columns.Add("ID", typeof(int));

                    foreach (DataRow row in table.Rows)
                    {
                        if ((row["ID"] == DBNull.Value || Convert.ToInt32(row["ID"]) == 0))
                        {
                            object idCandidate = ReadRowField(row, "TEORISNV_ID")
                                                 ?? ReadRowField(row, "SINAV_ID")
                                                 ?? ReadRowField(row, "ID_SINAV_TARIHI");
                            if (idCandidate != null && idCandidate != DBNull.Value)
                                row["ID"] = Convert.ToInt32(idCandidate);
                        }

                        if (row["SINAV_TARIHI"] == DBNull.Value || string.IsNullOrWhiteSpace(Convert.ToString(row["SINAV_TARIHI"])))
                            row["SINAV_TARIHI"] = BuildRowText(row);
                    }

                    return table;
                }
            }

            return new DataTable();
        }

        private List<SinavComboItem> BuildFallbackComboItems()
        {
            List<SinavTarihModel> tarihler = _service.SinavTarihleriniGetir();
            List<SinavComboItem> comboItems = new List<SinavComboItem>();
            foreach (SinavTarihModel tarih in tarihler)
            {
                comboItems.Add(new SinavComboItem
                {
                    ID = tarih.ID,
                    Text = BuildSinavTarihText(tarih)
                });
            }

            return comboItems;
        }

        private static string BuildSinavTarihText(SinavTarihModel tarih)
        {
            if (tarih == null) return string.Empty;

            // Farkli model versiyonlarinda alan adi degisebildigi icin guvenli fallback zinciri.
            string text = ReadStringProperty(tarih, "TarihText");
            if (!string.IsNullOrWhiteSpace(text)) return text;

            DateTime? dt = ReadDateTimeProperty(tarih, "SinavTarihi")
                           ?? ReadDateTimeProperty(tarih, "Tarih")
                           ?? ReadDateTimeProperty(tarih, "TARIH");
            if (dt.HasValue) return dt.Value.ToString("dd.MM.yyyy");

            text = ReadStringProperty(tarih, "Aciklama")
                   ?? ReadStringProperty(tarih, "SINAV_ACIKLAMA")
                   ?? ReadStringProperty(tarih, "Baslik");
            if (!string.IsNullOrWhiteSpace(text)) return text;

            return "Sınav #" + tarih.ID;
        }

        private static string BuildRowText(DataRow row)
        {
            DateTime dt;
            if (DateTime.TryParse(Convert.ToString(
                ReadRowField(row, "SINAV_TARIHI")
                ?? ReadRowField(row, "ESINAV_TARIHI")
                ?? ReadRowField(row, "E_SINAV_TARIHI")
                ?? ReadRowField(row, "TARIH")
                ?? ReadRowField(row, "SinavTarihi")
                ?? ReadRowField(row, "Tarih")
            ), out dt))
                return dt.ToString("dd.MM.yyyy");

            string text = Convert.ToString(
                ReadRowField(row, "E_SINAV_ACIKLAMA")
                ?? ReadRowField(row, "ESINAV_ACIKLAMA")
                ?? ReadRowField(row, "SINAV_ACIKLAMA")
                ?? ReadRowField(row, "ACIKLAMA")
                ?? ReadRowField(row, "Aciklama")
                ?? ReadRowField(row, "BASLIK")
            );
            if (!string.IsNullOrWhiteSpace(text))
                return text;

            object id = ReadRowField(row, "ID")
                        ?? ReadRowField(row, "TEORISNV_ID")
                        ?? ReadRowField(row, "SINAV_ID")
                        ?? ReadRowField(row, "ID_SINAV_TARIHI");
            return "Sınav #" + Convert.ToString(id);
        }

        private static object ReadRowField(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) ? row[columnName] : null;
        }

        private static string ReadStringProperty(object source, string propertyName)
        {
            object value = ReadProperty(source, propertyName);
            return value == null ? null : Convert.ToString(value);
        }

        private static DateTime? ReadDateTimeProperty(object source, string propertyName)
        {
            object value = ReadProperty(source, propertyName);
            if (value == null) return null;

            if (value is DateTime)
                return (DateTime)value;

            DateTime parsed;
            if (DateTime.TryParse(Convert.ToString(value), out parsed))
                return parsed;

            return null;
        }

        private static object ReadProperty(object source, string propertyName)
        {
            var prop = source.GetType().GetProperty(propertyName);
            return prop == null ? null : prop.GetValue(source, null);
        }

        private void Combo_Sinavlar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Combo_Sinavlar.SelectedIndex < 0) return;

            int secilenTarihId = 0;
            if (Combo_Sinavlar.SelectedValue != null)
                secilenTarihId = Convert.ToInt32(Combo_Sinavlar.SelectedValue);

            DateTime? secilenTarih = null;
            DataRowView selectedRow = Combo_Sinavlar.SelectedItem as DataRowView;
            if (selectedRow != null)
            {
                object dateValue = ReadRowField(selectedRow.Row, "SINAV_TARIHI_VALUE")
                                   ?? ReadRowField(selectedRow.Row, "SINAV_TARIHI_TEXT")
                                   ?? ReadRowField(selectedRow.Row, "SINAV_TARIHI");
                if (dateValue is DateTime)
                {
                    secilenTarih = ((DateTime)dateValue).Date;
                }
                else
                {
                    DateTime dt;
                    if (DateTime.TryParse(Convert.ToString(dateValue), out dt))
                        secilenTarih = dt.Date;
                }
            }

            if (secilenTarihId == 0 && !secilenTarih.HasValue)
            {
                Dgv_Listesi.DataSource = null;
                return;
            }

            DoldurGrid(secilenTarihId, secilenTarih);
        }
        #endregion

        #region Grid Doldurma
        private void DoldurGrid(int sinavTarihiId, DateTime? sinavTarihi = null)
        {
            List<EsinavModel> kaynak = new List<EsinavModel>();
            if (sinavTarihi.HasValue)
                kaynak = GetEsinavListeByDateFromDb(sinavTarihi.Value);
            if (kaynak.Count == 0 && sinavTarihiId > 0)
                kaynak = GetEsinavListeFromSp(sinavTarihiId);
            if (kaynak.Count == 0 && sinavTarihiId > 0)
            {
                try
                {
                    kaynak = _service.Liste(sinavTarihiId);
                }
                catch
                {
                    // Serviste kullanılan SP eksik olabilir; SQL fallback ile devam edilir.
                    kaynak = new List<EsinavModel>();
                }
            }
            if (kaynak.Count == 0 && sinavTarihiId > 0)
                kaynak = GetEsinavListeBySinavTarihiIdFromDb(sinavTarihiId);

            List<EsinavModel> liste = sinavTarihi.HasValue
                ? kaynak
                : FilterBySinavTarihiId(kaynak, sinavTarihiId, sinavTarihi);

            FillKursiyerKimlikBilgileri(liste);

            foreach (EsinavModel item in liste)
            {
                if (item.TeoNot == null) item.TeoNot = "";
                if (string.IsNullOrWhiteSpace(item.TeoDurum))
                    item.TeoDurum = HesaplaDurum(item.TeoNot);
                if (item.KursiyerAdi == null) item.KursiyerAdi = "";
                if (item.TC_NO == null) item.TC_NO = "";
                if (item.Trafik == null) item.Trafik = "";
                if (item.IlkYardim == null) item.IlkYardim = "";
                if (item.Motor == null) item.Motor = "";
                if (item.E_SinavSaati == null) item.E_SinavSaati = "";
                if (item.E_SinavYeri == null) item.E_SinavYeri = "";
                if (item.E_SinavAciklama == null) item.E_SinavAciklama = "";
                if (item.TeoGirisBelgesi == null) item.TeoGirisBelgesi = "";
            }

            Dgv_Listesi.DataSource = null;
            Dgv_Listesi.DataSource = liste;
        }

        private List<EsinavModel> GetEsinavListeBySinavTarihiIdFromDb(int sinavTarihiId)
        {
            List<EsinavModel> liste = new List<EsinavModel>();
            if (string.IsNullOrWhiteSpace(_connectionString) || sinavTarihiId <= 0)
                return liste;

            const string sql = @"
SELECT
    slt.ID AS TEORISNV_ID,
    slt.ID_KURSIYER,
    slt.ID_SINAV_TARIHI,
    slt.E_SINAV_TARIHI,
    slt.TEO_NOT,
    slt.TEO_HAK,
    slt.TEO_DURUM,
    slt.E_SINAV_SAATI,
    slt.E_SINAV_YERI,
    slt.E_SINAV_ACIKLAMA
FROM SINAV_LISTE_TEORI slt
WHERE slt.ID_SINAV_TARIHI = @ID_SINAV_TARIHI
ORDER BY ISNULL(slt.TEO_HAK, 0) ASC, slt.ID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavTarihiId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EsinavModel model = new EsinavModel
                            {
                                ID = SafeGetInt(reader, "TEORISNV_ID"),
                                ID_KURSIYER = SafeGetInt(reader, "ID_KURSIYER"),
                                ID_SINAV_TARIHI = SafeGetInt(reader, "ID_SINAV_TARIHI"),
                                E_SinavTarihi = SafeGetDateTime(reader, "E_SINAV_TARIHI"),
                                TeoNot = SafeGetString(reader, "TEO_NOT"),
                                TeoHak = SafeGetNullableInt(reader, "TEO_HAK"),
                                TeoDurum = SafeGetString(reader, "TEO_DURUM"),
                                E_SinavSaati = SafeGetString(reader, "E_SINAV_SAATI"),
                                E_SinavYeri = SafeGetString(reader, "E_SINAV_YERI"),
                                E_SinavAciklama = SafeGetString(reader, "E_SINAV_ACIKLAMA"),
                                TeoGirisBelgesi = SafeGetString(reader, "TEO_GIRIS_BELGESI")
                            };
                            liste.Add(model);
                        }
                    }
                }
            }
            catch
            {
                return new List<EsinavModel>();
            }

            return liste;
        }

        private void FillKursiyerKimlikBilgileri(List<EsinavModel> liste)
        {
            if (liste == null || liste.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return;

            List<int> ids = new List<int>();
            foreach (EsinavModel item in liste)
            {
                if (item.ID_KURSIYER > 0 && !ids.Contains(item.ID_KURSIYER))
                    ids.Add(item.ID_KURSIYER);
            }

            if (ids.Count == 0)
                return;

            Dictionary<int, Tuple<string, string>> kimlikMap = GetKursiyerKimlikMap(ids);
            if (kimlikMap.Count == 0)
                return;

            _kursiyerEkBilgiMap = GetKursiyerEkBilgiMap(ids);

            foreach (EsinavModel item in liste)
            {
                Tuple<string, string> info;
                if (kimlikMap.TryGetValue(item.ID_KURSIYER, out info))
                {
                    if (string.IsNullOrWhiteSpace(item.KursiyerAdi))
                        item.KursiyerAdi = info.Item1 ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(item.TC_NO))
                        item.TC_NO = info.Item2 ?? string.Empty;
                }
            }
        }

        private Dictionary<int, Tuple<string, string>> GetKursiyerKimlikMap(List<int> ids)
        {
            Dictionary<int, Tuple<string, string>> result = new Dictionary<int, Tuple<string, string>>();
            if (ids == null || ids.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return result;

            string idList = string.Join(",", ids);
            string[] sqlAdaylari =
            {
                "SELECT ID, LTRIM(RTRIM(ISNULL(ADI,'') + ' ' + ISNULL(SOYADI,''))) AS ADSOYAD, ISNULL(TC_NO,'') AS TC FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, LTRIM(RTRIM(ISNULL(ADI,'') + ' ' + ISNULL(SOYADI,''))) AS ADSOYAD, ISNULL(TC_NO,'') AS TC FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")",
                "SELECT ID, LTRIM(RTRIM(ISNULL(ADI,'') + ' ' + ISNULL(SOYADI,''))) AS ADSOYAD, ISNULL(TC_NO,'') AS TC FROM dbo.KURSİYER WHERE ID IN (" + idList + ")",
                "SELECT ID, LTRIM(RTRIM(ISNULL(AD,'') + ' ' + ISNULL(SOYAD,''))) AS ADSOYAD, ISNULL(TC,'') AS TC FROM dbo.KURSIYER WHERE ID IN (" + idList + ")"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = SafeGetInt(reader, "ID");
                                if (id <= 0) continue;
                                string adSoyad = SafeGetString(reader, "ADSOYAD");
                                string tc = SafeGetString(reader, "TC");
                                result[id] = Tuple.Create(adSoyad, tc);
                            }
                        }
                    }

                    if (result.Count > 0)
                        return result;
                }
                catch
                {
                    // Tablo/kolon yoksa sonraki aday sorgu denenir.
                }
            }

            return result;
        }

        private Dictionary<int, KursiyerEkBilgi> GetKursiyerEkBilgiMap(List<int> ids)
        {
            Dictionary<int, KursiyerEkBilgi> result = new Dictionary<int, KursiyerEkBilgi>();
            if (ids == null || ids.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return result;

            string idList = string.Join(",", ids);
            string[] sqlAdaylari =
            {
                "SELECT ID, CAST(ISNULL(ID_GRUP_KARTI, 0) AS varchar(20)) AS DONEM, ISNULL(SERTIFIKA_SINIFI, '') AS SINIF FROM dbo.KURSIYER WHERE ID IN (" + idList + ")",
                "SELECT ID, CAST(ISNULL(ID_GRUP_KARTI, 0) AS varchar(20)) AS DONEM, ISNULL(SERTIFIKA_SINIFI, '') AS SINIF FROM dbo.KURSIYERLER WHERE ID IN (" + idList + ")",
                "SELECT ID, CAST(ISNULL(ID_GRUP_KARTI, 0) AS varchar(20)) AS DONEM, ISNULL(SINIFI, '') AS SINIF FROM dbo.KURSIYER WHERE ID IN (" + idList + ")"
            };

            foreach (string sql in sqlAdaylari)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = SafeGetInt(reader, "ID");
                                if (id <= 0) continue;
                                KursiyerEkBilgi info = new KursiyerEkBilgi
                                {
                                    Donem = SafeGetString(reader, "DONEM"),
                                    IstenenSinif = SafeGetString(reader, "SINIF")
                                };
                                result[id] = info;
                            }
                        }
                    }

                    ResolveDonemAdlari(result);
                    if (result.Count > 0)
                        return result;
                }
                catch
                {
                    // Tablo/kolon yoksa sonraki aday sorgu denenir.
                }
            }

            return result;
        }

        private void ResolveDonemAdlari(Dictionary<int, KursiyerEkBilgi> map)
        {
            if (map == null || map.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return;

            List<int> donemIds = new List<int>();
            foreach (var kv in map)
            {
                int id;
                if (int.TryParse(kv.Value?.Donem, out id) && id > 0 && !donemIds.Contains(id))
                    donemIds.Add(id);
            }
            if (donemIds.Count == 0)
                return;

            Dictionary<int, string> donemAdiMap = GetDonemAdiMap(donemIds);
            if (donemAdiMap.Count == 0)
                return;

            foreach (var kv in map)
            {
                int id;
                if (!int.TryParse(kv.Value?.Donem, out id) || id <= 0)
                    continue;

                string ad;
                if (donemAdiMap.TryGetValue(id, out ad) && !string.IsNullOrWhiteSpace(ad))
                    kv.Value.Donem = ad;
            }
        }

        private Dictionary<int, string> GetDonemAdiMap(List<int> donemIds)
        {
            Dictionary<int, string> result = new Dictionary<int, string>();
            if (donemIds == null || donemIds.Count == 0 || string.IsNullOrWhiteSpace(_connectionString))
                return result;

            string idList = string.Join(",", donemIds);
            string donemTable = GetDonemTableName();
            if (string.IsNullOrWhiteSpace(donemTable))
                return result;

            string sql = "SELECT ID, DONEM_ADI FROM " + donemTable + " WHERE ID IN (" + idList + ")";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = SafeGetInt(reader, "ID");
                            if (id <= 0) continue;
                            string donemAdi = SafeGetString(reader, "DONEM_ADI");
                            result[id] = donemAdi;
                        }
                    }
                }
            }
            catch
            {
                return result;
            }

            return result;
        }

        private string GetDonemTableName()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return null;

            const string sql = @"
SELECT TOP 1
    QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME) AS FULL_TABLE_NAME
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.COLUMN_NAME IN ('ID', 'DONEM_ADI')
GROUP BY c.TABLE_SCHEMA, c.TABLE_NAME
HAVING COUNT(DISTINCT c.COLUMN_NAME) = 2
ORDER BY
    CASE WHEN c.TABLE_NAME LIKE '%DONEM%' THEN 0 ELSE 1 END,
    c.TABLE_NAME";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result == null ? null : Convert.ToString(result);
                }
            }
            catch
            {
                return null;
            }
        }

        private List<EsinavModel> GetEsinavListeByDateFromDb(DateTime sinavTarihi)
        {
            List<EsinavModel> liste = new List<EsinavModel>();
            if (string.IsNullOrWhiteSpace(_connectionString))
                return liste;

            string kursiyerTableName = GetKursiyerTableName();
            string selectKursiyer = string.IsNullOrWhiteSpace(kursiyerTableName)
                ? "'' AS KursiyerAdi, '' AS TC_NO"
                : "LTRIM(RTRIM(ISNULL(k.ADI, '') + ' ' + ISNULL(k.SOYADI, ''))) AS KursiyerAdi, ISNULL(k.TC_NO, '') AS TC_NO";
            string joinKursiyer = string.IsNullOrWhiteSpace(kursiyerTableName)
                ? string.Empty
                : "LEFT JOIN " + kursiyerTableName + " k ON k.ID = slt.ID_KURSIYER";

            string sql = @"
SELECT
    slt.ID AS TEORISNV_ID,
    slt.ID_KURSIYER,
    slt.ID_SINAV_TARIHI,
    slt.E_SINAV_TARIHI,
    slt.TEO_NOT,
    slt.TEO_HAK,
    slt.TEO_DURUM,
    slt.E_SINAV_SAATI,
    slt.E_SINAV_YERI,
    slt.E_SINAV_ACIKLAMA,
    " + selectKursiyer + @"
FROM SINAV_LISTE_TEORI slt
" + joinKursiyer + @"
WHERE slt.E_SINAV_TARIHI IS NOT NULL
  AND CAST(slt.E_SINAV_TARIHI AS date) = @SINAV_TARIHI
ORDER BY ISNULL(slt.TEO_HAK, 0) ASC, slt.ID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SINAV_TARIHI", sinavTarihi.Date);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EsinavModel model = new EsinavModel
                            {
                                ID = SafeGetInt(reader, "TEORISNV_ID"),
                                ID_KURSIYER = SafeGetInt(reader, "ID_KURSIYER"),
                                ID_SINAV_TARIHI = SafeGetInt(reader, "ID_SINAV_TARIHI"),
                                E_SinavTarihi = SafeGetDateTime(reader, "E_SINAV_TARIHI"),
                                TeoNot = SafeGetString(reader, "TEO_NOT"),
                                TeoHak = SafeGetNullableInt(reader, "TEO_HAK"),
                                TeoDurum = SafeGetString(reader, "TEO_DURUM"),
                                E_SinavSaati = SafeGetString(reader, "E_SINAV_SAATI"),
                                E_SinavYeri = SafeGetString(reader, "E_SINAV_YERI"),
                                E_SinavAciklama = SafeGetString(reader, "E_SINAV_ACIKLAMA"),
                                TeoGirisBelgesi = SafeGetString(reader, "TEO_GIRIS_BELGESI")
                            };
                            liste.Add(model);
                        }
                    }
                }
            }
            catch
            {
                // Bu fallback sorgusu hata verirse mevcut akista bos liste dondurulur.
            }

            return liste;
        }

        private string GetKursiyerTableName()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return null;

            const string sql = @"
SELECT TOP 1
    QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME) AS FULL_TABLE_NAME
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.COLUMN_NAME IN ('ID', 'ADI', 'SOYADI', 'TC_NO')
GROUP BY c.TABLE_SCHEMA, c.TABLE_NAME
HAVING COUNT(DISTINCT c.COLUMN_NAME) = 4
ORDER BY
    CASE WHEN c.TABLE_NAME LIKE '%KURSIYER%' THEN 0 ELSE 1 END,
    c.TABLE_NAME";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    return result == null ? null : Convert.ToString(result);
                }
            }
            catch
            {
                return null;
            }
        }

        private List<EsinavModel> GetEsinavListeFromSp(int sinavTarihiId)
        {
            List<EsinavModel> liste = new List<EsinavModel>();
            if (string.IsNullOrWhiteSpace(_connectionString) || sinavTarihiId <= 0)
                return liste;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("SP_KOLERA_KURSIYER_ESINAVI", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavTarihiId);
                    cmd.Parameters.AddWithValue("@ID_KURSIYER", DBNull.Value);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            EsinavModel model = new EsinavModel
                            {
                                ID = SafeGetInt(reader, "TEORISNV_ID"),
                                ID_KURSIYER = SafeGetInt(reader, "ID_KURSIYER"),
                                ID_SINAV_TARIHI = SafeGetInt(reader, "ID_SINAV_TARIHI"),
                                E_SinavTarihi = SafeGetDateTime(reader, "ESINAV_TARIHI") ?? SafeGetDateTime(reader, "E_SINAV_TARIHI"),
                                TeoNot = SafeGetString(reader, "TEO_NOT"),
                                TeoHak = SafeGetNullableInt(reader, "TEO_HAK"),
                                TeoDurum = SafeGetString(reader, "TEO_DURUM"),
                                E_SinavSaati = SafeGetString(reader, "E_SINAV_SAATI"),
                                E_SinavYeri = SafeGetString(reader, "E_SINAV_YERI"),
                                E_SinavAciklama = SafeGetString(reader, "E_SINAV_ACIKLAMA"),
                                TeoGirisBelgesi = SafeGetString(reader, "TEO_GIRIS_BELGESI"),
                                KursiyerAdi = SafeGetString(reader, "KursiyerAdi"),
                                TC_NO = SafeGetString(reader, "TC_NO")
                            };

                            liste.Add(model);
                        }
                    }
                }
            }
            catch
            {
                // Parametre uyumsuzlugu ya da SP hatasinda mevcut servis akisi devam eder.
            }

            return liste;
        }

        private static int SafeGetInt(SqlDataReader reader, string column)
        {
            int idx;
            try { idx = reader.GetOrdinal(column); }
            catch { return 0; }

            if (reader.IsDBNull(idx)) return 0;
            int value;
            return int.TryParse(Convert.ToString(reader.GetValue(idx)), out value) ? value : 0;
        }

        private static int? SafeGetNullableInt(SqlDataReader reader, string column)
        {
            int idx;
            try { idx = reader.GetOrdinal(column); }
            catch { return null; }

            if (reader.IsDBNull(idx)) return null;
            int value;
            return int.TryParse(Convert.ToString(reader.GetValue(idx)), out value) ? value : (int?)null;
        }

        private static string SafeGetString(SqlDataReader reader, string column)
        {
            int idx;
            try { idx = reader.GetOrdinal(column); }
            catch { return string.Empty; }

            return reader.IsDBNull(idx) ? string.Empty : Convert.ToString(reader.GetValue(idx));
        }

        private static DateTime? SafeGetDateTime(SqlDataReader reader, string column)
        {
            int idx;
            try { idx = reader.GetOrdinal(column); }
            catch { return null; }

            if (reader.IsDBNull(idx)) return null;
            DateTime dt;
            return DateTime.TryParse(Convert.ToString(reader.GetValue(idx)), out dt) ? dt : (DateTime?)null;
        }

        private static List<EsinavModel> FilterBySinavTarihiId(List<EsinavModel> kaynak, int sinavTarihiId, DateTime? sinavTarihi)
        {
            if (kaynak == null || kaynak.Count == 0) return new List<EsinavModel>();

            PropertyInfo idProp = typeof(EsinavModel).GetProperty("ID_SINAV_TARIHI");
            PropertyInfo tarihProp = typeof(EsinavModel).GetProperty("E_SinavTarihi");
            if (idProp == null && tarihProp == null) return kaynak;

            List<EsinavModel> filtreli = new List<EsinavModel>();
            foreach (EsinavModel item in kaynak)
            {
                bool match = false;

                if (sinavTarihiId > 0 && idProp != null)
                {
                    object raw = idProp.GetValue(item, null);
                    int id = 0;
                    if (raw != null && raw != DBNull.Value)
                        int.TryParse(Convert.ToString(raw), out id);
                    match = id == sinavTarihiId;
                }

                if (!match && sinavTarihi.HasValue && tarihProp != null)
                {
                    object rawTarih = tarihProp.GetValue(item, null);
                    DateTime dt;
                    if (rawTarih != null && DateTime.TryParse(Convert.ToString(rawTarih), out dt))
                        match = dt.Date == sinavTarihi.Value.Date;
                }

                if (match)
                    filtreli.Add(item);
            }

            // Bazi servislerde zaten filtreli dondugu icin property bos gelebilir.
            return filtreli.Count > 0 ? filtreli : kaynak;
        }

        private static string HesaplaDurum(string teoNotText)
        {
            int notDegeri;
            if (!int.TryParse(teoNotText, out notDegeri))
                return "GİRMEDİ";

            return notDegeri >= 70 ? "GEÇTİ" : "KALDI";
        }
        #endregion

        #region Kursiyer Ekleme
        public void KursiyerEkle(int kursiyerId)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            if (Combo_Sinavlar.SelectedIndex < 0) return;

            int secilenTarihId = ResolveSelectedSinavTarihiId();
            if (secilenTarihId <= 0)
            {
                MessageBox.Show("Seçilen sınav tarihi için geçerli ID bulunamadı.");
                return;
            }

            EsinavModel model = new EsinavModel();
            model.ID_SINAV_TARIHI = secilenTarihId;
            model.ID_KURSIYER = kursiyerId;
            model.TeoHak = 4;

            bool eklendi = false;
            try
            {
                _service.Ekle(model);
                eklendi = true;
                LisansPolitikasi.RegisterSuccessfulWrite();
            }
            catch
            {
                // Servis başarısız olursa doğrudan SQL fallback kullanılacak.
            }

            if (!eklendi)
                eklendi = InsertEsinavKaydi(kursiyerId, secilenTarihId);

            if (!eklendi)
            {
                MessageBox.Show("Aday ekleme işlemi yapılamadı.");
                return;
            }

            DoldurGrid(secilenTarihId);
        }

        private void Btn_EKLE_Click(object sender, EventArgs e)
        {
            if (Combo_Sinavlar.SelectedValue == null)
            {
                MessageBox.Show("Önce sınav tarihi seçiniz.");
                return;
            }

            int secilenTarihId = ResolveSelectedSinavTarihiId();
            if (secilenTarihId <= 0)
            {
                MessageBox.Show("Seçilen sınav tarihi için geçerli ID bulunamadı.");
                return;
            }

            var arama = new Arama_Sayfam(_connectionString, null) { Mod = AramaModu.SecimYap };
            arama.KursiyerSecildi += (kursiyerId) =>
            {
                try
                {
                    KursiyerEkle(kursiyerId);
                    Combo_Sinavlar_SelectedIndexChanged(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Kursiyer eklenemedi: " + ex.Message);
                }
            };

            arama.ShowDialog();
        }

        private int ResolveSelectedSinavTarihiId()
        {
            int id;
            if (int.TryParse(Convert.ToString(Combo_Sinavlar.SelectedValue), out id) && id > 0)
                return id;

            DataRowView selectedRow = Combo_Sinavlar.SelectedItem as DataRowView;
            if (selectedRow == null)
                return 0;

            DateTime dt;
            object dateValue = ReadRowField(selectedRow.Row, "SINAV_TARIHI_VALUE")
                               ?? ReadRowField(selectedRow.Row, "SINAV_TARIHI_TEXT")
                               ?? ReadRowField(selectedRow.Row, "SINAV_TARIHI");
            if (!TryParseSinavDate(dateValue, out dt))
                return 0;

            int foundId = GetSinavTarihiIdByDate(dt.Date);
            if (foundId > 0)
                return foundId;

            return EnsureSinavTarihiId(dt.Date);
        }

        private static bool TryParseSinavDate(object rawValue, out DateTime date)
        {
            date = DateTime.MinValue;
            if (rawValue == null) return false;

            if (rawValue is DateTime)
            {
                date = ((DateTime)rawValue).Date;
                return true;
            }

            string raw = Convert.ToString(rawValue);
            if (string.IsNullOrWhiteSpace(raw)) return false;

            DateTime parsed;
            if (DateTime.TryParse(raw, out parsed))
            {
                date = parsed.Date;
                return true;
            }

            int idx = raw.IndexOf(" (ID:", StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                string onlyDate = raw.Substring(0, idx).Trim();
                if (DateTime.TryParse(onlyDate, out parsed))
                {
                    date = parsed.Date;
                    return true;
                }
            }

            return false;
        }

        private int GetSinavTarihiIdByDate(DateTime date)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return 0;

            const string sql = @"
SELECT TOP 1 x.ID
FROM
(
    SELECT st.ID
    FROM SINAV_TARIHLERI st
    WHERE st.SINAV_TARIHI IS NOT NULL
      AND CAST(st.SINAV_TARIHI AS date) = @TARIH

    UNION

    SELECT slt.ID_SINAV_TARIHI AS ID
    FROM SINAV_LISTE_TEORI slt
    WHERE slt.E_SINAV_TARIHI IS NOT NULL
      AND CAST(slt.E_SINAV_TARIHI AS date) = @TARIH
      AND slt.ID_SINAV_TARIHI IS NOT NULL
      AND slt.ID_SINAV_TARIHI > 0
) x
ORDER BY x.ID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TARIH", date);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    int id;
                    return int.TryParse(Convert.ToString(result), out id) ? id : 0;
                }
            }
            catch
            {
                return 0;
            }
        }

        private int EnsureSinavTarihiId(DateTime date)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return 0;

            if (string.IsNullOrWhiteSpace(_connectionString))
                return 0;

            const string sql = @"
IF NOT EXISTS (SELECT 1 FROM SINAV_TARIHLERI WHERE CAST(SINAV_TARIHI AS date) = @TARIH)
BEGIN
    INSERT INTO SINAV_TARIHLERI (SINAV_TARIHI, SINAV_TURU)
    VALUES (@TARIH, 'TEORI')
END

SELECT TOP 1 ID
FROM SINAV_TARIHLERI
WHERE CAST(SINAV_TARIHI AS date) = @TARIH
ORDER BY ID DESC";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@TARIH", date);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    int id;
                    return int.TryParse(Convert.ToString(result), out id) ? id : 0;
                }
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        #region Silme
        private void Btn_ADAYSIL_Click(object sender, EventArgs e)
        {
            if (!LisansPolitikasi.EnsureWriteAllowed())
                return;

            if (Dgv_Listesi.CurrentRow == null) return;

            DialogResult onay = MessageBox.Show(
                "Seçili kursiyeri sınav listesinden çıkarmak istiyor musunuz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (onay != DialogResult.Yes) return;

            EsinavModel secili = Dgv_Listesi.CurrentRow.DataBoundItem as EsinavModel;
            int id = 0;
            if (secili != null)
                id = secili.ID;
            if (id <= 0 && Dgv_Listesi.CurrentRow.Cells["ID"].Value != null)
                int.TryParse(Convert.ToString(Dgv_Listesi.CurrentRow.Cells["ID"].Value), out id);

            bool silindi = false;
            try
            {
                if (id > 0)
                {
                    _service.Sil(id);
                    silindi = true;
                    LisansPolitikasi.RegisterSuccessfulWrite();
                }
            }
            catch
            {
                // Servis başarısız olursa doğrudan SQL fallback kullanılacak.
            }

            if (!silindi)
            {
                int kursiyerId = secili != null ? secili.ID_KURSIYER : 0;
                int sinavTarihiId = ResolveSelectedSinavTarihiId();
                silindi = DeleteEsinavKaydi(id, kursiyerId, sinavTarihiId);
            }

            if (!silindi)
            {
                MessageBox.Show("Silinecek kayıt bulunamadı veya silme işlemi başarısız.");
                return;
            }

            Combo_Sinavlar_SelectedIndexChanged(null, null);
        }

        private bool InsertEsinavKaydi(int kursiyerId, int sinavTarihiId)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (string.IsNullOrWhiteSpace(_connectionString) || kursiyerId <= 0 || sinavTarihiId <= 0)
                return false;

            const string sql = @"
IF NOT EXISTS
(
    SELECT 1
    FROM SINAV_LISTE_TEORI
    WHERE ID_KURSIYER = @ID_KURSIYER
      AND ID_SINAV_TARIHI = @ID_SINAV_TARIHI
)
BEGIN
    INSERT INTO SINAV_LISTE_TEORI (ID_KURSIYER, ID_SINAV_TARIHI, E_SINAV_TARIHI, TEO_HAK)
    SELECT
        @ID_KURSIYER,
        @ID_SINAV_TARIHI,
        st.SINAV_TARIHI,
        4
    FROM SINAV_TARIHLERI st
    WHERE st.ID = @ID_SINAV_TARIHI
END

SELECT 1";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                    cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavTarihiId);
                    conn.Open();
                    cmd.ExecuteScalar();
                    LisansPolitikasi.RegisterSuccessfulWrite();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteEsinavKaydi(int kayitId, int kursiyerId, int sinavTarihiId)
        {
            if (!LisansPolitikasi.IsWriteAllowed)
                return false;

            if (string.IsNullOrWhiteSpace(_connectionString))
                return false;

            const string sqlById = "DELETE FROM SINAV_LISTE_TEORI WHERE ID = @ID";
            const string sqlByPair = @"
DELETE FROM SINAV_LISTE_TEORI
WHERE ID_KURSIYER = @ID_KURSIYER
  AND ID_SINAV_TARIHI = @ID_SINAV_TARIHI";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    if (kayitId > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlById, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID", kayitId);
                            if (cmd.ExecuteNonQuery() > 0)
                            {
                                LisansPolitikasi.RegisterSuccessfulWrite();
                                return true;
                            }
                        }
                    }

                    if (kursiyerId > 0 && sinavTarihiId > 0)
                    {
                        using (SqlCommand cmd = new SqlCommand(sqlByPair, conn))
                        {
                            cmd.Parameters.AddWithValue("@ID_KURSIYER", kursiyerId);
                            cmd.Parameters.AddWithValue("@ID_SINAV_TARIHI", sinavTarihiId);
                            var ok = cmd.ExecuteNonQuery() > 0;
                            if (ok) LisansPolitikasi.RegisterSuccessfulWrite();
                            return ok;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private void Dgv_Listesi_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            EsinavModel item = Dgv_Listesi.Rows[e.RowIndex].DataBoundItem as EsinavModel;
            if (item == null || item.ID_KURSIYER <= 0) return;

            using (KursiyerDetay_Sayfam detay = new KursiyerDetay_Sayfam(_connectionString, item.ID_KURSIYER))
            {
                detay.ShowDialog();
            }
        }
        #endregion
    }
}