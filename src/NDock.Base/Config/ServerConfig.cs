using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base.Config
{
    public class ServerConfig : IServerConfig
    {
        public string Name { get; set; }

        public string Group { get; set; }

        public string Type { get; set; }
    }
}
