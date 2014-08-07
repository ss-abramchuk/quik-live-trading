using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

using log4net;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    public class TransactionDescription
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TransactionDescription));

        public int TransactionID
        {
            get;
            set;
        }

        public double StopOrderID
        {
            get;
            set;
        }

        public double LimitOrderID
        {
            get;
            set;
        }

        public List<Trade> Trades
        {
            get;
            set;
        }

        public double GetAvgFillPrice()
        {
            double qty = 0;
            double vol = 0;

            foreach (Trade trade in Trades)
            {
                vol += trade.Price * trade.Qty;
                qty += trade.Qty;
            }

            return vol / qty;
        }

        public double GetFillQty()
        {
            double result = 0;

            foreach (Trade trade in Trades)
            {
                result += trade.Qty;
            }

            return result;
        }

        public static string Serialize(TransactionDescription description)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TransactionDescription));

            XmlSerializerNamespaces xmlNameSpaces = new XmlSerializerNamespaces();
            xmlNameSpaces.Add("", "");

            try
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(stringWriter, description, xmlNameSpaces);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return string.Empty;
            }
        }

        public static TransactionDescription Deserialize(string description)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(description))
                    throw new ArgumentException("Строка TransactionDescription имеет значение null");

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TransactionDescription));

                using (StringReader stringReader = new StringReader(description))
                {
                    return (TransactionDescription)xmlSerializer.Deserialize(stringReader);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.Debug(ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }
    }
}
