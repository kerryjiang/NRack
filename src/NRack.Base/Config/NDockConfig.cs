using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Configuration;

namespace NDock.Base.Config
{
    [Serializable]
    public class NDockConfig : IConfigSource
    {
        public const int DefaultTcpRemotingPort = 0;

        public NDockConfig()
        {

        }

        public NDockConfig(IConfigSource configSource)
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
