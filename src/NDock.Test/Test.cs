using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NDock.Base;
using NDock.Server;
using NUnit.Framework;

namespace NDock.Test
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void ConfigLoadTest()
        {
            var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Asserts", "Config", "Basic.config");
            var bootstrap = BootstrapFactory.CreateBootstrapFromConfigFile(configFile);
            Assert.IsNotNull(bootstrap);

            Assert.IsTrue(bootstrap.Initialize());
        }
    }
}
