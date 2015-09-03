using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;

namespace NDock.Server.Isolation.ProcessIsolation
{
    class ExternalProcessAppServerMetadata : AppServerMetadata
    {
        public ExternalProcessAppServerMetadata(string appFile, string appArgs)
        {
            AppFile = appFile;
            AppArgs = appArgs;
        }

        public string AppFile { get; private set; }

        public string AppArgs { get; private set; }
    }
}
