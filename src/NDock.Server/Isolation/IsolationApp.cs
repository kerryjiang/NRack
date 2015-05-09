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

        protected const string WorkingDir = "AppRoot";

        protected IsolationApp(AppServerMetadataAttribute metadata)
        {
            State = ServerState.NotInitialized;
            m_Metadata = metadata;
        }

        public string Name { get; private set; }

        public bool Setup(IServerConfig config)
        {
            State = ServerState.Initializing;
            Config = config;
            Name = config.Name;
            State = ServerState.NotStarted;
            return true;
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
