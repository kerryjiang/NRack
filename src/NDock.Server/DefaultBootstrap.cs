using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;

namespace NDock.Server
{
    public class DefaultBootstrap : BootstrapBase
    {
        public DefaultBootstrap(IConfigSource configSource)
            : base(configSource)
        {
            
        }

        protected override IManagedApp CreateAppInstanceByMetadata(AppServerMetadataAttribute metadata)
        {
            return (IManagedApp)Activator.CreateInstance(Type.GetType(metadata.AppType, true, true));
        }
    }
}
