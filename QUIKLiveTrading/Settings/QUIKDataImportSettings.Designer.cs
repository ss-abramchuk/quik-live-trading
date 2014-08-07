namespace WLDSolutions.QUIKLiveTrading.Settings
{
    partial class QUIKDataImportSettings
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
            this.dgvSymbols = new System.Windows.Forms.DataGridView();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.lblSymbolTable = new System.Windows.Forms.Label();
            this.bsSymbolDescription = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSymbols)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsSymbolDescription)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvSymbols
            // 
            this.dgvSymbols.AllowUserToResizeColumns = false;
            this.dgvSymbols.AllowUserToResizeRows = false;
            this.dgvSymbols.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSymbols.Location = new System.Drawing.Point(12, 25);
            this.dgvSymbols.Name = "dgvSymbols";
            this.dgvSymbols.RowHeadersVisible = false;
            this.dgvSymbols.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvSymbols.Size = new System.Drawing.Size(551, 171);
            this.dgvSymbols.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(488, 202);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(407, 202);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 12;
            this.btnOk.Text = "Применить";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // lblSymbolTable
            // 
            this.lblSymbolTable.AutoSize = true;
            this.lblSymbolTable.Location = new System.Drawing.Point(9, 9);
            this.lblSymbolTable.Name = "lblSymbolTable";
            this.lblSymbolTable.Size = new System.Drawing.Size(196, 13);
            this.lblSymbolTable.TabIndex = 14;
            this.lblSymbolTable.Text = "Таблица соответствия инструментов";
            // 
            // QUIKDataImportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 233);
            this.Controls.Add(this.lblSymbolTable);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.dgvSymbols);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "QUIKDataImportSettings";
            this.Text = "Настроки импорта данных";
            ((System.ComponentModel.ISupportInitialize)(this.dgvSymbols)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bsSymbolDescription)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvSymbols;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label lblSymbolTable;
        private System.Windows.Forms.BindingSource bsSymbolDescription;
    }
}