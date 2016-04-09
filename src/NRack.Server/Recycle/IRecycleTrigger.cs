using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;

namespace NDock.Server.Recycle
{
    public interface IRecycleTrigger
    {
        string Name { get; }

        bool Initialize(NameValueCollection options);

        bool NeedBeRecycled(IManagedApp app, StatusInfoCollection status);
    }
}
