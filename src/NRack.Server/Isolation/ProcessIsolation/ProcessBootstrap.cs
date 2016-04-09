using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRack.Base;
using NRack.Base.Config;
using NRack.Base.Metadata;
using NRack.Server.Isolation;

namespace NRack.Server.Isolation.ProcessIsolation
{
    class ProcessBootstrap : IsolationBootstrap
    {
        public ProcessBootstrap(IConfigSource configSource)
            : base(configSource)
        {

        }

        protected override IManagedApp CreateAppInstance(IServerConfig serverConfig)
        {
            var appFile = serverConfig.Options.Get("appFile");

            if(string.IsNullOrEmpty(appFile))
                return base.CreateAppInstance(serverConfig);

            var serverMetadata = new ExternalProcessAppServerMetadata(serverConfig.Options.Get("appDir"), appFile, serverConfig.Options.Get("appArgs"));
            return new ExternalProcessApp(serverMetadata, ConfigFilePath);
        }

        protected override IManagedApp CreateAppInstanceByMetadata(AppServerMetadata metadata)
        {
            return new ProcessApp(metadata, ConfigFilePath);
        }
    }
}
