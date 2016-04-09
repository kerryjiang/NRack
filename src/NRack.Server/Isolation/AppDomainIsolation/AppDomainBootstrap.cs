using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using NRack.Base;
using NRack.Base.Config;
using NRack.Base.Configuration;
using NRack.Base.Metadata;
using NRack.Server.Isolation;

namespace NRack.Server.Isolation.AppDomainIsolation
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
