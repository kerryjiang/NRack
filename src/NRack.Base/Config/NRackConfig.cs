using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRack.Base.Configuration;

namespace NRack.Base.Config
{
    [Serializable]
    public class NRackConfig : IConfigSource
    {
        public const int DefaultTcpRemotingPort = 0;

        public NRackConfig()
        {

        }

        public NRackConfig(IConfigSource configSource)
        {
            Isolation = configSource.Isolation;
            LogFactory = configSource.LogFactory;
            StatusCollectInterval = configSource.StatusCollectInterval;
            TcpRemotingPort = DefaultTcpRemotingPort;

            if (configSource.Servers != null && configSource.Servers.Any())
            {
                this.Servers = configSource.Servers.Select(s => new ServerConfig(s)).ToArray();
            }
        }

        public IsolationMode Isolation { get; set; }

        public IEnumerable<IServerConfig> Servers { get; set; }

        public string LogFactory { get; set; }

        public int StatusCollectInterval { get; set; }

        public int TcpRemotingPort { get; set; }
    }
}
