using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using AnyLog;
using NDock.Base.Config;

namespace NDock.Base
{
    public interface IAppServer : IManagedApp
    {
        ILog Logger { get; }

        IAppEndPoint EndPoint { get; }

        IMessageBus MessageBus { get; }
    }
}
