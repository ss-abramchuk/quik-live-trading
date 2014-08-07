using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.QUIKLiveTrading.Dispatcher;
using WLDSolutions.QUIKLiveTrading.Settings;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    internal class QUIKLiveTradingDescription : LTProductDescription
    {
        public override string ProductName
        {
            get { return "QUIKLiveTrading"; }
        }

        public override string Version
        {
            get { return "2.0.0.0"; }
        }

        public override LTSettingsPanel SettingsPanel
        {
            get { return new QUIKSettingsPanel(); }
        }

        public override bool NeedActivation
        {
            get { return true; }
        }
    }
}
