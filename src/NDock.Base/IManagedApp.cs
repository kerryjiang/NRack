using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;
using NDock.Base.Metadata;

namespace NDock.Base
{
    public interface IManagedApp
    {
        string Name { get; }

        bool Setup(IServerConfig config, IServiceProvider serviceProvider);

        AppServerMetadata GetMetadata();

        IServerConfig Config { get; }

        bool Start();

        void Stop();
    }
}
