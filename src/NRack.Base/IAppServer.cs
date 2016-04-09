using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using AnyLog;
using NRack.Base.Config;

namespace NRack.Base
{
    public interface IAppServer : IManagedApp
    {
        ILog Logger { get; }

        IAppEndPoint EndPoint { get; }

        IMessageBus MessageBus { get; }
    }
}
