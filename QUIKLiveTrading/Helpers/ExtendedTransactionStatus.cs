using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    public class ExtendedTransactionStatus : TransactionStatus
    {
        public TransactionState? State
        {
            get;
            set;
        }

        public double OrderID
        {
            get;
            set;
        }
    }
}
