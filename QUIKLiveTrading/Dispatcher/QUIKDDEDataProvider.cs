using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Threading;

using NDde.Server;
using log4net;

using WLDSolutions.QUIKLiveTrading.Abstract;
using WLDSolutions.QUIKLiveTrading.DDEServer;

namespace WLDSolutions.QUIKLiveTrading.Dispatcher
{
    internal class QUIKDDEDataProvider : IQUIKDDEDataProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QUIKDDEDataProvider));

        public event Action<DataTable> NewData;

        private readonly List<string> _tables;
        private readonly Dictionary<string, List<string>> _tableStructures;

        private Dictionary<string, AutoResetEvent> _triggers;        
        private ConcurrentDictionary<string, ConcurrentQueue<List<List<object>>>> _dataQueues;

        QUIKDDEServer _ddeServer;

        public QUIKDDEDataProvider()
        {
            _tables = new List<string>()
            {
                "FutureAccounts",
                "FuturePositions",
                "StockAccounts",
                "StockPositions",
                "Securities",
                "StopOrders",
                "LimitOrders",
                "Trades"
            };

            _tableStructures = new Dictionary<string, List<string>>();

            _tableStructures.Add("FutureAccounts", new List<string>() 
                { 
                    "TRDACCID",
                    "CBPLIMIT",
                    "CBPLUSED",
                    "VARMARGIN",
                    "LIMIT_TYPE"
                });

            _tableStructures.Add("FuturePositions", new List<string>() 
                {
                    "TRDACCID",
                    "SECCODE",
                    "TOTAL_NET"
                });

            _tableStructures.Add("StockAccounts", new List<string>()
                {
                    "CLIENTCODE",
                    "TOTALMONEYBAL",
                    "AVLIMALL",
                    "ASSETS"
                });

            _tableStructures.Add("StockPositions", new List<string>()
                {
                    "TRDACCID",
                    "CLIENT_CODE",
                    "SECURITY",
                    "CURRENTBAL",
                    "WA_POSITION_PRICE"
                });

            _tableStructures.Add("Securities", new List<string>()
                {
                    "CLASS_CODE",
                    "CODE",
                    "CLASSNAME",
                    "LONGNAME",
                    "SEC_PRICE_STEP",
                    "SEC_SCALE",
                    "lotsize",
                    "status",
                    "last",
                    "selldepo",
                    "buydepo"
                });

            _tableStructures.Add("StopOrders", new List<string>()
                {
                    "ACCOUNT",
                    "CLIENTCODE",
                    "CLASSCODE",
                    "SECCODE",
                    "STOP_ORDERNUM",
                    "LINKED_ORDER",
                    "TRANSID",
                    "STOP_ORDERTIME",
                    "BUYSELL",
                    "CONDITION_PRICE",
                    "PRICE",
                    "QTY",
                    "FILLED_QTY",
                    "BROKERREF",
                    "STATUS",
                    "STATUS_DESC"
                });

            _tableStructures.Add("LimitOrders", new List<string>()
                {
                    "ACCOUNT",
                    "CLIENTCODE",
                    "CLASSCODE",
                    "SECCODE",
                    "ORDERNUM",
                    "TRANSID",
                    "ORDERTIME",
                    "BUYSELL",
                    "PRICE",
                    "QTY",
                    "BALANCE",
                    "BROKERREF",
                    "STATUS"
                });

            _tableStructures.Add("Trades", new List<string>()
                {
                    "ACCOUNT",
                    "CLIENTCODE",
                    "CLASSCODE",
                    "SECCODE",
                    "ORDERNUM",
                    "TRADENUM",
                    "TRADETIME",
                    "BUYSELL",
                    "PRICE",
                    "QTY",
                    "BROKERREF"
                });

            _dataQueues = new ConcurrentDictionary<string, ConcurrentQueue<List<List<object>>>>();

            _triggers = new Dictionary<string, AutoResetEvent>();

            foreach (string tableName in _tables)
            {
                _triggers.Add(tableName, new AutoResetEvent(false));

                _dataQueues.TryAdd(tableName, new ConcurrentQueue<List<List<object>>>());

                Task.Factory.StartNew(x =>
                {
                    string key = (string)x;

                    while (true)
                    {
                        _triggers[key].WaitOne();

                        while (!_dataQueues[key].IsEmpty)
                        {
                            List<List<object>> rawTable = null;
                            bool result = _dataQueues[key].TryDequeue(out rawTable);

                            if (result)
                            {
                                DataTable table = GetTable(key, rawTable);

                                if (NewData != null && table != null)
                                    NewData(table);
                            }
                        }
                    }
                    
                }, tableName, TaskCreationOptions.LongRunning).ContinueWith(observedTask =>
                {
                    foreach (var e in observedTask.Exception.Flatten().InnerExceptions)
                        _logger.Error(e);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }            

            _ddeServer = new QUIKDDEServer();
            _ddeServer.Register();

            _ddeServer.NewRawData += NewRawData;
            _ddeServer.DisconnectTable += Disconnect;
        }

        private void Disconnect(string tableName)
        {
            _triggers[tableName].Set();
        }

        private void NewRawData(string tableName, List<List<object>> rawData)
        {
            if (_tables.Contains(tableName))
            {
                try
                {
                    _dataQueues[tableName].Enqueue(rawData);
                    _triggers[tableName].Set();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }                
            }
            else
                _logger.Debug(string.Format("Неверное имя таблицы: {0}", tableName));
        }

        private DataTable GetTable(string tableName, List<List<object>> rawTable)
        {
            try
            {
                DataTable table = new DataTable(tableName);

                foreach (string columnName in _tableStructures[tableName])
                {
                    DataColumn column = new DataColumn()
                    {
                        ColumnName = columnName,
                        DataType = typeof(object)
                    };

                    table.Columns.Add(column);
                }

                for (int row = 0; row < rawTable.Count; row++)
                {
                    DataRow tableRow = table.NewRow();

                    for (int cell = 0; cell < rawTable[row].Count; cell++)
                    {
                        tableRow[cell] = rawTable[row][cell];
                    }

                    table.Rows.Add(tableRow);
                }

                return table;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }
    }
}
