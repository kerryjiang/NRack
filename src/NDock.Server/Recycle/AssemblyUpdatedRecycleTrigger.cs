using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Base.Configuration;
using NDock.Base.Provider;
using NDock.Server.Isolation;

namespace NDock.Server.Recycle
{
    [ProviderMetadata("AssemblyUpdatedTrigger")]
    public class AssemblyUpdatedRecycleTrigger : IRecycleTrigger
    {
        private int m_CheckInterval;
        private int m_RestartRelay;

        public bool Initialize(NameValueCollection options)
        {
            var checkInterval = 0;

            if (int.TryParse(options.GetValue("checkInterval", "5"), out checkInterval))
                return false;

            m_CheckInterval = checkInterval;

            var restartDelay = 0;

            if (int.TryParse(options.GetValue("restartDelay", "1"), out restartDelay))
                return false;

            m_RestartRelay = restartDelay;

            return true;
        }

        private bool IsDeplayOverdue(DateTime lastUpdatedTime)
        {
            if (lastUpdatedTime.AddMinutes(m_RestartRelay) <= DateTime.Now)
                return true;

            return false;
        }

        public bool NeedBeRecycled(IManagedApp app, StatusInfoCollection status)
        {
            var state = (app as IsolationApp).AssemblyUpdateState;

            // not start
            if (state == null)
                return false;

            if (state.LastUpdatedTime > state.CurrentAssemblyTime)
                return IsDeplayOverdue(state.LastUpdatedTime);

            // next check time has not reached yet
            if (state.LastCheckTime.AddMinutes(m_CheckInterval) > DateTime.Now)
                return false;

            if (!state.TryCheckUpdate())
                return false;

            return IsDeplayOverdue(state.LastUpdatedTime);
        }
    }
}
