using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;
using AnyLog;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using NDock.Base.CompositeTargtes;

namespace NDock.Base
{
    public abstract class AppServer : IAppServer
    {
        private CompositionContainer m_CompositionContainer;

        public IServerConfig Config { get; private set; }

        protected virtual void RegisterCompositeTarget(IList<ICompositeTarget> targets)
        {
            targets.Add(new LogFactoryCompositeTarget());
        }

        private void Composite(IServerConfig config)
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(IAppServer).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.dll"));

            //Create the CompositionContainer with the parts in the catalog
            m_CompositionContainer = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                var targets = new List<ICompositeTarget>();
                RegisterCompositeTarget(targets);

                if (targets.Any())
                {
                    targets.ForEach(t => m_CompositionContainer.ComposeParts(targets.OfType<object>().ToArray()));
                }
            }
            catch (CompositionException compositionException)
            {
                throw new Exception("MEF composite exception!", compositionException);
            }
        }

        protected internal ILogFactory LogFactory { get; internal set; }

        protected ILog Logger { get; private set; }

        public IAppEndPoint EndPoint { get; private set; }

        public IMessageBus MessageBus { get; private set; }

        void SetupLogFactory(IServerConfig config)
        {
            Lazy<ILogFactory, ILogFactoryMetadata> lazyLogFactory;

            if(!string.IsNullOrEmpty(config.LogFactory))
            {
                lazyLogFactory = m_LogFactories.FirstOrDefault(l =>
                    l.Metadata.Name.Equals(config.LogFactory, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                lazyLogFactory = m_LogFactories.FirstOrDefault();
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

            LogFactory = logFactory;
        }

        bool IWorkItem.Setup(IServerConfig config, IServiceProvider serviceProvider)
        {
            Config = config;
            Composite(config);

            // setup logfactory at first
            SetupLogFactory(config);

            // initialize default loggger
            Logger = LogFactory.GetLog(config.Name);

            return false;
        }

        protected abstract bool Setup(IServerConfig config, IServiceProvider serviceProvider);

        [ImportMany]
        private IEnumerable<Lazy<ILogFactory, ILogFactoryMetadata>> m_LogFactories = null;

        #region the code about starting

        protected virtual void OnPreStart()
        {

        }

        protected virtual void OnStarted()
        {

        }

        bool IWorkItem.Start()
        {
            OnPreStart();

            if (!Start())
                return false;

            OnStarted();
            return true;
        }

        public abstract bool Start();

        #endregion

        #region the code about stoppping

        protected virtual void OnPreStop()
        {
            
        }

        void IWorkItem.Stop()
        {
            OnPreStop();

            //TODO: Actual stopping code

            OnStopped();
        }

        public abstract void Stop();

        protected virtual void OnStopped()
        {

        }

        #endregion
    }
}
