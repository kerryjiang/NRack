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
        public TestAppServer()
        {
            
        }

        public override bool Start()
        {
            return true;
        }

        public override void Stop()
        {
            
        }
    }
}

