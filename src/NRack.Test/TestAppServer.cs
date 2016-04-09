using System;
using NDock.Base;
using NDock.Base.Config;
using NDock.Base.Metadata;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.IO;
using System.Text;

namespace NDock.Test
{
    [AppServerMetadata("TestAppServer")]
    public class TestAppServer : AppServer
    {
        public override bool Start()
        {
            var ndockText = ConfigurationManager.AppSettings["NDock.Test"];

            if(!string.IsNullOrEmpty(ndockText))
            {
                var ndockTextFilePath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName, "NDock.Test.txt");
                File.WriteAllText(ndockTextFilePath, ndockText, Encoding.UTF8);
            }

            return true;
        }

        public override void Stop()
        {

        }
    }
}

