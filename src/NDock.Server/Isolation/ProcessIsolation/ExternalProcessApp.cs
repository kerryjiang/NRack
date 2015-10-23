using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using NDock.Server.Utils;

namespace NDock.Server.Isolation.ProcessIsolation
{
    [StatusInfo(StatusInfoKeys.IsRunning, Name = "Is Running", DataType = typeof(bool), Order = 100)]
    [StatusInfo(StatusInfoKeys.CpuUsage, Name = "CPU Usage", Format = "{0:0.00}%", DataType = typeof(double), Order = 112)]
    [StatusInfo(StatusInfoKeys.MemoryUsage, Name = "Physical Memory Usage", Format = "{0:N}", DataType = typeof(double), Order = 113)]
    [StatusInfo(StatusInfoKeys.TotalThreadCount, Name = "Total Thread Count", Format = "{0}", DataType = typeof(double), Order = 114)]
    [StatusInfo(StatusInfoKeys.AvailableWorkingThreads, Name = "Available Working Threads", Format = "{0}", DataType = typeof(double), Order = 512)]
    [StatusInfo(StatusInfoKeys.AvailableCompletionPortThreads, Name = "Available Completion Port Threads", Format = "{0}", DataType = typeof(double), Order = 513)]
    [StatusInfo(StatusInfoKeys.MaxWorkingThreads, Name = "Maximum Working Threads", Format = "{0}", DataType = typeof(double), Order = 513)]
    [StatusInfo(StatusInfoKeys.MaxCompletionPortThreads, Name = "Maximum Completion Port Threads", Format = "{0}", DataType = typeof(double), Order = 514)]
    class ExternalProcessApp : IsolationApp
    {
        private Process m_WorkingProcess;

        private ProcessStartInfo m_StartInfo;

        private ProcessPerformanceCounter m_PerformanceCounter;

        private ProcessLocker m_Locker;

        private StatusInfoCollection m_Status;

        private string m_ExitCommand;

        private string m_ExternalAppDir;

        /// <summary>
        /// Gets the process id.
        /// </summary>
        /// <value>
        /// The process id. If the process id is zero, the server instance is not running
        /// </value>
        public int ProcessId
        {
            get
            {
                if (m_WorkingProcess == null)
                    return 0;

                return m_WorkingProcess.Id;
            }
        }

        public ExternalProcessApp(ExternalProcessAppServerMetadata metadata, string startupConfigFile)
            : base(metadata, startupConfigFile)
        {

        }

        public override bool Setup(IBootstrap bootstrap, IServerConfig config)
        {
            if (!base.Setup(bootstrap, config))
                return false;

            var metadata = GetMetadata() as ExternalProcessAppServerMetadata;

            var appFile = metadata.AppFile;

            if(string.IsNullOrEmpty(appFile))
            {
                OnExceptionThrown(new ArgumentNullException("appFile"));
                return false;
            }

            var workDir = AppWorkingDir;

            if (!string.IsNullOrEmpty(metadata.AppDir))
                appFile = Path.Combine(metadata.AppDir, appFile);

            if(!Path.IsPathRooted(appFile))
            {
                appFile = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, appFile));
            }

            if (!File.Exists(appFile))
            {
                OnExceptionThrown(new FileNotFoundException("The app file was not found.", appFile));
                return false;
            }

            workDir = Path.GetDirectoryName(appFile);

            m_ExternalAppDir = workDir;

            var args = metadata.AppArgs;

            var startInfo = new ProcessStartInfo(appFile, args);
            startInfo.WorkingDirectory = workDir;
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;

            m_StartInfo = startInfo;

            m_ExitCommand = config.Options.Get("exitCommand");

            m_Status = new StatusInfoCollection { Name = config.Name };

            return true;
        }

        protected override AppAssemblyUpdateState GetAppAssemblyUpdateState()
        {
            return new AppAssemblyUpdateState(m_ExternalAppDir);
        }

        protected override void OnStopped()
        {
            var unexpectedShutdown = (State == ServerState.Running);

            base.OnStopped();

            m_WorkingProcess.ErrorDataReceived -= m_WorkingProcess_ErrorDataReceived;
            m_WorkingProcess = null;

            if (unexpectedShutdown)
            {
                //auto restart if meet a unexpected shutdown
                ((IManagedAppBase)this).Start();
            }
        }

        protected override StatusInfoCollection CollectStatus()
        {
            var status = m_Status;

            status.CollectedTime = DateTime.Now;

            if (m_PerformanceCounter != null)
                m_PerformanceCounter.Collect(status);

            return status;
        }

        protected override IManagedAppBase CreateAndStartServerInstance()
        {
            var currentDomain = AppDomain.CurrentDomain;

            m_Locker = new ProcessLocker(AppWorkingDir, "instance.lock");

            var process = m_Locker.GetLockedProcess();

            if (process == null)
            {
                try
                {
                    m_WorkingProcess = Process.Start(m_StartInfo);
                }
                catch (Exception e)
                {
                    OnExceptionThrown(e);
                    return null;
                }

                m_Locker.SaveLock(process);
            }
            else
            {
                m_WorkingProcess = process;
            }

            m_WorkingProcess.EnableRaisingEvents = true;
            m_WorkingProcess.ErrorDataReceived += new DataReceivedEventHandler(m_WorkingProcess_ErrorDataReceived);
            m_WorkingProcess.BeginErrorReadLine();

            m_WorkingProcess.Exited += new EventHandler(m_WorkingProcess_Exited);

            m_PerformanceCounter = new ProcessPerformanceCounter(m_WorkingProcess, PerformanceCounterInfo.GetDefaultPerformanceCounterDefinitions());

            m_Status.StartedTime = DateTime.Now;
            m_Status[StatusInfoKeys.IsRunning] = true;

            return new NullManagedApp();
        }

        void m_WorkingProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            OnExceptionThrown(new Exception(e.Data));
        }

        void m_WorkingProcess_Exited(object sender, EventArgs e)
        {
            m_Locker.CleanLock();
            m_PerformanceCounter = null;
            OnStopped();
        }

        private void ShutdownProcess()
        {
            if (m_WorkingProcess != null)
            {
                try
                {
                    m_WorkingProcess.Kill();
                }
                catch
                {

                }
            }
        }

        protected override void Stop()
        {
            var process = m_WorkingProcess;

            if (process == null)
                return;

            if(string.IsNullOrEmpty(m_ExitCommand))
            {
                ShutdownProcess();
                return;
            }

            var appConsole = process.StandardInput;

            appConsole.Write(m_ExitCommand);
            appConsole.Flush();
        }

        class NullManagedApp : IManagedAppBase
        {
            public bool CanBeRecycled()
            {
                return true;
            }

            public StatusInfoCollection CollectStatus()
            {
                throw new NotSupportedException();
            }

            public AppServerMetadata GetMetadata()
            {
                throw new NotSupportedException();
            }

            public bool Start()
            {
                throw new NotSupportedException();
            }

            public void Stop()
            {
                //Do nothing
            }
        }
    }
}
