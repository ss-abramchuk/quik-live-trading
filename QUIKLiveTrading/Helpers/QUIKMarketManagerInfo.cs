using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WealthLab.DataProviders.MarketManagerService;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    internal class QUIKMarketManagerInfo : MarketManagerInfo
    {
        public override string ProviderName()
        {
            return "QUIKStaticDataProvider";
        }
    }
}
