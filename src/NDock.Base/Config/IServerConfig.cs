using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace NDock.Base.Config
{
    public interface IServerConfig
    {
        string Name { get; }

        string Group { get; }

        string Type { get; }

        string LogFactory { get; }

        StartupType StartupType { get; }

        NameValueCollection Options { get; }

        NameValueCollection OptionElements { get; }
    }
}
