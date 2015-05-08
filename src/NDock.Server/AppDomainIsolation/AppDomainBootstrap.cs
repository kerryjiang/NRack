using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;

namespace NDock.Server.AppDomainIsolation
{
    class AppDomainBootstrap : BootstrapBase
    {
        public AppDomainBootstrap(IConfigSource configSource)
            : base(configSource)
        {

        }

        protected override IManagedApp CreateAppInstance(IServerConfig serverConfig)
        {
            throw new NotImplementedException();
        }
    }
}
