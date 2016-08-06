using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRack.Base;
using NRack.Base.Metadata;

namespace NRack.Examples.TestApp
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
