using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

using WealthLab;

using WLDSolutions.QUIKLiveTrading.BrokerProvider;
using WLDSolutions.QUIKLiveTrading.Helpers;

namespace QUIKLiveTradingUnitTests
{
    [TestClass]
    public class QUIKBrokerProviderTests
    {
        [TestMethod]
        public void SendStockPositionsTest()
        {
            QUIKBrokerProvider brokerProvider = new QUIKBrokerProvider();

            Type brokerProviderType = brokerProvider.GetType();

            FieldInfo accountsField = brokerProviderType.GetField("_accounts", BindingFlags.Instance | BindingFlags.NonPublic);

            List<QUIKAccount> accounts = new List<QUIKAccount>()
            {
                new QUIKAccount()
                {
                    AccountNumber = "CLCODE-TRACC",
                    ClientCode = "CLCODE",
                    TradeAccount = "TRACC",
                    ProviderName = "QUIKLiveTrading",
                    Positions = new List<AccountPosition>()
                    {
                        new AccountPosition()
                        {
                            Symbol = "QJSIM.SBER",
                            PositionType = WealthLab.PositionType.Long,
                            Quantity = 100
                        },
                        new AccountPosition()
                        {
                            Symbol = "QJSIM.LKOX",
                            PositionType = WealthLab.PositionType.Long,
                            Quantity = 100
                        }
                    }
                }
            };

            accountsField.SetValue(brokerProvider, accounts);
        }
    }
}
