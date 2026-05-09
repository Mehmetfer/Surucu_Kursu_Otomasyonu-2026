namespace Kolera_Mtsk.Sayfalar
{
    partial class IstatistikForm
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
            this.lblBas = new System.Windows.Forms.Label();
            this.dtBaslangic = new System.Windows.Forms.DateTimePicker();
            this.lblBit = new System.Windows.Forms.Label();
            this.dtBitis = new System.Windows.Forms.DateTimePicker();
            this.lblSinif = new System.Windows.Forms.Label();
            this.cmbSertifika = new System.Windows.Forms.ComboBox();
            this.btnHazirla = new System.Windows.Forms.Button();
            this.dgvIstatistik = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIstatistik)).BeginInit();
            this.SuspendLayout();
            // 
            // lblBas
            // 
            this.lblBas.AutoSize = true;
            this.lblBas.Location = new System.Drawing.Point(12, 16);
            this.lblBas.Name = "lblBas";
            this.lblBas.Size = new System.Drawing.Size(76, 13);
            this.lblBas.TabIndex = 0;
            this.lblBas.Text = "Kayit Baslangic";
            // 
            // dtBaslangic
            // 
            this.dtBaslangic.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtBaslangic.Location = new System.Drawing.Point(94, 12);
            this.dtBaslangic.Name = "dtBaslangic";
            this.dtBaslangic.Size = new System.Drawing.Size(96, 20);
            this.dtBaslangic.TabIndex = 1;
            // 
            // lblBit
            // 
            this.lblBit.AutoSize = true;
            this.lblBit.Location = new System.Drawing.Point(208, 16);
            this.lblBit.Name = "lblBit";
            this.lblBit.Size = new System.Drawing.Size(52, 13);
            this.lblBit.TabIndex = 2;
            this.lblBit.Text = "Kayit Bitis";
            // 
            // dtBitis
            // 
            this.dtBitis.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtBitis.Location = new System.Drawing.Point(266, 12);
            this.dtBitis.Name = "dtBitis";
            this.dtBitis.Size = new System.Drawing.Size(96, 20);
            this.dtBitis.TabIndex = 3;
            // 
            // lblSinif
            // 
            this.lblSinif.AutoSize = true;
            this.lblSinif.Location = new System.Drawing.Point(378, 16);
            this.lblSinif.Name = "lblSinif";
            this.lblSinif.Size = new System.Drawing.Size(73, 13);
            this.lblSinif.TabIndex = 4;
            this.lblSinif.Text = "Sertifika Sinifi";
            // 
            // cmbSertifika
            // 
            this.cmbSertifika.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSertifika.FormattingEnabled = true;
            this.cmbSertifika.Location = new System.Drawing.Point(457, 12);
            this.cmbSertifika.Name = "cmbSertifika";
            this.cmbSertifika.Size = new System.Drawing.Size(130, 21);
            this.cmbSertifika.TabIndex = 5;
            // 
            // btnHazirla
            // 
            this.btnHazirla.Location = new System.Drawing.Point(603, 10);
            this.btnHazirla.Name = "btnHazirla";
            this.btnHazirla.Size = new System.Drawing.Size(120, 24);
            this.btnHazirla.TabIndex = 6;
            this.btnHazirla.Text = "Istatistik Icin Hazirla";
            this.btnHazirla.UseVisualStyleBackColor = true;
            // 
            // dgvIstatistik
            // 
            this.dgvIstatistik.AllowUserToAddRows = false;
            this.dgvIstatistik.AllowUserToDeleteRows = false;
            this.dgvIstatistik.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvIstatistik.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIstatistik.Location = new System.Drawing.Point(15, 47);
            this.dgvIstatistik.Name = "dgvIstatistik";
            this.dgvIstatistik.ReadOnly = true;
            this.dgvIstatistik.RowHeadersVisible = false;
            this.dgvIstatistik.Size = new System.Drawing.Size(1112, 536);
            this.dgvIstatistik.TabIndex = 7;
            // 
            // IstatistikForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1140, 595);
            this.Controls.Add(this.dgvIstatistik);
            this.Controls.Add(this.btnHazirla);
            this.Controls.Add(this.cmbSertifika);
            this.Controls.Add(this.lblSinif);
            this.Controls.Add(this.dtBitis);
            this.Controls.Add(this.lblBit);
            this.Controls.Add(this.dtBaslangic);
            this.Controls.Add(this.lblBas);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "IstatistikForm";
            this.Text = "IstatistikForm";
            ((System.ComponentModel.ISupportInitialize)(this.dgvIstatistik)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblBas;
        private System.Windows.Forms.DateTimePicker dtBaslangic;
        private System.Windows.Forms.Label lblBit;
        private System.Windows.Forms.DateTimePicker dtBitis;
        private System.Windows.Forms.Label lblSinif;
        private System.Windows.Forms.ComboBox cmbSertifika;
        private System.Windows.Forms.Button btnHazirla;
        private System.Windows.Forms.DataGridView dgvIstatistik;
    }
}
