using System;
using System.Collections.Generic;
using System.Text;

namespace NRack.Base.Config
{
    public interface IConfigSource
    {
        string LogFactory { get; }

        IsolationMode Isolation { get; }

        int StatusCollectInterval { get; }

        IEnumerable<IServerConfig> Servers { get; }

        int TcpRemotingPort { get; }
    }
}
