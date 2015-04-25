using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Server.AppDomain;
using NDock.Server.Process;

namespace NDock.Server
{
    /// <summary>
    /// Bootstrap Factory
    /// </summary>
    public static class BootstrapFactory
    {
        /// <summary>
        /// Creates the bootstrap.
        /// </summary>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        public static IBootstrap CreateBootstrap(IConfigSource config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (config.Isolation == IsolationMode.AppDomain)
                return new AppDomainBootstrap(config);
            else if (config.Isolation == IsolationMode.Process)
                return new ProcessBootstrap(config);
            else
                return new DefaultBootstrap(config);
        }

        /// <summary>
        /// Creates the bootstrap from app configuration's socketServer section.
        /// </summary>
        /// <returns></returns>
        public static IBootstrap CreateBootstrap()
        {
            var configSection = ConfigurationManager.GetSection("ndock");

            if (configSection == null)
                throw new ConfigurationErrorsException("Missing 'ndock' configuration section.");

            var configSource = configSection as IConfigSource;
            if (configSource == null)
                throw new ConfigurationErrorsException("Invalid 'ndock' configuration section.");

            return CreateBootstrap(configSource);
        }

        /// <summary>
        /// Creates the bootstrap from configuration file.
        /// </summary>
        /// <param name="configFile">The configuration file.</param>
        /// <returns></returns>
        public static IBootstrap CreateBootstrapFromConfigFile(string configFile)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = configFile;

            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            var configSection = config.GetSection("ndock");

            if (configSection == null)
                throw new ConfigurationErrorsException("Missing 'ndock' configuration section.");

            var configSource = configSection as IConfigSource;
            if (configSource == null)
                throw new ConfigurationErrorsException("Invalid 'ndock' configuration section.");

            return CreateBootstrap(configSource);
        }
    }
}
