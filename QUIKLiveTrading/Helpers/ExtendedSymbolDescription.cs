using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    internal class ExtendedSymbolDescription : SymbolDescription
    {
        public string SymbolName
        {
            get;
            set;
        }

        public double Tick
        {
            get;
            set;
        }

        public int Decimals
        {
            get;
            set;
        }

        public double Lot
        {
            get;
            set;
        }

        public double LastQuote
        {
            get;
            set;
        }
    }
}
