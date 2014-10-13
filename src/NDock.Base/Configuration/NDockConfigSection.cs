using NDock.Base.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace NDock.Base.Configuration
{
    public class NDockConfigSection : ConfigurationSection, INDockConfig
    {
        /// <summary>
        /// Gets the isolation mode.
        /// </summary>
        [ConfigurationProperty("isolation", IsRequired = false, DefaultValue = IsolationMode.None)]
        public IsolationMode Isolation
        {
            get { return (IsolationMode)this["isolation"]; }
        }

        /// <summary>
        /// Gets all the server configurations
        /// </summary>
        [ConfigurationProperty("servers")]
        public ServerCollection Servers
        {
            get
            {
                return this["servers"] as ServerCollection;
            }
        }

        IEnumerable<IServerConfig> INDockConfig.Servers
        {
            get { return this.Servers; }
        }
    }

    /// <summary>
    /// Server configuration collection
    /// </summary>
    [ConfigurationCollection(typeof(ServerConfigElement), AddItemName = "server")]
    public class ServerCollection : GenericConfigurationElementCollection<ServerConfigElement, IServerConfig>
    {
    }
}
