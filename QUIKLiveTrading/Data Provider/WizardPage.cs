using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WealthLab;
using log4net;

using WLDSolutions.QUIKLiveTrading.Helpers;

namespace WLDSolutions.QUIKLiveTrading.DataProvider
{
    public partial class WizardPage : UserControl
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(WizardPage));

        private List<string> _timeframes;
        private List<int> _intervals;

        public WizardPage()
        {
            InitializeComponent();

            _timeframes = new List<string>() {"Ticks", "Minutes", "Days", "Weeks", "Months"};
            _intervals = new List<int>() { 1, 5, 10, 15, 30, 60 };

            cbTimeframes.DataSource = _timeframes;
            cbIntervals.DataSource = _intervals;

            pbWarningImage.Image = SystemIcons.Warning.ToBitmap();
        }

        #region Возвращение данных для создания нового набора данных

        public List<SymbolDescription> GetSymbolsDescription(ref int errorsCount)
        {
            string buffer = rtbSymbols.Text;
            string[] symbols = buffer.Split(new string[] { ",", "\r\n", " " }, StringSplitOptions.RemoveEmptyEntries);

            List<SymbolDescription> descriptions = new List<SymbolDescription>();

            foreach(string symbol in symbols)
            {
                SymbolDescription description = GetSymbolDescription(symbol);

                if (description != null)
                    descriptions.Add(description);
                else
                    errorsCount++;
            }

            return descriptions;
        }

        private SymbolDescription GetSymbolDescription(string symbol)
        {
            try
            {
                SymbolDescription description = new SymbolDescription();

                description.MarketCode = symbol.Substring(0, symbol.IndexOf("."));
                description.SymbolCode = symbol.Substring(symbol.IndexOf(".") + 1, symbol.Length - symbol.IndexOf(".") - 1);

                return description;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        public BarDataScale GetDataScale()
        {
            BarDataScale dataScale;

            switch (cbTimeframes.SelectedIndex)
            {
                case 0:
                    dataScale = new BarDataScale(BarScale.Tick, 1);
                    break;

                case 1:
                    dataScale = new BarDataScale(BarScale.Minute, (int)cbIntervals.SelectedItem);
                    break;

                case 2:
                    dataScale = new BarDataScale(BarScale.Daily, 0);
                    break;

                case 3:
                    dataScale = new BarDataScale(BarScale.Weekly, 0);
                    break;

                case 4:
                    dataScale = new BarDataScale(BarScale.Monthly, 0);
                    break;

                default:
                    dataScale = new BarDataScale();
                    break;
            }

            return dataScale;
        }

        #endregion

        #region Обработка изменения фильтров

        private void cbTimeframes_SelectedValueChanged(object sender, EventArgs e)
        {
            if (cbTimeframes.SelectedItem == (object)"Minutes")
                cbIntervals.Enabled = true;
            else
                cbIntervals.Enabled = false;
        }

        #endregion
    }
}
