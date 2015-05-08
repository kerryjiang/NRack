using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;

namespace NDock.Server
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
        /// <returns></returns>
        bool Setup(string serverType, string bootstrapUri, string assemblyImportRoot, IServerConfig config);
    }
}
