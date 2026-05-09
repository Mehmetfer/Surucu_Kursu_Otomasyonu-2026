namespace Kolera_Mtsk.Sayfalar
{
    partial class KullaniciTanimlariForm
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
            this.splitKullaniciIcerik = new System.Windows.Forms.SplitContainer();
            this.grpKullaniciListesi = new System.Windows.Forms.GroupBox();
            this.dgvKullanicilar = new System.Windows.Forms.DataGridView();
            this.grpKullaniciYetkileri = new System.Windows.Forms.GroupBox();
            this.lblAdminUyari = new System.Windows.Forms.Label();
            this.btnSil = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.btnYeni = new System.Windows.Forms.Button();
            this.cmbYetki = new System.Windows.Forms.ComboBox();
            this.lblYetki = new System.Windows.Forms.Label();
            this.txtSifre = new System.Windows.Forms.TextBox();
            this.lblSifre = new System.Windows.Forms.Label();
            this.txtKullaniciAdi = new System.Windows.Forms.TextBox();
            this.lblKullaniciAdi = new System.Windows.Forms.Label();
            this.dgvYetkiler = new System.Windows.Forms.DataGridView();
            this.colOzellik = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colYetki = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.lblSecilenKullanici = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitKullaniciIcerik)).BeginInit();
            this.splitKullaniciIcerik.Panel1.SuspendLayout();
            this.splitKullaniciIcerik.Panel2.SuspendLayout();
            this.splitKullaniciIcerik.SuspendLayout();
            this.grpKullaniciListesi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvKullanicilar)).BeginInit();
            this.grpKullaniciYetkileri.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvYetkiler)).BeginInit();
            this.SuspendLayout();
            // 
            // splitKullaniciIcerik
            // 
            this.splitKullaniciIcerik.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitKullaniciIcerik.Location = new System.Drawing.Point(0, 0);
            this.splitKullaniciIcerik.Name = "splitKullaniciIcerik";
            // 
            // splitKullaniciIcerik.Panel1
            // 
            this.splitKullaniciIcerik.Panel1.Controls.Add(this.grpKullaniciListesi);
            // 
            // splitKullaniciIcerik.Panel2
            // 
            this.splitKullaniciIcerik.Panel2.Controls.Add(this.grpKullaniciYetkileri);
            this.splitKullaniciIcerik.Size = new System.Drawing.Size(950, 600);
            this.splitKullaniciIcerik.SplitterDistance = 620;
            this.splitKullaniciIcerik.TabIndex = 0;
            // 
            // grpKullaniciListesi
            // 
            this.grpKullaniciListesi.Controls.Add(this.dgvKullanicilar);
            this.grpKullaniciListesi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpKullaniciListesi.Location = new System.Drawing.Point(0, 0);
            this.grpKullaniciListesi.Name = "grpKullaniciListesi";
            this.grpKullaniciListesi.Padding = new System.Windows.Forms.Padding(8);
            this.grpKullaniciListesi.Size = new System.Drawing.Size(770, 600);
            this.grpKullaniciListesi.TabIndex = 0;
            this.grpKullaniciListesi.TabStop = false;
            this.grpKullaniciListesi.Text = "Kullanici Listesi";
            // 
            // dgvKullanicilar
            // 
            this.dgvKullanicilar.AllowUserToAddRows = false;
            this.dgvKullanicilar.AllowUserToDeleteRows = false;
            this.dgvKullanicilar.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvKullanicilar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvKullanicilar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvKullanicilar.Location = new System.Drawing.Point(8, 21);
            this.dgvKullanicilar.MultiSelect = false;
            this.dgvKullanicilar.Name = "dgvKullanicilar";
            this.dgvKullanicilar.ReadOnly = true;
            this.dgvKullanicilar.RowHeadersVisible = false;
            this.dgvKullanicilar.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvKullanicilar.Size = new System.Drawing.Size(754, 571);
            this.dgvKullanicilar.TabIndex = 0;
            this.dgvKullanicilar.SelectionChanged += new System.EventHandler(this.dgvKullanicilar_SelectionChanged);
            // 
            // grpKullaniciYetkileri
            // 
            this.grpKullaniciYetkileri.Controls.Add(this.lblAdminUyari);
            this.grpKullaniciYetkileri.Controls.Add(this.btnSil);
            this.grpKullaniciYetkileri.Controls.Add(this.btnKaydet);
            this.grpKullaniciYetkileri.Controls.Add(this.btnYeni);
            this.grpKullaniciYetkileri.Controls.Add(this.cmbYetki);
            this.grpKullaniciYetkileri.Controls.Add(this.lblYetki);
            this.grpKullaniciYetkileri.Controls.Add(this.txtSifre);
            this.grpKullaniciYetkileri.Controls.Add(this.lblSifre);
            this.grpKullaniciYetkileri.Controls.Add(this.txtKullaniciAdi);
            this.grpKullaniciYetkileri.Controls.Add(this.lblKullaniciAdi);
            this.grpKullaniciYetkileri.Controls.Add(this.dgvYetkiler);
            this.grpKullaniciYetkileri.Controls.Add(this.lblSecilenKullanici);
            this.grpKullaniciYetkileri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpKullaniciYetkileri.Location = new System.Drawing.Point(0, 0);
            this.grpKullaniciYetkileri.Name = "grpKullaniciYetkileri";
            this.grpKullaniciYetkileri.Padding = new System.Windows.Forms.Padding(8);
            this.grpKullaniciYetkileri.Size = new System.Drawing.Size(326, 600);
            this.grpKullaniciYetkileri.TabIndex = 0;
            this.grpKullaniciYetkileri.TabStop = false;
            this.grpKullaniciYetkileri.Text = "Yetki - MTSK";
            // 
            // lblAdminUyari
            // 
            this.lblAdminUyari.ForeColor = System.Drawing.Color.DarkRed;
            this.lblAdminUyari.Location = new System.Drawing.Point(12, 173);
            this.lblAdminUyari.Name = "lblAdminUyari";
            this.lblAdminUyari.Size = new System.Drawing.Size(302, 32);
            this.lblAdminUyari.TabIndex = 11;
            this.lblAdminUyari.Text = "Sadece ADMIN kullanicilar ekleme, guncelleme ve silme yapabilir.";
            // 
            // btnSil
            // 
            this.btnSil.Location = new System.Drawing.Point(224, 134);
            this.btnSil.Name = "btnSil";
            this.btnSil.Size = new System.Drawing.Size(90, 30);
            this.btnSil.TabIndex = 10;
            this.btnSil.Text = "Sil";
            this.btnSil.UseVisualStyleBackColor = true;
            this.btnSil.Click += new System.EventHandler(this.btnSil_Click);
            // 
            // btnKaydet
            // 
            this.btnKaydet.Location = new System.Drawing.Point(128, 134);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(90, 30);
            this.btnKaydet.TabIndex = 9;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            // 
            // btnYeni
            // 
            this.btnYeni.Location = new System.Drawing.Point(32, 134);
            this.btnYeni.Name = "btnYeni";
            this.btnYeni.Size = new System.Drawing.Size(90, 30);
            this.btnYeni.TabIndex = 8;
            this.btnYeni.Text = "Yeni";
            this.btnYeni.UseVisualStyleBackColor = true;
            this.btnYeni.Click += new System.EventHandler(this.btnYeni_Click);
            // 
            // cmbYetki
            // 
            this.cmbYetki.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbYetki.FormattingEnabled = true;
            this.cmbYetki.Items.AddRange(new object[] {
            "ADMIN",
            "KULLANICI"});
            this.cmbYetki.Location = new System.Drawing.Point(92, 105);
            this.cmbYetki.Name = "cmbYetki";
            this.cmbYetki.Size = new System.Drawing.Size(222, 21);
            this.cmbYetki.TabIndex = 7;
            // 
            // lblYetki
            // 
            this.lblYetki.AutoSize = true;
            this.lblYetki.Location = new System.Drawing.Point(12, 108);
            this.lblYetki.Name = "lblYetki";
            this.lblYetki.Size = new System.Drawing.Size(34, 13);
            this.lblYetki.TabIndex = 6;
            this.lblYetki.Text = "Yetki:";
            // 
            // txtSifre
            // 
            this.txtSifre.Location = new System.Drawing.Point(92, 76);
            this.txtSifre.Name = "txtSifre";
            this.txtSifre.Size = new System.Drawing.Size(222, 20);
            this.txtSifre.TabIndex = 5;
            // 
            // lblSifre
            // 
            this.lblSifre.AutoSize = true;
            this.lblSifre.Location = new System.Drawing.Point(12, 79);
            this.lblSifre.Name = "lblSifre";
            this.lblSifre.Size = new System.Drawing.Size(31, 13);
            this.lblSifre.TabIndex = 4;
            this.lblSifre.Text = "Sifre:";
            // 
            // txtKullaniciAdi
            // 
            this.txtKullaniciAdi.Location = new System.Drawing.Point(92, 47);
            this.txtKullaniciAdi.Name = "txtKullaniciAdi";
            this.txtKullaniciAdi.Size = new System.Drawing.Size(222, 20);
            this.txtKullaniciAdi.TabIndex = 3;
            // 
            // lblKullaniciAdi
            // 
            this.lblKullaniciAdi.AutoSize = true;
            this.lblKullaniciAdi.Location = new System.Drawing.Point(12, 50);
            this.lblKullaniciAdi.Name = "lblKullaniciAdi";
            this.lblKullaniciAdi.Size = new System.Drawing.Size(67, 13);
            this.lblKullaniciAdi.TabIndex = 2;
            this.lblKullaniciAdi.Text = "Kullanici Adi:";
            // 
            // dgvYetkiler
            // 
            this.dgvYetkiler.AllowUserToAddRows = false;
            this.dgvYetkiler.AllowUserToDeleteRows = false;
            this.dgvYetkiler.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvYetkiler.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvYetkiler.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvYetkiler.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colOzellik,
            this.colYetki});
            this.dgvYetkiler.Location = new System.Drawing.Point(8, 215);
            this.dgvYetkiler.Name = "dgvYetkiler";
            this.dgvYetkiler.RowHeadersVisible = false;
            this.dgvYetkiler.Size = new System.Drawing.Size(310, 377);
            this.dgvYetkiler.TabIndex = 0;
            // 
            // colOzellik
            // 
            this.colOzellik.HeaderText = "OZELLIK";
            this.colOzellik.Name = "colOzellik";
            this.colOzellik.ReadOnly = true;
            // 
            // colYetki
            // 
            this.colYetki.FillWeight = 40F;
            this.colYetki.HeaderText = "YETKI";
            this.colYetki.Name = "colYetki";
            // 
            // lblSecilenKullanici
            // 
            this.lblSecilenKullanici.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSecilenKullanici.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblSecilenKullanici.Location = new System.Drawing.Point(8, 21);
            this.lblSecilenKullanici.Name = "lblSecilenKullanici";
            this.lblSecilenKullanici.Size = new System.Drawing.Size(310, 22);
            this.lblSecilenKullanici.TabIndex = 1;
            this.lblSecilenKullanici.Text = "Kullanici Yetkileri";
            this.lblSecilenKullanici.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KullaniciTanimlariForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 600);
            this.Controls.Add(this.splitKullaniciIcerik);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "KullaniciTanimlariForm";
            this.Text = "KullaniciTanimlariForm";
            this.splitKullaniciIcerik.Panel1.ResumeLayout(false);
            this.splitKullaniciIcerik.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitKullaniciIcerik)).EndInit();
            this.splitKullaniciIcerik.ResumeLayout(false);
            this.grpKullaniciListesi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvKullanicilar)).EndInit();
            this.grpKullaniciYetkileri.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvYetkiler)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.SplitContainer splitKullaniciIcerik;
        private System.Windows.Forms.GroupBox grpKullaniciListesi;
        private System.Windows.Forms.DataGridView dgvKullanicilar;
        private System.Windows.Forms.GroupBox grpKullaniciYetkileri;
        private System.Windows.Forms.DataGridView dgvYetkiler;
        private System.Windows.Forms.DataGridViewTextBoxColumn colOzellik;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colYetki;
        private System.Windows.Forms.Label lblSecilenKullanici;
        private System.Windows.Forms.Label lblAdminUyari;
        private System.Windows.Forms.Button btnSil;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnYeni;
        private System.Windows.Forms.ComboBox cmbYetki;
        private System.Windows.Forms.Label lblYetki;
        private System.Windows.Forms.TextBox txtSifre;
        private System.Windows.Forms.Label lblSifre;
        private System.Windows.Forms.TextBox txtKullaniciAdi;
        private System.Windows.Forms.Label lblKullaniciAdi;
    }
}
