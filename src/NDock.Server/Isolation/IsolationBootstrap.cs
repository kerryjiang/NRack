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
    [StatusInfo(StatusInfoKeys.CpuUsage, Name = "CPU Usage", Format = "{0:0.00}%", DataType = typeof(double), Order = 112)]
    [StatusInfo(StatusInfoKeys.MemoryUsage, Name = "Physical Memory Usage", Format = "{0:N}", DataType = typeof(double), Order = 113)]
    [StatusInfo(StatusInfoKeys.TotalThreadCount, Name = "Total Thread Count", Format = "{0}", DataType = typeof(double), Order = 114)]
    abstract class IsolationBootstrap : BootstrapBase
    {
        protected string ConfigFilePath { get; private set; }

        private IBootstrap m_RemoteBootstrapWrap;

        private IEnumerable<Lazy<IRecycleTrigger, IProviderMetadata>> m_RecycleTriggers;

        public IsolationBootstrap(IConfigSource configSource)
            : base(GetSerializableConfigSource(configSource))
        {
            ConfigFilePath = ((ConfigurationElement)configSource).GetConfigSource();
            m_RemoteBootstrapWrap = new RemoteBootstrapProxy(this);
            m_RecycleTriggers = AppDomain.CurrentDomain.GetCurrentAppDomainExportProvider().GetExports<IRecycleTrigger, IProviderMetadata>();
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

                var result = validator.GetServerMetadata(serverConfig.Type);

                if(!result.Result)
                {
                    Log.Error(result.Message);
                    return null;
                }

                metadata = result.Value;
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

                if (recycleTriggers == null || !recycleTriggers.Any())
                    return;

                var triggers = new List<IRecycleTrigger>();

                foreach (var triggerConfig in recycleTriggers)
                {
                    var triggerType = m_RecycleTriggers.FirstOrDefault(t =>
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
            catch (Exception e)
            {
                Log.Error("Failed to load recycle triggers.", e);
            }
        }

        protected override bool Setup(IManagedApp managedApp, IServerConfig config)
        {
            var ret = managedApp.Setup(m_RemoteBootstrapWrap, config);

            if (!ret)
                return false;

            SetupRecycleTriggers(managedApp, config);
            return ret;
        }
    }
}
