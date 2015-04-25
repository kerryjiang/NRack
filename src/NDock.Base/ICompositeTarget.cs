using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base
{
    public interface ICompositeTarget
    {
        void Resolve(IAppServer appServer);
    }
}
