using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;

namespace NDock.Server.AppDomain
{
    class AppDomainBootstrap : BootstrapBase
    {
        public AppDomainBootstrap(IConfigurationSource configSource)
            : base(configSource)
        {

        }
    }
}
