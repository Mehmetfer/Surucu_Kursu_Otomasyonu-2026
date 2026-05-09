namespace Kolera_Mtsk.Sayfalar
{
    partial class YedekTanimlariForm
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
            this.topPanel = new System.Windows.Forms.Panel();
            this.btnKapat = new System.Windows.Forms.Button();
            this.btnHemenYedekAl = new System.Windows.Forms.Button();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.grpKonum = new System.Windows.Forms.GroupBox();
            this.locationLayout = new System.Windows.Forms.TableLayoutPanel();
            this.numSonYedekSayisi = new System.Windows.Forms.NumericUpDown();
            this.btnGozat = new System.Windows.Forms.Button();
            this.txtYedekKonumu = new System.Windows.Forms.TextBox();
            this.lblYedekKonumu = new System.Windows.Forms.Label();
            this.lblSonYedek = new System.Windows.Forms.Label();
            this.topPanel.SuspendLayout();
            this.grpKonum.SuspendLayout();
            this.locationLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSonYedekSayisi)).BeginInit();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.BackColor = System.Drawing.Color.Gainsboro;
            this.topPanel.Controls.Add(this.btnKapat);
            this.topPanel.Controls.Add(this.btnHemenYedekAl);
            this.topPanel.Controls.Add(this.btnKaydet);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Size = new System.Drawing.Size(900, 64);
            this.topPanel.TabIndex = 0;
            // 
            // btnKapat
            // 
            this.btnKapat.BackColor = System.Drawing.Color.White;
            this.btnKapat.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKapat.ForeColor = System.Drawing.Color.Black;
            this.btnKapat.Location = new System.Drawing.Point(332, 16);
            this.btnKapat.Name = "btnKapat";
            this.btnKapat.Size = new System.Drawing.Size(130, 32);
            this.btnKapat.TabIndex = 2;
            this.btnKapat.Text = "KAPAT";
            this.btnKapat.UseVisualStyleBackColor = false;
            this.btnKapat.Click += new System.EventHandler(this.btnKapat_Click);
            // 
            // btnHemenYedekAl
            // 
            this.btnHemenYedekAl.BackColor = System.Drawing.Color.White;
            this.btnHemenYedekAl.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHemenYedekAl.ForeColor = System.Drawing.Color.Black;
            this.btnHemenYedekAl.Location = new System.Drawing.Point(150, 16);
            this.btnHemenYedekAl.Name = "btnHemenYedekAl";
            this.btnHemenYedekAl.Size = new System.Drawing.Size(160, 32);
            this.btnHemenYedekAl.TabIndex = 1;
            this.btnHemenYedekAl.Text = "HEMEN YEDEK AL";
            this.btnHemenYedekAl.UseVisualStyleBackColor = false;
            this.btnHemenYedekAl.Click += new System.EventHandler(this.btnHemenYedekAl_Click);
            // 
            // btnKaydet
            // 
            this.btnKaydet.BackColor = System.Drawing.Color.White;
            this.btnKaydet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnKaydet.ForeColor = System.Drawing.Color.Black;
            this.btnKaydet.Location = new System.Drawing.Point(20, 16);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(110, 32);
            this.btnKaydet.TabIndex = 0;
            this.btnKaydet.Text = "KAYDET";
            this.btnKaydet.UseVisualStyleBackColor = false;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            // 
            // grpKonum
            // 
            this.grpKonum.Controls.Add(this.locationLayout);
            this.grpKonum.ForeColor = System.Drawing.Color.Gold;
            this.grpKonum.Location = new System.Drawing.Point(14, 77);
            this.grpKonum.Name = "grpKonum";
            this.grpKonum.Size = new System.Drawing.Size(872, 240);
            this.grpKonum.TabIndex = 1;
            this.grpKonum.TabStop = false;
            this.grpKonum.Text = "YEDEK KONUMU";
            // 
            // locationLayout
            // 
            this.locationLayout.ColumnCount = 3;
            this.locationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 170F));
            this.locationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.locationLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.locationLayout.Controls.Add(this.numSonYedekSayisi, 1, 1);
            this.locationLayout.Controls.Add(this.btnGozat, 2, 0);
            this.locationLayout.Controls.Add(this.txtYedekKonumu, 1, 0);
            this.locationLayout.Controls.Add(this.lblYedekKonumu, 0, 0);
            this.locationLayout.Controls.Add(this.lblSonYedek, 0, 1);
            this.locationLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.locationLayout.Location = new System.Drawing.Point(3, 16);
            this.locationLayout.Name = "locationLayout";
            this.locationLayout.Padding = new System.Windows.Forms.Padding(10);
            this.locationLayout.RowCount = 2;
            this.locationLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.locationLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.locationLayout.Size = new System.Drawing.Size(866, 180);
            this.locationLayout.TabIndex = 0;
            // 
            // numSonYedekSayisi
            // 
            this.numSonYedekSayisi.Dock = System.Windows.Forms.DockStyle.Left;
            this.numSonYedekSayisi.Location = new System.Drawing.Point(183, 63);
            this.numSonYedekSayisi.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numSonYedekSayisi.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numSonYedekSayisi.Name = "numSonYedekSayisi";
            this.numSonYedekSayisi.Size = new System.Drawing.Size(120, 20);
            this.numSonYedekSayisi.TabIndex = 4;
            this.numSonYedekSayisi.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // btnGozat
            // 
            this.btnGozat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnGozat.ForeColor = System.Drawing.Color.Black;
            this.btnGozat.Location = new System.Drawing.Point(759, 13);
            this.btnGozat.Name = "btnGozat";
            this.btnGozat.Size = new System.Drawing.Size(94, 44);
            this.btnGozat.TabIndex = 2;
            this.btnGozat.Text = "Gozat";
            this.btnGozat.UseVisualStyleBackColor = true;
            this.btnGozat.Click += new System.EventHandler(this.btnGozat_Click);
            // 
            // txtYedekKonumu
            // 
            this.txtYedekKonumu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtYedekKonumu.Location = new System.Drawing.Point(183, 13);
            this.txtYedekKonumu.Name = "txtYedekKonumu";
            this.txtYedekKonumu.Size = new System.Drawing.Size(570, 20);
            this.txtYedekKonumu.TabIndex = 1;
            // 
            // lblYedekKonumu
            // 
            this.lblYedekKonumu.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblYedekKonumu.ForeColor = System.Drawing.Color.White;
            this.lblYedekKonumu.Location = new System.Drawing.Point(13, 10);
            this.lblYedekKonumu.Name = "lblYedekKonumu";
            this.lblYedekKonumu.Size = new System.Drawing.Size(164, 50);
            this.lblYedekKonumu.TabIndex = 0;
            this.lblYedekKonumu.Text = "Yedek Konumu";
            this.lblYedekKonumu.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSonYedek
            // 
            this.lblSonYedek.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSonYedek.ForeColor = System.Drawing.Color.White;
            this.lblSonYedek.Location = new System.Drawing.Point(13, 60);
            this.lblSonYedek.Name = "lblSonYedek";
            this.lblSonYedek.Size = new System.Drawing.Size(164, 110);
            this.lblSonYedek.TabIndex = 3;
            this.lblSonYedek.Text = "Son Yedek Sayisi";
            this.lblSonYedek.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // YedekTanimlariForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(46)))), ((int)(((byte)(56)))));
            this.ClientSize = new System.Drawing.Size(900, 560);
            this.Controls.Add(this.grpKonum);
            this.Controls.Add(this.topPanel);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "YedekTanimlariForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "YEDEK TANIMLARI";
            this.topPanel.ResumeLayout(false);
            this.grpKonum.ResumeLayout(false);
            this.locationLayout.ResumeLayout(false);
            this.locationLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSonYedekSayisi)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel topPanel;
        private System.Windows.Forms.Button btnKapat;
        private System.Windows.Forms.Button btnHemenYedekAl;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.GroupBox grpKonum;
        private System.Windows.Forms.TableLayoutPanel locationLayout;
        private System.Windows.Forms.NumericUpDown numSonYedekSayisi;
        private System.Windows.Forms.Button btnGozat;
        private System.Windows.Forms.TextBox txtYedekKonumu;
        private System.Windows.Forms.Label lblYedekKonumu;
        private System.Windows.Forms.Label lblSonYedek;
    }
}
