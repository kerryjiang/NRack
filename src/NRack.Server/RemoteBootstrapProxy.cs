using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AnyLog;
using NRack.Base;
using NRack.Base.Config;

namespace NRack.Server
{
    class RemoteBootstrapProxy : MarshalByRefObject, IBootstrap, ILoggerProvider
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

            public bool CanBeRecycled()
            {
                return m_ManagedApp.CanBeRecycled();
            }

            public StatusInfoCollection CollectStatus()
            {
                return m_ManagedApp.CollectStatus();
            }

            public void ReportPotentialConfigChange(IServerConfig config)
            {
                m_ManagedApp.ReportPotentialConfigChange(config);
            }
        }

        private IBootstrap m_Bootstrap;

        private List<IManagedApp> m_ManagedApps;

        public RemoteBootstrapProxy(IBootstrap innerBootstrap)
        {
            m_Bootstrap = innerBootstrap;
            m_ManagedApps = new List<IManagedApp>();

            foreach (var s in m_Bootstrap.AppServers)
            {
                if (s is MarshalByRefObject)
                    m_ManagedApps.Add(s);
                else
                    m_ManagedApps.Add(new ServerProxy(s));
            }
        }

        public RemoteBootstrapProxy()
            : this(AppDomain.CurrentDomain.GetBootstrap())
        {
            
        }

        public IEnumerable<IManagedApp> AppServers
        {
            get
            {
                return m_ManagedApps;
            }
        }

        ILog ILoggerProvider.Logger
        {
            get
            {
                var loggerProvider = m_Bootstrap as ILoggerProvider;

                if (loggerProvider == null)
                    return null;

                return loggerProvider.Logger;
            }
        }

        public string ConfigFilePath
        {
            get
            {
                return m_Bootstrap.ConfigFilePath;
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
