using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;

namespace NDock.Server.Isolation
{
    class IsolationApp : IManagedApp
    {
        private AppServerMetadataAttribute m_Metadata;

        protected IsolationApp(AppServerMetadataAttribute metadata)
        {
            m_Metadata = metadata;
        }

        public string Name { get; private set; }

        public bool Setup(IServerConfig config)
        {
            throw new NotImplementedException();
        }

        public IServerConfig Config { get; private set; }

        public AppServerMetadataAttribute GetMetadata()
        {
            return m_Metadata;
        }

        public bool Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public ServerState State { get; protected set; }
    }
}
