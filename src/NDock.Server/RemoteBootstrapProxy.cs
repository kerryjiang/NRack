using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;

namespace NDock.Server
{
    class RemoteBootstrapProxy : MarshalByRefObject, IBootstrap
    {
        class ServerProxy : MarshalByRefObject, IManagedApp
        {
            private IManagedApp m_ManagedApp;

            public ServerProxy(IManagedApp app)
            {
                m_ManagedApp = app;
            }

            public ServerState State
            {
                get { return m_ManagedApp.State; }
            }

            public string Name
            {
                get { return m_ManagedApp.Name; }
            }
            public bool Start()
            {
                return m_ManagedApp.Start();
            }

            public void Stop()
            {
                m_ManagedApp.Stop();
            }

            public bool Setup(IBootstrap bootstrap, IServerConfig config)
            {
                throw new NotImplementedException();
            }

            public IServerConfig Config
            {
                get { return m_ManagedApp.Config; }
            }

            public AppServerMetadata GetMetadata()
            {
                return m_ManagedApp.GetMetadata();
            }
        }

        private IBootstrap m_Bootstrap;

        private List<IManagedApp> m_ManagedApps;

        public RemoteBootstrapProxy(IBootstrap bootstarp)
        {
            m_Bootstrap = bootstarp;
        }

        public IEnumerable<IManagedApp> AppServers
        {
            get
            {
                if(m_ManagedApps == null)
                {
                    m_ManagedApps = m_Bootstrap.AppServers.Select(s => new ServerProxy(s))
                        .OfType<IManagedApp>().ToList();
                }

                return m_ManagedApps;
            }
        }

        public bool Initialize()
        {
            throw new NotSupportedException();
        }

        public void Start()
        {
            throw new NotSupportedException();
        }

        public void Stop()
        {
            throw new NotSupportedException();
        }

        public override object InitializeLifetimeService()
        {
            //Never expire
            return null;
        }
    }
}
