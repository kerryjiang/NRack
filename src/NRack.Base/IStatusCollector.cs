using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyLog;

namespace NRack.Base
{
    public class AppServerStatus
    {
        public AppServerMetadata Metadata { get; private set; }

        public StatusInfoCollection DataCollection { get; private set; }

        public AppServerStatus(AppServerMetadata metadata, StatusInfoCollection dataCollection)
        {
            Metadata = metadata;
            DataCollection = dataCollection;
        }
    }

    public interface IStatusCollector
    {
        void Collect(AppServerStatus bootstrapStatus, IEnumerable<AppServerStatus> appStatusList, ILog logger);
    }
}
