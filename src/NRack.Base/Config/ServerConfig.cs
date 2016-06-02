using System;
using System.Collections.Generic;
using System.Collections.Specialized;
#if !DOTNETCORE
using System.Configuration;
using NRack.Base.Configuration;
#endif

namespace NRack.Base.Config
{
#if !DOTNETCORE
    [Serializable]
#endif
    public class ServerConfig : IServerConfig
    {
        public ServerConfig()
        {

        }

        public ServerConfig(IServerConfig serverConfig)
        {
#if !DOTNETCORE
            serverConfig.CopyPropertiesTo(this);
#endif
        }

        public string Name { get; set; }

        public string Group { get; set; }

        public string Type { get; set; }

        public string LogFactory { get; set; }

        public StartupType StartupType { get; set; }

        public NameValueCollection Options { get; set; }

        public NameValueCollection OptionElements { get; set; }

#if !DOTNETCORE

        /// <summary>
        /// Gets the child config.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config.</typeparam>
        /// <param name="childConfigName">Name of the child config.</param>
        /// <returns></returns>
        public virtual TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new()
        {
            return this.OptionElements.GetChildConfig<TConfig>(childConfigName);
        }
#endif

    }

}
