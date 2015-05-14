using System;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using System.ComponentModel.Composition.Hosting;

namespace NDock.Test
{
    [AppServerMetadata("TestAppServer")]
    public class TestAppServer : AppServer
    {
        public override bool Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
        
    }
}

