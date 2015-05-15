using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NDock.Base;

namespace NDock.Server
{
    class ControlCommand
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Func<IBootstrap, string[], bool> Handler { get; set; }
    }
}
