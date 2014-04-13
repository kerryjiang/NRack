using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base
{
    public interface IBootstrap
    {
        void Start();

        void Stop();
    }
}
