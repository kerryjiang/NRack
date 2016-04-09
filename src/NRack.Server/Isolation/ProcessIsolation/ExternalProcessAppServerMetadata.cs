using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRack.Base;

namespace NRack.Server.Isolation.ProcessIsolation
{
    class ExternalProcessAppServerMetadata : AppServerMetadata
    {
        public ExternalProcessAppServerMetadata(string appDir, string appFile, string appArgs)
        {
            AppDir = appDir;
            AppFile = appFile;
            AppArgs = appArgs;
        }

        public string AppDir { get; private set; }

        public string AppFile { get; private set; }

        public string AppArgs { get; private set; }
    }
}
