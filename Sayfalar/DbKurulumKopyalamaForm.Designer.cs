namespace Kolera_Mtsk.Sayfalar
{
    partial class DbKurulumKopyalamaForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.grpBaglanti = new System.Windows.Forms.GroupBox();
            this.btnDbleriListele = new System.Windows.Forms.Button();
            this.lblBaglantiDurumu = new System.Windows.Forms.Label();
            this.txtBaglantiTuru = new System.Windows.Forms.TextBox();
            this.cmbKaynakDb = new System.Windows.Forms.ComboBox();
            this.lblKaynakDb = new System.Windows.Forms.Label();
            this.btnBacSec = new System.Windows.Forms.Button();
            this.cmbHedefDb = new System.Windows.Forms.ComboBox();
            this.lblHedefDb = new System.Windows.Forms.Label();
            this.txtParola = new System.Windows.Forms.TextBox();
            this.lblParola = new System.Windows.Forms.Label();
            this.txtKullanici = new System.Windows.Forms.TextBox();
            this.lblKullanici = new System.Windows.Forms.Label();
            this.txtSunucu = new System.Windows.Forms.TextBox();
            this.lblSunucu = new System.Windows.Forms.Label();
            this.grpIslem = new System.Windows.Forms.GroupBox();
            this.btnKapat = new System.Windows.Forms.Button();
            this.btnKopyala = new System.Windows.Forms.Button();
            this.btnSemaKur = new System.Windows.Forms.Button();
            this.btnKontrolEt = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblYuzde = new System.Windows.Forms.Label();
            this.lblDurum = new System.Windows.Forms.Label();
            this.chkKopyalananlar = new System.Windows.Forms.CheckedListBox();
            this.lblKaynakBaslik = new System.Windows.Forms.Label();
            this.lblHedefBaslik = new System.Windows.Forms.Label();
            this.grpBaglanti.SuspendLayout();
            this.grpIslem.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpBaglanti
            // 
            this.grpBaglanti.Controls.Add(this.lblBaglantiDurumu);
            this.grpBaglanti.Controls.Add(this.txtBaglantiTuru);
            this.grpBaglanti.Controls.Add(this.btnDbleriListele);
            this.grpBaglanti.Controls.Add(this.btnBacSec);
            this.grpBaglanti.Controls.Add(this.cmbKaynakDb);
            this.grpBaglanti.Controls.Add(this.lblKaynakDb);
            this.grpBaglanti.Controls.Add(this.cmbHedefDb);
            this.grpBaglanti.Controls.Add(this.lblHedefDb);
            this.grpBaglanti.Controls.Add(this.txtParola);
            this.grpBaglanti.Controls.Add(this.lblParola);
            this.grpBaglanti.Controls.Add(this.txtKullanici);
            this.grpBaglanti.Controls.Add(this.lblKullanici);
            this.grpBaglanti.Controls.Add(this.txtSunucu);
            this.grpBaglanti.Controls.Add(this.lblSunucu);
            this.grpBaglanti.Location = new System.Drawing.Point(12, 12);
            this.grpBaglanti.Name = "grpBaglanti";
            this.grpBaglanti.Size = new System.Drawing.Size(318, 460);
            this.grpBaglanti.TabIndex = 0;
            this.grpBaglanti.TabStop = false;
            this.grpBaglanti.Text = "Sunucu Bağlantısı";
            // 
            // btnDbleriListele
            // 
            this.btnDbleriListele.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnDbleriListele.Location = new System.Drawing.Point(154, 157);
            this.btnDbleriListele.Name = "btnDbleriListele";
            this.btnDbleriListele.Size = new System.Drawing.Size(129, 35);
            this.btnDbleriListele.TabIndex = 10;
            this.btnDbleriListele.Text = "SERVER A BAĞLAN";
            this.btnDbleriListele.UseVisualStyleBackColor = false;
            // 
            // lblBaglantiDurumu
            // 
            this.lblBaglantiDurumu.AutoSize = true;
            this.lblBaglantiDurumu.Location = new System.Drawing.Point(16, 206);
            this.lblBaglantiDurumu.Name = "lblBaglantiDurumu";
            this.lblBaglantiDurumu.Size = new System.Drawing.Size(69, 13);
            this.lblBaglantiDurumu.TabIndex = 12;
            this.lblBaglantiDurumu.Text = "Bağlı değil...";
            // 
            // txtBaglantiTuru
            // 
            this.txtBaglantiTuru.BackColor = System.Drawing.SystemColors.WindowText;
            this.txtBaglantiTuru.ForeColor = System.Drawing.SystemColors.Window;
            this.txtBaglantiTuru.Location = new System.Drawing.Point(154, 72);
            this.txtBaglantiTuru.Name = "txtBaglantiTuru";
            this.txtBaglantiTuru.Size = new System.Drawing.Size(129, 20);
            this.txtBaglantiTuru.TabIndex = 11;
            this.txtBaglantiTuru.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // cmbKaynakDb
            // 
            this.cmbKaynakDb.FormattingEnabled = true;
            this.cmbKaynakDb.Location = new System.Drawing.Point(72, 282);
            this.cmbKaynakDb.Name = "cmbKaynakDb";
            this.cmbKaynakDb.Size = new System.Drawing.Size(230, 21);
            this.cmbKaynakDb.TabIndex = 9;
            // 
            // lblKaynakDb
            // 
            this.lblKaynakDb.AutoSize = true;
            this.lblKaynakDb.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblKaynakDb.ForeColor = System.Drawing.Color.IndianRed;
            this.lblKaynakDb.Location = new System.Drawing.Point(32, 250);
            this.lblKaynakDb.Name = "lblKaynakDb";
            this.lblKaynakDb.Size = new System.Drawing.Size(251, 25);
            this.lblKaynakDb.TabIndex = 8;
            this.lblKaynakDb.Text = "KAYNAK VERİ TABANI";
            // 
            // btnBacSec
            // 
            this.btnBacSec.Location = new System.Drawing.Point(19, 320);
            this.btnBacSec.Name = "btnBacSec";
            this.btnBacSec.Size = new System.Drawing.Size(283, 28);
            this.btnBacSec.TabIndex = 13;
            this.btnBacSec.Text = "BAC/BACPAC Dosyası Seç";
            this.btnBacSec.UseVisualStyleBackColor = true;
            // 
            // cmbHedefDb
            // 
            this.cmbHedefDb.FormattingEnabled = true;
            this.cmbHedefDb.Location = new System.Drawing.Point(72, 420);
            this.cmbHedefDb.Name = "cmbHedefDb";
            this.cmbHedefDb.Size = new System.Drawing.Size(230, 21);
            this.cmbHedefDb.TabIndex = 7;
            // 
            // lblHedefDb
            // 
            this.lblHedefDb.AutoSize = true;
            this.lblHedefDb.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.lblHedefDb.ForeColor = System.Drawing.Color.Red;
            this.lblHedefDb.Location = new System.Drawing.Point(32, 382);
            this.lblHedefDb.Name = "lblHedefDb";
            this.lblHedefDb.Size = new System.Drawing.Size(235, 25);
            this.lblHedefDb.TabIndex = 6;
            this.lblHedefDb.Text = "HEDEF VERİ TABANI";
            // 
            // txtParola
            // 
            this.txtParola.Location = new System.Drawing.Point(154, 131);
            this.txtParola.Name = "txtParola";
            this.txtParola.Size = new System.Drawing.Size(129, 20);
            this.txtParola.TabIndex = 5;
            this.txtParola.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtParola.UseSystemPasswordChar = true;
            // 
            // lblParola
            // 
            this.lblParola.AutoSize = true;
            this.lblParola.Location = new System.Drawing.Point(47, 134);
            this.lblParola.Name = "lblParola";
            this.lblParola.Size = new System.Drawing.Size(40, 13);
            this.lblParola.TabIndex = 4;
            this.lblParola.Text = "Parola ";
            // 
            // txtKullanici
            // 
            this.txtKullanici.Location = new System.Drawing.Point(154, 98);
            this.txtKullanici.Name = "txtKullanici";
            this.txtKullanici.Size = new System.Drawing.Size(129, 20);
            this.txtKullanici.TabIndex = 3;
            this.txtKullanici.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblKullanici
            // 
            this.lblKullanici.AutoSize = true;
            this.lblKullanici.Location = new System.Drawing.Point(47, 101);
            this.lblKullanici.Name = "lblKullanici";
            this.lblKullanici.Size = new System.Drawing.Size(64, 13);
            this.lblKullanici.TabIndex = 2;
            this.lblKullanici.Text = "Kullanıcı Adı";
            // 
            // txtSunucu
            // 
            this.txtSunucu.Location = new System.Drawing.Point(154, 39);
            this.txtSunucu.Name = "txtSunucu";
            this.txtSunucu.Size = new System.Drawing.Size(129, 20);
            this.txtSunucu.TabIndex = 1;
            this.txtSunucu.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblSunucu
            // 
            this.lblSunucu.AutoSize = true;
            this.lblSunucu.Location = new System.Drawing.Point(47, 42);
            this.lblSunucu.Name = "lblSunucu";
            this.lblSunucu.Size = new System.Drawing.Size(76, 13);
            this.lblSunucu.TabIndex = 0;
            this.lblSunucu.Text = "Sunucu Adresi";
            // 
            // grpIslem
            // 
            this.grpIslem.Controls.Add(this.lblHedefBaslik);
            this.grpIslem.Controls.Add(this.lblKaynakBaslik);
            this.grpIslem.Controls.Add(this.btnKapat);
            this.grpIslem.Controls.Add(this.btnKopyala);
            this.grpIslem.Controls.Add(this.btnSemaKur);
            this.grpIslem.Controls.Add(this.btnKontrolEt);
            this.grpIslem.Controls.Add(this.progressBar);
            this.grpIslem.Controls.Add(this.lblYuzde);
            this.grpIslem.Controls.Add(this.lblDurum);
            this.grpIslem.Controls.Add(this.chkKopyalananlar);
            this.grpIslem.Location = new System.Drawing.Point(336, 12);
            this.grpIslem.Name = "grpIslem";
            this.grpIslem.Size = new System.Drawing.Size(574, 599);
            this.grpIslem.TabIndex = 1;
            this.grpIslem.TabStop = false;
            this.grpIslem.Text = "Kopyalama İşlemi";
            // 
            // btnKapat
            // 
            this.btnKapat.Location = new System.Drawing.Point(464, 549);
            this.btnKapat.Name = "btnKapat";
            this.btnKapat.Size = new System.Drawing.Size(103, 28);
            this.btnKapat.TabIndex = 5;
            this.btnKapat.Text = "Kapat";
            this.btnKapat.UseVisualStyleBackColor = true;
            // 
            // btnKopyala
            // 
            this.btnKopyala.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.btnKopyala.Location = new System.Drawing.Point(19, 524);
            this.btnKopyala.Name = "btnKopyala";
            this.btnKopyala.Size = new System.Drawing.Size(197, 59);
            this.btnKopyala.TabIndex = 4;
            this.btnKopyala.Text = "B A Ş L A T";
            this.btnKopyala.UseVisualStyleBackColor = true;
            // 
            // btnSemaKur
            // 
            this.btnSemaKur.Location = new System.Drawing.Point(222, 549);
            this.btnSemaKur.Name = "btnSemaKur";
            this.btnSemaKur.Size = new System.Drawing.Size(122, 28);
            this.btnSemaKur.TabIndex = 3;
            this.btnSemaKur.Text = "Hedefe Sema Kur";
            this.btnSemaKur.UseVisualStyleBackColor = true;
            // 
            // btnKontrolEt
            // 
            this.btnKontrolEt.Location = new System.Drawing.Point(344, 549);
            this.btnKontrolEt.Name = "btnKontrolEt";
            this.btnKontrolEt.Size = new System.Drawing.Size(108, 28);
            this.btnKontrolEt.TabIndex = 6;
            this.btnKontrolEt.Text = "Sema Kontrol Et";
            this.btnKontrolEt.UseVisualStyleBackColor = true;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(181, 20);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(330, 23);
            this.progressBar.TabIndex = 2;
            // 
            // lblYuzde
            // 
            this.lblYuzde.AutoSize = true;
            this.lblYuzde.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.lblYuzde.Location = new System.Drawing.Point(517, 25);
            this.lblYuzde.Name = "lblYuzde";
            this.lblYuzde.Size = new System.Drawing.Size(25, 13);
            this.lblYuzde.TabIndex = 7;
            this.lblYuzde.Text = "%0";
            // 
            // lblDurum
            // 
            this.lblDurum.AutoSize = true;
            this.lblDurum.Location = new System.Drawing.Point(16, 25);
            this.lblDurum.Name = "lblDurum";
            this.lblDurum.Size = new System.Drawing.Size(35, 13);
            this.lblDurum.TabIndex = 1;
            this.lblDurum.Text = "Durum";
            // 
            // chkKopyalananlar
            // 
            this.chkKopyalananlar.CheckOnClick = true;
            this.chkKopyalananlar.FormattingEnabled = true;
            this.chkKopyalananlar.Location = new System.Drawing.Point(19, 60);
            this.chkKopyalananlar.Name = "chkKopyalananlar";
            this.chkKopyalananlar.Size = new System.Drawing.Size(542, 454);
            this.chkKopyalananlar.TabIndex = 0;
            // 
            // lblKaynakBaslik
            // 
            this.lblKaynakBaslik.AutoSize = true;
            this.lblKaynakBaslik.Location = new System.Drawing.Point(18, 289);
            this.lblKaynakBaslik.Name = "lblKaynakBaslik";
            this.lblKaynakBaslik.Size = new System.Drawing.Size(0, 13);
            this.lblKaynakBaslik.TabIndex = 8;
            // 
            // lblHedefBaslik
            // 
            this.lblHedefBaslik.AutoSize = true;
            this.lblHedefBaslik.Location = new System.Drawing.Point(18, 423);
            this.lblHedefBaslik.Name = "lblHedefBaslik";
            this.lblHedefBaslik.Size = new System.Drawing.Size(0, 13);
            this.lblHedefBaslik.TabIndex = 9;
            // 
            // DbKurulumKopyalamaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(922, 623);
            this.Controls.Add(this.grpIslem);
            this.Controls.Add(this.grpBaglanti);
            this.Name = "DbKurulumKopyalamaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Veri Tabanı Kopyalama";
            this.grpBaglanti.ResumeLayout(false);
            this.grpBaglanti.PerformLayout();
            this.grpIslem.ResumeLayout(false);
            this.grpIslem.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.GroupBox grpBaglanti;
        private System.Windows.Forms.Button btnDbleriListele;
        private System.Windows.Forms.Label lblBaglantiDurumu;
        private System.Windows.Forms.TextBox txtBaglantiTuru;
        private System.Windows.Forms.ComboBox cmbKaynakDb;
        private System.Windows.Forms.Label lblKaynakDb;
        private System.Windows.Forms.Button btnBacSec;
        private System.Windows.Forms.ComboBox cmbHedefDb;
        private System.Windows.Forms.Label lblHedefDb;
        private System.Windows.Forms.TextBox txtParola;
        private System.Windows.Forms.Label lblParola;
        private System.Windows.Forms.TextBox txtKullanici;
        private System.Windows.Forms.Label lblKullanici;
        private System.Windows.Forms.TextBox txtSunucu;
        private System.Windows.Forms.Label lblSunucu;
        private System.Windows.Forms.GroupBox grpIslem;
        private System.Windows.Forms.Button btnKapat;
        private System.Windows.Forms.Button btnKopyala;
        private System.Windows.Forms.Button btnSemaKur;
        private System.Windows.Forms.Button btnKontrolEt;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblYuzde;
        private System.Windows.Forms.Label lblDurum;
        private System.Windows.Forms.CheckedListBox chkKopyalananlar;
        private System.Windows.Forms.Label lblKaynakBaslik;
        private System.Windows.Forms.Label lblHedefBaslik;
    }
}
