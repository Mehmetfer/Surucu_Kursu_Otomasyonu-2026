using Kolera_Mtsk.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class Db_olsturvekopyala : Form
    {
        public string SunucuAdi { get; set; }
        public string KullaniciAdi { get; set; }
        public string Parola { get; set; }
        public string KaynakVeritabaniAdi { get; set; }

        public Db_olsturvekopyala()
        {
            InitializeComponent();
            Load += Db_olsturvekopyala_Load;
            Btn_Veritabani_Olustur.Click += Btn_Veritabani_Olustur_Click;
            Btn_Kopyala_Baslat.Click += Btn_Kopyala_Baslat_Click;
            Btn_Test100.Click += Btn_Test100_Click;
            Btn_Test20.Click += Btn_Test20_Click;
            Btn_Demo.Click += Btn_Demo_Click;
        }

        private void Db_olsturvekopyala_Load(object sender, EventArgs e)
        {
            FormWorkspaceLayoutHelper.ApplyWorkingAreaMaximized(this);
            KSunucuAdresiBox.Text = SunucuAdi ?? string.Empty;
            Txt_datam1.Text = KaynakVeritabaniAdi ?? string.Empty;
            KSunucuAdresiBox.ReadOnly = true;
            Txt_datam1.ReadOnly = true;
            Lbl_Durumu.Text = "Hazır";
            Yukleme_Durumu.Value = 0;
            Grp_Kopya.Visible = false;
            listBox1.Items.Clear();
            listBox1.Items.Add("Sunucu: " + KSunucuAdresiBox.Text);
            listBox1.Items.Add("Bağlı Veritabanı: " + Txt_datam1.Text);
        }

        private async void Btn_Veritabani_Olustur_Click(object sender, EventArgs e)
        {
            string dbAdi = (DataAdi.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(dbAdi))
            {
                MessageBox.Show("Yeni veritabanı adı (DataAdi) boş olamaz.");
                return;
            }

            try
            {
                Btn_Veritabani_Olustur.Enabled = false;
                Yukleme_Durumu.Value = 0;
                Lbl_Durumu.Text = "Oluşturuluyor...";
                Lbl_Durumu.ForeColor = System.Drawing.Color.DarkOrange;
                listBox1.Items.Add("Kurulum başladı: " + dbAdi);

                var installer = new EmptyDatabaseInstaller(BuildMasterConnectionString(), dbAdi, Log);
                installer.ProgressChanged += (done, total) =>
                {
                    int pct = total > 0 ? (int)Math.Round(done * 100.0 / total) : 0;
                    Ui(() => Yukleme_Durumu.Value = Math.Max(0, Math.Min(100, pct)));
                };

                await installer.InstallAsync();

                Lbl_Durumu.Text = "Oluşturuldu";
                Lbl_Durumu.ForeColor = System.Drawing.Color.Green;
                Txt_datam1.Text = dbAdi;
                listBox1.Items.Add("Tamamlandı: " + dbAdi);
                await YukleDbListeleriAsync(dbAdi);
                Grp_Kopya.Visible = true;
                MessageBox.Show("Veritabanı başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                Lbl_Durumu.Text = "Hata";
                Lbl_Durumu.ForeColor = System.Drawing.Color.Red;
                listBox1.Items.Add("Hata: " + ex.Message);
                MessageBox.Show(ex.Message, "Veritabanı Oluştur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Btn_Veritabani_Olustur.Enabled = true;
            }
        }

        private async Task YukleDbListeleriAsync(string varsayilanHedefDb)
        {
            var dbler = await Task.Run(GetDatabaseList);
            Ui(() =>
            {
                cmbKaynakDb.Items.Clear();
                cmbHedefDb.Items.Clear();
                foreach (var db in dbler)
                {
                    cmbKaynakDb.Items.Add(db);
                    cmbHedefDb.Items.Add(db);
                }

                // Kaynak seçimini kullanıcı yapsın, otomatik seçim yok.
                cmbKaynakDb.SelectedIndex = -1;

                if (cmbHedefDb.Items.Count > 0)
                    cmbHedefDb.SelectedItem = varsayilanHedefDb;
                if (cmbHedefDb.SelectedItem == null && cmbHedefDb.Items.Count > 0)
                    cmbHedefDb.SelectedIndex = 0;
            });
        }

        private async void Btn_Kopyala_Baslat_Click(object sender, EventArgs e)
        {
            if (cmbKaynakDb.SelectedItem == null || cmbHedefDb.SelectedItem == null)
            {
                MessageBox.Show("Kaynak ve hedef veritabanı seçiniz.");
                return;
            }

            string kaynakDb = cmbKaynakDb.SelectedItem.ToString();
            string hedefDb = cmbHedefDb.SelectedItem.ToString();
            if (string.Equals(kaynakDb, hedefDb, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Kaynak ve hedef veritabanı aynı olamaz.");
                return;
            }

            try
            {
                LisansPolitikasi.BeginSuppressWebLicenseCalls();
                Btn_Kopyala_Baslat.Enabled = false;
                Btn_Test100.Enabled = false;
                Btn_Test20.Enabled = false;
                listBox1.Items.Clear();
                listBox1.Dock = DockStyle.Fill;
                listBox1.BringToFront();

                Kopya_ilerle_durum.Value = 0;
                lblKopyaYuzde.Text = "%0";
                lblKopyalananTablo.Text = "Kopyalanan: (başlıyor)";
                Lbl_Durumu.Text = "Kopyalanıyor...";
                Lbl_Durumu.ForeColor = System.Drawing.Color.DarkOrange;
                Log("Kopyalama başlıyor: " + kaynakDb + " -> " + hedefDb);

                await CopyDataAsync(kaynakDb, hedefDb);

                Lbl_Durumu.Text = "Kopyalama tamam";
                Lbl_Durumu.ForeColor = System.Drawing.Color.Green;
                lblKopyalananTablo.Text = "Kopyalanan: tamamlandı";
                Log("Kopyalama işlemi tamamlandı.");
                MessageBox.Show("Kopyalama tamamlandı.");
            }
            catch (Exception ex)
            {
                Lbl_Durumu.Text = "Kopyalama hatası";
                Lbl_Durumu.ForeColor = System.Drawing.Color.Red;
                lblKopyalananTablo.Text = "Kopyalanan: hata";
                Log("Hata: " + ex.Message);
                MessageBox.Show(ex.Message, "Kopyalama", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                LisansPolitikasi.EndSuppressWebLicenseCalls();
                Btn_Kopyala_Baslat.Enabled = true;
                Btn_Test100.Enabled = true;
                Btn_Test20.Enabled = true;
            }
        }

        private async void Btn_Test100_Click(object sender, EventArgs e)
        {
            if (cmbKaynakDb.SelectedItem == null || cmbHedefDb.SelectedItem == null)
            {
                MessageBox.Show("Kaynak ve hedef veritabanı seçiniz.");
                return;
            }

            string kaynakDb = cmbKaynakDb.SelectedItem.ToString();
            string hedefDb = cmbHedefDb.SelectedItem.ToString();
            if (string.Equals(kaynakDb, hedefDb, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Kaynak ve hedef veritabanı aynı olamaz.");
                return;
            }

            try
            {
                LisansPolitikasi.BeginSuppressWebLicenseCalls();
                Btn_Test100.Enabled = false;
                Btn_Test20.Enabled = false;
                Btn_Kopyala_Baslat.Enabled = false;
                listBox1.Items.Clear();
                listBox1.Dock = DockStyle.Fill;
                listBox1.BringToFront();

                Kopya_ilerle_durum.Value = 0;
                lblKopyaYuzde.Text = "%0";
                lblKopyalananTablo.Text = "Kopyalanan: TEST 300";
                Log("TEST 300 başladı: " + kaynakDb + " -> " + hedefDb);

                await CopyDataAsync(kaynakDb, hedefDb, 300);

                lblKopyalananTablo.Text = "Kopyalanan: TEST 300 tamam";
                Log("TEST 300 tamamlandı.");
                MessageBox.Show("TEST 300 kopyalama tamamlandı.");
            }
            catch (Exception ex)
            {
                lblKopyalananTablo.Text = "Kopyalanan: hata";
                Log("Hata: " + ex.Message);
                MessageBox.Show(ex.Message, "TEST 300", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                LisansPolitikasi.EndSuppressWebLicenseCalls();
                Btn_Test100.Enabled = true;
                Btn_Test20.Enabled = true;
                Btn_Kopyala_Baslat.Enabled = true;
            }
        }

        private async void Btn_Test20_Click(object sender, EventArgs e)
        {
            if (cmbKaynakDb.SelectedItem == null || cmbHedefDb.SelectedItem == null)
            {
                MessageBox.Show("Kaynak ve hedef veritabanı seçiniz.");
                return;
            }

            string kaynakDb = cmbKaynakDb.SelectedItem.ToString();
            string hedefDb = cmbHedefDb.SelectedItem.ToString();
            if (string.Equals(kaynakDb, hedefDb, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Kaynak ve hedef veritabanı aynı olamaz.");
                return;
            }

            if (MessageBox.Show(
                    "Her tabloda son 20 kayıt alınacak; identity sütunu olan tablolarda yeni ID'ler 1..N olacak ve "
                    + "foreign key değerleri bu eşlemeye göre güncellenecek.\r\n\r\n"
                    + "Üst tabloda seçilen 20 satırda olmayan bir anahtara bağlı satırlar, sütun nullable ise NULL yapılır.\r\n\r\n"
                    + "Devam edilsin mi?",
                    "TEST 20",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                LisansPolitikasi.BeginSuppressWebLicenseCalls();
                Btn_Test100.Enabled = false;
                Btn_Test20.Enabled = false;
                Btn_Kopyala_Baslat.Enabled = false;
                listBox1.Items.Clear();
                listBox1.Dock = DockStyle.Fill;
                listBox1.BringToFront();

                Kopya_ilerle_durum.Value = 0;
                lblKopyaYuzde.Text = "%0";
                lblKopyalananTablo.Text = "Kopyalanan: TEST 20";
                Log("TEST 20 (ID yeniden 1..N) başladı: " + kaynakDb + " -> " + hedefDb);

                await CopyDataAsync(kaynakDb, hedefDb, 20, resequenceIdentityFromOne: true);

                lblKopyalananTablo.Text = "Kopyalanan: TEST 20 tamam";
                Log("TEST 20 tamamlandı.");
                MessageBox.Show("TEST 20 kopyalama tamamlandı.");
            }
            catch (Exception ex)
            {
                lblKopyalananTablo.Text = "Kopyalanan: hata";
                Log("Hata: " + ex.Message);
                MessageBox.Show(ex.Message, "TEST 20", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                LisansPolitikasi.EndSuppressWebLicenseCalls();
                Btn_Test100.Enabled = true;
                Btn_Test20.Enabled = true;
                Btn_Kopyala_Baslat.Enabled = true;
            }
        }

        private void Btn_Demo_Click(object sender, EventArgs e)
        {
            using (var frm = new DbKurulumKopyalamaForm())
            {
                frm.SunucuAdi = SunucuAdi ?? KSunucuAdresiBox.Text;
                frm.KullaniciAdi = KullaniciAdi;
                frm.Parola = Parola;
                frm.BaglantiTuru = string.IsNullOrWhiteSpace(KullaniciAdi) ? "Windows" : "SQL";
                frm.HedefVeritabani = (DataAdi.Text ?? string.Empty).Trim();
                frm.ShowDialog(this);
            }
        }

        private string BuildMasterConnectionString()
        {
            string server = (KSunucuAdresiBox.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(server))
                throw new InvalidOperationException("Sunucu adresi boş olamaz.");

            string windowsConn = "Server=" + server + ";Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
            try
            {
                using (var con = new SqlConnection(windowsConn))
                {
                    con.Open();
                    return windowsConn;
                }
            }
            catch
            {
                return "Server=" + server + ";Database=master;User Id=" + (KullaniciAdi ?? string.Empty).Trim() + ";Password=" + (Parola ?? string.Empty) + ";TrustServerCertificate=True;Encrypt=False;";
            }
        }

        private string BuildDbConnectionString(string dbName)
        {
            var sb = new SqlConnectionStringBuilder(BuildMasterConnectionString())
            {
                InitialCatalog = dbName
            };
            return sb.ConnectionString;
        }

        private List<string> GetDatabaseList()
        {
            var list = new List<string>();
            using (var con = new SqlConnection(BuildMasterConnectionString()))
            using (var cmd = new SqlCommand("SELECT name FROM sys.databases ORDER BY name", con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        list.Add(dr.GetString(0));
                }
            }
            return list;
        }

        private async Task CopyDataAsync(string kaynakDb, string hedefDb, int topLimit = 0, bool resequenceIdentityFromOne = false)
        {
            using (var kaynakCon = new SqlConnection(BuildDbConnectionString(kaynakDb)))
            using (var hedefCon = new SqlConnection(BuildDbConnectionString(hedefDb)))
            {
                await kaynakCon.OpenAsync();
                await hedefCon.OpenAsync();

                var kaynakTablolar = await GetTableNamesAsync(kaynakCon);
                var hedefTablolar = await GetTableNamesAsync(hedefCon);
                var kopyaEslesmeleri = BuildCopyPairs(kaynakTablolar, hedefTablolar);

                if (kopyaEslesmeleri.Count == 0)
                    throw new InvalidOperationException("Kaynak ve hedefte ortak tablo bulunamadı.");

                if (topLimit > 0 && resequenceIdentityFromOne)
                {
                    await CopyDatabaseSubsetResequenceIdentitiesAsync(
                        kaynakCon, hedefCon, kopyaEslesmeleri, topLimit);
                    await ApplyLegacyParameterSyncAsync(kaynakCon, hedefCon);
                    await ApplyFinalSqlFixupAsync(kaynakDb, hedefCon);
                    await ApplyKullaniciPostCopyAsync(hedefCon, hedefDb);
                    await LisansIlkKurulumDefaults.ApplyAsync(hedefCon);
                    return;
                }

                int toplam = kopyaEslesmeleri.Count;
                int done = 0;

                foreach (var eslesme in kopyaEslesmeleri)
                {
                    string kaynakTablo = eslesme.SourceTable;
                    string hedefTablo = eslesme.TargetTable;
                    Log("Kopyalaniyor: " + kaynakTablo + " -> " + hedefTablo);
                    var kaynakKolonlar = await GetColumnInfosAsync(kaynakCon, kaynakTablo);
                    var hedefKolonlar = await GetColumnInfosAsync(hedefCon, hedefTablo);
                    await EnsureMissingColumnsAsync(hedefCon, kaynakTablo, hedefTablo, kaynakKolonlar, hedefKolonlar);
                    hedefKolonlar = await GetColumnInfosAsync(hedefCon, hedefTablo);
                    var kopyaKolonlari = BuildColumnCopyMaps(kaynakTablo + "->" + hedefTablo, kaynakKolonlar, hedefKolonlar);
                    if (kopyaKolonlari.Count == 0)
                    {
                        Log("Atlandı: " + kaynakTablo + " -> " + hedefTablo + " (ortak kolon yok)");
                        done++;
                        UpdateProgress(done, toplam, hedefTablo);
                        continue;
                    }

                    string kolons = string.Join(",", kopyaKolonlari.Select(c => c.SourceSelectSql + " AS [" + c.TargetColumn + "]"));
                    using (var temizle = new SqlCommand("DELETE FROM [" + hedefTablo + "]", hedefCon))
                    {
                        temizle.CommandTimeout = 0;
                        await temizle.ExecuteNonQueryAsync();
                    }

                    bool aracTipFiltrele =
                        hedefTablo.Equals("AracParam", StringComparison.OrdinalIgnoreCase)
                        && kaynakKolonlar.ContainsKey("ARAC_TIPI");
                    string whereAracTip = aracTipFiltrele
                        ? " WHERE ISNULL(LTRIM(RTRIM(ARAC_TIPI)),'') <> ''"
                        : string.Empty;

                    string selectSql;
                    if (topLimit > 0)
                    {
                        if (kaynakKolonlar.ContainsKey("ID"))
                            selectSql = "SELECT TOP (" + topLimit + ") " + kolons + " FROM [" + kaynakTablo + "]" + whereAracTip + " ORDER BY [ID] DESC";
                        else
                            selectSql = "SELECT TOP (" + topLimit + ") " + kolons + " FROM [" + kaynakTablo + "]" + whereAracTip + " ORDER BY 1 DESC";
                    }
                    else
                    {
                        selectSql = "SELECT " + kolons + " FROM [" + kaynakTablo + "]" + whereAracTip;
                    }

                    using (var srcCmd = new SqlCommand(selectSql, kaynakCon))
                    using (var bulk = new SqlBulkCopy(hedefCon, SqlBulkCopyOptions.KeepIdentity, null))
                    {
                        srcCmd.CommandTimeout = 0;
                        using (var reader = await srcCmd.ExecuteReaderAsync())
                        {
                        bulk.DestinationTableName = "[" + hedefTablo + "]";
                        bulk.BulkCopyTimeout = 0;
                        bulk.BatchSize = 5000;
                        foreach (var col in kopyaKolonlari)
                            bulk.ColumnMappings.Add(col.TargetColumn, col.TargetColumn);
                        await bulk.WriteToServerAsync(reader);
                        }
                    }

                    Log("Kopyalandı: " + kaynakTablo + " -> " + hedefTablo);
                    done++;
                    UpdateProgress(done, toplam, hedefTablo);
                }

                await SyncKursiyerOdemeColumnsAsync(kaynakCon, hedefCon, kopyaEslesmeleri);
                await ApplyKullaniciPostCopyAsync(hedefCon, hedefDb);

                // Bire bir kopya modu: kaynak veriyi sonradan degistiren post-process adimlari calistirilmaz.
                Log("Bire bir kopya modu: post-process adimlari atlandi.");
            }
        }

        private async Task SyncKursiyerOdemeColumnsAsync(
            SqlConnection kaynakCon,
            SqlConnection hedefCon,
            List<TableCopyPair> kopyaEslesmeleri)
        {
            if (kaynakCon == null || hedefCon == null || kopyaEslesmeleri == null || kopyaEslesmeleri.Count == 0)
                return;

            var pair = kopyaEslesmeleri.FirstOrDefault(p =>
                string.Equals(p.TargetTable, "KURSIYER", StringComparison.OrdinalIgnoreCase));
            if (pair == null)
                return;

            var srcCols = await GetColumnInfosAsync(kaynakCon, pair.SourceTable);
            var dstCols = await GetColumnInfosAsync(hedefCon, pair.TargetTable);
            string[] odemeCols = { "KALANBORC", "TOPLAM_BORC", "TOPLAM_ODENEN" };
            var ortak = odemeCols.Where(c => srcCols.ContainsKey(c) && dstCols.ContainsKey(c)).ToList();
            if (ortak.Count == 0 || !srcCols.ContainsKey("ID") || !dstCols.ContainsKey("ID"))
                return;

            string setSql = string.Join(", ", ortak.Select(c => "t.[" + c + "] = s.[" + c + "]"));
            string sql = @"
UPDATE t
SET " + setSql + @"
FROM [" + pair.TargetTable + @"] t
INNER JOIN [" + pair.SourceTable + @"] s ON t.[ID] = s.[ID];";

            using (var cmd = new SqlCommand(sql, hedefCon))
            {
                cmd.CommandTimeout = 0;
                int etkilenen = await cmd.ExecuteNonQueryAsync();
                Log("Kursiyer odeme alanlari senkronlandi: " + etkilenen + " satir (" + string.Join(", ", ortak) + ")");
            }
        }

        private async Task CopyDatabaseSubsetResequenceIdentitiesAsync(
            SqlConnection kaynakCon,
            SqlConnection hedefCon,
            List<TableCopyPair> kopyaEslesmeleri,
            int topLimit)
        {
            var sourceToTarget = kopyaEslesmeleri.ToDictionary(
                p => p.SourceTable,
                p => p.TargetTable,
                StringComparer.OrdinalIgnoreCase);

            var fkList = await LoadFkColumnsMappedAsync(kaynakCon, sourceToTarget);
            var depDup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var depList = new List<Tuple<string, string>>();
            foreach (var fk in fkList)
            {
                if (string.Equals(fk.ReferencedTarget, fk.ReferencingTarget, StringComparison.OrdinalIgnoreCase))
                    continue;
                string key = fk.ReferencedTarget + "\0" + fk.ReferencingTarget;
                if (!depDup.Add(key))
                    continue;
                depList.Add(Tuple.Create(fk.ReferencedTarget, fk.ReferencingTarget));
            }

            var orderedPairs = TopoOrderPairs(kopyaEslesmeleri, depList);
            var idMaps = new Dictionary<string, Dictionary<int, int>>(StringComparer.OrdinalIgnoreCase);
            int toplam = orderedPairs.Count;
            int done = 0;

            foreach (var eslesme in orderedPairs)
            {
                string kaynakTablo = eslesme.SourceTable;
                string hedefTablo = eslesme.TargetTable;
                Log("TEST20 kopyalaniyor: " + kaynakTablo + " -> " + hedefTablo);

                var kaynakKolonlar = await GetColumnInfosAsync(kaynakCon, kaynakTablo);
                var hedefKolonlar = await GetColumnInfosAsync(hedefCon, hedefTablo);
                await EnsureMissingColumnsAsync(hedefCon, kaynakTablo, hedefTablo, kaynakKolonlar, hedefKolonlar);
                hedefKolonlar = await GetColumnInfosAsync(hedefCon, hedefTablo);
                var kopyaKolonlari = BuildColumnCopyMaps(kaynakTablo + "->" + hedefTablo, kaynakKolonlar, hedefKolonlar);
                if (kopyaKolonlari.Count == 0)
                {
                    Log("Atlandı (TEST20): " + kaynakTablo + " -> " + hedefTablo + " (ortak kolon yok)");
                    done++;
                    UpdateProgress(done, toplam, hedefTablo);
                    continue;
                }

                using (var temizle = new SqlCommand("DELETE FROM [" + hedefTablo + "]", hedefCon))
                {
                    temizle.CommandTimeout = 0;
                    await temizle.ExecuteNonQueryAsync();
                }

                bool aracTipFiltrele =
                    hedefTablo.Equals("AracParam", StringComparison.OrdinalIgnoreCase)
                    && kaynakKolonlar.ContainsKey("ARAC_TIPI");
                string whereAracTip = aracTipFiltrele
                    ? " WHERE ISNULL(LTRIM(RTRIM(ARAC_TIPI)),'') <> ''"
                    : string.Empty;

                string kolons = string.Join(",", kopyaKolonlari.Select(c => c.SourceSelectSql + " AS [" + c.TargetColumn + "]"));
                string selectSql;
                if (kaynakKolonlar.ContainsKey("ID"))
                    selectSql = "SELECT TOP (" + topLimit + ") " + kolons + " FROM [" + kaynakTablo + "]" + whereAracTip + " ORDER BY [ID] DESC";
                else
                    selectSql = "SELECT TOP (" + topLimit + ") " + kolons + " FROM [" + kaynakTablo + "]" + whereAracTip + " ORDER BY 1 DESC";

                var dt = new DataTable();
                using (var da = new SqlDataAdapter(selectSql, kaynakCon))
                {
                    da.SelectCommand.CommandTimeout = 0;
                    da.Fill(dt);
                }

                string identityCol = await GetIdentityColumnNameAsync(hedefCon, hedefTablo);
                if (dt.Rows.Count > 0 && !string.IsNullOrEmpty(identityCol) && dt.Columns.Contains(identityCol))
                {
                    DataRow[] sorted = dt.Select("", identityCol + " ASC");
                    var map = new Dictionary<int, int>();
                    int ni = 1;
                    foreach (DataRow row in sorted)
                    {
                        int oldId = Convert.ToInt32(row[identityCol], System.Globalization.CultureInfo.InvariantCulture);
                        map[oldId] = ni;
                        row[identityCol] = ni;
                        ni++;
                    }
                    idMaps[hedefTablo] = map;
                }
                else
                {
                    idMaps[hedefTablo] = new Dictionary<int, int>();
                    if (dt.Rows.Count > 0)
                        Log("UYARI (TEST20): " + hedefTablo + " için identity sütunu yok veya bulunamadı; ID değerleri kaynak ile aynı kalır.");
                }

                if (dt.Rows.Count > 0)
                {
                    var fksForTable = fkList
                        .Where(f => string.Equals(f.ReferencingTarget, hedefTablo, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (var fk in fksForTable)
                        {
                            if (!dt.Columns.Contains(fk.ReferencingColumn))
                                continue;
                            object v = row[fk.ReferencingColumn];
                            if (v == null || v == DBNull.Value)
                                continue;
                            int oldFk;
                            try
                            {
                                oldFk = Convert.ToInt32(v, System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch
                            {
                                continue;
                            }

                            if (!idMaps.TryGetValue(fk.ReferencedTarget, out var refMap) || refMap == null)
                                continue;
                            if (refMap.TryGetValue(oldFk, out int newFk))
                                row[fk.ReferencingColumn] = newFk;
                            else if (hedefKolonlar.TryGetValue(fk.ReferencingColumn, out var ci) && ci.IsNullable)
                                row[fk.ReferencingColumn] = DBNull.Value;
                            else
                                Log("UYARI (TEST20): " + hedefTablo + "." + fk.ReferencingColumn + " FK=" + oldFk + " -> " + fk.ReferencedTarget + " eşlenemedi (NOT NULL).");
                        }
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    done++;
                    UpdateProgress(done, toplam, hedefTablo);
                    continue;
                }

                using (var bulk = new SqlBulkCopy(hedefCon, SqlBulkCopyOptions.KeepIdentity, null))
                {
                    bulk.DestinationTableName = "[" + hedefTablo + "]";
                    bulk.BulkCopyTimeout = 0;
                    bulk.BatchSize = 1000;
                    foreach (DataColumn dc in dt.Columns)
                        bulk.ColumnMappings.Add(dc.ColumnName, dc.ColumnName);
                    await bulk.WriteToServerAsync(dt);
                }

                Log("Kopyalandı (TEST20): " + kaynakTablo + " -> " + hedefTablo + " (" + dt.Rows.Count + " satır)");
                done++;
                UpdateProgress(done, toplam, hedefTablo);
            }
        }

        private async Task<List<FkColumnRef>> LoadFkColumnsMappedAsync(
            SqlConnection con,
            Dictionary<string, string> sourceToTarget)
        {
            const string sql = @"
SELECT OBJECT_NAME(f.parent_object_id) AS ChildTable,
       COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ChildCol,
       OBJECT_NAME(f.referenced_object_id) AS RefTable,
       COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS RefCol
FROM sys.foreign_keys AS f
INNER JOIN sys.foreign_key_columns AS fc ON f.object_id = fc.constraint_object_id
WHERE f.is_disabled = 0";

            var list = new List<FkColumnRef>();
            using (var cmd = new SqlCommand(sql, con))
            using (var r = await cmd.ExecuteReaderAsync())
            {
                while (await r.ReadAsync())
                {
                    string childSrc = r.GetString(0);
                    string childCol = r.GetString(1);
                    string refSrc = r.GetString(2);
                    string refCol = r.GetString(3);
                    if (!sourceToTarget.TryGetValue(childSrc, out string childTgt))
                        continue;
                    if (!sourceToTarget.TryGetValue(refSrc, out string refTgt))
                        continue;
                    list.Add(new FkColumnRef
                    {
                        ReferencingTarget = childTgt,
                        ReferencingColumn = childCol,
                        ReferencedTarget = refTgt,
                        ReferencedColumn = refCol
                    });
                }
            }
            return list;
        }

        private static List<TableCopyPair> TopoOrderPairs(
            List<TableCopyPair> pairs,
            List<Tuple<string, string>> refToChildEdges)
        {
            var byTarget = pairs.ToDictionary(p => p.TargetTable, p => p, StringComparer.OrdinalIgnoreCase);
            var nodes = byTarget.Keys.ToList();
            var indegree = nodes.ToDictionary(n => n, n => 0, StringComparer.OrdinalIgnoreCase);
            var adj = nodes.ToDictionary(n => n, n => new List<string>(), StringComparer.OrdinalIgnoreCase);
            var edgeSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var e in refToChildEdges)
            {
                if (!byTarget.ContainsKey(e.Item1) || !byTarget.ContainsKey(e.Item2))
                    continue;
                if (string.Equals(e.Item1, e.Item2, StringComparison.OrdinalIgnoreCase))
                    continue;
                string ek = e.Item1 + "\0" + e.Item2;
                if (!edgeSeen.Add(ek))
                    continue;
                adj[e.Item1].Add(e.Item2);
                indegree[e.Item2]++;
            }

            var resultNames = new List<string>();
            var q = new Queue<string>(nodes.Where(n => indegree[n] == 0).OrderBy(n => n, StringComparer.OrdinalIgnoreCase));
            while (q.Count > 0)
            {
                string u = q.Dequeue();
                resultNames.Add(u);
                foreach (string v in adj[u])
                {
                    indegree[v]--;
                    if (indegree[v] == 0)
                        q.Enqueue(v);
                }
            }

            foreach (string n in nodes.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                if (!resultNames.Any(r => string.Equals(r, n, StringComparison.OrdinalIgnoreCase)))
                    resultNames.Add(n);
            }

            return resultNames.Select(t => byTarget[t]).ToList();
        }

        private async Task<string> GetIdentityColumnNameAsync(SqlConnection con, string tableName)
        {
            string fullName = "[dbo].[" + (tableName ?? string.Empty).Replace("]", "]]") + "]";
            const string sql = @"
SELECT TOP 1 c.name
FROM sys.columns c
WHERE c.object_id = OBJECT_ID(@full) AND c.is_identity = 1
ORDER BY c.column_id";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@full", fullName);
                object o = await cmd.ExecuteScalarAsync();
                return o == null || o == DBNull.Value ? null : Convert.ToString(o);
            }
        }

        private sealed class FkColumnRef
        {
            public string ReferencingTarget { get; set; }
            public string ReferencingColumn { get; set; }
            public string ReferencedTarget { get; set; }
            public string ReferencedColumn { get; set; }
        }

        private List<TableCopyPair> BuildCopyPairs(List<string> sourceTables, List<string> targetTables)
        {
            var src = new HashSet<string>(sourceTables, StringComparer.OrdinalIgnoreCase);
            var dst = new HashSet<string>(targetTables, StringComparer.OrdinalIgnoreCase);
            var pairs = new List<TableCopyPair>();
            var usedTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var table in sourceTables.OrderBy(t => t))
            {
                if (dst.Contains(table))
                {
                    pairs.Add(new TableCopyPair(table, table));
                    usedTargets.Add(table);
                    continue;
                }

                string mapped = MapTableName(table);
                if (!string.Equals(mapped, table, StringComparison.OrdinalIgnoreCase) && dst.Contains(mapped))
                {
                    pairs.Add(new TableCopyPair(table, mapped));
                    usedTargets.Add(mapped);
                }
            }

            foreach (var target in targetTables.OrderBy(t => t))
            {
                if (usedTargets.Contains(target))
                    continue;

                string legacy = MapTableName(target);
                if (!string.Equals(legacy, target, StringComparison.OrdinalIgnoreCase) && src.Contains(legacy))
                {
                    pairs.Add(new TableCopyPair(legacy, target));
                    usedTargets.Add(target);
                }
            }

            return pairs;
        }

        private string MapTableName(string tableName)
        {
            string mapped = MapToNewTableName(tableName);
            if (!string.Equals(mapped, tableName, StringComparison.OrdinalIgnoreCase))
                return mapped;

            mapped = MapToLegacyTableName(tableName);
            if (!string.Equals(mapped, tableName, StringComparison.OrdinalIgnoreCase))
                return mapped;

            return tableName;
        }

        private string MapToNewTableName(string tableName)
        {
            switch ((tableName ?? string.Empty).ToUpperInvariant())
            {
                case "PARAM_ARAC_TANIMLARI":
                    return "AracParam";
                case "PARAM_KURSBILGILERI":
                    return "KursBilgiParam";
                case "PARAM_SINIFLAR":
                    return "SinifParam";
                case "PARAM_SERTIFIKA_UCRETILANI":
                    return "SertifikaUcretParam";
                case "PARAM_SETTINGS":
                    return "SettingsParam";
                case "PARAM_GENEL_PARAMETRELER":
                    return "GenelParam";
                default:
                    return tableName;
            }
        }

        private string MapToLegacyTableName(string tableName)
        {
            switch ((tableName ?? string.Empty).ToUpperInvariant())
            {
                case "ARACPARAM":
                    return "PARAM_ARAC_TANIMLARI";
                case "KURSBILGIPARAM":
                    return "PARAM_KURSBILGILERI";
                case "SINIFPARAM":
                    return "PARAM_SINIFLAR";
                case "SERTIFIKAUCRETPARAM":
                    return "PARAM_SERTIFIKA_UCRETILANI";
                case "SETTINGSPARAM":
                    return "PARAM_SETTINGS";
                case "GENELPARAM":
                    return "PARAM_GENEL_PARAMETRELER";
                default:
                    return tableName;
            }
        }

        private async Task<bool> TableExistsAsync(SqlConnection con, string tableName)
        {
            const string sql = @"
SELECT TOP 1 1
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME=@t";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@t", tableName);
                return (await cmd.ExecuteScalarAsync()) != null;
            }
        }

        private async Task<int> GetTableRowCountAsync(SqlConnection con, string tableName)
        {
            if (!await TableExistsAsync(con, tableName))
                return 0;
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM [" + tableName + "]", con))
            {
                object o = await cmd.ExecuteScalarAsync();
                return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
            }
        }

        private async Task ForceCopyTableAsync(SqlConnection sourceCon, SqlConnection targetCon, string sourceTableName, string targetTableName)
        {
            var sourceColumns = await GetColumnInfosAsync(sourceCon, sourceTableName);
            var targetColumns = await GetColumnInfosAsync(targetCon, targetTableName);
            await EnsureMissingColumnsAsync(targetCon, sourceTableName, targetTableName, sourceColumns, targetColumns);
            targetColumns = await GetColumnInfosAsync(targetCon, targetTableName);

            var copyMaps = BuildColumnCopyMaps(sourceTableName + "->" + targetTableName, sourceColumns, targetColumns);
            if (copyMaps.Count == 0)
                return;

            string selectCols = string.Join(",", copyMaps.Select(c => c.SourceSelectSql + " AS [" + c.TargetColumn + "]"));
            using (var delCmd = new SqlCommand("DELETE FROM [" + targetTableName + "]", targetCon))
                await delCmd.ExecuteNonQueryAsync();

            using (var srcCmd = new SqlCommand("SELECT " + selectCols + " FROM [" + sourceTableName + "]", sourceCon))
            using (var reader = await srcCmd.ExecuteReaderAsync())
            using (var bulk = new SqlBulkCopy(targetCon, SqlBulkCopyOptions.KeepIdentity, null))
            {
                bulk.DestinationTableName = "[" + targetTableName + "]";
                bulk.BulkCopyTimeout = 0;
                bulk.BatchSize = 5000;
                foreach (var map in copyMaps)
                    bulk.ColumnMappings.Add(map.TargetColumn, map.TargetColumn);
                await bulk.WriteToServerAsync(reader);
            }
        }

        private async Task ApplyLegacyParameterSyncAsync(SqlConnection sourceCon, SqlConnection targetCon)
        {
            var legacyPairs = new[]
            {
                new TableCopyPair("PARAM_KURSBILGILERI", "KursBilgiParam"),
                new TableCopyPair("PARAM_SINIFLAR", "SinifParam"),
                new TableCopyPair("PARAM_SERTIFIKA_UCRETILANI", "SertifikaUcretParam"),
                new TableCopyPair("PARAM_SETTINGS", "SettingsParam"),
                new TableCopyPair("PARAM_GENEL_PARAMETRELER", "GenelParam")
            };

            foreach (var pair in legacyPairs)
            {
                if (!await TableExistsAsync(sourceCon, pair.SourceTable) || !await TableExistsAsync(targetCon, pair.TargetTable))
                    continue;

                int srcCount = await GetTableRowCountAsync(sourceCon, pair.SourceTable);
                if (srcCount <= 0)
                    continue;

                Log("Legacy param senkron: " + pair.SourceTable + " -> " + pair.TargetTable + " (" + srcCount + " satır)");
                await ForceCopyTableAsync(sourceCon, targetCon, pair.SourceTable, pair.TargetTable);
            }
        }

        private async Task ApplyFinalSqlFixupAsync(string sourceDbName, SqlConnection targetCon)
        {
            if (string.IsNullOrWhiteSpace(sourceDbName) || targetCon == null)
                return;

            Log("Son duzeltme turu basliyor (PARAM_* -> hedef param tablolar)...");

            await ExecFixupIfSourceExistsAsync(
                targetCon,
                sourceDbName,
                "PARAM_KURSBILGILERI",
                @"DELETE FROM dbo.KursBilgiParam;
INSERT INTO dbo.KursBilgiParam
(KURS_ADI,ADRES,ILCE,IL,TELEFON,GSM,PK,WEB,E_POSTA,KURUCU_ADI,MUDUR_ADI,MUSTERI_NO,KURS_ADI_KISA,SOZLESME_BANKA_HESAPNO)
SELECT
KURS_ADI,ADRES,ILCE,IL,TELEFON,GSM,PK,WEB,E_POSTA,KURUCU_ADI,MUDUR_ADI,MUSTERI_NO,KURS_ADI_KISA,SOZLESME_BANKA_HESAPNO
FROM [{DB}].dbo.[PARAM_KURSBILGILERI];");

            await ExecFixupIfSourceExistsAsync(
                targetCon,
                sourceDbName,
                "PARAM_SINIFLAR",
                @"DELETE FROM dbo.SinifParam;
INSERT INTO dbo.SinifParam
(SINIF_DURUMU,SINIF_MEVCUT,SINIF_YENI,SINIF_YAS,SINIF_KUL_ARACLAR,SINIF_KAPSAMI,SINIF_DENEYIM,SINIF_KURS_UCRETI,SINIF_TEORI_UCRETI,SINIF_DRKS_UCRETI,SINIF_TEORI_TRAFIK,SINIF_TEORI_MOTOR,SINIF_TEORI_ILKYRDM,SINIF_TEORI_TRAFIK_ADABI,SINIF_TEORI_TOP_SAAT,SINIF_TEORI_1SAAT_UCRETI,SINIF_TEORI_TOP_UCRETI,SINIF_DRKS_SAAT,SINIF_DRKS_1SAAT_UCRETI,SINIF_DRKS_TOP_UCRETI,SINIF_TABAN_FIYAT,SINIF_DRKS_SMLT_EGTM,SINIF_DRKS_TOP_SAAT,YIL,SERT_2016_ONCESI,YUZ_YIRMI_BES_CC,E_SINAV_MUAF)
SELECT
SINIF_DURUMU,SINIF_MEVCUT,SINIF_YENI,SINIF_YAS,SINIF_KUL_ARACLAR,SINIF_KAPSAMI,SINIF_DENEYIM,SINIF_KURS_UCRETI,SINIF_TEORI_UCRETI,SINIF_DRKS_UCRETI,SINIF_TEORI_TRAFIK,SINIF_TEORI_MOTOR,SINIF_TEORI_ILKYRDM,SINIF_TEORI_TRAFIK_ADABI,SINIF_TEORI_TOP_SAAT,SINIF_TEORI_1SAAT_UCRETI,SINIF_TEORI_TOP_UCRETI,SINIF_DRKS_SAAT,SINIF_DRKS_1SAAT_UCRETI,SINIF_DRKS_TOP_UCRETI,SINIF_TABAN_FIYAT,SINIF_DRKS_SMLT_EGTM,SINIF_DRKS_TOP_SAAT,YIL,SERT_2016_ONCESI,YUZ_YIRMI_BES_CC,E_SINAV_MUAF
FROM [{DB}].dbo.[PARAM_SINIFLAR];");

            await ExecFixupIfSourceExistsAsync(
                targetCon,
                sourceDbName,
                "PARAM_SERTIFIKA_UCRETILANI",
                @"DELETE FROM dbo.SertifikaUcretParam;
INSERT INTO dbo.SertifikaUcretParam
(ID_SERTIFIKA,UC_SINIF,UC_DONEM_ADI,UC_DONEM_YILI,UC_TEORIK_1_SAAT,UC_DIREKS_1_SAAT,UC_TOPLAM_DERS_SA,UC_ACIKLAMA,UC_SINIF_ONC)
SELECT
ID_SERTIFIKA,UC_SINIF,UC_DONEM_ADI,UC_DONEM_YILI,UC_TEORIK_1_SAAT,UC_DIREKS_1_SAAT,UC_TOPLAM_DERS_SA,UC_ACIKLAMA,UC_SINIF_ONC
FROM [{DB}].dbo.[PARAM_SERTIFIKA_UCRETILANI];");

            await ExecFixupIfSourceExistsAsync(
                targetCon,
                sourceDbName,
                "PARAM_SETTINGS",
                @"DELETE FROM dbo.SettingsParam;
INSERT INTO dbo.SettingsParam (LSN_KURUM_KODU,LSN_LISANS_NO,LSN_BITIS_TARIHI)
SELECT LSN_KURUM_KODU,LSN_LISANS_NO,LSN_BITIS_TARIHI
FROM [{DB}].dbo.[PARAM_SETTINGS];");

            await ExecFixupIfSourceExistsAsync(
                targetCon,
                sourceDbName,
                "PARAM_GENEL_PARAMETRELER",
                @"DELETE FROM dbo.GenelParam;
INSERT INTO dbo.GenelParam (
MEBBIS_KUL_ADI_1,MEBBIS_KUL_SIF_1,MEBBIS_KUL_YET_1,
MEBBIS_KUL_ADI_2,MEBBIS_KUL_SIF_2,MEBBIS_KUL_YET_2,
MEBBIS_KUL_ADI_3,MEBBIS_KUL_SIF_3,MEBBIS_KUL_YET_3,
MEBBIS_KUL_ADI_4,MEBBIS_KUL_SIF_4,MEBBIS_KUL_YET_4,
MEBBIS_KUL_ADI_5,MEBBIS_KUL_SIF_5,MEBBIS_KUL_YET_5)
SELECT TOP (1)
ISNULL(MEBBIS_KUL_ADI_1,''),ISNULL(MEBBIS_KUL_SIF_1,''),MEBBIS_KUL_YET_1,
ISNULL(MEBBIS_KUL_ADI_2,''),ISNULL(MEBBIS_KUL_SIF_2,''),MEBBIS_KUL_YET_2,
ISNULL(MEBBIS_KUL_ADI_3,''),ISNULL(MEBBIS_KUL_SIF_3,''),MEBBIS_KUL_YET_3,
ISNULL(MEBBIS_KUL_ADI_4,''),ISNULL(MEBBIS_KUL_SIF_4,''),MEBBIS_KUL_YET_4,
ISNULL(MEBBIS_KUL_ADI_5,''),ISNULL(MEBBIS_KUL_SIF_5,''),MEBBIS_KUL_YET_5
FROM [{DB}].dbo.[PARAM_GENEL_PARAMETRELER];");

            Log("Son duzeltme turu tamamlandi.");
        }

        private async Task ApplyKullaniciPostCopyAsync(SqlConnection targetCon, string targetDbName)
        {
            if (targetCon == null || string.IsNullOrWhiteSpace(targetDbName))
                return;
            if (!await TableExistsAsync(targetCon, "KULLANICI"))
                return;

            // Kaynaktan gelen hashli/sifreli parolalari tek formatta normalize et.
            using (var normalizeAllPasswordsCmd = new SqlCommand(@"
IF OBJECT_ID('dbo.KULLANICI','U') IS NOT NULL
AND COL_LENGTH('dbo.KULLANICI','KULLANICI_SIFRE') IS NOT NULL
BEGIN
    UPDATE dbo.KULLANICI
    SET KULLANICI_SIFRE = 'Admin'
    WHERE ISNULL(KULLANICI_ADI,'') <> '';
END", targetCon))
            {
                await normalizeAllPasswordsCmd.ExecuteNonQueryAsync();
            }

            const string normalizeSql = @"
IF OBJECT_ID('dbo.KULLANICI','U') IS NULL
    RETURN;

IF COL_LENGTH('dbo.KULLANICI','KULLANICI_ADI') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.KULLANICI','KULLANICI_SIFRE') IS NOT NULL
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.KULLANICI WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi))
        BEGIN
            IF COL_LENGTH('dbo.KULLANICI','KAYIT_TARIHI') IS NOT NULL AND COL_LENGTH('dbo.KULLANICI','YETKI') IS NOT NULL
                UPDATE dbo.KULLANICI SET KULLANICI_ADI = @adi, KULLANICI_SIFRE = 'Admin', KAYIT_TARIHI = GETDATE(), YETKI = 'ADMIN' WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi);
            ELSE IF COL_LENGTH('dbo.KULLANICI','KAYIT_TARIHI') IS NOT NULL
                UPDATE dbo.KULLANICI SET KULLANICI_ADI = @adi, KULLANICI_SIFRE = 'Admin', KAYIT_TARIHI = GETDATE() WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi);
            ELSE IF COL_LENGTH('dbo.KULLANICI','YETKI') IS NOT NULL
                UPDATE dbo.KULLANICI SET KULLANICI_ADI = @adi, KULLANICI_SIFRE = 'Admin', YETKI = 'ADMIN' WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi);
            ELSE
                UPDATE dbo.KULLANICI SET KULLANICI_ADI = @adi, KULLANICI_SIFRE = 'Admin' WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi);
        END
        ELSE
        BEGIN
            IF COL_LENGTH('dbo.KULLANICI','KAYIT_TARIHI') IS NOT NULL AND COL_LENGTH('dbo.KULLANICI','YETKI') IS NOT NULL
                INSERT INTO dbo.KULLANICI (KULLANICI_ADI, KULLANICI_SIFRE, KAYIT_TARIHI, YETKI) VALUES (@adi, 'Admin', GETDATE(), 'ADMIN');
            ELSE IF COL_LENGTH('dbo.KULLANICI','KAYIT_TARIHI') IS NOT NULL
                INSERT INTO dbo.KULLANICI (KULLANICI_ADI, KULLANICI_SIFRE, KAYIT_TARIHI) VALUES (@adi, 'Admin', GETDATE());
            ELSE IF COL_LENGTH('dbo.KULLANICI','YETKI') IS NOT NULL
                INSERT INTO dbo.KULLANICI (KULLANICI_ADI, KULLANICI_SIFRE, YETKI) VALUES (@adi, 'Admin', 'ADMIN');
            ELSE
                INSERT INTO dbo.KULLANICI (KULLANICI_ADI, KULLANICI_SIFRE) VALUES (@adi, 'Admin');
        END
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.KULLANICI WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi))
        BEGIN
            IF COL_LENGTH('dbo.KULLANICI','KAYIT_TARIHI') IS NOT NULL AND COL_LENGTH('dbo.KULLANICI','YETKI') IS NOT NULL
                UPDATE dbo.KULLANICI SET KULLANICI_ADI = @adi, KAYIT_TARIHI = GETDATE(), YETKI = 'ADMIN' WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi);
            ELSE IF COL_LENGTH('dbo.KULLANICI','KAYIT_TARIHI') IS NOT NULL
                UPDATE dbo.KULLANICI SET KULLANICI_ADI = @adi, KAYIT_TARIHI = GETDATE() WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi);
            ELSE IF COL_LENGTH('dbo.KULLANICI','YETKI') IS NOT NULL
                UPDATE dbo.KULLANICI SET KULLANICI_ADI = @adi, YETKI = 'ADMIN' WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi);
            ELSE
                UPDATE dbo.KULLANICI SET KULLANICI_ADI = @adi WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@adi);
        END
        ELSE
        BEGIN
            IF COL_LENGTH('dbo.KULLANICI','KAYIT_TARIHI') IS NOT NULL AND COL_LENGTH('dbo.KULLANICI','YETKI') IS NOT NULL
                INSERT INTO dbo.KULLANICI (KULLANICI_ADI, KAYIT_TARIHI, YETKI) VALUES (@adi, GETDATE(), 'ADMIN');
            ELSE IF COL_LENGTH('dbo.KULLANICI','KAYIT_TARIHI') IS NOT NULL
                INSERT INTO dbo.KULLANICI (KULLANICI_ADI, KAYIT_TARIHI) VALUES (@adi, GETDATE());
            ELSE IF COL_LENGTH('dbo.KULLANICI','YETKI') IS NOT NULL
                INSERT INTO dbo.KULLANICI (KULLANICI_ADI, YETKI) VALUES (@adi, 'ADMIN');
            ELSE
                INSERT INTO dbo.KULLANICI (KULLANICI_ADI) VALUES (@adi);
        END
    END
END;";
            using (var cmd = new SqlCommand(normalizeSql, targetCon))
            {
                cmd.Parameters.AddWithValue("@adi", targetDbName.Length > 100 ? targetDbName.Substring(0, 100) : targetDbName);
                await cmd.ExecuteNonQueryAsync();
            }

            string[] dropColumns = new[]
            {
                "CALISMA_TURU",
                "CALISTIGI_KURUM",
                "DURUMU",
                "IZIN_GVNLK",
                "IZIN_ISMAK",
                "IZIN_KASA",
                "IZIN_MTSK",
                "IZIN_PSIKO",
                "IZIN_SRC",
                "KOD",
                "KURUM_CONN_GVNLK",
                "KURUM_CONN_ISMAK",
                "KURUM_CONN_MTSK",
                "KURUM_CONN_PSIKO",
                "KURUM_CONN_SRC"
            };

            foreach (var col in dropColumns)
            {
                string dropSql = @"
IF OBJECT_ID('dbo.KULLANICI','U') IS NOT NULL
AND COL_LENGTH('dbo.KULLANICI', @col) IS NOT NULL
BEGIN
    BEGIN TRY
        EXEC(N'ALTER TABLE dbo.KULLANICI DROP COLUMN [' + @col + ']');
    END TRY
    BEGIN CATCH
    END CATCH
END";
                using (var cmd = new SqlCommand(dropSql, targetCon))
                {
                    cmd.Parameters.AddWithValue("@col", col);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            Log("KULLANICI tablo post-islem tamamlandi (tek kullanici: " + targetDbName + ").");
        }

        private async Task ExecFixupIfSourceExistsAsync(SqlConnection targetCon, string sourceDbName, string sourceTable, string sqlTemplate)
        {
            const string existsSql = @"
IF EXISTS (
    SELECT 1
    FROM [__DB__].INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME=@t
) SELECT 1 ELSE SELECT 0";
            string sourceDbEscaped = sourceDbName.Replace("]", "]]");
            string existsSqlFinal = existsSql.Replace("__DB__", sourceDbEscaped);
            bool exists;
            using (var cmdExists = new SqlCommand(existsSqlFinal, targetCon))
            {
                cmdExists.Parameters.AddWithValue("@t", sourceTable);
                var o = await cmdExists.ExecuteScalarAsync();
                exists = o != null && Convert.ToInt32(o) == 1;
            }

            if (!exists)
            {
                Log("Son duzeltme atlandi: " + sourceTable + " bulunamadi.");
                return;
            }

            string sql = sqlTemplate.Replace("{DB}", sourceDbEscaped);
            using (var cmd = new SqlCommand(sql, targetCon))
            {
                cmd.CommandTimeout = 0;
                await cmd.ExecuteNonQueryAsync();
            }
            Log("Son duzeltme uygulandi: " + sourceTable);
        }

        private async Task<List<string>> GetTableNamesAsync(SqlConnection con)
        {
            var list = new List<string>();
            using (var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' ORDER BY TABLE_NAME", con))
            using (var dr = await cmd.ExecuteReaderAsync())
            {
                while (await dr.ReadAsync())
                    list.Add(dr.GetString(0));
            }
            return list;
        }

        private async Task<List<string>> GetCommonColumnsAsync(SqlConnection kaynakCon, SqlConnection hedefCon, string tableName)
        {
            var src = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var dst = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t", kaynakCon))
            {
                cmd.Parameters.AddWithValue("@t", tableName);
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                        src.Add(dr.GetString(0));
                }
            }

            using (var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t", hedefCon))
            {
                cmd.Parameters.AddWithValue("@t", tableName);
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                        dst.Add(dr.GetString(0));
                }
            }

            return src.Where(dst.Contains).OrderBy(x => x).ToList();
        }

        private async Task<Dictionary<string, ColumnInfo>> GetColumnInfosAsync(SqlConnection con, string tableName)
        {
            var result = new Dictionary<string, ColumnInfo>(StringComparer.OrdinalIgnoreCase);
            const string sql = @"
SELECT COLUMN_NAME, DATA_TYPE, ISNULL(CHARACTER_MAXIMUM_LENGTH, -1) AS CHARACTER_MAXIMUM_LENGTH,
       ISNULL(NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION, ISNULL(NUMERIC_SCALE, 0) AS NUMERIC_SCALE,
       IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME=@t";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@t", tableName);
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        var info = new ColumnInfo
                        {
                            Name = dr.GetString(0),
                            DataType = dr.GetString(1),
                            MaxLength = Convert.ToInt32(dr[2]),
                            Precision = Convert.ToByte(dr[3]),
                            Scale = Convert.ToInt32(dr[4]),
                            IsNullable = string.Equals(Convert.ToString(dr[5]), "YES", StringComparison.OrdinalIgnoreCase)
                        };
                        result[info.Name] = info;
                    }
                }
            }

            return result;
        }

        private async Task EnsureMissingColumnsAsync(
            SqlConnection targetCon,
            string sourceTableName,
            string targetTableName,
            Dictionary<string, ColumnInfo> sourceColumns,
            Dictionary<string, ColumnInfo> targetColumns)
        {
            Log("Kolon ekleme kapali: " + targetTableName + " (sadece mevcut kolonlar kopyalanir)");
            await Task.CompletedTask;
        }

        private bool IsDeprecatedLegacyColumn(string columnName)
        {
            switch ((columnName ?? string.Empty).ToUpperInvariant())
            {
                case "KIM_BABA_ADI":
                case "KIM_DOGUM_YERI":
                case "KIM_KAYIT_NO":
                case "SARI_NOTLAR":
                case "EKSK_OGRNIM_BEL":
                case "EKSK_SAGLIK":
                case "EKSK_SAVCILIK":
                case "EKSK_SOZLESME":
                case "OGR_BEL_TURU":
                case "OGR_BEL_VEREN_KURUM":
                case "OGR_BEL_TARIHI":
                case "OGR_BEL_SAYISI":
                case "SAG_RAPOR_VEREN_KURUM":
                case "SAG_RAPOR_TARIHI":
                case "SAG_RAPOR_BELGENO":
                case "SAVCILIK_BEL_NO":
                case "IMG_OGRNIM_BEL":
                case "IMG_SAGLIK":
                case "IMG_SAVCILIK":
                case "IMG_SOZLESME_ON":
                case "IMG_SOZLESME_ARKA":
                case "IMG_IMZA":
                case "EKSK_IMZA":
                case "EKSK_WEPCAM":
                    return true;
                default:
                    return false;
            }
        }

        private string BuildSqlType(ColumnInfo col)
        {
            string t = (col.DataType ?? string.Empty).ToLowerInvariant();
            switch (t)
            {
                case "varchar":
                case "char":
                case "nvarchar":
                case "nchar":
                case "varbinary":
                case "binary":
                    return col.MaxLength == -1 ? t + "(MAX)" : t + "(" + col.MaxLength + ")";
                case "decimal":
                case "numeric":
                    return t + "(" + col.Precision + "," + col.Scale + ")";
                default:
                    return col.DataType;
            }
        }

        private List<string> GetCopyableColumns(string tableName, Dictionary<string, ColumnInfo> sourceColumns, Dictionary<string, ColumnInfo> targetColumns)
        {
            var copyable = new List<string>();
            var sourceOnly = sourceColumns.Keys.Where(k => !targetColumns.ContainsKey(k)).OrderBy(k => k).ToList();
            var targetOnly = targetColumns.Keys.Where(k => !sourceColumns.ContainsKey(k)).OrderBy(k => k).ToList();
            var commonNames = sourceColumns.Keys.Where(targetColumns.ContainsKey).OrderBy(k => k).ToList();

            if (sourceOnly.Count > 0)
                Log("Uyarı (" + tableName + "): Hedefte olmayan kolonlar -> " + string.Join(", ", sourceOnly));
            if (targetOnly.Count > 0)
                Log("Uyarı (" + tableName + "): Kaynakta olmayan kolonlar -> " + string.Join(", ", targetOnly));

            foreach (var col in commonNames)
            {
                var src = sourceColumns[col];
                var dst = targetColumns[col];
                string reason;
                if (AreColumnsCompatible(src, dst, out reason))
                {
                    copyable.Add(col);
                }
                else
                {
                    Log("Uyarı (" + tableName + "." + col + "): Tip/uzunluk uyumsuzluğu, atlandı -> " + reason);
                }
            }

            return copyable;
        }

        private List<ColumnCopyMap> BuildColumnCopyMaps(string tableName, Dictionary<string, ColumnInfo> sourceColumns, Dictionary<string, ColumnInfo> targetColumns)
        {
            var result = new List<ColumnCopyMap>();
            var legacyMap = GetLegacyToNewColumnMap();

            foreach (var targetCol in targetColumns.Keys.OrderBy(k => k))
            {
                ColumnInfo srcInfo;
                if (sourceColumns.TryGetValue(targetCol, out srcInfo))
                {
                    ColumnInfo dstInfo = targetColumns[targetCol];
                    string reason;
                    if (AreColumnsCompatible(srcInfo, dstInfo, out reason))
                    {
                        result.Add(new ColumnCopyMap { SourceSelectSql = "[" + targetCol + "]", TargetColumn = targetCol });
                    }
                    else
                    {
                        Log("Uyarı (" + tableName + "." + targetCol + "): Tip/uzunluk uyumsuzluğu, atlandı -> " + reason);
                    }
                    continue;
                }

                string legacySource;
                if (legacyMap.TryGetValue(targetCol, out legacySource) && sourceColumns.TryGetValue(legacySource, out srcInfo))
                {
                    ColumnInfo dstInfo = targetColumns[targetCol];
                    string reason;
                    if (AreColumnsCompatible(srcInfo, dstInfo, out reason))
                    {
                        result.Add(new ColumnCopyMap { SourceSelectSql = "[" + legacySource + "]", TargetColumn = targetCol });
                    }
                    else
                    {
                        Log("Uyarı (" + tableName + "." + targetCol + "): Legacy map tip uyumsuzluğu, atlandı -> " + reason);
                    }
                }
            }

            return result;
        }

        private Dictionary<string, string> GetLegacyToNewColumnMap()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "KIMLIK_BABA_ADI", "KIM_BABA_ADI" },
                { "KIMLIK_DOGUM_YERI", "KIM_DOGUM_YERI" },
                { "KIMLIK_KAYIT_NO", "KIM_KAYIT_NO" },
                { "ON_NOTLAR", "SARI_NOTLAR" },
                { "EKSIK_OGRNIM_BEL", "EKSK_OGRNIM_BEL" },
                { "EKSIK_SAGLIK", "EKSK_SAGLIK" },
                { "EKSIK_SAVCILIK", "EKSK_SAVCILIK" },
                { "EKSIK_SOZLESME", "EKSK_SOZLESME" },
                { "OGRNM_BEL_TURU", "OGR_BEL_TURU" },
                { "OGRNM_BEL_VEREN_KURUM", "OGR_BEL_VEREN_KURUM" },
                { "OGRNM_BEL_TARIHI", "OGR_BEL_TARIHI" },
                { "OGRNM_BEL_SAYISI", "OGR_BEL_SAYISI" },
                { "SAG_RAP_VEREN_KURUM", "SAG_RAPOR_VEREN_KURUM" },
                { "SAG_RAP_TARIHI", "SAG_RAPOR_TARIHI" },
                { "SAG_RAP_BELGENO", "SAG_RAPOR_BELGENO" },
                { "CriminalNo", "SAVCILIK_BEL_NO" },
                { "RES_OGRNIM_BEL", "IMG_OGRNIM_BEL" },
                { "RES_SAGLIK", "IMG_SAGLIK" },
                { "RES_SAVCILIK", "IMG_SAVCILIK" },
                { "RES_SOZLESME_ON", "IMG_SOZLESME_ON" },
                { "RES_SOZLESME_ARKA", "IMG_SOZLESME_ARKA" },
                { "RES_IMZA", "IMG_IMZA" },
                { "EKSIK_IMZA", "EKSK_IMZA" },
                { "EKSIK_WEPCAM", "EKSK_WEPCAM" }
            };
        }

        private bool AreColumnsCompatible(ColumnInfo src, ColumnInfo dst, out string reason)
        {
            reason = string.Empty;
            string srcType = (src.DataType ?? string.Empty).ToLowerInvariant();
            string dstType = (dst.DataType ?? string.Empty).ToLowerInvariant();

            if (GetTypeFamily(srcType) != GetTypeFamily(dstType))
            {
                reason = src.DataType + " -> " + dst.DataType;
                return false;
            }

            if (IsLengthBasedType(srcType) && dst.MaxLength != -1 && src.MaxLength > dst.MaxLength)
            {
                reason = "uzunluk " + src.MaxLength + " -> " + dst.MaxLength;
                return false;
            }

            if (IsNumericType(srcType) && (src.Precision > dst.Precision || src.Scale > dst.Scale))
            {
                reason = "precision/scale " + src.Precision + "," + src.Scale + " -> " + dst.Precision + "," + dst.Scale;
                return false;
            }

            return true;
        }

        private string GetTypeFamily(string dataType)
        {
            switch (dataType)
            {
                case "char":
                case "varchar":
                case "nchar":
                case "nvarchar":
                case "text":
                case "ntext":
                    return "text";
                case "binary":
                case "varbinary":
                case "image":
                    return "binary";
                case "tinyint":
                case "smallint":
                case "int":
                case "bigint":
                    return "int";
                case "decimal":
                case "numeric":
                case "money":
                case "smallmoney":
                case "float":
                case "real":
                    return "numeric";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "datetimeoffset":
                case "time":
                    return "date";
                case "bit":
                    return "bit";
                case "uniqueidentifier":
                    return "guid";
                default:
                    return dataType;
            }
        }

        private bool IsLengthBasedType(string dataType)
        {
            switch (dataType)
            {
                case "char":
                case "varchar":
                case "nchar":
                case "nvarchar":
                case "binary":
                case "varbinary":
                    return true;
                default:
                    return false;
            }
        }

        private bool IsNumericType(string dataType)
        {
            switch (dataType)
            {
                case "decimal":
                case "numeric":
                    return true;
                default:
                    return false;
            }
        }

        private void UpdateProgress(int done, int total, string tableName)
        {
            int pct = total > 0 ? (int)Math.Round(done * 100.0 / total) : 0;
            Ui(() =>
            {
                int val = Math.Max(0, Math.Min(100, pct));
                Kopya_ilerle_durum.Value = val;
                lblKopyaYuzde.Text = "%" + val;
                lblKopyalananTablo.Text = "Kopyalanan: " + tableName;
            });
        }

        private void Log(string message)
        {
            Ui(() =>
            {
                listBox1.Items.Add(message);
                if (listBox1.Items.Count > 0)
                    listBox1.TopIndex = listBox1.Items.Count - 1;
            });
        }

        private void Ui(Action action)
        {
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }

        private sealed class ColumnInfo
        {
            public string Name { get; set; }
            public string DataType { get; set; }
            public int MaxLength { get; set; }
            public byte Precision { get; set; }
            public int Scale { get; set; }
            public bool IsNullable { get; set; }
        }

        private sealed class ColumnCopyMap
        {
            public string SourceSelectSql { get; set; }
            public string TargetColumn { get; set; }
        }

        private sealed class TableCopyPair
        {
            public string SourceTable { get; private set; }
            public string TargetTable { get; private set; }

            public TableCopyPair(string sourceTable, string targetTable)
            {
                SourceTable = sourceTable;
                TargetTable = targetTable;
            }
        }
    }
}
