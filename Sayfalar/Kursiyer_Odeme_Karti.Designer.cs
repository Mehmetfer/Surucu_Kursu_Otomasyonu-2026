namespace Kolera_Mtsk.Sayfalar
{
    partial class Kursiyer_Odeme_Karti
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
            this.lblBaslik = new System.Windows.Forms.Label();
            this.grpKursiyer = new System.Windows.Forms.GroupBox();
            this.txtKalanBorc = new System.Windows.Forms.TextBox();
            this.lblKalanBorc = new System.Windows.Forms.Label();
            this.txtToplamOdenen = new System.Windows.Forms.TextBox();
            this.lblToplamOdenen = new System.Windows.Forms.Label();
            this.txtToplamBorc = new System.Windows.Forms.TextBox();
            this.lblToplamBorc = new System.Windows.Forms.Label();
            this.txtAciklama = new System.Windows.Forms.TextBox();
            this.lblAciklama = new System.Windows.Forms.Label();
            this.txtTcNo = new System.Windows.Forms.TextBox();
            this.lblTcNo = new System.Windows.Forms.Label();
            this.txtAdSoyad = new System.Windows.Forms.TextBox();
            this.lblAdSoyad = new System.Windows.Forms.Label();
            this.grpOdemeler = new System.Windows.Forms.GroupBox();
            this.dgvOdemeler = new System.Windows.Forms.DataGridView();
            this.colTarih = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colToplam = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOdenen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colKalan = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMakbuzNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnKapat = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.btnOdemeYap = new System.Windows.Forms.Button();
            this.btnMakbuz = new System.Windows.Forms.Button();
            this.btnMakbuzGor = new System.Windows.Forms.Button();
            this.btnMakbuzSil = new System.Windows.Forms.Button();
            this.btnMakbuzDuzelt = new System.Windows.Forms.Button();
            this.btnSilSatir = new System.Windows.Forms.Button();
            this.btnYeniSatir = new System.Windows.Forms.Button();
            this.grpKursiyer.SuspendLayout();
            this.grpOdemeler.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOdemeler)).BeginInit();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblBaslik
            // 
            this.lblBaslik.AutoSize = true;
            this.lblBaslik.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblBaslik.Location = new System.Drawing.Point(12, 9);
            this.lblBaslik.Name = "lblBaslik";
            this.lblBaslik.Size = new System.Drawing.Size(176, 21);
            this.lblBaslik.TabIndex = 0;
            this.lblBaslik.Text = "Kursiyer Odeme Karti";
            // 
            // grpKursiyer
            // 
            this.grpKursiyer.Controls.Add(this.txtKalanBorc);
            this.grpKursiyer.Controls.Add(this.lblKalanBorc);
            this.grpKursiyer.Controls.Add(this.txtToplamOdenen);
            this.grpKursiyer.Controls.Add(this.lblToplamOdenen);
            this.grpKursiyer.Controls.Add(this.txtToplamBorc);
            this.grpKursiyer.Controls.Add(this.lblToplamBorc);
            this.grpKursiyer.Controls.Add(this.txtAciklama);
            this.grpKursiyer.Controls.Add(this.lblAciklama);
            this.grpKursiyer.Controls.Add(this.txtTcNo);
            this.grpKursiyer.Controls.Add(this.lblTcNo);
            this.grpKursiyer.Controls.Add(this.txtAdSoyad);
            this.grpKursiyer.Controls.Add(this.lblAdSoyad);
            this.grpKursiyer.Location = new System.Drawing.Point(12, 42);
            this.grpKursiyer.Name = "grpKursiyer";
            this.grpKursiyer.Size = new System.Drawing.Size(860, 115);
            this.grpKursiyer.TabIndex = 1;
            this.grpKursiyer.TabStop = false;
            this.grpKursiyer.Text = "Kursiyer Bilgisi";
            // 
            // txtKalanBorc
            // 
            this.txtKalanBorc.Location = new System.Drawing.Point(722, 22);
            this.txtKalanBorc.Name = "txtKalanBorc";
            this.txtKalanBorc.Size = new System.Drawing.Size(121, 20);
            this.txtKalanBorc.TabIndex = 11;
            // 
            // lblKalanBorc
            // 
            this.lblKalanBorc.AutoSize = true;
            this.lblKalanBorc.Location = new System.Drawing.Point(656, 25);
            this.lblKalanBorc.Name = "lblKalanBorc";
            this.lblKalanBorc.Size = new System.Drawing.Size(61, 13);
            this.lblKalanBorc.TabIndex = 10;
            this.lblKalanBorc.Text = "Kalan Borc:";
            // 
            // txtToplamOdenen
            // 
            this.txtToplamOdenen.Location = new System.Drawing.Point(486, 22);
            this.txtToplamOdenen.Name = "txtToplamOdenen";
            this.txtToplamOdenen.Size = new System.Drawing.Size(164, 20);
            this.txtToplamOdenen.TabIndex = 9;
            // 
            // lblToplamOdenen
            // 
            this.lblToplamOdenen.AutoSize = true;
            this.lblToplamOdenen.Location = new System.Drawing.Point(394, 25);
            this.lblToplamOdenen.Name = "lblToplamOdenen";
            this.lblToplamOdenen.Size = new System.Drawing.Size(86, 13);
            this.lblToplamOdenen.TabIndex = 8;
            this.lblToplamOdenen.Text = "Toplam Odenen:";
            // 
            // txtToplamBorc
            // 
            this.txtToplamBorc.Location = new System.Drawing.Point(292, 22);
            this.txtToplamBorc.Name = "txtToplamBorc";
            this.txtToplamBorc.Size = new System.Drawing.Size(96, 20);
            this.txtToplamBorc.TabIndex = 7;
            // 
            // lblToplamBorc
            // 
            this.lblToplamBorc.AutoSize = true;
            this.lblToplamBorc.Location = new System.Drawing.Point(219, 25);
            this.lblToplamBorc.Name = "lblToplamBorc";
            this.lblToplamBorc.Size = new System.Drawing.Size(67, 13);
            this.lblToplamBorc.TabIndex = 6;
            this.lblToplamBorc.Text = "Toplam Borc:";
            // 
            // txtAciklama
            // 
            this.txtAciklama.Location = new System.Drawing.Point(591, 49);
            this.txtAciklama.Name = "txtAciklama";
            this.txtAciklama.Size = new System.Drawing.Size(252, 20);
            this.txtAciklama.TabIndex = 5;
            // 
            // lblAciklama
            // 
            this.lblAciklama.AutoSize = true;
            this.lblAciklama.Location = new System.Drawing.Point(529, 52);
            this.lblAciklama.Name = "lblAciklama";
            this.lblAciklama.Size = new System.Drawing.Size(56, 13);
            this.lblAciklama.TabIndex = 4;
            this.lblAciklama.Text = "Aciklama:";
            // 
            // txtTcNo
            // 
            this.txtTcNo.Location = new System.Drawing.Point(85, 49);
            this.txtTcNo.MaxLength = 11;
            this.txtTcNo.Name = "txtTcNo";
            this.txtTcNo.Size = new System.Drawing.Size(181, 20);
            this.txtTcNo.TabIndex = 3;
            // 
            // lblTcNo
            // 
            this.lblTcNo.AutoSize = true;
            this.lblTcNo.Location = new System.Drawing.Point(19, 52);
            this.lblTcNo.Name = "lblTcNo";
            this.lblTcNo.Size = new System.Drawing.Size(60, 13);
            this.lblTcNo.TabIndex = 2;
            this.lblTcNo.Text = "TC Kimlik:";
            // 
            // txtAdSoyad
            // 
            this.txtAdSoyad.Location = new System.Drawing.Point(85, 22);
            this.txtAdSoyad.Name = "txtAdSoyad";
            this.txtAdSoyad.Size = new System.Drawing.Size(450, 20);
            this.txtAdSoyad.TabIndex = 1;
            // 
            // lblAdSoyad
            // 
            this.lblAdSoyad.AutoSize = true;
            this.lblAdSoyad.Location = new System.Drawing.Point(24, 25);
            this.lblAdSoyad.Name = "lblAdSoyad";
            this.lblAdSoyad.Size = new System.Drawing.Size(55, 13);
            this.lblAdSoyad.TabIndex = 0;
            this.lblAdSoyad.Text = "Ad Soyad:";
            // 
            // grpOdemeler
            // 
            this.grpOdemeler.Controls.Add(this.dgvOdemeler);
            this.grpOdemeler.Location = new System.Drawing.Point(12, 163);
            this.grpOdemeler.Name = "grpOdemeler";
            this.grpOdemeler.Size = new System.Drawing.Size(860, 335);
            this.grpOdemeler.TabIndex = 2;
            this.grpOdemeler.TabStop = false;
            this.grpOdemeler.Text = "Odeme Hareketleri";
            // 
            // dgvOdemeler
            // 
            this.dgvOdemeler.AllowUserToAddRows = false;
            this.dgvOdemeler.AllowUserToDeleteRows = false;
            this.dgvOdemeler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOdemeler.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTarih,
            this.colToplam,
            this.colOdenen,
            this.colKalan,
            this.colNot,
            this.colMakbuzNo});
            this.dgvOdemeler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvOdemeler.Location = new System.Drawing.Point(3, 16);
            this.dgvOdemeler.Name = "dgvOdemeler";
            this.dgvOdemeler.RowHeadersVisible = false;
            this.dgvOdemeler.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOdemeler.Size = new System.Drawing.Size(854, 316);
            this.dgvOdemeler.TabIndex = 0;
            // 
            // colTarih
            // 
            this.colTarih.HeaderText = "Tarih";
            this.colTarih.Name = "colTarih";
            this.colTarih.Width = 120;
            // 
            // colToplam
            // 
            this.colToplam.HeaderText = "Toplam";
            this.colToplam.Name = "colToplam";
            this.colToplam.Width = 120;
            // 
            // colOdenen
            // 
            this.colOdenen.HeaderText = "Odenen";
            this.colOdenen.Name = "colOdenen";
            this.colOdenen.Width = 120;
            // 
            // colKalan
            // 
            this.colKalan.HeaderText = "Kalan";
            this.colKalan.Name = "colKalan";
            this.colKalan.Width = 120;
            // 
            // colNot
            // 
            this.colNot.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colNot.HeaderText = "Not";
            this.colNot.Name = "colNot";
            // 
            // colMakbuzNo
            // 
            this.colMakbuzNo.HeaderText = "MakbuzNo";
            this.colMakbuzNo.Name = "colMakbuzNo";
            this.colMakbuzNo.Visible = false;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnKapat);
            this.pnlButtons.Controls.Add(this.btnKaydet);
            this.pnlButtons.Controls.Add(this.btnMakbuzDuzelt);
            this.pnlButtons.Controls.Add(this.btnMakbuzSil);
            this.pnlButtons.Controls.Add(this.btnMakbuzGor);
            this.pnlButtons.Controls.Add(this.btnMakbuz);
            this.pnlButtons.Controls.Add(this.btnOdemeYap);
            this.pnlButtons.Controls.Add(this.btnSilSatir);
            this.pnlButtons.Controls.Add(this.btnYeniSatir);
            this.pnlButtons.Location = new System.Drawing.Point(12, 500);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(860, 46);
            this.pnlButtons.TabIndex = 3;
            // 
            // btnKapat
            // 
            this.btnKapat.Location = new System.Drawing.Point(760, 10);
            this.btnKapat.Name = "btnKapat";
            this.btnKapat.Size = new System.Drawing.Size(83, 28);
            this.btnKapat.TabIndex = 3;
            this.btnKapat.Text = "Kapat";
            this.btnKapat.UseVisualStyleBackColor = true;
            // 
            // btnKaydet
            // 
            this.btnKaydet.Location = new System.Drawing.Point(671, 10);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(83, 28);
            this.btnKaydet.TabIndex = 2;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            // 
            // btnOdemeYap
            // 
            this.btnOdemeYap.Location = new System.Drawing.Point(181, 10);
            this.btnOdemeYap.Name = "btnOdemeYap";
            this.btnOdemeYap.Size = new System.Drawing.Size(96, 28);
            this.btnOdemeYap.TabIndex = 2;
            this.btnOdemeYap.Text = "Odeme Yap";
            this.btnOdemeYap.UseVisualStyleBackColor = true;
            // 
            // btnMakbuz
            // 
            this.btnMakbuz.Location = new System.Drawing.Point(283, 10);
            this.btnMakbuz.Name = "btnMakbuz";
            this.btnMakbuz.Size = new System.Drawing.Size(83, 28);
            this.btnMakbuz.TabIndex = 3;
            this.btnMakbuz.Text = "Makbuz";
            this.btnMakbuz.UseVisualStyleBackColor = true;
            // 
            // btnMakbuzGor
            // 
            this.btnMakbuzGor.Location = new System.Drawing.Point(372, 10);
            this.btnMakbuzGor.Name = "btnMakbuzGor";
            this.btnMakbuzGor.Size = new System.Drawing.Size(95, 28);
            this.btnMakbuzGor.TabIndex = 4;
            this.btnMakbuzGor.Text = "Makbuzu Gor";
            this.btnMakbuzGor.UseVisualStyleBackColor = true;
            // 
            // btnMakbuzSil
            // 
            this.btnMakbuzSil.Location = new System.Drawing.Point(473, 10);
            this.btnMakbuzSil.Name = "btnMakbuzSil";
            this.btnMakbuzSil.Size = new System.Drawing.Size(90, 28);
            this.btnMakbuzSil.TabIndex = 5;
            this.btnMakbuzSil.Text = "Makbuzu Sil";
            this.btnMakbuzSil.UseVisualStyleBackColor = true;
            // 
            // btnMakbuzDuzelt
            // 
            this.btnMakbuzDuzelt.Location = new System.Drawing.Point(569, 10);
            this.btnMakbuzDuzelt.Name = "btnMakbuzDuzelt";
            this.btnMakbuzDuzelt.Size = new System.Drawing.Size(96, 28);
            this.btnMakbuzDuzelt.TabIndex = 6;
            this.btnMakbuzDuzelt.Text = "Makbuzu Duzelt";
            this.btnMakbuzDuzelt.UseVisualStyleBackColor = true;
            // 
            // btnSilSatir
            // 
            this.btnSilSatir.Location = new System.Drawing.Point(92, 10);
            this.btnSilSatir.Name = "btnSilSatir";
            this.btnSilSatir.Size = new System.Drawing.Size(83, 28);
            this.btnSilSatir.TabIndex = 7;
            this.btnSilSatir.Text = "Satir Sil";
            this.btnSilSatir.UseVisualStyleBackColor = true;
            // 
            // btnYeniSatir
            // 
            this.btnYeniSatir.Location = new System.Drawing.Point(3, 10);
            this.btnYeniSatir.Name = "btnYeniSatir";
            this.btnYeniSatir.Size = new System.Drawing.Size(83, 28);
            this.btnYeniSatir.TabIndex = 8;
            this.btnYeniSatir.Text = "Yeni Satir";
            this.btnYeniSatir.UseVisualStyleBackColor = true;
            // 
            // Kursiyer_Odeme_Karti
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 546);
            this.Controls.Add(this.pnlButtons);
            this.Controls.Add(this.grpOdemeler);
            this.Controls.Add(this.grpKursiyer);
            this.Controls.Add(this.lblBaslik);
            this.Name = "Kursiyer_Odeme_Karti";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Kursiyer_Odeme_Karti";
            this.grpKursiyer.ResumeLayout(false);
            this.grpKursiyer.PerformLayout();
            this.grpOdemeler.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOdemeler)).EndInit();
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblBaslik;
        private System.Windows.Forms.GroupBox grpKursiyer;
        private System.Windows.Forms.TextBox txtAdSoyad;
        private System.Windows.Forms.Label lblAdSoyad;
        private System.Windows.Forms.TextBox txtAciklama;
        private System.Windows.Forms.Label lblAciklama;
        private System.Windows.Forms.TextBox txtTcNo;
        private System.Windows.Forms.Label lblTcNo;
        private System.Windows.Forms.GroupBox grpOdemeler;
        private System.Windows.Forms.DataGridView dgvOdemeler;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnKapat;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnOdemeYap;
        private System.Windows.Forms.Button btnMakbuz;
        private System.Windows.Forms.Button btnMakbuzGor;
        private System.Windows.Forms.Button btnMakbuzSil;
        private System.Windows.Forms.Button btnMakbuzDuzelt;
        private System.Windows.Forms.Button btnSilSatir;
        private System.Windows.Forms.Button btnYeniSatir;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTarih;
        private System.Windows.Forms.DataGridViewTextBoxColumn colToplam;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOdenen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colKalan;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNot;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMakbuzNo;
        private System.Windows.Forms.TextBox txtKalanBorc;
        private System.Windows.Forms.Label lblKalanBorc;
        private System.Windows.Forms.TextBox txtToplamOdenen;
        private System.Windows.Forms.Label lblToplamOdenen;
        private System.Windows.Forms.TextBox txtToplamBorc;
        private System.Windows.Forms.Label lblToplamBorc;
    }
}
