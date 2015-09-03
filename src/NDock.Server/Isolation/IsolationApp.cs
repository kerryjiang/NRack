using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Configuration;
using NDock.Base.Metadata;
using NDock.Server.Recycle;

namespace NDock.Server.Isolation
{
    public class IsolationAppConst
    {
        public const string WorkingDir = "AppRoot";

        public const string AppConfigFile = "App.config";
    }

    abstract class IsolationApp : MarshalByRefObject, IManagedApp
    {
        private AppServerMetadata m_Metadata;

        protected IManagedAppBase ManagedApp { get; private set; }

        protected IBootstrap Bootstrap { get; private set; }

        protected string StartupConfigFile { get; private set; }

        protected string AppWorkingDir { get; private set; }

        internal static string GetAppWorkingDir(string appName)
        {
            return Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IsolationAppConst.WorkingDir), appName);
        }

        protected IsolationApp(AppServerMetadata metadata, string startupConfigFile)
        {
            State = ServerState.NotInitialized;
            m_Metadata = metadata;

            var isolationStatusFields = StatusInfoAttribute.GetFromType(this.GetType());

            if(isolationStatusFields.Any())
            {
                m_Metadata.StatusFields = m_Metadata.StatusFields.Union(isolationStatusFields).ToArray();
            }

            StartupConfigFile = startupConfigFile;
        }

        public string Name { get; private set; }

        protected string GetAppConfigFile()
        {
            var configFile = Config.Options.GetValue("configFile");

            if (string.IsNullOrEmpty(configFile))
                configFile = IsolationAppConst.AppConfigFile;
            else if (Path.IsPathRooted(configFile))
                return configFile;

            configFile = Path.Combine(AppWorkingDir, configFile);

            if (!File.Exists(configFile))
                return string.Empty;

            return configFile;
        }

        public virtual bool Setup(IBootstrap bootstrap, IServerConfig config)
        {
            Bootstrap = bootstrap;
            State = ServerState.Initializing;
            Config = config;
            Name = config.Name;            
            State = ServerState.NotStarted;

            AppWorkingDir = GetAppWorkingDir(Name);

            if (!Directory.Exists(AppWorkingDir))
                Directory.CreateDirectory(AppWorkingDir);

            var appConfigFilePath = GetAppConfigFile();

            // use the application's own config file if it has
            //AppRoot\AppName\App.config
            if (!string.IsNullOrEmpty(appConfigFilePath))
                StartupConfigFile = appConfigFilePath;

            return true;
        }

        public IServerConfig Config { get; private set; }

        public AppServerMetadata GetMetadata()
        {
            return m_Metadata;
        }

        protected abstract IManagedAppBase CreateAndStartServerInstance();

        public bool Start()
        {
            State = ServerState.Starting;

            ManagedApp = CreateAndStartServerInstance();

            if (ManagedApp != null)
            {
                State = ServerState.Running;
                return true;
            }
            else
            {
                State = ServerState.NotStarted;
                return false;
            }
        }

        void IManagedAppBase.Stop()
        {
            var app = ManagedApp;

            if (app == null)
                return;

            State = ServerState.Stopping;
            app.Stop();

            m_StopTaskSrc = new TaskCompletionSource<bool>();

            var stopTask = m_StopTaskSrc.Task;

            Stop();

            stopTask.Wait();
        }

        private TaskCompletionSource<bool> m_StopTaskSrc;

        protected virtual void OnStopped()
        {
            State = ServerState.NotStarted;
            ManagedApp = null;
            m_StopTaskSrc.SetResult(true);
        }

        protected abstract void Stop();

        public ServerState State { get; protected set; }

        public event EventHandler<Base.ErrorEventArgs> ExceptionThrown;

        protected void OnExceptionThrown(Exception exc)
        {
            var handler = ExceptionThrown;

            if (handler == null)
                return;

            handler(this, new Base.ErrorEventArgs(exc));
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns>
        /// An object of type <see cref="T:System.Runtime.Remoting.Lifetime.ILease" /> used to control the lifetime policy for this instance. This is the current lifetime service object for this instance if one exists; otherwise, a new lifetime service object initialized to the value of the <see cref="P:System.Runtime.Remoting.Lifetime.LifetimeServices.LeaseManagerPollTime" /> property.
        /// </returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="RemotingConfiguration, Infrastructure" />
        /// </PermissionSet>
        public override object InitializeLifetimeService()
        {
            return null;
        }


        public bool CanBeRecycled()
        {
            var app = ManagedApp;

            if (app == null)
                return false;

            return app.CanBeRecycled();
        }

        public IRecycleTrigger[] RecycleTriggers { get; internal set; }

        StatusInfoCollection IManagedAppBase.CollectStatus()
        {
            var status = CollectStatus();
            RunRecycleTriggers(status);
            return status;
        }

        void RunRecycleTriggers(StatusInfoCollection status)
        {
            var triggers = RecycleTriggers;

            if (triggers == null || !triggers.Any())
                return;

            foreach (var trigger in triggers)
            {
                var toBeRecycle = false;

                try
                {
                    toBeRecycle = trigger.NeedBeRecycled(this, status) && CanBeRecycled();
                }
                catch (Exception e)
                {
                    OnExceptionThrown(e);
                    continue;
                }

                if (toBeRecycle)
                {
                    Task.Factory.StartNew(Restart)
                        .ContinueWith(t => OnExceptionThrown(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
                }
            }
        }

        private void Restart()
        {
            Stop();
            Start();
        }

        protected abstract StatusInfoCollection CollectStatus();
    }
}
