using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if DOTNETCORE
using System.Composition;
#else
using System.ComponentModel.Composition;
#endif


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
