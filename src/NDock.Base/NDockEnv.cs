using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base
{
    /// <summary>
    /// System environment variables of NDock
    /// </summary>
    public static class NDockEnv
    {
        /// <summary>
        /// Is this runtime Mono
        /// </summary>
        public static readonly bool IsMono;

        static NDockEnv()
        {
            // detect the runtime by the type Mono.Runtime
            IsMono = Type.GetType("Mono.Runtime") != null;
        }
    }
}
