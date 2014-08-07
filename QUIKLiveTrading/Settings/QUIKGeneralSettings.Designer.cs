namespace WLDSolutions.QUIKLiveTrading.Settings
{
    partial class QUIKGeneralSettings
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
            this.components = new System.ComponentModel.Container();
            this.gbSlippageSettings = new System.Windows.Forms.GroupBox();
            this.numFuturesSlippage = new System.Windows.Forms.NumericUpDown();
            this.lblFuturesSlippage = new System.Windows.Forms.Label();
            this.lblStocksSlippage = new System.Windows.Forms.Label();
            this.numStocksSlippage = new System.Windows.Forms.NumericUpDown();
            this.chbSlippageEnable = new System.Windows.Forms.CheckBox();
            this.gbStopOrderTIF = new System.Windows.Forms.GroupBox();
            this.rbTifGTC = new System.Windows.Forms.RadioButton();
            this.rbTifToday = new System.Windows.Forms.RadioButton();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chbBrokerAdapterEnable = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.gbAccounts = new System.Windows.Forms.GroupBox();
            this.dgvAccounts = new System.Windows.Forms.DataGridView();
            this.gbGeneralSettings = new System.Windows.Forms.GroupBox();
            this.tbFolder = new System.Windows.Forms.TextBox();
            this.btnChangeFolder = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.bsAccounts = new System.Windows.Forms.BindingSource(this.components);
            this.gbSlippageSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFuturesSlippage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStocksSlippage)).BeginInit();
            this.gbStopOrderTIF.SuspendLayout();
            this.gbAccounts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAccounts)).BeginInit();
            this.gbGeneralSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsAccounts)).BeginInit();
            this.SuspendLayout();
            // 
            // gbSlippageSettings
            // 
            this.gbSlippageSettings.Controls.Add(this.numFuturesSlippage);
            this.gbSlippageSettings.Controls.Add(this.lblFuturesSlippage);
            this.gbSlippageSettings.Controls.Add(this.lblStocksSlippage);
            this.gbSlippageSettings.Controls.Add(this.numStocksSlippage);
            this.gbSlippageSettings.Controls.Add(this.chbSlippageEnable);
            this.gbSlippageSettings.Location = new System.Drawing.Point(347, 313);
            this.gbSlippageSettings.Name = "gbSlippageSettings";
            this.gbSlippageSettings.Size = new System.Drawing.Size(306, 98);
            this.gbSlippageSettings.TabIndex = 13;
            this.gbSlippageSettings.TabStop = false;
            this.gbSlippageSettings.Text = "Настройки проскальзывания";
            // 
            // numFuturesSlippage
            // 
            this.numFuturesSlippage.Location = new System.Drawing.Point(6, 69);
            this.numFuturesSlippage.Name = "numFuturesSlippage";
            this.numFuturesSlippage.Size = new System.Drawing.Size(71, 20);
            this.numFuturesSlippage.TabIndex = 4;
            // 
            // lblFuturesSlippage
            // 
            this.lblFuturesSlippage.AutoSize = true;
            this.lblFuturesSlippage.Location = new System.Drawing.Point(83, 71);
            this.lblFuturesSlippage.Name = "lblFuturesSlippage";
            this.lblFuturesSlippage.Size = new System.Drawing.Size(214, 13);
            this.lblFuturesSlippage.TabIndex = 5;
            this.lblFuturesSlippage.Text = "Проскальзывание для фьючерсов (тики)";
            // 
            // lblStocksSlippage
            // 
            this.lblStocksSlippage.AutoSize = true;
            this.lblStocksSlippage.Location = new System.Drawing.Point(83, 45);
            this.lblStocksSlippage.Name = "lblStocksSlippage";
            this.lblStocksSlippage.Size = new System.Drawing.Size(172, 13);
            this.lblStocksSlippage.TabIndex = 4;
            this.lblStocksSlippage.Text = "Проскальзывание для акций (%)";
            // 
            // numStocksSlippage
            // 
            this.numStocksSlippage.DecimalPlaces = 2;
            this.numStocksSlippage.Increment = new decimal(new int[] {
            5,
            0,
            0,
            131072});
            this.numStocksSlippage.Location = new System.Drawing.Point(6, 43);
            this.numStocksSlippage.Name = "numStocksSlippage";
            this.numStocksSlippage.Size = new System.Drawing.Size(71, 20);
            this.numStocksSlippage.TabIndex = 3;
            // 
            // chbSlippageEnable
            // 
            this.chbSlippageEnable.AutoSize = true;
            this.chbSlippageEnable.Location = new System.Drawing.Point(6, 20);
            this.chbSlippageEnable.Name = "chbSlippageEnable";
            this.chbSlippageEnable.Size = new System.Drawing.Size(256, 17);
            this.chbSlippageEnable.TabIndex = 2;
            this.chbSlippageEnable.Text = "Включить проскальзывание для стоп заявок";
            this.chbSlippageEnable.UseVisualStyleBackColor = true;
            // 
            // gbStopOrderTIF
            // 
            this.gbStopOrderTIF.Controls.Add(this.rbTifGTC);
            this.gbStopOrderTIF.Controls.Add(this.rbTifToday);
            this.gbStopOrderTIF.Location = new System.Drawing.Point(12, 313);
            this.gbStopOrderTIF.Name = "gbStopOrderTIF";
            this.gbStopOrderTIF.Size = new System.Drawing.Size(328, 98);
            this.gbStopOrderTIF.TabIndex = 12;
            this.gbStopOrderTIF.TabStop = false;
            this.gbStopOrderTIF.Text = "Срок действия заявок";
            // 
            // rbTifGTC
            // 
            this.rbTifGTC.AutoSize = true;
            this.rbTifGTC.Location = new System.Drawing.Point(9, 42);
            this.rbTifGTC.Name = "rbTifGTC";
            this.rbTifGTC.Size = new System.Drawing.Size(161, 17);
            this.rbTifGTC.TabIndex = 1;
            this.rbTifGTC.Text = "Действительна до отмены";
            this.rbTifGTC.UseVisualStyleBackColor = true;
            // 
            // rbTifToday
            // 
            this.rbTifToday.AutoSize = true;
            this.rbTifToday.Location = new System.Drawing.Point(9, 19);
            this.rbTifToday.Name = "rbTifToday";
            this.rbTifToday.Size = new System.Drawing.Size(309, 17);
            this.rbTifToday.TabIndex = 0;
            this.rbTifToday.Text = "Действительна до окончания текущей торговой сессии";
            this.rbTifToday.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(578, 417);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 11;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chbBrokerAdapterEnable
            // 
            this.chbBrokerAdapterEnable.AutoSize = true;
            this.chbBrokerAdapterEnable.Checked = true;
            this.chbBrokerAdapterEnable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chbBrokerAdapterEnable.Location = new System.Drawing.Point(12, 421);
            this.chbBrokerAdapterEnable.Name = "chbBrokerAdapterEnable";
            this.chbBrokerAdapterEnable.Size = new System.Drawing.Size(308, 17);
            this.chbBrokerAdapterEnable.TabIndex = 8;
            this.chbBrokerAdapterEnable.Text = "Включить брокер адаптер (требуется перезапуск WLD)";
            this.chbBrokerAdapterEnable.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(497, 417);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 9;
            this.btnOk.Text = "Применить";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // gbAccounts
            // 
            this.gbAccounts.Controls.Add(this.dgvAccounts);
            this.gbAccounts.Location = new System.Drawing.Point(12, 78);
            this.gbAccounts.Name = "gbAccounts";
            this.gbAccounts.Size = new System.Drawing.Size(641, 229);
            this.gbAccounts.TabIndex = 10;
            this.gbAccounts.TabStop = false;
            this.gbAccounts.Text = "Управление аккаунтами";
            // 
            // dgvAccounts
            // 
            this.dgvAccounts.AllowUserToResizeColumns = false;
            this.dgvAccounts.AllowUserToResizeRows = false;
            this.dgvAccounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAccounts.Location = new System.Drawing.Point(9, 19);
            this.dgvAccounts.Name = "dgvAccounts";
            this.dgvAccounts.RowHeadersVisible = false;
            this.dgvAccounts.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvAccounts.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dgvAccounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAccounts.Size = new System.Drawing.Size(623, 198);
            this.dgvAccounts.TabIndex = 0;
            // 
            // gbGeneralSettings
            // 
            this.gbGeneralSettings.Controls.Add(this.tbFolder);
            this.gbGeneralSettings.Controls.Add(this.btnChangeFolder);
            this.gbGeneralSettings.Location = new System.Drawing.Point(12, 12);
            this.gbGeneralSettings.Name = "gbGeneralSettings";
            this.gbGeneralSettings.Size = new System.Drawing.Size(641, 60);
            this.gbGeneralSettings.TabIndex = 7;
            this.gbGeneralSettings.TabStop = false;
            this.gbGeneralSettings.Text = "Расположение терминала QUIK";
            // 
            // tbFolder
            // 
            this.tbFolder.Location = new System.Drawing.Point(9, 23);
            this.tbFolder.Name = "tbFolder";
            this.tbFolder.Size = new System.Drawing.Size(542, 20);
            this.tbFolder.TabIndex = 0;
            // 
            // btnChangeFolder
            // 
            this.btnChangeFolder.Location = new System.Drawing.Point(557, 21);
            this.btnChangeFolder.Name = "btnChangeFolder";
            this.btnChangeFolder.Size = new System.Drawing.Size(75, 23);
            this.btnChangeFolder.TabIndex = 1;
            this.btnChangeFolder.Text = "Выбрать";
            this.btnChangeFolder.UseVisualStyleBackColor = true;
            this.btnChangeFolder.Click += new System.EventHandler(this.btnChangeFolder_Click);
            // 
            // QUIKGeneralSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 446);
            this.Controls.Add(this.gbSlippageSettings);
            this.Controls.Add(this.gbStopOrderTIF);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.chbBrokerAdapterEnable);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.gbAccounts);
            this.Controls.Add(this.gbGeneralSettings);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "QUIKGeneralSettings";
            this.Text = "Общие настройки и управление аккаунтами";
            this.gbSlippageSettings.ResumeLayout(false);
            this.gbSlippageSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numFuturesSlippage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numStocksSlippage)).EndInit();
            this.gbStopOrderTIF.ResumeLayout(false);
            this.gbStopOrderTIF.PerformLayout();
            this.gbAccounts.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAccounts)).EndInit();
            this.gbGeneralSettings.ResumeLayout(false);
            this.gbGeneralSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsAccounts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbSlippageSettings;
        private System.Windows.Forms.NumericUpDown numFuturesSlippage;
        private System.Windows.Forms.Label lblFuturesSlippage;
        private System.Windows.Forms.Label lblStocksSlippage;
        private System.Windows.Forms.NumericUpDown numStocksSlippage;
        private System.Windows.Forms.CheckBox chbSlippageEnable;
        private System.Windows.Forms.GroupBox gbStopOrderTIF;
        private System.Windows.Forms.RadioButton rbTifGTC;
        private System.Windows.Forms.RadioButton rbTifToday;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chbBrokerAdapterEnable;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.GroupBox gbAccounts;
        private System.Windows.Forms.DataGridView dgvAccounts;
        private System.Windows.Forms.GroupBox gbGeneralSettings;
        private System.Windows.Forms.TextBox tbFolder;
        private System.Windows.Forms.Button btnChangeFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.BindingSource bsAccounts;
    }
}