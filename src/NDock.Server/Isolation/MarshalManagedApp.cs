using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;

namespace NDock.Server.Isolation
{
    /// <summary>
    /// Marshal wrap for managed app instance
    /// We don't want to make the real app instance marshalable
    /// </summary>
    class MarshalManagedApp : MarshalByRefObject, IManagedApp
    {
        private IManagedApp m_ManagedApp;

        public MarshalManagedApp(string appTypeName)
        {
            var appType = Type.GetType(appTypeName);
            m_ManagedApp = (IManagedApp)Activator.CreateInstance(appType);
        }

        public string Name
        {
            get { return m_ManagedApp.Name; }
        }

        public bool Setup(IBootstrap bootstrap, IServerConfig config)
        {
            return m_ManagedApp.Setup(bootstrap, config);
        }

        public ServerState State
        {
            get { return m_ManagedApp.State; }
        }

        public IServerConfig Config
        {
            get { return m_ManagedApp.Config; }
        }

        public AppServerMetadata GetMetadata()
        {
            return m_ManagedApp.GetMetadata();
        }

        public bool Start()
        {
            return m_ManagedApp.Start();
        }

        public void Stop()
        {
            m_ManagedApp.Stop();
        }

        public bool CanBeRecycled()
        {
            return m_ManagedApp.CanBeRecycled();
        }

        public StatusInfoCollection CollectStatus()
        {
            return m_ManagedApp.CollectStatus();
        }
    }
}
