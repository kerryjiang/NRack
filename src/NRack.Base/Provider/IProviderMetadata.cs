using System;

namespace NRack.Base.Provider
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
