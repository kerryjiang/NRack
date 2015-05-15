using System;
using System.Collections.Generic;
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
    }
}
