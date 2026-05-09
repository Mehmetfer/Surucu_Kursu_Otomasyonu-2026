namespace Kolera_Mtsk
{
    partial class Frm_Yardim
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
            this.TxtLisansKodu = new System.Windows.Forms.TextBox();
            this.BtnOnayla = new System.Windows.Forms.Button();
            this.BtnIptal = new System.Windows.Forms.Button();
            this.LblBaslik = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TxtLisansKodu
            // 
            this.TxtLisansKodu.Location = new System.Drawing.Point(119, 73);
            this.TxtLisansKodu.Name = "TxtLisansKodu";
            this.TxtLisansKodu.Size = new System.Drawing.Size(182, 20);
            this.TxtLisansKodu.TabIndex = 0;
            // 
            // BtnOnayla
            // 
            this.BtnOnayla.Location = new System.Drawing.Point(82, 175);
            this.BtnOnayla.Name = "BtnOnayla";
            this.BtnOnayla.Size = new System.Drawing.Size(75, 23);
            this.BtnOnayla.TabIndex = 1;
            this.BtnOnayla.Text = "button1";
            this.BtnOnayla.UseVisualStyleBackColor = true;
            this.BtnOnayla.Click += new System.EventHandler(this.BtnOnayla_Click);
            // 
            // BtnIptal
            // 
            this.BtnIptal.Location = new System.Drawing.Point(241, 175);
            this.BtnIptal.Name = "BtnIptal";
            this.BtnIptal.Size = new System.Drawing.Size(75, 23);
            this.BtnIptal.TabIndex = 1;
            this.BtnIptal.Text = "button1";
            this.BtnIptal.UseVisualStyleBackColor = true;
            this.BtnIptal.Click += new System.EventHandler(this.BtnIptal_Click);
            // 
            // LblBaslik
            // 
            this.LblBaslik.AutoSize = true;
            this.LblBaslik.Location = new System.Drawing.Point(119, 119);
            this.LblBaslik.Name = "LblBaslik";
            this.LblBaslik.Size = new System.Drawing.Size(35, 13);
            this.LblBaslik.TabIndex = 2;
            this.LblBaslik.Text = "label1";
            // 
            // Frm_Yardim
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 300);
            this.Controls.Add(this.LblBaslik);
            this.Controls.Add(this.BtnIptal);
            this.Controls.Add(this.BtnOnayla);
            this.Controls.Add(this.TxtLisansKodu);
            this.Name = "Frm_Yardim";
            this.Text = "Frm_Yardim";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TxtLisansKodu;
        private System.Windows.Forms.Button BtnOnayla;
        private System.Windows.Forms.Button BtnIptal;
        private System.Windows.Forms.Label LblBaslik;
    }
}