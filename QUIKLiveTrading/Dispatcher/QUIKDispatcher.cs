using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Data;

using log4net;

using WLDSolutions.QUIKLiveTrading.Abstract;
using WLDSolutions.QUIKLiveTrading.DDEServer;

using WLDSolutions.LiveTradingManager.Abstract;
using WLDSolutions.LiveTradingManager.Settings;
using WLDSolutions.LiveTradingManager.Dispatcher;

namespace WLDSolutions.QUIKLiveTrading.Dispatcher
{
    internal class QUIKDispatcher
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(QUIKDispatcher));

        #region Свойства диспетчера - провайдеры и события

        public ILTSettingsProvider RTTSettingsProvider
        {
            get;
            private set;
        }

        public ILTSettingsProvider SettingsProvider
        {
            get;
            private set;
        }

        public IQUIKStaticProvider StaticProvider
        {
            get;
            private set;
        }

        public IQUIKStreamingProvider StreamProvider
        {
            get;
            private set;
        }

        public IQUIKOrderProvider OrderProvider
        {
            get;
            private set;
        }

        public IQUIKDDEDataProvider DDEDataProvider
        {
            get;
            private set;
        }

        #endregion

        #region Singleton шаблон

        private static readonly QUIKDispatcher _instance = new QUIKDispatcher();

        public static QUIKDispatcher Instance
        {
            get { return _instance; }
        }

        static QUIKDispatcher()
        {

        }

        private QUIKDispatcher()
        {
            try
            {
                string dataPath = string.Concat(Application.UserAppDataPath, "\\Data");
                
                RTTSettingsProvider = LTDispatcher.Instance.LTSettingsProvider;
                SettingsProvider = new LiveTradingSettingsProvider(dataPath, "\\QUIKLiveTrading.xml");

                QUIKDataProvider dataProvider = new QUIKDataProvider(SettingsProvider);               

                StaticProvider = dataProvider;
                StreamProvider = dataProvider;

                DDEDataProvider = new QUIKDDEDataProvider();

                OrderProvider = new QUIKOrderProvider();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        #endregion
    }
}
