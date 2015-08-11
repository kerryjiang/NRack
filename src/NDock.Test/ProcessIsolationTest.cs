using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NDock.Test
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
                var ndockTextFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NDock.Test.txt");

                if (!File.Exists(ndockTextFilePath))
                    Assert.Fail(string.Format("The file {0} was not generated.", ndockTextFilePath));

                Assert.AreEqual("Hello NDock", File.ReadAllText(ndockTextFilePath, Encoding.UTF8));

                File.Delete(ndockTextFilePath);
            });
        }
    }
}
