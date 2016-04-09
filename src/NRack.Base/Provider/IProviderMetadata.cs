using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDock.Base.Provider
{
    /// <summary>
    /// The provider metadata interface
    /// </summary>
    public interface IProviderMetadata
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }
    }
}
