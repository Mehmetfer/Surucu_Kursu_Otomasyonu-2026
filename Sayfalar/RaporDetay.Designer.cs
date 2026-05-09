namespace Kolera_Mtsk.Sayfalar
{
    partial class RaporDetay
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

        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.Btn_Onizle = new System.Windows.Forms.ToolStripButton();
            this.Btn_Yazdir = new System.Windows.Forms.ToolStripButton();
            this.panelUst = new System.Windows.Forms.Panel();
            this.Lbl_SeciliKayit = new System.Windows.Forms.Label();
            this.panelAlt = new System.Windows.Forms.Panel();
            this.Btn_DuzenleAlt = new System.Windows.Forms.Button();
            this.Btn_OnizleAlt = new System.Windows.Forms.Button();
            this.Btn_YazdirAlt = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Dgv_Raporlar = new System.Windows.Forms.DataGridView();
            this.panelSag = new System.Windows.Forms.Panel();
            this.Btn_Html = new System.Windows.Forms.Button();
            this.Btn_Jpg = new System.Windows.Forms.Button();
            this.Btn_Doc = new System.Windows.Forms.Button();
            this.Btn_XlsSag = new System.Windows.Forms.Button();
            this.Btn_PdfSag = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Lbl_Durum = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            this.panelUst.SuspendLayout();
            this.panelAlt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Dgv_Raporlar)).BeginInit();
            this.panelSag.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Btn_Onizle,
            this.Btn_Yazdir});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(924, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            this.toolStrip1.Visible = false;
            // 
            // Btn_Onizle
            // 
            this.Btn_Onizle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Btn_Onizle.Name = "Btn_Onizle";
            this.Btn_Onizle.Size = new System.Drawing.Size(44, 22);
            this.Btn_Onizle.Text = "Onizle";
            // 
            // Btn_Yazdir
            // 
            this.Btn_Yazdir.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Btn_Yazdir.Name = "Btn_Yazdir";
            this.Btn_Yazdir.Size = new System.Drawing.Size(42, 22);
            this.Btn_Yazdir.Text = "Yazdir";
            // 
            // panelUst
            // 
            this.panelUst.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panelUst.Controls.Add(this.Lbl_SeciliKayit);
            this.panelUst.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelUst.Location = new System.Drawing.Point(0, 0);
            this.panelUst.Name = "panelUst";
            this.panelUst.Size = new System.Drawing.Size(1071, 50);
            this.panelUst.TabIndex = 1;
            // 
            // Lbl_SeciliKayit
            // 
            this.Lbl_SeciliKayit.AutoSize = true;
            this.Lbl_SeciliKayit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Lbl_SeciliKayit.ForeColor = System.Drawing.Color.White;
            this.Lbl_SeciliKayit.Location = new System.Drawing.Point(12, 11);
            this.Lbl_SeciliKayit.Name = "Lbl_SeciliKayit";
            this.Lbl_SeciliKayit.Size = new System.Drawing.Size(87, 20);
            this.Lbl_SeciliKayit.TabIndex = 0;
            this.Lbl_SeciliKayit.Text = "SECILI ...";
            // 
            // panelAlt
            // 
            this.panelAlt.Controls.Add(this.Btn_DuzenleAlt);
            this.panelAlt.Controls.Add(this.Btn_OnizleAlt);
            this.panelAlt.Controls.Add(this.Btn_YazdirAlt);
            this.panelAlt.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelAlt.Location = new System.Drawing.Point(0, 444);
            this.panelAlt.Name = "panelAlt";
            this.panelAlt.Size = new System.Drawing.Size(1071, 39);
            this.panelAlt.TabIndex = 3;
            // 
            // Btn_DuzenleAlt
            // 
            this.Btn_DuzenleAlt.Location = new System.Drawing.Point(10, 8);
            this.Btn_DuzenleAlt.Name = "Btn_DuzenleAlt";
            this.Btn_DuzenleAlt.Size = new System.Drawing.Size(86, 24);
            this.Btn_DuzenleAlt.TabIndex = 0;
            this.Btn_DuzenleAlt.Text = "Düzenle";
            this.Btn_DuzenleAlt.UseVisualStyleBackColor = true;
            // 
            // Btn_OnizleAlt
            // 
            this.Btn_OnizleAlt.Location = new System.Drawing.Point(102, 8);
            this.Btn_OnizleAlt.Name = "Btn_OnizleAlt";
            this.Btn_OnizleAlt.Size = new System.Drawing.Size(86, 24);
            this.Btn_OnizleAlt.TabIndex = 1;
            this.Btn_OnizleAlt.Text = "Önizleme";
            this.Btn_OnizleAlt.UseVisualStyleBackColor = true;
            // 
            // Btn_YazdirAlt
            // 
            this.Btn_YazdirAlt.Location = new System.Drawing.Point(194, 8);
            this.Btn_YazdirAlt.Name = "Btn_YazdirAlt";
            this.Btn_YazdirAlt.Size = new System.Drawing.Size(86, 24);
            this.Btn_YazdirAlt.TabIndex = 2;
            this.Btn_YazdirAlt.Text = "Yazdır";
            this.Btn_YazdirAlt.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 50);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Dgv_Raporlar);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelSag);
            this.splitContainer1.Panel2MinSize = 200;
            this.splitContainer1.Size = new System.Drawing.Size(1071, 394);
            this.splitContainer1.SplitterDistance = 380;
            this.splitContainer1.TabIndex = 1;
            // 
            // Dgv_Raporlar
            // 
            this.Dgv_Raporlar.AllowUserToAddRows = false;
            this.Dgv_Raporlar.AllowUserToDeleteRows = false;
            this.Dgv_Raporlar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Dgv_Raporlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Dgv_Raporlar.Location = new System.Drawing.Point(0, 0);
            this.Dgv_Raporlar.Name = "Dgv_Raporlar";
            this.Dgv_Raporlar.Size = new System.Drawing.Size(380, 394);
            this.Dgv_Raporlar.TabIndex = 0;
            // 
            // panelSag
            // 
            this.panelSag.BackColor = System.Drawing.Color.Gainsboro;
            this.panelSag.Controls.Add(this.Btn_Html);
            this.panelSag.Controls.Add(this.Btn_Jpg);
            this.panelSag.Controls.Add(this.Btn_Doc);
            this.panelSag.Controls.Add(this.Btn_XlsSag);
            this.panelSag.Controls.Add(this.Btn_PdfSag);
            this.panelSag.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelSag.Location = new System.Drawing.Point(194, 0);
            this.panelSag.Name = "panelSag";
            this.panelSag.Size = new System.Drawing.Size(74, 394);
            this.panelSag.TabIndex = 0;
            // 
            // Btn_Html
            // 
            this.Btn_Html.Location = new System.Drawing.Point(11, 168);
            this.Btn_Html.Name = "Btn_Html";
            this.Btn_Html.Size = new System.Drawing.Size(54, 26);
            this.Btn_Html.TabIndex = 4;
            this.Btn_Html.Text = "HTML";
            this.Btn_Html.UseVisualStyleBackColor = true;
            // 
            // Btn_Jpg
            // 
            this.Btn_Jpg.Location = new System.Drawing.Point(11, 136);
            this.Btn_Jpg.Name = "Btn_Jpg";
            this.Btn_Jpg.Size = new System.Drawing.Size(54, 26);
            this.Btn_Jpg.TabIndex = 3;
            this.Btn_Jpg.Text = "JPG";
            this.Btn_Jpg.UseVisualStyleBackColor = true;
            // 
            // Btn_Doc
            // 
            this.Btn_Doc.Location = new System.Drawing.Point(11, 104);
            this.Btn_Doc.Name = "Btn_Doc";
            this.Btn_Doc.Size = new System.Drawing.Size(54, 26);
            this.Btn_Doc.TabIndex = 2;
            this.Btn_Doc.Text = "DOC";
            this.Btn_Doc.UseVisualStyleBackColor = true;
            // 
            // Btn_XlsSag
            // 
            this.Btn_XlsSag.Location = new System.Drawing.Point(11, 72);
            this.Btn_XlsSag.Name = "Btn_XlsSag";
            this.Btn_XlsSag.Size = new System.Drawing.Size(54, 26);
            this.Btn_XlsSag.TabIndex = 1;
            this.Btn_XlsSag.Text = "XLS";
            this.Btn_XlsSag.UseVisualStyleBackColor = true;
            // 
            // Btn_PdfSag
            // 
            this.Btn_PdfSag.Location = new System.Drawing.Point(11, 40);
            this.Btn_PdfSag.Name = "Btn_PdfSag";
            this.Btn_PdfSag.Size = new System.Drawing.Size(54, 26);
            this.Btn_PdfSag.TabIndex = 0;
            this.Btn_PdfSag.Text = "PDF";
            this.Btn_PdfSag.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Lbl_Durum});
            this.statusStrip1.Location = new System.Drawing.Point(0, 483);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1071, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Lbl_Durum
            // 
            this.Lbl_Durum.Name = "Lbl_Durum";
            this.Lbl_Durum.Size = new System.Drawing.Size(76, 17);
            this.Lbl_Durum.Text = "Rapor seciniz";
            // 
            // RaporDetay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1071, 505);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelAlt);
            this.Controls.Add(this.panelUst);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "RaporDetay";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Rapor Detay";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panelUst.ResumeLayout(false);
            this.panelUst.PerformLayout();
            this.panelAlt.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Dgv_Raporlar)).EndInit();
            this.panelSag.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton Btn_Onizle;
        private System.Windows.Forms.ToolStripButton Btn_Yazdir;
        private System.Windows.Forms.Panel panelUst;
        private System.Windows.Forms.Label Lbl_SeciliKayit;
        private System.Windows.Forms.Panel panelAlt;
        private System.Windows.Forms.Button Btn_DuzenleAlt;
        private System.Windows.Forms.Button Btn_OnizleAlt;
        private System.Windows.Forms.Button Btn_YazdirAlt;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView Dgv_Raporlar;
        private System.Windows.Forms.Panel panelSag;
        private System.Windows.Forms.Button Btn_Html;
        private System.Windows.Forms.Button Btn_Jpg;
        private System.Windows.Forms.Button Btn_Doc;
        private System.Windows.Forms.Button Btn_XlsSag;
        private System.Windows.Forms.Button Btn_PdfSag;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel Lbl_Durum;
    }
}
