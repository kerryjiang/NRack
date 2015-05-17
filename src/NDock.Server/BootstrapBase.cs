using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
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

        protected virtual CompositionContainer CreateExportProvider()
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

        protected virtual bool Setup(IManagedApp managedApp, IServerConfig config)
        {
            return managedApp.Setup(this, config);
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
                    if (!Setup(server, config))
                        throw new Exception("Unknown reason");
                }
                catch(Exception e)
                {
                    Log.Error(string.Format("Failed to setup server instance with {0}", config.Name), e);
                    return false;
                }

                ManagedApps.Add(server);
            }

            try
            {
                RegisterRemotingService();
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                    Log.Error("Failed to register remoting access service!", e);

                return false;
            }

            return true;
        }

        IEnumerable<IManagedApp> IBootstrap.AppServers
        {
            get { return ManagedApps; }
        }

        /// <summary>
        /// Registers the bootstrap remoting access service.
        /// </summary>
        protected virtual void RegisterRemotingService()
        {
            var bootstrapIpcPort = string.Format("NDock.Bootstrap[{0}]", Math.Abs(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar).GetHashCode()));

            var serverChannelName = "Bootstrap";

            var serverChannel = ChannelServices.RegisteredChannels.FirstOrDefault(c => c.ChannelName == serverChannelName);

            if (serverChannel != null)
                ChannelServices.UnregisterChannel(serverChannel);

            serverChannel = new IpcServerChannel(serverChannelName, bootstrapIpcPort);
            ChannelServices.RegisterChannel(serverChannel, false);

            AppDomain.CurrentDomain.SetData("BootstrapIpcPort", bootstrapIpcPort);

            var bootstrapProxyType = typeof(RemoteBootstrapProxy);

            if (!RemotingConfiguration.GetRegisteredWellKnownServiceTypes().Any(s => s.ObjectType == bootstrapProxyType))
                RemotingConfiguration.RegisterWellKnownServiceType(bootstrapProxyType, "Bootstrap.rem", WellKnownObjectMode.Singleton);
        }
    }
}
