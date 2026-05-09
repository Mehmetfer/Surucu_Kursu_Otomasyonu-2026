namespace Kolera_Mtsk.Sayfalar
{
    partial class SmsSablonDuzenleForm
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
            this.lblKayitliSablon = new System.Windows.Forms.Label();
            this.cmbKayitliSablonlar = new System.Windows.Forms.ComboBox();
            this.lblSablon = new System.Windows.Forms.Label();
            this.txtSablon = new System.Windows.Forms.TextBox();
            this.lblAnahtar = new System.Windows.Forms.Label();
            this.lblOnizleme = new System.Windows.Forms.Label();
            this.txtOnizleme = new System.Windows.Forms.TextBox();
            this.btnOnizle = new System.Windows.Forms.Button();
            this.btnKaydetDevam = new System.Windows.Forms.Button();
            this.btnIptal = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblKayitliSablon
            // 
            this.lblKayitliSablon.AutoSize = true;
            this.lblKayitliSablon.Location = new System.Drawing.Point(12, 9);
            this.lblKayitliSablon.Name = "lblKayitliSablon";
            this.lblKayitliSablon.Size = new System.Drawing.Size(83, 13);
            this.lblKayitliSablon.TabIndex = 8;
            this.lblKayitliSablon.Text = "Kayitli sablonlar:";
            // 
            // cmbKayitliSablonlar
            // 
            this.cmbKayitliSablonlar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKayitliSablonlar.FormattingEnabled = true;
            this.cmbKayitliSablonlar.Location = new System.Drawing.Point(120, 6);
            this.cmbKayitliSablonlar.Name = "cmbKayitliSablonlar";
            this.cmbKayitliSablonlar.Size = new System.Drawing.Size(780, 21);
            this.cmbKayitliSablonlar.TabIndex = 0;
            this.cmbKayitliSablonlar.SelectedIndexChanged += new System.EventHandler(this.cmbKayitliSablonlar_SelectedIndexChanged);
            // 
            // lblSablon
            // 
            this.lblSablon.AutoSize = true;
            this.lblSablon.Location = new System.Drawing.Point(12, 36);
            this.lblSablon.Name = "lblSablon";
            this.lblSablon.Size = new System.Drawing.Size(108, 13);
            this.lblSablon.TabIndex = 1;
            this.lblSablon.Text = "SMS Sablon Metni:";
            // 
            // txtSablon
            // 
            this.txtSablon.Location = new System.Drawing.Point(15, 52);
            this.txtSablon.Multiline = true;
            this.txtSablon.Name = "txtSablon";
            this.txtSablon.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSablon.Size = new System.Drawing.Size(885, 100);
            this.txtSablon.TabIndex = 2;
            // 
            // lblAnahtar
            // 
            this.lblAnahtar.AutoSize = true;
            this.lblAnahtar.Location = new System.Drawing.Point(12, 160);
            this.lblAnahtar.Name = "lblAnahtar";
            this.lblAnahtar.Size = new System.Drawing.Size(447, 13);
            this.lblAnahtar.TabIndex = 3;
            this.lblAnahtar.Text = "Kullanilabilir alanlar: [AD SOYAD], [TARIH], [SAAT], [KURS ADI], [TELEFON]";
            // 
            // lblOnizleme
            // 
            this.lblOnizleme.AutoSize = true;
            this.lblOnizleme.Location = new System.Drawing.Point(12, 184);
            this.lblOnizleme.Name = "lblOnizleme";
            this.lblOnizleme.Size = new System.Drawing.Size(50, 13);
            this.lblOnizleme.TabIndex = 4;
            this.lblOnizleme.Text = "Onizleme";
            // 
            // txtOnizleme
            // 
            this.txtOnizleme.Location = new System.Drawing.Point(15, 200);
            this.txtOnizleme.Multiline = true;
            this.txtOnizleme.Name = "txtOnizleme";
            this.txtOnizleme.ReadOnly = true;
            this.txtOnizleme.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOnizleme.Size = new System.Drawing.Size(885, 114);
            this.txtOnizleme.TabIndex = 5;
            // 
            // btnOnizle
            // 
            this.btnOnizle.Location = new System.Drawing.Point(15, 330);
            this.btnOnizle.Name = "btnOnizle";
            this.btnOnizle.Size = new System.Drawing.Size(85, 30);
            this.btnOnizle.TabIndex = 6;
            this.btnOnizle.Text = "Onizleme";
            this.btnOnizle.UseVisualStyleBackColor = true;
            this.btnOnizle.Click += new System.EventHandler(this.btnOnizle_Click);
            // 
            // btnKaydetDevam
            // 
            this.btnKaydetDevam.Location = new System.Drawing.Point(711, 330);
            this.btnKaydetDevam.Name = "btnKaydetDevam";
            this.btnKaydetDevam.Size = new System.Drawing.Size(95, 30);
            this.btnKaydetDevam.TabIndex = 7;
            this.btnKaydetDevam.Text = "Kaydet/Devam";
            this.btnKaydetDevam.UseVisualStyleBackColor = true;
            this.btnKaydetDevam.Click += new System.EventHandler(this.btnKaydetDevam_Click);
            // 
            // btnIptal
            // 
            this.btnIptal.Location = new System.Drawing.Point(812, 330);
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Size = new System.Drawing.Size(88, 30);
            this.btnIptal.TabIndex = 9;
            this.btnIptal.Text = "Iptal";
            this.btnIptal.UseVisualStyleBackColor = true;
            this.btnIptal.Click += new System.EventHandler(this.btnIptal_Click);
            // 
            // SmsSablonDuzenleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(916, 372);
            this.Controls.Add(this.btnIptal);
            this.Controls.Add(this.btnKaydetDevam);
            this.Controls.Add(this.btnOnizle);
            this.Controls.Add(this.txtOnizleme);
            this.Controls.Add(this.lblOnizleme);
            this.Controls.Add(this.lblAnahtar);
            this.Controls.Add(this.txtSablon);
            this.Controls.Add(this.lblSablon);
            this.Controls.Add(this.cmbKayitliSablonlar);
            this.Controls.Add(this.lblKayitliSablon);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SmsSablonDuzenleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SMS Sablon Duzenleme";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblKayitliSablon;
        private System.Windows.Forms.ComboBox cmbKayitliSablonlar;
        private System.Windows.Forms.Label lblSablon;
        private System.Windows.Forms.TextBox txtSablon;
        private System.Windows.Forms.Label lblAnahtar;
        private System.Windows.Forms.Label lblOnizleme;
        private System.Windows.Forms.TextBox txtOnizleme;
        private System.Windows.Forms.Button btnOnizle;
        private System.Windows.Forms.Button btnKaydetDevam;
        private System.Windows.Forms.Button btnIptal;
    }
}
