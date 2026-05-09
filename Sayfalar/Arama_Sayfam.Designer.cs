namespace Kolera_Mtsk.Sayfalar
{
    partial class Arama_Sayfam
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Arama_Sayfam));
            this.Tnk_Adim = new System.Windows.Forms.Label();
            this.Grparama = new System.Windows.Forms.GroupBox();
            this.Dvg_Kursiyerler = new System.Windows.Forms.DataGridView();
            this.PrgBar = new System.Windows.Forms.ProgressBar();
            this.Txt_Ara = new System.Windows.Forms.TextBox();
            this.Panel_Sol = new System.Windows.Forms.Panel();
            this.Panel_Alt = new System.Windows.Forms.Panel();
            this.Lisans_Durumu = new System.Windows.Forms.Label();
            this.Kullanici_txt = new System.Windows.Forms.Label();
            this.Lbl_Lisans = new System.Windows.Forms.Label();
            this.Tnk_RESIM = new System.Windows.Forms.PictureBox();
            this.Piclogo = new System.Windows.Forms.PictureBox();
            this.PictureBox4 = new System.Windows.Forms.PictureBox();
            this.Grparama.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Dvg_Kursiyerler)).BeginInit();
            this.Panel_Sol.SuspendLayout();
            this.Panel_Alt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Tnk_RESIM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Piclogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox4)).BeginInit();
            this.SuspendLayout();
            // 
            // Tnk_Adim
            // 
            this.Tnk_Adim.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Tnk_Adim.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Tnk_Adim.ForeColor = System.Drawing.Color.IndianRed;
            this.Tnk_Adim.Location = new System.Drawing.Point(-624, 188);
            this.Tnk_Adim.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Tnk_Adim.Name = "Tnk_Adim";
            this.Tnk_Adim.Size = new System.Drawing.Size(91, 71);
            this.Tnk_Adim.TabIndex = 10;
            this.Tnk_Adim.Text = "Kursiyer";
            // 
            // Grparama
            // 
            this.Grparama.Controls.Add(this.Dvg_Kursiyerler);
            this.Grparama.Controls.Add(this.Tnk_Adim);
            this.Grparama.Controls.Add(this.PrgBar);
            this.Grparama.Controls.Add(this.Txt_Ara);
            this.Grparama.Controls.Add(this.Panel_Sol);
            this.Grparama.Location = new System.Drawing.Point(12, 12);
            this.Grparama.Name = "Grparama";
            this.Grparama.Size = new System.Drawing.Size(1258, 713);
            this.Grparama.TabIndex = 52;
            this.Grparama.TabStop = false;
            this.Grparama.Text = "Kuriyer Bilgileri";
            // 
            // Dvg_Kursiyerler
            // 
            this.Dvg_Kursiyerler.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Dvg_Kursiyerler.Location = new System.Drawing.Point(105, 64);
            this.Dvg_Kursiyerler.Margin = new System.Windows.Forms.Padding(2);
            this.Dvg_Kursiyerler.Name = "Dvg_Kursiyerler";
            this.Dvg_Kursiyerler.Size = new System.Drawing.Size(1150, 646);
            this.Dvg_Kursiyerler.TabIndex = 13;
            this.Dvg_Kursiyerler.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.DvgKursiyerler_CellClick);
            this.Dvg_Kursiyerler.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.Dvg_Kursiyerler_CellDoubleClick);
            // 
            // PrgBar
            // 
            this.PrgBar.Location = new System.Drawing.Point(6, 24);
            this.PrgBar.Margin = new System.Windows.Forms.Padding(2);
            this.PrgBar.Name = "PrgBar";
            this.PrgBar.Size = new System.Drawing.Size(98, 19);
            this.PrgBar.TabIndex = 7;
            this.PrgBar.Visible = false;
            // 
            // Txt_Ara
            // 
            this.Txt_Ara.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.Txt_Ara.Location = new System.Drawing.Point(108, 17);
            this.Txt_Ara.Margin = new System.Windows.Forms.Padding(2);
            this.Txt_Ara.Name = "Txt_Ara";
            this.Txt_Ara.Size = new System.Drawing.Size(1776, 26);
            this.Txt_Ara.TabIndex = 6;
            // 
            // Panel_Sol
            // 
            this.Panel_Sol.Controls.Add(this.Tnk_RESIM);
            this.Panel_Sol.Controls.Add(this.Piclogo);
            this.Panel_Sol.Dock = System.Windows.Forms.DockStyle.Left;
            this.Panel_Sol.Location = new System.Drawing.Point(3, 16);
            this.Panel_Sol.Name = "Panel_Sol";
            this.Panel_Sol.Size = new System.Drawing.Size(102, 694);
            this.Panel_Sol.TabIndex = 11;
            // 
            // Panel_Alt
            // 
            this.Panel_Alt.Controls.Add(this.Lisans_Durumu);
            this.Panel_Alt.Controls.Add(this.Kullanici_txt);
            this.Panel_Alt.Controls.Add(this.Lbl_Lisans);
            this.Panel_Alt.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.Panel_Alt.Location = new System.Drawing.Point(0, 666);
            this.Panel_Alt.Name = "Panel_Alt";
            this.Panel_Alt.Size = new System.Drawing.Size(1273, 29);
            this.Panel_Alt.TabIndex = 54;
            // 
            // Lisans_Durumu
            // 
            this.Lisans_Durumu.AutoSize = true;
            this.Lisans_Durumu.Location = new System.Drawing.Point(1560, 10);
            this.Lisans_Durumu.Name = "Lisans_Durumu";
            this.Lisans_Durumu.Size = new System.Drawing.Size(35, 13);
            this.Lisans_Durumu.TabIndex = 28;
            this.Lisans_Durumu.Text = "label1";
            // 
            // Kullanici_txt
            // 
            this.Kullanici_txt.AutoSize = true;
            this.Kullanici_txt.Location = new System.Drawing.Point(12, 10);
            this.Kullanici_txt.Name = "Kullanici_txt";
            this.Kullanici_txt.Size = new System.Drawing.Size(35, 13);
            this.Kullanici_txt.TabIndex = 28;
            this.Kullanici_txt.Text = "label1";
            // 
            // Lbl_Lisans
            // 
            this.Lbl_Lisans.AutoSize = true;
            this.Lbl_Lisans.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Lbl_Lisans.ForeColor = System.Drawing.Color.Transparent;
            this.Lbl_Lisans.Location = new System.Drawing.Point(1728, 10);
            this.Lbl_Lisans.Name = "Lbl_Lisans";
            this.Lbl_Lisans.Size = new System.Drawing.Size(145, 13);
            this.Lbl_Lisans.TabIndex = 27;
            this.Lbl_Lisans.Text = "Tarantula @2025 Ver.1.9.8.0";
            // 
            // Tnk_RESIM
            // 
            this.Tnk_RESIM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Tnk_RESIM.Image = ((System.Drawing.Image)(resources.GetObject("Tnk_RESIM.Image")));
            this.Tnk_RESIM.Location = new System.Drawing.Point(8, 48);
            this.Tnk_RESIM.Margin = new System.Windows.Forms.Padding(2);
            this.Tnk_RESIM.Name = "Tnk_RESIM";
            this.Tnk_RESIM.Size = new System.Drawing.Size(87, 122);
            this.Tnk_RESIM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Tnk_RESIM.TabIndex = 9;
            this.Tnk_RESIM.TabStop = false;
            // 
            // Piclogo
            // 
            this.Piclogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.Piclogo.Image = ((System.Drawing.Image)(resources.GetObject("Piclogo.Image")));
            this.Piclogo.Location = new System.Drawing.Point(24, 373);
            this.Piclogo.Margin = new System.Windows.Forms.Padding(2);
            this.Piclogo.Name = "Piclogo";
            this.Piclogo.Size = new System.Drawing.Size(77, 230);
            this.Piclogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Piclogo.TabIndex = 8;
            this.Piclogo.TabStop = false;
            // 
            // PictureBox4
            // 
            this.PictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox4.Image")));
            this.PictureBox4.Location = new System.Drawing.Point(1867, -110);
            this.PictureBox4.Name = "PictureBox4";
            this.PictureBox4.Size = new System.Drawing.Size(39, 36);
            this.PictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.PictureBox4.TabIndex = 53;
            this.PictureBox4.TabStop = false;
            // 
            // Arama_Sayfam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1273, 695);
            this.Controls.Add(this.Grparama);
            this.Controls.Add(this.Panel_Alt);
            this.Controls.Add(this.PictureBox4);
            this.Name = "Arama_Sayfam";
            this.Text = "Arama_Sayfam";
            this.Grparama.ResumeLayout(false);
            this.Grparama.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Dvg_Kursiyerler)).EndInit();
            this.Panel_Sol.ResumeLayout(false);
            this.Panel_Alt.ResumeLayout(false);
            this.Panel_Alt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Tnk_RESIM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Piclogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label Tnk_Adim;
        private System.Windows.Forms.GroupBox Grparama;
        private System.Windows.Forms.DataGridView Dvg_Kursiyerler;
        private System.Windows.Forms.ProgressBar PrgBar;
        private System.Windows.Forms.TextBox Txt_Ara;
        private System.Windows.Forms.Panel Panel_Sol;
        private System.Windows.Forms.PictureBox Tnk_RESIM;
        private System.Windows.Forms.PictureBox Piclogo;
        private System.Windows.Forms.Panel Panel_Alt;
        private System.Windows.Forms.Label Lisans_Durumu;
        private System.Windows.Forms.Label Kullanici_txt;
        private System.Windows.Forms.Label Lbl_Lisans;
        private System.Windows.Forms.PictureBox PictureBox4;
    }
}