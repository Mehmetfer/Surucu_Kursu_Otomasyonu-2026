namespace Kolera_Mtsk.Sayfalar
{
    partial class Raporlar
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
            this.panelUst = new System.Windows.Forms.Panel();
            this.Btn_KaydetGrid = new System.Windows.Forms.Button();
            this.Cmb_RaporGrubu = new System.Windows.Forms.ComboBox();
            this.Lbl_RaporGrubu = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Dgv_Raporlar = new System.Windows.Forms.DataGridView();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Lbl_Durum = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            this.panelUst.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
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
            this.toolStrip1.Size = new System.Drawing.Size(1167, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
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
            // panelUst
            // 
            this.panelUst.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.panelUst.Controls.Add(this.Btn_KaydetGrid);
            this.panelUst.Controls.Add(this.Cmb_RaporGrubu);
            this.panelUst.Controls.Add(this.Lbl_RaporGrubu);
            this.panelUst.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelUst.Location = new System.Drawing.Point(0, 25);
            this.panelUst.Name = "panelUst";
            this.panelUst.Size = new System.Drawing.Size(1167, 42);
            this.panelUst.TabIndex = 3;
            // 
            // Btn_KaydetGrid
            // 
            this.Btn_KaydetGrid.Location = new System.Drawing.Point(339, 9);
            this.Btn_KaydetGrid.Name = "Btn_KaydetGrid";
            this.Btn_KaydetGrid.Size = new System.Drawing.Size(104, 25);
            this.Btn_KaydetGrid.TabIndex = 2;
            this.Btn_KaydetGrid.Text = "Kaydet";
            this.Btn_KaydetGrid.UseVisualStyleBackColor = true;
            // 
            // Cmb_RaporGrubu
            // 
            this.Cmb_RaporGrubu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Cmb_RaporGrubu.FormattingEnabled = true;
            this.Cmb_RaporGrubu.Location = new System.Drawing.Point(107, 10);
            this.Cmb_RaporGrubu.Name = "Cmb_RaporGrubu";
            this.Cmb_RaporGrubu.Size = new System.Drawing.Size(226, 21);
            this.Cmb_RaporGrubu.TabIndex = 1;
            // 
            // Lbl_RaporGrubu
            // 
            this.Lbl_RaporGrubu.AutoSize = true;
            this.Lbl_RaporGrubu.ForeColor = System.Drawing.Color.White;
            this.Lbl_RaporGrubu.Location = new System.Drawing.Point(12, 14);
            this.Lbl_RaporGrubu.Name = "Lbl_RaporGrubu";
            this.Lbl_RaporGrubu.Size = new System.Drawing.Size(71, 13);
            this.Lbl_RaporGrubu.TabIndex = 0;
            this.Lbl_RaporGrubu.Text = "Rapor Grubu:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 67);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Dgv_Raporlar);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainer1.Panel1MinSize = 200;
            this.splitContainer1.Panel2MinSize = 200;
            this.splitContainer1.Size = new System.Drawing.Size(1167, 395);
            this.splitContainer1.SplitterDistance = 582;
            this.splitContainer1.TabIndex = 1;
            // 
            // Dgv_Raporlar
            // 
            this.Dgv_Raporlar.AllowUserToOrderColumns = true;
            this.Dgv_Raporlar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Dgv_Raporlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Dgv_Raporlar.Location = new System.Drawing.Point(0, 0);
            this.Dgv_Raporlar.Name = "Dgv_Raporlar";
            this.Dgv_Raporlar.Size = new System.Drawing.Size(582, 395);
            this.Dgv_Raporlar.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Lbl_Durum});
            this.statusStrip1.Location = new System.Drawing.Point(0, 462);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1167, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Lbl_Durum
            // 
            this.Lbl_Durum.Name = "Lbl_Durum";
            this.Lbl_Durum.Size = new System.Drawing.Size(76, 17);
            this.Lbl_Durum.Text = "Rapor seciniz";
            // 
            // Raporlar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1167, 484);
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panelUst);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "Raporlar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Raporlar";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panelUst.ResumeLayout(false);
            this.panelUst.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
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
        private System.Windows.Forms.Panel panelUst;
        private System.Windows.Forms.Button Btn_KaydetGrid;
        private System.Windows.Forms.ComboBox Cmb_RaporGrubu;
        private System.Windows.Forms.Label Lbl_RaporGrubu;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView Dgv_Raporlar;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel Lbl_Durum;
    }
}