namespace Kolera_Mtsk.Sayfalar
{
    partial class Db_olsturvekopyala
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
            this.Txt_datam1 = new System.Windows.Forms.TextBox();
            this.KSunucuAdresiBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.Grp_Olustur = new System.Windows.Forms.GroupBox();
            this.DataAdi = new System.Windows.Forms.TextBox();
            this.Lbl_Durumu = new System.Windows.Forms.Label();
            this.Btn_Veritabani_Olustur = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Yukleme_Durumu = new System.Windows.Forms.ProgressBar();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.Grp_Kopya = new System.Windows.Forms.GroupBox();
            this.Btn_Test20 = new System.Windows.Forms.Button();
            this.Btn_Test100 = new System.Windows.Forms.Button();
            this.lblKopyalananTablo = new System.Windows.Forms.Label();
            this.lblKopyaYuzde = new System.Windows.Forms.Label();
            this.Kopya_ilerle_durum = new System.Windows.Forms.ProgressBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Btn_Kopyala_Baslat = new System.Windows.Forms.Button();
            this.cmbHedefDb = new System.Windows.Forms.ComboBox();
            this.cmbKaynakDb = new System.Windows.Forms.ComboBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.Btn_Demo = new System.Windows.Forms.Button();
            this.Grp_Olustur.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.Grp_Kopya.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // Txt_datam1
            // 
            this.Txt_datam1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Txt_datam1.Location = new System.Drawing.Point(31, 79);
            this.Txt_datam1.Multiline = true;
            this.Txt_datam1.Name = "Txt_datam1";
            this.Txt_datam1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Txt_datam1.Size = new System.Drawing.Size(257, 47);
            this.Txt_datam1.TabIndex = 36;
            // 
            // KSunucuAdresiBox
            // 
            this.KSunucuAdresiBox.Location = new System.Drawing.Point(19, 38);
            this.KSunucuAdresiBox.Name = "KSunucuAdresiBox";
            this.KSunucuAdresiBox.Size = new System.Drawing.Size(148, 20);
            this.KSunucuAdresiBox.TabIndex = 35;
            this.KSunucuAdresiBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 13);
            this.label1.TabIndex = 37;
            this.label1.Text = " Bağlı Olduğun Veri Tabanı Bilgileri";
            // 
            // Grp_Olustur
            // 
            this.Grp_Olustur.BackColor = System.Drawing.Color.DarkGray;
            this.Grp_Olustur.Controls.Add(this.DataAdi);
            this.Grp_Olustur.Controls.Add(this.Lbl_Durumu);
            this.Grp_Olustur.Controls.Add(this.Btn_Veritabani_Olustur);
            this.Grp_Olustur.Controls.Add(this.label7);
            this.Grp_Olustur.Controls.Add(this.label4);
            this.Grp_Olustur.Controls.Add(this.Yukleme_Durumu);
            this.Grp_Olustur.Controls.Add(this.listBox1);
            this.Grp_Olustur.Location = new System.Drawing.Point(19, 13);
            this.Grp_Olustur.Name = "Grp_Olustur";
            this.Grp_Olustur.Size = new System.Drawing.Size(833, 517);
            this.Grp_Olustur.TabIndex = 38;
            this.Grp_Olustur.TabStop = false;
            this.Grp_Olustur.Text = "Boş Veritabanı Oluşturma";
            // 
            // DataAdi
            // 
            this.DataAdi.Location = new System.Drawing.Point(74, 61);
            this.DataAdi.Name = "DataAdi";
            this.DataAdi.Size = new System.Drawing.Size(148, 20);
            this.DataAdi.TabIndex = 32;
            // 
            // Lbl_Durumu
            // 
            this.Lbl_Durumu.AutoSize = true;
            this.Lbl_Durumu.BackColor = System.Drawing.Color.Transparent;
            this.Lbl_Durumu.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Lbl_Durumu.ForeColor = System.Drawing.Color.Red;
            this.Lbl_Durumu.Location = new System.Drawing.Point(167, 200);
            this.Lbl_Durumu.Name = "Lbl_Durumu";
            this.Lbl_Durumu.Size = new System.Drawing.Size(101, 16);
            this.Lbl_Durumu.TabIndex = 28;
            this.Lbl_Durumu.Text = "Oluşturulmadı";
            // 
            // Btn_Veritabani_Olustur
            // 
            this.Btn_Veritabani_Olustur.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.Btn_Veritabani_Olustur.Location = new System.Drawing.Point(37, 125);
            this.Btn_Veritabani_Olustur.Name = "Btn_Veritabani_Olustur";
            this.Btn_Veritabani_Olustur.Size = new System.Drawing.Size(267, 42);
            this.Btn_Veritabani_Olustur.TabIndex = 29;
            this.Btn_Veritabani_Olustur.Text = "Boş Veri Tabanı oluştur";
            this.Btn_Veritabani_Olustur.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.BackColor = System.Drawing.Color.Transparent;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label7.ForeColor = System.Drawing.SystemColors.Desktop;
            this.label7.Location = new System.Drawing.Point(-3, 33);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(323, 16);
            this.label7.TabIndex = 26;
            this.label7.Text = "Oluşturmak istediğiniz Veri tabanı Adını Giriniz";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.Color.Transparent;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label4.Location = new System.Drawing.Point(41, 200);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 16);
            this.label4.TabIndex = 27;
            this.label4.Text = "Bağlantı Durumu";
            // 
            // Yukleme_Durumu
            // 
            this.Yukleme_Durumu.Location = new System.Drawing.Point(29, 257);
            this.Yukleme_Durumu.Name = "Yukleme_Durumu";
            this.Yukleme_Durumu.Size = new System.Drawing.Size(291, 23);
            this.Yukleme_Durumu.TabIndex = 31;
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.Color.Black;
            this.listBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.listBox1.ForeColor = System.Drawing.Color.White;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(397, -41);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(474, 524);
            this.listBox1.TabIndex = 30;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Btn_Demo);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.KSunucuAdresiBox);
            this.panel1.Controls.Add(this.Txt_datam1);
            this.panel1.Location = new System.Drawing.Point(0, -2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(869, 143);
            this.panel1.TabIndex = 39;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.Grp_Olustur);
            this.panel2.Location = new System.Drawing.Point(12, 147);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(857, 492);
            this.panel2.TabIndex = 40;
            // 
            // Grp_Kopya
            // 
            this.Grp_Kopya.Controls.Add(this.Btn_Test20);
            this.Grp_Kopya.Controls.Add(this.Btn_Test100);
            this.Grp_Kopya.Controls.Add(this.lblKopyalananTablo);
            this.Grp_Kopya.Controls.Add(this.lblKopyaYuzde);
            this.Grp_Kopya.Controls.Add(this.Kopya_ilerle_durum);
            this.Grp_Kopya.Controls.Add(this.label5);
            this.Grp_Kopya.Controls.Add(this.label2);
            this.Grp_Kopya.Controls.Add(this.Btn_Kopyala_Baslat);
            this.Grp_Kopya.Controls.Add(this.cmbHedefDb);
            this.Grp_Kopya.Controls.Add(this.cmbKaynakDb);
            this.Grp_Kopya.Location = new System.Drawing.Point(12, 3);
            this.Grp_Kopya.Name = "Grp_Kopya";
            this.Grp_Kopya.Size = new System.Drawing.Size(857, 118);
            this.Grp_Kopya.TabIndex = 41;
            this.Grp_Kopya.TabStop = false;
            this.Grp_Kopya.Text = "Kopyalama";
            // 
            // Btn_Test20
            // 
            this.Btn_Test20.Location = new System.Drawing.Point(677, 88);
            this.Btn_Test20.Name = "Btn_Test20";
            this.Btn_Test20.Size = new System.Drawing.Size(163, 22);
            this.Btn_Test20.TabIndex = 24;
            this.Btn_Test20.Text = "TEST 20 (ID 1..N)";
            this.Btn_Test20.UseVisualStyleBackColor = true;
            // 
            // Btn_Test100
            // 
            this.Btn_Test100.Location = new System.Drawing.Point(677, 58);
            this.Btn_Test100.Name = "Btn_Test100";
            this.Btn_Test100.Size = new System.Drawing.Size(163, 22);
            this.Btn_Test100.TabIndex = 23;
            this.Btn_Test100.Text = "TEST 300";
            this.Btn_Test100.UseVisualStyleBackColor = true;
            // 
            // lblKopyalananTablo
            // 
            this.lblKopyalananTablo.AutoSize = true;
            this.lblKopyalananTablo.Location = new System.Drawing.Point(30, 68);
            this.lblKopyalananTablo.Name = "lblKopyalananTablo";
            this.lblKopyalananTablo.Size = new System.Drawing.Size(92, 13);
            this.lblKopyalananTablo.TabIndex = 22;
            this.lblKopyalananTablo.Text = "Kopyalanan: (yok)";
            // 
            // lblKopyaYuzde
            // 
            this.lblKopyaYuzde.AutoSize = true;
            this.lblKopyaYuzde.Location = new System.Drawing.Point(636, 68);
            this.lblKopyaYuzde.Name = "lblKopyaYuzde";
            this.lblKopyaYuzde.Size = new System.Drawing.Size(21, 13);
            this.lblKopyaYuzde.TabIndex = 21;
            this.lblKopyaYuzde.Text = "%0";
            // 
            // Kopya_ilerle_durum
            // 
            this.Kopya_ilerle_durum.Location = new System.Drawing.Point(392, 66);
            this.Kopya_ilerle_durum.Name = "Kopya_ilerle_durum";
            this.Kopya_ilerle_durum.Size = new System.Drawing.Size(238, 16);
            this.Kopya_ilerle_durum.TabIndex = 20;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.ForeColor = System.Drawing.Color.Red;
            this.label5.Location = new System.Drawing.Point(392, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(235, 25);
            this.label5.TabIndex = 19;
            this.label5.Text = "HEDEF VERİ TABANI";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.ForeColor = System.Drawing.Color.IndianRed;
            this.label2.Location = new System.Drawing.Point(64, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(251, 25);
            this.label2.TabIndex = 18;
            this.label2.Text = "KAYNAK VERİ TABANI";
            // 
            // Btn_Kopyala_Baslat
            // 
            this.Btn_Kopyala_Baslat.Location = new System.Drawing.Point(677, 30);
            this.Btn_Kopyala_Baslat.Name = "Btn_Kopyala_Baslat";
            this.Btn_Kopyala_Baslat.Size = new System.Drawing.Size(163, 32);
            this.Btn_Kopyala_Baslat.TabIndex = 2;
            this.Btn_Kopyala_Baslat.Text = "Kopyala";
            this.Btn_Kopyala_Baslat.UseVisualStyleBackColor = true;
            // 
            // cmbHedefDb
            // 
            this.cmbHedefDb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbHedefDb.FormattingEnabled = true;
            this.cmbHedefDb.Location = new System.Drawing.Point(392, 44);
            this.cmbHedefDb.Name = "cmbHedefDb";
            this.cmbHedefDb.Size = new System.Drawing.Size(262, 21);
            this.cmbHedefDb.TabIndex = 1;
            // 
            // cmbKaynakDb
            // 
            this.cmbKaynakDb.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKaynakDb.FormattingEnabled = true;
            this.cmbKaynakDb.Location = new System.Drawing.Point(30, 44);
            this.cmbKaynakDb.Name = "cmbKaynakDb";
            this.cmbKaynakDb.Size = new System.Drawing.Size(309, 21);
            this.cmbKaynakDb.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.Grp_Kopya);
            this.panel3.Location = new System.Drawing.Point(0, 645);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(869, 128);
            this.panel3.TabIndex = 42;
            // 
            // Btn_Demo
            // 
            this.Btn_Demo.Location = new System.Drawing.Point(732, 9);
            this.Btn_Demo.Name = "Btn_Demo";
            this.Btn_Demo.Size = new System.Drawing.Size(134, 131);
            this.Btn_Demo.TabIndex = 38;
            this.Btn_Demo.Text = "DEMO VERI TABANI OLUSTUR 20 KİŞİLİK";
            this.Btn_Demo.UseVisualStyleBackColor = true;
            // 
            // Db_olsturvekopyala
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(888, 776);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Db_olsturvekopyala";
            this.Text = "Db_olsturvekopyala";
            this.Grp_Olustur.ResumeLayout(false);
            this.Grp_Olustur.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.Grp_Kopya.ResumeLayout(false);
            this.Grp_Kopya.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox Txt_datam1;
        private System.Windows.Forms.TextBox KSunucuAdresiBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox Grp_Olustur;
        private System.Windows.Forms.TextBox DataAdi;
        private System.Windows.Forms.Label Lbl_Durumu;
        private System.Windows.Forms.Button Btn_Veritabani_Olustur;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ProgressBar Yukleme_Durumu;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox Grp_Kopya;
        private System.Windows.Forms.Button Btn_Test20;
        private System.Windows.Forms.Button Btn_Test100;
        private System.Windows.Forms.Label lblKopyalananTablo;
        private System.Windows.Forms.Label lblKopyaYuzde;
        private System.Windows.Forms.ProgressBar Kopya_ilerle_durum;
        private System.Windows.Forms.Button Btn_Kopyala_Baslat;
        private System.Windows.Forms.ComboBox cmbHedefDb;
        private System.Windows.Forms.ComboBox cmbKaynakDb;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Btn_Demo;
    }
}