using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using AnyLog;

namespace NDock.Base.CompositeTargtes
{
    class LogFactoryCompositeTarget : ICompositeTarget
    {
        public bool Resolve(IAppServer appServer, ExportProvider exportProvider)
        {
            var server = appServer as AppServer;
            var config = server.Config;

            var logFactories = exportProvider.GetExports<ILogFactory, ILogFactoryMetadata>();

            Lazy<ILogFactory, ILogFactoryMetadata> lazyLogFactory;

            if (!string.IsNullOrEmpty(config.LogFactory))
            {
                lazyLogFactory = logFactories.FirstOrDefault(l =>
                    l.Metadata.Name.Equals(config.LogFactory, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                lazyLogFactory = logFactories.FirstOrDefault();
            }

            if (lazyLogFactory == null)
                throw new Exception("No available LogFacotry has been found!");

            var logFactory = lazyLogFactory.Value;

            var metadata = lazyLogFactory.Metadata;

            var currentAppDomain = AppDomain.CurrentDomain;
            var isolation = IsolationMode.None;

            var isolationValue = currentAppDomain.GetData(typeof(IsolationMode).Name);

            if (isolationValue != null)
                isolation = (IsolationMode)isolationValue;

            var configFileName = metadata.ConfigFileName;

            if (Path.DirectorySeparatorChar != '\\')
            {
                configFileName = Path.GetFileNameWithoutExtension(configFileName) + ".unix" + Path.GetExtension(configFileName);
            }

            var configFiles = new List<string>();

            if (isolation == IsolationMode.None)
            {
                configFiles.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName));
                configFiles.Add(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), configFileName));
            }
            else //The running AppServer is in isolated appdomain
            {
                configFiles.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFileName));
                configFiles.Add(Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), configFileName));

                //go to the application's root
                //the appdomain's root is /WorkingDir/DomainName, so get parent path twice to reach the application root
                var rootDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;

                configFiles.Add(Path.Combine(rootDir, AppDomain.CurrentDomain.FriendlyName + "." + configFileName));
                configFiles.Add(Path.Combine(Path.Combine(rootDir, "Config"), AppDomain.CurrentDomain.FriendlyName + "." + configFileName));
                configFiles.Add(Path.Combine(rootDir, configFileName));
                configFiles.Add(Path.Combine(Path.Combine(rootDir, "Config"), configFileName));
            }

            if (!logFactory.Initialize(configFiles.ToArray()))
            {
                throw new Exception("Failed to initialize the logfactory:" + metadata.Name);
            }

            server.LogFactory = logFactory;
            server.Logger = logFactory.GetLog(server.Name);
            return true;
        }
    }
}
