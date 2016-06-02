using System;

namespace NRack.Base
{
    /// <summary>
    /// System environment variables of NRack
    /// </summary>
    public static class NRackEnv
    {
        /// <summary>
        /// Is this runtime Mono
        /// </summary>
        public static readonly bool IsMono;

        static NRackEnv()
        {
            // detect the runtime by the type Mono.Runtime
            IsMono = Type.GetType("Mono.Runtime") != null;
        }
    }
}
