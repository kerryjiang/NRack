using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using NDock.Server.Isolation;
using System.Diagnostics;

namespace NDock.Server.Isolation.AppDomainIsolation
{
    [StatusInfo(StatusInfoKeys.CpuUsage, Name = "CPU Usage", Format = "{0:0.00}%", DataType = typeof(double), Order = 112)]
    [StatusInfo(StatusInfoKeys.MemoryUsage, Name = "Memory Usage", Format = "{0:N}", DataType = typeof(double), Order = 113)]
    class AppDomainApp : IsolationApp
    {
        private AppDomain m_HostDomain;

        private readonly static bool m_AppDomainMonitoringSupported = false;

        static AppDomainApp()
        {
            try
            {
                AppDomain.MonitoringIsEnabled = true;
                m_AppDomainMonitoringSupported = true;
            }
            catch (NotImplementedException)
            {
                return;
            }
        }

        public AppDomainApp(AppServerMetadata metadata, string startupConfigFile)
            : base(metadata, startupConfigFile)
        {

        }

        private AppDomain CreateHostAppDomain()
        {
            var currentDomain = AppDomain.CurrentDomain;

            var startupConfigFile = StartupConfigFile;

            if (!string.IsNullOrEmpty(startupConfigFile))
            {
                if (!Path.IsPathRooted(startupConfigFile))
                    startupConfigFile = Path.Combine(currentDomain.BaseDirectory, startupConfigFile);
            }

            var setupInfo = new AppDomainSetup
            {
                ApplicationName = Name,
                ApplicationBase = AppWorkingDir,
                ConfigurationFile = startupConfigFile,
                ShadowCopyFiles = "true",
                CachePath = Path.Combine(currentDomain.BaseDirectory, IsolationAppConst.ShadowCopyDir)
            };

            var hostAppDomain = AppDomain.CreateDomain(Name, currentDomain.Evidence, setupInfo);

            var assemblyImportType = typeof(AssemblyImport);

            hostAppDomain.CreateInstanceFrom(assemblyImportType.Assembly.CodeBase,
                assemblyImportType.FullName,
                true,
                BindingFlags.CreateInstance,
                null,
                new object[] { currentDomain.BaseDirectory },
                null,
                new object[0]);

            return hostAppDomain;
        }

        protected override IManagedAppBase CreateAndStartServerInstance()
        {
            IManagedApp appServer;

            try
            {
                m_HostDomain = CreateHostAppDomain();

                m_HostDomain.SetData(typeof(IsolationMode).Name, IsolationMode.AppDomain);

                var marshalServerType = typeof(MarshalManagedApp);

                appServer = (IManagedApp)m_HostDomain.CreateInstanceAndUnwrap(marshalServerType.Assembly.FullName,
                    marshalServerType.FullName,
                    true,
                    BindingFlags.CreateInstance,
                    null,
                    new object[] { GetMetadata().AppType },
                    null,
                    new object[0]);

                if (!appServer.Setup(Bootstrap, Config))
                {
                    OnExceptionThrown(new Exception("Failed to setup MarshalManagedApp"));
                    return null;
                }

                if (!appServer.Start())
                {
                    OnExceptionThrown(new Exception("Failed to start MarshalManagedApp"));
                    return null;
                }

                m_HostDomain.DomainUnload += new EventHandler(m_HostDomain_DomainUnload);

                return appServer;
            }
            catch (Exception e)
            {
                if (m_HostDomain != null)
                {
                    AppDomain.Unload(m_HostDomain);
                    m_HostDomain = null;
                }

                OnExceptionThrown(e);
                return null;
            }
        }

        protected override void Stop()
        {
            if (m_HostDomain != null)
            {
                AppDomain.Unload(m_HostDomain);
            }
        }


        void m_HostDomain_DomainUnload(object sender, EventArgs e)
        {
            m_HostDomain = null;
            OnStopped();
        }

        protected override StatusInfoCollection CollectStatus()
        {
            var app = ManagedApp;

            if (app == null)
                return null;

            var status = app.CollectStatus();

            if(m_AppDomainMonitoringSupported)
            {
                status[StatusInfoKeys.MemoryUsage] = m_HostDomain == null ? 0 : m_HostDomain.MonitoringSurvivedMemorySize;

                var process = Process.GetCurrentProcess();
                var value = m_HostDomain.MonitoringTotalProcessorTime.TotalMilliseconds * 100 / process.TotalProcessorTime.TotalMilliseconds;
                status[StatusInfoKeys.CpuUsage] = value;
            }

            return status;
        }
    }
}
