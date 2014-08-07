using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    public class OrderStatus
    {
        public long TransactionID
        {
            get;
            set;
        }

        public double OrderID
        {
            get;
            set;
        }

        public OrderState State
        {
            get;
            set;
        }
    }
}
