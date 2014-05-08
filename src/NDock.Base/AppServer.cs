using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base.Config;

namespace NDock.Base
{
    public abstract class AppServer : IAppServer
    {
        public abstract bool Setup(IServerConfig config, IServiceProvider serviceProvider);

        #region the code about starting

        protected virtual void OnPreStart()
        {

        }

        protected virtual void OnStarted()
        {

        }

        bool IWorkItem.Start()
        {
            OnPreStart();

            if (!Start())
                return false;

            OnStarted();
            return true;
        }

        public abstract bool Start();

        #endregion

        #region the code about stoppping

        protected virtual void OnPreStop()
        {
            
        }

        void IWorkItem.Stop()
        {
            OnPreStop();

            //TODO: Actual stopping code

            OnStopped();
        }

        public abstract void Stop();

        protected virtual void OnStopped()
        {

        }

        #endregion
    }
}
