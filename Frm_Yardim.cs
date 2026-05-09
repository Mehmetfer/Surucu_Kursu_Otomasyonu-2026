using System;
using System.Windows.Forms;

namespace Kolera_Mtsk
{
    public partial class Frm_Yardim : Form
    {
        public bool LisansAktifEdildi { get; private set; } = false;

        public Frm_Yardim()
        {
            InitializeComponent();
        }

        private void BtnOnayla_Click(object sender, EventArgs e)
        {
            string kod = TxtLisansKodu.Text.Trim();

            if (kod == "111")
            {
                GeciciLisansHelper.AktifEt();
                LisansAktifEdildi = true;

                MessageBox.Show("Lisans aktif edildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Geçersiz lisans kodu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnIptal_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}