using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using NDock.Server.Isolation;

namespace NDock.Server.Isolation.AppDomainIsolation
{
    class AppDomainApp : IsolationApp
    {
        public AppDomainApp(AppServerMetadataAttribute metadata)
            : base(metadata)
        {

        }
    }
}
