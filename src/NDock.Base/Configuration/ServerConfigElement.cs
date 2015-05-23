using NDock.Base.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace NDock.Base.Configuration
{
    public class ServerConfigElement : ConfigurationElementBase, IServerConfig
    {
        [ConfigurationProperty("group", IsRequired = false)]
        public string Group
        {
            get { return this["group"] as string; }
        }

        [ConfigurationProperty("type", IsRequired = false)]
        public string Type
        {
            get { return this["type"] as string; }
        }

        [ConfigurationProperty("logFactory", IsRequired = false)]
        public string LogFactory
        {
            get { return this["logFactory"] as string; }
        }

        /// <summary>
        /// Gets the child config.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config.</typeparam>
        /// <param name="childConfigName">Name of the child config.</param>
        /// <returns></returns>
        public TConfig GetChildConfig<TConfig>(string childConfigName)
            where TConfig : ConfigurationElement, new()
        {
            return this.OptionElements.GetChildConfig<TConfig>(childConfigName);
        }
    }
}
