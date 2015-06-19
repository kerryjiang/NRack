using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Base.Provider;

namespace NDock.Server.Recycle
{
    [ProviderMetadata("MemoryTrigger")]
    public class MemoryRecycleTrigger : IRecycleTrigger
    {
        public bool Initialize(NameValueCollection options)
        {
            throw new NotImplementedException();
        }

        public bool NeedBeRecycled(IManagedApp app)
        {
            throw new NotImplementedException();
        }
    }
}
