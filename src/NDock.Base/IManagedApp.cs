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

        bool Setup(IServerConfig config);

        ServerState State { get; }

        IServerConfig Config { get; }
    }

    public interface IManagedAppBase
    {
        AppServerMetadataAttribute GetMetadata();

        bool Start();

        void Stop();
    }
}
