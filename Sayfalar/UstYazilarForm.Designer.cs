namespace Kolera_Mtsk.Sayfalar
{
    partial class UstYazilarForm
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
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblHeaderSub = new System.Windows.Forms.Label();
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.pnlLeftCard = new System.Windows.Forms.Panel();
            this.pnlLeftButtonsBottom = new System.Windows.Forms.FlowLayoutPanel();
            this.btnYeni = new System.Windows.Forms.Button();
            this.btnAc = new System.Windows.Forms.Button();
            this.btnDosyaEkle = new System.Windows.Forms.Button();
            this.btnYenile = new System.Windows.Forms.Button();
            this.btnKlasorAc = new System.Windows.Forms.Button();
            this.btnSil = new System.Windows.Forms.Button();
            this.btnKopyaOlustur = new System.Windows.Forms.Button();
            this.lstSablonlar = new System.Windows.Forms.ListBox();
            this.txtSablonAra = new System.Windows.Forms.TextBox();
            this.lblSablonlar = new System.Windows.Forms.Label();
            this.splitEditorRight = new System.Windows.Forms.SplitContainer();
            this.pnlEditorWrap = new System.Windows.Forms.Panel();
            this.pnlEditorCard = new System.Windows.Forms.Panel();
            this.rtbEditor = new System.Windows.Forms.RichTextBox();
            this.tsFormat = new System.Windows.Forms.ToolStrip();
            this.cmbFont = new System.Windows.Forms.ToolStripComboBox();
            this.cmbFontSize = new System.Windows.Forms.ToolStripComboBox();
            this.btnBold = new System.Windows.Forms.ToolStripButton();
            this.btnItalic = new System.Windows.Forms.ToolStripButton();
            this.btnUnderline = new System.Windows.Forms.ToolStripButton();
            this.btnAlignLeft = new System.Windows.Forms.ToolStripButton();
            this.btnAlignCenter = new System.Windows.Forms.ToolStripButton();
            this.btnAlignRight = new System.Windows.Forms.ToolStripButton();
            this.btnBullet = new System.Windows.Forms.ToolStripButton();
            this.lblAktifDosya = new System.Windows.Forms.Label();
            this.pnlRightCard = new System.Windows.Forms.Panel();
            this.flowRightActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnKaydet = new System.Windows.Forms.Button();
            this.btnOnizleme = new System.Windows.Forms.Button();
            this.btnYazdir = new System.Windows.Forms.Button();
            this.btnWordIleAc = new System.Windows.Forms.Button();
            this.btnSablonBilgisi = new System.Windows.Forms.Button();
            this.statusMain = new System.Windows.Forms.StatusStrip();
            this.lblDurumDosya = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDurumAyrac1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDurumSayi = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDurumAyrac2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblDurumMesaj = new System.Windows.Forms.ToolStripStatusLabel();
            this.docYazdir = new System.Drawing.Printing.PrintDocument();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.pnlLeftCard.SuspendLayout();
            this.pnlLeftButtonsBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitEditorRight)).BeginInit();
            this.splitEditorRight.Panel1.SuspendLayout();
            this.splitEditorRight.Panel2.SuspendLayout();
            this.splitEditorRight.SuspendLayout();
            this.pnlEditorWrap.SuspendLayout();
            this.pnlEditorCard.SuspendLayout();
            this.tsFormat.SuspendLayout();
            this.pnlRightCard.SuspendLayout();
            this.flowRightActions.SuspendLayout();
            this.statusMain.SuspendLayout();
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
            this.pnlHeader.Size = new System.Drawing.Size(1260, 88);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblHeaderSub
            // 
            this.lblHeaderSub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeaderSub.Location = new System.Drawing.Point(16, 44);
            this.lblHeaderSub.Name = "lblHeaderSub";
            this.lblHeaderSub.Size = new System.Drawing.Size(1228, 34);
            this.lblHeaderSub.TabIndex = 1;
            this.lblHeaderSub.Text = "Kurumsal yazışma şablonlarını oluşturun, düzenleyin ve yazdırın.";
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeaderTitle.Location = new System.Drawing.Point(16, 10);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(1228, 34);
            this.lblHeaderTitle.TabIndex = 0;
            this.lblHeaderTitle.Text = "ÜST YAZI YÖNETİM MERKEZİ";
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(0, 88);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.pnlLeftCard);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.splitEditorRight);
            this.splitMain.Size = new System.Drawing.Size(1260, 589);
            this.splitMain.SplitterDistance = 300;
            this.splitMain.TabIndex = 1;
            // 
            // pnlLeftCard
            // 
            this.pnlLeftCard.Controls.Add(this.pnlLeftButtonsBottom);
            this.pnlLeftCard.Controls.Add(this.lstSablonlar);
            this.pnlLeftCard.Controls.Add(this.txtSablonAra);
            this.pnlLeftCard.Controls.Add(this.lblSablonlar);
            this.pnlLeftCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftCard.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftCard.Name = "pnlLeftCard";
            this.pnlLeftCard.Padding = new System.Windows.Forms.Padding(12);
            this.pnlLeftCard.Size = new System.Drawing.Size(300, 589);
            this.pnlLeftCard.TabIndex = 0;
            // 
            // pnlLeftButtonsBottom
            // 
            this.pnlLeftButtonsBottom.Controls.Add(this.btnYeni);
            this.pnlLeftButtonsBottom.Controls.Add(this.btnAc);
            this.pnlLeftButtonsBottom.Controls.Add(this.btnDosyaEkle);
            this.pnlLeftButtonsBottom.Controls.Add(this.btnYenile);
            this.pnlLeftButtonsBottom.Controls.Add(this.btnKlasorAc);
            this.pnlLeftButtonsBottom.Controls.Add(this.btnSil);
            this.pnlLeftButtonsBottom.Controls.Add(this.btnKopyaOlustur);
            this.pnlLeftButtonsBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlLeftButtonsBottom.Location = new System.Drawing.Point(12, 468);
            this.pnlLeftButtonsBottom.Name = "pnlLeftButtonsBottom";
            this.pnlLeftButtonsBottom.Size = new System.Drawing.Size(276, 109);
            this.pnlLeftButtonsBottom.TabIndex = 3;
            // 
            // btnYeni
            // 
            this.btnYeni.Location = new System.Drawing.Point(3, 3);
            this.btnYeni.Name = "btnYeni";
            this.btnYeni.Size = new System.Drawing.Size(83, 30);
            this.btnYeni.TabIndex = 0;
            this.btnYeni.Text = "Yeni";
            this.btnYeni.UseVisualStyleBackColor = true;
            this.btnYeni.Click += new System.EventHandler(this.btnYeni_Click);
            // 
            // btnAc
            // 
            this.btnAc.Location = new System.Drawing.Point(92, 3);
            this.btnAc.Name = "btnAc";
            this.btnAc.Size = new System.Drawing.Size(83, 30);
            this.btnAc.TabIndex = 1;
            this.btnAc.Text = "Aç";
            this.btnAc.UseVisualStyleBackColor = true;
            this.btnAc.Click += new System.EventHandler(this.btnAc_Click);
            // 
            // btnDosyaEkle
            // 
            this.btnDosyaEkle.Location = new System.Drawing.Point(181, 3);
            this.btnDosyaEkle.Name = "btnDosyaEkle";
            this.btnDosyaEkle.Size = new System.Drawing.Size(90, 30);
            this.btnDosyaEkle.TabIndex = 2;
            this.btnDosyaEkle.Text = "Dosya Ekle";
            this.btnDosyaEkle.UseVisualStyleBackColor = true;
            this.btnDosyaEkle.Click += new System.EventHandler(this.btnDosyaEkle_Click);
            // 
            // btnYenile
            // 
            this.btnYenile.Location = new System.Drawing.Point(3, 39);
            this.btnYenile.Name = "btnYenile";
            this.btnYenile.Size = new System.Drawing.Size(83, 30);
            this.btnYenile.TabIndex = 3;
            this.btnYenile.Text = "Yenile";
            this.btnYenile.UseVisualStyleBackColor = true;
            this.btnYenile.Click += new System.EventHandler(this.btnYenile_Click);
            // 
            // btnKlasorAc
            // 
            this.btnKlasorAc.Location = new System.Drawing.Point(92, 39);
            this.btnKlasorAc.Name = "btnKlasorAc";
            this.btnKlasorAc.Size = new System.Drawing.Size(90, 30);
            this.btnKlasorAc.TabIndex = 4;
            this.btnKlasorAc.Text = "Klasör Aç";
            this.btnKlasorAc.UseVisualStyleBackColor = true;
            this.btnKlasorAc.Click += new System.EventHandler(this.btnKlasorAc_Click);
            // 
            // btnSil
            // 
            this.btnSil.Location = new System.Drawing.Point(188, 39);
            this.btnSil.Name = "btnSil";
            this.btnSil.Size = new System.Drawing.Size(83, 30);
            this.btnSil.TabIndex = 5;
            this.btnSil.Text = "Sil";
            this.btnSil.UseVisualStyleBackColor = true;
            // 
            // btnKopyaOlustur
            // 
            this.btnKopyaOlustur.Location = new System.Drawing.Point(3, 75);
            this.btnKopyaOlustur.Name = "btnKopyaOlustur";
            this.btnKopyaOlustur.Size = new System.Drawing.Size(179, 30);
            this.btnKopyaOlustur.TabIndex = 6;
            this.btnKopyaOlustur.Text = "Kopyasını Oluştur";
            this.btnKopyaOlustur.UseVisualStyleBackColor = true;
            // 
            // lstSablonlar
            // 
            this.lstSablonlar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSablonlar.FormattingEnabled = true;
            this.lstSablonlar.Location = new System.Drawing.Point(12, 71);
            this.lstSablonlar.Name = "lstSablonlar";
            this.lstSablonlar.Size = new System.Drawing.Size(276, 506);
            this.lstSablonlar.TabIndex = 2;
            this.lstSablonlar.DoubleClick += new System.EventHandler(this.lstSablonlar_DoubleClick);
            // 
            // txtSablonAra
            // 
            this.txtSablonAra.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtSablonAra.Location = new System.Drawing.Point(12, 49);
            this.txtSablonAra.Name = "txtSablonAra";
            this.txtSablonAra.Size = new System.Drawing.Size(276, 20);
            this.txtSablonAra.TabIndex = 1;
            // 
            // lblSablonlar
            // 
            this.lblSablonlar.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSablonlar.Location = new System.Drawing.Point(12, 12);
            this.lblSablonlar.Name = "lblSablonlar";
            this.lblSablonlar.Size = new System.Drawing.Size(276, 37);
            this.lblSablonlar.TabIndex = 0;
            this.lblSablonlar.Text = "Şablonlar";
            this.lblSablonlar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitEditorRight
            // 
            this.splitEditorRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitEditorRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitEditorRight.Location = new System.Drawing.Point(0, 0);
            this.splitEditorRight.Name = "splitEditorRight";
            // 
            // splitEditorRight.Panel1
            // 
            this.splitEditorRight.Panel1.Controls.Add(this.pnlEditorWrap);
            // 
            // splitEditorRight.Panel2
            // 
            this.splitEditorRight.Panel2.Controls.Add(this.pnlRightCard);
            this.splitEditorRight.Size = new System.Drawing.Size(956, 589);
            this.splitEditorRight.SplitterDistance = 736;
            this.splitEditorRight.TabIndex = 0;
            // 
            // pnlEditorWrap
            // 
            this.pnlEditorWrap.Controls.Add(this.pnlEditorCard);
            this.pnlEditorWrap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditorWrap.Location = new System.Drawing.Point(0, 0);
            this.pnlEditorWrap.Name = "pnlEditorWrap";
            this.pnlEditorWrap.Padding = new System.Windows.Forms.Padding(10);
            this.pnlEditorWrap.Size = new System.Drawing.Size(736, 589);
            this.pnlEditorWrap.TabIndex = 0;
            // 
            // pnlEditorCard
            // 
            this.pnlEditorCard.Controls.Add(this.rtbEditor);
            this.pnlEditorCard.Controls.Add(this.tsFormat);
            this.pnlEditorCard.Controls.Add(this.lblAktifDosya);
            this.pnlEditorCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditorCard.Location = new System.Drawing.Point(10, 10);
            this.pnlEditorCard.Name = "pnlEditorCard";
            this.pnlEditorCard.Padding = new System.Windows.Forms.Padding(12);
            this.pnlEditorCard.Size = new System.Drawing.Size(716, 569);
            this.pnlEditorCard.TabIndex = 0;
            // 
            // rtbEditor
            // 
            this.rtbEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbEditor.Location = new System.Drawing.Point(12, 63);
            this.rtbEditor.Name = "rtbEditor";
            this.rtbEditor.Size = new System.Drawing.Size(692, 494);
            this.rtbEditor.TabIndex = 2;
            this.rtbEditor.Text = "";
            this.rtbEditor.SelectionChanged += new System.EventHandler(this.RtbEditor_SelectionChanged);
            // 
            // tsFormat
            // 
            this.tsFormat.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmbFont,
            this.cmbFontSize,
            this.btnBold,
            this.btnItalic,
            this.btnUnderline,
            this.btnAlignLeft,
            this.btnAlignCenter,
            this.btnAlignRight,
            this.btnBullet});
            this.tsFormat.Location = new System.Drawing.Point(12, 38);
            this.tsFormat.Name = "tsFormat";
            this.tsFormat.Size = new System.Drawing.Size(692, 25);
            this.tsFormat.TabIndex = 1;
            // 
            // cmbFont
            // 
            this.cmbFont.AutoSize = false;
            this.cmbFont.Name = "cmbFont";
            this.cmbFont.Size = new System.Drawing.Size(160, 23);
            this.cmbFont.SelectedIndexChanged += new System.EventHandler(this.cmbFont_SelectedIndexChanged);
            // 
            // cmbFontSize
            // 
            this.cmbFontSize.AutoSize = false;
            this.cmbFontSize.Name = "cmbFontSize";
            this.cmbFontSize.Size = new System.Drawing.Size(75, 23);
            this.cmbFontSize.SelectedIndexChanged += new System.EventHandler(this.cmbFontSize_SelectedIndexChanged);
            // 
            // btnBold
            // 
            this.btnBold.CheckOnClick = true;
            this.btnBold.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnBold.Name = "btnBold";
            this.btnBold.Size = new System.Drawing.Size(23, 22);
            this.btnBold.Text = "B";
            this.btnBold.Click += new System.EventHandler(this.btnBold_Click);
            // 
            // btnItalic
            // 
            this.btnItalic.CheckOnClick = true;
            this.btnItalic.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnItalic.Name = "btnItalic";
            this.btnItalic.Size = new System.Drawing.Size(23, 22);
            this.btnItalic.Text = "I";
            this.btnItalic.Click += new System.EventHandler(this.btnItalic_Click);
            // 
            // btnUnderline
            // 
            this.btnUnderline.CheckOnClick = true;
            this.btnUnderline.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnUnderline.Name = "btnUnderline";
            this.btnUnderline.Size = new System.Drawing.Size(23, 22);
            this.btnUnderline.Text = "U";
            this.btnUnderline.Click += new System.EventHandler(this.btnUnderline_Click);
            // 
            // btnAlignLeft
            // 
            this.btnAlignLeft.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAlignLeft.Name = "btnAlignLeft";
            this.btnAlignLeft.Size = new System.Drawing.Size(23, 22);
            this.btnAlignLeft.Text = "L";
            this.btnAlignLeft.Click += new System.EventHandler(this.btnAlignLeft_Click);
            // 
            // btnAlignCenter
            // 
            this.btnAlignCenter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAlignCenter.Name = "btnAlignCenter";
            this.btnAlignCenter.Size = new System.Drawing.Size(23, 22);
            this.btnAlignCenter.Text = "C";
            this.btnAlignCenter.Click += new System.EventHandler(this.btnAlignCenter_Click);
            // 
            // btnAlignRight
            // 
            this.btnAlignRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAlignRight.Name = "btnAlignRight";
            this.btnAlignRight.Size = new System.Drawing.Size(23, 22);
            this.btnAlignRight.Text = "R";
            this.btnAlignRight.Click += new System.EventHandler(this.btnAlignRight_Click);
            // 
            // btnBullet
            // 
            this.btnBullet.CheckOnClick = true;
            this.btnBullet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnBullet.Name = "btnBullet";
            this.btnBullet.Size = new System.Drawing.Size(23, 22);
            this.btnBullet.Text = "•";
            this.btnBullet.Click += new System.EventHandler(this.btnBullet_Click);
            // 
            // lblAktifDosya
            // 
            this.lblAktifDosya.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAktifDosya.Location = new System.Drawing.Point(12, 12);
            this.lblAktifDosya.Name = "lblAktifDosya";
            this.lblAktifDosya.Size = new System.Drawing.Size(692, 26);
            this.lblAktifDosya.TabIndex = 0;
            this.lblAktifDosya.Text = "Aktif: -";
            this.lblAktifDosya.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pnlRightCard
            // 
            this.pnlRightCard.Controls.Add(this.flowRightActions);
            this.pnlRightCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRightCard.Location = new System.Drawing.Point(0, 0);
            this.pnlRightCard.Name = "pnlRightCard";
            this.pnlRightCard.Padding = new System.Windows.Forms.Padding(12);
            this.pnlRightCard.Size = new System.Drawing.Size(216, 589);
            this.pnlRightCard.TabIndex = 0;
            // 
            // flowRightActions
            // 
            this.flowRightActions.Controls.Add(this.btnKaydet);
            this.flowRightActions.Controls.Add(this.btnOnizleme);
            this.flowRightActions.Controls.Add(this.btnYazdir);
            this.flowRightActions.Controls.Add(this.btnWordIleAc);
            this.flowRightActions.Controls.Add(this.btnSablonBilgisi);
            this.flowRightActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowRightActions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowRightActions.Location = new System.Drawing.Point(12, 12);
            this.flowRightActions.Name = "flowRightActions";
            this.flowRightActions.Size = new System.Drawing.Size(192, 565);
            this.flowRightActions.TabIndex = 0;
            this.flowRightActions.WrapContents = false;
            // 
            // btnKaydet
            // 
            this.btnKaydet.Location = new System.Drawing.Point(3, 3);
            this.btnKaydet.Name = "btnKaydet";
            this.btnKaydet.Size = new System.Drawing.Size(185, 34);
            this.btnKaydet.TabIndex = 0;
            this.btnKaydet.Text = "Kaydet";
            this.btnKaydet.UseVisualStyleBackColor = true;
            this.btnKaydet.Click += new System.EventHandler(this.btnKaydet_Click);
            // 
            // btnOnizleme
            // 
            this.btnOnizleme.Location = new System.Drawing.Point(3, 43);
            this.btnOnizleme.Name = "btnOnizleme";
            this.btnOnizleme.Size = new System.Drawing.Size(185, 34);
            this.btnOnizleme.TabIndex = 1;
            this.btnOnizleme.Text = "Önizleme";
            this.btnOnizleme.UseVisualStyleBackColor = true;
            this.btnOnizleme.Click += new System.EventHandler(this.btnOnizleme_Click);
            // 
            // btnYazdir
            // 
            this.btnYazdir.Location = new System.Drawing.Point(3, 83);
            this.btnYazdir.Name = "btnYazdir";
            this.btnYazdir.Size = new System.Drawing.Size(185, 34);
            this.btnYazdir.TabIndex = 2;
            this.btnYazdir.Text = "Yazdır";
            this.btnYazdir.UseVisualStyleBackColor = true;
            this.btnYazdir.Click += new System.EventHandler(this.btnYazdir_Click);
            // 
            // btnWordIleAc
            // 
            this.btnWordIleAc.Location = new System.Drawing.Point(3, 123);
            this.btnWordIleAc.Name = "btnWordIleAc";
            this.btnWordIleAc.Size = new System.Drawing.Size(185, 34);
            this.btnWordIleAc.TabIndex = 3;
            this.btnWordIleAc.Text = "Word ile Aç";
            this.btnWordIleAc.UseVisualStyleBackColor = true;
            // 
            // btnSablonBilgisi
            // 
            this.btnSablonBilgisi.Location = new System.Drawing.Point(3, 163);
            this.btnSablonBilgisi.Name = "btnSablonBilgisi";
            this.btnSablonBilgisi.Size = new System.Drawing.Size(185, 34);
            this.btnSablonBilgisi.TabIndex = 4;
            this.btnSablonBilgisi.Text = "Şablon Bilgisi";
            this.btnSablonBilgisi.UseVisualStyleBackColor = true;
            // 
            // statusMain
            // 
            this.statusMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblDurumDosya,
            this.lblDurumAyrac1,
            this.lblDurumSayi,
            this.lblDurumAyrac2,
            this.lblDurumMesaj});
            this.statusMain.Location = new System.Drawing.Point(0, 677);
            this.statusMain.Name = "statusMain";
            this.statusMain.Size = new System.Drawing.Size(1260, 22);
            this.statusMain.TabIndex = 2;
            // 
            // lblDurumDosya
            // 
            this.lblDurumDosya.Name = "lblDurumDosya";
            this.lblDurumDosya.Size = new System.Drawing.Size(54, 17);
            this.lblDurumDosya.Text = "Dosya: -";
            // 
            // lblDurumAyrac1
            // 
            this.lblDurumAyrac1.Name = "lblDurumAyrac1";
            this.lblDurumAyrac1.Size = new System.Drawing.Size(14, 17);
            this.lblDurumAyrac1.Text = "|";
            // 
            // lblDurumSayi
            // 
            this.lblDurumSayi.Name = "lblDurumSayi";
            this.lblDurumSayi.Size = new System.Drawing.Size(89, 17);
            this.lblDurumSayi.Text = "Şablon Sayısı: 0";
            // 
            // lblDurumAyrac2
            // 
            this.lblDurumAyrac2.Name = "lblDurumAyrac2";
            this.lblDurumAyrac2.Size = new System.Drawing.Size(14, 17);
            this.lblDurumAyrac2.Text = "|";
            // 
            // lblDurumMesaj
            // 
            this.lblDurumMesaj.Name = "lblDurumMesaj";
            this.lblDurumMesaj.Size = new System.Drawing.Size(33, 17);
            this.lblDurumMesaj.Text = "Hazır";
            // 
            // docYazdir
            // 
            this.docYazdir.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.docYazdir_PrintPage);
            // 
            // UstYazilarForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1260, 699);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.statusMain);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "UstYazilarForm";
            this.Text = "UstYazilarForm";
            this.pnlHeader.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.pnlLeftCard.ResumeLayout(false);
            this.pnlLeftCard.PerformLayout();
            this.pnlLeftButtonsBottom.ResumeLayout(false);
            this.splitEditorRight.Panel1.ResumeLayout(false);
            this.splitEditorRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitEditorRight)).EndInit();
            this.splitEditorRight.ResumeLayout(false);
            this.pnlEditorWrap.ResumeLayout(false);
            this.pnlEditorCard.ResumeLayout(false);
            this.pnlEditorCard.PerformLayout();
            this.tsFormat.ResumeLayout(false);
            this.tsFormat.PerformLayout();
            this.pnlRightCard.ResumeLayout(false);
            this.flowRightActions.ResumeLayout(false);
            this.statusMain.ResumeLayout(false);
            this.statusMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lblHeaderSub;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Panel pnlLeftCard;
        private System.Windows.Forms.FlowLayoutPanel pnlLeftButtonsBottom;
        private System.Windows.Forms.Button btnYeni;
        private System.Windows.Forms.Button btnAc;
        private System.Windows.Forms.Button btnDosyaEkle;
        private System.Windows.Forms.Button btnYenile;
        private System.Windows.Forms.Button btnKlasorAc;
        private System.Windows.Forms.Button btnSil;
        private System.Windows.Forms.Button btnKopyaOlustur;
        private System.Windows.Forms.ListBox lstSablonlar;
        private System.Windows.Forms.TextBox txtSablonAra;
        private System.Windows.Forms.Label lblSablonlar;
        private System.Windows.Forms.SplitContainer splitEditorRight;
        private System.Windows.Forms.Panel pnlEditorWrap;
        private System.Windows.Forms.Panel pnlEditorCard;
        private System.Windows.Forms.RichTextBox rtbEditor;
        private System.Windows.Forms.ToolStrip tsFormat;
        private System.Windows.Forms.ToolStripComboBox cmbFont;
        private System.Windows.Forms.ToolStripComboBox cmbFontSize;
        private System.Windows.Forms.ToolStripButton btnBold;
        private System.Windows.Forms.ToolStripButton btnItalic;
        private System.Windows.Forms.ToolStripButton btnUnderline;
        private System.Windows.Forms.ToolStripButton btnAlignLeft;
        private System.Windows.Forms.ToolStripButton btnAlignCenter;
        private System.Windows.Forms.ToolStripButton btnAlignRight;
        private System.Windows.Forms.ToolStripButton btnBullet;
        private System.Windows.Forms.Label lblAktifDosya;
        private System.Windows.Forms.Panel pnlRightCard;
        private System.Windows.Forms.FlowLayoutPanel flowRightActions;
        private System.Windows.Forms.Button btnKaydet;
        private System.Windows.Forms.Button btnOnizleme;
        private System.Windows.Forms.Button btnYazdir;
        private System.Windows.Forms.Button btnWordIleAc;
        private System.Windows.Forms.Button btnSablonBilgisi;
        private System.Windows.Forms.StatusStrip statusMain;
        private System.Windows.Forms.ToolStripStatusLabel lblDurumDosya;
        private System.Windows.Forms.ToolStripStatusLabel lblDurumAyrac1;
        private System.Windows.Forms.ToolStripStatusLabel lblDurumSayi;
        private System.Windows.Forms.ToolStripStatusLabel lblDurumAyrac2;
        private System.Windows.Forms.ToolStripStatusLabel lblDurumMesaj;
        private System.Drawing.Printing.PrintDocument docYazdir;
    }
}
