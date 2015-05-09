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
        public static AppServerMetadataAttribute GetAppServerMetadata(Type serverType)
        {
            if (serverType == null)
                throw new ArgumentNullException("serverType");

            var topType = serverType;

            var attType = typeof(AppServerMetadataAttribute);

            while (true)
            {
                var atts = serverType.GetCustomAttributes(attType, false);

                if (atts != null && atts.Length > 0)
                {
                    var metatdata = atts[0] as AppServerMetadataAttribute;
                    metatdata.AppType = topType.ToString();
                    return metatdata;
                }

                if (serverType.BaseType == null)
                    return null;

                serverType = serverType.BaseType;
            }
        }

        public string Name { get; set; }

        public string AppType { get; set; }

        /// <summary>
        /// Gets/sets the status fields.
        /// </summary>
        /// <value>
        /// The status fields.
        /// </value>
        public StatusInfoAttribute[] StatusFields { get; set; }

        public AppServerMetadataAttribute()
        {
            var attType = this.GetType();

            StatusFields = attType
                .GetCustomAttributes(typeof(StatusInfoAttribute), true)
                .OfType<StatusInfoAttribute>().ToArray();

            AppType = this.ContractType.ToString();
        }

        public AppServerMetadataAttribute(string name)
            : this()
        {
            Name = name;
        }
    }
}
