using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base.Metadata
{
    public interface IAppServerMetadata
    {
        string Name { get; }

        string AppType { get; }

        StatusInfoAttribute[] StatusFields { get; }
    }
}
