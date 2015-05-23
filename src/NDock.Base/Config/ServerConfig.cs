using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using NDock.Base.Configuration;

namespace NDock.Base.Config
{
    [Serializable]
    public class ServerConfig : IServerConfig
    {
        public ServerConfig()
        {

        }

        public ServerConfig(IServerConfig serverConfig)
        {
            serverConfig.CopyPropertiesTo(this);
        }

        public string Name { get; set; }

        public string Group { get; set; }

        public string Type { get; set; }

        public string LogFactory { get; set; }

        public NameValueCollection Options { get; set; }

        public NameValueCollection OptionElements { get; set; }

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
    }
}
