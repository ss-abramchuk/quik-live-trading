using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    public enum StopOrderResult
    {
        Active,
        Sent,
        Rejected,
        Canceled,
        Unknown
    }
}
