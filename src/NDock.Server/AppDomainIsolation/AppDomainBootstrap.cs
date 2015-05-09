using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using NDock.Server.Isolation;

namespace NDock.Server.AppDomainIsolation
{
    class AppDomainBootstrap : IsolationBootstrap
    {
        public AppDomainBootstrap(IConfigSource configSource)
            : base(configSource)
        {

        }

        protected override IManagedApp CreateAppInstanceByMetadata(AppServerMetadataAttribute metadata)
        {
            return new AppDomainApp(metadata);
        }
    }
}
