using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WealthLab;

using WLDSolutions.QUIKLiveTrading.Helpers;

namespace WLDSolutions.QUIKLiveTrading.Abstract
{
    internal interface IQUIKStaticProvider
    {
        List<Candle> GetStaticData(BarDataScale dataScale, string symbol, string suffix, out string securityName);
    }
}
