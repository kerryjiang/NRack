using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using NDock.Base.Config;

namespace NDock.Base.Configuration
{
    public class NDockConfigSection : ConfigurationSection, IConfigSource
    {
        /// <summary>
        /// Gets the isolation mode.
        /// </summary>
        [ConfigurationProperty("isolation", IsRequired = false, DefaultValue = IsolationMode.None)]
        public IsolationMode Isolation
        {
            get { return (IsolationMode)this["isolation"]; }
        }

        [ConfigurationProperty("logFactory", IsRequired = false)]
        public string LogFactory
        {
            get { return (string)this["logFactory"]; }
        }

        [ConfigurationProperty("statusCollectInterval", IsRequired = false, DefaultValue = 60)]
        public int StatusCollectInterval
        {
            get { return (int)this["statusCollectInterval"]; }
        }

        [ConfigurationProperty("tcpRemotingPort", IsRequired = false, DefaultValue = NDockConfig.DefaultTcpRemotingPort)]
        public int TcpRemotingPort
        {
            get { return (int)this["tcpRemotingPort"]; }
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

        IEnumerable<IServerConfig> IConfigSource.Servers
        {
            get { return this.Servers; }
        }

        /// <summary>
        /// Gets the option elements.
        /// </summary>
        public NameValueCollection OptionElements { get; private set; }

        /// <summary>
        /// Gets a value indicating whether an unknown element is encountered during deserialization.
        /// To keep compatible with old configuration
        /// </summary>
        /// <param name="elementName">The name of the unknown subelement.</param>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> being used for deserialization.</param>
        /// <returns>
        /// true when an unknown element is encountered while deserializing; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element identified by <paramref name="elementName"/> is locked.- or -One or more of the element's attributes is locked.- or -<paramref name="elementName"/> is unrecognized, or the element has an unrecognized attribute.- or -The element has a Boolean attribute with an invalid value.- or -An attempt was made to deserialize a property more than once.- or -An attempt was made to deserialize a property that is not a valid member of the element.- or -The element cannot contain a CDATA or text element.</exception>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            if (OptionElements == null)
                OptionElements = new NameValueCollection();

            OptionElements.Add(elementName, reader.ReadOuterXml());
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether an unknown attribute is encountered during deserialization.
        /// </summary>
        /// <param name="name">The name of the unrecognized attribute.</param>
        /// <param name="value">The value of the unrecognized attribute.</param>
        /// <returns>
        /// true when an unknown attribute is encountered while deserializing; otherwise, false.
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            const string xmlns = "xmlns";
            const string xmlnsPrefix = "xmlns:";
            const string xsiPrefix = "xsi:";

            //for configuration intellisense, allow these unrecognized attributes: xmlns, xmlns:*, xsi:*
            if (name.Equals(xmlns) || name.StartsWith(xmlnsPrefix) || name.StartsWith(xsiPrefix))
                return true;

            return false;
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

    /// <summary>
    /// Server configuration collection
    /// </summary>
    [ConfigurationCollection(typeof(ServerConfigElement), AddItemName = "server")]
    public class ServerCollection : GenericConfigurationElementCollection<ServerConfigElement, IServerConfig>
    {
    }
}
