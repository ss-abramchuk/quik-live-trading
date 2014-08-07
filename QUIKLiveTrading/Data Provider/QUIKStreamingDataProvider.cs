using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using WealthLab;
using WealthLab.DataProviders.MarketManagerService;
using log4net;

using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.LiveTradingManager.Helpers;
using WLDSolutions.QUIKLiveTrading.Abstract;
using WLDSolutions.QUIKLiveTrading.Dispatcher;
using WLDSolutions.QUIKLiveTrading.Helpers;

namespace WLDSolutions.QUIKLiveTrading.DataProvider
{
    public class QUIKStreamingDataProvider : StreamingDataProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QUIKStreamingDataProvider));

        private ILTSettingsProvider _rttSettingsProvider;
        private IQUIKStreamingProvider _streamingProvider;

        private QUIKStaticDataProvider _quikStaticProvider;

        private MarketInfo _marketInfo;

        private BarDataStore _dataStore;

        private Dictionary<string, CancellationTokenSource> _subscriptions;

        public override void Initialize(IDataHost dataHost)
        {
            base.Initialize(dataHost);

            _rttSettingsProvider = QUIKDispatcher.Instance.RTTSettingsProvider;
            _streamingProvider = QUIKDispatcher.Instance.StreamProvider;

            _streamingProvider.NewQuote += UpdateNewQuote;

            _quikStaticProvider = new QUIKStaticDataProvider();

            _dataStore = new BarDataStore(dataHost, _quikStaticProvider);

            _subscriptions = new Dictionary<string, CancellationTokenSource>();
        }

        public override MarketInfo GetMarketInfo(string symbol)
        {
            _marketInfo = MarketManager.GetMarketInfo(symbol, "Russian Standard Time", "QUIKStaticDataProvider");

            if (_marketInfo.Name != null)
            {
                List<MarketTimeZone> timeZones = _rttSettingsProvider.GetObject("MarketTimeZones", typeof(List<MarketTimeZone>)) as List<MarketTimeZone> ?? new List<MarketTimeZone>();

                _marketInfo.TimeZoneName = (from timeZone in timeZones
                                            where timeZone.MarketName == _marketInfo.Name
                                            select timeZone.TimeZoneName).DefaultIfEmpty("Russian Standard Time").First();
            }
            else
            {
                _marketInfo.Name = "Default Russian Market";
                _marketInfo.TimeZoneName = "Russian Standard Time";
                _marketInfo.OpenTimeNative = new DateTime(1970, 1, 1, 0, 0, 0);
                _marketInfo.CloseTimeNative = new DateTime(1970, 1, 1, 23, 59, 59);
            }

            return _marketInfo;
        }

        protected override void Subscribe(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol) || !IsConnected)
                return;

            if (_dataStore.GetExistingSymbols().Exists(x => x == symbol) && !_subscriptions.ContainsKey(symbol))
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();

                _subscriptions.Add(symbol, tokenSource);

                Task.Factory.StartNew(x =>
                {
                    string subscribedSymbol = (string)x;

                    _streamingProvider.Stream(subscribedSymbol, tokenSource.Token);

                    _subscriptions.Remove(subscribedSymbol);
                }, symbol, tokenSource.Token);
            }
        }

        protected override void UnSubscribe(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol) || !IsConnected)
                return;

            CancellationTokenSource tokenSource;
            bool gotValue = _subscriptions.TryGetValue(symbol, out tokenSource);

            if (gotValue)
            {
                try
                {
                    tokenSource.Cancel();                    
                }
                catch (Exception exception)
                {
                    _logger.Error(exception);
                }
            }
        }

        public override StaticDataProvider GetStaticProvider()
        {
            return _quikStaticProvider;
        }

        public override bool IsConnected
        {
            get { return true; }
        }

        #region Поддержка Stream в Strategy Monitor

        public override bool SupportsStreamingBars
        {
            get { return false; }
        }

        public override List<int> StreamingBarIntervals
        {
            get { return new List<int>() { 1, 5, 10, 15, 30, 60 }; }
        }

        protected override void SubscribeBars(string symbol, int barInterval)
        {
            base.SubscribeBars(symbol, barInterval);
        }

        protected override void UnSubscribeBars(string symbol, int barInterval)
        {
            base.UnSubscribeBars(symbol, barInterval);
        }

        #endregion

        #region Обновление полученной информации

        private void UpdateNewQuote(Quote quote, Candle candle)
        {
            UpdateQuote(quote);
        }

        #endregion

        #region Служебная информация

        public override string Description
        {
            get { return "Импорт данных в реальном времени через терминал QUIK"; }
        }

        public override string FriendlyName
        {
            get { return "QUIK Streaming Data"; }
        }

        public override string URL
        {
            get { return "http://WLDSolutions.ru"; }
        }

        public override System.Drawing.Bitmap Glyph
        {
            get { return Properties.Resources.QUIK16x16; }
        }

        #endregion
    }
}
