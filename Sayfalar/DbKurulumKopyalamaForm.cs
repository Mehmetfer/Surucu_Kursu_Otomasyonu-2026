using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using Kolera_Mtsk.Services;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class DbKurulumKopyalamaForm : Form
    {
        private static readonly string MsgRestoreUzakSunucuYerelSurucu = string.Join(Environment.NewLine, new[]
        {
            "RESTORE komutu yedek dosyasini SQL Server'in calistigi makineden okur.",
            "Ag sunucusuna bagli iken sizin bilgisayarinizdaki D:\\... veya C:\\... yolu sunucuda aranir; dosya otomatik kopyalanmaz.",
            "",
            "Ne yapabilirsiniz:",
            "• .bak dosyasini sunucunun erisebildigi bir klasore kopyalayip RESTORE'da o tam yolu kullanin, veya",
            "• SQL Server hizmet hesabinin okuma izni oldugu UNC yol kullanin (ornek: \\\\SUNUCU_ADI\\Paylasim\\MELISA.bak).",
            "• .bacpac secip yukleme: sqlpackage genelde dosyayi sizin PC'nizden okuyup uzaga aktarir (.bak'tan farklidir)."
        });
        public string SunucuAdi { get; set; }
        public string KullaniciAdi { get; set; }
        public string Parola { get; set; }
        public string HedefVeritabani { get; set; }
        public string BaglantiTuru { get; set; }
        private string _backupDosyaYolu;
        public string SecilenHedefVeritabani
        {
            get { return (cmbHedefDb.Text ?? string.Empty).Trim(); }
        }

        public DbKurulumKopyalamaForm()
        {
            InitializeComponent();
            Load += DbKurulumKopyalamaForm_Load;
            btnDbleriListele.Click += BtnDbleriListele_Click;
            btnSemaKur.Click += BtnSemaKur_Click;
            btnKontrolEt.Click += BtnKontrolEt_Click;
            btnKopyala.Click += BtnKopyala_Click;
            btnBacSec.Click += BtnBacSec_Click;
            btnKapat.Click += (s, e) => Close();
        }

        private void DbKurulumKopyalamaForm_Load(object sender, EventArgs e)
        {
            FormWorkspaceLayoutHelper.ApplyWorkingAreaMaximized(this);
            txtSunucu.Text = SunucuAdi ?? string.Empty;
            txtKullanici.Text = KullaniciAdi ?? string.Empty;
            txtParola.Text = Parola ?? string.Empty;
            txtBaglantiTuru.Text = string.IsNullOrWhiteSpace(BaglantiTuru) ? "SQL" : BaglantiTuru;
            cmbHedefDb.Text = HedefVeritabani ?? string.Empty;
            txtSunucu.ReadOnly = true;
            txtKullanici.ReadOnly = true;
            txtParola.ReadOnly = true;
            txtBaglantiTuru.ReadOnly = true;
            progressBar.Value = 0;
            lblYuzde.Text = "%0";
            lblDurum.Text = "Hazır";
            lblBaglantiDurumu.Text = "Bağlı değil...";
        }

        private async void BtnDbleriListele_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Veritabanları listeleniyor...");
                cmbKaynakDb.Items.Clear();
                cmbHedefDb.Items.Clear();
                var dbler = await Task.Run(GetDatabaseList);
                cmbKaynakDb.Items.AddRange(dbler.ToArray());
                cmbHedefDb.Items.AddRange(dbler.ToArray());
                if (dbler.Count > 0)
                {
                    cmbKaynakDb.SelectedIndex = 0;
                    if (string.IsNullOrWhiteSpace(cmbHedefDb.Text))
                        cmbHedefDb.SelectedIndex = 0;
                }
                lblBaglantiDurumu.Text = "Sunucuya bağlantı başarılı.";
                Log("Veritabanları yüklendi.");
            }
            catch (Exception ex)
            {
                lblBaglantiDurumu.Text = "Bağlantı hatası.";
                Log("Hata: " + ex.Message);
                MessageBox.Show(ex.Message, "DB Listeleme", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSemaKur_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cmbHedefDb.Text))
            {
                MessageBox.Show("Hedef veritabanı boş olamaz.");
                return;
            }

            try
            {
                SetBusy(true, "Şema kuruluyor...");
                await Task.Run(InstallSchemaSafe);
                Log("Şema kurulumu tamamlandı.");
                MessageBox.Show("Şema kurulumu tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("Hata: " + ex.Message);
                MessageBox.Show("Şema kurulumu başarısız:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, "Hazır");
            }
        }

        private async void BtnKopyala_Click(object sender, EventArgs e)
        {
            bool dosyadanYukleme = !string.IsNullOrWhiteSpace(_backupDosyaYolu) && File.Exists(_backupDosyaYolu);
            if (!dosyadanYukleme && cmbKaynakDb.SelectedItem == null)
            {
                MessageBox.Show("Kaynak veritabanı seçiniz.");
                return;
            }

            string kaynakDb = dosyadanYukleme ? "(DOSYA)" : cmbKaynakDb.SelectedItem.ToString();
            string hedefDb = (cmbHedefDb.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(hedefDb))
            {
                MessageBox.Show("Hedef veritabanı boş olamaz.");
                return;
            }

            if (!dosyadanYukleme && string.Equals(kaynakDb, hedefDb, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Kaynak ve hedef veritabanı aynı olamaz.");
                return;
            }

            try
            {
                LisansPolitikasi.BeginSuppressWebLicenseCalls();
                SetBusy(true, "Kopyalama başlıyor...");
                if (dosyadanYukleme)
                {
                    await DosyadanYukleAsync(hedefDb, _backupDosyaYolu);
                    Log("Dosyadan yükleme tamamlandı.");
                }
                else
                {
                    await CopyDataAsync(kaynakDb, hedefDb);
                    Log("Veri kopyalama tamamlandı.");
                }
                MessageBox.Show("Veri kopyalama tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (dosyadanYukleme)
                    MarkDosyaSatiri(false, ex.Message);
                Log("Hata: " + ex.Message);
                MessageBox.Show("Kopyalama hatası:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                LisansPolitikasi.EndSuppressWebLicenseCalls();
                SetBusy(false, "Hazır");
            }
        }

        private async void BtnKontrolEt_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cmbHedefDb.Text))
            {
                MessageBox.Show("Hedef veritabanı boş olamaz.");
                return;
            }

            try
            {
                SetBusy(true, "Şema kontrol ediliyor...");
                var eksikler = await Task.Run(() => CheckMissingObjects(cmbHedefDb.Text.Trim()));
                if (eksikler.Count == 0)
                {
                    Log("Kontrol tamam: Eksik nesne bulunmadı.");
                    MessageBox.Show("Kontrol tamamlandı. Eksik tablo/SP yok.", "Kontrol", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (var eksik in eksikler)
                    Log("Eksik: " + eksik);

                var sonuc = MessageBox.Show(
                    "Eksik nesneler bulundu (" + eksikler.Count + ").\nEksikleri şimdi oluşturmak ister misiniz?",
                    "Şema Kontrol",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (sonuc == DialogResult.Yes)
                {
                    await Task.Run(InstallSchemaSafe);
                    Log("Eksik nesneler oluşturuldu.");
                    MessageBox.Show("Eksik nesneler oluşturuldu.", "Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log("Hata: " + ex.Message);
                MessageBox.Show("Şema kontrol hatası:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false, "Hazır");
            }
        }

        private void BtnBacSec_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "BAC/BACPAC/BACKUP dosyası seç";
                dlg.Filter = "Veri dosyaları (*.bac;*.bacpac;*.bak)|*.bac;*.bacpac;*.bak|Tüm dosyalar (*.*)|*.*";
                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                _backupDosyaYolu = dlg.FileName;
                RefreshChecklistForSelectedFile();
                Log("Dosya seçildi: " + _backupDosyaYolu);
                string extSel = Path.GetExtension(dlg.FileName ?? string.Empty).ToLowerInvariant();
                if ((extSel == ".bak" || extSel == ".bac")
                    && !SqlSunucuBuMakineyeGoreYerelMi()
                    && YerelSurucuYoluGibi(Path.GetFullPath(dlg.FileName)))
                {
                    Log("UYARI (uzak SQL): .bak RESTORE icin bu yol sunucuda aranir; dosya sizin PC'nizde kalmissa RESTORE basarisiz olur. " + MsgRestoreUzakSunucuYerelSurucu.Replace(Environment.NewLine, " "));
                }
            }
        }

        private void RefreshChecklistForSelectedFile()
        {
            InvokeUi(() =>
            {
                chkKopyalananlar.Items.Clear();
                if (!string.IsNullOrWhiteSpace(_backupDosyaYolu))
                    chkKopyalananlar.Items.Add("DOSYA: " + Path.GetFileName(_backupDosyaYolu), false);
            });
        }

        private async Task DosyadanYukleAsync(string hedefDb, string dosyaYolu)
        {
            string ext = Path.GetExtension(dosyaYolu ?? string.Empty).ToLowerInvariant();
            if (ext == ".bacpac")
            {
                await BacpacImportAsync(hedefDb, dosyaYolu);
                MarkDosyaSatiri(true, null);
                return;
            }

            if (ext == ".bak" || ext == ".bac")
            {
                if (ext == ".bac")
                    Log("UYARI: .bac uzantısı SQL standardı değildir, .bak gibi restore deneniyor.");
                await BackupRestoreAsync(hedefDb, dosyaYolu);
                MarkDosyaSatiri(true, null);
                return;
            }

            throw new InvalidOperationException("Desteklenmeyen dosya türü: " + ext + " (.bak/.bac/.bacpac kullanın)");
        }

        private bool SqlSunucuBuMakineyeGoreYerelMi()
        {
            string host = SunucuAdindenAnaMakineAdi(txtSunucu.Text);
            return SqlSunucuAnaMakineYerelMi(host);
        }

        private static string SunucuAdindenAnaMakineAdi(string serverName)
        {
            if (string.IsNullOrWhiteSpace(serverName))
                return string.Empty;
            string s = serverName.Trim();
            if (s.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(4).Trim();
                int comma = s.IndexOf(',');
                if (comma > 0)
                    s = s.Substring(0, comma).Trim();
            }
            int bs = s.IndexOf('\\');
            if (bs > 0)
                s = s.Substring(0, bs);
            return s.Trim();
        }

        private static bool SqlSunucuAnaMakineYerelMi(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return true;
            if (string.Equals(host, ".", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(host, "(local)", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(host, "127.0.0.1", StringComparison.OrdinalIgnoreCase))
                return true;
            if (host.IndexOf("localdb", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            string machine = Environment.MachineName;
            if (string.Equals(host, machine, StringComparison.OrdinalIgnoreCase))
                return true;
            if (IPAddress.TryParse(host, out IPAddress ip))
                return IPAddress.IsLoopback(ip);
            return false;
        }

        private static bool YerelSurucuYoluGibi(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;
            string t = path.Trim();
            if (t.StartsWith(@"\\", StringComparison.Ordinal))
                return false;
            return t.Length >= 3 && char.IsLetter(t[0]) && t[1] == ':' && (t[2] == '\\' || t[2] == '/');
        }

        private static bool RestoreDosyaErisimHatasiMi(SqlException ex)
        {
            if (ex == null)
                return false;
            string m = ex.Message ?? string.Empty;
            return m.IndexOf("Operating system error 3", StringComparison.OrdinalIgnoreCase) >= 0
                || m.IndexOf("Cannot open backup device", StringComparison.OrdinalIgnoreCase) >= 0
                || m.IndexOf("belirtilen yolu bulamıyor", StringComparison.OrdinalIgnoreCase) >= 0
                || m.IndexOf("sistem belirtilen yolu bulamıyor", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private async Task BackupRestoreAsync(string hedefDb, string dosyaYolu)
        {
            if (string.IsNullOrWhiteSpace(dosyaYolu))
                throw new ArgumentException("Yedek dosya yolu bos olamaz.", nameof(dosyaYolu));

            string tamYol = Path.GetFullPath(dosyaYolu);

            if (SqlSunucuBuMakineyeGoreYerelMi())
            {
                if (!File.Exists(tamYol))
                    throw new FileNotFoundException("Yedek dosyasi bulunamadi: " + tamYol, tamYol);
            }
            else if (YerelSurucuYoluGibi(tamYol))
            {
                throw new InvalidOperationException(MsgRestoreUzakSunucuYerelSurucu);
            }

            using (var con = new SqlConnection(GetMasterConnectionString()))
            {
                await con.OpenAsync();
                const string sql = @"
IF DB_ID(@db) IS NULL
    EXEC('CREATE DATABASE [' + @db + ']');

DECLARE @sql NVARCHAR(MAX) = N'
ALTER DATABASE [' + @db + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [' + @db + '] FROM DISK = @p WITH REPLACE, RECOVERY;
ALTER DATABASE [' + @db + '] SET MULTI_USER;';

EXEC sp_executesql @sql, N'@p NVARCHAR(4000)', @p=@path;";
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@db", hedefDb);
                    cmd.Parameters.AddWithValue("@path", tamYol);
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (SqlException ex) when (RestoreDosyaErisimHatasiMi(ex))
                    {
                        string ek = SqlSunucuBuMakineyeGoreYerelMi()
                            ? Environment.NewLine + Environment.NewLine
                              + "Yerel SQL: dosya var olsa bile SQL Server hizmet hesabi bu klasore okuyamiyor olabilir; yedeği sunucunun eriştiği bir klasor veya paylasima tasiyin."
                            : Environment.NewLine + Environment.NewLine + MsgRestoreUzakSunucuYerelSurucu;
                        throw new InvalidOperationException(ex.Message + ek, ex);
                    }
                }
            }
        }

        private Task BacpacImportAsync(string hedefDb, string dosyaYolu)
        {
            return Task.Run(() =>
            {
                string server = (txtSunucu.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(server))
                    throw new InvalidOperationException("Sunucu bilgisi boş.");
                if (!File.Exists(dosyaYolu))
                    throw new FileNotFoundException("Seçilen dosya bulunamadı: " + dosyaYolu);

                string args;
                if (!string.IsNullOrWhiteSpace(txtKullanici.Text))
                {
                    args = "/Action:Import /TargetServerName:\"" + server + "\" /TargetDatabaseName:\"" + hedefDb
                        + "\" /TargetUser:\"" + txtKullanici.Text + "\" /TargetPassword:\"" + txtParola.Text
                        + "\" /SourceFile:\"" + dosyaYolu + "\" /p:CommandTimeout=0";
                }
                else
                {
                    args = "/Action:Import /TargetServerName:\"" + server + "\" /TargetDatabaseName:\"" + hedefDb
                        + "\" /SourceFile:\"" + dosyaYolu + "\" /p:CommandTimeout=0";
                }

                string sqlPackageExe = ResolveSqlPackageExecutable();
                if (string.IsNullOrWhiteSpace(sqlPackageExe))
                {
                    throw new InvalidOperationException(
                        "sqlpackage bulunamadı. .bacpac yüklemek için SQLPackage aracı kurulu olmalı ve PATH'e eklenmelidir."
                        + Environment.NewLine
                        + "Geçici çözüm: .bak ile geri yükleyin veya SQL Server Data Tools/SqlPackage kurun.");
                }

                var psi = new ProcessStartInfo
                {
                    FileName = sqlPackageExe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    if (!string.IsNullOrWhiteSpace(output))
                        Log(output);
                    if (p.ExitCode != 0)
                        throw new InvalidOperationException("sqlpackage import hatası:" + Environment.NewLine + error);
                }
            });
        }

        private string ResolveSqlPackageExecutable()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "sqlpackage.exe",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (var p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    if (p.ExitCode == 0)
                    {
                        string first = (output ?? string.Empty)
                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(first))
                            return first.Trim();
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private void MarkDosyaSatiri(bool success, string hata)
        {
            InvokeUi(() =>
            {
                if (chkKopyalananlar.Items.Count == 0)
                    return;
                string text = chkKopyalananlar.Items[0].ToString();
                if (!success && !string.IsNullOrWhiteSpace(hata))
                    text += " (Hata: " + hata + ")";
                chkKopyalananlar.Items[0] = text;
                chkKopyalananlar.SetItemChecked(0, success);
            });
        }

        private string GetMasterConnectionString()
        {
            string server = (txtSunucu.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(server))
                throw new InvalidOperationException("Sunucu alanı boş.");

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
                return "Server=" + server + ";Database=master;User Id=" + (txtKullanici.Text ?? string.Empty).Trim() + ";Password=" + (txtParola.Text ?? string.Empty) + ";TrustServerCertificate=True;Encrypt=False;";
            }
        }

        private string BuildConnectionString(string database)
        {
            var sb = new SqlConnectionStringBuilder(GetMasterConnectionString())
            {
                InitialCatalog = database
            };
            return sb.ConnectionString;
        }

        private List<string> GetDatabaseList()
        {
            var result = new List<string>();
            using (var con = new SqlConnection(GetMasterConnectionString()))
            using (var cmd = new SqlCommand("SELECT name FROM sys.databases ORDER BY name", con))
            {
                con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        result.Add(dr.GetString(0));
                }
            }

            return result;
        }

        private void InstallSchemaSafe()
        {
            string dbName = cmbHedefDb.Text.Trim();
            using (var con = new SqlConnection(GetMasterConnectionString()))
            {
                con.Open();
                using (var cmd = new SqlCommand("IF DB_ID(@db) IS NULL EXEC('CREATE DATABASE [' + @db + ']');", con))
                {
                    cmd.Parameters.Add("@db", SqlDbType.NVarChar, 128).Value = dbName;
                    cmd.ExecuteNonQuery();
                }
            }

            using (var con = new SqlConnection(BuildConnectionString(dbName)))
            {
                con.Open();
                foreach (string script in GetSafeCreateScripts())
                {
                    using (var cmd = new SqlCommand(script, con))
                    {
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }
                }

                LisansIlkKurulumDefaults.Apply(con);
            }
        }

        private async Task CopyDataAsync(string kaynakDb, string hedefDb)
        {
            using (var kaynakCon = new SqlConnection(BuildConnectionString(kaynakDb)))
            using (var hedefCon = new SqlConnection(BuildConnectionString(hedefDb)))
            {
                await kaynakCon.OpenAsync();
                await hedefCon.OpenAsync();

                var kaynakTablolar = await GetTableNamesAsync(kaynakCon);
                var hedefTablolar = await GetTableNamesAsync(hedefCon);
                var kopyaEslesmeleri = BuildCopyPairs(kaynakTablolar, hedefTablolar);

                if (kopyaEslesmeleri.Count == 0)
                    throw new InvalidOperationException("Kaynak ve hedefte ortak tablo bulunamadı.");

                InvokeUi(() =>
                {
                    progressBar.Minimum = 0;
                    progressBar.Maximum = kopyaEslesmeleri.Count;
                    progressBar.Value = 0;
                    lblYuzde.Text = "%0";
                    chkKopyalananlar.Items.Clear();
                    foreach (var eslesme in kopyaEslesmeleri)
                        chkKopyalananlar.Items.Add(eslesme.TargetTable, false);
                });

                foreach (var eslesme in kopyaEslesmeleri)
                {
                    string kaynakTablo = eslesme.SourceTable;
                    string hedefTablo = eslesme.TargetTable;
                    var kaynakKolonlar = await GetColumnInfosAsync(kaynakCon, kaynakTablo);
                    var hedefKolonlar = await GetColumnInfosAsync(hedefCon, hedefTablo);
                    await EnsureMissingColumnsAsync(hedefCon, kaynakTablo, hedefTablo, kaynakKolonlar, hedefKolonlar);
                    hedefKolonlar = await GetColumnInfosAsync(hedefCon, hedefTablo);
                    var kopyaKolonlari = BuildColumnCopyMaps(kaynakTablo + "->" + hedefTablo, kaynakKolonlar, hedefKolonlar);
                    if (kopyaKolonlari.Count == 0)
                    {
                        Log("Atlandı: " + kaynakTablo + " -> " + hedefTablo + " (ortak kolon yok)");
                        UpdateProgress(hedefTablo);
                        continue;
                    }

                    string kolons = string.Join(",", kopyaKolonlari.Select(c => c.SourceSelectSql + " AS [" + c.TargetColumn + "]"));
                    bool aracTipFiltrele =
                        hedefTablo.Equals("AracParam", StringComparison.OrdinalIgnoreCase)
                        && kaynakKolonlar.ContainsKey("ARAC_TIPI");
                    string whereAracTip = aracTipFiltrele
                        ? " WHERE ISNULL(LTRIM(RTRIM(ARAC_TIPI)),'') <> ''"
                        : string.Empty;
                    try
                    {
                        using (var temizle = new SqlCommand("DELETE FROM [" + hedefTablo + "]", hedefCon))
                            await temizle.ExecuteNonQueryAsync();

                        using (var srcCmd = new SqlCommand("SELECT " + kolons + " FROM [" + kaynakTablo + "]" + whereAracTip, kaynakCon))
                        using (var reader = await srcCmd.ExecuteReaderAsync())
                        using (var bulk = new SqlBulkCopy(hedefCon, SqlBulkCopyOptions.KeepIdentity, null))
                        {
                            bulk.DestinationTableName = hedefTablo;
                            bulk.BulkCopyTimeout = 0;
                            bulk.BatchSize = 5000;
                            foreach (var col in kopyaKolonlari)
                                bulk.ColumnMappings.Add(col.TargetColumn, col.TargetColumn);
                            await bulk.WriteToServerAsync(reader);
                        }

                        Log("Tamam: " + kaynakTablo + " -> " + hedefTablo);
                        MarkTable(hedefTablo, true);
                    }
                    catch (Exception ex)
                    {
                        Log("Hata (" + kaynakTablo + "->" + hedefTablo + "): " + ex.Message);
                        MarkTable(hedefTablo, false);
                    }

                    UpdateProgress(hedefTablo);
                }

                await SyncKursiyerOdemeColumnsAsync(kaynakCon, hedefCon, kopyaEslesmeleri);
                await ApplyKullaniciPostCopyAsync(hedefCon, hedefDb);

                // Bire bir kopya modu: kaynaktaki veriyi degistiren tum post-process adimlari atlanir.
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

        private IEnumerable<string> GetCriticalParameterTables()
        {
            return new[]
            {
                "KursBilgiParam",
                "GenelParam",
                "SinifParam",
                "SertifikaSinifParam",
                "SertifikaUcretParam",
                "DurumParam",
                "IlParam",
                "IlceParam",
                "SettingsParam"
            };
        }

        private async Task EnsureCriticalTableCopiedAsync(SqlConnection sourceCon, SqlConnection targetCon, string tableName)
        {
            string sourceTableName = await ResolveSourceTableForTargetAsync(sourceCon, tableName);
            if (string.IsNullOrWhiteSpace(sourceTableName))
                return;

            int sourceCount = await GetTableRowCountAsync(sourceCon, sourceTableName);
            if (sourceCount <= 0)
                return;

            int targetCount = await GetTableRowCountAsync(targetCon, tableName);
            if (targetCount > 0)
                return;

            Log("Kritik tablo yeniden kopyalaniyor: " + sourceTableName + " -> " + tableName);
            await ForceCopyTableAsync(sourceCon, targetCon, sourceTableName, tableName);
            targetCount = await GetTableRowCountAsync(targetCon, tableName);
            if (targetCount <= 0)
                throw new InvalidOperationException(tableName + " kaynaktan dolu olmasina ragmen hedefe kopyalanamadi.");
        }

        private async Task<string> ResolveSourceTableForTargetAsync(SqlConnection sourceCon, string targetTableName)
        {
            foreach (var candidate in GetSourceTableCandidates(targetTableName))
            {
                if (await TableExistsAsync(sourceCon, candidate))
                    return candidate;
            }
            return null;
        }

        private IEnumerable<string> GetSourceTableCandidates(string targetTableName)
        {
            if (string.IsNullOrWhiteSpace(targetTableName))
                yield break;

            yield return targetTableName;

            string legacy = MapToLegacyTableName(targetTableName);
            if (!string.Equals(legacy, targetTableName, StringComparison.OrdinalIgnoreCase))
                yield return legacy;
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

        private async Task ForceCopyTableAsync(SqlConnection sourceCon, SqlConnection targetCon, string sourceTableName, string targetTableName)
        {
            var sourceColumns = await GetColumnInfosAsync(sourceCon, sourceTableName);
            var targetColumns = await GetColumnInfosAsync(targetCon, targetTableName);
            await EnsureMissingColumnsAsync(targetCon, sourceTableName, targetTableName, sourceColumns, targetColumns);
            targetColumns = await GetColumnInfosAsync(targetCon, targetTableName);

            var copyMaps = BuildColumnCopyMaps(sourceTableName + "->" + targetTableName, sourceColumns, targetColumns);
            if (copyMaps.Count == 0)
                throw new InvalidOperationException(sourceTableName + " -> " + targetTableName + " icin ortak kopyalanabilir kolon bulunamadi.");

            string selectCols = string.Join(",", copyMaps.Select(c => c.SourceSelectSql + " AS [" + c.TargetColumn + "]"));
            using (var delCmd = new SqlCommand("DELETE FROM [" + targetTableName + "]", targetCon))
                await delCmd.ExecuteNonQueryAsync();

            using (var srcCmd = new SqlCommand("SELECT " + selectCols + " FROM [" + sourceTableName + "]", sourceCon))
            using (var reader = await srcCmd.ExecuteReaderAsync())
            using (var bulk = new SqlBulkCopy(targetCon, SqlBulkCopyOptions.KeepIdentity, null))
            {
                bulk.DestinationTableName = targetTableName;
                bulk.BulkCopyTimeout = 0;
                bulk.BatchSize = 5000;
                foreach (var map in copyMaps)
                    bulk.ColumnMappings.Add(map.TargetColumn, map.TargetColumn);
                await bulk.WriteToServerAsync(reader);
            }
        }

        private async Task LogCriticalTableCountsAsync(SqlConnection sourceCon, SqlConnection targetCon, IEnumerable<string> tableNames)
        {
            foreach (var tableName in tableNames)
            {
                string sourceTableName = await ResolveSourceTableForTargetAsync(sourceCon, tableName);
                int sourceCount = string.IsNullOrWhiteSpace(sourceTableName) ? 0 : await GetTableRowCountAsync(sourceCon, sourceTableName);
                int targetCount = await GetTableRowCountAsync(targetCon, tableName);
                Log("Satir sayisi [" + tableName + "] kaynak(" + (sourceTableName ?? "-") + ")=" + sourceCount + " hedef=" + targetCount);
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
                if (!await TableExistsAsync(sourceCon, pair.SourceTable))
                    continue;
                if (!await TableExistsAsync(targetCon, pair.TargetTable))
                    continue;

                int srcCount = await GetTableRowCountAsync(sourceCon, pair.SourceTable);
                if (srcCount <= 0)
                    continue;

                Log("Legacy param senkron: " + pair.SourceTable + " -> " + pair.TargetTable + " (" + srcCount + " satir)");
                await ForceCopyTableAsync(sourceCon, targetCon, pair.SourceTable, pair.TargetTable);
            }
        }

        private async Task ApplyFinalSqlFixupAsync(string sourceDbName, SqlConnection targetCon)
        {
            if (string.IsNullOrWhiteSpace(sourceDbName) || targetCon == null)
                return;

            await ExecFixupIfSourceExistsAsync(targetCon, sourceDbName, "PARAM_SINIFLAR",
                @"DELETE FROM dbo.SinifParam;
INSERT INTO dbo.SinifParam
(SINIF_DURUMU,SINIF_MEVCUT,SINIF_YENI,SINIF_YAS,SINIF_KUL_ARACLAR,SINIF_KAPSAMI,SINIF_DENEYIM,SINIF_KURS_UCRETI,SINIF_TEORI_UCRETI,SINIF_DRKS_UCRETI,SINIF_TEORI_TRAFIK,SINIF_TEORI_MOTOR,SINIF_TEORI_ILKYRDM,SINIF_TEORI_TRAFIK_ADABI,SINIF_TEORI_TOP_SAAT,SINIF_TEORI_1SAAT_UCRETI,SINIF_TEORI_TOP_UCRETI,SINIF_DRKS_SAAT,SINIF_DRKS_1SAAT_UCRETI,SINIF_DRKS_TOP_UCRETI,SINIF_TABAN_FIYAT,SINIF_DRKS_SMLT_EGTM,SINIF_DRKS_TOP_SAAT,YIL,SERT_2016_ONCESI,YUZ_YIRMI_BES_CC,E_SINAV_MUAF)
SELECT
SINIF_DURUMU,SINIF_MEVCUT,SINIF_YENI,SINIF_YAS,SINIF_KUL_ARACLAR,SINIF_KAPSAMI,SINIF_DENEYIM,SINIF_KURS_UCRETI,SINIF_TEORI_UCRETI,SINIF_DRKS_UCRETI,SINIF_TEORI_TRAFIK,SINIF_TEORI_MOTOR,SINIF_TEORI_ILKYRDM,SINIF_TEORI_TRAFIK_ADABI,SINIF_TEORI_TOP_SAAT,SINIF_TEORI_1SAAT_UCRETI,SINIF_TEORI_TOP_UCRETI,SINIF_DRKS_SAAT,SINIF_DRKS_1SAAT_UCRETI,SINIF_DRKS_TOP_UCRETI,SINIF_TABAN_FIYAT,SINIF_DRKS_SMLT_EGTM,SINIF_DRKS_TOP_SAAT,YIL,SERT_2016_ONCESI,YUZ_YIRMI_BES_CC,E_SINAV_MUAF
FROM [{DB}].dbo.[PARAM_SINIFLAR];");

            await ExecFixupIfSourceExistsAsync(targetCon, sourceDbName, "PARAM_SERTIFIKA_UCRETILANI",
                @"DELETE FROM dbo.SertifikaUcretParam;
INSERT INTO dbo.SertifikaUcretParam
(ID_SERTIFIKA,UC_SINIF,UC_DONEM_ADI,UC_DONEM_YILI,UC_TEORIK_1_SAAT,UC_DIREKS_1_SAAT,UC_TOPLAM_DERS_SA,UC_ACIKLAMA,UC_SINIF_ONC)
SELECT
ID_SERTIFIKA,UC_SINIF,UC_DONEM_ADI,UC_DONEM_YILI,UC_TEORIK_1_SAAT,UC_DIREKS_1_SAAT,UC_TOPLAM_DERS_SA,UC_ACIKLAMA,UC_SINIF_ONC
FROM [{DB}].dbo.[PARAM_SERTIFIKA_UCRETILANI];");

            await ExecFixupIfSourceExistsAsync(targetCon, sourceDbName, "PARAM_SETTINGS",
                @"DELETE FROM dbo.SettingsParam;
INSERT INTO dbo.SettingsParam (LSN_KURUM_KODU,LSN_LISANS_NO,LSN_BITIS_TARIHI)
SELECT LSN_KURUM_KODU,LSN_LISANS_NO,LSN_BITIS_TARIHI
FROM [{DB}].dbo.[PARAM_SETTINGS];");

            await ExecFixupIfSourceExistsAsync(targetCon, sourceDbName, "PARAM_GENEL_PARAMETRELER",
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
            string sourceDbEscaped = sourceDbName.Replace("]", "]]");
            string existsSql = @"
IF EXISTS (
    SELECT 1
    FROM [" + sourceDbEscaped + @"].INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME=@t
) SELECT 1 ELSE SELECT 0";

            bool exists;
            using (var cmdExists = new SqlCommand(existsSql, targetCon))
            {
                cmdExists.Parameters.AddWithValue("@t", sourceTable);
                var o = await cmdExists.ExecuteScalarAsync();
                exists = o != null && Convert.ToInt32(o) == 1;
            }

            if (!exists)
                return;

            string sql = sqlTemplate.Replace("{DB}", sourceDbEscaped);
            using (var cmd = new SqlCommand(sql, targetCon))
            {
                cmd.CommandTimeout = 0;
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task BackfillSettingsParamFromLocalLicenseAsync(SqlConnection targetCon)
        {
            if (!await TableExistsAsync(targetCon, "APP_LOCAL_LISANS") || !await TableExistsAsync(targetCon, "SettingsParam"))
                return;

            string lisansNo = string.Empty;
            string kurumKodu = string.Empty;
            string bitisTarihi = string.Empty;

            try
            {
                const string sql = @"
SELECT TOP 1
    ISNULL(MUSTERI_NO,'') AS MUSTERI_NO,
    ISNULL(LISANS_NO,'') AS LISANS_NO,
    ISNULL(CONVERT(VARCHAR(10), BITIS_TARIHI, 23), '') AS BITIS_TARIHI
FROM APP_LOCAL_LISANS
ORDER BY ID DESC";
                using (var cmd = new SqlCommand(sql, targetCon))
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (!await dr.ReadAsync())
                        return;
                    kurumKodu = Convert.ToString(dr["MUSTERI_NO"]).Trim();
                    lisansNo = Convert.ToString(dr["LISANS_NO"]).Trim();
                    bitisTarihi = Convert.ToString(dr["BITIS_TARIHI"]).Trim();
                }
            }
            catch
            {
                // Eski kurulumlarda MUSTERI_NO/BITIS_TARIHI kolonu olmayabilir.
                const string fallbackSql = @"
SELECT TOP 1
    '' AS MUSTERI_NO,
    ISNULL(LISANS_NO,'') AS LISANS_NO,
    '' AS BITIS_TARIHI
FROM APP_LOCAL_LISANS
ORDER BY ID DESC";
                using (var cmd = new SqlCommand(fallbackSql, targetCon))
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (!await dr.ReadAsync())
                        return;
                    kurumKodu = string.Empty;
                    lisansNo = Convert.ToString(dr["LISANS_NO"]).Trim();
                    bitisTarihi = string.Empty;
                }
            }

            if (string.IsNullOrWhiteSpace(lisansNo) && string.IsNullOrWhiteSpace(kurumKodu) && string.IsNullOrWhiteSpace(bitisTarihi))
                return;

            const string upsertSql = @"
IF EXISTS (SELECT 1 FROM SettingsParam)
BEGIN
    UPDATE SettingsParam
    SET
        LSN_KURUM_KODU = CASE WHEN ISNULL(LSN_KURUM_KODU,'') = '' THEN @KURUM ELSE LSN_KURUM_KODU END,
        LSN_LISANS_NO = CASE WHEN ISNULL(LSN_LISANS_NO,'') = '' THEN @LISANS ELSE LSN_LISANS_NO END,
        LSN_BITIS_TARIHI = CASE WHEN ISNULL(LSN_BITIS_TARIHI,'') = '' THEN @BITIS ELSE LSN_BITIS_TARIHI END
    WHERE ID = (SELECT TOP 1 ID FROM SettingsParam ORDER BY ID DESC);
END
ELSE
BEGIN
    INSERT INTO SettingsParam (LSN_KURUM_KODU, LSN_LISANS_NO, LSN_BITIS_TARIHI)
    VALUES (@KURUM, @LISANS, @BITIS);
END";
            using (var cmd = new SqlCommand(upsertSql, targetCon))
            {
                cmd.Parameters.AddWithValue("@KURUM", kurumKodu);
                cmd.Parameters.AddWithValue("@LISANS", lisansNo);
                cmd.Parameters.AddWithValue("@BITIS", bitisTarihi);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private async Task EnsureAracParamReadyAsync(SqlConnection sourceCon, SqlConnection targetCon)
        {
            if (!await TableExistsAsync(targetCon, "AracParam"))
                return;

            int targetValidCount = await GetAracValidCountAsync(targetCon, "AracParam");
            if (targetValidCount > 0)
                return;

            string sourceTableName = await ResolveSourceTableForTargetAsync(sourceCon, "AracParam");
            if (!string.IsNullOrWhiteSpace(sourceTableName) && await TableExistsAsync(sourceCon, sourceTableName))
            {
                int sourceValidCount = await GetAracValidCountAsync(sourceCon, sourceTableName);
                if (sourceValidCount > 0)
                {
                    Log("AracParam yeniden dolduruluyor: " + sourceTableName + " -> AracParam");
                    await CopyAracParamWithFilterAsync(sourceCon, targetCon, sourceTableName);
                }
            }

            targetValidCount = await GetAracValidCountAsync(targetCon, "AracParam");
            if (targetValidCount <= 0)
            {
                Log("AracParam boş kaldı, örnek araç kaydı oluşturuluyor.");
                await InsertSampleAracIfEmptyAsync(targetCon);
            }
        }

        private async Task<int> GetAracValidCountAsync(SqlConnection con, string tableName)
        {
            if (!await TableExistsAsync(con, tableName))
                return 0;

            string sql = "SELECT COUNT(1) FROM [" + tableName + "] WHERE ISNULL(LTRIM(RTRIM(ARAC_PLAKA)),'') <> ''";
            using (var cmd = new SqlCommand(sql, con))
            {
                object o = await cmd.ExecuteScalarAsync();
                return o == null || o == DBNull.Value ? 0 : Convert.ToInt32(o);
            }
        }

        private async Task CopyAracParamWithFilterAsync(SqlConnection sourceCon, SqlConnection targetCon, string sourceTableName)
        {
            var sourceColumns = await GetColumnInfosAsync(sourceCon, sourceTableName);
            var targetColumns = await GetColumnInfosAsync(targetCon, "AracParam");
            var copyMaps = BuildColumnCopyMaps(sourceTableName + "->AracParam", sourceColumns, targetColumns);
            if (copyMaps.Count == 0)
                return;

            string selectCols = string.Join(",", copyMaps.Select(c => c.SourceSelectSql + " AS [" + c.TargetColumn + "]"));
            string srcSql = "SELECT " + selectCols + " FROM [" + sourceTableName + "] WHERE ISNULL(LTRIM(RTRIM(ARAC_PLAKA)),'') <> ''";

            using (var delCmd = new SqlCommand("DELETE FROM [AracParam]", targetCon))
                await delCmd.ExecuteNonQueryAsync();

            using (var srcCmd = new SqlCommand(srcSql, sourceCon))
            using (var reader = await srcCmd.ExecuteReaderAsync())
            using (var bulk = new SqlBulkCopy(targetCon, SqlBulkCopyOptions.KeepIdentity, null))
            {
                bulk.DestinationTableName = "AracParam";
                bulk.BulkCopyTimeout = 0;
                bulk.BatchSize = 5000;
                foreach (var map in copyMaps)
                    bulk.ColumnMappings.Add(map.TargetColumn, map.TargetColumn);
                await bulk.WriteToServerAsync(reader);
            }
        }

        private async Task InsertSampleAracIfEmptyAsync(SqlConnection targetCon)
        {
            const string sql = @"
IF NOT EXISTS (
    SELECT 1 FROM AracParam WHERE ISNULL(LTRIM(RTRIM(ARAC_PLAKA)),'') <> ''
)
BEGIN
    INSERT INTO AracParam (ARAC_TIPI, ARAC_PLAKA, DURUMU, MARKASI, RENGI, VITES_TURU, MODEL, MUHAYENE_TAR, AKT)
    VALUES ('OTOMOBIL', '34ABC123', 'Aktif', 'Ornek', 'Beyaz', 'Manuel', 'Ornek', GETDATE(), 1);
END";
            using (var cmd = new SqlCommand(sql, targetCon))
                await cmd.ExecuteNonQueryAsync();
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
                    Log("Uyarı (" + tableName + "." + col + "): Tip uyumsuzluğu, atlandı -> " + reason);
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
                        Log("Uyarı (" + tableName + "." + targetCol + "): Tip uyumsuzluğu, atlandı -> " + reason);
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

        private List<string> GetSafeCreateScripts()
        {
            return new List<string>
            {
                @"IF OBJECT_ID('dbo.KULLANICI','U') IS NULL
                  CREATE TABLE dbo.KULLANICI(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    KULLANICI_ADI VARCHAR(100) NULL,
                    KULLANICI_SIFRE VARCHAR(100) NULL,
                    KAYIT_TARIHI DATETIME NULL,
                    YETKI VARCHAR(100) NULL
                  );",
                @"IF OBJECT_ID('dbo.KULLANICI','U') IS NOT NULL
                  BEGIN
                    DECLARE @dropSql NVARCHAR(MAX) = N'';
                    SELECT @dropSql = @dropSql +
                        N'ALTER TABLE dbo.KULLANICI DROP COLUMN [' + c.name + N'];'
                    FROM sys.columns c
                    WHERE c.object_id = OBJECT_ID('dbo.KULLANICI')
                      AND c.name NOT IN ('ID','KULLANICI_ADI','KULLANICI_SIFRE','KAYIT_TARIHI','YETKI')
                      AND c.is_identity = 0
                      AND c.is_computed = 0;
                    IF LEN(@dropSql) > 0
                        EXEC sp_executesql @dropSql;
                  END;",
                @"IF OBJECT_ID('dbo.KULLANICI','U') IS NOT NULL
                  BEGIN
                    DECLARE @DefaultUserName VARCHAR(100) = LEFT(DB_NAME(), 100);
                    IF NOT EXISTS (SELECT 1 FROM dbo.KULLANICI WHERE UPPER(ISNULL(KULLANICI_ADI,'')) = UPPER(@DefaultUserName))
                    BEGIN
                        INSERT INTO dbo.KULLANICI (KULLANICI_ADI, KULLANICI_SIFRE, KAYIT_TARIHI, YETKI)
                        VALUES (@DefaultUserName, 'Admin', GETDATE(), 'ADMIN');
                    END
                  END;",

                @"IF OBJECT_ID('dbo.KURSIYER','U') IS NULL
                  CREATE TABLE dbo.KURSIYER(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ADI NVARCHAR(100) NULL,
                    SOYADI NVARCHAR(100) NULL,
                    TC_NO VARCHAR(11) NULL,
                    GSM_1 NVARCHAR(20) NULL,
                    GSM_2 NVARCHAR(20) NULL,
                    KIMLIK_BABA_ADI NVARCHAR(100) NULL,
                    KIM_ANA_ADI NVARCHAR(100) NULL,
                    KIMLIK_DOGUM_YERI NVARCHAR(100) NULL,
                    EV_ADRESI NVARCHAR(500) NULL,
                    ADAY_NO INT NULL,
                    ON_NOTLAR NVARCHAR(MAX) NULL,
                    DOGUM_TARIHI DATE NULL,
                    KAYIT_TARIHI DATE NULL,
                    RESIM VARBINARY(MAX) NULL,
                    ID_GRUP_KARTI INT NULL,
                    SERTIFIKA_SINIFI NVARCHAR(50) NULL,
                    ONCE_SERT_SINIFI NVARCHAR(50) NULL,
                    KURSIYER_DURUMU INT NULL,
                    ONCE_SERT_BELGESAYI NVARCHAR(50) NULL,
                    RESIM_WEBCAM VARBINARY(MAX) NULL,
                    KIMLIK_KAYIT_NO NVARCHAR(50) NULL,
                    EV_TELEFON NVARCHAR(20) NULL,
                    CINSIYET VARCHAR(20) NULL,
                    TAHSILI NVARCHAR(100) NULL
                  );",

                @"IF OBJECT_ID('dbo.KURSIYERLER','U') IS NULL
                  CREATE TABLE dbo.KURSIYERLER(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ADI NVARCHAR(200) NULL,
                    SOYADI NVARCHAR(200) NULL,
                    TC_NO VARCHAR(11) NULL,
                    TC VARCHAR(11) NULL,
                    GSM_1 NVARCHAR(40) NULL,
                    GSM NVARCHAR(40) NULL,
                    ID_GRUP_KARTI INT NULL,
                    DOGUM_TARIHI DATE NULL,
                    KAYIT_TARIHI DATE NULL,
                    SERTIFIKA_SINIFI NVARCHAR(100) NULL,
                    CINSIYET VARCHAR(20) NULL,
                    TAHSILI NVARCHAR(100) NULL
                  );",

                @"IF COL_LENGTH('dbo.KURSIYER', 'CINSIYET') IS NULL
                  ALTER TABLE dbo.KURSIYER ADD CINSIYET VARCHAR(20) NULL;",
                @"IF COL_LENGTH('dbo.KURSIYER', 'TAHSILI') IS NULL
                  ALTER TABLE dbo.KURSIYER ADD TAHSILI NVARCHAR(100) NULL;",
                @"IF COL_LENGTH('dbo.KURSIYERLER', 'CINSIYET') IS NULL
                  ALTER TABLE dbo.KURSIYERLER ADD CINSIYET VARCHAR(20) NULL;",
                @"IF COL_LENGTH('dbo.KURSIYERLER', 'TAHSILI') IS NULL
                  ALTER TABLE dbo.KURSIYERLER ADD TAHSILI NVARCHAR(100) NULL;",
                @"IF COL_LENGTH('dbo.KURSIYER', 'EV_IL') IS NULL
                  ALTER TABLE dbo.KURSIYER ADD EV_IL NVARCHAR(50) NULL;",
                @"IF COL_LENGTH('dbo.KURSIYER', 'EV_ILCE') IS NULL
                  ALTER TABLE dbo.KURSIYER ADD EV_ILCE NVARCHAR(100) NULL;",
                @"IF COL_LENGTH('dbo.KURSIYER', 'IS_ADRESI') IS NULL
                  ALTER TABLE dbo.KURSIYER ADD IS_ADRESI NVARCHAR(500) NULL;",
                @"IF COL_LENGTH('dbo.KURSIYERLER', 'IS_ADRESI') IS NULL
                  ALTER TABLE dbo.KURSIYERLER ADD IS_ADRESI NVARCHAR(500) NULL;",

                @"IF OBJECT_ID('dbo.SINAV_TARIHLERI','U') IS NULL
                  CREATE TABLE dbo.SINAV_TARIHLERI(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    SINAV_TARIHI DATE NULL,
                    SINAV_TURU VARCHAR(20) NULL,
                    SINAV_DURUMU VARCHAR(20) NULL,
                    DURUM VARCHAR(20) NULL,
                    ACIKLAMA VARCHAR(250) NULL,
                    SINAV_ACIKLAMA VARCHAR(250) NULL
                  );",

                @"IF OBJECT_ID('dbo.SINAV_LISTE_TEORI','U') IS NULL
                  CREATE TABLE dbo.SINAV_LISTE_TEORI(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ID_SINAV_TARIHI INT NULL,
                    ID_KURSIYER INT NULL,
                    TEO_HAK INT NULL,
                    TEO_NOT VARCHAR(15) NULL,
                    TEO_DURUM VARCHAR(30) NULL,
                    E_SINAV_TARIHI DATE NULL,
                    E_SINAV_SAATI VARCHAR(10) NULL,
                    E_SINAV_YERI VARCHAR(350) NULL,
                    E_SINAV_ACIKLAMA VARCHAR(150) NULL
                  );",
                @"IF OBJECT_ID('dbo.SINAV_LISTE_TEORI','U') IS NOT NULL
                  BEGIN
                    IF COL_LENGTH('dbo.SINAV_LISTE_TEORI','E_SINAV_YERI') IS NULL ALTER TABLE dbo.SINAV_LISTE_TEORI ADD E_SINAV_YERI VARCHAR(350) NULL;
                    IF COL_LENGTH('dbo.SINAV_LISTE_TEORI','E_SINAV_ACIKLAMA') IS NULL ALTER TABLE dbo.SINAV_LISTE_TEORI ADD E_SINAV_ACIKLAMA VARCHAR(150) NULL;
                  END;",

                @"IF OBJECT_ID('dbo.SINAV_LISTE_DIREKSIYON','U') IS NULL
                  CREATE TABLE dbo.SINAV_LISTE_DIREKSIYON(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ID_SINAV_TARIHI INT NULL,
                    ID_KURSIYER INT NULL,
                    DIR_HAK INT NULL,
                    DIR_NOT VARCHAR(15) NULL,
                    DIR_DURUM VARCHAR(15) NULL,
                    RANDEVU_SAATI VARCHAR(20) NULL,
                    ID_PERSONEL INT NULL,
                    ID_ARAC INT NULL
                  );",
                @"IF OBJECT_ID('dbo.KURSIYER_EVRAK','U') IS NULL
                  CREATE TABLE dbo.KURSIYER_EVRAK(
                    ID_KURSIYER INT PRIMARY KEY,
                    EKSIK_OGRNIM_BEL VARCHAR(5) NULL,
                    EKSIK_SAGLIK VARCHAR(5) NULL,
                    EKSIK_SAVCILIK VARCHAR(5) NULL,
                    EKSIK_SOZLESME VARCHAR(5) NULL,
                    OGRNM_BEL_TURU VARCHAR(70) NULL,
                    OGRNM_BEL_VEREN_KURUM VARCHAR(200) NULL,
                    OGRNM_BEL_TARIHI DATE NULL,
                    OGRNM_BEL_SAYISI VARCHAR(20) NULL,
                    SAG_RAP_VEREN_KURUM VARCHAR(200) NULL,
                    SAG_RAP_TARIHI DATE NULL,
                    SAG_RAP_BELGENO VARCHAR(50) NULL,
                    SAVCILIK_BEL_VEREN_KURUM VARCHAR(200) NULL,
                    SAVCILIK_BEL_TARIHI DATE NULL,
                    CriminalNo VARCHAR(60) NULL,
                    RES_OGRNIM_BEL VARBINARY(MAX) NULL,
                    RES_SAGLIK VARBINARY(MAX) NULL,
                    RES_SAVCILIK VARBINARY(MAX) NULL,
                    RES_SOZLESME_ON VARBINARY(MAX) NULL,
                    RES_SOZLESME_ARKA VARBINARY(MAX) NULL,
                    EKSIK_IMZA VARCHAR(5) NULL,
                    RES_IMZA VARBINARY(MAX) NULL,
                    OZUR_DURUMU VARCHAR(200) NULL,
                    YABANCI_DIL VARCHAR(100) NULL,
                    SAG_RAPOR_REFERANS VARCHAR(80) NULL,
                    SAG_RAPOR_HESKODU VARCHAR(30) NULL,
                    EKSIK_WEPCAM VARCHAR(5) NULL,
                    FATURA_NO VARCHAR(100) NULL,
                    FATURA_TARIHI DATE NULL,
                    FATURA_TUTARI MONEY NULL
                  );",

                @"IF OBJECT_ID('dbo.GRUP_KARTI','U') IS NULL
                  CREATE TABLE dbo.GRUP_KARTI(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    DONEM_ADI VARCHAR(200) NULL,
                    DONEM_YILI VARCHAR(6) NULL,
                    DONEM_AYI VARCHAR(50) NULL,
                    DONEM_SUBESI VARCHAR(50) NULL,
                    DONEM_GRUBU VARCHAR(50) NULL,
                    BAS_TAR DATE NULL,
                    BIT_TAR DATE NULL
                  );",

                @"IF OBJECT_ID('dbo.DONEMLER','U') IS NULL
                  CREATE TABLE dbo.DONEMLER(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    DONEM_ADI VARCHAR(200) NULL
                  );",

                @"IF OBJECT_ID('dbo.PERSONEL','U') IS NULL
                  CREATE TABLE dbo.PERSONEL(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ADI VARCHAR(50) NULL,
                    SOYADI VARCHAR(50) NULL,
                    TC_NO VARCHAR(11) NULL
                  );",

                @"IF OBJECT_ID('dbo.AracParam','U') IS NULL
                  CREATE TABLE dbo.AracParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ARAC_TIPI VARCHAR(150) NULL,
                    ARAC_PLAKA VARCHAR(50) NULL,
                    DURUMU VARCHAR(50) NULL,
                    MARKASI VARCHAR(100) NULL,
                    RENGI VARCHAR(50) NULL,
                    VITES_TURU VARCHAR(50) NULL,
                    MODEL VARCHAR(100) NULL,
                    ARAC_TESCIL_TAR DATE NULL,
                    HIZ_BAS_TAR DATE NULL,
                    MUHAYENE_TAR DATETIME NULL,
                    SIGORTA_BEL_NO VARCHAR(100) NULL,
                    AKT INT NULL
                  );",
                @"IF OBJECT_ID('dbo.AracParam','U') IS NOT NULL
                  BEGIN
                    IF COL_LENGTH('dbo.AracParam','DURUMU') IS NULL ALTER TABLE dbo.AracParam ADD DURUMU VARCHAR(50) NULL;
                    IF COL_LENGTH('dbo.AracParam','MARKASI') IS NULL ALTER TABLE dbo.AracParam ADD MARKASI VARCHAR(100) NULL;
                    IF COL_LENGTH('dbo.AracParam','RENGI') IS NULL ALTER TABLE dbo.AracParam ADD RENGI VARCHAR(50) NULL;
                    IF COL_LENGTH('dbo.AracParam','VITES_TURU') IS NULL ALTER TABLE dbo.AracParam ADD VITES_TURU VARCHAR(50) NULL;
                    IF COL_LENGTH('dbo.AracParam','MODEL') IS NULL ALTER TABLE dbo.AracParam ADD MODEL VARCHAR(100) NULL;
                    IF COL_LENGTH('dbo.AracParam','ARAC_TESCIL_TAR') IS NULL ALTER TABLE dbo.AracParam ADD ARAC_TESCIL_TAR DATE NULL;
                    IF COL_LENGTH('dbo.AracParam','HIZ_BAS_TAR') IS NULL ALTER TABLE dbo.AracParam ADD HIZ_BAS_TAR DATE NULL;
                    IF COL_LENGTH('dbo.AracParam','MUHAYENE_TAR') IS NULL ALTER TABLE dbo.AracParam ADD MUHAYENE_TAR DATETIME NULL;
                    IF COL_LENGTH('dbo.AracParam','SIGORTA_BEL_NO') IS NULL ALTER TABLE dbo.AracParam ADD SIGORTA_BEL_NO VARCHAR(100) NULL;
                    IF COL_LENGTH('dbo.AracParam','AKT') IS NULL ALTER TABLE dbo.AracParam ADD AKT INT NULL;
                  END;",

                @"IF OBJECT_ID('dbo.KursBilgiParam','U') IS NULL
                  CREATE TABLE dbo.KursBilgiParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    KURS_ADI VARCHAR(150) NULL,
                    KURUM_KODU VARCHAR(50) NULL,
                    ADRES VARCHAR(500) NULL,
                    ILCE VARCHAR(50) NULL,
                    IL VARCHAR(40) NULL,
                    TELEFON VARCHAR(40) NULL,
                    GSM VARCHAR(40) NULL,
                    MUSTERI_NO VARCHAR(20) NULL
                  );",

                @"IF OBJECT_ID('dbo.KursBilgiParam','U') IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM dbo.KursBilgiParam)
                  INSERT INTO dbo.KursBilgiParam (KURS_ADI, KURUM_KODU, ADRES, ILCE, IL, TELEFON, GSM, MUSTERI_NO)
                  VALUES (N'KOLERA MTSK', N'1234', N'', N'', N'', N'', N'', N'1234');",

                @"IF OBJECT_ID('dbo.GenelParam','U') IS NULL
                  CREATE TABLE dbo.GenelParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    MEBBIS_KUL_ADI_1 VARCHAR(100) NULL,
                    MEBBIS_KUL_SIF_1 VARCHAR(100) NULL,
                    MEBBIS_KUL_YET_1 VARCHAR(MAX) NULL,
                    MEBBIS_KUL_ADI_2 VARCHAR(100) NULL,
                    MEBBIS_KUL_SIF_2 VARCHAR(100) NULL,
                    MEBBIS_KUL_YET_2 VARCHAR(MAX) NULL,
                    MEBBIS_KUL_ADI_3 VARCHAR(100) NULL,
                    MEBBIS_KUL_SIF_3 VARCHAR(100) NULL,
                    MEBBIS_KUL_YET_3 VARCHAR(MAX) NULL,
                    MEBBIS_KUL_ADI_4 VARCHAR(100) NULL,
                    MEBBIS_KUL_SIF_4 VARCHAR(100) NULL,
                    MEBBIS_KUL_YET_4 VARCHAR(MAX) NULL,
                    MEBBIS_KUL_ADI_5 VARCHAR(100) NULL,
                    MEBBIS_KUL_SIF_5 VARCHAR(100) NULL,
                    MEBBIS_KUL_YET_5 VARCHAR(MAX) NULL
                  );",
                @"IF OBJECT_ID('dbo.GenelParam','U') IS NOT NULL
                  BEGIN
                    IF COL_LENGTH('dbo.GenelParam','MEBBIS_KUL_ADI_5') IS NULL ALTER TABLE dbo.GenelParam ADD MEBBIS_KUL_ADI_5 VARCHAR(100) NULL;
                    IF COL_LENGTH('dbo.GenelParam','MEBBIS_KUL_SIF_5') IS NULL ALTER TABLE dbo.GenelParam ADD MEBBIS_KUL_SIF_5 VARCHAR(100) NULL;
                    IF COL_LENGTH('dbo.GenelParam','MEBBIS_KUL_YET_5') IS NULL ALTER TABLE dbo.GenelParam ADD MEBBIS_KUL_YET_5 VARCHAR(MAX) NULL;
                  END;",

                @"IF OBJECT_ID('dbo.SinifParam','U') IS NULL
                  CREATE TABLE dbo.SinifParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    SINIF_MEVCUT VARCHAR(30) NULL,
                    SINIF_YENI VARCHAR(30) NULL,
                    SINIF_TEORI_UCRETI MONEY NULL,
                    SINIF_DRKS_UCRETI MONEY NULL
                  );",

                @"IF OBJECT_ID('dbo.APP_LOCAL_LISANS','U') IS NULL
                  CREATE TABLE dbo.APP_LOCAL_LISANS(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    LISANS_NO NVARCHAR(150) NULL,
                    UPDATED_AT DATETIME NULL
                  );",

                @"IF OBJECT_ID('dbo.RAPOR_TANIMLARI','U') IS NULL
                  CREATE TABLE dbo.RAPOR_TANIMLARI(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    RAPOR_ADI NVARCHAR(200) NULL,
                    RAPOR_ICERIK NVARCHAR(MAX) NULL
                  );",

                @"IF OBJECT_ID('dbo.SMSSABLONLARI','U') IS NULL
                  CREATE TABLE dbo.SMSSABLONLARI(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    SABLON_ADI NVARCHAR(200) NOT NULL,
                    SABLON_METNI NVARCHAR(MAX) NOT NULL,
                    SIRA_NO INT NOT NULL CONSTRAINT DF_SMSSABLONLARI_SIRA DEFAULT(0),
                    AKTIF BIT NOT NULL CONSTRAINT DF_SMSSABLONLARI_AKTIF DEFAULT(1),
                    OLUSTURMA_TARIHI DATETIME2(7) NOT NULL CONSTRAINT DF_SMSSABLONLARI_OLUSTUR DEFAULT(SYSUTCDATETIME()),
                    GUNCELLEME_TARIHI DATETIME2(7) NULL
                  );
                  IF OBJECT_ID('dbo.SMSSABLONLARI','U') IS NOT NULL
                  BEGIN
                    IF NOT EXISTS (SELECT 1 FROM dbo.SMSSABLONLARI WHERE SABLON_ADI = N'Kursiyer - direksiyon bilgi')
                      INSERT INTO dbo.SMSSABLONLARI (SABLON_ADI, SABLON_METNI, SIRA_NO, AKTIF)
                      VALUES (N'Kursiyer - direksiyon bilgi', N'SAYIN [AD SOYAD]; DIREKSIYON SINAV BILGILERI ICIN [KURS ADI] KURSUMUZU ARAYINIZ. Tel: [TELEFON]', 1, 1);
                    IF NOT EXISTS (SELECT 1 FROM dbo.SMSSABLONLARI WHERE SABLON_ADI = N'E-sinav hatirlatma')
                      INSERT INTO dbo.SMSSABLONLARI (SABLON_ADI, SABLON_METNI, SIRA_NO, AKTIF)
                      VALUES (N'E-sinav hatirlatma', N'SAYIN [AD SOYAD]; [TARIH] SAAT:[SAAT] E-SINAVINIZ VARDIR. SINAV SAATINDEN ONCE KIMLIGINIZLE HAZIR BULUNUNUZ. [KURS ADI] [TELEFON]', 2, 1);
                    IF NOT EXISTS (SELECT 1 FROM dbo.SMSSABLONLARI WHERE SABLON_ADI = N'Direksiyon sinav hatirlatma')
                      INSERT INTO dbo.SMSSABLONLARI (SABLON_ADI, SABLON_METNI, SIRA_NO, AKTIF)
                      VALUES (N'Direksiyon sinav hatirlatma', N'SAYIN [AD SOYAD]; [TARIH] YARIN SAAT:[SAAT] DIREKSIYON SINAVINIZ VARDIR. SINAV SAATINDEN YARIM SAAT ONCE DIREKSIYON EGITIM PISTINDE KIMLIGINIZLE BULUNMANIZ ONEMLE RICA OLUNUR. [KURS ADI] [TELEFON]', 3, 1);
                  END",

                @"IF OBJECT_ID('dbo.SertifikaSinifParam','U') IS NULL
                  CREATE TABLE dbo.SertifikaSinifParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    MEVCUT_SINIF VARCHAR(20) NULL,
                    YENI_SINIF VARCHAR(20) NULL,
                    UCRET MONEY NULL,
                    TEORI_HARC MONEY NULL,
                    DRKS_HARC MONEY NULL
                  );",

                @"IF OBJECT_ID('dbo.SertifikaUcretParam','U') IS NULL
                  CREATE TABLE dbo.SertifikaUcretParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    ID_SERTIFIKA INT NULL,
                    UC_SINIF VARCHAR(30) NULL,
                    UC_DONEM_ADI VARCHAR(40) NULL,
                    UC_DONEM_YILI VARCHAR(15) NULL,
                    UC_TEORIK_1_SAAT MONEY NULL,
                    UC_DIREKS_1_SAAT MONEY NULL,
                    UC_TOPLAM_DERS_SA INT NULL,
                    UC_ACIKLAMA VARCHAR(300) NULL,
                    UC_SINIF_ONC VARCHAR(30) NULL
                  );",

                @"IF OBJECT_ID('dbo.DurumParam','U') IS NULL
                  CREATE TABLE dbo.DurumParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    KOD INT NOT NULL,
                    ACIKLAMA VARCHAR(20) NULL
                  );",

                @"IF OBJECT_ID('dbo.IlParam','U') IS NULL
                  CREATE TABLE dbo.IlParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    IL_KODU INT NULL,
                    IL_ADI VARCHAR(150) NULL
                  );",

                @"IF OBJECT_ID('dbo.IlceParam','U') IS NULL
                  CREATE TABLE dbo.IlceParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    IL_KODU INT NULL,
                    ILCE_ADI VARCHAR(150) NULL
                  );",

                @"IF OBJECT_ID('dbo.SettingsParam','U') IS NULL
                  CREATE TABLE dbo.SettingsParam(
                    ID INT IDENTITY(1,1) PRIMARY KEY,
                    LSN_KURUM_KODU VARCHAR(80) NULL,
                    LSN_LISANS_NO VARCHAR(80) NULL,
                    LSN_BITIS_TARIHI VARCHAR(10) NULL
                  );",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_KULLANICI
                    @ISLEM CHAR(1),
                    @ID INT = NULL,
                    @KULLANICI_ADI NVARCHAR(100) = NULL,
                    @PAROLA NVARCHAR(100) = NULL,
                    @YETKI NVARCHAR(100) = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    IF @ISLEM = 'S'
                      SELECT ID, KULLANICI_ADI, YETKI, KAYIT_TARIHI FROM KULLANICI ORDER BY KULLANICI_ADI;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_KURSIYER_ESINAVI
                    @ID_KURSIYER INT
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    SELECT ID AS TEORISNV_ID, ID_KURSIYER, E_SINAV_TARIHI AS ESINAV_TARIHI, TEO_NOT, TEO_HAK,
                           CASE WHEN TRY_CONVERT(INT, TEO_NOT) >= 70 THEN 'GEÇTİ'
                                WHEN TRY_CONVERT(INT, TEO_NOT) < 70 THEN 'KALDI'
                                ELSE 'GİRMEDİ' END AS TEO_DURUM,
                           E_SINAV_SAATI, E_SINAV_YERI, E_SINAV_ACIKLAMA
                    FROM SINAV_LISTE_TEORI
                    WHERE ID_KURSIYER = @ID_KURSIYER
                    ORDER BY TEO_HAK ASC;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_ESINAV_TARIHLERI_LISTELE
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    SELECT ID, SINAV_TARIHI,
                           CASE WHEN ISNULL(SINAV_DURUMU,'') IN ('1','Hazır') OR ISNULL(DURUM,'') IN ('1','Hazır')
                                THEN 'Hazır' ELSE 'Hazır Değil' END AS DURUM_TEXT,
                           ISNULL(ACIKLAMA, ISNULL(SINAV_ACIKLAMA, '')) AS ACIKLAMA
                    FROM SINAV_TARIHLERI
                    ORDER BY SINAV_TARIHI DESC, ID DESC;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_ARACLAR
                    @ISLEM CHAR(1),
                    @ID INT = NULL,
                    @ARAC_TIPI NVARCHAR(150) = NULL,
                    @ARAC_PLAKA NVARCHAR(50) = NULL,
                    @DURUMU NVARCHAR(20) = NULL,
                    @RENGI NVARCHAR(20) = NULL,
                    @VITES_TURU NVARCHAR(20) = NULL,
                    @MODEL NVARCHAR(50) = NULL,
                    @MUHAYENE_TAR DATETIME = NULL,
                    @AKT INT = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    IF @ISLEM = 'S'
                      SELECT * FROM AracParam WHERE (@ID IS NULL OR ID = @ID);
                    ELSE IF @ISLEM = 'I'
                      INSERT INTO AracParam (ARAC_TIPI,ARAC_PLAKA,DURUMU,RENGI,VITES_TURU,MODEL,MUHAYENE_TAR,AKT)
                      VALUES (@ARAC_TIPI,@ARAC_PLAKA,@DURUMU,@RENGI,@VITES_TURU,@MODEL,@MUHAYENE_TAR,@AKT);
                    ELSE IF @ISLEM = 'U'
                      UPDATE AracParam SET ARAC_TIPI=@ARAC_TIPI,ARAC_PLAKA=@ARAC_PLAKA,DURUMU=@DURUMU,RENGI=@RENGI,VITES_TURU=@VITES_TURU,MODEL=@MODEL,MUHAYENE_TAR=@MUHAYENE_TAR,AKT=@AKT WHERE ID=@ID;
                    ELSE IF @ISLEM = 'D'
                      DELETE FROM AracParam WHERE ID=@ID;
                    ELSE IF @ISLEM = 'P'
                      UPDATE AracParam SET AKT=0, DURUMU='Pasif' WHERE ID=@ID;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_PERSONELLER
                    @ISLEM CHAR(1),
                    @ID INT = NULL,
                    @TC_NO VARCHAR(11) = NULL,
                    @ADI VARCHAR(50) = NULL,
                    @SOYADI VARCHAR(50) = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    IF @ISLEM = 'S'
                      SELECT * FROM PERSONEL WHERE (@ID IS NULL OR ID = @ID) ORDER BY ADI ASC;
                    ELSE IF @ISLEM = 'I'
                    BEGIN
                      INSERT INTO PERSONEL (TC_NO,ADI,SOYADI) VALUES (@TC_NO,@ADI,@SOYADI);
                      SELECT SCOPE_IDENTITY();
                    END
                    ELSE IF @ISLEM = 'U'
                      UPDATE PERSONEL SET TC_NO=@TC_NO, ADI=@ADI, SOYADI=@SOYADI WHERE ID=@ID;
                    ELSE IF @ISLEM = 'D'
                      DELETE FROM PERSONEL WHERE ID=@ID;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_KURSIYER
                    @ISLEM_TIPI CHAR(1),
                    @ID INT = NULL,
                    @ADI NVARCHAR(200) = NULL,
                    @SOYADI NVARCHAR(200) = NULL,
                    @TC_NO NVARCHAR(20) = NULL,
                    @KIM_KAYIT_NO NVARCHAR(50) = NULL,
                    @ID_GRUP_KARTI INT = NULL,
                    @SERTIFIKA_SINIFI NVARCHAR(50) = NULL,
                    @ONCE_SERT_SINIFI NVARCHAR(50) = NULL,
                    @ONCE_SERT_BELGESAYI NVARCHAR(50) = NULL,
                    @ADAY_NO INT = NULL,
                    @KURSIYER_DURUMU INT = NULL,
                    @DOGUM_TARIHI DATETIME = NULL,
                    @KAYIT_TARIHI DATETIME = NULL,
                    @RESIM VARBINARY(MAX) = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    IF @ISLEM_TIPI='I'
                    BEGIN
                      INSERT INTO KURSIYER
                      (ADI,SOYADI,TC_NO,KIMLIK_KAYIT_NO,ID_GRUP_KARTI,SERTIFIKA_SINIFI,ONCE_SERT_SINIFI,ONCE_SERT_BELGESAYI,ADAY_NO,KURSIYER_DURUMU,DOGUM_TARIHI,KAYIT_TARIHI,RESIM)
                      VALUES
                      (@ADI,@SOYADI,@TC_NO,@KIM_KAYIT_NO,@ID_GRUP_KARTI,@SERTIFIKA_SINIFI,@ONCE_SERT_SINIFI,@ONCE_SERT_BELGESAYI,@ADAY_NO,ISNULL(@KURSIYER_DURUMU,1),@DOGUM_TARIHI,ISNULL(@KAYIT_TARIHI,GETDATE()),@RESIM);
                      SELECT CAST(SCOPE_IDENTITY() AS INT);
                    END
                    ELSE IF @ISLEM_TIPI='U'
                    BEGIN
                      UPDATE KURSIYER SET
                        ADI=@ADI,
                        SOYADI=@SOYADI,
                        TC_NO=@TC_NO,
                        KIMLIK_KAYIT_NO=@KIM_KAYIT_NO,
                        ID_GRUP_KARTI=@ID_GRUP_KARTI,
                        SERTIFIKA_SINIFI=@SERTIFIKA_SINIFI,
                        ONCE_SERT_SINIFI=@ONCE_SERT_SINIFI,
                        ONCE_SERT_BELGESAYI=@ONCE_SERT_BELGESAYI,
                        ADAY_NO=@ADAY_NO,
                        KURSIYER_DURUMU=@KURSIYER_DURUMU,
                        DOGUM_TARIHI=@DOGUM_TARIHI,
                        KAYIT_TARIHI=@KAYIT_TARIHI,
                        RESIM=@RESIM
                      WHERE ID=@ID;
                      SELECT @ID;
                    END
                    ELSE IF @ISLEM_TIPI='D'
                      DELETE FROM KURSIYER WHERE ID=@ID;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_KURSIYER_DIREKSIYONSINAVI
                    @ID_KURSIYER INT
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    SELECT d.ID, st.SINAV_TARIHI, ISNULL(d.RANDEVU_SAATI,'') AS RANDEVU_SAATI,
                           ISNULL(d.DIR_HAK,0) AS DIR_HAK, ISNULL(d.DIR_DURUM,'GIRMEDI') AS DIR_DURUM
                    FROM SINAV_LISTE_DIREKSIYON d
                    INNER JOIN SINAV_TARIHLERI st ON st.ID = d.ID_SINAV_TARIHI
                    WHERE d.ID_KURSIYER = @ID_KURSIYER
                    ORDER BY st.SINAV_TARIHI DESC;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_DONEMGRUP
                    @ISLEM CHAR(1),
                    @ID INT = NULL,
                    @YIL INT = NULL,
                    @AY NVARCHAR(20) = NULL,
                    @SUBE NVARCHAR(100) = NULL,
                    @DONEM_ADI NVARCHAR(150) = NULL,
                    @GRUP_ADI NVARCHAR(100) = NULL,
                    @BAS_TAR DATE = NULL,
                    @BIT_TAR DATE = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    IF @ISLEM='L' SELECT * FROM GRUP_KARTI ORDER BY BAS_TAR DESC;
                    ELSE IF @ISLEM='I' INSERT INTO GRUP_KARTI (DONEM_YILI,DONEM_AYI,DONEM_SUBESI,DONEM_ADI,DONEM_GRUBU,BAS_TAR,BIT_TAR) VALUES (@YIL,@AY,@SUBE,@DONEM_ADI,@GRUP_ADI,@BAS_TAR,@BIT_TAR);
                    ELSE IF @ISLEM='U' UPDATE GRUP_KARTI SET DONEM_YILI=@YIL,DONEM_AYI=@AY,DONEM_SUBESI=@SUBE,DONEM_ADI=@DONEM_ADI,DONEM_GRUBU=@GRUP_ADI,BAS_TAR=@BAS_TAR,BIT_TAR=@BIT_TAR WHERE ID=@ID;
                    ELSE IF @ISLEM='D' DELETE FROM GRUP_KARTI WHERE ID=@ID;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_GRUPLAR
                    @IslemTipi NVARCHAR(10),
                    @Id INT = NULL,
                    @Yil INT = NULL,
                    @Ay NVARCHAR(20) = NULL,
                    @Sube NVARCHAR(50) = NULL,
                    @DonemAdi NVARCHAR(100) = NULL,
                    @GrupAdi NVARCHAR(50) = NULL,
                    @Baslangic DATETIME = NULL,
                    @Bitis DATETIME = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    IF @IslemTipi='INSERT'
                      INSERT INTO GRUP_KARTI (DONEM_YILI,DONEM_AYI,DONEM_SUBESI,DONEM_ADI,DONEM_GRUBU,BAS_TAR,BIT_TAR) VALUES (@Yil,@Ay,@Sube,@DonemAdi,@GrupAdi,@Baslangic,@Bitis);
                    ELSE IF @IslemTipi='UPDATE'
                      UPDATE GRUP_KARTI SET DONEM_YILI=@Yil,DONEM_AYI=@Ay,DONEM_SUBESI=@Sube,DONEM_ADI=@DonemAdi,DONEM_GRUBU=@GrupAdi,BAS_TAR=@Baslangic,BIT_TAR=@Bitis WHERE ID=@Id;
                    ELSE IF @IslemTipi='DELETE'
                      DELETE FROM GRUP_KARTI WHERE ID=@Id;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_UPSERT_KURSIYER_EVRAK
                    @ID_KURSIYER INT,
                    @EKSIK_OGRNIM_BEL VARCHAR(5) = NULL,
                    @EKSIK_SAGLIK VARCHAR(5) = NULL,
                    @EKSIK_SAVCILIK VARCHAR(5) = NULL,
                    @EKSIK_SOZLESME VARCHAR(5) = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    IF EXISTS (SELECT 1 FROM KURSIYER_EVRAK WHERE ID_KURSIYER=@ID_KURSIYER)
                      UPDATE KURSIYER_EVRAK SET EKSIK_OGRNIM_BEL=@EKSIK_OGRNIM_BEL,EKSIK_SAGLIK=@EKSIK_SAGLIK,EKSIK_SAVCILIK=@EKSIK_SAVCILIK,EKSIK_SOZLESME=@EKSIK_SOZLESME WHERE ID_KURSIYER=@ID_KURSIYER;
                    ELSE
                      INSERT INTO KURSIYER_EVRAK (ID_KURSIYER,EKSIK_OGRNIM_BEL,EKSIK_SAGLIK,EKSIK_SAVCILIK,EKSIK_SOZLESME)
                      VALUES (@ID_KURSIYER,@EKSIK_OGRNIM_BEL,@EKSIK_SAGLIK,@EKSIK_SAVCILIK,@EKSIK_SOZLESME);
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.UPSERT_KURSIYER_EVRAK
                    @ID_KURSIYER INT,
                    @EKSIK_OGRNIM_BEL VARCHAR(5) = NULL,
                    @EKSIK_SAGLIK VARCHAR(5) = NULL,
                    @EKSIK_SAVCILIK VARCHAR(5) = NULL,
                    @EKSIK_SOZLESME VARCHAR(5) = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    EXEC dbo.SP_KOLERA_UPSERT_KURSIYER_EVRAK
                        @ID_KURSIYER=@ID_KURSIYER,
                        @EKSIK_OGRNIM_BEL=@EKSIK_OGRNIM_BEL,
                        @EKSIK_SAGLIK=@EKSIK_SAGLIK,
                        @EKSIK_SAVCILIK=@EKSIK_SAVCILIK,
                        @EKSIK_SOZLESME=@EKSIK_SOZLESME;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_UPSERT_KURSIYER_EVRAK
                    @ID_KURSIYER INT,
                    @EKSIK_OGRNIM_BEL VARCHAR(5) = NULL,
                    @EKSIK_SAGLIK VARCHAR(5) = NULL,
                    @EKSIK_SAVCILIK VARCHAR(5) = NULL,
                    @EKSIK_SOZLESME VARCHAR(5) = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    EXEC dbo.SP_KOLERA_UPSERT_KURSIYER_EVRAK
                        @ID_KURSIYER=@ID_KURSIYER,
                        @EKSIK_OGRNIM_BEL=@EKSIK_OGRNIM_BEL,
                        @EKSIK_SAGLIK=@EKSIK_SAGLIK,
                        @EKSIK_SAVCILIK=@EKSIK_SAVCILIK,
                        @EKSIK_SOZLESME=@EKSIK_SOZLESME;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_DIREKSIYON_SINAVI_HAZIRLA
                    @ISLEM CHAR(1),
                    @SINAV_ID INT = NULL,
                    @KURSIYER_ID INT = NULL,
                    @ID INT = NULL,
                    @PERSONEL_ID INT = NULL,
                    @ARAC_ID INT = NULL,
                    @SAAT VARCHAR(20) = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    IF @ISLEM='L'
                      SELECT * FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@SINAV_ID;
                    ELSE IF @ISLEM='E'
                    BEGIN
                      IF NOT EXISTS (SELECT 1 FROM SINAV_LISTE_DIREKSIYON WHERE ID_SINAV_TARIHI=@SINAV_ID AND ID_KURSIYER=@KURSIYER_ID)
                        INSERT INTO SINAV_LISTE_DIREKSIYON (ID_SINAV_TARIHI,ID_KURSIYER,DIR_HAK,DIR_DURUM) VALUES (@SINAV_ID,@KURSIYER_ID,0,'GIRMEDI');
                    END
                    ELSE IF @ISLEM='G'
                      UPDATE SINAV_LISTE_DIREKSIYON SET ID_PERSONEL=@PERSONEL_ID,ID_ARAC=@ARAC_ID,RANDEVU_SAATI=@SAAT WHERE ID=@ID;
                    ELSE IF @ISLEM='D'
                      DELETE FROM SINAV_LISTE_DIREKSIYON WHERE ID=@ID;
                  END;",

                @"CREATE OR ALTER PROCEDURE dbo.SP_KOLERA_MEBBISAKTAR
                    @ID_KURSIYER INT = NULL
                  AS
                  BEGIN
                    SET NOCOUNT ON;
                    SELECT k.*, e.OGRNM_BEL_TURU, e.SAG_RAP_BELGENO, e.CriminalNo
                    FROM KURSIYER k
                    LEFT JOIN KURSIYER_EVRAK e ON e.ID_KURSIYER = k.ID
                    WHERE (@ID_KURSIYER IS NULL OR k.ID=@ID_KURSIYER);
                  END;"
            };
        }

        private List<string> CheckMissingObjects(string targetDb)
        {
            var eksikler = new List<string>();
            using (var con = new SqlConnection(BuildConnectionString(targetDb)))
            {
                con.Open();

                foreach (var table in GetRequiredTables())
                {
                    if (!ObjectExists(con, table, "U"))
                        eksikler.Add("TABLE: " + table);
                }

                foreach (var sp in GetRequiredProcedures())
                {
                    if (!ObjectExists(con, sp, "P"))
                        eksikler.Add("PROC: " + sp);
                }
            }
            return eksikler;
        }

        private bool ObjectExists(SqlConnection con, string name, string type)
        {
            using (var cmd = new SqlCommand("SELECT 1 FROM sys.objects WHERE name=@n AND type=@t", con))
            {
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@t", type);
                return cmd.ExecuteScalar() != null;
            }
        }

        private List<string> GetRequiredTables()
        {
            return new List<string>
            {
                "KULLANICI",
                "KURSIYER",
                "KURSIYERLER",
                "SINAV_TARIHLERI",
                "SINAV_LISTE_TEORI",
                "SINAV_LISTE_DIREKSIYON",
                "KURSIYER_EVRAK",
                "DONEMLER",
                "KursBilgiParam",
                "GenelParam",
                "SinifParam",
                "SertifikaSinifParam",
                "SertifikaUcretParam",
                "DurumParam",
                "IlParam",
                "IlceParam",
                "SettingsParam",
                "APP_LOCAL_LISANS",
                "RAPOR_TANIMLARI",
                "SMSSABLONLARI"
            };
        }

        private List<string> GetRequiredProcedures()
        {
            return new List<string>
            {
                "SP_KOLERA_KULLANICI",
                "SP_KOLERA_KURSIYER_ESINAVI",
                "SP_KOLERA_ESINAV_TARIHLERI_LISTELE",
                "SP_KOLERA_ARACLAR",
                "SP_KOLERA_PERSONELLER",
                "SP_KOLERA_KURSIYER",
                "SP_KOLERA_KURSIYER_DIREKSIYONSINAVI",
                "SP_KOLERA_DONEMGRUP",
                "SP_KOLERA_GRUPLAR",
                "SP_KOLERA_UPSERT_KURSIYER_EVRAK",
                "UPSERT_KURSIYER_EVRAK",
                "SP_UPSERT_KURSIYER_EVRAK",
                "SP_KOLERA_DIREKSIYON_SINAVI_HAZIRLA",
                "SP_KOLERA_MEBBISAKTAR"
            };
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

        private void SetBusy(bool busy, string durum)
        {
            InvokeUi(() =>
            {
                btnDbleriListele.Enabled = !busy;
                btnSemaKur.Enabled = !busy;
                btnKopyala.Enabled = !busy;
                btnBacSec.Enabled = !busy;
                lblDurum.Text = durum;
                lblDurum.ForeColor = busy ? Color.DarkOrange : Color.DarkGreen;
            });
        }

        private void UpdateProgress(string tableName)
        {
            InvokeUi(() =>
            {
                if (progressBar.Value < progressBar.Maximum)
                    progressBar.Value += 1;
                lblDurum.Text = "İşleniyor: " + tableName;
                int yuzde = progressBar.Maximum > 0
                    ? (int)Math.Round((progressBar.Value * 100.0) / progressBar.Maximum)
                    : 0;
                lblYuzde.Text = "%" + yuzde;
            });
        }

        private void MarkTable(string tableName, bool success)
        {
            InvokeUi(() =>
            {
                int index = chkKopyalananlar.Items.IndexOf(tableName);
                if (index >= 0)
                {
                    chkKopyalananlar.SetItemChecked(index, success);
                    if (!success)
                        chkKopyalananlar.Items[index] = tableName + " (Hata)";
                }
                chkKopyalananlar.TopIndex = Math.Max(0, chkKopyalananlar.Items.Count - 1);
            });
        }

        private void Log(string message)
        {
            InvokeUi(() =>
            {
                lblDurum.Text = DateTime.Now.ToString("HH:mm:ss") + "  " + message;
            });
        }

        private void InvokeUi(Action action)
        {
            if (InvokeRequired) Invoke(action);
            else action();
        }
    }
}
