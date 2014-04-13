using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using NDock.Base.Config;

namespace NDock.Base
{
    public interface IAppServer
    {
        bool Setup(IServerConfig config, IServiceProvider serviceProvider);

        bool Start();

        void Stop();
    }
}
