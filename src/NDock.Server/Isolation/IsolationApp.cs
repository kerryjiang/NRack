using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using NDock.Server.Recycle;

namespace NDock.Server.Isolation
{
    abstract class IsolationApp : MarshalByRefObject, IManagedApp
    {
        private AppServerMetadata m_Metadata;

        protected IManagedAppBase ManagedApp { get; private set; }

        protected const string WorkingDir = "AppRoot";

        protected IBootstrap Bootstrap { get; private set; }

        protected IsolationApp(AppServerMetadata metadata)
        {
            State = ServerState.NotInitialized;
            m_Metadata = metadata;
        }

        public string Name { get; private set; }

        public bool Setup(IBootstrap bootstrap, IServerConfig config)
        {
            Bootstrap = bootstrap;
            State = ServerState.Initializing;
            Config = config;
            Name = config.Name;
            State = ServerState.NotStarted;
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

        public event EventHandler<ErrorEventArgs> ExceptionThrown;

        protected void OnExceptionThrown(Exception exc)
        {
            var handler = ExceptionThrown;

            if (handler == null)
                return;

            handler(this, new ErrorEventArgs(exc));
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

        public abstract StatusInfoCollection CollectStatus();
    }
}
