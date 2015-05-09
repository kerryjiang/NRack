using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using NDock.Server.Isolation;

namespace NDock.Server.ProcessIsolation
{
    class ProcessApp : IsolationApp
    {
        public ProcessApp(AppServerMetadataAttribute metadata)
            : base(metadata)
        {

        }
    }
}
