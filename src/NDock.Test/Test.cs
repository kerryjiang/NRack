using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            StartStopWrap(null);
        }

        protected void StartStopWrap(Action action)
        {
            var bootstrap = GetBootstrap();
            Assert.IsNotNull(bootstrap);
            Assert.IsTrue(bootstrap.Initialize());

            var server = bootstrap.AppServers.FirstOrDefault();

            Assert.AreEqual(ServerState.NotStarted, server.State);

            bootstrap.Start();
            Assert.AreEqual(ServerState.Running, server.State);

            if(action != null)
                action();

            bootstrap.Stop();
            Assert.AreEqual(ServerState.NotStarted, server.State);
        }

        [Test]
        public void StatusCollectTest()
        {
            StartStopWrap(() =>
            {
                Thread.Sleep(1000 * 60 * 5);
            });
        }
    }
}
