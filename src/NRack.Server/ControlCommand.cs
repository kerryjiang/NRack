using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRack.Base;

namespace NRack.Server
{
    class ControlCommand
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Func<IBootstrap, string[], bool> Handler { get; set; }
    }
}
