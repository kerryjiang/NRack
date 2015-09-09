using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading;
using AnyLog;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using NDock.Server.Utils;

namespace NDock.Server
{
    public abstract class BootstrapBase : IBootstrap
    {
        protected IConfigSource ConfigSource { get; private set; }

        protected List<IManagedApp> ManagedApps { get; private set; }

        protected ExportProvider ExportProvider { get; private set; }

        protected ILogFactory LogFactory { get; private set; }

        protected ILog Log { get; private set; }

        private Timer m_StatusCollectTimer;

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
                var ret = app.Start();

                if (ret)
                    Log.InfoFormat("The app server instance [{0}] is started successfully.", app.Name);
                else
                    Log.InfoFormat("The app server instance [{0}] failed to start.", app.Name);
            }

            StartStatusCollect();
        }

        public virtual void Stop()
        {
            foreach (var app in ManagedApps)
            {
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
                Logger = LogFactory.GetLog("NDockStatus"),
                PerformanceCounter = new ProcessPerformanceCounter(Process.GetCurrentProcess(), PerformanceCounterInfo.GetDefaultPerformanceCounterDefinitions(), this.ConfigSource.Isolation == IsolationMode.None),
                BootstrapStatus = new AppServerStatus(GetBootstrapMetadata(), new StatusInfoCollection("[Bootstrap]"))
            };

            m_StatusCollectTimer = new Timer(OnStatusCollectTimerCallback, state, interval, interval);
        }

        private void StopStatusCollect()
        {
            m_StatusCollectTimer.Dispose();
            m_StatusCollectTimer = null;
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
                Log.Error("One exception was thrown in OnStatusCollectTimerCallback", e);
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
                Log.Error(result.Message);
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
            AnyLog.LogFactory.Configurate(ExportProvider, ConfigSource.LogFactory);
            var logFactory = AnyLog.LogFactory.Current;

            if (logFactory == null)
                throw new Exception("Failed to load LogFactory.");

            LogFactory = logFactory;
            Log = logFactory.GetLog(this.GetType().Name);

            AppDomain.CurrentDomain.SetData("Bootstrap", this);

            foreach(var config in ConfigSource.Servers)
            {
                IManagedApp server = null;
                
                try
                {
                    server = CreateAppInstance(config);
                    Log.InfoFormat("The app server instance [{0}] is created.", config.Name);
                }
                catch(Exception e)
                {
                    Log.Error(string.Format("Failed to create the app server instance [{0}].", config.Name), e);
                    return false;
                }

                if(server == null)
                {
                    Log.Error(string.Format("Failed to create  the server instance [{0}]", config.Name));
                    return false;
                }

                try
                {
                    if (!Setup(server, config))
                        throw new Exception(string.Format("The app server instance [{0}] failed to be setup.", config.Name));

                    Log.InfoFormat("The app server instance [{0}] is setup successfully.", config.Name);
                }
                catch(Exception e)
                {
                    Log.Error(string.Format("Failed to setup the app server instance [{0}]", config.Name), e);
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
