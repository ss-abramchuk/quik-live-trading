using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WLDSolutions.QUIKLiveTrading.Helpers;

namespace WLDSolutions.QUIKLiveTrading.Abstract
{
    internal interface IQUIKOrderProvider
    {
        event Action<TransactionStatus> TransactionStatusChanged;
        event Action<TradeStatus> TradeStatusChanged;
        event Action<OrderStatus> OrderStatusChanged;

        ConnectionStatus Connect(string path);
        bool DLLConnected { get; }

        void SubscribeOrders();

        void SendTransaction(string transaction, int transactionID);        
    }
}
