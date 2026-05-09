namespace Kolera_Mtsk.Sayfalar
{
    partial class WebCamResimForm
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
            this._lblInfo = new System.Windows.Forms.Label();
            this._dropPanel = new System.Windows.Forms.Panel();
            this._picture = new System.Windows.Forms.PictureBox();
            this._webCamView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this._bottomPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._btnSec = new System.Windows.Forms.Button();
            this._btnTemizle = new System.Windows.Forms.Button();
            this._btnKaydet = new System.Windows.Forms.Button();
            this._btnSifirla = new System.Windows.Forms.Button();
            this._chkMirror = new System.Windows.Forms.CheckBox();
            this._cmbCameraSource = new System.Windows.Forms.ComboBox();
            this._btnWebcamAl = new System.Windows.Forms.Button();
            this._dropPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._picture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this._webCamView)).BeginInit();
            this._bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _lblInfo
            // 
            this._lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this._lblInfo.Location = new System.Drawing.Point(0, 0);
            this._lblInfo.Name = "_lblInfo";
            this._lblInfo.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this._lblInfo.Size = new System.Drawing.Size(1069, 48);
            this._lblInfo.TabIndex = 0;
            this._lblInfo.Text = "Resmi surukleyip birakin veya \'Resim Sec\' ile yukleyin. Kaydederken webcam gorunt" +
    "usu gibi normalize edilir. Fare tekerlegi ile zoom, surukleyerek tasima yapabili" +
    "rsin.";
            this._lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _dropPanel
            // 
            this._dropPanel.AllowDrop = true;
            this._dropPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this._dropPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._dropPanel.Controls.Add(this._picture);
            this._dropPanel.Controls.Add(this._webCamView);
            this._dropPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dropPanel.Location = new System.Drawing.Point(0, 48);
            this._dropPanel.Name = "_dropPanel";
            this._dropPanel.Padding = new System.Windows.Forms.Padding(12);
            this._dropPanel.Size = new System.Drawing.Size(1069, 457);
            this._dropPanel.TabIndex = 1;
            // 
            // _picture
            // 
            this._picture.BackColor = System.Drawing.Color.White;
            this._picture.Dock = System.Windows.Forms.DockStyle.Fill;
            this._picture.Location = new System.Drawing.Point(12, 34);
            this._picture.Name = "_picture";
            this._picture.Size = new System.Drawing.Size(1043, 409);
            this._picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._picture.TabIndex = 0;
            // 
            // _webCamView
            // 
            this._webCamView.AllowExternalDrop = true;
            this._webCamView.CreationProperties = null;
            this._webCamView.DefaultBackgroundColor = System.Drawing.Color.Black;
            this._webCamView.Dock = System.Windows.Forms.DockStyle.Top;
            this._webCamView.Location = new System.Drawing.Point(12, 12);
            this._webCamView.Name = "_webCamView";
            this._webCamView.Size = new System.Drawing.Size(1043, 22);
            this._webCamView.TabIndex = 1;
            this._webCamView.ZoomFactor = 1D;
            // 
            // _bottomPanel
            // 
            this._bottomPanel.Controls.Add(this._btnSec);
            this._bottomPanel.Controls.Add(this._btnTemizle);
            this._bottomPanel.Controls.Add(this._btnKaydet);
            this._bottomPanel.Controls.Add(this._btnSifirla);
            this._bottomPanel.Controls.Add(this._chkMirror);
            this._bottomPanel.Controls.Add(this._cmbCameraSource);
            this._bottomPanel.Controls.Add(this._btnWebcamAl);
            this._bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._bottomPanel.Location = new System.Drawing.Point(0, 505);
            this._bottomPanel.Name = "_bottomPanel";
            this._bottomPanel.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this._bottomPanel.Size = new System.Drawing.Size(1069, 56);
            this._bottomPanel.TabIndex = 2;
            // 
            // _btnSec
            // 
            this._btnSec.Location = new System.Drawing.Point(13, 11);
            this._btnSec.Name = "_btnSec";
            this._btnSec.Size = new System.Drawing.Size(120, 32);
            this._btnSec.TabIndex = 0;
            this._btnSec.Text = "Resim Sec";
            this._btnSec.UseVisualStyleBackColor = true;
            // 
            // _btnTemizle
            // 
            this._btnTemizle.Location = new System.Drawing.Point(139, 11);
            this._btnTemizle.Name = "_btnTemizle";
            this._btnTemizle.Size = new System.Drawing.Size(120, 32);
            this._btnTemizle.TabIndex = 1;
            this._btnTemizle.Text = "Temizle";
            this._btnTemizle.UseVisualStyleBackColor = true;
            // 
            // _btnKaydet
            // 
            this._btnKaydet.Location = new System.Drawing.Point(265, 11);
            this._btnKaydet.Name = "_btnKaydet";
            this._btnKaydet.Size = new System.Drawing.Size(190, 32);
            this._btnKaydet.TabIndex = 2;
            this._btnKaydet.Text = "WebCam Olarak Kaydet";
            this._btnKaydet.UseVisualStyleBackColor = true;
            // 
            // _btnSifirla
            // 
            this._btnSifirla.Location = new System.Drawing.Point(461, 11);
            this._btnSifirla.Name = "_btnSifirla";
            this._btnSifirla.Size = new System.Drawing.Size(100, 32);
            this._btnSifirla.TabIndex = 3;
            this._btnSifirla.Text = "Sifirla";
            this._btnSifirla.UseVisualStyleBackColor = true;
            // 
            // _chkMirror
            // 
            this._chkMirror.AutoSize = true;
            this._chkMirror.Checked = true;
            this._chkMirror.CheckState = System.Windows.Forms.CheckState.Checked;
            this._chkMirror.Location = new System.Drawing.Point(567, 11);
            this._chkMirror.Name = "_chkMirror";
            this._chkMirror.Size = new System.Drawing.Size(85, 17);
            this._chkMirror.TabIndex = 4;
            this._chkMirror.Text = "Ayna (Mirror)";
            this._chkMirror.UseVisualStyleBackColor = true;
            // 
            // _cmbCameraSource
            // 
            this._cmbCameraSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cmbCameraSource.FormattingEnabled = true;
            this._cmbCameraSource.Location = new System.Drawing.Point(658, 11);
            this._cmbCameraSource.Name = "_cmbCameraSource";
            this._cmbCameraSource.Size = new System.Drawing.Size(220, 21);
            this._cmbCameraSource.TabIndex = 5;
            // 
            // _btnWebcamAl
            // 
            this._btnWebcamAl.Location = new System.Drawing.Point(884, 11);
            this._btnWebcamAl.Name = "_btnWebcamAl";
            this._btnWebcamAl.Size = new System.Drawing.Size(110, 32);
            this._btnWebcamAl.TabIndex = 6;
            this._btnWebcamAl.Text = "WebCam\'den Al";
            this._btnWebcamAl.UseVisualStyleBackColor = true;
            // 
            // WebCamResimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1069, 561);
            this.Controls.Add(this._dropPanel);
            this.Controls.Add(this._bottomPanel);
            this.Controls.Add(this._lblInfo);
            this.MinimumSize = new System.Drawing.Size(760, 560);
            this.Name = "WebCamResimForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Web Cam Resim";
            this._dropPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._picture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this._webCamView)).EndInit();
            this._bottomPanel.ResumeLayout(false);
            this._bottomPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label _lblInfo;
        private System.Windows.Forms.Panel _dropPanel;
        private System.Windows.Forms.PictureBox _picture;
        private System.Windows.Forms.FlowLayoutPanel _bottomPanel;
        private System.Windows.Forms.Button _btnSec;
        private System.Windows.Forms.Button _btnTemizle;
        private System.Windows.Forms.Button _btnKaydet;
        private System.Windows.Forms.Button _btnSifirla;
        private System.Windows.Forms.CheckBox _chkMirror;
        private System.Windows.Forms.ComboBox _cmbCameraSource;
        private System.Windows.Forms.Button _btnWebcamAl;
        private Microsoft.Web.WebView2.WinForms.WebView2 _webCamView;
    }
}
