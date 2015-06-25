using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyLog;

namespace NDock.Base
{
    public interface IStatusCollector
    {
        void Collect(IEnumerable<KeyValuePair<AppServerMetadata, StatusInfoCollection>> appStatusList, ILog logger);
    }
}
