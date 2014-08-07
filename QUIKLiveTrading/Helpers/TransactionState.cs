using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    public enum TransactionState
    {
        SentToServer = 0,
        ReceivedByServer = 1,
        FailedMICEXConnection = 2,
        Complete = 3,
        Failed = 4,
        GeneralCheckFailed = 5,
        LimitCheckFailed = 6,
        UnsupportedTransaction = 10,
        SignatureCheckFailed = 11,
        GetAnswerFailed = 12,
        DeniedCrossDeal = 13,
    }
}
