using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;

namespace NDock.Server
{
    class DefaultBootstrap : BootstrapBase
    {
        public DefaultBootstrap(IConfigSource configSource)
            : base(configSource)
        {
            
        }
    }
}
