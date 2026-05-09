using Kolera.Evrak.Models;
using Kolera.Mebbis;
using Kolera_Mtsk.Mebbis;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace Kolera_Mtsk.Sayfalar
{
    partial class MebbisWebForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MebbisWebForm));
            this.panelBrowser = new System.Windows.Forms.Panel();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.Btn_OzelMTSK = new System.Windows.Forms.Button();
            this.Btn_HizliAktar = new System.Windows.Forms.Button();
            this.Btn_IslemiKes = new System.Windows.Forms.Button();
            this.Grp_2 = new System.Windows.Forms.GroupBox();
            this.Lbl_1 = new System.Windows.Forms.Label();
            this.Lbl_5 = new System.Windows.Forms.Label();
            this.Onceki_Ehliyet = new System.Windows.Forms.Label();
            this.Donem_Adi = new System.Windows.Forms.Label();
            this.Ehliyetsinif = new System.Windows.Forms.Label();
            this.KursiyersSoyadi = new System.Windows.Forms.Label();
            this.KursiyerAdi = new System.Windows.Forms.Label();
            this.Grp_1 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Btn_fatura = new System.Windows.Forms.Button();
            this.Btn8_Menu1 = new System.Windows.Forms.Button();
            this.Btn_adres = new System.Windows.Forms.Button();
            this.Btn7_Menu2 = new System.Windows.Forms.Button();
            this.Btn7_Menu1 = new System.Windows.Forms.Button();
            this.Btn_Sozlesme = new System.Windows.Forms.Button();
            this.Btn6_Menu1 = new System.Windows.Forms.Button();
            this.Btn_imzasi = new System.Windows.Forms.Button();
            this.Btn5_Menu2 = new System.Windows.Forms.Button();
            this.Btn5_Menu1 = new System.Windows.Forms.Button();
            this.Btn_SABIKA = new System.Windows.Forms.Button();
            this.Btn4_Menu2 = new System.Windows.Forms.Button();
            this.Btn4_Menu1 = new System.Windows.Forms.Button();
            this.Btn_Saglik = new System.Windows.Forms.Button();
            this.Btn3_Menu2 = new System.Windows.Forms.Button();
            this.Btn3_Menu1 = new System.Windows.Forms.Button();
            this.Btn_OgrnBilgileri = new System.Windows.Forms.Button();
            this.Btn2_Menu2 = new System.Windows.Forms.Button();
            this.Btn2_Menu1 = new System.Windows.Forms.Button();
            this.Btn_Resim = new System.Windows.Forms.Button();
            this.Btn_Menu2 = new System.Windows.Forms.Button();
            this.Btn_Menu1 = new System.Windows.Forms.Button();
            this.Tnk_RESIM_Kursiyer = new System.Windows.Forms.PictureBox();
            this.panelBrowser.SuspendLayout();
            this.Grp_2.SuspendLayout();
            this.Grp_1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Tnk_RESIM_Kursiyer)).BeginInit();
            this.SuspendLayout();
            // 
            // panelBrowser
            // 
            this.panelBrowser.Controls.Add(this.webBrowser1);
            this.panelBrowser.Location = new System.Drawing.Point(143, 145);
            this.panelBrowser.Name = "panelBrowser";
            this.panelBrowser.Size = new System.Drawing.Size(1295, 904);
            this.panelBrowser.TabIndex = 0;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Left;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(1292, 904);
            this.webBrowser1.TabIndex = 0;
            // 
            // Btn_OzelMTSK
            // 
            this.Btn_OzelMTSK.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_OzelMTSK.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_OzelMTSK.Location = new System.Drawing.Point(0, 68);
            this.Btn_OzelMTSK.Name = "Btn_OzelMTSK";
            this.Btn_OzelMTSK.Size = new System.Drawing.Size(125, 34);
            this.Btn_OzelMTSK.TabIndex = 1;
            this.Btn_OzelMTSK.Text = "ADAY KAYIT İŞLEMLERİ";
            this.Btn_OzelMTSK.UseVisualStyleBackColor = false;
            // 
            // Btn_HizliAktar
            // 
            this.Btn_HizliAktar.BackColor = System.Drawing.Color.ForestGreen;
            this.Btn_HizliAktar.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_HizliAktar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn_HizliAktar.ForeColor = System.Drawing.Color.White;
            this.Btn_HizliAktar.Location = new System.Drawing.Point(0, 34);
            this.Btn_HizliAktar.Name = "Btn_HizliAktar";
            this.Btn_HizliAktar.Size = new System.Drawing.Size(125, 34);
            this.Btn_HizliAktar.TabIndex = 63;
            this.Btn_HizliAktar.Text = "HIZLI AKTAR";
            this.Btn_HizliAktar.UseVisualStyleBackColor = false;
            // 
            // Btn_IslemiKes
            // 
            this.Btn_IslemiKes.BackColor = System.Drawing.Color.Firebrick;
            this.Btn_IslemiKes.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_IslemiKes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn_IslemiKes.ForeColor = System.Drawing.Color.White;
            this.Btn_IslemiKes.Location = new System.Drawing.Point(0, 0);
            this.Btn_IslemiKes.Name = "Btn_IslemiKes";
            this.Btn_IslemiKes.Size = new System.Drawing.Size(125, 34);
            this.Btn_IslemiKes.TabIndex = 64;
            this.Btn_IslemiKes.Text = "İŞLEMİ KES";
            this.Btn_IslemiKes.UseVisualStyleBackColor = false;
            // 
            // Grp_2
            // 
            this.Grp_2.Controls.Add(this.Lbl_1);
            this.Grp_2.Controls.Add(this.Lbl_5);
            this.Grp_2.Controls.Add(this.Onceki_Ehliyet);
            this.Grp_2.Controls.Add(this.Donem_Adi);
            this.Grp_2.Controls.Add(this.Ehliyetsinif);
            this.Grp_2.Controls.Add(this.KursiyersSoyadi);
            this.Grp_2.Controls.Add(this.KursiyerAdi);
            this.Grp_2.Location = new System.Drawing.Point(137, 1);
            this.Grp_2.Name = "Grp_2";
            this.Grp_2.Size = new System.Drawing.Size(761, 119);
            this.Grp_2.TabIndex = 16;
            this.Grp_2.TabStop = false;
            this.Grp_2.Text = "KURSİYER BİLGİLERİ";
            // 
            // Lbl_1
            // 
            this.Lbl_1.AutoSize = true;
            this.Lbl_1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Lbl_1.Location = new System.Drawing.Point(0, 74);
            this.Lbl_1.Name = "Lbl_1";
            this.Lbl_1.Size = new System.Drawing.Size(102, 16);
            this.Lbl_1.TabIndex = 58;
            this.Lbl_1.Text = "Sertifika Sınıfı";
            // 
            // Lbl_5
            // 
            this.Lbl_5.AutoSize = true;
            this.Lbl_5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Lbl_5.Location = new System.Drawing.Point(15, 19);
            this.Lbl_5.Name = "Lbl_5";
            this.Lbl_5.Size = new System.Drawing.Size(30, 16);
            this.Lbl_5.TabIndex = 57;
            this.Lbl_5.Text = "Adı";
            // 
            // Onceki_Ehliyet
            // 
            this.Onceki_Ehliyet.AutoSize = true;
            this.Onceki_Ehliyet.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Onceki_Ehliyet.Location = new System.Drawing.Point(53, 45);
            this.Onceki_Ehliyet.Name = "Onceki_Ehliyet";
            this.Onceki_Ehliyet.Size = new System.Drawing.Size(138, 13);
            this.Onceki_Ehliyet.TabIndex = 56;
            this.Onceki_Ehliyet.Text = "ÖNCEKİ EHLİYET YOK";
            // 
            // Donem_Adi
            // 
            this.Donem_Adi.AutoSize = true;
            this.Donem_Adi.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Donem_Adi.ForeColor = System.Drawing.Color.IndianRed;
            this.Donem_Adi.Location = new System.Drawing.Point(391, 45);
            this.Donem_Adi.Name = "Donem_Adi";
            this.Donem_Adi.Size = new System.Drawing.Size(132, 25);
            this.Donem_Adi.TabIndex = 56;
            this.Donem_Adi.Text = "Donem_Adi";
            // 
            // Ehliyetsinif
            // 
            this.Ehliyetsinif.AutoSize = true;
            this.Ehliyetsinif.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Ehliyetsinif.Location = new System.Drawing.Point(120, 74);
            this.Ehliyetsinif.Name = "Ehliyetsinif";
            this.Ehliyetsinif.Size = new System.Drawing.Size(0, 13);
            this.Ehliyetsinif.TabIndex = 56;
            // 
            // KursiyersSoyadi
            // 
            this.KursiyersSoyadi.AutoSize = true;
            this.KursiyersSoyadi.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.KursiyersSoyadi.Location = new System.Drawing.Point(81, 43);
            this.KursiyersSoyadi.Name = "KursiyersSoyadi";
            this.KursiyersSoyadi.Size = new System.Drawing.Size(0, 13);
            this.KursiyersSoyadi.TabIndex = 59;
            // 
            // KursiyerAdi
            // 
            this.KursiyerAdi.AutoSize = true;
            this.KursiyerAdi.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.KursiyerAdi.Location = new System.Drawing.Point(81, 16);
            this.KursiyerAdi.Name = "KursiyerAdi";
            this.KursiyerAdi.Size = new System.Drawing.Size(0, 16);
            this.KursiyerAdi.TabIndex = 51;
            // 
            // Grp_1
            // 
            this.Grp_1.Controls.Add(this.Tnk_RESIM_Kursiyer);
            this.Grp_1.Location = new System.Drawing.Point(27, 1);
            this.Grp_1.Name = "Grp_1";
            this.Grp_1.Size = new System.Drawing.Size(104, 143);
            this.Grp_1.TabIndex = 17;
            this.Grp_1.TabStop = false;
            this.Grp_1.Text = "KURSİYER";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Btn_fatura);
            this.panel1.Controls.Add(this.Btn8_Menu1);
            this.panel1.Controls.Add(this.Btn_adres);
            this.panel1.Controls.Add(this.Btn7_Menu2);
            this.panel1.Controls.Add(this.Btn7_Menu1);
            this.panel1.Controls.Add(this.Btn_Sozlesme);
            this.panel1.Controls.Add(this.Btn6_Menu1);
            this.panel1.Controls.Add(this.Btn_imzasi);
            this.panel1.Controls.Add(this.Btn5_Menu2);
            this.panel1.Controls.Add(this.Btn5_Menu1);
            this.panel1.Controls.Add(this.Btn_SABIKA);
            this.panel1.Controls.Add(this.Btn4_Menu2);
            this.panel1.Controls.Add(this.Btn4_Menu1);
            this.panel1.Controls.Add(this.Btn_Saglik);
            this.panel1.Controls.Add(this.Btn3_Menu2);
            this.panel1.Controls.Add(this.Btn3_Menu1);
            this.panel1.Controls.Add(this.Btn_OgrnBilgileri);
            this.panel1.Controls.Add(this.Btn2_Menu2);
            this.panel1.Controls.Add(this.Btn2_Menu1);
            this.panel1.Controls.Add(this.Btn_Resim);
            this.panel1.Controls.Add(this.Btn_Menu2);
            this.panel1.Controls.Add(this.Btn_Menu1);
            this.panel1.Controls.Add(this.Btn_OzelMTSK);
            this.panel1.Controls.Add(this.Btn_HizliAktar);
            this.panel1.Controls.Add(this.Btn_IslemiKes);
            this.panel1.Location = new System.Drawing.Point(12, 150);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(125, 626);
            this.panel1.TabIndex = 52;
            // 
            // Btn_fatura
            // 
            this.Btn_fatura.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_fatura.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_fatura.Location = new System.Drawing.Point(0, 653);
            this.Btn_fatura.Name = "Btn_fatura";
            this.Btn_fatura.Size = new System.Drawing.Size(125, 29);
            this.Btn_fatura.TabIndex = 62;
            this.Btn_fatura.Text = "FATURA BİLGİSİ";
            this.Btn_fatura.UseVisualStyleBackColor = false;
            // 
            // Btn8_Menu1
            // 
            this.Btn8_Menu1.BackColor = System.Drawing.Color.IndianRed;
            this.Btn8_Menu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn8_Menu1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn8_Menu1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Btn8_Menu1.Location = new System.Drawing.Point(0, 628);
            this.Btn8_Menu1.Name = "Btn8_Menu1";
            this.Btn8_Menu1.Size = new System.Drawing.Size(125, 25);
            this.Btn8_Menu1.TabIndex = 61;
            this.Btn8_Menu1.Text = "Ön Sayfa Gönder";
            this.Btn8_Menu1.UseVisualStyleBackColor = false;
            this.Btn8_Menu1.Visible = false;
            // 
            // Btn_adres
            // 
            this.Btn_adres.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_adres.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_adres.Location = new System.Drawing.Point(0, 599);
            this.Btn_adres.Name = "Btn_adres";
            this.Btn_adres.Size = new System.Drawing.Size(125, 29);
            this.Btn_adres.TabIndex = 60;
            this.Btn_adres.Text = "ADRES BİLGİSİ";
            this.Btn_adres.UseVisualStyleBackColor = false;
            // 
            // Btn7_Menu2
            // 
            this.Btn7_Menu2.BackColor = System.Drawing.Color.IndianRed;
            this.Btn7_Menu2.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn7_Menu2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn7_Menu2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Btn7_Menu2.Location = new System.Drawing.Point(0, 574);
            this.Btn7_Menu2.Name = "Btn7_Menu2";
            this.Btn7_Menu2.Size = new System.Drawing.Size(125, 25);
            this.Btn7_Menu2.TabIndex = 59;
            this.Btn7_Menu2.Text = "Arka Sayfa Gönder";
            this.Btn7_Menu2.UseVisualStyleBackColor = false;
            this.Btn7_Menu2.Visible = false;
            // 
            // Btn7_Menu1
            // 
            this.Btn7_Menu1.BackColor = System.Drawing.Color.IndianRed;
            this.Btn7_Menu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn7_Menu1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn7_Menu1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Btn7_Menu1.Location = new System.Drawing.Point(0, 549);
            this.Btn7_Menu1.Name = "Btn7_Menu1";
            this.Btn7_Menu1.Size = new System.Drawing.Size(125, 25);
            this.Btn7_Menu1.TabIndex = 58;
            this.Btn7_Menu1.Text = "Ön Sayfa Gönder";
            this.Btn7_Menu1.UseVisualStyleBackColor = false;
            this.Btn7_Menu1.Visible = false;
            // 
            // Btn_Sozlesme
            // 
            this.Btn_Sozlesme.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_Sozlesme.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_Sozlesme.Location = new System.Drawing.Point(0, 520);
            this.Btn_Sozlesme.Name = "Btn_Sozlesme";
            this.Btn_Sozlesme.Size = new System.Drawing.Size(125, 29);
            this.Btn_Sozlesme.TabIndex = 57;
            this.Btn_Sozlesme.Text = "SÖZLEŞME";
            this.Btn_Sozlesme.UseVisualStyleBackColor = false;
            // 
            // Btn6_Menu1
            // 
            this.Btn6_Menu1.BackColor = System.Drawing.Color.IndianRed;
            this.Btn6_Menu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn6_Menu1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn6_Menu1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Btn6_Menu1.Location = new System.Drawing.Point(0, 495);
            this.Btn6_Menu1.Name = "Btn6_Menu1";
            this.Btn6_Menu1.Size = new System.Drawing.Size(125, 25);
            this.Btn6_Menu1.TabIndex = 56;
            this.Btn6_Menu1.Text = "Belge Gönder";
            this.Btn6_Menu1.UseVisualStyleBackColor = false;
            this.Btn6_Menu1.Visible = false;
            // 
            // Btn_imzasi
            // 
            this.Btn_imzasi.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_imzasi.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_imzasi.Location = new System.Drawing.Point(0, 466);
            this.Btn_imzasi.Name = "Btn_imzasi";
            this.Btn_imzasi.Size = new System.Drawing.Size(125, 29);
            this.Btn_imzasi.TabIndex = 55;
            this.Btn_imzasi.Text = "İMZA";
            this.Btn_imzasi.UseVisualStyleBackColor = false;
            // 
            // Btn5_Menu2
            // 
            this.Btn5_Menu2.BackColor = System.Drawing.Color.IndianRed;
            this.Btn5_Menu2.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn5_Menu2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn5_Menu2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Btn5_Menu2.Location = new System.Drawing.Point(0, 441);
            this.Btn5_Menu2.Name = "Btn5_Menu2";
            this.Btn5_Menu2.Size = new System.Drawing.Size(125, 25);
            this.Btn5_Menu2.TabIndex = 54;
            this.Btn5_Menu2.Text = "Belge Gönder";
            this.Btn5_Menu2.UseVisualStyleBackColor = false;
            this.Btn5_Menu2.Visible = false;
            // 
            // Btn5_Menu1
            // 
            this.Btn5_Menu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn5_Menu1.Location = new System.Drawing.Point(0, 418);
            this.Btn5_Menu1.Name = "Btn5_Menu1";
            this.Btn5_Menu1.Size = new System.Drawing.Size(125, 23);
            this.Btn5_Menu1.TabIndex = 53;
            this.Btn5_Menu1.Text = "Bilgileri Gönder";
            this.Btn5_Menu1.UseVisualStyleBackColor = true;
            this.Btn5_Menu1.Visible = false;
            // 
            // Btn_SABIKA
            // 
            this.Btn_SABIKA.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_SABIKA.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_SABIKA.Location = new System.Drawing.Point(0, 389);
            this.Btn_SABIKA.Name = "Btn_SABIKA";
            this.Btn_SABIKA.Size = new System.Drawing.Size(125, 29);
            this.Btn_SABIKA.TabIndex = 52;
            this.Btn_SABIKA.Text = "SABIKA BİLGİLERİ";
            this.Btn_SABIKA.UseVisualStyleBackColor = false;
            // 
            // Btn4_Menu2
            // 
            this.Btn4_Menu2.BackColor = System.Drawing.Color.IndianRed;
            this.Btn4_Menu2.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn4_Menu2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn4_Menu2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Btn4_Menu2.Location = new System.Drawing.Point(0, 364);
            this.Btn4_Menu2.Name = "Btn4_Menu2";
            this.Btn4_Menu2.Size = new System.Drawing.Size(125, 25);
            this.Btn4_Menu2.TabIndex = 51;
            this.Btn4_Menu2.Text = "Belge Gönder";
            this.Btn4_Menu2.UseVisualStyleBackColor = false;
            this.Btn4_Menu2.Visible = false;
            // 
            // Btn4_Menu1
            // 
            this.Btn4_Menu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn4_Menu1.Location = new System.Drawing.Point(0, 341);
            this.Btn4_Menu1.Name = "Btn4_Menu1";
            this.Btn4_Menu1.Size = new System.Drawing.Size(125, 23);
            this.Btn4_Menu1.TabIndex = 50;
            this.Btn4_Menu1.Text = "Bilgileri Gönder";
            this.Btn4_Menu1.UseVisualStyleBackColor = true;
            this.Btn4_Menu1.Visible = false;
            // 
            // Btn_Saglik
            // 
            this.Btn_Saglik.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_Saglik.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_Saglik.Location = new System.Drawing.Point(0, 312);
            this.Btn_Saglik.Name = "Btn_Saglik";
            this.Btn_Saglik.Size = new System.Drawing.Size(125, 29);
            this.Btn_Saglik.TabIndex = 49;
            this.Btn_Saglik.Text = "SAĞ.RAP BİLGİLERİ";
            this.Btn_Saglik.UseVisualStyleBackColor = false;
            // 
            // Btn3_Menu2
            // 
            this.Btn3_Menu2.BackColor = System.Drawing.Color.IndianRed;
            this.Btn3_Menu2.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn3_Menu2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Btn3_Menu2.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Btn3_Menu2.Location = new System.Drawing.Point(0, 287);
            this.Btn3_Menu2.Name = "Btn3_Menu2";
            this.Btn3_Menu2.Size = new System.Drawing.Size(125, 25);
            this.Btn3_Menu2.TabIndex = 48;
            this.Btn3_Menu2.Text = "Belge Gönder";
            this.Btn3_Menu2.UseVisualStyleBackColor = false;
            this.Btn3_Menu2.Visible = false;
            // 
            // Btn3_Menu1
            // 
            this.Btn3_Menu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn3_Menu1.Location = new System.Drawing.Point(0, 264);
            this.Btn3_Menu1.Name = "Btn3_Menu1";
            this.Btn3_Menu1.Size = new System.Drawing.Size(125, 23);
            this.Btn3_Menu1.TabIndex = 47;
            this.Btn3_Menu1.Text = "Bilgileri Gönder";
            this.Btn3_Menu1.UseVisualStyleBackColor = true;
            this.Btn3_Menu1.Visible = false;
            // 
            // Btn_OgrnBilgileri
            // 
            this.Btn_OgrnBilgileri.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_OgrnBilgileri.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_OgrnBilgileri.Location = new System.Drawing.Point(0, 235);
            this.Btn_OgrnBilgileri.Name = "Btn_OgrnBilgileri";
            this.Btn_OgrnBilgileri.Size = new System.Drawing.Size(125, 29);
            this.Btn_OgrnBilgileri.TabIndex = 4;
            this.Btn_OgrnBilgileri.Text = "ÖĞRENİM BİLGİLERİ";
            this.Btn_OgrnBilgileri.UseVisualStyleBackColor = false;
            // 
            // Btn2_Menu2
            // 
            this.Btn2_Menu2.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn2_Menu2.Location = new System.Drawing.Point(0, 206);
            this.Btn2_Menu2.Name = "Btn2_Menu2";
            this.Btn2_Menu2.Size = new System.Drawing.Size(125, 29);
            this.Btn2_Menu2.TabIndex = 3;
            this.Btn2_Menu2.Text = "Web Resim Aktar";
            this.Btn2_Menu2.UseVisualStyleBackColor = true;
            this.Btn2_Menu2.Visible = false;
            // 
            // Btn2_Menu1
            // 
            this.Btn2_Menu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn2_Menu1.Location = new System.Drawing.Point(0, 177);
            this.Btn2_Menu1.Name = "Btn2_Menu1";
            this.Btn2_Menu1.Size = new System.Drawing.Size(125, 29);
            this.Btn2_Menu1.TabIndex = 3;
            this.Btn2_Menu1.Text = "Resim Aktar";
            this.Btn2_Menu1.UseVisualStyleBackColor = true;
            this.Btn2_Menu1.Visible = false;
            // 
            // Btn_Resim
            // 
            this.Btn_Resim.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Btn_Resim.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_Resim.Location = new System.Drawing.Point(0, 148);
            this.Btn_Resim.Name = "Btn_Resim";
            this.Btn_Resim.Size = new System.Drawing.Size(125, 29);
            this.Btn_Resim.TabIndex = 3;
            this.Btn_Resim.Text = "RESİM İŞLEMLERİ";
            this.Btn_Resim.UseVisualStyleBackColor = false;
            // 
            // Btn_Menu2
            // 
            this.Btn_Menu2.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_Menu2.Location = new System.Drawing.Point(0, 125);
            this.Btn_Menu2.Name = "Btn_Menu2";
            this.Btn_Menu2.Size = new System.Drawing.Size(125, 23);
            this.Btn_Menu2.TabIndex = 2;
            this.Btn_Menu2.Text = "Bilgileri Gönder";
            this.Btn_Menu2.UseVisualStyleBackColor = true;
            this.Btn_Menu2.Visible = false;
            // 
            // Btn_Menu1
            // 
            this.Btn_Menu1.Dock = System.Windows.Forms.DockStyle.Top;
            this.Btn_Menu1.Location = new System.Drawing.Point(0, 102);
            this.Btn_Menu1.Name = "Btn_Menu1";
            this.Btn_Menu1.Size = new System.Drawing.Size(125, 23);
            this.Btn_Menu1.TabIndex = 2;
            this.Btn_Menu1.Text = "Adayı Kaydet";
            this.Btn_Menu1.UseVisualStyleBackColor = true;
            this.Btn_Menu1.Visible = false;
            // 
            // Tnk_RESIM_Kursiyer
            // 
            this.Tnk_RESIM_Kursiyer.BackColor = System.Drawing.Color.White;
            this.Tnk_RESIM_Kursiyer.Image = ((System.Drawing.Image)(resources.GetObject("Tnk_RESIM_Kursiyer.Image")));
            this.Tnk_RESIM_Kursiyer.Location = new System.Drawing.Point(8, 19);
            this.Tnk_RESIM_Kursiyer.Name = "Tnk_RESIM_Kursiyer";
            this.Tnk_RESIM_Kursiyer.Size = new System.Drawing.Size(90, 114);
            this.Tnk_RESIM_Kursiyer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Tnk_RESIM_Kursiyer.TabIndex = 33;
            this.Tnk_RESIM_Kursiyer.TabStop = false;
            // 
            // MebbisWebForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1450, 1061);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Grp_2);
            this.Controls.Add(this.Grp_1);
            this.Controls.Add(this.panelBrowser);
            this.Name = "MebbisWebForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MebbisWebForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelBrowser.ResumeLayout(false);
            this.Grp_2.ResumeLayout(false);
            this.Grp_2.PerformLayout();
            this.Grp_1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Tnk_RESIM_Kursiyer)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBrowser;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button Btn_OzelMTSK;
        private System.Windows.Forms.GroupBox Grp_2;
        private System.Windows.Forms.Label Lbl_1;
        private System.Windows.Forms.Label Lbl_5;
        private System.Windows.Forms.Label Ehliyetsinif;
        private System.Windows.Forms.Label KursiyersSoyadi;
        private System.Windows.Forms.Label KursiyerAdi;
        private System.Windows.Forms.GroupBox Grp_1;
        private System.Windows.Forms.PictureBox Tnk_RESIM_Kursiyer;
        private System.Windows.Forms.Label Onceki_Ehliyet;
        private System.Windows.Forms.Label Donem_Adi;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button Btn_Menu2;
        private System.Windows.Forms.Button Btn_Menu1;
        private System.Windows.Forms.Button Btn_Resim;
        private System.Windows.Forms.Button Btn2_Menu1;
        private System.Windows.Forms.Button Btn2_Menu2;
        private System.Windows.Forms.Button Btn3_Menu2;
        private System.Windows.Forms.Button Btn3_Menu1;
        private System.Windows.Forms.Button Btn_OgrnBilgileri;
        private System.Windows.Forms.Button Btn5_Menu2;
        private System.Windows.Forms.Button Btn5_Menu1;
        private System.Windows.Forms.Button Btn_SABIKA;
        private System.Windows.Forms.Button Btn4_Menu2;
        private System.Windows.Forms.Button Btn4_Menu1;
        private System.Windows.Forms.Button Btn_Saglik;
        private System.Windows.Forms.Button Btn7_Menu2;
        private System.Windows.Forms.Button Btn7_Menu1;
        private System.Windows.Forms.Button Btn_Sozlesme;
        private System.Windows.Forms.Button Btn6_Menu1;
        private System.Windows.Forms.Button Btn_imzasi;
        private System.Windows.Forms.Button Btn_fatura;
        private System.Windows.Forms.Button Btn8_Menu1;
        private System.Windows.Forms.Button Btn_adres;
        private System.Windows.Forms.Button Btn_HizliAktar;
        private System.Windows.Forms.Button Btn_IslemiKes;
    }
}