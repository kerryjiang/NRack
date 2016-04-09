using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDock.Base.Provider
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ProviderMetadataAttribute : ExportAttribute, IProviderMetadata
    {
        public string Name { get; private set; }

        public ProviderMetadataAttribute(string name)
        {
            Name = name;
        }
    }
}
