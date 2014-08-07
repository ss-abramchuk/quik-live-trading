using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Management;

using log4net;

using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.QUIKLiveTrading.Abstract;
using WLDSolutions.QUIKLiveTrading.Helpers;
using Microsoft.Win32;

namespace WLDSolutions.QUIKLiveTrading.Dispatcher
{
    internal class QUIKOrderProvider : IQUIKOrderProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QUIKOrderProvider));

        public QUIKOrderProvider()
        {
            _transactionCallback = new TransactionReplyCallback(TransactionReplyCallbackIMPL);
            _tradeCallback = new TradeStatusCallback(TradeStatusCallbackIMPL);
            _orderCallback = new OrderStatusCallback(OrderStatusCallbackIMPL);
        }

        #region Константы возвращаемых значений

        private const long TRANS2QUIK_SUCCESS = 0;
        private const long TRANS2QUIK_FAILED = 1;
        private const long TRANS2QUIK_QUIK_TERMINAL_NOT_FOUND = 2;
        private const long TRANS2QUIK_DLL_VERSION_NOT_SUPPORTED = 3;
        private const long TRANS2QUIK_ALREADY_CONNECTED_TO_QUIK = 4;
        private const long TRANS2QUIK_WRONG_SYNTAX = 5;
        private const long TRANS2QUIK_QUIK_NOT_CONNECTED = 6;
        private const long TRANS2QUIK_DLL_NOT_CONNECTED = 7;
        private const long TRANS2QUIK_QUIK_CONNECTED = 8;
        private const long TRANS2QUIK_QUIK_DISCONNECTED = 9;
        private const long TRANS2QUIK_DLL_CONNECTED = 10;
        private const long TRANS2QUIK_DLL_DISCONNECTED = 11;
        private const long TRANS2QUIK_MEMORY_ALLOCATION_ERROR = 12;
        private const long TRANS2QUIK_WRONG_CONNECTION_HANDLE = 13;
        private const long TRANS2QUIK_WRONG_INPUT_PARAMS = 14;    

        #endregion

        #region События, делегаты, поля для получения статуса ордеров

        public event Action<TransactionStatus> TransactionStatusChanged;
        public event Action<TradeStatus> TradeStatusChanged;
        public event Action<OrderStatus> OrderStatusChanged;

        private delegate void TransactionReplyCallback(Int32 nTransactionResult, Int32 nTransactionExtendedErrorCode,
            Int32 nTransactionReplyCode, UInt32 dwTransId, Double dOrderNum, [MarshalAs(UnmanagedType.LPStr)] string TransactionReplyMessage);
        private TransactionReplyCallback _transactionCallback;

        private delegate void TradeStatusCallback(Int32 nMode, Double dNumber, Double dOrderNumber, [MarshalAs(UnmanagedType.LPStr)]string ClassCode,
                [MarshalAs(UnmanagedType.LPStr)]string SecCode, Double dPrice, Int32 nQty, Double dValue, Int32 nIsSell, Int32 nOrderDescriptor);
        private TradeStatusCallback _tradeCallback;

        private delegate void OrderStatusCallback(Int32 nMode, UInt32 dwTransID, Double dNumber, [MarshalAs(UnmanagedType.LPStr)]string ClassCode,
                [MarshalAs(UnmanagedType.LPStr)]string SecCode, Double dPrice, Int32 nBalance, Double dValue, Int32 nIsSell, Int32 nStatus, Int32 nOrderDescriptor);
        private OrderStatusCallback _orderCallback;

        #endregion

        #region Связь с терминалом

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_CONNECT@16", CallingConvention = CallingConvention.StdCall)]
        public static extern long ConnectToQUIK(string lpcstrConnectionParamsString, ref long pnExtendedErrorCode,
           byte[] lpstrErrorMessage, UInt32 dwErrorMessageSize);

        public ConnectionStatus Connect(string path)
        {
            long extendedErrorCode = 0;
            byte[] errorMessage = new byte[50];
            uint errorMessageSize = 50;

            long result = 1;

            result = ConnectToQUIK(path, ref extendedErrorCode, errorMessage, errorMessageSize) & 255;

            ConnectionStatus connectionStatus = new ConnectionStatus()
            {
                Connected = result == TRANS2QUIK_SUCCESS ? true : false,
                Message = GetStatusMessage(result)
            };

            return connectionStatus;
        }

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_IS_DLL_CONNECTED@12", CallingConvention = CallingConvention.StdCall)]
        private static extern long GetDLLConnectionState(ref long pnExtendedErrorCode, byte[] lpstrErrorMessage, UInt32 dwErrorMessageSize);

        public bool DLLConnected
        {
            get
            {
                long extendedErrorCode = 0;
                byte[] errorMessage = new byte[50];
                uint errorMessageSize = 50;

                long result = -1;

                result = GetDLLConnectionState(ref extendedErrorCode, errorMessage, errorMessageSize) & 255;

                if (result == TRANS2QUIK_DLL_CONNECTED)
                    return true;
                else
                    return false;
            }
        }  

        #endregion

        #region Подписка на изменение статуса ордеров

        public void SubscribeOrders()
        {
            Byte[] EMsg = new Byte[50];
            UInt32 EMsgSz = 50;
            Int32 ExtEC = 0;

            long transactionResult = -1;
            transactionResult = SetTransactionReplyCallback(_transactionCallback, ref ExtEC, EMsg, EMsgSz);

            if (transactionResult != TRANS2QUIK_SUCCESS)
            {
                string message = ByteToString(EMsg);
                _logger.Debug("Не удалось установить функцию обратного вызова для получения информации об отправленной асинхронной транзакции." + (string.IsNullOrWhiteSpace(message) ? "" : " " + message));
            }

            int subscribeTradesResult = -1;
            subscribeTradesResult = SubscribeTrades("", "");

            if (subscribeTradesResult != TRANS2QUIK_SUCCESS)
                _logger.Debug("Не удалось создать список классов бумаг и инструментов для подписки на получение сделок по ним. SubscribeTradesResult: " + subscribeTradesResult);

            StartTrades(_tradeCallback);

            int subscribeOrdersResult = -1;
            subscribeOrdersResult = SubscribeOrders("", "");

            if (subscribeOrdersResult != TRANS2QUIK_SUCCESS)
                _logger.Debug("Не удалось создать список классов бумаг и инструментов для подписки на получение заявок по ним. SubscribeOrdersResult: " + subscribeOrdersResult);

            StartOrders(_orderCallback);
        }

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_SUBSCRIBE_TRADES@8", CallingConvention = CallingConvention.StdCall)]
        private static extern int SubscribeTrades([MarshalAs(UnmanagedType.LPStr)]string class_code, [MarshalAs(UnmanagedType.LPStr)]string sec_code);

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_UNSUBSCRIBE_TRADES@0", CallingConvention = CallingConvention.StdCall)]
        private static extern int UbsubscribeTrades();

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_SUBSCRIBE_ORDERS@8", CallingConvention = CallingConvention.StdCall)]
        private static extern int SubscribeOrders([MarshalAs(UnmanagedType.LPStr)]string class_code, [MarshalAs(UnmanagedType.LPStr)]string sec_code);

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_UNSUBSCRIBE_ORDERS@0", CallingConvention = CallingConvention.StdCall)]
        private static extern int UbsubscribeOrders();

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_START_TRADES@4", CallingConvention = CallingConvention.StdCall)]
        private static extern long StartTrades(TradeStatusCallback pfTradeStatusCallback);

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_START_ORDERS@4", CallingConvention = CallingConvention.StdCall)]
        private static extern long StartOrders(OrderStatusCallback pfOrderStatusCallback);

        private void TradeStatusCallbackIMPL(Int32 nMode, Double dNumber, Double dOrderNumber, string ClassCode, string SecCode,
                Double dPrice, Int32 nQty, Double dValue, Int32 nIsSell, Int32 nTradeDescriptor)
        {
            if (TradeStatusChanged != null)
            {
                TradeStatus tradeStatus = new TradeStatus()
                {
                    OrderID = dOrderNumber,
                    TradeID = dNumber,
                    Price = dPrice,
                    Qty = nQty
                };

                TradeStatusChanged(tradeStatus);
            }
        }

        private void OrderStatusCallbackIMPL(Int32 nMode, UInt32 dwTransID, Double dNumber, string ClassCode, string SecCode,
                Double dPrice, Int32 nBalance, Double dValue, Int32 nIsSell, Int32 nStatus, Int32 nOrderDescriptor)
        {
            if(OrderStatusChanged != null)
            {
                OrderState orderState;

                switch (nStatus)
                {
                    case 1: orderState = OrderState.Active; break;
                    case 2: orderState = OrderState.Canceled; break;
                    default: orderState = OrderState.Complete; break;
                }

                OrderStatus orderStatus = new OrderStatus()
                {
                    TransactionID = dwTransID,
                    OrderID = dNumber,
                    State = orderState
                };

                OrderStatusChanged(orderStatus);
            }
        }

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_SET_TRANSACTIONS_REPLY_CALLBACK@16", CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 SetTransactionReplyCallback(TransactionReplyCallback pfTransactionReplyCallback,
            ref Int32 pnExtendedErrorCode, byte[] lpstrErrorMessage, UInt32 dwErrorMessageSize);

        private void TransactionReplyCallbackIMPL(Int32 nTransactionResult, Int32 nTransactionExtendedErrorCode,
            Int32 nTransactionReplyCode, UInt32 dwTransId, Double dOrderNum, [MarshalAs(UnmanagedType.LPStr)] string TransactionReplyMessage)
        {
            if (TransactionStatusChanged != null)
            {
                string message = GetStatusMessage(nTransactionResult);

                ExtendedTransactionStatus transactionStatus = new ExtendedTransactionStatus()
                {
                    TransactionID = dwTransId,
                    Sent = nTransactionResult == TRANS2QUIK_SUCCESS ? true : false,
                    Message = string.Format("TransactionID: {2}. {0} {1}", message, TransactionReplyMessage.Replace("\n", ". "), dwTransId.ToString()).Trim(),
                    ExtCode = nTransactionExtendedErrorCode,
                    OrderID = dOrderNum,
                    State = GetTransactionState(nTransactionReplyCode)
                };

                TransactionStatusChanged(transactionStatus);
            }
        }

        private TransactionState? GetTransactionState(int transactionReplyCode)
        {
            switch (transactionReplyCode)
            {
                case 0: return TransactionState.SentToServer;
                case 1: return TransactionState.ReceivedByServer;
                case 2: return TransactionState.FailedMICEXConnection;
                case 3: return TransactionState.Complete;
                case 4: return TransactionState.Failed;
                case 5: return TransactionState.GeneralCheckFailed;
                case 6: return TransactionState.LimitCheckFailed;
                case 10: return TransactionState.UnsupportedTransaction;
                case 11: return TransactionState.SignatureCheckFailed;
                case 12: return TransactionState.GetAnswerFailed;
                case 13: return TransactionState.DeniedCrossDeal;

                default:
                    return null;
            }
        }

        #endregion

        #region Отправка ордеров

        [DllImport("TRANS2QUIK.DLL", EntryPoint = "_TRANS2QUIK_SEND_ASYNC_TRANSACTION@16", CallingConvention = CallingConvention.StdCall)]
        private static extern Int32 send_async_transaction([MarshalAs(UnmanagedType.LPStr)]string transactionString,
           ref Int32 nExtendedErrorCode, byte[] lpstrErrorMessage, UInt32 dwErrorMessageSize);

        public void SendTransaction(string transaction, int transactionID)
        {
            UInt32 err_msg_size = 256;
            Byte[] err_msg = new Byte[err_msg_size];
            Int32 nExtendedErrorCode = 0;

            long res = send_async_transaction(transaction, ref nExtendedErrorCode, err_msg, err_msg_size);

            if (TransactionStatusChanged != null)
            {
                string message = GetStatusMessage(res);

                TransactionStatus transactionStatus = new TransactionStatus()
                {
                    TransactionID = transactionID,
                    Sent = res == TRANS2QUIK_SUCCESS ? true : false,
                    Message = string.Format("{0} {1}", message, ByteToString(err_msg)).Trim(),
                    ExtCode = nExtendedErrorCode
                };

                TransactionStatusChanged(transactionStatus);
            }
        }

        private string GetStatusMessage(long result)
        {
            string message = string.Empty;

            switch (result)
            {
                case TRANS2QUIK_SUCCESS:
                    message = "Транзакция успешно отправлена на сервер.";
                    break;

                case TRANS2QUIK_WRONG_SYNTAX:
                    message = "Строка транзакции заполнена неверно.";
                    break;

                case TRANS2QUIK_DLL_NOT_CONNECTED:
                    message = "Отсутствует соединение между библиотекой Trans2QUIK.dll и терминалом QUIK.";
                    break;

                case TRANS2QUIK_QUIK_NOT_CONNECTED:
                    message = "Отсутствует соединение между терминалом QUIK и сервером.";
                    break;

                case TRANS2QUIK_FAILED:
                    message = "Транзакцию отправить не удалось.";
                    break;

                case TRANS2QUIK_QUIK_TERMINAL_NOT_FOUND:
                    message = "В указанном каталоге либо отсутствует INFO.EXE, либо у него не запущен сервис обработки внешних подключений.";
                    break;

                case TRANS2QUIK_DLL_VERSION_NOT_SUPPORTED:
                    message = "Используемая версия Trans2QUIK.dll не поддерживается указанным INFO.EXE.";
                    break;

                case TRANS2QUIK_ALREADY_CONNECTED_TO_QUIK:
                    message = "Соединение уже установлено.";
                    break;

                default:
                    message = "Неизвестный статус транзакции.";
                    break;
            }

            return message;
        }

        #endregion

        public string ByteToString(byte[] Str)
        {
            int count = 0;

            for (int i = 0; i < Str.Length; ++i)
            {
                if (0 == Str[i])
                {
                    count = i;
                    break;
                }
            }

            try
            {
                return System.Text.Encoding.Default.GetString(Str, 0, count);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex);
                return string.Empty;
            }
        }
    }
}
