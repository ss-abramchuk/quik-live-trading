using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WLDSolutions.LiveTradingManager.Abstract;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    public class QUIKAccount : LTAccount
    {
        public string TradeAccount
        {
            get;
            set;
        }

        public string ClientCode
        {
            get;
            set;
        }

        public string FullAccountNumber
        {
            get { return string.Format("{0}-{1}", TradeAccount, ClientCode); }
        }
    }
}
