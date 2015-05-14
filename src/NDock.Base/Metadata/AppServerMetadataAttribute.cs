using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace NDock.Base.Metadata
{
    [Serializable]
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AppServerMetadataAttribute : ExportAttribute, IAppServerMetadata
    {
        public string Name { get; set; }

        public AppServerMetadataAttribute()
            : base(typeof(IAppServer))
        {

        }

        public AppServerMetadataAttribute(string name)
            : this()
        {
            Name = name;
        }
    }
}
