namespace Kolera_Mtsk.Sayfalar
{
    partial class KoleraIstampaForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblAlan;
        private System.Windows.Forms.ComboBox _cmbAlan;
        private System.Windows.Forms.Label lblAciklama;
        private System.Windows.Forms.TextBox _txtAciklama;
        private System.Windows.Forms.Label _lblBoyut;
        private System.Windows.Forms.Button _btnTara;
        private System.Windows.Forms.Button _btnDosya;
        private System.Windows.Forms.Button _btnKaydet;
        private System.Windows.Forms.Button _btnIstampaYap;
        private System.Windows.Forms.PictureBox _pic;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblAlan = new System.Windows.Forms.Label();
            this._cmbAlan = new System.Windows.Forms.ComboBox();
            this.lblAciklama = new System.Windows.Forms.Label();
            this._txtAciklama = new System.Windows.Forms.TextBox();
            this._lblBoyut = new System.Windows.Forms.Label();
            this._btnTara = new System.Windows.Forms.Button();
            this._btnDosya = new System.Windows.Forms.Button();
            this._btnKaydet = new System.Windows.Forms.Button();
            this._btnIstampaYap = new System.Windows.Forms.Button();
            this._pic = new System.Windows.Forms.PictureBox();
            this.pnlTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pic)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this._btnIstampaYap);
            this.pnlTop.Controls.Add(this._btnKaydet);
            this.pnlTop.Controls.Add(this._btnDosya);
            this.pnlTop.Controls.Add(this._btnTara);
            this.pnlTop.Controls.Add(this._lblBoyut);
            this.pnlTop.Controls.Add(this._txtAciklama);
            this.pnlTop.Controls.Add(this.lblAciklama);
            this.pnlTop.Controls.Add(this._cmbAlan);
            this.pnlTop.Controls.Add(this.lblAlan);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(824, 120);
            this.pnlTop.TabIndex = 0;
            // 
            // lblAlan
            // 
            this.lblAlan.AutoSize = true;
            this.lblAlan.Location = new System.Drawing.Point(12, 15);
            this.lblAlan.Name = "lblAlan";
            this.lblAlan.Size = new System.Drawing.Size(27, 13);
            this.lblAlan.TabIndex = 0;
            this.lblAlan.Text = "Alan";
            // 
            // _cmbAlan
            // 
            this._cmbAlan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbAlan.FormattingEnabled = true;
            this._cmbAlan.Location = new System.Drawing.Point(90, 12);
            this._cmbAlan.Name = "_cmbAlan";
            this._cmbAlan.Size = new System.Drawing.Size(280, 21);
            this._cmbAlan.TabIndex = 1;
            this._cmbAlan.SelectedIndexChanged += new System.EventHandler(this.YukleSeciliAlan);
            // 
            // lblAciklama
            // 
            this.lblAciklama.AutoSize = true;
            this.lblAciklama.Location = new System.Drawing.Point(12, 50);
            this.lblAciklama.Name = "lblAciklama";
            this.lblAciklama.Size = new System.Drawing.Size(50, 13);
            this.lblAciklama.TabIndex = 2;
            this.lblAciklama.Text = "Aciklama";
            // 
            // _txtAciklama
            // 
            this._txtAciklama.Location = new System.Drawing.Point(90, 47);
            this._txtAciklama.Name = "_txtAciklama";
            this._txtAciklama.Size = new System.Drawing.Size(450, 20);
            this._txtAciklama.TabIndex = 3;
            // 
            // _lblBoyut
            // 
            this._lblBoyut.AutoSize = true;
            this._lblBoyut.ForeColor = System.Drawing.Color.DimGray;
            this._lblBoyut.Location = new System.Drawing.Point(90, 82);
            this._lblBoyut.Name = "_lblBoyut";
            this._lblBoyut.Size = new System.Drawing.Size(57, 13);
            this._lblBoyut.TabIndex = 4;
            this._lblBoyut.Text = "Resim yok";
            // 
            // _btnTara
            // 
            this._btnTara.Location = new System.Drawing.Point(560, 8);
            this._btnTara.Name = "_btnTara";
            this._btnTara.Size = new System.Drawing.Size(110, 28);
            this._btnTara.TabIndex = 5;
            this._btnTara.Text = "Tara";
            this._btnTara.UseVisualStyleBackColor = true;
            this._btnTara.Click += new System.EventHandler(this.BtnTara_Click);
            // 
            // _btnDosya
            // 
            this._btnDosya.Location = new System.Drawing.Point(560, 42);
            this._btnDosya.Name = "_btnDosya";
            this._btnDosya.Size = new System.Drawing.Size(110, 28);
            this._btnDosya.TabIndex = 6;
            this._btnDosya.Text = "Dosyadan Sec";
            this._btnDosya.UseVisualStyleBackColor = true;
            this._btnDosya.Click += new System.EventHandler(this.BtnDosya_Click);
            // 
            // _btnKaydet
            // 
            this._btnKaydet.Location = new System.Drawing.Point(680, 8);
            this._btnKaydet.Name = "_btnKaydet";
            this._btnKaydet.Size = new System.Drawing.Size(110, 62);
            this._btnKaydet.TabIndex = 7;
            this._btnKaydet.Text = "Kaydet";
            this._btnKaydet.UseVisualStyleBackColor = true;
            this._btnKaydet.Click += new System.EventHandler(this.BtnKaydet_Click);
            // 
            // _btnIstampaYap
            // 
            this._btnIstampaYap.Location = new System.Drawing.Point(560, 76);
            this._btnIstampaYap.Name = "_btnIstampaYap";
            this._btnIstampaYap.Size = new System.Drawing.Size(230, 28);
            this._btnIstampaYap.TabIndex = 8;
            this._btnIstampaYap.Text = "Mouse'u Istampa Yap";
            this._btnIstampaYap.UseVisualStyleBackColor = true;
            this._btnIstampaYap.Click += new System.EventHandler(this.BtnIstampaYap_Click);
            // 
            // _pic
            // 
            this._pic.BackColor = System.Drawing.Color.WhiteSmoke;
            this._pic.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pic.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pic.Location = new System.Drawing.Point(0, 120);
            this._pic.Name = "_pic";
            this._pic.Size = new System.Drawing.Size(824, 401);
            this._pic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._pic.TabIndex = 1;
            this._pic.TabStop = false;
            // 
            // KoleraIstampaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 521);
            this.Controls.Add(this._pic);
            this.Controls.Add(this.pnlTop);
            this.MinimumSize = new System.Drawing.Size(760, 500);
            this.Name = "KoleraIstampaForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kolera Istampa Tanimlari";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pic)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
