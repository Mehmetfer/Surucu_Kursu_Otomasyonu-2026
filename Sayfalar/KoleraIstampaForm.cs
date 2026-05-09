using Kolera_Mtsk.Services;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class KoleraIstampaForm : Form
    {
        private readonly string _connectionString;
        private readonly KoleraIstampaRepository _repo;
        private byte[] _aktifResim;
        public byte[] SecilenIstampaResmi { get; private set; }

        public KoleraIstampaForm(string connectionString)
        {
            _connectionString = connectionString ?? string.Empty;
            _repo = new KoleraIstampaRepository(_connectionString);

            InitializeComponent();
            Load += KoleraIstampaForm_Load;
            Shown += KoleraIstampaForm_Shown;
        }

        private void KoleraIstampaForm_Load(object sender, EventArgs e)
        {
            // Bazi ortamlarda Shown once data-binding tetiklenmeyebiliyor; yuklemeyi garantiye al.
            if (_cmbAlan.Items.Count == 0)
                YukleAlanlar();
        }

        private void KoleraIstampaForm_Shown(object sender, EventArgs e)
        {
            YukleAlanlar();
        }

        private void BtnIstampaYap_Click(object sender, EventArgs e)
        {
            if (_aktifResim == null || _aktifResim.Length == 0)
            {
                MessageBox.Show("Once bir istampa resmi seciniz.", "Uyari", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SecilenIstampaResmi = _aktifResim;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void YukleAlanlar()
        {
            DataTable dt = _repo.GetAll();
            if (dt == null)
                dt = new DataTable("KOLERA_ISTAMPA");

            if (dt.Columns.Count == 0)
            {
                dt.Columns.Add("ALAN_KODU", typeof(string));
                dt.Columns.Add("ALAN_ADI", typeof(string));
                dt.Columns.Add("RESIM", typeof(byte[]));
                dt.Columns.Add("ACIKLAMA", typeof(string));
            }

            if (dt.Rows.Count == 0)
            {
                foreach (var kv in KoleraIstampaRepository.GetDefaultDefinitions())
                {
                    var r = dt.NewRow();
                    r["ALAN_KODU"] = kv.Key;
                    r["ALAN_ADI"] = kv.Value;
                    r["ACIKLAMA"] = string.Empty;
                    dt.Rows.Add(r);
                }
            }

            // ALAN_ADI bos gelen satir olursa listede gorunsun
            int ordAd = KoleraIstampaRepository.FindColumnOrdinal(dt, "ALAN_ADI");
            int ordKod = KoleraIstampaRepository.FindColumnOrdinal(dt, "ALAN_KODU");
            if (ordAd >= 0 && ordKod >= 0)
            {
                foreach (DataRow row in dt.Rows.Cast<DataRow>())
                {
                    if (row.RowState == DataRowState.Deleted)
                        continue;
                    if (string.IsNullOrWhiteSpace(Convert.ToString(row[ordAd])))
                        row[ordAd] = Convert.ToString(row[ordKod]);
                }
            }

            _cmbAlan.DataSource = dt;
            _cmbAlan.DisplayMember = "ALAN_ADI";
            _cmbAlan.ValueMember = "ALAN_KODU";
            if (_cmbAlan.Items.Count > 0)
                _cmbAlan.SelectedIndex = 0;
        }

        private void YukleSeciliAlan()
        {
            string kod = Convert.ToString(_cmbAlan.SelectedValue) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kod))
                return;

            var dt = _cmbAlan.DataSource as DataTable;
            if (!KoleraIstampaRepository.TryFindStampRow(dt, kod, out DataRow row))
                return;
            if (row.RowState == DataRowState.Detached)
                return;

            _txtAciklama.Text = Convert.ToString(KoleraIstampaRepository.GetColumnValue(row, "ACIKLAMA")) ?? string.Empty;
            object res = KoleraIstampaRepository.GetColumnValue(row, "RESIM");
            _aktifResim = (res == null || res == DBNull.Value) ? null : res as byte[];
            SetPicture(_aktifResim);
        }

        private void YukleSeciliAlan(object sender, EventArgs e)
        {
            YukleSeciliAlan();
        }

        private void BtnTara_Click(object sender, EventArgs e)
        {
            var tarama = new Tarama_Sayfam(_aktifResim, Tarama_Sayfam.TaramaTipi.Imza, _connectionString);
            tarama.TaramaTamamlandi += bytes =>
            {
                _aktifResim = bytes;
                SetPicture(_aktifResim);
            };
            tarama.ShowDialog(this);
        }

        private void BtnDosya_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Resim Dosyalari|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                _aktifResim = File.ReadAllBytes(ofd.FileName);
                SetPicture(_aktifResim);
            }
        }

        private void BtnKaydet_Click(object sender, EventArgs e)
        {
            string kod = Convert.ToString(_cmbAlan.SelectedValue) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(kod))
            {
                MessageBox.Show("Gecerli bir istampa alani seciniz.", "Uyari",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string ad = Convert.ToString(_cmbAlan.Text) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ad) && KoleraIstampaRepository.GetDefaultDefinitions().TryGetValue(kod, out string varsayilanAd))
                ad = varsayilanAd;

            if (_aktifResim == null || _aktifResim.Length == 0)
            {
                MessageBox.Show("Once tarama veya dosyadan bir istampa resmi yukleyin.", "Uyari",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_repo.SaveImage(kod, ad, _aktifResim, _txtAciklama.Text, out string hata))
            {
                MessageBox.Show(string.IsNullOrEmpty(hata) ? "Kayit basarisiz." : hata, "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Istampa KOLERA_ISTAMPA tablosuna kaydedildi.", "Bilgi",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            YukleAlanlar();
            SelectByCode(kod);
        }

        private void SelectByCode(string code)
        {
            if (_cmbAlan.DataSource == null)
                return;
            string hedef = (code ?? string.Empty).Trim().ToUpperInvariant();
            for (int i = 0; i < _cmbAlan.Items.Count; i++)
            {
                var drv = _cmbAlan.Items[i] as DataRowView;
                if (drv?.Row == null || drv.Row.RowState == DataRowState.Detached)
                    continue;
                string k = Convert.ToString(KoleraIstampaRepository.GetColumnValue(drv.Row, "ALAN_KODU")) ?? string.Empty;
                if (string.Equals(k.Trim().ToUpperInvariant(), hedef, StringComparison.Ordinal))
                {
                    _cmbAlan.SelectedIndex = i;
                    return;
                }
            }
        }

        private void SetPicture(byte[] data)
        {
            try
            {
                if (_pic.Image != null)
                {
                    var old = _pic.Image;
                    _pic.Image = null;
                    old.Dispose();
                }

                if (data == null || data.Length == 0)
                {
                    _lblBoyut.Text = "Resim yok";
                    return;
                }

                using (var ms = new MemoryStream(data))
                using (var img = Image.FromStream(ms))
                    _pic.Image = new Bitmap(img);

                _lblBoyut.Text = "Boyut: " + (data.Length / 1024d).ToString("0.##") + " KB";
            }
            catch
            {
                _lblBoyut.Text = "Resim okunamadi";
            }
        }
    }
}
