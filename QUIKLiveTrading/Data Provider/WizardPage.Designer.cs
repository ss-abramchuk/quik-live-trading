namespace WLDSolutions.QUIKLiveTrading.DataProvider
{
    partial class WizardPage
    {
        /// <summary> 
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardPage));
            this.cbTimeframes = new System.Windows.Forms.ComboBox();
            this.lblTimeFrame = new System.Windows.Forms.Label();
            this.cbIntervals = new System.Windows.Forms.ComboBox();
            this.rtbSymbols = new System.Windows.Forms.RichTextBox();
            this.gbInformation = new System.Windows.Forms.GroupBox();
            this.pbWarningImage = new System.Windows.Forms.PictureBox();
            this.rtbInformation = new System.Windows.Forms.RichTextBox();
            this.gbInformation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbWarningImage)).BeginInit();
            this.SuspendLayout();
            // 
            // cbTimeframes
            // 
            this.cbTimeframes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTimeframes.FormattingEnabled = true;
            this.cbTimeframes.Location = new System.Drawing.Point(81, 3);
            this.cbTimeframes.Name = "cbTimeframes";
            this.cbTimeframes.Size = new System.Drawing.Size(118, 21);
            this.cbTimeframes.TabIndex = 0;
            this.cbTimeframes.SelectedValueChanged += new System.EventHandler(this.cbTimeframes_SelectedValueChanged);
            // 
            // lblTimeFrame
            // 
            this.lblTimeFrame.AutoSize = true;
            this.lblTimeFrame.Location = new System.Drawing.Point(4, 6);
            this.lblTimeFrame.Name = "lblTimeFrame";
            this.lblTimeFrame.Size = new System.Drawing.Size(71, 13);
            this.lblTimeFrame.TabIndex = 1;
            this.lblTimeFrame.Text = "Таймфрейм:";
            // 
            // cbIntervals
            // 
            this.cbIntervals.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbIntervals.FormattingEnabled = true;
            this.cbIntervals.Location = new System.Drawing.Point(205, 3);
            this.cbIntervals.Name = "cbIntervals";
            this.cbIntervals.Size = new System.Drawing.Size(47, 21);
            this.cbIntervals.TabIndex = 2;
            // 
            // rtbSymbols
            // 
            this.rtbSymbols.Location = new System.Drawing.Point(7, 90);
            this.rtbSymbols.Name = "rtbSymbols";
            this.rtbSymbols.Size = new System.Drawing.Size(536, 267);
            this.rtbSymbols.TabIndex = 4;
            this.rtbSymbols.Text = "";
            // 
            // gbInformation
            // 
            this.gbInformation.Controls.Add(this.pbWarningImage);
            this.gbInformation.Controls.Add(this.rtbInformation);
            this.gbInformation.Location = new System.Drawing.Point(7, 25);
            this.gbInformation.Name = "gbInformation";
            this.gbInformation.Size = new System.Drawing.Size(536, 59);
            this.gbInformation.TabIndex = 5;
            this.gbInformation.TabStop = false;
            // 
            // pbWarningImage
            // 
            this.pbWarningImage.Location = new System.Drawing.Point(7, 10);
            this.pbWarningImage.Name = "pbWarningImage";
            this.pbWarningImage.Size = new System.Drawing.Size(45, 45);
            this.pbWarningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbWarningImage.TabIndex = 1;
            this.pbWarningImage.TabStop = false;
            // 
            // rtbInformation
            // 
            this.rtbInformation.BackColor = System.Drawing.SystemColors.Control;
            this.rtbInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbInformation.Enabled = false;
            this.rtbInformation.Location = new System.Drawing.Point(56, 13);
            this.rtbInformation.Name = "rtbInformation";
            this.rtbInformation.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbInformation.Size = new System.Drawing.Size(474, 43);
            this.rtbInformation.TabIndex = 0;
            this.rtbInformation.Text = resources.GetString("rtbInformation.Text");
            // 
            // WizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbInformation);
            this.Controls.Add(this.rtbSymbols);
            this.Controls.Add(this.cbTimeframes);
            this.Controls.Add(this.lblTimeFrame);
            this.Controls.Add(this.cbIntervals);
            this.Name = "WizardPage";
            this.Size = new System.Drawing.Size(550, 360);
            this.gbInformation.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbWarningImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbTimeframes;
        private System.Windows.Forms.Label lblTimeFrame;
        private System.Windows.Forms.RichTextBox rtbSymbols;
        private System.Windows.Forms.ComboBox cbIntervals;
        private System.Windows.Forms.GroupBox gbInformation;
        private System.Windows.Forms.PictureBox pbWarningImage;
        private System.Windows.Forms.RichTextBox rtbInformation;
    }
}
