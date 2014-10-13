using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;
using AnyLog;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace NDock.Base
{
    public abstract class AppServer : IAppServer
    {
        private CompositionContainer m_CompositionContainer;

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
                m_CompositionContainer.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                throw new Exception("MEF composite exception!", compositionException);
            }
        }

        protected ILogFactory LogFactory { get; private set; }

        protected ILog Logger { get; private set; }

        void SetupLogFactory(IServerConfig config)
        {
            if(!string.IsNullOrEmpty(config.LogFactory))
            {
                var lazyLogFactory = m_LogFactories.FirstOrDefault(l =>
                    l.Metadata.Name.Equals(config.LogFactory, StringComparison.OrdinalIgnoreCase));

                if (lazyLogFactory == null)
                    throw new Exception(string.Format("Cannot find the specific log factory: [{0}]!", config.LogFactory));

                LogFactory = lazyLogFactory.Value;
            }
            else
            {
                var lazyLogFactory = m_LogFactories.FirstOrDefault();

                if(lazyLogFactory == null)
                    throw new Exception("No available LogFactories have been found!");

                LogFactory = lazyLogFactory.Value;
            }
        }

        bool IWorkItem.Setup(IServerConfig config, IServiceProvider serviceProvider)
        {
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
