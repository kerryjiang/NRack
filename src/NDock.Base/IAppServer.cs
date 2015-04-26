using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using NDock.Base.Config;

namespace NDock.Base
{
    public interface IAppServer : IRemoteApp
    {
        IAppEndPoint EndPoint { get; }

        IMessageBus MessageBus { get; }
    }
}
