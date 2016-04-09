using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NRack.Test
{
    [TestFixture]
    public class ProcessIsolationTest : Test
    {
        protected override string ConfigFile
        {
            get
            {
                return "ProcessIsolation.config";
            }
        }

        [Test]
        public void ConfigurationShareTest()
        {
            StartStopWrap(() =>
            {
                var NRackTextFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NRack.Test.txt");

                if (!File.Exists(NRackTextFilePath))
                    Assert.Fail(string.Format("The file {0} was not generated.", NRackTextFilePath));

                Assert.AreEqual("Hello NRack", File.ReadAllText(NRackTextFilePath, Encoding.UTF8));

                File.Delete(NRackTextFilePath);
            });
        }
    }
}
