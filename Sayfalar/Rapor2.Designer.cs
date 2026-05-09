namespace Kolera_Mtsk.Sayfalar
{
    partial class Rapor2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.Btn_Onizle = new System.Windows.Forms.ToolStripButton();
            this.Btn_Yazdir = new System.Windows.Forms.ToolStripButton();
            this.Btn_Pdf = new System.Windows.Forms.ToolStripButton();
            this.Btn_Xls = new System.Windows.Forms.ToolStripButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Dgv_Raporlar = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Lbl_Durum = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Dgv_Raporlar)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Btn_Onizle,
            this.Btn_Yazdir,
            this.Btn_Pdf,
            this.Btn_Xls});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1298, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
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
            // Btn_Pdf
            // 
            this.Btn_Pdf.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Btn_Pdf.Name = "Btn_Pdf";
            this.Btn_Pdf.Size = new System.Drawing.Size(32, 22);
            this.Btn_Pdf.Text = "PDF";
            // 
            // Btn_Xls
            // 
            this.Btn_Xls.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.Btn_Xls.Name = "Btn_Xls";
            this.Btn_Xls.Size = new System.Drawing.Size(30, 22);
            this.Btn_Xls.Text = "XLS";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Dgv_Raporlar);
            this.splitContainer1.Size = new System.Drawing.Size(1298, 791);
            this.splitContainer1.SplitterDistance = 512;
            this.splitContainer1.TabIndex = 4;
            // 
            // Dgv_Raporlar
            // 
            this.Dgv_Raporlar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Dgv_Raporlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Dgv_Raporlar.Location = new System.Drawing.Point(0, 0);
            this.Dgv_Raporlar.Name = "Dgv_Raporlar";
            this.Dgv_Raporlar.Size = new System.Drawing.Size(512, 791);
            this.Dgv_Raporlar.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Lbl_Durum});
            this.statusStrip1.Location = new System.Drawing.Point(0, 791);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1298, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Lbl_Durum
            // 
            this.Lbl_Durum.Name = "Lbl_Durum";
            this.Lbl_Durum.Size = new System.Drawing.Size(76, 17);
            this.Lbl_Durum.Text = "Rapor seciniz";
            // 
            // Rapor2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1298, 813);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Rapor2";
            this.Text = "Rapor2";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Dgv_Raporlar)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton Btn_Onizle;
        private System.Windows.Forms.ToolStripButton Btn_Yazdir;
        private System.Windows.Forms.ToolStripButton Btn_Pdf;
        private System.Windows.Forms.ToolStripButton Btn_Xls;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView Dgv_Raporlar;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel Lbl_Durum;
    }
}