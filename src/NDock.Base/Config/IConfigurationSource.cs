using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base.Config
{
    public interface IConfigurationSource
    {
        IsolationMode Isolation { get; }

        IEnumerable<IServerConfig> Servers { get; }
    }
}
