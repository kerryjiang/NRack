using System;
using NRack.Base;
using NRack.Base.Config;
using NRack.Base.Metadata;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Text;

namespace NRack.Test
{
    [AppServerMetadata("TestAppServer")]
    public class TestAppServer : AppServer
    {
        public override bool Start()
        {
            var NRackText = ConfigurationManager.AppSettings["NRack.Test"];

            if(!string.IsNullOrEmpty(NRackText))
            {
                var NRackTextFilePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "NRack.Test.txt");
                File.WriteAllText(NRackTextFilePath, NRackText, Encoding.UTF8);
            }

            return true;
        }

        public override void Stop()
        {

        }
    }
}

