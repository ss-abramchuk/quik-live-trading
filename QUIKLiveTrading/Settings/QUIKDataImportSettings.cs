using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.QUIKLiveTrading.Helpers;

namespace WLDSolutions.QUIKLiveTrading.Settings
{
    public partial class QUIKDataImportSettings : Form
    {
        ILTSettingsProvider _settingsProvider;

        List<SymbolDescription> _symbols;

        public QUIKDataImportSettings(ILTSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;

            InitializeComponent();

            _symbols = (List<SymbolDescription>)_settingsProvider.GetObject("ImportSymbols", typeof(List<SymbolDescription>)) ?? new List<SymbolDescription>();

            dgvSymbols.AutoGenerateColumns = false;

            bsSymbolDescription.DataSource = _symbols;
            dgvSymbols.DataSource = bsSymbolDescription;

            DataGridViewTextBoxColumn classCode = new DataGridViewTextBoxColumn();
            classCode.HeaderText = "Код класса";
            classCode.Width = 140;
            classCode.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            classCode.DataPropertyName = "MarketCode";

            DataGridViewTextBoxColumn symbolCode = new DataGridViewTextBoxColumn();
            symbolCode.HeaderText = "Код инструмента";
            symbolCode.Width = 140;
            symbolCode.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            symbolCode.DataPropertyName = "SymbolCode";

            DataGridViewTextBoxColumn exportName = new DataGridViewTextBoxColumn();
            exportName.HeaderText = "Обозначение инструмента";
            exportName.Width = 140;
            exportName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            exportName.DataPropertyName = "ExportName";

            dgvSymbols.Columns.Add(classCode);
            dgvSymbols.Columns.Add(symbolCode);
            dgvSymbols.Columns.Add(exportName);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _settingsProvider.SaveObject("ImportSymbols", _symbols);
        }
    }
}
