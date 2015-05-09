using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;

namespace NDock.Server.Isolation
{
    abstract class IsolationBootstrap : BootstrapBase
    {
        public IsolationBootstrap(IConfigSource configSource)
            : base(configSource)
        {

        }

        protected virtual AppServerMetadataAttribute GetAppServerMetadata(IServerConfig serverConfig)
        {
            AppDomain validateDomain = null;
            AppServerMetadataAttribute metadata = null;

            try
            {
                validateDomain = AppDomain.CreateDomain("ValidationDomain", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.BaseDirectory, string.Empty, false);
                AssemblyImport.RegisterAssembplyImport(validateDomain);

                var validatorType = typeof(RemoteAppTypeValidator);
                var validator = (RemoteAppTypeValidator)validateDomain.CreateInstanceAndUnwrap(validatorType.Assembly.FullName, validatorType.FullName);

                metadata = validator.GetServerMetadata(serverConfig.Type);
            }
            finally
            {
                if (validateDomain != null)
                    AppDomain.Unload(validateDomain);
            }

            return metadata;
        }

        protected abstract IManagedApp CreateAppInstanceByMetadata(AppServerMetadataAttribute metadata);

        protected override IManagedApp CreateAppInstance(IServerConfig serverConfig)
        {
            var metadata = GetAppServerMetadata(serverConfig);

            if (metadata == null)
                throw new Exception("Failed to load server's type");

            return CreateAppInstanceByMetadata(metadata);
        }
    }
}
