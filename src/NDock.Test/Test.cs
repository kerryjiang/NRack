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
        protected virtual string ConfigFile
        {
            get { return "Basic.config"; }
        }

        private IBootstrap GetBootstrap()
        {
            var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Asserts", "Config", ConfigFile);
            return BootstrapFactory.CreateBootstrapFromConfigFile(configFile);
        }

        [Test]
        public void ConfigLoadTest()
        {
            var bootstrap = GetBootstrap();
            Assert.IsNotNull(bootstrap);
            Assert.IsTrue(bootstrap.Initialize());
        }

        [Test]
        public void StartStopTest()
        {
            var bootstrap = GetBootstrap();
            Assert.IsNotNull(bootstrap);
            Assert.IsTrue(bootstrap.Initialize());

            var server = bootstrap.AppServers.FirstOrDefault();

            Assert.AreEqual(ServerState.NotStarted, server.State);

            bootstrap.Start();
            Assert.AreEqual(ServerState.Running, server.State);

            bootstrap.Stop();
            Assert.AreEqual(ServerState.NotStarted, server.State);
        }
    }
}
