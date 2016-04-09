using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace NRack.Base.Metadata
{
    [Serializable]
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [StatusInfo(StatusInfoKeys.IsRunning, Name = "Is Running", DataType = typeof(bool), Order = 100)]
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
