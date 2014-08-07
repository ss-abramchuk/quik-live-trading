using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WLDSolutions.QUIKLiveTrading.Abstract
{
    internal interface IQUIKDDEDataProvider
    {
        event Action<DataTable> NewData;
    }
}
