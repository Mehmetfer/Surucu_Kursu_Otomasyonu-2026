namespace Kolera_Mtsk.Sayfalar
{
    partial class SistemParametreForm
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
            this.lblBilgi = new System.Windows.Forms.Label();
            this.dgvParametreler = new System.Windows.Forms.DataGridView();
            this.pnlAlt = new System.Windows.Forms.Panel();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.btnYeni = new System.Windows.Forms.Button();
            this.btnSil = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParametreler)).BeginInit();
            this.pnlAlt.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblBilgi
            // 
            this.lblBilgi.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBilgi.Height = 48;
            this.lblBilgi.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.lblBilgi.Text = "Ornek anahtarlar: DONEM_YILI_MIN_OFFSET, DONEM_YILI_MAX_OFFSET, DONEM_YILI_OZEL_L" +
    "ISTE (orn: 2027,2028).";
            this.lblBilgi.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dgvParametreler
            // 
            this.dgvParametreler.AllowUserToAddRows = false;
            this.dgvParametreler.AllowUserToDeleteRows = false;
            this.dgvParametreler.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvParametreler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvParametreler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvParametreler.Location = new System.Drawing.Point(0, 48);
            this.dgvParametreler.MultiSelect = false;
            this.dgvParametreler.Name = "dgvParametreler";
            this.dgvParametreler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvParametreler.Size = new System.Drawing.Size(884, 459);
            this.dgvParametreler.TabIndex = 1;
            // 
            // pnlAlt
            // 
            this.pnlAlt.Controls.Add(this.btnSil);
            this.pnlAlt.Controls.Add(this.btnYeni);
            this.pnlAlt.Controls.Add(this.btnKaydet);
            this.pnlAlt.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlAlt.Location = new System.Drawing.Point(0, 507);
            this.pnlAlt.Name = "pnlAlt";
            this.pnlAlt.Size = new System.Drawing.Size(884, 54);
            this.pnlAlt.TabIndex = 2;
            // 
            // btnKaydet
            // 
            this.btnKaydet.Location = new System.Drawing.Point(12, 11);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(100, 32);
            this.btnKaydet.TabIndex = 0;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            // 
            // btnYeni
            // 
            this.btnYeni.Location = new System.Drawing.Point(118, 11);
            this.btnYeni.Name = "btnYeni";
            this.btnYeni.Size = new System.Drawing.Size(100, 32);
            this.btnYeni.TabIndex = 1;
            this.btnYeni.Text = "Yeni Satir";
            this.btnYeni.UseVisualStyleBackColor = true;
            // 
            // btnSil
            // 
            this.btnSil.Location = new System.Drawing.Point(224, 11);
            this.btnSil.Name = "btnSil";
            this.btnSil.Size = new System.Drawing.Size(100, 32);
            this.btnSil.TabIndex = 2;
            this.btnSil.Text = "Satir Sil";
            this.btnSil.UseVisualStyleBackColor = true;
            // 
            // SistemParametreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.dgvParametreler);
            this.Controls.Add(this.pnlAlt);
            this.Controls.Add(this.lblBilgi);
            this.MinimizeBox = false;
            this.Name = "SistemParametreForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sistem Parametreleri";
            ((System.ComponentModel.ISupportInitialize)(this.dgvParametreler)).EndInit();
            this.pnlAlt.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblBilgi;
        private System.Windows.Forms.DataGridView dgvParametreler;
        private System.Windows.Forms.Panel pnlAlt;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnYeni;
        private System.Windows.Forms.Button btnSil;
    }
}
