using System;
using System.Collections.Generic;
#if DOTNETCORE
using ILog = Microsoft.Extensions.Logging.ILogger;
#else
using AnyLog;
#endif

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
