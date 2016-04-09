using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using NRack.Base;
using NRack.Base.Metadata;

namespace NRack.Server
{
    [Serializable]
    public class RemoteTypeLoadResult<T>
    {
        public bool Result { get; set; }

        public T Value { get; set; }

        public string Message { get; set; }
    }

    public class RemoteAppTypeValidator : MarshalByRefObject
    {
        private CompositionContainer m_ExportProvider;

        public RemoteAppTypeValidator()
        {
            m_ExportProvider = AppDomain.CurrentDomain.GetCurrentAppDomainExportProvider();
        }

        public RemoteTypeLoadResult<AppServerMetadata> GetServerMetadata(string serverTypeName)
        {
            Lazy<IAppServer, IAppServerMetadata> lazyServerFactory = null;

            try
            {
                lazyServerFactory = m_ExportProvider.GetExports<IAppServer, IAppServerMetadata>()
                    .FirstOrDefault(f => f.Metadata.Name.Equals(serverTypeName, StringComparison.OrdinalIgnoreCase));
            }
            catch (ReflectionTypeLoadException e)
            {
                var msg = e.Message;

                foreach (var le in e.LoaderExceptions)
                {
                    msg += Environment.NewLine + le.Message;
                }

                return new RemoteTypeLoadResult<AppServerMetadata> { Message = msg };
            }

            AppServerMetadata metadata = null;

            try
            {
                if (lazyServerFactory != null)
                {
                    metadata = new AppServerMetadata(lazyServerFactory.Metadata, lazyServerFactory.GetExportType());
                }
                else
                {
                    metadata = AppServerMetadata.GetAppServerMetadata(Type.GetType(serverTypeName, true, true));
                }
            }
            catch(Exception e)
            {
                return new RemoteTypeLoadResult<AppServerMetadata> { Message = e.Message + Environment.NewLine + e.StackTrace };
            }

            return new RemoteTypeLoadResult<AppServerMetadata> { Result = true, Value = metadata };
        }
    }
}
