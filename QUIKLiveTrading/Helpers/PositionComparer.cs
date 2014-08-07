using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WealthLab;

namespace WLDSolutions.QUIKLiveTrading.Helpers
{
    internal class PositionComparer : IEqualityComparer<AccountPosition>
    {
        public bool Equals(AccountPosition x, AccountPosition y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Symbol == y.Symbol;
        }

        public int GetHashCode(AccountPosition obj)
        {
            if (Object.ReferenceEquals(obj, null)) return 0;

            int hashProductName = obj.Symbol == null ? 0 : obj.Symbol.GetHashCode();

            return hashProductName;
        }
    }
}
