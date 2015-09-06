using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Base.Configuration;
using NDock.Base.Metadata;
using NDock.Base.Provider;

namespace NDock.Server.Recycle
{
    [Export(typeof(IRecycleTrigger))]
    [ProviderMetadata("MemoryTrigger")]
    public class MemoryRecycleTrigger : IRecycleTrigger
    {
        private long m_MaxMemoryUsage;

        public bool Initialize(NameValueCollection options)
        {
            if (long.TryParse(options.GetValue("maxMemoryUsage"), out m_MaxMemoryUsage) || m_MaxMemoryUsage <= 0)
                return false;

            return true;
        }

        public bool NeedBeRecycled(IManagedApp app, StatusInfoCollection status)
        {
            var memoryUsage = status[StatusInfoKeys.MemoryUsage];

            if (memoryUsage == null)
                return false;

            return (long)memoryUsage >= m_MaxMemoryUsage;
        }
    }
}
