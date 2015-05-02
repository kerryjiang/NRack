using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;
using AnyLog;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using NDock.Base.CompositeTargets;
using NDock.Base.Metadata;

namespace NDock.Base
{
    public abstract class AppServer : IAppServer
    {
        public string Name { get; private set; }

        protected CompositionContainer CompositionContainer { get; private set; }

        public IServerConfig Config { get; private set; }

        protected virtual void RegisterCompositeTarget(IList<ICompositeTarget> targets)
        {
            targets.Add(new LogFactoryCompositeTarget((value) =>
            {
                LogFactory = value;
                Logger = value.GetLog(Name);
            }));
        }

        protected virtual CompositionContainer GetCompositionContainer(IServerConfig config)
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(IAppServer).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.dll"));

            //Create the CompositionContainer with the parts in the catalog
            return new CompositionContainer(catalog);
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

        public ILogFactory LogFactory { get; private set; }

        public ILog Logger { get; private set; }

        public IAppEndPoint EndPoint { get; private set; }

        public IMessageBus MessageBus { get; private set; }

        public virtual AppServerMetadata GetMetadata()
        {
            return AppServerMetadata.GetAppServerMetadata(this.GetType());
        }

        bool IManagedApp.Setup(IServerConfig config, IServiceProvider serviceProvider)
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

            return false;
        }

        protected abstract bool Setup(IServerConfig config, IServiceProvider serviceProvider);

        #region the code about starting

        protected virtual void OnPreStart()
        {

        }

        protected virtual void OnStarted()
        {

        }

        bool IManagedApp.Start()
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

        void IManagedApp.Stop()
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
