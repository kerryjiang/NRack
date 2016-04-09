using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base
{
    public interface IBootstrap
    {
        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns></returns>
        bool Initialize();

        /// <summary>
        /// Gets all the app servers running in this bootstrap
        /// </summary>
        IEnumerable<IManagedApp> AppServers { get; }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void Stop();

        /// <summary>
        /// Gets the configuration file path.
        /// </summary>
        /// <value>
        /// The configuration file path.
        /// </value>
        string ConfigFilePath { get; }
    }
}
