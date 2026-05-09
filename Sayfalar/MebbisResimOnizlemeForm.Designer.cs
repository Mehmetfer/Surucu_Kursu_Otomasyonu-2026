namespace Kolera_Mtsk.Sayfalar
{
    partial class MebbisResimOnizlemeForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label _lblInfo;
        private System.Windows.Forms.PictureBox _picture;
        private System.Windows.Forms.Panel _altPanel;
        private System.Windows.Forms.Button _btnAktar;
        private System.Windows.Forms.Button _btnVazgec;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._lblInfo = new System.Windows.Forms.Label();
            this._picture = new System.Windows.Forms.PictureBox();
            this._altPanel = new System.Windows.Forms.Panel();
            this._btnAktar = new System.Windows.Forms.Button();
            this._btnVazgec = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this._picture)).BeginInit();
            this._altPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblInfo
            // 
            this._lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this._lblInfo.Height = 45;
            this._lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this._lblInfo.Padding = new System.Windows.Forms.Padding(12, 0, 0, 0);
            // 
            // _picture
            // 
            this._picture.BackColor = System.Drawing.Color.White;
            this._picture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._picture.Dock = System.Windows.Forms.DockStyle.Fill;
            this._picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            // 
            // _altPanel
            // 
            this._altPanel.Controls.Add(this._btnAktar);
            this._altPanel.Controls.Add(this._btnVazgec);
            this._altPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._altPanel.Height = 70;
            // 
            // _btnAktar
            // 
            this._btnAktar.Location = new System.Drawing.Point(380, 16);
            this._btnAktar.Name = "_btnAktar";
            this._btnAktar.Size = new System.Drawing.Size(120, 36);
            this._btnAktar.Text = "Aktar";
            this._btnAktar.UseVisualStyleBackColor = true;
            // 
            // _btnVazgec
            // 
            this._btnVazgec.Location = new System.Drawing.Point(510, 16);
            this._btnVazgec.Name = "_btnVazgec";
            this._btnVazgec.Size = new System.Drawing.Size(120, 36);
            this._btnVazgec.Text = "Vazgec";
            this._btnVazgec.UseVisualStyleBackColor = true;
            // 
            // MebbisResimOnizlemeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 520);
            this.Controls.Add(this._picture);
            this.Controls.Add(this._lblInfo);
            this.Controls.Add(this._altPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MebbisResimOnizlemeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MebbisResimOnizlemeForm";
            ((System.ComponentModel.ISupportInitialize)(this._picture)).EndInit();
            this._altPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
