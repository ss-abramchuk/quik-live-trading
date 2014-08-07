#define SPECIAL

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Concurrent;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

using WealthLab;
using log4net;

using WLDSolutions.LiveTradingManager;
using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.LiveTradingManager.Helpers;
using WLDSolutions.QUIKLiveTrading.Dispatcher;
using WLDSolutions.QUIKLiveTrading.Helpers;
using WLDSolutions.QUIKLiveTrading.Abstract;

namespace WLDSolutions.QUIKLiveTrading.BrokerProvider
{
    public class QUIKBrokerProvider : LTBrokerProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QUIKBrokerProvider));

        private ILTSettingsProvider _settingsProvider;
        private IQUIKOrderProvider _orderProvider;

        private Random _randomizer;

        private List<QUIKAccount> _accounts;
        private List<ExtendedSymbolDescription> _symbols;
        private List<object> _delayedUpdates;

        private DataTable _actualFuturePositions = null;
        private DataTable _actualStockPositions = null;

        private bool _initStockPositionsUpdate;
        private bool _initFuturePositionsUpdate;

        public override event Action<LTAccount> AccountUpdate;
        public override event Action<OrderUpdateInfo> OrderUpdate;

        private System.Threading.Timer _updateTimer;

        private ReaderWriterLockSlim _actualStockPositionsLock = new ReaderWriterLockSlim();
        private ReaderWriterLockSlim _actualFuturePositionsLock = new ReaderWriterLockSlim();

        private object _symbolsLock = new object();
        private object _accountsLock = new object();
        private object _statusLock = new object();       

        public override List<Order> Orders
        {
            get;
            set;
        }

        #region Список названий экспортируемых таблицж

        private const string _futureAccountsTableName = "FutureAccounts";
        private const string _stockAccountsTableName = "StockAccounts";
        private const string _futurePositionsTableName = "FuturePositions";
        private const string _stockPositionsTableName = "StockPositions";
        private const string _securitiesTableName = "Securities";
        private const string _stopOrdersTableName = "StopOrders";
        private const string _limitOrdersTableName = "LimitOrders";
        private const string _tradesTableName = "Trades";

        #endregion

        public QUIKBrokerProvider()
        {
            _randomizer = new Random();

            Orders = new List<Order>();
            _symbols = new List<ExtendedSymbolDescription>();
            _delayedUpdates = new List<object>();

            _initStockPositionsUpdate = true;
            _initFuturePositionsUpdate = true;
        }

        public override void Initialize()
        {
            _settingsProvider = QUIKDispatcher.Instance.SettingsProvider;
            _orderProvider = QUIKDispatcher.Instance.OrderProvider;

            _orderProvider.TransactionStatusChanged += TransactionStatusChanged;
            _orderProvider.TradeStatusChanged += TradeStatusChanged;
            _orderProvider.OrderStatusChanged += OrderStatusChanged;

            _accounts = _settingsProvider.GetObject("Accounts", typeof(List<QUIKAccount>)) as List<QUIKAccount> ?? new List<QUIKAccount>();

            foreach (LTAccount account in _accounts)
            {
                foreach (AccountPosition position in account.Positions)
                {
                    position.Account = account;
                }
            }

            QUIKDispatcher.Instance.DDEDataProvider.NewData += NewData;

            int delayedUpdateInterval = _settingsProvider.GetParameter("DelayedUpdateInterval", 5);

            _updateTimer = new System.Threading.Timer(new TimerCallback(DelayedUpdates), null, delayedUpdateInterval * 1000, delayedUpdateInterval * 1000);

            ConnectionStatus connectionStatus = _orderProvider.Connect(_settingsProvider.GetParameter("QUIKPath", @"C:\Program Files\QUIK"));

            if (connectionStatus.Connected)
                _orderProvider.SubscribeOrders();          
        }

        private void NewData(DataTable table)
        {
            if (string.Compare(table.TableName, _futureAccountsTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
                UpdateFutureAccounts(table);
            else if (string.Compare(table.TableName, _stockAccountsTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
                UpdateStockAccounts(table);
            else if (string.Compare(table.TableName, _futurePositionsTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
                UpdateFuturePositions(table);
            else if (string.Compare(table.TableName, _stockPositionsTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
                UpdateStockPositions(table);
            else if (string.Compare(table.TableName, _securitiesTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
                UpdateSecurities(table);
            else if (string.Compare(table.TableName, _stopOrdersTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
                StopOrderStatusChanged(table);
            else if (string.Compare(table.TableName, _limitOrdersTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
                LimitOrderStatusChanged(table);
            else if (string.Compare(table.TableName, _tradesTableName, StringComparison.InvariantCultureIgnoreCase) == 0)
                TradeStatusChanged(table);
            #if TRACE
            else
                _logger.Debug(string.Format("{0} не совпадает ни с одним из заданных значений.", table.TableName));
            #endif
        }

        private void Connect()
        {
            string path = _settingsProvider.GetParameter("QUIKPath", @"C:\Program Files\QUIK");
            _orderProvider.Connect(path);
            _orderProvider.SubscribeOrders();
        }

        #region Получение информации об аккаунтах и открытых позициях        

        private void UpdateFutureAccounts(DataTable table)
        {
            lock (_accountsLock)
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        if (Convert.ToString(row["LIMIT_TYPE"]) == "Ден.средства")
                        {
                            QUIKAccount account = _accounts.Find(x => x.TradeAccount == Convert.ToString(row["TRDACCID"]));

                            if (account != null)
                            {
                                account.AvailableCash = Convert.ToDouble(row["CBPLIMIT"]) - Convert.ToDouble(row["CBPLUSED"]);
                                account.BuyingPower = account.AvailableCash;
                                account.AccountValue = Convert.ToDouble(row["CBPLUSED"]);
                                account.AccountValueTimeStamp = DateTime.Now;

                                if (AccountUpdate != null)
                                    AccountUpdate(account);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
            }
        }

        private void UpdateStockAccounts(DataTable table)
        {
            lock (_accountsLock)
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        LTAccount account = _accounts.Find(x => x.ClientCode == Convert.ToString(row["CLIENTCODE"]));

                        if (account != null)
                        {
                            account.AvailableCash = Convert.ToDouble(row["TOTALMONEYBAL"]);
                            account.BuyingPower = Convert.ToDouble(row["AVLIMALL"]);
                            account.AccountValue = Convert.ToDouble(row["ASSETS"]);
                            account.AccountValueTimeStamp = DateTime.Now;

                            if (AccountUpdate != null)
                                AccountUpdate(account);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
            }
        }

        private void UpdateFuturePositions(DataTable table)
        {
            SaveActualFuturePositions(table);            

            if (_initFuturePositionsUpdate)
            {
                SendFuturePositions(_actualFuturePositions);
                _initFuturePositionsUpdate = false;
            }
        }

        private void SaveActualFuturePositions(DataTable table)
        {
            _actualFuturePositionsLock.EnterWriteLock();
            
            if (_actualFuturePositions == null)
            {
                _actualFuturePositions = table;

                _actualFuturePositionsLock.ExitWriteLock();

                return;
            }

            foreach (DataRow tableRow in table.Rows)
            {
                try
                {
                    DataRow actualRow = (from DataRow row in _actualFuturePositions.Rows
                                            let actualTradeAcc = Convert.ToString(row["TRDACCID"])
                                            let tableTradeAcc = Convert.ToString(tableRow["TRDACCID"])
                                            let actualSec = Convert.ToString(row["SECCODE"])
                                            let tableSec = Convert.ToString(tableRow["SECCODE"])
                                            where actualTradeAcc == tableTradeAcc &&
                                            actualSec == tableSec
                                            select row).FirstOrDefault();

                    if (actualRow != null)
                    {
                        actualRow["TOTAL_NET"] = tableRow["TOTAL_NET"];
                    }
                    else
                    {
                        actualRow = _actualFuturePositions.NewRow();

                        actualRow["TRDACCID"] = tableRow["TRDACCID"];
                        actualRow["SECCODE"] = tableRow["SECCODE"];
                        actualRow["TOTAL_NET"] = tableRow["TOTAL_NET"];

                        _actualFuturePositions.Rows.Add(actualRow);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }
            
            _actualFuturePositionsLock.ExitWriteLock();
        }

        private void SendFuturePositions(DataTable table)
        {
            if (table == null)
                return;

            _actualFuturePositionsLock.EnterReadLock();

            lock (_accountsLock)
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        LTAccount account = _accounts.Find(x => x.ClientCode == Convert.ToString(row["TRDACCID"]));

                        ExtendedSymbolDescription symbolDescription = _symbols.Find(x => x.SymbolCode == Convert.ToString(row["SECCODE"]));

                        if (account != null && symbolDescription != null)
                        {
                            IEnumerable<AccountPosition> positions = account.Positions.Distinct(new PositionComparer());
                            account.Positions = positions.ToList();

                            AccountPosition position = account.Positions.Find(x => x.Symbol == symbolDescription.FullCode);

                            double quantity = Convert.ToDouble(row["TOTAL_NET"]);

                            if (position != null)
                            {
                                if (quantity != 0)
                                {
                                    position.PositionType = quantity > 0 ? PositionType.Long : PositionType.Short;
                                    position.Quantity = Math.Abs(quantity);
                                    position.LastPrice = symbolDescription.LastQuote;
                                }
                                else
                                    account.Positions.Remove(position);
                            }
                            else if (quantity != 0)
                            {
                                position = new AccountPosition()
                                {
                                    Account = account,
                                    Symbol = symbolDescription.FullCode,
                                    PositionType = quantity > 0 ? PositionType.Long : PositionType.Short,
                                    Quantity = Math.Abs(quantity),
                                    EntryPrice = 0,
                                    LastPrice = symbolDescription.LastQuote
                                };

                                account.Positions.Add(position);
                            }

                            account.AccountValueTimeStamp = DateTime.Now;

                            if (AccountUpdate != null)
                                AccountUpdate(account);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
            }

            _actualFuturePositionsLock.ExitReadLock();
        }

        private void UpdateStockPositions(DataTable table)
        {
            SaveActualStockPositions(table);

            if (_initStockPositionsUpdate)
            {
                SendStockPositions(_actualStockPositions);
                _initStockPositionsUpdate = false;
            }
        }

        private void SaveActualStockPositions(DataTable table)
        {
            _actualStockPositionsLock.EnterWriteLock();

            if (_actualStockPositions == null)
            {
                _actualStockPositions = table;

                _actualStockPositionsLock.ExitWriteLock();

                return;
            }           

            foreach (DataRow tableRow in table.Rows)
            {
                try
                {
                    DataRow actualRow = (from DataRow row in _actualStockPositions.Rows
                                            let actualTradeAcc = Convert.ToString(row["TRDACCID"])
                                            let tableTradeAcc = Convert.ToString(tableRow["TRDACCID"])
                                            let actualCode = Convert.ToString(row["CLIENT_CODE"])
                                            let tableCode = Convert.ToString(tableRow["CLIENT_CODE"])
                                            let actualSec = Convert.ToString(row["SECURITY"])
                                            let tableSec = Convert.ToString(tableRow["SECURITY"])
                                            where string.Compare(actualTradeAcc, tableTradeAcc, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                                            string.Compare(actualCode, tableCode, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                                            string.Compare(actualSec, tableSec, StringComparison.InvariantCultureIgnoreCase) == 0
                                            select row).FirstOrDefault();

                    if (actualRow != null)
                    {
                        actualRow["CURRENTBAL"] = tableRow["CURRENTBAL"];
                        actualRow["WA_POSITION_PRICE"] = tableRow["WA_POSITION_PRICE"];
                    }
                    else
                    {
                        actualRow = _actualStockPositions.NewRow();

                        actualRow["TRDACCID"] = tableRow["TRDACCID"];
                        actualRow["CLIENT_CODE"] = tableRow["CLIENT_CODE"];
                        actualRow["SECURITY"] = tableRow["SECURITY"];
                        actualRow["CURRENTBAL"] = tableRow["CURRENTBAL"];
                        actualRow["WA_POSITION_PRICE"] = tableRow["WA_POSITION_PRICE"];

                        _actualStockPositions.Rows.Add(actualRow);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }            
            
            _actualStockPositionsLock.ExitWriteLock();
        }

        private void SendStockPositions(DataTable table)
        {
            if (table == null)
                return;

            _actualStockPositionsLock.EnterReadLock();

            lock (_accountsLock)
            {
                foreach (DataRow row in table.Rows)
                {
                    try
                    {
                        LTAccount account = _accounts.Find(x => x.ClientCode == Convert.ToString(row["CLIENT_CODE"]) && x.TradeAccount == Convert.ToString(row["TRDACCID"]));

                        ExtendedSymbolDescription symbolDescription = _symbols.Find(x => x.SymbolCode == Convert.ToString(row["SECURITY"]));

                        if (account != null && symbolDescription != null)
                        {
                            IEnumerable<AccountPosition> positions = account.Positions.Distinct(new PositionComparer());
                            account.Positions = positions.ToList();

                            AccountPosition position = account.Positions.Find(x => x.Symbol == symbolDescription.FullCode);

                            double quantity = Convert.ToDouble(row["CURRENTBAL"]);

                            if (position != null)
                            {
                                if (quantity != 0)
                                {
                                    position.PositionType = quantity > 0 ? PositionType.Long : PositionType.Short;
                                    position.Quantity = Math.Abs(quantity);
                                    position.EntryPrice = Convert.ToDouble(row["WA_POSITION_PRICE"]);
                                    position.LastPrice = symbolDescription.LastQuote;
                                }
                                else
                                    account.Positions.Remove(position);
                            }
                            else if (quantity != 0)
                            {
                                position = new AccountPosition()
                                {
                                    Account = account,
                                    Symbol = symbolDescription.FullCode,
                                    PositionType = quantity > 0 ? PositionType.Long : PositionType.Short,
                                    Quantity = Math.Abs(quantity),
                                    EntryPrice = Convert.ToDouble(row["WA_POSITION_PRICE"]),
                                    LastPrice = symbolDescription.LastQuote
                                };

                                account.Positions.Add(position);
                            }

                            account.AccountValueTimeStamp = DateTime.Now;

                            if (AccountUpdate != null)
                                AccountUpdate(account);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }                    
                }
            }

            _actualStockPositionsLock.ExitReadLock();
        }

        private void UpdateSecurities(DataTable table)
        {
            foreach(DataRow row in table.Rows)
            {
                lock (_symbolsLock)
                {
                    ExtendedSymbolDescription symbolDescription = _symbols.Find(x => x.MarketCode == Convert.ToString(row["CLASS_CODE"]) && x.SymbolCode == Convert.ToString(row["CODE"]));

                    if (symbolDescription == null)
                    {
                        symbolDescription = new ExtendedSymbolDescription()
                        {
                            MarketCode = Convert.ToString(row["CLASS_CODE"]),
                            SymbolCode = Convert.ToString(row["CODE"]),
                            SymbolName = Convert.ToString(row["LONGNAME"])
                        };

                        if (string.IsNullOrWhiteSpace(symbolDescription.MarketCode) || string.IsNullOrWhiteSpace(symbolDescription.SymbolCode))
                        {
                            #if TRACE
                            _logger.Debug(string.Format("Невозможно добавить инструмент: {0}", symbolDescription.SymbolName));
                            #endif
                            continue;
                        }

                        try
                        {
                            symbolDescription.Tick = Convert.ToDouble(row["SEC_PRICE_STEP"]);
                        }
                        catch
                        {
                            symbolDescription.Tick = -1;

                            #if TRACE
                            _logger.Debug(string.Format("Невозможно получить размер тика для инструмента {0}", symbolDescription.FullCode));
                            #endif
                        }

                        try
                        {
                            symbolDescription.Lot = Convert.ToDouble(row["lotsize"]);
                        }
                        catch
                        {
                            symbolDescription.Lot = -1;

                            #if TRACE
                            _logger.Debug(string.Format("Невозможно получить размер лота для инструмента {0}", symbolDescription.FullCode));
                            #endif
                        }

                        try
                        {
                            symbolDescription.Decimals = Convert.ToInt32(row["SEC_SCALE"]);
                        }
                        catch
                        {
                            symbolDescription.Decimals = -1;

                            #if TRACE
                            _logger.Debug(string.Format("Невозможно получить количество знаков после запятой для инструмента {0}", symbolDescription.FullCode));
                            #endif
                        }

                        try
                        {
                            symbolDescription.LastQuote = Convert.ToDouble(row["last"]);
                        }
                        catch
                        {
                            symbolDescription.LastQuote = 0;

                            #if TRACE
                            _logger.Debug(string.Format("Невозможно получить последнюю котировку для инструмента {0}", symbolDescription.FullCode));
                            #endif
                        }                        

                        _symbols.Add(symbolDescription);

                        #if TRACE
                        _logger.Debug(string.Format("Инструмент {0} добавлен в список", symbolDescription.FullCode));
                        #endif
                    }
                    else
                    {
                        if (symbolDescription.Tick == -1)
                        {
                            try
                            {
                                symbolDescription.Tick = Convert.ToDouble(row["SEC_PRICE_STEP"]);
                            }
                            catch
                            {
                                #if TRACE
                                _logger.Debug(string.Format("Невозможно получить размер тика для инструмента {0}", symbolDescription.FullCode));
                                #endif
                            }
                        }

                        if (symbolDescription.Lot == -1)
                        {
                            try
                            {
                                symbolDescription.Lot = Convert.ToDouble(row["lotsize"]);
                            }
                            catch
                            {
                                #if TRACE
                                _logger.Debug(string.Format("Невозможно получить размер лота для инструмента {0}", symbolDescription.FullCode));
                                #endif
                            }
                        }

                        if (symbolDescription.Decimals == -1)
                        {
                            try
                            {
                                symbolDescription.Decimals = Convert.ToInt32(row["SEC_SCALE"]);
                            }
                            catch
                            {
                                #if TRACE
                                _logger.Debug(string.Format("Невозможно получить количество знаков после запятой для инструмента {0}", symbolDescription.FullCode));
                                #endif
                            }
                        }

                        try
                        {
                            symbolDescription.LastQuote = Convert.ToDouble(row["last"]);
                        }
                        catch
                        {
                            #if TRACE
                            _logger.Debug(string.Format("Невозможно получить последнюю котировку для инструмента {0}", symbolDescription.FullCode));
                            #endif
                        }
                    }
                }
            }
        }

        public override List<LTAccount> GetAccounts()
        {
            List<LTAccount> result = new List<LTAccount>();
            result.AddRange(_accounts);

            return result;
        }

        public override void UpdateAccounts()
        {
            SendStockPositions(_actualStockPositions);

            SendFuturePositions(_actualFuturePositions);
        }

        public override List<string> AccountTradeTypes(string account)
        {
            return new List<string>() { "Default" };
        }

        #endregion

        #region Отправка, отмена и обновление статуса ордеров

        public override void PlaceOrder(Order order)
        {
            if (!_orderProvider.DLLConnected)
                Connect();

            Orders.Add(order);

            TransactionDescription description = new TransactionDescription()
            {
                TransactionID = _randomizer.Next(1, int.MaxValue),
                StopOrderID = 0,
                LimitOrderID = 0,
                Trades = new List<Helpers.Trade>(),
            };

            try
            {
                order.BrokerTag = TransactionDescription.Serialize(description);

                string transaction = FormTransactionString(order, description, false);

                _orderProvider.SendTransaction(transaction, description.TransactionID);
            }
            catch (Exception ex)
            {
                OrderUpdateInfo updateInfo = new OrderUpdateInfo()
                {
                    OrderID = order.OrderID,
                    OrderStatus = WealthLab.OrderStatus.Error,
                    TimeStamp = DateTime.Now,
                    FillPrice = 0,
                    FillQty = 0,
                    Code = 0,
                    Message = string.Format("Произошла ошибка при отправке ордера: {0}", ex.Message)
                };

                lock (_statusLock)
                {
                    if (OrderUpdate != null)
                        OrderUpdate(updateInfo);
                }

                _logger.Error(ex);
            }
        }        

        public override void CancelOrder(Order order)
        {
            if (!_orderProvider.DLLConnected)
                Connect();

            try
            {
                TransactionDescription description = TransactionDescription.Deserialize((string)order.BrokerTag);

                string transaction = FormTransactionString(order, description, true);

                _orderProvider.SendTransaction(transaction, description.TransactionID);
            }
            catch (Exception ex)
            {
                OrderUpdateInfo updateInfo = new OrderUpdateInfo()
                {
                    OrderID = order.OrderID,
                    OrderStatus = WealthLab.OrderStatus.Error,
                    TimeStamp = DateTime.Now,
                    FillPrice = 0,
                    FillQty = 0,
                    Code = 0,
                    Message = string.Format("Произошла ошибка при отмене ордера: {0}", ex.Message)
                };

                lock (_statusLock)
                {
                    if (OrderUpdate != null)
                        OrderUpdate(updateInfo);
                }

                _logger.Error(ex);
            }            
        }

        private string FormTransactionString(Order order, TransactionDescription description, bool cancelOrder)
        {
            string account = string.Empty;
            string clientCode = string.Empty;
            string action = string.Empty;
            string type = string.Empty;
            string operation = string.Empty;
            string price = string.Empty;
            string stopPrice = string.Empty;
            string expiryDate = string.Empty;
            string quantity = string.Empty;
            string orderKey = string.Empty;
            double priceValue = 0;

            QUIKAccount acc = _accounts.Find(x => x.FullAccountNumber == order.Account);
            ExtendedSymbolDescription symbolDescription = _symbols.Find(x => x.FullCode == order.Symbol);

            if (acc == null)
                throw new NullReferenceException(string.Format("Аккаунт не был найден: {0}", acc.FullAccountNumber));

            if (symbolDescription == null)
                throw new NullReferenceException(string.Format("Инструмент не был найден: {0}", order.Symbol));

            if (!cancelOrder)
            {
                account = string.Format("ACCOUNT = {0}; ", acc.TradeAccount);
                clientCode = string.Format("CLIENT_CODE = {0}; ", acc.ClientCode);

                switch (order.OrderType)
                {
                    case (OrderType.Limit):
                        action = "ACTION = NEW_ORDER; ";
                        type = "TYPE = L; ";
                        break;

                    case (OrderType.Market):
                        action = "ACTION = NEW_ORDER; ";
                        type = "TYPE = M; ";
                        break;

                    case (OrderType.Stop):
                        action = "ACTION = NEW_STOP_ORDER; ";

                        priceValue = order.Price;

                        if (_settingsProvider.GetParameter("EnableSlippage", false))
                        {
                            if (symbolDescription != null)
                            {
                                if (symbolDescription.MarketCode == "SPBFUT")
                                {
                                    double slippage = _settingsProvider.GetParameter("SlippageTicks", 1);

                                    priceValue = order.AlertType == TradeType.Buy || order.AlertType == TradeType.Cover ? order.Price + slippage * symbolDescription.Tick :
                                        order.Price - slippage * symbolDescription.Tick;
                                }
                                else
                                {
                                    double slippage = _settingsProvider.GetParameter("SlippageUnits", 0.0);

                                    priceValue = order.AlertType == TradeType.Buy || order.AlertType == TradeType.Cover ? order.Price + (slippage / 100) * order.Price :
                                        order.Price - (slippage / 100) * order.Price;

                                    priceValue = Math.Round(priceValue, symbolDescription.Decimals);
                                }
                            }
                        }

                        stopPrice = string.Format("STOPPRICE = {0}; ", order.Price.ToString());

                        expiryDate = string.Format("EXPIRY_DATE = {0}; ", _settingsProvider.GetParameter("TIF", "TODAY"));
                        break;
                }

                operation = order.AlertType == TradeType.Buy || order.AlertType == TradeType.Cover ? "OPERATION = B; " :
                    "OPERATION = S; ";

                quantity = string.Format("QUANTITY = {0}; ", order.Shares);

                price = string.Format("PRICE = {0}; ", order.OrderType == OrderType.Stop ? priceValue : order.Price);
            }
            else
            {
                switch (order.OrderType)
                {
                    case (OrderType.Limit):
                    case (OrderType.Market):
                        action = "ACTION = KILL_ORDER; ";
                        orderKey = string.Format("ORDER_KEY = {0}; ", description.LimitOrderID);
                        break;

                    case (OrderType.Stop):
                        if (description.LimitOrderID == 0)
                        {
                            action = "ACTION = KILL_STOP_ORDER; ";
                            orderKey = string.Format("STOP_ORDER_KEY = {0}; ", description.StopOrderID);
                        }
                        else
                        {
                            action = "ACTION = KILL_ORDER; ";
                            orderKey = string.Format("ORDER_KEY = {0}; ", description.LimitOrderID);
                        }
                        break;
                }
            }

            string transactionID = String.Format("TRANS_ID = {0}; ", description.TransactionID);

            string classCodeValue = string.Empty;
            string secCodeValue = string.Empty;

            classCodeValue = symbolDescription.MarketCode;
            secCodeValue = symbolDescription.SymbolCode;

            string classCode = string.Format("CLASSCODE = {0}; ", classCodeValue);
            string secCode = string.Format("SECCODE = {0}; ", secCodeValue);

            string result = string.Concat(transactionID, account, clientCode, classCode, secCode, action, type,
                operation, quantity, price, stopPrice, orderKey, expiryDate);

            #if TRACE
                _logger.Debug(string.Format("Строка транзакции: {0}", result));
            #endif

            return result;
        }

        public override void OrderStatusUpdate(Order order)
        {

        }

        public override bool AllowOrderType(OrderType orderType)
        {
            if (orderType == OrderType.AtClose)
                return false;
            else
                return true;
        }

        public override List<string> TifsAllowed()
        {
            return new List<string>() { "Default" };
        }

        private void TransactionStatusChanged(TransactionStatus status)
        {
            Order order;

            try
            {
                order = (from ord in Orders
                         let description = TransactionDescription.Deserialize((string)ord.BrokerTag)
                         where description != null && status.TransactionID == description.TransactionID
                         select ord).FirstOrDefault();
            }
            catch
            {
                order = null;
            }

            if (order != null)
            {
                UpdateTransactionStatus(status, order);
            }
            else if (status.TransactionID != 0)
                _delayedUpdates.Add(status);
        }

        private void TradeStatusChanged(TradeStatus status)
        {
            Order order;

            try
            {
                order = (from ord in Orders
                         let description = TransactionDescription.Deserialize((string)ord.BrokerTag)
                         where description != null && status.OrderID == description.LimitOrderID
                         select ord).FirstOrDefault();
            }
            catch
            {
                order = null;
            }

            if (order != null)
            {
                UpdateTradeStatus(status, order);
            }
            else
                _delayedUpdates.Add(status);
        }

        private void OrderStatusChanged(Helpers.OrderStatus status)
        {
            Order order;

            try
            {
                order = (from ord in Orders
                         let description = TransactionDescription.Deserialize((string)ord.BrokerTag)
                         where description != null && status.TransactionID == description.TransactionID
                         select ord).FirstOrDefault();
            }
            catch
            {
                order = null;
            }

            if (order != null)
            {
                UpdateOrderStatus(status, order);
            }
            else if (status.TransactionID != 0)
                _delayedUpdates.Add(status);
        }        

        private void StopOrderStatusChanged(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                try
                {
                    OrderState state;
                    switch (Convert.ToString(row["STATUS"]))
                    {
                        case "ACTIVE": state = OrderState.Active; break;
                        case "FILLED": state = OrderState.Complete; break;
                        case "KILLED": state = OrderState.Canceled; break;
                        default: state = OrderState.Unknown; break;
                    }

                    StopOrderResult result;
                    switch (Convert.ToString(row["STATUS_DESC"]))
                    {
                        case "": result = StopOrderResult.Active; break;
                        case "ORDER SENT TO TS": result = StopOrderResult.Sent; break;
                        case "REJECTED BY TS": result = StopOrderResult.Rejected; break;
                        case "KILLED": result = StopOrderResult.Canceled; break;
                        default: result = StopOrderResult.Unknown; break;
                    }

                    StopOrderStatus status = new StopOrderStatus()
                    {
                        TransactionID = Convert.ToInt64(row["TRANSID"]),
                        OrderID = Convert.ToDouble(row["STOP_ORDERNUM"]),
                        LimitOrderID = Convert.ToDouble(row["LINKED_ORDER"]),
                        State = state,
                        Result = result
                    };

#if SPECIAL
                    //_logger.Debug(string.Format("[SO] TransID: {0}, StopID: {1}, LimitID: {2}, Status: {3}, Desc: {4}",
                    //    row["TRANSID"], row["STOP_ORDERNUM"], row["LINKED_ORDER"], row["STATUS"], row["STATUS_DESC"]));
                    _logger.Debug(string.Format("[SO] TransID: {0}, StopID: {1}, LimitID: {2}, Status: {3}, Desc: {4}",
                        status.TransactionID, status.OrderID, status.LimitOrderID, status.State, status.Result));
#endif

                    Order order;

                    try
                    {
                        order = (from ord in Orders
                                 let description = TransactionDescription.Deserialize((string)ord.BrokerTag)
                                 where description != null && status.TransactionID == description.TransactionID
                                 select ord).FirstOrDefault();
                    }
                    catch
                    {
                        order = null;
                    }

                    if (order != null)
                        UpdateStopOrderStatus(status, order);
                    else if (status.TransactionID != 0)
                    {
#if SPECIAL
                        _logger.Debug(string.Format("[SO] TransID: {0} - Отложенное обновление", status.TransactionID));
#endif
                        _delayedUpdates.Add(status);
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    break;
                }
            }
        }

        private void LimitOrderStatusChanged(DataTable table)
        {
            //throw new NotImplementedException();
        }


        private void TradeStatusChanged(DataTable table)
        {
            //throw new NotImplementedException();
        }

        private void DelayedUpdates(object state)
        {
            List<object> buffer = new List<object>();
            buffer.AddRange(_delayedUpdates);

            foreach (object obj in buffer)
            {
                if (obj is TransactionStatus)
                {
                    TransactionStatus status = (TransactionStatus)obj;

                    Order order;

                    try
                    {
                        order = (from ord in Orders
                                 let description = TransactionDescription.Deserialize((string)ord.BrokerTag)
                                 where description != null && status.TransactionID == description.TransactionID
                                 select ord).FirstOrDefault();
                    }
                    catch
                    {
                        order = null;
                    }

                    if (order != null)
                    {
                        UpdateTransactionStatus(status, order);
                        _delayedUpdates.Remove(obj);
                    }
                }
                else if (obj is TradeStatus)
                {
                    TradeStatus status = (TradeStatus)obj;

                    Order order;

                    try
                    {
                        order = (from ord in Orders
                                 let description = TransactionDescription.Deserialize((string)ord.BrokerTag)
                                 where description != null && status.OrderID == description.LimitOrderID
                                 select ord).FirstOrDefault();
                    }
                    catch
                    {
                        order = null;
                    }

                    if (order != null)
                    {
                        UpdateTradeStatus(status, order);
                        _delayedUpdates.Remove(obj);
                    }
                    
                }
                else if(obj is Helpers.OrderStatus)
                {
                    Helpers.OrderStatus status = (Helpers.OrderStatus)obj;

                    Order order;

                    try
                    {
                        order = (from ord in Orders
                                 let description = TransactionDescription.Deserialize((string)ord.BrokerTag)
                                 where description != null && status.TransactionID == description.TransactionID
                                 select ord).FirstOrDefault();
                    }
                    catch
                    {
                        order = null;
                    }

                    if (order != null)
                    {
                        UpdateOrderStatus(status, order);
                        _delayedUpdates.Remove(obj);
                    }
                }
                else if (obj is StopOrderStatus)
                {
                    StopOrderStatus status = (StopOrderStatus)obj;

                    Order order;

                    try
                    {
                        order = (from ord in Orders
                                 let description = TransactionDescription.Deserialize((string)ord.BrokerTag)
                                 where description != null && status.TransactionID == description.TransactionID
                                 select ord).FirstOrDefault();
                    }
                    catch
                    {
                        order = null;
                    }

                    if (order != null)
                    {
                        UpdateStopOrderStatus(status, order);
                        _delayedUpdates.Remove(obj);
                    }
                }
            }

            UpdateAccounts();
        }

        private void UpdateTransactionStatus(TransactionStatus status, Order order)
        {
            WealthLab.OrderStatus orderStatus = status.Sent ? 
                order.Status == WealthLab.OrderStatus.CancelPending ? WealthLab.OrderStatus.CancelPending : WealthLab.OrderStatus.Submitted : 
                WealthLab.OrderStatus.Error;

            if (status is ExtendedTransactionStatus)
            {
                ExtendedTransactionStatus extStatus = (ExtendedTransactionStatus)status;

                orderStatus = extStatus.Sent && 
                    (extStatus.State == TransactionState.Complete || extStatus.State == TransactionState.SentToServer || extStatus.State == TransactionState.ReceivedByServer)
                    ? order.Status == WealthLab.OrderStatus.CancelPending ? WealthLab.OrderStatus.CancelPending : WealthLab.OrderStatus.Active : 
                    WealthLab.OrderStatus.Error;                

                if (extStatus.OrderID != 0)
                {
                    TransactionDescription description = TransactionDescription.Deserialize((string)order.BrokerTag);

                    if (order.OrderType == OrderType.Stop && description.StopOrderID == 0)
                    {
                        description.StopOrderID = extStatus.OrderID;
                    }
                    else
                        description.LimitOrderID = extStatus.OrderID;

                    order.BrokerTag = TransactionDescription.Serialize(description);
                }                
            }

            OrderUpdateInfo updateInfo = new OrderUpdateInfo()
            {
                OrderID = order.OrderID,
                OrderStatus = orderStatus,
                TimeStamp = DateTime.Now,
                FillPrice = 0,
                FillQty = 0,
                Code = (int)status.ExtCode,
                Message = status.Message
            };

            lock (_statusLock)
            {
                if (OrderUpdate != null && (order.Status == WealthLab.OrderStatus.Active || order.Status == WealthLab.OrderStatus.Submitted ||
                    order.Status == WealthLab.OrderStatus.CancelPending))
                {
                    OrderUpdate(updateInfo);
                }
            }
        }

        private void UpdateTradeStatus(TradeStatus status, Order order)
        {
            TransactionDescription description = TransactionDescription.Deserialize((string)order.BrokerTag);

            description.Trades.Add(new Helpers.Trade() { TradeID = status.TradeID, Price = status.Price, Qty = status.Qty });

            order.BrokerTag = TransactionDescription.Serialize(description);

            OrderUpdateInfo updateInfo = new OrderUpdateInfo()
            {
                OrderID = order.OrderID,
                OrderStatus = order.Status == WealthLab.OrderStatus.Filled ? WealthLab.OrderStatus.Filled : WealthLab.OrderStatus.PartialFilled,
                TimeStamp = DateTime.Now,
                FillPrice = description.GetAvgFillPrice(),
                FillQty = description.GetFillQty(),
                Code = 0,
                Message = string.Format("{0} - удовлетворено {1} по цене {2}.", status.TradeID, status.Qty, status.Price)
            };

            if (order.Status == WealthLab.OrderStatus.Filled && order.FillQty != order.Shares)
                order.Status = WealthLab.OrderStatus.PartialFilled;

            lock (_statusLock)
            {
                if (OrderUpdate != null)
                {
                    OrderUpdate(updateInfo);
                }
            }
        }

        private void UpdateOrderStatus(Helpers.OrderStatus status, Order order)
        {
            WealthLab.OrderStatus orderStatus;

            switch (status.State)
            {
                case OrderState.Active:
                    orderStatus = order.FillQty != 0 ? WealthLab.OrderStatus.PartialFilled : WealthLab.OrderStatus.Active;
                    break;

                case OrderState.Canceled:
                    orderStatus = WealthLab.OrderStatus.Canceled;
                    break;

                case OrderState.Complete:
                    orderStatus = WealthLab.OrderStatus.Filled;
                    break;

                default:
                    orderStatus = WealthLab.OrderStatus.Unknown;
                    break;
            }

            OrderUpdateInfo updateInfo = new OrderUpdateInfo()
            {
                OrderID = order.OrderID,
                OrderStatus = orderStatus,
                TimeStamp = DateTime.Now,
                FillPrice = 0,
                FillQty = 0,
                Code = 0,
                Message = string.Empty
            };

            lock (_statusLock)
            {
                if (OrderUpdate != null && order.Status != WealthLab.OrderStatus.Error && order.Status != WealthLab.OrderStatus.Filled &&
                    order.Status != WealthLab.OrderStatus.Canceled)
                {
                    OrderUpdate(updateInfo);
                }
            }
        }

        private void UpdateStopOrderStatus(StopOrderStatus status, Order order)
        {
#if SPECIAL
            _logger.Debug(string.Format("[SOU] TransID: {0}, StopID: {1}, LimitID: {2}, Status: {3}, Desc: {4}",
                status.TransactionID, status.OrderID, status.LimitOrderID, status.State, status.Result));
#endif

            string message = string.Empty;

            WealthLab.OrderStatus orderStatus;
            switch(status.State)
            {
                case OrderState.Active: orderStatus = WealthLab.OrderStatus.Active; break;
                case OrderState.Canceled: orderStatus = WealthLab.OrderStatus.Canceled; break;
                case OrderState.Complete:
                    switch (status.Result)
                    {
                        case StopOrderResult.Rejected:
                            orderStatus = WealthLab.OrderStatus.Error;
                            message = "Заявка отвергнута ТС.";
                            break;

                        case StopOrderResult.Sent:
                            orderStatus = WealthLab.OrderStatus.Active;
                            message = string.Format("Заявка №{0} выставленна в ТС.", status.LimitOrderID);

                            TransactionDescription description = TransactionDescription.Deserialize((string)order.BrokerTag);
                            description.LimitOrderID = status.LimitOrderID;
                            order.BrokerTag = TransactionDescription.Serialize(description);
                            break;

                        default: orderStatus = WealthLab.OrderStatus.Unknown; break;
                    }
                    break;
                default: orderStatus = WealthLab.OrderStatus.Unknown; break;
            }

            OrderUpdateInfo updateInfo = new OrderUpdateInfo()
            {
                OrderID = order.OrderID,
                OrderStatus = orderStatus,
                TimeStamp = DateTime.Now,
                FillPrice = 0,
                FillQty = 0,
                Code = 0,
                Message = message
            };

            lock (_statusLock)
            {
                if (OrderUpdate != null && (order.Status == WealthLab.OrderStatus.Active || order.Status == WealthLab.OrderStatus.Submitted
                    || order.Status == WealthLab.OrderStatus.Unknown || order.Status == WealthLab.OrderStatus.CancelPending))
                {
#if SPECIAL
                    _logger.Debug(string.Format("[SOU] TransID: {0} - обновление статуса", status.TransactionID));
#endif

                    OrderUpdate(updateInfo);
                }
            }
        }

        #endregion

        #region Служебная информация

        public override string ProviderName
        {
            get { return "QUIKLiveTrading"; }
        }

        public override bool Enable
        {
            get { return _settingsProvider.GetParameter("BrokerProviderActive", true); }
        }

        #endregion
    }
}
