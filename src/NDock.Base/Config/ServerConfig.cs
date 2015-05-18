using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base.Config
{
    [Serializable]
    public class ServerConfig : IServerConfig
    {
        public ServerConfig()
        {

        }

        public ServerConfig(IServerConfig serverConfig)
        {
            serverConfig.CopyPropertiesTo(this);
        }
        public string Name { get; set; }

        public string Group { get; set; }

        public string Type { get; set; }

        public string LogFactory { get; set; }
    }
}
