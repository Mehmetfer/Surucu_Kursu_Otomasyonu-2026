namespace Kolera_Mtsk.Sayfalar
{
    partial class Frm_SMS_Gonder
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelUst = new System.Windows.Forms.Panel();
            this.Lbl_Durum = new System.Windows.Forms.Label();
            this.Btn_PaneleGec = new System.Windows.Forms.Button();
            this.Btn_Gonder = new System.Windows.Forms.Button();
            this.Txt_Mesaj = new System.Windows.Forms.TextBox();
            this.Lbl_Mesaj = new System.Windows.Forms.Label();
            this.Txt_Gsm = new System.Windows.Forms.TextBox();
            this.Lbl_Gsm = new System.Windows.Forms.Label();
            this.panelBrowser = new System.Windows.Forms.Panel();
            this.panelUst.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelUst
            // 
            this.panelUst.Controls.Add(this.Lbl_Durum);
            this.panelUst.Controls.Add(this.Btn_PaneleGec);
            this.panelUst.Controls.Add(this.Btn_Gonder);
            this.panelUst.Controls.Add(this.Txt_Mesaj);
            this.panelUst.Controls.Add(this.Lbl_Mesaj);
            this.panelUst.Controls.Add(this.Txt_Gsm);
            this.panelUst.Controls.Add(this.Lbl_Gsm);
            this.panelUst.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelUst.Location = new System.Drawing.Point(0, 0);
            this.panelUst.MinimumSize = new System.Drawing.Size(0, 132);
            this.panelUst.Name = "panelUst";
            this.panelUst.Padding = new System.Windows.Forms.Padding(12, 10, 12, 8);
            this.panelUst.Size = new System.Drawing.Size(984, 132);
            this.panelUst.TabIndex = 0;
            // 
            // Lbl_Durum
            // 
            this.Lbl_Durum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Lbl_Durum.Location = new System.Drawing.Point(15, 10);
            this.Lbl_Durum.Name = "Lbl_Durum";
            this.Lbl_Durum.Size = new System.Drawing.Size(954, 22);
            this.Lbl_Durum.TabIndex = 6;
            this.Lbl_Durum.Text = "Hazırlanıyor...";
            // 
            // Btn_PaneleGec
            // 
            this.Btn_PaneleGec.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_PaneleGec.Location = new System.Drawing.Point(696, 86);
            this.Btn_PaneleGec.Name = "Btn_PaneleGec";
            this.Btn_PaneleGec.Size = new System.Drawing.Size(270, 32);
            this.Btn_PaneleGec.TabIndex = 5;
            this.Btn_PaneleGec.Text = "Toplu SMS sayfasına git";
            this.Btn_PaneleGec.UseVisualStyleBackColor = true;
            // 
            // Btn_Gonder
            // 
            this.Btn_Gonder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Btn_Gonder.Location = new System.Drawing.Point(696, 40);
            this.Btn_Gonder.Name = "Btn_Gonder";
            this.Btn_Gonder.Size = new System.Drawing.Size(270, 36);
            this.Btn_Gonder.TabIndex = 4;
            this.Btn_Gonder.Text = "Gönder (önizleme + gönder)";
            this.Btn_Gonder.UseVisualStyleBackColor = true;
            // 
            // Txt_Mesaj
            // 
            this.Txt_Mesaj.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Txt_Mesaj.Location = new System.Drawing.Point(88, 70);
            this.Txt_Mesaj.Multiline = true;
            this.Txt_Mesaj.Name = "Txt_Mesaj";
            this.Txt_Mesaj.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Txt_Mesaj.Size = new System.Drawing.Size(590, 48);
            this.Txt_Mesaj.TabIndex = 3;
            // 
            // Lbl_Mesaj
            // 
            this.Lbl_Mesaj.AutoSize = true;
            this.Lbl_Mesaj.Location = new System.Drawing.Point(15, 73);
            this.Lbl_Mesaj.Name = "Lbl_Mesaj";
            this.Lbl_Mesaj.Size = new System.Drawing.Size(37, 13);
            this.Lbl_Mesaj.TabIndex = 2;
            this.Lbl_Mesaj.Text = "Mesaj:";
            // 
            // Txt_Gsm
            // 
            this.Txt_Gsm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Txt_Gsm.Location = new System.Drawing.Point(88, 40);
            this.Txt_Gsm.Name = "Txt_Gsm";
            this.Txt_Gsm.Size = new System.Drawing.Size(590, 20);
            this.Txt_Gsm.TabIndex = 1;
            // 
            // Lbl_Gsm
            // 
            this.Lbl_Gsm.AutoSize = true;
            this.Lbl_Gsm.Location = new System.Drawing.Point(15, 43);
            this.Lbl_Gsm.Name = "Lbl_Gsm";
            this.Lbl_Gsm.Size = new System.Drawing.Size(32, 13);
            this.Lbl_Gsm.TabIndex = 0;
            this.Lbl_Gsm.Text = "GSM:";
            // 
            // panelBrowser
            // 
            this.panelBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBrowser.Location = new System.Drawing.Point(0, 132);
            this.panelBrowser.Name = "panelBrowser";
            this.panelBrowser.Padding = new System.Windows.Forms.Padding(12, 0, 12, 12);
            this.panelBrowser.Size = new System.Drawing.Size(984, 529);
            this.panelBrowser.TabIndex = 1;
            // 
            // Frm_SMS_Gonder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 661);
            this.Controls.Add(this.panelBrowser);
            this.Controls.Add(this.panelUst);
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "Frm_SMS_Gonder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SMS Gönder (MyAsist)";
            this.panelUst.ResumeLayout(false);
            this.panelUst.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelUst;
        private System.Windows.Forms.Label Lbl_Gsm;
        private System.Windows.Forms.TextBox Txt_Gsm;
        private System.Windows.Forms.Label Lbl_Mesaj;
        private System.Windows.Forms.TextBox Txt_Mesaj;
        private System.Windows.Forms.Button Btn_Gonder;
        private System.Windows.Forms.Button Btn_PaneleGec;
        private System.Windows.Forms.Label Lbl_Durum;
        private System.Windows.Forms.Panel panelBrowser;
    }
}
