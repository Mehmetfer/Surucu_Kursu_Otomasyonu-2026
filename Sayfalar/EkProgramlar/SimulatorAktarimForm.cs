using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Kolera_Mtsk.Sayfalar.EkProgramlar
{
    public partial class SimulatorAktarimForm : Form
    {
        private List<TakvimDers> _dersler = new List<TakvimDers>();
        private readonly List<SimilatorKayit> _kayitlar = new List<SimilatorKayit>();
        private bool _comboHazir;
        private readonly string _initialKullanici;
        private readonly string _initialSifre;
        private readonly string _connectionString;

        public SimulatorAktarimForm(string kullaniciAdi = null, string sifre = null, string connectionString = null)
        {
            InitializeComponent();
            _initialKullanici = kullaniciAdi;
            _initialSifre = sifre;
            _connectionString = connectionString;
            Load += SimulatorAktarimForm_Load;
            btnGiris.Click += btnGiris_Click;
            btnTeorikCek.Click += async (s, e) => await DersleriCek(true);
            btnUygulamaCek.Click += async (s, e) => await DersleriCek(false);
            btnTakvimAc.Click += btnTakvimAc_Click;
            btnSimListele.Click += async (s, e) => await BtnSimListele_Click();
            btnZipOlustur.Click += async (s, e) => await ZipOlusturAsync(null);
            comboHocalar.SelectedIndexChanged += (s, e) => { if (!_comboHazir) return; };
        }

        private async void SimulatorAktarimForm_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_initialKullanici))
                txtKullanici.Text = _initialKullanici;
            if (!string.IsNullOrWhiteSpace(_initialSifre))
                txtSifre.Text = _initialSifre;
            YukleKursAdi();

            await webView1.EnsureCoreWebView2Async(null);
            webView1.CoreWebView2.Navigate("https://mebbis.meb.gov.tr/default.aspx?lg1");

            if (!string.IsNullOrWhiteSpace(txtKullanici.Text) && !string.IsNullOrWhiteSpace(txtSifre.Text))
            {
                await Task.Delay(1500);
                await MebbisGirisDenemeAsync(false);
            }
        }

        private void YukleKursAdi()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                return;

            try
            {
                string tableName = ResolveKursBilgiTableName();
                if (string.IsNullOrWhiteSpace(tableName))
                    return;
                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = new SqlCommand("SELECT TOP 1 ISNULL(KURS_ADI,'') FROM [" + tableName + "]", conn))
                {
                    conn.Open();
                    var val = cmd.ExecuteScalar();
                    var kursAdi = val == null ? string.Empty : val.ToString().Trim();
                    if (!string.IsNullOrWhiteSpace(kursAdi))
                        txtKursAdi.Text = kursAdi;
                }
            }
            catch
            {
                // Baglanti/tabloda sorun olsa da form acilisin kesmeyelim.
            }
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

        private async void btnGiris_Click(object sender, EventArgs e)
        {
            await MebbisGirisDenemeAsync(true);
        }

        private async Task MebbisGirisDenemeAsync(bool mesajGoster)
        {
            string userJs = ToJsonString(txtKullanici.Text);
            string passJs = ToJsonString(txtSifre.Text);
            string script = $@"(function(){{
var txt=document.querySelector('input[type=text]');
var pass=document.querySelector('input[type=password]');
var btn=document.querySelector('input[type=submit]');
if(txt) txt.value={userJs};
if(pass) pass.value={passJs};
if(btn) btn.click();
return true; }})();";
            await webView1.ExecuteScriptAsync(script);
            if (mesajGoster)
                MessageBox.Show("Giris denendi.");
        }

        private async Task DersleriCek(bool teorik)
        {
            var pf = new ProgressForm();
            pf.Show(this);
            pf.SetText(teorik ? "Teorik dersler cekiliyor..." : "Uygulama dersler cekiliyor...");
            pf.SetProgress(20);
            try
            {
                var yeni = teorik ? await TeorikDersleriCek() : await UygulamaDersleriCek();
                _dersler.AddRange(yeni);
                _dersler = System.Linq.Enumerable.ToList(
                    System.Linq.Enumerable.OrderBy(
                        System.Linq.Enumerable.Select(
                            System.Linq.Enumerable.GroupBy(
                                System.Linq.Enumerable.Where(_dersler, x => x != null && x.Baslangic != DateTime.MinValue && !string.IsNullOrWhiteSpace(x.DersiVeren)),
                                x => new { x.DersTarihi, x.DersSaati, x.DersiVeren, x.Subesi, x.AdayAdSoyad }),
                            g => System.Linq.Enumerable.First(g)),
                        x => x.Baslangic));
                EkProgramState.SonCekilenDersler = new List<TakvimDers>(_dersler);
                HocalariDoldur();
                pf.SetProgress(100);
                pf.SetText("Tamamlandi");
                MessageBox.Show($"{yeni.Count} kayit cekildi.");
            }
            finally
            {
                await Task.Delay(300);
                pf.Close();
            }
        }

        private async Task BtnSimListele_Click()
        {
            var pf = new ProgressForm { TopMost = true, StartPosition = FormStartPosition.CenterScreen };
            pf.Show(this);
            try
            {
                _kayitlar.Clear();
                DateTime bitis = DateTime.Now;
                int yil = rb1Yil.Checked ? 1 : rb3Yil.Checked ? 3 : 5;
                DateTime baslangic = bitis.AddYears(-yil);

                var simAraclar = await SimAraclariniGetir();
                if (simAraclar.Count == 0)
                {
                    MessageBox.Show("Simulator araci bulunamadi.");
                    return;
                }

                int toplamAdim = System.Linq.Enumerable.Sum(simAraclar, _ => AdimSayisiHesapla(baslangic, bitis));
                int current = 0;

                foreach (var arac in simAraclar)
                {
                    DateTime currentStart = baslangic;
                    while (currentStart < bitis)
                    {
                        DateTime currentEnd = currentStart.AddMonths(2);
                        if (currentEnd > bitis) currentEnd = bitis;
                        string t1 = currentStart.ToString("dd/MM/yyyy");
                        string t2 = currentEnd.ToString("dd/MM/yyyy");

                        string js = $@"(function(){{
document.getElementById('ddlArac').value = {ToJsonString(arac.Value)};
document.getElementById('Us_tarih1_txtTarihGiris').value = {ToJsonString(t1)};
document.getElementById('Us_tarih2_txtTarihGiris').value = {ToJsonString(t2)};
var b = document.getElementById('btnListeleGrid'); if(b) b.click();
return true; }})();";
                        await webView1.ExecuteScriptAsync(js);

                        pf.SetText($"Veri cekiliyor... {t1} - {t2}");
                        await Task.Delay(3500);
                        await VeriCekWebViewAsync();

                        current++;
                        pf.SetProgress((int)(current * 100.0 / Math.Max(1, toplamAdim)));
                        currentStart = currentEnd;
                    }
                }

                if (_kayitlar.Count == 0)
                {
                    MessageBox.Show("Kayit bulunamadi.");
                    return;
                }

                SimilatorCsvYaz();
                pf.SetText("Tamamlandi");
                pf.SetProgress(100);
                if (MessageBox.Show("Veriler cekildi. PDF + ZIP olusturulsun mu?", "Onay", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    await ZipOlusturAsync(pf);
            }
            finally
            {
                await Task.Delay(250);
                pf.Close();
            }
        }

        private async Task<List<TakvimDers>> TeorikDersleriCek()
        {
            string script = @"(function(){
function temiz(x){ return (x||'').replace(/\s+/g,' ').trim(); }
var data=[]; var table=document.getElementById('dgDonemler'); if(!table) return JSON.stringify([]);
var rows=table.getElementsByTagName('tr');
for(var i=0;i<rows.length;i++){ var c=rows[i].cells; if(!c||c.length<10) continue;
var col0=temiz(c[0].innerText), col1=temiz(c[1].innerText), col3=temiz(c[3].innerText), col4=temiz(c[4].innerText), col5=temiz(c[5].innerText), col6=temiz(c[6].innerText), col7=temiz(c[7].innerText), col8=temiz(c[8].innerText), col9=temiz(c[9].innerText);
if(col0==='Dönemi'||!col0||!col6||!col7||!col8||col7.indexOf('-')===-1) continue;
data.push({ Donem:col0, GrupAdi:col1, Subesi:col3, DersTuru:col4, DerslikAdi:col5, DersTarihi:col6, DersSaati:col7, DersiVeren:col8, EgitimTuru:col9, AdayAdSoyad:'', AracPlakasi:''});
} return JSON.stringify(data); })();";
            var raw = await webView1.ExecuteScriptAsync(script);
            return JsonSerializer.Deserialize<List<TakvimDers>>(Decode(raw)) ?? new List<TakvimDers>();
        }

        private async Task<List<TakvimDers>> UygulamaDersleriCek()
        {
            string script = @"(function(){
function temiz(x){ return (x||'').replace(/\s+/g,' ').trim(); }
var data=[]; var tables=document.getElementsByTagName('table'); var target=null;
for(var t=0;t<tables.length;t++){ var txt=temiz(tables[t].innerText); if(txt.indexOf('Dönemi')>-1 && txt.indexOf('Aday Ad Soyad')>-1){ target=tables[t]; break; } }
if(!target) return JSON.stringify([]);
var rows=target.getElementsByTagName('tr');
for(var i=0;i<rows.length;i++){ var c=rows[i].cells; if(!c||c.length<11) continue;
var col0=temiz(c[0].innerText), col1=temiz(c[1].innerText), col3=temiz(c[3].innerText), col4=temiz(c[4].innerText), col5=temiz(c[5].innerText), col6=temiz(c[6].innerText), col7=temiz(c[7].innerText), col8=temiz(c[8].innerText), col9=temiz(c[9].innerText), col10=temiz(c[10].innerText);
if(col0==='Dönemi'||!col0||!col7||!col8||!col9||col8.indexOf('-')===-1) continue;
data.push({ Donem:col0, GrupAdi:col1, Subesi:col3, DersTuru:'Uygulama', DerslikAdi:col6, DersTarihi:col7, DersSaati:col8, DersiVeren:col9, EgitimTuru:col10, AdayAdSoyad:col4, AracPlakasi:col5});
} return JSON.stringify(data); })();";
            var raw = await webView1.ExecuteScriptAsync(script);
            return JsonSerializer.Deserialize<List<TakvimDers>>(Decode(raw)) ?? new List<TakvimDers>();
        }

        private void HocalariDoldur()
        {
            _comboHazir = false;
            var secili = comboHocalar.SelectedItem?.ToString();
            var hocalar = System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.OrderBy(
                    System.Linq.Enumerable.Distinct(
                        System.Linq.Enumerable.Where(
                            System.Linq.Enumerable.Select(_dersler, x => PersonelTemizle(x.DersiVeren)),
                            x => !string.IsNullOrWhiteSpace(x))),
                    x => x));
            comboHocalar.DataSource = null;
            comboHocalar.Items.Clear();
            if (System.Linq.Enumerable.Any(hocalar))
            {
                comboHocalar.DataSource = hocalar;
                if (!string.IsNullOrWhiteSpace(secili) && System.Linq.Enumerable.Contains(hocalar, secili)) comboHocalar.SelectedItem = secili;
            }
            _comboHazir = true;
        }

        private void btnTakvimAc_Click(object sender, EventArgs e)
        {
            if (comboHocalar.SelectedItem == null) { MessageBox.Show("Lutfen hoca secin."); return; }
            string secilen = comboHocalar.SelectedItem.ToString().Trim();
            var filtre = System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.OrderBy(
                    System.Linq.Enumerable.Where(_dersler, x => !string.IsNullOrWhiteSpace(x.DersiVeren) && PersonelTemizle(x.DersiVeren).Equals(secilen, StringComparison.OrdinalIgnoreCase)),
                    x => x.Baslangic));
            if (!System.Linq.Enumerable.Any(filtre)) { MessageBox.Show("Secilen hocaya ait ders yok."); return; }
            var ilkAy = System.Linq.Enumerable.Min(filtre, x => x.Baslangic);
            new AylikTakvimForm(filtre, new DateTime(ilkAy.Year, ilkAy.Month, 1)).Show(this);
        }

        private string Decode(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw) || raw == "null" || raw == "\"\"") return "";
            try { return JsonSerializer.Deserialize<string>(raw) ?? ""; }
            catch { return raw; }
        }

        private string ToJsonString(string value)
        {
            return JsonSerializer.Serialize(value ?? string.Empty);
        }

        private string PersonelTemizle(string ad)
        {
            if (string.IsNullOrWhiteSpace(ad)) return null;
            ad = Regex.Replace(ad.Replace('\u00A0', ' ').Replace("\r", " ").Replace("\n", " "), @"\s+", " ").Trim();
            if (ad.Length < 3) return null;
            return ad.ToUpper(new System.Globalization.CultureInfo("tr-TR"));
        }

        private async Task<List<AracItem>> SimAraclariniGetir()
        {
            string script = @"(function(){
var ddl = document.getElementById('ddlArac');
if(!ddl) return JSON.stringify([]);
var opts = Array.prototype.slice.call(ddl.options || []);
return JSON.stringify(opts.map(function(o){ return { Text: (o.innerText||'').trim(), Value: o.value||'' }; }));
})();";
            var raw = await webView1.ExecuteScriptAsync(script);
            var list = JsonSerializer.Deserialize<List<AracItem>>(Decode(raw)) ?? new List<AracItem>();
            return System.Linq.Enumerable.ToList(
                System.Linq.Enumerable.Where(list, a => !string.IsNullOrWhiteSpace(a.Text) && a.Text.IndexOf("sim", StringComparison.OrdinalIgnoreCase) >= 0));
        }

        private int AdimSayisiHesapla(DateTime baslangic, DateTime bitis)
        {
            int adim = 0;
            DateTime d = baslangic;
            while (d < bitis) { adim++; d = d.AddMonths(2); }
            return adim;
        }

        private async Task VeriCekWebViewAsync()
        {
            string script = @"(function(){
function t(x){ return (x||'').replace(/\s+/g,' ').trim(); }
var rows = document.querySelectorAll('#dgDonemler tr');
var data=[];
for(var i=0;i<rows.length;i++){
  var c = rows[i].querySelectorAll('td');
  if(!c || c.length < 11) continue;
  if(t(c[6].innerText).toLowerCase().indexOf('sim') === -1) continue;
  data.push({
    Donem:t(c[0].innerText), GrupAdi:t(c[1].innerText), BaslamaTarihi:t(c[2].innerText), Subesi:t(c[3].innerText),
    Aday:t(c[4].innerText), Plaka:t(c[5].innerText), DersYeri:t(c[6].innerText), DersTarihi:t(c[7].innerText),
    DersSaati:t(c[8].innerText), DersiVeren:t(c[9].innerText), EgitimTuru:t(c[10].innerText)
  });
}
return JSON.stringify(data); })();";
            var raw = await webView1.ExecuteScriptAsync(script);
            var liste = JsonSerializer.Deserialize<List<SimilatorKayit>>(Decode(raw)) ?? new List<SimilatorKayit>();
            foreach (var k in liste)
            {
                if (k == null || string.IsNullOrWhiteSpace(k.Aday) || string.IsNullOrWhiteSpace(k.DersTarihi)) continue;
                if (!System.Linq.Enumerable.Any(_kayitlar, x => x.Aday == k.Aday && x.DersTarihi == k.DersTarihi && x.DersSaati == k.DersSaati))
                    _kayitlar.Add(k);
            }
        }

        private void SimilatorCsvYaz()
        {
            string path = Path.Combine(Application.StartupPath, "similator.csv");
            var sb = new StringBuilder();
            sb.AppendLine("AdayAdSoyad,Donem,DersTarihi,DersiVeren,AracPlakasi");
            foreach (var k in _kayitlar)
                sb.AppendLine($"{Csv(k.Aday)},{Csv(k.Donem)},{Csv(k.DersTarihi)},{Csv(k.DersiVeren)},{Csv(k.Plaka)}");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        private async Task ZipOlusturAsync(ProgressForm pf)
        {
            if (_kayitlar.Count == 0) { MessageBox.Show("Veri yok."); return; }
            string pdfRoot = Path.Combine(Application.StartupPath, "PDF");
            Directory.CreateDirectory(pdfRoot);
            string sablon = Path.Combine(pdfRoot, "Ana_Sablon.pdf");
            string ek4Template = Path.Combine(pdfRoot, "ek4_sablon.pdf");
            string ek3Template = Path.Combine(pdfRoot, "ek-3-a2.pdf");
            if (!File.Exists(sablon) || !File.Exists(ek4Template) || !File.Exists(ek3Template))
            {
                MessageBox.Show("PDF sablonlari eksik. 'PDF' klasorune Ana_Sablon.pdf, ek4_sablon.pdf, ek-3-a2.pdf dosyalarini atiniz.");
                return;
            }

            string hedefRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "SimulatorZip");
            Directory.CreateDirectory(hedefRoot);
            string kursText = $"OZEL {txtKursAdi.Text.Trim()} MOTORLU TASIT SURUCULERI KURSU MUDURLUGU";

            var gruplu = System.Linq.Enumerable.ToDictionary(
                System.Linq.Enumerable.GroupBy(
                    System.Linq.Enumerable.Where(_kayitlar, k => DateTime.TryParse(k.DersTarihi, out _)),
                    k => DateTime.Parse(k.DersTarihi).Year.ToString()),
                yg => yg.Key,
                yg => System.Linq.Enumerable.ToDictionary(
                    System.Linq.Enumerable.GroupBy(yg, k => string.IsNullOrWhiteSpace(k.Donem) ? "Bilinmeyen" : k.Donem.Trim()),
                    dg => dg.Key,
                    dg => System.Linq.Enumerable.ToList(dg)));

            int toplam = System.Linq.Enumerable.Sum(gruplu, y => System.Linq.Enumerable.Sum(y.Value, d => d.Value.Count));
            await Task.Run(() =>
            {
                int done = 0;
                BaseFont bf = BaseFont.CreateFont(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"),
                    BaseFont.IDENTITY_H,
                    BaseFont.EMBEDDED);

                foreach (var yil in gruplu)
                {
                    string yilFolder = Path.Combine(hedefRoot, yil.Key);
                    Directory.CreateDirectory(yilFolder);
                    foreach (var donem in yil.Value)
                    {
                        string tempFolder = Path.Combine(Path.GetTempPath(), "Sim_" + Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(tempFolder);
                        foreach (var k in donem.Value)
                        {
                            string safeAd = TemizDosyaAdi(k.Aday);
                            string adayFolder = Path.Combine(tempFolder, safeAd);
                            Directory.CreateDirectory(adayFolder);
                            SimPdfOlustur(adayFolder, k, kursText, sablon, ek4Template, ek3Template, bf);
                            done++;
                            UpdateProgress(pf, $"PDF olusturuluyor... {k.Aday}", (int)(done * 100.0 / Math.Max(1, toplam)));
                        }
                        string zipPath = Path.Combine(yilFolder, TemizDosyaAdi(donem.Key) + ".zip");
                        if (File.Exists(zipPath)) File.Delete(zipPath);
                        System.IO.Compression.ZipFile.CreateFromDirectory(tempFolder, zipPath);
                        Directory.Delete(tempFolder, true);
                    }
                }
            });
            UpdateProgress(pf, "Tamamlandi", 100);
            MessageBox.Show("PDF ve ZIP dosyalari Masaustu/SimulatorZip klasorune olusturuldu.");
        }

        private void SimPdfOlustur(string adayFolder, SimilatorKayit k, string kursText, string sablon, string ek4Template, string ek3Template, BaseFont bf)
        {
            string[] simulasyonlar =
            {
                "algi-ve-refleks-simulasyonu","degisik-hava-kosullari-simulasyonu","direksiyon-egitim-alani-simulasyonu",
                "gece-gunduz-sisli-hava-simulasyonu","inis-cikis-egimli-yol-simulasyonu","park-egitimi-simulasyonu",
                "sehir-ici-yol-simulasyonu","sehirler-arasi-yol-simulasyonu","trafik-isaretleri-simulasyonu",
                "trafik-ortami-simulasyonu","virajli-yolda-surus-simulasyonu","acil-durum-simulasyonu"
            };
            string tarih = DateTime.TryParse(k.DersTarihi, out DateTime dt) ? dt.ToString("dd.MM.yyyy") : k.DersTarihi;
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            foreach (string sim in simulasyonlar)
            {
                string pdfPath = Path.Combine(adayFolder, sim + ".pdf");
                using (var reader = new PdfReader(sablon))
                using (var fs = new FileStream(pdfPath, FileMode.Create))
                using (var stamper = new PdfStamper(reader, fs))
                {
                    PdfContentByte cb = stamper.GetOverContent(1);
                    cb.BeginText();
                    cb.SetFontAndSize(bf, 10);
                    cb.ShowTextAligned(Element.ALIGN_CENTER, kursText, 300, 775, 0);
                    cb.ShowTextAligned(Element.ALIGN_LEFT, k.Aday, 120, 740, 0);
                    cb.ShowTextAligned(Element.ALIGN_LEFT, sim, 120, 700, 0);
                    cb.ShowTextAligned(Element.ALIGN_LEFT, k.Donem, 380, 740, 0);
                    cb.ShowTextAligned(Element.ALIGN_LEFT, tarih, 120, 720, 0);
                    cb.ShowTextAligned(Element.ALIGN_LEFT, rnd.Next(70, 100).ToString(CultureInfo.InvariantCulture), 380, 720, 0);
                    cb.ShowTextAligned(Element.ALIGN_LEFT, k.Plaka, 120, 665, 0);
                    cb.ShowTextAligned(Element.ALIGN_LEFT, k.DersiVeren, 140, 310, 0);
                    cb.EndText();
                }
            }
            KopyaPdfBas(ek4Template, Path.Combine(adayFolder, "Similator_Sinav_Raporu(B).pdf"), k, kursText, tarih, bf, 725, 755);
            KopyaPdfBas(ek3Template, Path.Combine(adayFolder, "Similator_Sinav_Raporu(a2).pdf"), k, kursText, tarih, bf, 750, 785);
        }

        private void UpdateProgress(ProgressForm pf, string text, int? progress)
        {
            if (pf == null || pf.IsDisposed) return;
            if (pf.InvokeRequired)
            {
                pf.BeginInvoke(new Action(() => UpdateProgress(pf, text, progress)));
                return;
            }

            if (!string.IsNullOrWhiteSpace(text))
                pf.SetText(text);
            if (progress.HasValue)
                pf.SetProgress(progress.Value);
        }

        private void KopyaPdfBas(string template, string hedef, SimilatorKayit k, string kursText, string tarih, BaseFont bf, float ySatir, float yKurs)
        {
            using (var r = new PdfReader(template))
            using (var fs = new FileStream(hedef, FileMode.Create))
            using (var s = new PdfStamper(r, fs))
            {
                var cb = s.GetOverContent(1);
                cb.BeginText();
                cb.SetFontAndSize(bf, 10);
                cb.ShowTextAligned(Element.ALIGN_LEFT, k.Aday, 70, ySatir, 0);
                cb.ShowTextAligned(Element.ALIGN_CENTER, kursText, 300, yKurs, 0);
                cb.ShowTextAligned(Element.ALIGN_LEFT, k.Plaka, 360, ySatir, 0);
                cb.ShowTextAligned(Element.ALIGN_LEFT, k.DersiVeren, 470, ySatir, 0);
                cb.EndText();
                if (r.NumberOfPages > 1)
                {
                    var cb2 = s.GetOverContent(2);
                    cb2.BeginText();
                    cb2.SetFontAndSize(bf, 10);
                    cb2.ShowTextAligned(Element.ALIGN_RIGHT, tarih, 100, 70, 0);
                    cb2.EndText();
                }
            }
        }

        private string Csv(string text)
        {
            if (string.IsNullOrEmpty(text)) return "\"\"";
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private string TemizDosyaAdi(string ad)
        {
            if (string.IsNullOrWhiteSpace(ad)) return "Bilinmeyen";
            foreach (char c in Path.GetInvalidFileNameChars()) ad = ad.Replace(c, '_');
            return ad.Trim();
        }

        public class SimilatorKayit
        {
            public string Donem { get; set; }
            public string GrupAdi { get; set; }
            public string BaslamaTarihi { get; set; }
            public string Subesi { get; set; }
            public string Aday { get; set; }
            public string Plaka { get; set; }
            public string DersYeri { get; set; }
            public string DersTarihi { get; set; }
            public string DersSaati { get; set; }
            public string DersiVeren { get; set; }
            public string EgitimTuru { get; set; }
        }

        public class AracItem
        {
            public string Text { get; set; }
            public string Value { get; set; }
        }
    }
}
