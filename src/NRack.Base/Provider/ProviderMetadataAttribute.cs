using System;
#if DOTNETCORE
using System.Composition;
#else
using System.ComponentModel.Composition;
#endif

namespace NRack.Base.Provider
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
