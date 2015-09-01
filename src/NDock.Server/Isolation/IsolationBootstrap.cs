using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Configuration;
using NDock.Base.Metadata;
using NDock.Base.Provider;
using NDock.Server.Config;
using NDock.Server.Recycle;

namespace NDock.Server.Isolation
{
    abstract class IsolationBootstrap : BootstrapBase
    {
        protected string ConfigFilePath { get; private set; }

        public IsolationBootstrap(IConfigSource configSource)
            : base(GetSerializableConfigSource(configSource))
        {
            ConfigFilePath = ((ConfigurationElement)configSource).GetConfigSource();
        }

        private static IConfigSource GetSerializableConfigSource(IConfigSource configSource)
        {
            if (configSource.GetType().IsSerializable)
            {
                return configSource;
            }

            return new NDockConfig(configSource);
        }

        protected override AppServerMetadata GetAppServerMetadata(IServerConfig serverConfig)
        {
            AppDomain validateDomain = null;
            AppServerMetadata metadata = null;

            try
            {
                validateDomain = AppDomain.CreateDomain("ValidationDomain", AppDomain.CurrentDomain.Evidence, IsolationApp.GetAppWorkingDir(serverConfig.Name), string.Empty, false);
                
                AssemblyImport.RegisterAssembplyImport(validateDomain);

                validateDomain.SetData(typeof(IsolationMode).Name, ConfigSource.Isolation);

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

        void SetupRecycleTriggers(IManagedApp managedApp, IServerConfig config)
        {
            try
            {
                var recycleTriggers = config.OptionElements.GetChildConfig<RecycleTriggerConfigCollection>("recycleTriggers");

                if (recycleTriggers != null && recycleTriggers.Count > 0)
                {
                    var exportProvider = AppDomain.CurrentDomain.GetCurrentAppDomainExportProvider();
                    var recycleTriggerTypes = exportProvider.GetExports<IRecycleTrigger, IProviderMetadata>();

                    var triggers = new List<IRecycleTrigger>();

                    foreach (var triggerConfig in recycleTriggers)
                    {
                        var triggerType = recycleTriggerTypes.FirstOrDefault(t =>
                                t.Metadata.Name.Equals(triggerConfig.Name, StringComparison.OrdinalIgnoreCase));

                        if (triggerType == null)
                        {
                            Log.ErrorFormat("We cannot find a RecycleTrigger with the name '{0}'.", triggerConfig.Name);
                            continue;
                        }

                        var trigger = triggerType.Value;

                        if (!trigger.Initialize(triggerConfig.Options))
                        {
                            Log.ErrorFormat("Failed to initialize the RecycleTrigger '{0}'.", triggerConfig.Name);
                            continue;
                        }

                        triggers.Add(trigger);
                    }

                    if (triggers.Any())
                    {
                        (managedApp as IsolationApp).RecycleTriggers = triggers.ToArray();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to load recycle triggers.", e);
            }
        }

        protected override bool Setup(IManagedApp managedApp, IServerConfig config)
        {
            var ret = base.Setup(managedApp, config);

            if (!ret)
                return false;

            SetupRecycleTriggers(managedApp, config);
            return ret;
        }
    }
}
