namespace Kolera_Mtsk.Sayfalar
{
    partial class Kursiyer_Detay_Raporlar
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.ustPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._btnYenile = new System.Windows.Forms.Button();
            this._btnOnizle = new System.Windows.Forms.Button();
            this._btnYazdir = new System.Windows.Forms.Button();
            this._btnDuzenle = new System.Windows.Forms.Button();
            this._lblBilgi = new System.Windows.Forms.Label();
            this._dgv = new System.Windows.Forms.DataGridView();
            this.ustPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dgv)).BeginInit();
            this.SuspendLayout();
            // 
            // ustPanel
            // 
            this.ustPanel.Controls.Add(this._btnYenile);
            this.ustPanel.Controls.Add(this._btnOnizle);
            this.ustPanel.Controls.Add(this._btnYazdir);
            this.ustPanel.Controls.Add(this._btnDuzenle);
            this.ustPanel.Controls.Add(this._lblBilgi);
            this.ustPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ustPanel.Location = new System.Drawing.Point(0, 0);
            this.ustPanel.Name = "ustPanel";
            this.ustPanel.Padding = new System.Windows.Forms.Padding(8, 8, 8, 4);
            this.ustPanel.Size = new System.Drawing.Size(964, 45);
            this.ustPanel.TabIndex = 0;
            // 
            // _btnYenile
            // 
            this._btnYenile.Location = new System.Drawing.Point(11, 11);
            this._btnYenile.Name = "_btnYenile";
            this._btnYenile.Size = new System.Drawing.Size(90, 27);
            this._btnYenile.TabIndex = 0;
            this._btnYenile.Text = "Yenile";
            this._btnYenile.UseVisualStyleBackColor = true;
            this._btnYenile.Click += new System.EventHandler(this.BtnYenile_Click);
            // 
            // _btnOnizle
            // 
            this._btnOnizle.Location = new System.Drawing.Point(107, 11);
            this._btnOnizle.Name = "_btnOnizle";
            this._btnOnizle.Size = new System.Drawing.Size(90, 27);
            this._btnOnizle.TabIndex = 1;
            this._btnOnizle.Text = "Onizle";
            this._btnOnizle.UseVisualStyleBackColor = true;
            this._btnOnizle.Click += new System.EventHandler(this.BtnOnizle_Click);
            // 
            // _btnYazdir
            // 
            this._btnYazdir.Location = new System.Drawing.Point(203, 11);
            this._btnYazdir.Name = "_btnYazdir";
            this._btnYazdir.Size = new System.Drawing.Size(90, 27);
            this._btnYazdir.TabIndex = 2;
            this._btnYazdir.Text = "Yazdir";
            this._btnYazdir.UseVisualStyleBackColor = true;
            this._btnYazdir.Click += new System.EventHandler(this.BtnYazdir_Click);
            // 
            // _btnDuzenle
            // 
            this._btnDuzenle.Location = new System.Drawing.Point(299, 11);
            this._btnDuzenle.Name = "_btnDuzenle";
            this._btnDuzenle.Size = new System.Drawing.Size(100, 27);
            this._btnDuzenle.TabIndex = 3;
            this._btnDuzenle.Text = "FRX Duzenle";
            this._btnDuzenle.UseVisualStyleBackColor = true;
            this._btnDuzenle.Click += new System.EventHandler(this.BtnDuzenle_Click);
            // 
            // _lblBilgi
            // 
            this._lblBilgi.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this._lblBilgi.AutoSize = true;
            this._lblBilgi.Location = new System.Drawing.Point(405, 18);
            this._lblBilgi.Name = "_lblBilgi";
            this._lblBilgi.Size = new System.Drawing.Size(115, 13);
            this._lblBilgi.TabIndex = 4;
            this._lblBilgi.Text = "Kursiyer raporu seciniz.";
            // 
            // _dgv
            // 
            this._dgv.AllowUserToAddRows = false;
            this._dgv.AllowUserToDeleteRows = false;
            this._dgv.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._dgv.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dgv.Location = new System.Drawing.Point(0, 45);
            this._dgv.MultiSelect = false;
            this._dgv.Name = "_dgv";
            this._dgv.ReadOnly = true;
            this._dgv.RowHeadersVisible = false;
            this._dgv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._dgv.Size = new System.Drawing.Size(964, 556);
            this._dgv.TabIndex = 1;
            this._dgv.SelectionChanged += new System.EventHandler(this.Dgv_SelectionChanged);
            // 
            // Kursiyer_Detay_Raporlar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 601);
            this.Controls.Add(this._dgv);
            this.Controls.Add(this.ustPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Kursiyer_Detay_Raporlar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kursiyer Raporlari";
            this.Load += new System.EventHandler(this.KursiyerRaporSecimForm_Load);
            this.ustPanel.ResumeLayout(false);
            this.ustPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._dgv)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel ustPanel;
        private System.Windows.Forms.DataGridView _dgv;
        private System.Windows.Forms.Button _btnYenile;
        private System.Windows.Forms.Button _btnOnizle;
        private System.Windows.Forms.Button _btnYazdir;
        private System.Windows.Forms.Button _btnDuzenle;
        private System.Windows.Forms.Label _lblBilgi;
    }
}
