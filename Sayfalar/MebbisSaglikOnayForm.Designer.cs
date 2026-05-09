using System.Drawing;
using System.Windows.Forms;

namespace Kolera_Mtsk.Sayfalar
{
    public partial class MebbisSaglikOnayForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private Label lblTitle;
        private Label lblInfo;
        private Label lblDataState;
        private Label lblKursiyer;
        private CheckBox chkBelgeDogru;
        private CheckBox chkSorumlulukOnay;
        private Label lblCaptcha;
        private TextBox txtCaptcha;
        private Button btnIptal;
        private Button btnDevam;
        private PictureBox picEvrak;
        private Label lblImageState;

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblDataState = new System.Windows.Forms.Label();
            this.lblKursiyer = new System.Windows.Forms.Label();
            this.chkBelgeDogru = new System.Windows.Forms.CheckBox();
            this.chkSorumlulukOnay = new System.Windows.Forms.CheckBox();
            this.lblCaptcha = new System.Windows.Forms.Label();
            this.txtCaptcha = new System.Windows.Forms.TextBox();
            this.btnIptal = new System.Windows.Forms.Button();
            this.btnDevam = new System.Windows.Forms.Button();
            this.picEvrak = new System.Windows.Forms.PictureBox();
            this.lblImageState = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picEvrak)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(210)))), ((int)(((byte)(120)))));
            this.lblTitle.Location = new System.Drawing.Point(16, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(270, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "SAGLIK EVRAK AKISI ON KONTROL";
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.ForeColor = System.Drawing.Color.White;
            this.lblInfo.Location = new System.Drawing.Point(18, 44);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(323, 15);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "Lutfen kursiyer bilgilerini kontrol edin ve onaylari isaretleyin.";
            // 
            // lblDataState
            // 
            this.lblDataState.AutoSize = true;
            this.lblDataState.ForeColor = System.Drawing.Color.LightGreen;
            this.lblDataState.Location = new System.Drawing.Point(18, 60);
            this.lblDataState.Name = "lblDataState";
            this.lblDataState.Size = new System.Drawing.Size(127, 15);
            this.lblDataState.TabIndex = 2;
            this.lblDataState.Text = "Kursiyer verisi yuklendi";
            // 
            // lblKursiyer
            // 
            this.lblKursiyer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblKursiyer.ForeColor = System.Drawing.Color.White;
            this.lblKursiyer.Location = new System.Drawing.Point(20, 80);
            this.lblKursiyer.Name = "lblKursiyer";
            this.lblKursiyer.Size = new System.Drawing.Size(238, 130);
            this.lblKursiyer.TabIndex = 3;
            this.lblKursiyer.Text = "Kursiyer bilgileri";
            // 
            // chkBelgeDogru
            // 
            this.chkBelgeDogru.AutoSize = true;
            this.chkBelgeDogru.ForeColor = System.Drawing.Color.White;
            this.chkBelgeDogru.Location = new System.Drawing.Point(22, 214);
            this.chkBelgeDogru.Name = "chkBelgeDogru";
            this.chkBelgeDogru.Size = new System.Drawing.Size(172, 19);
            this.chkBelgeDogru.TabIndex = 4;
            this.chkBelgeDogru.Text = "Saglik evragi dogru kuriyere";
            this.chkBelgeDogru.UseVisualStyleBackColor = true;
            // 
            // chkSorumlulukOnay
            // 
            this.chkSorumlulukOnay.AutoSize = true;
            this.chkSorumlulukOnay.ForeColor = System.Drawing.Color.White;
            this.chkSorumlulukOnay.Location = new System.Drawing.Point(22, 240);
            this.chkSorumlulukOnay.Name = "chkSorumlulukOnay";
            this.chkSorumlulukOnay.Size = new System.Drawing.Size(141, 19);
            this.chkSorumlulukOnay.TabIndex = 5;
            this.chkSorumlulukOnay.Text = "Aktarimi onayliyorum";
            this.chkSorumlulukOnay.UseVisualStyleBackColor = true;
            // 
            // lblCaptcha
            // 
            this.lblCaptcha.AutoSize = true;
            this.lblCaptcha.ForeColor = System.Drawing.Color.White;
            this.lblCaptcha.Location = new System.Drawing.Point(22, 274);
            this.lblCaptcha.Name = "lblCaptcha";
            this.lblCaptcha.Size = new System.Drawing.Size(94, 15);
            this.lblCaptcha.TabIndex = 6;
            this.lblCaptcha.Text = "Guvenlik kodu: -";
            // 
            // txtCaptcha
            // 
            this.txtCaptcha.Location = new System.Drawing.Point(150, 270);
            this.txtCaptcha.Name = "txtCaptcha";
            this.txtCaptcha.Size = new System.Drawing.Size(130, 23);
            this.txtCaptcha.TabIndex = 7;
            // 
            // btnIptal
            // 
            this.btnIptal.BackColor = System.Drawing.Color.DimGray;
            this.btnIptal.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnIptal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIptal.ForeColor = System.Drawing.Color.White;
            this.btnIptal.Location = new System.Drawing.Point(360, 324);
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Size = new System.Drawing.Size(84, 34);
            this.btnIptal.TabIndex = 8;
            this.btnIptal.Text = "Iptal";
            this.btnIptal.UseVisualStyleBackColor = false;
            // 
            // btnDevam
            // 
            this.btnDevam.BackColor = System.Drawing.Color.ForestGreen;
            this.btnDevam.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDevam.ForeColor = System.Drawing.Color.White;
            this.btnDevam.Location = new System.Drawing.Point(452, 324);
            this.btnDevam.Name = "btnDevam";
            this.btnDevam.Size = new System.Drawing.Size(88, 34);
            this.btnDevam.TabIndex = 9;
            this.btnDevam.Text = "Devam Et";
            this.btnDevam.UseVisualStyleBackColor = false;
            this.btnDevam.Click += new System.EventHandler(this.btnDevam_Click);
            // 
            // picEvrak
            // 
            this.picEvrak.BackColor = System.Drawing.Color.White;
            this.picEvrak.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picEvrak.Location = new System.Drawing.Point(347, 35);
            this.picEvrak.Name = "picEvrak";
            this.picEvrak.Size = new System.Drawing.Size(212, 258);
            this.picEvrak.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picEvrak.TabIndex = 10;
            this.picEvrak.TabStop = false;
            // 
            // lblImageState
            // 
            this.lblImageState.AutoSize = true;
            this.lblImageState.ForeColor = System.Drawing.Color.White;
            this.lblImageState.Location = new System.Drawing.Point(413, 17);
            this.lblImageState.Name = "lblImageState";
            this.lblImageState.Size = new System.Drawing.Size(88, 15);
            this.lblImageState.TabIndex = 11;
            this.lblImageState.Text = "Evrak Onizleme";
            // 
            // MebbisSaglikOnayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(614, 391);
            this.Controls.Add(this.lblImageState);
            this.Controls.Add(this.picEvrak);
            this.Controls.Add(this.btnDevam);
            this.Controls.Add(this.btnIptal);
            this.Controls.Add(this.txtCaptcha);
            this.Controls.Add(this.lblCaptcha);
            this.Controls.Add(this.chkSorumlulukOnay);
            this.Controls.Add(this.chkBelgeDogru);
            this.Controls.Add(this.lblKursiyer);
            this.Controls.Add(this.lblDataState);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MebbisSaglikOnayForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Saglik Evrak Onayi";
            ((System.ComponentModel.ISupportInitialize)(this.picEvrak)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
