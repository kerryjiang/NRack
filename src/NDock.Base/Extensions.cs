using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;

namespace NDock.Base
{
    public static class Extensions
    {

        private const string CurrentAppDomainExportProviderKey = "CurrentAppDomainExportProvider";

        /// <summary>
        /// Gets the current application domain's export provider, if it doesn't exist, create one.
        /// </summary>
        /// <param name="appDomain">The application domain.</param>
        /// <returns></returns>
        public static CompositionContainer GetCurrentAppDomainExportProvider(this AppDomain appDomain)
        {
            var exportProvider = appDomain.GetData(CurrentAppDomainExportProviderKey) as CompositionContainer;

            if (exportProvider != null)
                return exportProvider;

            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(IAppServer).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory, "*.dll"));
            exportProvider = new CompositionContainer(catalog);

            appDomain.SetData(CurrentAppDomainExportProviderKey, exportProvider);
            return exportProvider;
        }
    }
}
