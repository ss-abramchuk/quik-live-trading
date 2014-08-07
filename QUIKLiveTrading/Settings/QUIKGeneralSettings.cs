using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WealthLab;

using WLDSolutions.QUIKLiveTrading.Helpers;
using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.LiveTradingManager.Helpers;

namespace WLDSolutions.QUIKLiveTrading.Settings
{
    public partial class QUIKGeneralSettings : Form
    {
        private ILTSettingsProvider _settingsProvider;

        private List<QUIKAccount> _accounts;

        public QUIKGeneralSettings(ILTSettingsProvider settingsProvider)
        {
            _settingsProvider = settingsProvider;

            InitializeComponent();
            
            tbFolder.Text = _settingsProvider.GetParameter("QUIKPath", @"C:\Program Files\QUIK");
            chbBrokerAdapterEnable.Checked = _settingsProvider.GetParameter("BrokerProviderActive", false);

            string tif = _settingsProvider.GetParameter("TIF", "TODAY");

            if (tif.Contains("TODAY"))
                rbTifToday.Checked = true;
            else
                rbTifGTC.Checked = true;

            chbSlippageEnable.Checked = _settingsProvider.GetParameter("EnableSlippage", false);
            numStocksSlippage.Value = (decimal)_settingsProvider.GetParameter("SlippageUnits", 0.0);
            numFuturesSlippage.Value = (decimal)_settingsProvider.GetParameter("SlippageTicks", 1);

            _accounts = _settingsProvider.GetObject("Accounts", typeof(List<QUIKAccount>)) as List<QUIKAccount> ?? new List<QUIKAccount>();

            dgvAccounts.AutoGenerateColumns = false;

            bsAccounts.DataSource = _accounts;
            dgvAccounts.DataSource = bsAccounts;

            DataGridViewTextBoxColumn accountNumber = new DataGridViewTextBoxColumn();
            accountNumber.HeaderText = "Торговый счет";
            accountNumber.Width = 140;
            accountNumber.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            accountNumber.DataPropertyName = "TradeAccount";

            DataGridViewTextBoxColumn clientCode = new DataGridViewTextBoxColumn();
            clientCode.HeaderText = "Код клиента";
            clientCode.Width = 140;
            clientCode.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            clientCode.DataPropertyName = "ClientCode";

            dgvAccounts.Columns.Add(accountNumber);
            dgvAccounts.Columns.Add(clientCode);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            _settingsProvider.SetParameter("QUIKPath", tbFolder.Text);
            _settingsProvider.SetParameter("BrokerProviderActive", chbBrokerAdapterEnable.Checked);

            string tif = string.Empty;
            if (rbTifGTC.Checked)
                tif = "GTC";
            else
                tif = "TODAY";

            _settingsProvider.SetParameter("TIF", tif);

            _settingsProvider.SetParameter("EnableSlippage", chbSlippageEnable.Checked);
            _settingsProvider.SetParameter("SlippageUnits", (double)numStocksSlippage.Value);
            _settingsProvider.SetParameter("SlippageTicks", (double)numFuturesSlippage.Value);

            foreach (QUIKAccount account in _accounts)
            {
                if (string.IsNullOrWhiteSpace(account.ProviderName) || account.AccountNumber != account.FullAccountNumber)
                {
                    account.ProviderName = "QUIKLiveTrading";
                    account.IsPaperAccount = false;
                    account.AccountNumber = account.FullAccountNumber;
                    account.AvailableCash = 0;
                    account.BuyingPower = 0;
                    account.AccountValue = 0;
                    account.AccountValueTimeStamp = DateTime.Now;
                }
            }

            _settingsProvider.SaveObject("Accounts", _accounts);

            this.Close();
        }

        private void btnChangeFolder_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                tbFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
