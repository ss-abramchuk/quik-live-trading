using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

using WealthLab;
using log4net;
using WealthLab.DataProviders.MarketManagerService;

using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.LiveTradingManager.Helpers;
using WLDSolutions.QUIKLiveTrading.Abstract;
using WLDSolutions.QUIKLiveTrading.Dispatcher;
using WLDSolutions.QUIKLiveTrading.Helpers;

namespace WLDSolutions.QUIKLiveTrading.DataProvider
{
    public class QUIKStaticDataProvider : StaticDataProvider
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QUIKStaticDataProvider));

        private IQUIKStaticProvider _staticProvider;
        private ILTSettingsProvider _rttSettingsProvider;

        private BarDataScale _newDSScale;
        private List<SymbolDescription> _newDSSymbols;

        private BarDataStore _dataStore;

        private CancellationTokenSource _cancelTokenSource = null;

        private object _locker = new object();
        private object _getHistoryLocker = new object();

        public override void Initialize(IDataHost dataHost)
        {
            base.Initialize(dataHost);

            _rttSettingsProvider = QUIKDispatcher.Instance.RTTSettingsProvider;
            _staticProvider = QUIKDispatcher.Instance.StaticProvider;

            _dataStore = new BarDataStore(dataHost, this);
        }

        #region Создание и изменение наборов данных

        public override DataSource CreateDataSource()
        {
            DataSource dataSource = new DataSource(this);

            dataSource.BarDataScale = _newDSScale;
            dataSource.DSString = SymbolDescription.SerializeList(_newDSSymbols);

            return dataSource;
        }

        public override string SuggestedDataSourceName
        {
            get { return "QUIK Dataset"; }
        }

        public override string ModifySymbols(DataSource ds, List<string> symbols)
        {
            List<SymbolDescription> symbolsDescription = SymbolDescription.DeserializeList(ds.DSString);

            // Удаление инструментов
            //
            List<SymbolDescription> removeQuery = (from description in symbolsDescription
                                                   where !symbols.Exists(x => x == description.FullCode)
                                                   select description).ToList();

            foreach (SymbolDescription description in removeQuery)
            {
                symbolsDescription.Remove(description);
            }

            // Добавление инструментов
            //
            var addQuery = from symbol in symbols
                           where !symbolsDescription.Exists(x => x.FullCode == symbol)
                           select symbol;

            int errorsCount = 0;

            foreach (string symbol in addQuery)
            {
                SymbolDescription description = GetSymbolDescription(symbol);

                if (description != null)
                    symbolsDescription.Add(description);
                else
                    errorsCount++;
            }

            if (errorsCount > 0)
                MessageBox.Show(string.Format("В формате имени одного или нескольких инструментов была допущена ошибка. Всего ошибок: {0}.", errorsCount),
                    "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return SymbolDescription.SerializeList(symbolsDescription);
        }

        public override void PopulateSymbols(DataSource ds, List<string> symbols)
        {
            if (string.IsNullOrEmpty(ds.Name)) return;

            List<SymbolDescription> symbolsDescription = SymbolDescription.DeserializeList(ds.DSString);

            var query = from description in symbolsDescription
                        select description.FullCode;

            symbols.AddRange(query);
        }

        private SymbolDescription GetSymbolDescription(string symbol)
        {
            try
            {
                SymbolDescription description = new SymbolDescription();

                description.MarketCode = symbol.Substring(0, symbol.IndexOf("."));
                description.SymbolCode = symbol.Substring(symbol.IndexOf(".") + 1, symbol.Length - symbol.IndexOf(".") - 1);

                return description;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
        }

        public override System.Windows.Forms.UserControl WizardFirstPage()
        {
            WizardPage wizardPage = new WizardPage();

            return wizardPage;
        }

        public override System.Windows.Forms.UserControl WizardNextPage(System.Windows.Forms.UserControl currentPage)
        {
            WizardPage wizardPage = currentPage as WizardPage;

            if (wizardPage != null)
            {
                _newDSScale = wizardPage.GetDataScale();

                int errors = 0;
                _newDSSymbols = wizardPage.GetSymbolsDescription(ref errors);

                if(errors > 0)
                    MessageBox.Show(string.Format("В формате имени одного или нескольких инструментов была допущена ошибка. Всего ошибок: {0}.", errors),
                    "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        public override System.Windows.Forms.UserControl WizardPreviousPage(System.Windows.Forms.UserControl currentPage)
        {
            return null;
        }

        public override bool CanModifySymbols
        {
            get { return true; }
        }

        public override bool CanEditSymbolDataFile
        {
            get { return false; }
        }

        public override bool CanDeleteSymbolDataFile
        {
            get { return true; }
        }

        #endregion

        #region Получение исторических данных и обновление наборов данных

        public override MarketInfo GetMarketInfo(string symbol)
        {
            MarketInfo marketInfo = MarketManager.GetMarketInfo(symbol, "Russian Standard Time", "QUIKStaticDataProvider");

            if (marketInfo.Name != null)
            {
                List<MarketTimeZone> timeZones = _rttSettingsProvider.GetObject("MarketTimeZones", typeof(List<MarketTimeZone>)) as List<MarketTimeZone> ?? new List<MarketTimeZone>();

                marketInfo.TimeZoneName = (from timeZone in timeZones
                                           where timeZone.MarketName == marketInfo.Name
                                           select timeZone.TimeZoneName).DefaultIfEmpty("Russian Standard Time").First();
            }
            else
            {
                marketInfo.Name = "Default Russian Market";
                marketInfo.TimeZoneName = "Russian Standard Time";
                marketInfo.OpenTimeNative = new DateTime(1970, 1, 1, 0, 0, 0);
                marketInfo.CloseTimeNative = new DateTime(1970, 1, 1, 23, 59, 59);
            }

            return marketInfo;
        }

        public override Bars RequestData(DataSource ds, string symbol, DateTime startDate, DateTime endDate, int maxBars, bool includePartialBar)
        {
            Bars bars = new Bars(symbol, ds.Scale, ds.BarInterval);            

            List<SymbolDescription> symbolDescriptions = SymbolDescription.DeserializeList(ds.DSString);
            SymbolDescription description = symbolDescriptions.Find(x => x.FullCode == symbol);

            if (description != null && OnDemandUpdate)
            {
                _dataStore.LoadBarsObject(bars);

                DateTime currentDate = DateTime.Now;

                try
                {
                    string suffix = GetSuffix(ds.BarDataScale);

                    int correction = 0;
                    bars.AppendWithCorrections(GetHistory(ds.BarDataScale, description.FullCode, suffix), out correction);
                }
                catch (Exception exception)
                {
                    logger.Error(exception);
                    MessageBox.Show(string.Format("[{0}] {1}", symbol, exception.Message), "Внимание! Произошла ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (bars.Count > 0 && bars.Date[bars.Count - 1] > currentDate && !includePartialBar)
                        bars.Delete(bars.Count - 1);

                if (!base.IsStreamingRequest)
                    lock (_locker)
                        _dataStore.SaveBarsObject(bars);

            }
            else
                _dataStore.LoadBarsObject(bars, startDate, endDate, maxBars);

            return bars;
        }

        public override void RequestUpdates(List<string> symbols, DateTime startDate, DateTime endDate, BarScale scale, int barInterval, IUpdateRequestCompleted requestCompleted)
        {
            List<Task> tasks = new List<Task>();

            BarDataScale dataScale = new BarDataScale(scale, barInterval);

            foreach (string updSymbol in symbols)
            {
                SymbolDescription symbolDescription = GetSymbolDescription(updSymbol);

                if (symbolDescription != null)
                {
                    tasks.Add(Task.Factory.StartNew((object updateRequiredSymbol) =>
                    {
                        string symbol = (string)updateRequiredSymbol;

                        DateTime currentDate = DateTime.Now;

                        Bars bars = new Bars(symbol, scale, barInterval);

                        try
                        {
                            string suffix = GetSuffix(dataScale);

                            int corrections = 0;
                            bars.AppendWithCorrections(GetHistory(dataScale, symbol, suffix), out corrections);

                            if (bars.Count > 0 && bars.Date[bars.Count - 1] > currentDate)
                                bars.Delete(bars.Count - 1);

                            requestCompleted.UpdateCompleted(bars);
                        }
                        catch (Exception exception)
                        {
                            logger.Error(exception);
                            requestCompleted.UpdateError(symbol, exception);
                        }
                    }, updSymbol));
                }
                else
                {
                    requestCompleted.UpdateError(updSymbol, new Exception("В формате имени инструмента была допущена ошибка"));
                }
            }

            if (tasks.Count > 0)
                Task.WaitAll(tasks.ToArray());

            requestCompleted.ProcessingCompleted();
        }

        public override void UpdateDataSource(DataSource ds, IDataUpdateMessage dataUpdateMsg)
        {
             _cancelTokenSource = new CancellationTokenSource();

            dataUpdateMsg.ReportUpdateProgress(0);

            if (string.IsNullOrWhiteSpace(ds.DSString))
            {
                List<SymbolDescription> symbolDescriptions = (from symbol in ds.Symbols
                                                              select GetSymbolDescription(symbol)).ToList();

                ds.DSString = SymbolDescription.SerializeList(symbolDescriptions);
            }

            List<SymbolDescription> updateRequired = (from description in SymbolDescription.DeserializeList(ds.DSString)
                                                     where UpdateRequired(description, ds.BarDataScale)
                                                     select description).ToList();

            dataUpdateMsg.DisplayUpdateMessage(string.Format("Количество инструментов требующих обновления: {0}", updateRequired.Count));

            if (updateRequired.Count > 0)
            {
                dataUpdateMsg.DisplayUpdateMessage("Запуск обновления инструментов:");

                Task[] tasks = new Task[updateRequired.Count];

                for (int i = 0; i < updateRequired.Count; i++)
                {
                    dataUpdateMsg.DisplayUpdateMessage(string.Format("[START] Инструмент: {0} - Обновление запущено", updateRequired[i].FullCode));

                    tasks[i] = Task.Factory.StartNew((object updateRequiredSymbol) => 
                    {
                        string symbol = (string)updateRequiredSymbol;

                        DateTime currentDate = DateTime.Now;

                        Bars bars = new Bars(symbol, ds.Scale, ds.BarInterval);

                        _dataStore.LoadBarsObject(bars);

                        try
                        {
                            string suffix = GetSuffix(ds.BarDataScale);

                            int corrections = 0;
                            bars.AppendWithCorrections(GetHistory(ds.BarDataScale, symbol, suffix), out corrections);

                            if (bars.Count > 0 && bars.Date[bars.Count - 1] > currentDate)
                                bars.Delete(bars.Count - 1);

                            lock (_locker)
                                _dataStore.SaveBarsObject(bars);

                            dataUpdateMsg.DisplayUpdateMessage(string.Format("[COMPLETE] Инструмент: {0} - Обновление завершено", symbol));
                        }
                        catch (Exception exception)
                        {
                            logger.Error(exception);
                            dataUpdateMsg.DisplayUpdateMessage(string.Format("[ERROR] Инструмент {0} - {1}", symbol, exception.Message));
                        }
                    }, updateRequired[i].FullCode);
                }

                try
                {
                    Task.WaitAll(tasks);
                }
                catch (AggregateException exception)
                {
                    exception.Handle((inner) =>
                    {
                        if (inner is OperationCanceledException)
                            return true;
                        else
                        {
                            logger.Error(inner);
                            return false;
                        }
                    });
                }
            }
        }

        public override void UpdateProvider(IDataUpdateMessage dataUpdateMsg, List<DataSource> dataSources, bool updateNonDSSymbols, bool deleteNonDSSymbols)
        {
            foreach (BarDataScale scale in this._dataStore.GetExistingBarScales())
            {
                if (_cancelTokenSource != null && _cancelTokenSource.IsCancellationRequested)
                {
                    dataUpdateMsg.DisplayUpdateMessage("Обновление провайдера отменено");
                    break;
                }

                dataUpdateMsg.DisplayUpdateMessage("Обновление таймфрейма " + scale.ToString());

                var visibleSymbols = from dataSource in dataSources
                                     where dataSource.BarDataScale == scale
                                     select SymbolDescription.DeserializeList(dataSource.DSString);

                List<SymbolDescription> symbols = new List<SymbolDescription>();

                foreach (List<SymbolDescription> visibleSymbol in visibleSymbols)
                {
                    symbols.AddRange(visibleSymbol);
                }

                if (updateNonDSSymbols)
                {
                    var nonDSSymbols = from symbol in _dataStore.GetExistingSymbols(scale.Scale, scale.BarInterval)
                                       where !symbols.Exists(x => x.FullCode == symbol)
                                       select GetSymbolDescription(symbol);

                    symbols.AddRange(nonDSSymbols);
                }

                DataSource dsVirtual = new DataSource(this);

                dsVirtual.Name = "VirtualDSr";
                dsVirtual.BarDataScale = scale;

                dsVirtual.DSString = SymbolDescription.SerializeList(symbols);

                UpdateDataSource(dsVirtual, dataUpdateMsg);

                if (deleteNonDSSymbols)
                {
                    dataUpdateMsg.DisplayUpdateMessage("--------------");
                    dataUpdateMsg.DisplayUpdateMessage("Удаление истории инструментов не входящих ни в один набор данных данного таймфрейма:");

                    var nonDSSymbols = from symbol in _dataStore.GetExistingSymbols(scale.Scale, scale.BarInterval)
                                       where !symbols.Exists(x => x.FullCode == symbol)
                                       select symbol;

                    foreach (string symbol in nonDSSymbols)
                    {
                        lock (_locker)
                            _dataStore.RemoveFile(symbol, scale.Scale, scale.BarInterval);
                        dataUpdateMsg.DisplayUpdateMessage(string.Format("[DELETED] Инструмент {0} - История удалена", symbol));
                    }

                    if (nonDSSymbols.Count() == 0)
                        dataUpdateMsg.DisplayUpdateMessage(string.Format("[NA] Инструменты для удаления не найдены"));
                }

                dataUpdateMsg.DisplayUpdateMessage("--------------");
            }
        }

        private Bars GetHistory(BarDataScale dataScale, string symbol, string suffix)
        {
            string securityName;

            List<Candle> candles;

            lock(_getHistoryLocker)
                candles = _staticProvider.GetStaticData(dataScale, symbol, suffix, out securityName);

            Bars bars = new Bars(symbol, dataScale.Scale, dataScale.BarInterval);

            foreach (Candle candle in candles)
            {
                bars.Add(candle.Date, candle.Open, candle.High, candle.Low, candle.Close, candle.Volume);
            }

            return bars;
        }

        private string GetSuffix(BarDataScale dataScale)
        {
            string suffix;

            switch (dataScale.Scale)
            {
                case BarScale.Tick:
                    suffix = ".T";
                    break;

                case BarScale.Minute:
                    suffix = "." + dataScale.BarInterval.ToString();
                    break;

                case BarScale.Daily:
                    suffix = ".D";
                    break;

                case BarScale.Weekly:
                    suffix = ".W";
                    break;

                case BarScale.Monthly:
                    suffix = ".M";
                    break;

                default:
                    suffix = string.Empty;
                    break;
            }

            return suffix;
        }

        private bool UpdateRequired(SymbolDescription description, BarDataScale barDataScale)
        {
            if (!_dataStore.ContainsSymbol(description.FullCode, barDataScale.Scale, barDataScale.BarInterval))
                return true;

            MarketHours mktHours = new MarketHours();
            mktHours.Market = GetMarketInfo(description.FullCode);
            DateTime updateTime = _dataStore.SymbolLastUpdated(description.FullCode, barDataScale.Scale, barDataScale.BarInterval);

            if (!barDataScale.IsIntraday)
            {
                if ((DateTime.Now.Date >= updateTime.Date.AddDays(1)) ||
                    (updateTime.Date < mktHours.LastTradingSessionEndedNative.Date))
                    return true;
                else
                    return false;
            }
            else
            {
                if (mktHours.IsMarketOpenNow || (updateTime < mktHours.LastTradingSessionEndedNative))
                    return true;
                else
                    return false;
            }
        }

        public override void CancelUpdate()
        {
            if (_cancelTokenSource != null)
                _cancelTokenSource.Cancel();
        }

        public override bool CanRequestUpdates
        {
            get { return true; }
        }

        public override bool SupportsDataSourceUpdate
        {
            get { return true; }
        }

        public override bool SupportsProviderUpdate
        {
            get { return true; }
        }

        public override bool SupportsDynamicUpdate(BarScale scale)
        {
            return true;
        }

        private bool OnDemandUpdate
        {
            get { return base.DataHost.OnDemandUpdateEnabled; }
        }

        #endregion

        #region Служебная информация

        public override string Description
        {
            get { return "Импорт исторических данных через терминал QUIK"; }
        }

        public override string FriendlyName
        {
            get { return "QUIK Static Data"; }
        }

        public override System.Drawing.Bitmap Glyph
        {
            get { return Properties.Resources.QUIK16x16; }
        }

        public override string URL
        {
            get { return "http://WLDSolutions.ru"; }
        }

        #endregion
    }
}
