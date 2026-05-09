namespace Kolera_Mtsk.Sayfalar
{
    partial class DbSecimForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DbSecimForm));
            this.RdbLocal = new System.Windows.Forms.RadioButton();
            this.RdbSql = new System.Windows.Forms.RadioButton();
            this.Btn_Devam = new System.Windows.Forms.Button();
            this.baglantiDurumu = new System.Windows.Forms.Label();
            this.Baglanti_Kur = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.Txt_baglanti1 = new System.Windows.Forms.TextBox();
            this.KParolaBox = new System.Windows.Forms.TextBox();
            this.KKullaniciAdiBox = new System.Windows.Forms.TextBox();
            this.KSunucuAdresiBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Lbl4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.Btn_Server_Ara = new System.Windows.Forms.Button();
            this.Cmb_Datalistele = new System.Windows.Forms.ComboBox();
            this.Btn_testConnection = new System.Windows.Forms.Button();
            this.Btn_save = new System.Windows.Forms.Button();
            this.Listele = new System.Windows.Forms.ListBox();
            this.Txt_datam1 = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // RdbLocal
            // 
            this.RdbLocal.AutoSize = true;
            this.RdbLocal.Location = new System.Drawing.Point(312, 76);
            this.RdbLocal.Name = "RdbLocal";
            this.RdbLocal.Size = new System.Drawing.Size(67, 20);
            this.RdbLocal.TabIndex = 0;
            this.RdbLocal.TabStop = true;
            this.RdbLocal.Text = "LOCAL";
            this.RdbLocal.UseVisualStyleBackColor = true;
            // 
            // RdbSql
            // 
            this.RdbSql.AutoSize = true;
            this.RdbSql.Location = new System.Drawing.Point(312, 37);
            this.RdbSql.Name = "RdbSql";
            this.RdbSql.Size = new System.Drawing.Size(51, 20);
            this.RdbSql.TabIndex = 0;
            this.RdbSql.TabStop = true;
            this.RdbSql.Text = "SQL";
            this.RdbSql.UseVisualStyleBackColor = true;
            // 
            // Btn_Devam
            // 
            this.Btn_Devam.Location = new System.Drawing.Point(65, 690);
            this.Btn_Devam.Name = "Btn_Devam";
            this.Btn_Devam.Size = new System.Drawing.Size(263, 23);
            this.Btn_Devam.TabIndex = 1;
            this.Btn_Devam.Text = "İlk Kurulum İçin Veri Tabanı Oluştur";
            this.Btn_Devam.UseVisualStyleBackColor = true;
            this.Btn_Devam.Click += new System.EventHandler(this.Btn_Devam_Click);
            // 
            // baglantiDurumu
            // 
            this.baglantiDurumu.AutoSize = true;
            this.baglantiDurumu.BackColor = System.Drawing.Color.Transparent;
            this.baglantiDurumu.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.baglantiDurumu.ForeColor = System.Drawing.Color.Red;
            this.baglantiDurumu.Location = new System.Drawing.Point(211, 634);
            this.baglantiDurumu.Name = "baglantiDurumu";
            this.baglantiDurumu.Size = new System.Drawing.Size(42, 16);
            this.baglantiDurumu.TabIndex = 9;
            this.baglantiDurumu.Text = "Pasif";
            // 
            // Baglanti_Kur
            // 
            this.Baglanti_Kur.ForeColor = System.Drawing.Color.Black;
            this.Baglanti_Kur.Location = new System.Drawing.Point(28, 165);
            this.Baglanti_Kur.Name = "Baglanti_Kur";
            this.Baglanti_Kur.Size = new System.Drawing.Size(247, 38);
            this.Baglanti_Kur.TabIndex = 3;
            this.Baglanti_Kur.Text = "Bağlantı Kur";
            this.Baglanti_Kur.UseVisualStyleBackColor = true;
            this.Baglanti_Kur.Click += new System.EventHandler(this.Baglanti_Kur_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.Transparent;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label5.Location = new System.Drawing.Point(47, 634);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 16);
            this.label5.TabIndex = 8;
            this.label5.Text = "Bağlantı Durumu";
            // 
            // Txt_baglanti1
            // 
            this.Txt_baglanti1.BackColor = System.Drawing.SystemColors.WindowText;
            this.Txt_baglanti1.ForeColor = System.Drawing.SystemColors.Window;
            this.Txt_baglanti1.Location = new System.Drawing.Point(127, 65);
            this.Txt_baglanti1.Name = "Txt_baglanti1";
            this.Txt_baglanti1.Size = new System.Drawing.Size(148, 22);
            this.Txt_baglanti1.TabIndex = 1;
            // 
            // KParolaBox
            // 
            this.KParolaBox.Location = new System.Drawing.Point(127, 128);
            this.KParolaBox.Name = "KParolaBox";
            this.KParolaBox.PasswordChar = '*';
            this.KParolaBox.Size = new System.Drawing.Size(148, 22);
            this.KParolaBox.TabIndex = 6;
            this.KParolaBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // KKullaniciAdiBox
            // 
            this.KKullaniciAdiBox.Location = new System.Drawing.Point(127, 91);
            this.KKullaniciAdiBox.Name = "KKullaniciAdiBox";
            this.KKullaniciAdiBox.Size = new System.Drawing.Size(148, 22);
            this.KKullaniciAdiBox.TabIndex = 5;
            this.KKullaniciAdiBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // KSunucuAdresiBox
            // 
            this.KSunucuAdresiBox.Location = new System.Drawing.Point(127, 32);
            this.KSunucuAdresiBox.Name = "KSunucuAdresiBox";
            this.KSunucuAdresiBox.Size = new System.Drawing.Size(148, 22);
            this.KSunucuAdresiBox.TabIndex = 4;
            this.KSunucuAdresiBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Parola ";
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.OrangeRed;
            this.groupBox1.Controls.Add(this.Baglanti_Kur);
            this.groupBox1.Controls.Add(this.Txt_baglanti1);
            this.groupBox1.Controls.Add(this.KParolaBox);
            this.groupBox1.Controls.Add(this.KKullaniciAdiBox);
            this.groupBox1.Controls.Add(this.KSunucuAdresiBox);
            this.groupBox1.Controls.Add(this.Btn_Server_Ara);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.Lbl4);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.RdbSql);
            this.groupBox1.Controls.Add(this.RdbLocal);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(535, 223);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Programda Çalışacak Veritabanı Bilgileri";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Kullanıcı Adı";
            // 
            // Lbl4
            // 
            this.Lbl4.AutoSize = true;
            this.Lbl4.Location = new System.Drawing.Point(25, 68);
            this.Lbl4.Name = "Lbl4";
            this.Lbl4.Size = new System.Drawing.Size(86, 16);
            this.Lbl4.TabIndex = 0;
            this.Lbl4.Text = "Bağlantı Türü";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Sunucu Adresi";
            // 
            // Btn_Server_Ara
            // 
            this.Btn_Server_Ara.Location = new System.Drawing.Point(432, 68);
            this.Btn_Server_Ara.Name = "Btn_Server_Ara";
            this.Btn_Server_Ara.Size = new System.Drawing.Size(97, 99);
            this.Btn_Server_Ara.TabIndex = 19;
            this.Btn_Server_Ara.Text = "Server Ara";
            this.Btn_Server_Ara.UseVisualStyleBackColor = true;
            // 
            // Cmb_Datalistele
            // 
            this.Cmb_Datalistele.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Cmb_Datalistele.FormattingEnabled = true;
            this.Cmb_Datalistele.Location = new System.Drawing.Point(21, 418);
            this.Cmb_Datalistele.Name = "Cmb_Datalistele";
            this.Cmb_Datalistele.Size = new System.Drawing.Size(520, 21);
            this.Cmb_Datalistele.TabIndex = 38;
            // 
            // Btn_testConnection
            // 
            this.Btn_testConnection.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_testConnection.Location = new System.Drawing.Point(31, 541);
            this.Btn_testConnection.Name = "Btn_testConnection";
            this.Btn_testConnection.Size = new System.Drawing.Size(99, 42);
            this.Btn_testConnection.TabIndex = 36;
            this.Btn_testConnection.Text = "Bağlantıyı Test Et";
            this.Btn_testConnection.UseVisualStyleBackColor = true;
            // 
            // Btn_save
            // 
            this.Btn_save.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("Btn_save.BackgroundImage")));
            this.Btn_save.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_save.Location = new System.Drawing.Point(402, 532);
            this.Btn_save.Name = "Btn_save";
            this.Btn_save.Size = new System.Drawing.Size(103, 42);
            this.Btn_save.TabIndex = 35;
            this.Btn_save.UseVisualStyleBackColor = true;
            // 
            // Listele
            // 
            this.Listele.BackColor = System.Drawing.Color.Black;
            this.Listele.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.Listele.ForeColor = System.Drawing.Color.White;
            this.Listele.FormattingEnabled = true;
            this.Listele.Location = new System.Drawing.Point(21, 260);
            this.Listele.Name = "Listele";
            this.Listele.Size = new System.Drawing.Size(520, 134);
            this.Listele.TabIndex = 37;
            // 
            // Txt_datam1
            // 
            this.Txt_datam1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Txt_datam1.Location = new System.Drawing.Point(21, 462);
            this.Txt_datam1.Multiline = true;
            this.Txt_datam1.Name = "Txt_datam1";
            this.Txt_datam1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Txt_datam1.Size = new System.Drawing.Size(520, 47);
            this.Txt_datam1.TabIndex = 34;
            // 
            // DbSecimForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(555, 770);
            this.Controls.Add(this.Cmb_Datalistele);
            this.Controls.Add(this.Btn_testConnection);
            this.Controls.Add(this.Btn_save);
            this.Controls.Add(this.Listele);
            this.Controls.Add(this.Txt_datam1);
            this.Controls.Add(this.baglantiDurumu);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.Btn_Devam);
            this.Controls.Add(this.label5);
            this.Name = "DbSecimForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DbSecimForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton RdbLocal;
        private System.Windows.Forms.RadioButton RdbSql;
        private System.Windows.Forms.Button Btn_Devam;
        private System.Windows.Forms.Label baglantiDurumu;
        private System.Windows.Forms.Button Baglanti_Kur;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox Txt_baglanti1;
        private System.Windows.Forms.TextBox KParolaBox;
        private System.Windows.Forms.TextBox KKullaniciAdiBox;
        private System.Windows.Forms.TextBox KSunucuAdresiBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label Lbl4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button Btn_Server_Ara;
        private System.Windows.Forms.ComboBox Cmb_Datalistele;
        private System.Windows.Forms.Button Btn_testConnection;
        private System.Windows.Forms.Button Btn_save;
        private System.Windows.Forms.ListBox Listele;
        private System.Windows.Forms.TextBox Txt_datam1;
    }
}