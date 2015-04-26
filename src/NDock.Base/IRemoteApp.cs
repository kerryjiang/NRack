using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;

namespace NDock.Base
{
    public interface IRemoteApp
    {
        bool Setup(IServerConfig config, IServiceProvider serviceProvider);

        IServerConfig Config { get; }

        bool Start();

        void Stop();
    }
}
