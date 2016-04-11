using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using AnyLog;
using NRack.Base;
using NRack.Base.Config;
using NRack.Base.Configuration;
using NRack.Base.Metadata;
using NRack.Server.Utils;

namespace NRack.Server
{
    public abstract class BootstrapBase : IBootstrap, ILoggerProvider, ILoggerFactoryProvider
    {
        protected IConfigSource ConfigSource { get; private set; }

        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        /// <value>
        /// The configuration file path.
        /// </value>
        public string ConfigFilePath { get; private set; }

        protected List<IManagedApp> ManagedApps { get; private set; }

        protected ExportProvider ExportProvider { get; private set; }

        protected ILoggerFactory LoggerFactory { get; private set; }

        ILoggerFactory ILoggerFactoryProvider.LoggerFactory
        {
            get
            {
                return this.LoggerFactory;
            }
        }

        protected ILog Logger { get; private set; }

        ILog ILoggerProvider.Logger
        {
            get
            {
                return this.Logger;
            }
        }

        private Timer m_StatusCollectTimer;

        public BootstrapBase(IConfigSource configSource)
        {
            if (configSource == null)
                throw new ArgumentNullException("configSource");

            HandleConfigSource(configSource);

            ConfigSource = configSource;
            ManagedApps = new List<IManagedApp>();
            ExportProvider = CreateExportProvider();

            
        }

        protected void HandleConfigSource(IConfigSource configSource)
        {
            var configSection = configSource as ConfigurationSection;

            if (configSection != null)
            {
                ConfigFilePath = configSection.GetConfigFilePath();
                ConfigurationWatcher.Watch(configSection, this);
            }
        }

        protected virtual CompositionContainer CreateExportProvider()
        {
            return AppDomain.CurrentDomain.GetCurrentAppDomainExportProvider();
        }

        public virtual void Start()
        {
            foreach (var app in ManagedApps.Where(s => s.Config.StartupType == StartupType.Automatic))
            {
                var ret = app.Start();

                if (ret)
                    Logger.InfoFormat("The app server instance [{0}] is started successfully.", app.Name);
                else
                    Logger.InfoFormat("The app server instance [{0}] failed to start.", app.Name);
            }

            StartStatusCollect();
        }

        public virtual void Stop()
        {
            foreach (var app in ManagedApps)
            {
                if(app.State == ServerState.Running)
                    app.Stop();
            }

            StopStatusCollect();
        }

        #region status collect

        protected virtual AppServerMetadata GetBootstrapMetadata()
        {
            var metadata = new AppServerMetadata();
            metadata.Name = "[Bootstrap]";
            metadata.StatusFields = StatusInfoAttribute.GetFromType(this.GetType()).ToArray();
            return metadata;
        }

        private void StartStatusCollect()
        {
            var interval = this.ConfigSource.StatusCollectInterval;

            if(interval == 0)
                interval = 60;

            interval = interval * 1000;

            var state = new StatusCollectState
            {
                Interval = interval,
                Collector = ExportProvider.GetExport<IStatusCollector>().Value,
                Logger = LoggerFactory.GetLogger("NRackStatus"),
                PerformanceCounter = new ProcessPerformanceCounter(Process.GetCurrentProcess(), PerformanceCounterInfo.GetDefaultPerformanceCounterDefinitions(), this.ConfigSource.Isolation == IsolationMode.None),
                BootstrapStatus = new AppServerStatus(GetBootstrapMetadata(), new StatusInfoCollection("[Bootstrap]"))
            };

            m_StatusCollectTimer = new Timer(OnStatusCollectTimerCallback, state, interval, interval);
        }

        private void StopStatusCollect()
        {
            var timer = m_StatusCollectTimer;

            if (timer != null)
            {
                if(Interlocked.CompareExchange(ref m_StatusCollectTimer, null, timer) == timer)
                {
                    timer.Dispose();
                }
            }
        }

        private void OnStatusCollectTimerCallback(object state)
        {
            var collectState = state as StatusCollectState;
            var collector = collectState.Collector;

            m_StatusCollectTimer.Change(Timeout.Infinite, Timeout.Infinite);

            try
            {
                var statusList = new List<AppServerStatus>();

                foreach(var app in ManagedApps)
                {
                    var meta = app.GetMetadata();
                    var appStatus = app.CollectStatus();

                    statusList.Add(new AppServerStatus(meta, appStatus));
                }

                collectState.PerformanceCounter.Collect(collectState.BootstrapStatus.DataCollection);
                collector.Collect(collectState.BootstrapStatus, statusList, collectState.Logger);
            }
            catch(Exception e)
            {
                Logger.Error("One exception was thrown in OnStatusCollectTimerCallback", e);
            }

            int interval = collectState.Interval;
            m_StatusCollectTimer.Change(interval, interval);
        }

        #endregion

        protected virtual AppServerMetadata GetAppServerMetadata(IServerConfig serverConfig)
        {
            var typeValidator = new RemoteAppTypeValidator();

            var result = typeValidator.GetServerMetadata(serverConfig.Type);

            if (!result.Result)
            {
                Logger.Error(result.Message);
                return null;
            }

            return result.Value;
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
            AnyLog.LoggerFactory.Configurate(ExportProvider, ConfigSource.LogFactory);
            var loggerFactory = AnyLog.LoggerFactory.Current;

            if (loggerFactory == null)
                throw new Exception("Failed to load loggerFactory.");

            LoggerFactory = loggerFactory;
            Logger = loggerFactory.GetLogger(this.GetType().Name);

            AppDomain.CurrentDomain.SetData("Bootstrap", this);

            foreach(var config in ConfigSource.Servers.Where(s => s.StartupType != StartupType.Disabled))
            {
                IManagedApp server = null;
                
                try
                {
                    server = CreateAppInstance(config);
                    Logger.InfoFormat("The app server instance [{0}] is created.", config.Name);
                }
                catch(Exception e)
                {
                    Logger.Error(string.Format("Failed to create the app server instance [{0}].", config.Name), e);
                    return false;
                }

                if(server == null)
                {
                    Logger.Error(string.Format("Failed to create  the server instance [{0}]", config.Name));
                    return false;
                }

                try
                {
                    if (!Setup(server, config))
                        throw new Exception(string.Format("The app server instance [{0}] failed to be setup.", config.Name));

                    Logger.InfoFormat("The app server instance [{0}] is setup successfully.", config.Name);
                }
                catch(Exception e)
                {
                    Logger.Error(string.Format("Failed to setup the app server instance [{0}]", config.Name), e);
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
                if (Logger.IsErrorEnabled)
                    Logger.Error("Failed to register remoting access service!", e);

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
            var bootstrapIpcPort = string.Format("NRack.Bootstrap[{0}]", Math.Abs(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar).GetHashCode()));

            var serverIpcChannelName = "BootstrapIpc";

            var serverChannel = ChannelServices.RegisteredChannels.FirstOrDefault(c => c.ChannelName == serverIpcChannelName);

            if (serverChannel != null)
                ChannelServices.UnregisterChannel(serverChannel);

            serverChannel = new IpcServerChannel(serverIpcChannelName, bootstrapIpcPort, new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full });
            ChannelServices.RegisterChannel(serverChannel, false);

            if (ConfigSource.TcpRemotingPort > 0)
            {
                var serverTcpChannelName = "BootstrapTcp";

                serverChannel = ChannelServices.RegisteredChannels.FirstOrDefault(c => c.ChannelName == serverTcpChannelName);

                if (serverChannel != null)
                    ChannelServices.UnregisterChannel(serverChannel);

                serverChannel = new TcpServerChannel(serverTcpChannelName, ConfigSource.TcpRemotingPort, new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full });
                ChannelServices.RegisterChannel(serverChannel, false);
            }            

            AppDomain.CurrentDomain.SetData("BootstrapIpcPort", bootstrapIpcPort);

            var bootstrapProxyType = typeof(RemoteBootstrapProxy);

            if (!RemotingConfiguration.GetRegisteredWellKnownServiceTypes().Any(s => s.ObjectType == bootstrapProxyType))
                RemotingConfiguration.RegisterWellKnownServiceType(bootstrapProxyType, "Bootstrap.rem", WellKnownObjectMode.Singleton);
        }

        class StatusCollectState
        {
            public int Interval { get; set; }

            public IStatusCollector Collector { get; set; }

            public ILog Logger { get; set; }

            public ProcessPerformanceCounter PerformanceCounter { get; set; }

            public AppServerStatus BootstrapStatus { get; set; }
        }
    }
}
