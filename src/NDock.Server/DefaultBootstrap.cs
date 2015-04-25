using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;

namespace NDock.Server
{
    class DefaultBootstrap : BootstrapBase
    {
        public DefaultBootstrap(IConfigSource configSource)
            : base(configSource)
        {
            var exportProvider = ExportProvider;

            foreach (var config in configSource.Servers)
            {
                var workItemSrc = exportProvider.GetExport<IAppServer>(config.Type);
                WorkItems.Add(workItemSrc.Value);
            }
        }
    }
}
