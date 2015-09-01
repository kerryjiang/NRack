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
        private IBootstrap m_RemoteBootstrapWrap;

        public AppDomainBootstrap(IConfigSource configSource)
            : base(configSource)
        {
            m_RemoteBootstrapWrap = new RemoteBootstrapProxy(this);
        }

        protected override IManagedApp CreateAppInstanceByMetadata(AppServerMetadata metadata)
        {
            return new AppDomainApp(metadata, ConfigFilePath);
        }

        protected override bool Setup(IManagedApp managedApp, IServerConfig config)
        {
            return managedApp.Setup(m_RemoteBootstrapWrap, config);
        }
    }
}
