namespace Kolera_Mtsk.Sayfalar.EkProgramlar
{
    partial class AylikTakvimForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelTakvim;
        private System.Windows.Forms.Button btnOncekiAy;
        private System.Windows.Forms.Button btnSonrakiAy;
        private System.Windows.Forms.Label lblAy;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTakvim = new System.Windows.Forms.Panel();
            this.btnOncekiAy = new System.Windows.Forms.Button();
            this.btnSonrakiAy = new System.Windows.Forms.Button();
            this.lblAy = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // panelTakvim
            // 
            this.panelTakvim.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTakvim.AutoScroll = true;
            this.panelTakvim.Location = new System.Drawing.Point(12, 52);
            this.panelTakvim.Name = "panelTakvim";
            this.panelTakvim.Size = new System.Drawing.Size(1160, 597);
            this.panelTakvim.TabIndex = 0;
            // 
            // btnOncekiAy
            // 
            this.btnOncekiAy.Location = new System.Drawing.Point(12, 12);
            this.btnOncekiAy.Name = "btnOncekiAy";
            this.btnOncekiAy.Size = new System.Drawing.Size(92, 30);
            this.btnOncekiAy.TabIndex = 1;
            this.btnOncekiAy.Text = "< Onceki Ay";
            this.btnOncekiAy.UseVisualStyleBackColor = true;
            // 
            // btnSonrakiAy
            // 
            this.btnSonrakiAy.Location = new System.Drawing.Point(110, 12);
            this.btnSonrakiAy.Name = "btnSonrakiAy";
            this.btnSonrakiAy.Size = new System.Drawing.Size(92, 30);
            this.btnSonrakiAy.TabIndex = 2;
            this.btnSonrakiAy.Text = "Sonraki Ay >";
            this.btnSonrakiAy.UseVisualStyleBackColor = true;
            // 
            // lblAy
            // 
            this.lblAy.AutoSize = true;
            this.lblAy.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblAy.Location = new System.Drawing.Point(220, 17);
            this.lblAy.Name = "lblAy";
            this.lblAy.Size = new System.Drawing.Size(69, 20);
            this.lblAy.TabIndex = 3;
            this.lblAy.Text = "Ay Yil";
            // 
            // AylikTakvimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 661);
            this.Controls.Add(this.lblAy);
            this.Controls.Add(this.btnSonrakiAy);
            this.Controls.Add(this.btnOncekiAy);
            this.Controls.Add(this.panelTakvim);
            this.Name = "AylikTakvimForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Ders Programi Cizelgesi";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
