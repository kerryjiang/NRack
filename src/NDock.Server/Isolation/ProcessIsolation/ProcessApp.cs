using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using NDock.Server.Isolation;
using NDock.Server.Utils;

namespace NDock.Server.Isolation.ProcessIsolation
{
    public class ProcessAppConst : IsolationAppConst
    {
        public const string PortNameTemplate = "{0}[NDock.Worker:{1}]";

        public const string WorkerAssemblyName = "NDock.Worker.exe";

        public const string WorkerUri = "ipc://{0}/{1}";

        public const string WorkerRemoteName = "ManagedAppWorker.rem";
    }

    [StatusInfo(StatusInfoKeys.CpuUsage, Name = "CPU Usage", Format = "{0:0.00}%", DataType = typeof(double), Order = 112)]
    [StatusInfo(StatusInfoKeys.MemoryUsage, Name = "Physical Memory Usage", Format = "{0:N}", DataType = typeof(double), Order = 113)]
    [StatusInfo(StatusInfoKeys.TotalThreadCount, Name = "Total Thread Count", Format = "{0}", DataType = typeof(double), Order = 114)]
    [StatusInfo(StatusInfoKeys.AvailableWorkingThreads, Name = "Available Working Threads", Format = "{0}", DataType = typeof(double), Order = 512)]
    [StatusInfo(StatusInfoKeys.AvailableCompletionPortThreads, Name = "Available Completion Port Threads", Format = "{0}", DataType = typeof(double), Order = 513)]
    [StatusInfo(StatusInfoKeys.MaxWorkingThreads, Name = "Maximum Working Threads", Format = "{0}", DataType = typeof(double), Order = 513)]
    [StatusInfo(StatusInfoKeys.MaxCompletionPortThreads, Name = "Maximum Completion Port Threads", Format = "{0}", DataType = typeof(double), Order = 514)]
    class ProcessApp : IsolationApp
    {
        private Process m_WorkingProcess;

        private string m_ServerTag;

        private ProcessPerformanceCounter m_PerformanceCounter;

        public string ServerTag
        {
            get { return m_ServerTag; }
        }

        private ProcessLocker m_Locker;

        private AutoResetEvent m_ProcessWorkEvent = new AutoResetEvent(false);

        private string m_ProcessWorkStatus = string.Empty;

        public ProcessApp(AppServerMetadata metadata, string startupConfigFile)
            : base(metadata, startupConfigFile)
        {

        }

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

        protected override IManagedAppBase CreateAndStartServerInstance()
        {
            var currentDomain = AppDomain.CurrentDomain;

            m_Locker = new ProcessLocker(AppWorkingDir, "instance.lock");

            var process = m_Locker.GetLockedProcess();

            if (process == null)
            {
                var args = string.Join(" ", (new string[] { Name }).Select(a => "\"" + a + "\"").ToArray());

                ProcessStartInfo startInfo;

                if (!NDock.Base.NDockEnv.IsMono)
                {
                    startInfo = new ProcessStartInfo(ProcessAppConst.WorkerAssemblyName, args);
                }
                else
                {
                    startInfo = new ProcessStartInfo((Path.DirectorySeparatorChar == '\\' ? "mono.exe" : "mono"), "--runtime=v" + System.Environment.Version.ToString(2) + " \"" + ProcessAppConst.WorkerAssemblyName + "\" " + args);
                }

                startInfo.CreateNoWindow = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = currentDomain.BaseDirectory;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardInput = true;

                try
                {
                    m_WorkingProcess = Process.Start(startInfo);
                }
                catch (Exception e)
                {
                    OnExceptionThrown(e);
                    return null;
                }
            }
            else
            {
                m_WorkingProcess = process;
            }

            m_WorkingProcess.EnableRaisingEvents = true;
            m_WorkingProcess.ErrorDataReceived += new DataReceivedEventHandler(m_WorkingProcess_ErrorDataReceived);
            m_WorkingProcess.OutputDataReceived += new DataReceivedEventHandler(m_WorkingProcess_OutputDataReceived);
            m_WorkingProcess.BeginErrorReadLine();
            m_WorkingProcess.BeginOutputReadLine();

            var portName = string.Format(ProcessAppConst.PortNameTemplate, Name, m_WorkingProcess.Id);
            m_ServerTag = portName;

            var remoteUri = string.Format(ProcessAppConst.WorkerUri, portName, ProcessAppConst.WorkerRemoteName);

            IRemoteManagedApp appServer = null;

            if (process == null)
            {
                if (!m_ProcessWorkEvent.WaitOne(10000))
                {
                    ShutdownProcess();
                    OnExceptionThrown(new Exception("The remote work item was timeout to setup!"));
                    return null;
                }

                if (!"Ok".Equals(m_ProcessWorkStatus, StringComparison.OrdinalIgnoreCase))
                {
                    OnExceptionThrown(new Exception("The worker process didn't start successfully!"));
                    return null;
                }

                appServer = GetRemoteServer(remoteUri);

                if (appServer == null)
                    return null;

                var bootstrapIpcPort = AppDomain.CurrentDomain.GetData("BootstrapIpcPort") as string;

                if (string.IsNullOrEmpty(bootstrapIpcPort))
                    throw new Exception("The bootstrap's remoting service has not been started.");

                var ret = false;
                Exception exc = null;

                try
                {
                    //Setup and then start the remote server instance
                    ret = appServer.Setup(GetMetadata().AppType, "ipc://" + bootstrapIpcPort + "/Bootstrap.rem", currentDomain.BaseDirectory, Config, StartupConfigFile);
                }
                catch (Exception e)
                {
                    exc = e;
                }

                if (!ret)
                {
                    ShutdownProcess();
                    OnExceptionThrown(new Exception("The remote work item failed to setup!", exc));
                    return null;
                }

                try
                {
                    ret = appServer.Start();
                }
                catch (Exception e)
                {
                    ret = false;
                    exc = e;
                }

                if (!ret)
                {
                    ShutdownProcess();
                    OnExceptionThrown(new Exception("The remote work item failed to start!", exc));
                    return null;
                }

                m_Locker.SaveLock(m_WorkingProcess);
            }
            else
            {
                appServer = GetRemoteServer(remoteUri);

                if (appServer == null)
                    return null;
            }

            m_WorkingProcess.Exited += new EventHandler(m_WorkingProcess_Exited);

            m_PerformanceCounter = new ProcessPerformanceCounter(m_WorkingProcess, PerformanceCounterInfo.GetDefaultPerformanceCounterDefinitions());

            return appServer;
        }

        IRemoteManagedApp GetRemoteServer(string remoteUri)
        {
            try
            {
                return (IRemoteManagedApp)Activator.GetObject(typeof(IRemoteManagedApp), remoteUri);
            }
            catch(Exception e)
            {
                ShutdownProcess();
                OnExceptionThrown(new Exception("Failed to get server instance of a remote process!", e));
                return null;
            }
        }

        void m_WorkingProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            if (string.IsNullOrEmpty(m_ProcessWorkStatus))
            {
                m_ProcessWorkStatus = e.Data.Trim();
                m_ProcessWorkEvent.Set();
                return;
            }
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

        protected override void OnStopped()
        {
            var unexpectedShutdown = (State == ServerState.Running);

            base.OnStopped();
            m_WorkingProcess.OutputDataReceived -= m_WorkingProcess_OutputDataReceived;
            m_WorkingProcess.ErrorDataReceived -= m_WorkingProcess_ErrorDataReceived;
            m_WorkingProcess = null;
            m_ProcessWorkStatus = string.Empty;

            if (unexpectedShutdown)
            {
                //auto restart if meet a unexpected shutdown
                ((IManagedAppBase)this).Start();
            }
        }

        protected override void Stop()
        {
            ShutdownProcess();
        }

        protected override StatusInfoCollection CollectStatus()
        {
            var app = ManagedApp;

            if (app == null)
                return null;

            var status = app.CollectStatus();

            if (m_PerformanceCounter != null)
                m_PerformanceCounter.Collect(status);

            return status;
        }
    }
}
