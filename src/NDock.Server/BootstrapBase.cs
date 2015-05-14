using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using AnyLog;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;

namespace NDock.Server
{
    public abstract class BootstrapBase : IBootstrap
    {
        protected IConfigSource ConfigSource { get; private set; }

        protected List<IManagedApp> ManagedApps { get; private set; }

        protected ExportProvider ExportProvider { get; private set; }

        protected ILogFactory LogFactory { get; private set; }

        protected ILog Log { get; private set; }

        public BootstrapBase(IConfigSource configSource)
        {
            if (configSource == null)
                throw new ArgumentNullException("configSource");

            ConfigSource = configSource;
            ManagedApps = new List<IManagedApp>();
            ExportProvider = CreateExportProvider();
        }

        protected virtual ExportProvider CreateExportProvider()
        {
            return AppDomain.CurrentDomain.GetCurrentAppDomainExportProvider();
        }

        public virtual void Start()
        {
            foreach (var app in ManagedApps)
            {
                app.Start();
            }
        }

        public virtual void Stop()
        {
            foreach (var app in ManagedApps)
            {
                app.Stop();
            }
        }

        protected virtual AppServerMetadata GetAppServerMetadata(IServerConfig serverConfig)
        {
            var typeValidator = new RemoteAppTypeValidator();
            return typeValidator.GetServerMetadata(serverConfig.Type);
        }

        protected abstract IManagedApp CreateAppInstanceByMetadata(AppServerMetadata metadata);

        protected virtual IManagedApp CreateAppInstance(IServerConfig serverConfig)
        {
            var metadata = GetAppServerMetadata(serverConfig);

            if (metadata == null)
                throw new Exception("Failed to load server's type: " + serverConfig.Type);

            return CreateAppInstanceByMetadata(metadata);
        }

        public virtual bool Initialize()
        {
            AnyLog.LogFactory.Configurate(ExportProvider, ConfigSource.LogFactory);
            var logFactory = AnyLog.LogFactory.Current;

            if (logFactory == null)
                throw new Exception("Failed to load LogFactory.");

            LogFactory = logFactory;
            Log = logFactory.GetLog(this.GetType().Name);

            foreach(var config in ConfigSource.Servers)
            {
                IManagedApp server = null;
                
                try
                {
                    server = CreateAppInstance(config);
                }
                catch(Exception e)
                {
                    Log.Error(string.Format("Failed to create server instance with {0}", config.Type), e);
                    return false;
                }

                if(server == null)
                {
                    Log.Error(string.Format("Failed to create server instance with {0}", config.Type));
                    return false;
                }

                try
                {
                    if (!server.Setup(config))
                        throw new Exception("Unknown reason");
                }
                catch(Exception e)
                {
                    Log.Error(string.Format("Failed to setup server instance with {0}", config.Name), e);
                    return false;
                }

                ManagedApps.Add(server);
            }

            return true;
        }

        IEnumerable<IManagedApp> IBootstrap.AppServers
        {
            get { return ManagedApps; }
        }
    }
}
