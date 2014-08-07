using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using WealthLab;

using WLDSolutions.QUIKLiveTrading.Helpers;

namespace WLDSolutions.QUIKLiveTrading.Abstract
{
    internal interface IQUIKStreamingProvider
    {
        event Action<Quote, Candle> NewQuote;

        void Stream(string symbol, CancellationToken token);
    }
}
