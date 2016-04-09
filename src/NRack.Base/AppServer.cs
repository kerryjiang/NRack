using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyLog;
using NRack.Base.CompositeTargets;
using NRack.Base.Config;
using NRack.Base.Metadata;

namespace NRack.Base
{
    public abstract partial class AppServer : IAppServer
    {
        public string Name { get; private set; }

        protected CompositionContainer CompositionContainer { get; private set; }

        public IServerConfig Config { get; private set; }

        protected IBootstrap Bootstrap { get; private set; }

        public DateTime StartedTime { get; private set; }

        private StatusInfoCollection m_AppStatus;

        protected virtual void RegisterCompositeTarget(IList<ICompositeTarget> targets)
        {
            targets.Add(new LoggerFactoryCompositeTarget((value) =>
            {
                LoggerFactory = value;
                Logger = value.GetLogger(Name);
            }));
        }

        protected virtual CompositionContainer GetCompositionContainer(IServerConfig config)
        {
            return AppDomain.CurrentDomain.GetCurrentAppDomainExportProvider();
        }

        private bool Composite(IServerConfig config)
        {
            CompositionContainer = GetCompositionContainer(config);

            //Fill the imports of this object
            try
            {
                var targets = new List<ICompositeTarget>();
                RegisterCompositeTarget(targets);

                if (targets.Any())
                {
                    foreach (var t in targets)
                    {
                        if(!t.Resolve(this, CompositionContainer))
                        {
                            throw new Exception("Failed to resolve the instance of the type: " + t.GetType().FullName);
                        }
                    }
                }

                return true;
            }
            catch(Exception e)
            {
                var logger = Logger;

                if (logger == null)
                    throw e;

                logger.Error("Composition error", e);
                return false;
            }
        }

        public ILoggerFactory LoggerFactory { get; private set; }

        public ILog Logger { get; private set; }

        public ServerState State { get; private set; }

        public IAppEndPoint EndPoint { get; private set; }

        public IMessageBus MessageBus { get; private set; }


        private AppServerMetadata m_AppMetadata;

        public virtual AppServerMetadata GetMetadata()
        {
            if(m_AppMetadata == null)
                m_AppMetadata = AppServerMetadata.GetAppServerMetadata(this.GetType());

            return m_AppMetadata;
        }

        bool IManagedApp.Setup(IBootstrap bootstrap, IServerConfig config)
        {
            Bootstrap = Bootstrap;

            var initialized = false;
            State = ServerState.Initializing;

            try
            {
                if (config == null)
                    throw new ArgumentNullException("config");

                if (!string.IsNullOrEmpty(config.Name))
                    Name = config.Name;
                else
                    Name = string.Format("{0}-{1}", this.GetType().Name, Math.Abs(this.GetHashCode()));

                Config = config;

                if (!Composite(config))
                    return false;

                initialized = Setup(config, CompositionContainer);
                return initialized;
            }
            finally
            {
                if (initialized)
                    State = ServerState.NotStarted;
                else
                    State = ServerState.NotInitialized;
            }
        }

        protected virtual bool Setup(IServerConfig config, ExportProvider exportProvider)
        {
            return true;
        }

        public virtual bool CanBeRecycled()
        {
            return true;
        }

        #region the code about starting

        protected virtual void OnPreStart()
        {

        }

        protected virtual void OnStarted()
        {

        }

        bool IManagedAppBase.Start()
        {
            var started = false;

            try
            {
                State = ServerState.Starting;

                OnPreStart();

                if (!Start())
                    return false;

                OnStarted();
                started = true;
                StartedTime = DateTime.Now;
                return true;
            }
            finally
            {
                State = started ? ServerState.Running : ServerState.NotStarted;
            }
        }

        public abstract bool Start();

        #endregion

        #region the code about stoppping

        protected virtual void OnPreStop()
        {
            
        }

        void IManagedAppBase.Stop()
        {
            var stoppped = false;

            try
            {
                State = ServerState.Stopping;

                OnPreStop();

                Stop();

                OnStopped();
                stoppped = true;
            }
            finally
            {
                State = stoppped ? ServerState.NotStarted : ServerState.Running;
            }
        }

        public abstract void Stop();

        protected virtual void OnStopped()
        {

        }

        #endregion


        StatusInfoCollection IManagedAppBase.CollectStatus()
        {
            var status = m_AppStatus;

            if (status == null)
            {
                status = new StatusInfoCollection();
                status.Name = Name;
                status.Tag = Name;
                status.StartedTime = StartedTime;
                m_AppStatus = status;
            }

            UpdateStatus(status);
            Task.Factory.StartNew(() => OnStatusCollected(status)).ContinueWith(t =>
                {
                    Logger.LogAggregateException("Exception happend in OnStatusCollected.", t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            return status;
        }

        protected virtual void UpdateStatus(StatusInfoCollection status)
        {
            status[StatusInfoKeys.IsRunning] = (State == ServerState.Running);
        }

        /// <summary>
        /// Called when [status collected].
        /// </summary>
        /// <param name="status">The app status.</param>
        protected virtual void OnStatusCollected(StatusInfoCollection status)
        {

        }

        /// <summary>
        /// Gets the physical file path by the relative file path,
        /// search both in the appserver's root and in the NRack root dir if the isolation level has been set other than 'None'.
        /// </summary>
        /// <param name="relativeFilePath">The relative file path.</param>
        /// <returns></returns>
        public string GetFilePath(string relativeFilePath)
        {
            var currentAppDomain = AppDomain.CurrentDomain;

            var filePath = Path.Combine(currentAppDomain.BaseDirectory, relativeFilePath);

            if (File.Exists(filePath))
                return filePath;

            var isolationValue = currentAppDomain.GetData(typeof(IsolationMode).Name);

            if (isolationValue == null || ((IsolationMode)isolationValue) == IsolationMode.None)
                return filePath;

            var rootDir = Directory.GetParent(currentAppDomain.BaseDirectory).Parent.FullName;
            var rootFilePath = Path.Combine(rootDir, relativeFilePath);

            if (File.Exists(rootFilePath))
                return rootFilePath;

            return filePath;
        }
    }
}
