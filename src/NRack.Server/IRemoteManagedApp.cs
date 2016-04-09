using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRack.Base;
using NRack.Base.Config;

namespace NRack.Server
{
    /// <summary>
    /// IRemoteManagedApp
    /// </summary>
    public interface IRemoteManagedApp : IManagedAppBase
    {
        /// <summary>
        /// Setups with the specified config.
        /// </summary>
        /// <param name="serverType">Type of the server.</param>
        /// <param name="bootstrapUri">The bootstrap URI.</param>
        /// <param name="assemblyImportRoot">The assembly import root.</param>
        /// <param name="config">The config.</param>
        /// <param name="startupConfigFile">The startup configuration file path.</param>
        /// <returns></returns>
        bool Setup(string serverType, string bootstrapUri, string assemblyImportRoot, IServerConfig config, string startupConfigFile);
    }
}
