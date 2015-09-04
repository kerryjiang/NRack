using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Configuration;
using NDock.Base.Metadata;
using NDock.Server.Isolation;

namespace NDock.Server.Isolation.AppDomainIsolation
{
    class AppDomainBootstrap : IsolationBootstrap
    {
        public AppDomainBootstrap(IConfigSource configSource)
            : base(configSource)
        {

        }

        protected override IManagedApp CreateAppInstanceByMetadata(AppServerMetadata metadata)
        {
            return new AppDomainApp(metadata, ConfigFilePath);
        }
    }
}
