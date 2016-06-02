using System;
using NRack.Base.Config;
using NRack.Base.Metadata;

namespace NRack.Base
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

        void ReportPotentialConfigChange(IServerConfig config);
    }
}
