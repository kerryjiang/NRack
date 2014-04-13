using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;

namespace NDock.Base
{
    public abstract class AppServer : IAppServer
    {
        public bool Setup(IServerConfig config, IServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        protected virtual void OnPreStart()
        {

        }

        public bool Start()
        {
            OnPreStart();

            //TODO: Actual starting code

            OnStarted();
            return true;
        }

        protected virtual void OnStarted()
        {

        }

        protected virtual void OnPreStop()
        {
            
        }

        public void Stop()
        {
            OnPreStop();

            //TODO: Actual stopping code

            OnStopped();
        }

        protected virtual void OnStopped()
        {
            
        }
    }
}
