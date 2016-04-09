using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NRack.Test
{
    [TestFixture]
    public class AppDomainIsolationTest : Test
    {
        protected override string ConfigFile
        {
            get
            {
                return "AppDomainIsolation.config";
            }
        }
    }
}
