using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base.Metadata
{
    public class AppServerMetadata
    {
        public static AppServerMetadata GetAppServerMetadata(Type serverType)
        {
            if (serverType == null)
                throw new ArgumentNullException("serverType");

            var attType = typeof(AppServerMetadataTypeAttribute);

            while (true)
            {
                var atts = serverType.GetCustomAttributes(attType, false);

                if (atts != null && atts.Length > 0)
                {
                    var serverMetadataTypeAtt = atts[0] as AppServerMetadataTypeAttribute;
                    return Activator.CreateInstance(serverMetadataTypeAtt.MetadataType) as AppServerMetadata;
                }

                if (serverType.BaseType == null)
                    return null;

                serverType = serverType.BaseType;
            }
        }
    }
}
