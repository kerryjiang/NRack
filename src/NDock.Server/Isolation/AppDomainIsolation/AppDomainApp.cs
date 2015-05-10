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

namespace NDock.Server.Isolation.AppDomainIsolation
{
    class AppDomainApp : IsolationApp
    {
        private AppDomain m_HostDomain;

        private string m_StartupConfigFile;

        public AppDomainApp(AppServerMetadataAttribute metadata, string startupConfigFile)
            : base(metadata)
        {
            m_StartupConfigFile = startupConfigFile;
        }

        private AppDomain CreateHostAppDomain()
        {
            var currentDomain = AppDomain.CurrentDomain;

            var workingDir = Path.Combine(Path.Combine(currentDomain.BaseDirectory, WorkingDir), Name);

            if (!Directory.Exists(workingDir))
                Directory.CreateDirectory(workingDir);

            var startupConfigFile = m_StartupConfigFile;

            if (!string.IsNullOrEmpty(startupConfigFile))
            {
                if (!Path.IsPathRooted(startupConfigFile))
                    startupConfigFile = Path.Combine(currentDomain.BaseDirectory, startupConfigFile);
            }

            var hostAppDomain = AppDomain.CreateDomain(Name, currentDomain.Evidence, new AppDomainSetup
                {
                    ApplicationName = Name,
                    ApplicationBase = workingDir,
                    ConfigurationFile = startupConfigFile
                });

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

        protected override IManagedApp CreateAndStartServerInstance()
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

                if (!appServer.Setup(Config))
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
    }
}
