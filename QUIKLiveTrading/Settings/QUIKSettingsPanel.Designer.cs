namespace WLDSolutions.QUIKLiveTrading.Settings
{
    partial class QUIKSettingsPanel
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
            this.gbSettings = new System.Windows.Forms.GroupBox();
            this.btnSettings = new System.Windows.Forms.Button();
            this.cbSettings = new System.Windows.Forms.ComboBox();
            this.gbSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbSettings
            // 
            this.gbSettings.Controls.Add(this.btnSettings);
            this.gbSettings.Controls.Add(this.cbSettings);
            this.gbSettings.Location = new System.Drawing.Point(3, 2);
            this.gbSettings.Name = "gbSettings";
            this.gbSettings.Size = new System.Drawing.Size(405, 57);
            this.gbSettings.TabIndex = 4;
            this.gbSettings.TabStop = false;
            this.gbSettings.Text = "Настройки";
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(315, 21);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(75, 23);
            this.btnSettings.TabIndex = 1;
            this.btnSettings.Text = "Перейти";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // cbSettings
            // 
            this.cbSettings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSettings.FormattingEnabled = true;
            this.cbSettings.Items.AddRange(new object[] {
            "Общие настройки и управление аккаунтами",
            "Настроки импорта данных"});
            this.cbSettings.Location = new System.Drawing.Point(15, 22);
            this.cbSettings.Name = "cbSettings";
            this.cbSettings.Size = new System.Drawing.Size(294, 21);
            this.cbSettings.TabIndex = 0;
            // 
            // QUIKSettingsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbSettings);
            this.Name = "QUIKSettingsPanel";
            this.gbSettings.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbSettings;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.ComboBox cbSettings;
    }
}
