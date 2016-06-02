using System;

#if DOTNETCORE
using ILog = Microsoft.Extensions.Logging.ILogger;
#else
using AnyLog;
#endif


namespace NRack.Base
{
    public interface IAppServer : IManagedApp
    {
        ILog Logger { get; }

        IAppEndPoint EndPoint { get; }

        IMessageBus MessageBus { get; }
    }
}
