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
            : base(GetSerializableConfigSource(configSource))
        {

        }

        private static IConfigSource GetSerializableConfigSource(IConfigSource configSource)
        {
            if (configSource.GetType().IsSerializable)
                return configSource;

            return new NDockConfig(configSource);
        }

        protected override AppServerMetadata GetAppServerMetadata(IServerConfig serverConfig)
        {
            AppDomain validateDomain = null;
            AppServerMetadata metadata = null;

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
    }
}
