using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;
using NDock.Base.Metadata;

namespace NDock.Base
{
    public interface IManagedApp : IManagedAppBase
    {
        string Name { get; }

        bool Setup(IBootstrap bootstrap, IServerConfig config);

        ServerState State { get; }

        IServerConfig Config { get; }
    }

    public interface IManagedAppBase
    {
        AppServerMetadata GetMetadata();

        bool Start();

        void Stop();

        bool CanBeRecycled();

        StatusInfoCollection CollectStatus();
    }
}
