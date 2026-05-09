namespace Kolera_Mtsk.Sayfalar
{
    partial class LisansDetayForm
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeaderSub = new System.Windows.Forms.Label();
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pnlMiddle = new System.Windows.Forms.TableLayoutPanel();
            this.pnlKurumBilgileri = new System.Windows.Forms.Panel();
            this.tblKurum = new System.Windows.Forms.TableLayoutPanel();
            this.lblKurumKoduTitle = new System.Windows.Forms.Label();
            this.lblKurumAdiTitle = new System.Windows.Forms.Label();
            this.lblMusteriNoTitle = new System.Windows.Forms.Label();
            this.lblKurumKoduValue = new System.Windows.Forms.Label();
            this.lblKurumAdiValue = new System.Windows.Forms.Label();
            this.lblMusteriNoValue = new System.Windows.Forms.Label();
            this.lblKurumCardTitle = new System.Windows.Forms.Label();
            this.pnlLisansIslemleri = new System.Windows.Forms.Panel();
            this.btnKapat = new System.Windows.Forms.Button();
            this.btnWebPanelAc = new System.Windows.Forms.Button();
            this.btnGeciciDegistir = new System.Windows.Forms.Button();
            this.txtYeniLisansNo = new System.Windows.Forms.TextBox();
            this.lblYeniNo = new System.Windows.Forms.Label();
            this.txtAdminSifre = new System.Windows.Forms.TextBox();
            this.lblAdminSifre = new System.Windows.Forms.Label();
            this.lblLisansCardTitle = new System.Windows.Forms.Label();
            this.pnlSummaryCards = new System.Windows.Forms.TableLayoutPanel();
            this.cardLisansNo = new System.Windows.Forms.Panel();
            this.lblLisansNoValue = new System.Windows.Forms.Label();
            this.lblLisansNoTitle = new System.Windows.Forms.Label();
            this.cardDurum = new System.Windows.Forms.Panel();
            this.lblDurumRozet = new System.Windows.Forms.Label();
            this.lblDurumValue = new System.Windows.Forms.Label();
            this.lblDurumTitle = new System.Windows.Forms.Label();
            this.cardBitis = new System.Windows.Forms.Panel();
            this.lblKalanGunValue = new System.Windows.Forms.Label();
            this.lblBitisValue = new System.Windows.Forms.Label();
            this.lblBitisTitle = new System.Windows.Forms.Label();
            this.pnlLog = new System.Windows.Forms.Panel();
            this.rtbLog = new System.Windows.Forms.RichTextBox();
            this.lblLogTitle = new System.Windows.Forms.Label();
            this.pnlHeader.SuspendLayout();
            this.pnlMain.SuspendLayout();
            this.pnlMiddle.SuspendLayout();
            this.pnlKurumBilgileri.SuspendLayout();
            this.tblKurum.SuspendLayout();
            this.pnlLisansIslemleri.SuspendLayout();
            this.pnlSummaryCards.SuspendLayout();
            this.cardLisansNo.SuspendLayout();
            this.cardDurum.SuspendLayout();
            this.cardBitis.SuspendLayout();
            this.pnlLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.Controls.Add(this.lblHeaderSub);
            this.pnlHeader.Controls.Add(this.lblHeaderTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
            this.pnlHeader.Size = new System.Drawing.Size(980, 92);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeaderSub
            // 
            this.lblHeaderSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeaderSub.Location = new System.Drawing.Point(16, 46);
            this.lblHeaderSub.Name = "lblHeaderSub";
            this.lblHeaderSub.Size = new System.Drawing.Size(948, 36);
            this.lblHeaderSub.TabIndex = 1;
            this.lblHeaderSub.Text = "Program lisans durumu, kurum bilgileri ve yerel lisans işlemleri bu ekrandan yöne" +
    "tilir.";
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeaderTitle.Location = new System.Drawing.Point(16, 10);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(948, 36);
            this.lblHeaderTitle.TabIndex = 0;
            this.lblHeaderTitle.Text = "LİSANS VE KURUM BİLGİLERİ";
            // 
            // pnlMain
            // 
            this.pnlMain.Controls.Add(this.pnlMiddle);
            this.pnlMain.Controls.Add(this.pnlSummaryCards);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 92);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Padding = new System.Windows.Forms.Padding(12, 10, 12, 8);
            this.pnlMain.Size = new System.Drawing.Size(980, 399);
            this.pnlMain.TabIndex = 1;
            // 
            // pnlMiddle
            // 
            this.pnlMiddle.ColumnCount = 2;
            this.pnlMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlMiddle.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.pnlMiddle.Controls.Add(this.pnlKurumBilgileri, 0, 0);
            this.pnlMiddle.Controls.Add(this.pnlLisansIslemleri, 1, 0);
            this.pnlMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMiddle.Location = new System.Drawing.Point(12, 130);
            this.pnlMiddle.Name = "pnlMiddle";
            this.pnlMiddle.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlMiddle.Size = new System.Drawing.Size(956, 261);
            this.pnlMiddle.TabIndex = 1;
            // 
            // pnlKurumBilgileri
            // 
            this.pnlKurumBilgileri.Controls.Add(this.tblKurum);
            this.pnlKurumBilgileri.Controls.Add(this.lblKurumCardTitle);
            this.pnlKurumBilgileri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlKurumBilgileri.Location = new System.Drawing.Point(6, 0);
            this.pnlKurumBilgileri.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.pnlKurumBilgileri.Name = "pnlKurumBilgileri";
            this.pnlKurumBilgileri.Padding = new System.Windows.Forms.Padding(12);
            this.pnlKurumBilgileri.Size = new System.Drawing.Size(466, 261);
            this.pnlKurumBilgileri.TabIndex = 0;
            // 
            // tblKurum
            // 
            this.tblKurum.ColumnCount = 2;
            this.tblKurum.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tblKurum.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblKurum.Controls.Add(this.lblKurumKoduTitle, 0, 0);
            this.tblKurum.Controls.Add(this.lblKurumAdiTitle, 0, 1);
            this.tblKurum.Controls.Add(this.lblMusteriNoTitle, 0, 2);
            this.tblKurum.Controls.Add(this.lblKurumKoduValue, 1, 0);
            this.tblKurum.Controls.Add(this.lblKurumAdiValue, 1, 1);
            this.tblKurum.Controls.Add(this.lblMusteriNoValue, 1, 2);
            this.tblKurum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblKurum.Location = new System.Drawing.Point(12, 40);
            this.tblKurum.Name = "tblKurum";
            this.tblKurum.RowCount = 3;
            this.tblKurum.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblKurum.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblKurum.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tblKurum.Size = new System.Drawing.Size(442, 209);
            this.tblKurum.TabIndex = 1;
            // 
            // lblKurumKoduTitle
            // 
            this.lblKurumKoduTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKurumKoduTitle.Location = new System.Drawing.Point(3, 0);
            this.lblKurumKoduTitle.Name = "lblKurumKoduTitle";
            this.lblKurumKoduTitle.Size = new System.Drawing.Size(114, 69);
            this.lblKurumKoduTitle.TabIndex = 0;
            this.lblKurumKoduTitle.Text = "Kurum Kodu";
            this.lblKurumKoduTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKurumAdiTitle
            // 
            this.lblKurumAdiTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKurumAdiTitle.Location = new System.Drawing.Point(3, 69);
            this.lblKurumAdiTitle.Name = "lblKurumAdiTitle";
            this.lblKurumAdiTitle.Size = new System.Drawing.Size(114, 69);
            this.lblKurumAdiTitle.TabIndex = 1;
            this.lblKurumAdiTitle.Text = "Firma Adı";
            this.lblKurumAdiTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMusteriNoTitle
            // 
            this.lblMusteriNoTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMusteriNoTitle.Location = new System.Drawing.Point(3, 138);
            this.lblMusteriNoTitle.Name = "lblMusteriNoTitle";
            this.lblMusteriNoTitle.Size = new System.Drawing.Size(114, 71);
            this.lblMusteriNoTitle.TabIndex = 2;
            this.lblMusteriNoTitle.Text = "Müşteri No";
            this.lblMusteriNoTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKurumKoduValue
            // 
            this.lblKurumKoduValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKurumKoduValue.Location = new System.Drawing.Point(123, 0);
            this.lblKurumKoduValue.Name = "lblKurumKoduValue";
            this.lblKurumKoduValue.Size = new System.Drawing.Size(316, 69);
            this.lblKurumKoduValue.TabIndex = 3;
            this.lblKurumKoduValue.Text = "-";
            this.lblKurumKoduValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKurumAdiValue
            // 
            this.lblKurumAdiValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblKurumAdiValue.Location = new System.Drawing.Point(123, 69);
            this.lblKurumAdiValue.Name = "lblKurumAdiValue";
            this.lblKurumAdiValue.Size = new System.Drawing.Size(316, 69);
            this.lblKurumAdiValue.TabIndex = 4;
            this.lblKurumAdiValue.Text = "-";
            this.lblKurumAdiValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMusteriNoValue
            // 
            this.lblMusteriNoValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMusteriNoValue.Location = new System.Drawing.Point(123, 138);
            this.lblMusteriNoValue.Name = "lblMusteriNoValue";
            this.lblMusteriNoValue.Size = new System.Drawing.Size(316, 71);
            this.lblMusteriNoValue.TabIndex = 5;
            this.lblMusteriNoValue.Text = "-";
            this.lblMusteriNoValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblKurumCardTitle
            // 
            this.lblKurumCardTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblKurumCardTitle.Location = new System.Drawing.Point(12, 12);
            this.lblKurumCardTitle.Name = "lblKurumCardTitle";
            this.lblKurumCardTitle.Size = new System.Drawing.Size(442, 28);
            this.lblKurumCardTitle.TabIndex = 0;
            this.lblKurumCardTitle.Text = "Kurum Bilgileri";
            // 
            // pnlLisansIslemleri
            // 
            this.pnlLisansIslemleri.Controls.Add(this.btnKapat);
            this.pnlLisansIslemleri.Controls.Add(this.btnWebPanelAc);
            this.pnlLisansIslemleri.Controls.Add(this.btnGeciciDegistir);
            this.pnlLisansIslemleri.Controls.Add(this.txtYeniLisansNo);
            this.pnlLisansIslemleri.Controls.Add(this.lblYeniNo);
            this.pnlLisansIslemleri.Controls.Add(this.txtAdminSifre);
            this.pnlLisansIslemleri.Controls.Add(this.lblAdminSifre);
            this.pnlLisansIslemleri.Controls.Add(this.lblLisansCardTitle);
            this.pnlLisansIslemleri.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLisansIslemleri.Location = new System.Drawing.Point(484, 0);
            this.pnlLisansIslemleri.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.pnlLisansIslemleri.Name = "pnlLisansIslemleri";
            this.pnlLisansIslemleri.Padding = new System.Windows.Forms.Padding(12);
            this.pnlLisansIslemleri.Size = new System.Drawing.Size(466, 261);
            this.pnlLisansIslemleri.TabIndex = 1;
            // 
            // btnKapat
            // 
            this.btnKapat.Location = new System.Drawing.Point(19, 224);
            this.btnKapat.Name = "btnKapat";
            this.btnKapat.Size = new System.Drawing.Size(430, 29);
            this.btnKapat.TabIndex = 7;
            this.btnKapat.Text = "Kapat";
            this.btnKapat.UseVisualStyleBackColor = true;
            this.btnKapat.Click += new System.EventHandler(this.btnKapat_Click);
            // 
            // btnWebPanelAc
            // 
            this.btnWebPanelAc.Location = new System.Drawing.Point(19, 189);
            this.btnWebPanelAc.Name = "btnWebPanelAc";
            this.btnWebPanelAc.Size = new System.Drawing.Size(430, 29);
            this.btnWebPanelAc.TabIndex = 6;
            this.btnWebPanelAc.Text = "Web Lisansı Kontrol Et";
            this.btnWebPanelAc.UseVisualStyleBackColor = true;
            this.btnWebPanelAc.Click += new System.EventHandler(this.btnWebPanelAc_Click);
            // 
            // btnGeciciDegistir
            // 
            this.btnGeciciDegistir.Location = new System.Drawing.Point(19, 154);
            this.btnGeciciDegistir.Name = "btnGeciciDegistir";
            this.btnGeciciDegistir.Size = new System.Drawing.Size(430, 29);
            this.btnGeciciDegistir.TabIndex = 5;
            this.btnGeciciDegistir.Text = "Yerel Lisansı Güncelle";
            this.btnGeciciDegistir.UseVisualStyleBackColor = true;
            this.btnGeciciDegistir.Click += new System.EventHandler(this.btnGeciciDegistir_Click);
            // 
            // txtYeniLisansNo
            // 
            this.txtYeniLisansNo.Location = new System.Drawing.Point(19, 125);
            this.txtYeniLisansNo.Name = "txtYeniLisansNo";
            this.txtYeniLisansNo.Size = new System.Drawing.Size(430, 20);
            this.txtYeniLisansNo.TabIndex = 4;
            // 
            // lblYeniNo
            // 
            this.lblYeniNo.Location = new System.Drawing.Point(16, 102);
            this.lblYeniNo.Name = "lblYeniNo";
            this.lblYeniNo.Size = new System.Drawing.Size(140, 20);
            this.lblYeniNo.TabIndex = 3;
            this.lblYeniNo.Text = "Yeni Lisans No";
            // 
            // txtAdminSifre
            // 
            this.txtAdminSifre.Location = new System.Drawing.Point(19, 75);
            this.txtAdminSifre.Name = "txtAdminSifre";
            this.txtAdminSifre.PasswordChar = '*';
            this.txtAdminSifre.Size = new System.Drawing.Size(430, 20);
            this.txtAdminSifre.TabIndex = 2;
            // 
            // lblAdminSifre
            // 
            this.lblAdminSifre.Location = new System.Drawing.Point(16, 52);
            this.lblAdminSifre.Name = "lblAdminSifre";
            this.lblAdminSifre.Size = new System.Drawing.Size(120, 20);
            this.lblAdminSifre.TabIndex = 1;
            this.lblAdminSifre.Text = "Admin Şifresi";
            // 
            // lblLisansCardTitle
            // 
            this.lblLisansCardTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLisansCardTitle.Location = new System.Drawing.Point(12, 12);
            this.lblLisansCardTitle.Name = "lblLisansCardTitle";
            this.lblLisansCardTitle.Size = new System.Drawing.Size(442, 28);
            this.lblLisansCardTitle.TabIndex = 0;
            this.lblLisansCardTitle.Text = "Lisans İşlemleri";
            // 
            // pnlSummaryCards
            // 
            this.pnlSummaryCards.ColumnCount = 3;
            this.pnlSummaryCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlSummaryCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.pnlSummaryCards.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.pnlSummaryCards.Controls.Add(this.cardLisansNo, 0, 0);
            this.pnlSummaryCards.Controls.Add(this.cardDurum, 1, 0);
            this.pnlSummaryCards.Controls.Add(this.cardBitis, 2, 0);
            this.pnlSummaryCards.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSummaryCards.Location = new System.Drawing.Point(12, 10);
            this.pnlSummaryCards.Name = "pnlSummaryCards";
            this.pnlSummaryCards.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.pnlSummaryCards.Size = new System.Drawing.Size(956, 120);
            this.pnlSummaryCards.TabIndex = 0;
            // 
            // cardLisansNo
            // 
            this.cardLisansNo.Controls.Add(this.lblLisansNoValue);
            this.cardLisansNo.Controls.Add(this.lblLisansNoTitle);
            this.cardLisansNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardLisansNo.Location = new System.Drawing.Point(6, 6);
            this.cardLisansNo.Margin = new System.Windows.Forms.Padding(6);
            this.cardLisansNo.Name = "cardLisansNo";
            this.cardLisansNo.Padding = new System.Windows.Forms.Padding(12);
            this.cardLisansNo.Size = new System.Drawing.Size(306, 108);
            this.cardLisansNo.TabIndex = 0;
            // 
            // lblLisansNoValue
            // 
            this.lblLisansNoValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblLisansNoValue.Location = new System.Drawing.Point(12, 36);
            this.lblLisansNoValue.Name = "lblLisansNoValue";
            this.lblLisansNoValue.Size = new System.Drawing.Size(282, 60);
            this.lblLisansNoValue.TabIndex = 1;
            this.lblLisansNoValue.Text = "-";
            this.lblLisansNoValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLisansNoTitle
            // 
            this.lblLisansNoTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLisansNoTitle.Location = new System.Drawing.Point(12, 12);
            this.lblLisansNoTitle.Name = "lblLisansNoTitle";
            this.lblLisansNoTitle.Size = new System.Drawing.Size(282, 24);
            this.lblLisansNoTitle.TabIndex = 0;
            this.lblLisansNoTitle.Text = "Lisans No";
            // 
            // cardDurum
            // 
            this.cardDurum.Controls.Add(this.lblDurumRozet);
            this.cardDurum.Controls.Add(this.lblDurumValue);
            this.cardDurum.Controls.Add(this.lblDurumTitle);
            this.cardDurum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardDurum.Location = new System.Drawing.Point(324, 6);
            this.cardDurum.Margin = new System.Windows.Forms.Padding(6);
            this.cardDurum.Name = "cardDurum";
            this.cardDurum.Padding = new System.Windows.Forms.Padding(12);
            this.cardDurum.Size = new System.Drawing.Size(306, 108);
            this.cardDurum.TabIndex = 1;
            // 
            // lblDurumRozet
            // 
            this.lblDurumRozet.Location = new System.Drawing.Point(16, 69);
            this.lblDurumRozet.Name = "lblDurumRozet";
            this.lblDurumRozet.Size = new System.Drawing.Size(120, 24);
            this.lblDurumRozet.TabIndex = 2;
            this.lblDurumRozet.Text = "-";
            this.lblDurumRozet.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDurumValue
            // 
            this.lblDurumValue.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDurumValue.Location = new System.Drawing.Point(12, 36);
            this.lblDurumValue.Name = "lblDurumValue";
            this.lblDurumValue.Size = new System.Drawing.Size(282, 28);
            this.lblDurumValue.TabIndex = 1;
            this.lblDurumValue.Text = "-";
            this.lblDurumValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblDurumTitle
            // 
            this.lblDurumTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDurumTitle.Location = new System.Drawing.Point(12, 12);
            this.lblDurumTitle.Name = "lblDurumTitle";
            this.lblDurumTitle.Size = new System.Drawing.Size(282, 24);
            this.lblDurumTitle.TabIndex = 0;
            this.lblDurumTitle.Text = "Lisans Durumu";
            // 
            // cardBitis
            // 
            this.cardBitis.Controls.Add(this.lblKalanGunValue);
            this.cardBitis.Controls.Add(this.lblBitisValue);
            this.cardBitis.Controls.Add(this.lblBitisTitle);
            this.cardBitis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardBitis.Location = new System.Drawing.Point(642, 6);
            this.cardBitis.Margin = new System.Windows.Forms.Padding(6);
            this.cardBitis.Name = "cardBitis";
            this.cardBitis.Padding = new System.Windows.Forms.Padding(12);
            this.cardBitis.Size = new System.Drawing.Size(308, 108);
            this.cardBitis.TabIndex = 2;
            // 
            // lblKalanGunValue
            // 
            this.lblKalanGunValue.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblKalanGunValue.Location = new System.Drawing.Point(12, 64);
            this.lblKalanGunValue.Name = "lblKalanGunValue";
            this.lblKalanGunValue.Size = new System.Drawing.Size(284, 24);
            this.lblKalanGunValue.TabIndex = 2;
            this.lblKalanGunValue.Text = "Kalan Gün: -";
            this.lblKalanGunValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBitisValue
            // 
            this.lblBitisValue.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBitisValue.Location = new System.Drawing.Point(12, 36);
            this.lblBitisValue.Name = "lblBitisValue";
            this.lblBitisValue.Size = new System.Drawing.Size(284, 28);
            this.lblBitisValue.TabIndex = 1;
            this.lblBitisValue.Text = "-";
            this.lblBitisValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblBitisTitle
            // 
            this.lblBitisTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblBitisTitle.Location = new System.Drawing.Point(12, 12);
            this.lblBitisTitle.Name = "lblBitisTitle";
            this.lblBitisTitle.Size = new System.Drawing.Size(284, 24);
            this.lblBitisTitle.TabIndex = 0;
            this.lblBitisTitle.Text = "Bitiş Tarihi / Kalan Gün";
            // 
            // pnlLog
            // 
            this.pnlLog.Controls.Add(this.rtbLog);
            this.pnlLog.Controls.Add(this.lblLogTitle);
            this.pnlLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLog.Location = new System.Drawing.Point(0, 491);
            this.pnlLog.Name = "pnlLog";
            this.pnlLog.Padding = new System.Windows.Forms.Padding(12);
            this.pnlLog.Size = new System.Drawing.Size(980, 129);
            this.pnlLog.TabIndex = 2;
            // 
            // rtbLog
            // 
            this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLog.Location = new System.Drawing.Point(12, 36);
            this.rtbLog.Name = "rtbLog";
            this.rtbLog.ReadOnly = true;
            this.rtbLog.Size = new System.Drawing.Size(956, 81);
            this.rtbLog.TabIndex = 1;
            this.rtbLog.Text = "";
            // 
            // lblLogTitle
            // 
            this.lblLogTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblLogTitle.Location = new System.Drawing.Point(12, 12);
            this.lblLogTitle.Name = "lblLogTitle";
            this.lblLogTitle.Size = new System.Drawing.Size(956, 24);
            this.lblLogTitle.TabIndex = 0;
            this.lblLogTitle.Text = "İşlem Durum / Log";
            // 
            // LisansDetayForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 620);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlLog);
            this.Controls.Add(this.pnlHeader);
            this.Name = "LisansDetayForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MTSK LISANS ISLEMLERI";
            this.pnlHeader.ResumeLayout(false);
            this.pnlMain.ResumeLayout(false);
            this.pnlMiddle.ResumeLayout(false);
            this.pnlKurumBilgileri.ResumeLayout(false);
            this.tblKurum.ResumeLayout(false);
            this.pnlLisansIslemleri.ResumeLayout(false);
            this.pnlLisansIslemleri.PerformLayout();
            this.pnlSummaryCards.ResumeLayout(false);
            this.cardLisansNo.ResumeLayout(false);
            this.cardDurum.ResumeLayout(false);
            this.cardBitis.ResumeLayout(false);
            this.pnlLog.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeaderSub;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.TableLayoutPanel pnlMiddle;
        private System.Windows.Forms.Panel pnlKurumBilgileri;
        private System.Windows.Forms.TableLayoutPanel tblKurum;
        private System.Windows.Forms.Label lblKurumCardTitle;
        private System.Windows.Forms.Label lblKurumKoduTitle;
        private System.Windows.Forms.Label lblKurumAdiTitle;
        private System.Windows.Forms.Label lblMusteriNoTitle;
        private System.Windows.Forms.Label lblKurumKoduValue;
        private System.Windows.Forms.Label lblKurumAdiValue;
        private System.Windows.Forms.Label lblMusteriNoValue;
        private System.Windows.Forms.Panel pnlLisansIslemleri;
        private System.Windows.Forms.Label lblLisansCardTitle;
        private System.Windows.Forms.Label lblAdminSifre;
        private System.Windows.Forms.TextBox txtAdminSifre;
        private System.Windows.Forms.Label lblYeniNo;
        private System.Windows.Forms.TextBox txtYeniLisansNo;
        private System.Windows.Forms.Button btnGeciciDegistir;
        private System.Windows.Forms.Button btnWebPanelAc;
        private System.Windows.Forms.Button btnKapat;
        private System.Windows.Forms.TableLayoutPanel pnlSummaryCards;
        private System.Windows.Forms.Panel cardLisansNo;
        private System.Windows.Forms.Label lblLisansNoTitle;
        private System.Windows.Forms.Label lblLisansNoValue;
        private System.Windows.Forms.Panel cardDurum;
        private System.Windows.Forms.Label lblDurumTitle;
        private System.Windows.Forms.Label lblDurumValue;
        private System.Windows.Forms.Label lblDurumRozet;
        private System.Windows.Forms.Panel cardBitis;
        private System.Windows.Forms.Label lblBitisTitle;
        private System.Windows.Forms.Label lblBitisValue;
        private System.Windows.Forms.Label lblKalanGunValue;
        private System.Windows.Forms.Panel pnlLog;
        private System.Windows.Forms.Label lblLogTitle;
        private System.Windows.Forms.RichTextBox rtbLog;
    }
}
