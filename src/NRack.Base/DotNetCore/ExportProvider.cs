using System.Collections.Generic;

namespace System.ComponentModel.Composition.Hosting
{
    public class ExportProvider
    {
        public IEnumerable<Lazy<TTarget, TMetadata>> GetExports<TTarget, TMetadata>()
        {
            throw new NotImplementedException();
        }
    }
}