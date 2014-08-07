using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    public class TransactionStatus
    {
        public long TransactionID
        {
            get;
            set;
        }

        public bool Sent
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public long ExtCode
        {
            get;
            set;
        }
    }
}
