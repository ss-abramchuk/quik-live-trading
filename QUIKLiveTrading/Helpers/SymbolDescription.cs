using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    public class SymbolDescription
    {
        public SymbolDescription()
        {
            MarketCode = string.Empty;
            SymbolCode = string.Empty;
            ExportName = string.Empty;
        }

        public string MarketCode
        {
            get;
            set;
        }

        public string SymbolCode
        {
            get;
            set;
        }

        public string FullCode
        {
            get
            {
                return string.Format("{0}.{1}", MarketCode, SymbolCode);
            }
        }

        public string ExportName
        {
            get;
            set;
        }

        public static string SerializeList(List<SymbolDescription> symbols)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SymbolDescription>));

            XmlSerializerNamespaces xmlNameSpaces = new XmlSerializerNamespaces();
            xmlNameSpaces.Add("", "");

            try
            {
                using (StringWriter stringWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(stringWriter, symbols, xmlNameSpaces);
                    return stringWriter.ToString();
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public static List<SymbolDescription> DeserializeList(string symbols)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SymbolDescription>));

            try
            {
                using (StringReader stringReader = new StringReader(symbols))
                {
                    return (List<SymbolDescription>)xmlSerializer.Deserialize(stringReader);
                }
            }
            catch
            {
                return new List<SymbolDescription>();
            }
        }
    }
}
