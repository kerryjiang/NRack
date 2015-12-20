using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;
using NDock.Base.Metadata;

namespace NDock.Examples.TestApp
{
    [AppServerMetadata("TestAppServer")]
    public class TestAppServer : AppServer
    {
        public override bool Start()
        {
            Console.WriteLine("This AppServer is started");
            return true;
        }

        public override void Stop()
        {

        }
    }
}
