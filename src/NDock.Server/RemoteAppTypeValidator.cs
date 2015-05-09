using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Metadata;

namespace NDock.Server
{
    public class RemoteAppTypeValidator : MarshalByRefObject
    {
        private ExportProvider m_ExportProvider;

        public RemoteAppTypeValidator()
        {
            m_ExportProvider = AppDomain.CurrentDomain.GetCurrentAppDomainExportProvider();
        }

        public AppServerMetadataAttribute GetServerMetadata(string serverTypeName)
        {
            var lazyServerFactory = m_ExportProvider.GetExports<IAppServer, IAppServerMetadata>()
                .FirstOrDefault(f => f.Metadata.Name.Equals(serverTypeName, StringComparison.OrdinalIgnoreCase));

            if (lazyServerFactory != null)
                return lazyServerFactory.Metadata as AppServerMetadataAttribute;

            try
            {
                return AppServerMetadataAttribute.GetAppServerMetadata(Type.GetType(serverTypeName, true, true));
            }
            catch
            {
                return null;
            }
        }
    }
}
