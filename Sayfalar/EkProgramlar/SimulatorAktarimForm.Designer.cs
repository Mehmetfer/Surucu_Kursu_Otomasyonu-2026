namespace Kolera_Mtsk.Sayfalar.EkProgramlar
{
    partial class SimulatorAktarimForm
    {
        private System.ComponentModel.IContainer components = null;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView1;
        private System.Windows.Forms.TextBox txtKullanici;
        private System.Windows.Forms.TextBox txtSifre;
        private System.Windows.Forms.Button btnGiris;
        private System.Windows.Forms.Button btnTeorikCek;
        private System.Windows.Forms.Button btnUygulamaCek;
        private System.Windows.Forms.ComboBox comboHocalar;
        private System.Windows.Forms.Button btnTakvimAc;
        private System.Windows.Forms.TextBox txtKursAdi;
        private System.Windows.Forms.Label lblKursAdi;
        private System.Windows.Forms.RadioButton rb1Yil;
        private System.Windows.Forms.RadioButton rb3Yil;
        private System.Windows.Forms.RadioButton rb5Yil;
        private System.Windows.Forms.Button btnSimListele;
        private System.Windows.Forms.Button btnZipOlustur;
        private System.Windows.Forms.Label lblK;
        private System.Windows.Forms.Label lblS;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.webView1 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.txtKullanici = new System.Windows.Forms.TextBox();
            this.txtSifre = new System.Windows.Forms.TextBox();
            this.btnGiris = new System.Windows.Forms.Button();
            this.btnTeorikCek = new System.Windows.Forms.Button();
            this.btnUygulamaCek = new System.Windows.Forms.Button();
            this.comboHocalar = new System.Windows.Forms.ComboBox();
            this.btnTakvimAc = new System.Windows.Forms.Button();
            this.txtKursAdi = new System.Windows.Forms.TextBox();
            this.lblKursAdi = new System.Windows.Forms.Label();
            this.rb1Yil = new System.Windows.Forms.RadioButton();
            this.rb3Yil = new System.Windows.Forms.RadioButton();
            this.rb5Yil = new System.Windows.Forms.RadioButton();
            this.btnSimListele = new System.Windows.Forms.Button();
            this.btnZipOlustur = new System.Windows.Forms.Button();
            this.lblK = new System.Windows.Forms.Label();
            this.lblS = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.webView1)).BeginInit();
            this.SuspendLayout();
            // 
            // webView1
            // 
            this.webView1.AllowExternalDrop = true;
            this.webView1.CreationProperties = null;
            this.webView1.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView1.Location = new System.Drawing.Point(12, 84);
            this.webView1.Name = "webView1";
            this.webView1.Size = new System.Drawing.Size(1160, 565);
            this.webView1.TabIndex = 0;
            this.webView1.ZoomFactor = 1D;
            // 
            // txtKullanici
            // 
            this.txtKullanici.Location = new System.Drawing.Point(78, 15);
            this.txtKullanici.Name = "txtKullanici";
            this.txtKullanici.Size = new System.Drawing.Size(140, 20);
            this.txtKullanici.TabIndex = 1;
            // 
            // txtSifre
            // 
            this.txtSifre.Location = new System.Drawing.Point(279, 15);
            this.txtSifre.Name = "txtSifre";
            this.txtSifre.PasswordChar = '*';
            this.txtSifre.Size = new System.Drawing.Size(120, 20);
            this.txtSifre.TabIndex = 2;
            // 
            // btnGiris
            // 
            this.btnGiris.Location = new System.Drawing.Point(405, 13);
            this.btnGiris.Name = "btnGiris";
            this.btnGiris.Size = new System.Drawing.Size(60, 23);
            this.btnGiris.TabIndex = 3;
            this.btnGiris.Text = "Giris";
            this.btnGiris.UseVisualStyleBackColor = true;
            // 
            // btnTeorikCek
            // 
            this.btnTeorikCek.Location = new System.Drawing.Point(471, 13);
            this.btnTeorikCek.Name = "btnTeorikCek";
            this.btnTeorikCek.Size = new System.Drawing.Size(86, 23);
            this.btnTeorikCek.TabIndex = 4;
            this.btnTeorikCek.Text = "Teorik Cek";
            this.btnTeorikCek.UseVisualStyleBackColor = true;
            // 
            // btnUygulamaCek
            // 
            this.btnUygulamaCek.Location = new System.Drawing.Point(563, 13);
            this.btnUygulamaCek.Name = "btnUygulamaCek";
            this.btnUygulamaCek.Size = new System.Drawing.Size(94, 23);
            this.btnUygulamaCek.TabIndex = 5;
            this.btnUygulamaCek.Text = "Uygulama Cek";
            this.btnUygulamaCek.UseVisualStyleBackColor = true;
            // 
            // comboHocalar
            // 
            this.comboHocalar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboHocalar.FormattingEnabled = true;
            this.comboHocalar.Location = new System.Drawing.Point(663, 14);
            this.comboHocalar.Name = "comboHocalar";
            this.comboHocalar.Size = new System.Drawing.Size(152, 21);
            this.comboHocalar.TabIndex = 6;
            // 
            // btnTakvimAc
            // 
            this.btnTakvimAc.Location = new System.Drawing.Point(821, 13);
            this.btnTakvimAc.Name = "btnTakvimAc";
            this.btnTakvimAc.Size = new System.Drawing.Size(86, 23);
            this.btnTakvimAc.TabIndex = 7;
            this.btnTakvimAc.Text = "Takvimi Ac";
            this.btnTakvimAc.UseVisualStyleBackColor = true;
            // 
            // txtKursAdi
            // 
            this.txtKursAdi.Location = new System.Drawing.Point(81, 52);
            this.txtKursAdi.Name = "txtKursAdi";
            this.txtKursAdi.Size = new System.Drawing.Size(318, 20);
            this.txtKursAdi.TabIndex = 10;
            // 
            // lblKursAdi
            // 
            this.lblKursAdi.AutoSize = true;
            this.lblKursAdi.Location = new System.Drawing.Point(12, 55);
            this.lblKursAdi.Name = "lblKursAdi";
            this.lblKursAdi.Size = new System.Drawing.Size(54, 13);
            this.lblKursAdi.TabIndex = 11;
            this.lblKursAdi.Text = "Kurs Adi:";
            // 
            // rb1Yil
            // 
            this.rb1Yil.AutoSize = true;
            this.rb1Yil.Checked = true;
            this.rb1Yil.Location = new System.Drawing.Point(417, 53);
            this.rb1Yil.Name = "rb1Yil";
            this.rb1Yil.Size = new System.Drawing.Size(48, 17);
            this.rb1Yil.TabIndex = 12;
            this.rb1Yil.TabStop = true;
            this.rb1Yil.Text = "1 Yil";
            this.rb1Yil.UseVisualStyleBackColor = true;
            // 
            // rb3Yil
            // 
            this.rb3Yil.AutoSize = true;
            this.rb3Yil.Location = new System.Drawing.Point(471, 53);
            this.rb3Yil.Name = "rb3Yil";
            this.rb3Yil.Size = new System.Drawing.Size(48, 17);
            this.rb3Yil.TabIndex = 13;
            this.rb3Yil.Text = "3 Yil";
            this.rb3Yil.UseVisualStyleBackColor = true;
            // 
            // rb5Yil
            // 
            this.rb5Yil.AutoSize = true;
            this.rb5Yil.Location = new System.Drawing.Point(525, 53);
            this.rb5Yil.Name = "rb5Yil";
            this.rb5Yil.Size = new System.Drawing.Size(48, 17);
            this.rb5Yil.TabIndex = 14;
            this.rb5Yil.Text = "5 Yil";
            this.rb5Yil.UseVisualStyleBackColor = true;
            // 
            // btnSimListele
            // 
            this.btnSimListele.Location = new System.Drawing.Point(579, 49);
            this.btnSimListele.Name = "btnSimListele";
            this.btnSimListele.Size = new System.Drawing.Size(122, 23);
            this.btnSimListele.TabIndex = 15;
            this.btnSimListele.Text = "Simulator Listele";
            this.btnSimListele.UseVisualStyleBackColor = true;
            // 
            // btnZipOlustur
            // 
            this.btnZipOlustur.Location = new System.Drawing.Point(707, 49);
            this.btnZipOlustur.Name = "btnZipOlustur";
            this.btnZipOlustur.Size = new System.Drawing.Size(122, 23);
            this.btnZipOlustur.TabIndex = 16;
            this.btnZipOlustur.Text = "PDF + ZIP Olustur";
            this.btnZipOlustur.UseVisualStyleBackColor = true;
            // 
            // lblK
            // 
            this.lblK.AutoSize = true;
            this.lblK.Location = new System.Drawing.Point(12, 18);
            this.lblK.Name = "lblK";
            this.lblK.Size = new System.Drawing.Size(60, 13);
            this.lblK.TabIndex = 8;
            this.lblK.Text = "Kullanici:";
            // 
            // lblS
            // 
            this.lblS.AutoSize = true;
            this.lblS.Location = new System.Drawing.Point(230, 18);
            this.lblS.Name = "lblS";
            this.lblS.Size = new System.Drawing.Size(33, 13);
            this.lblS.TabIndex = 9;
            this.lblS.Text = "Sifre:";
            // 
            // SimulatorAktarimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 661);
            this.Controls.Add(this.btnZipOlustur);
            this.Controls.Add(this.btnSimListele);
            this.Controls.Add(this.rb5Yil);
            this.Controls.Add(this.rb3Yil);
            this.Controls.Add(this.rb1Yil);
            this.Controls.Add(this.lblKursAdi);
            this.Controls.Add(this.txtKursAdi);
            this.Controls.Add(this.lblS);
            this.Controls.Add(this.lblK);
            this.Controls.Add(this.btnTakvimAc);
            this.Controls.Add(this.comboHocalar);
            this.Controls.Add(this.btnUygulamaCek);
            this.Controls.Add(this.btnTeorikCek);
            this.Controls.Add(this.btnGiris);
            this.Controls.Add(this.txtSifre);
            this.Controls.Add(this.txtKullanici);
            this.Controls.Add(this.webView1);
            this.Name = "SimulatorAktarimForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simulator Aktarimi";
            ((System.ComponentModel.ISupportInitialize)(this.webView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
