using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyLog;
using NDock.Base;
using NDock.Base.Provider;

namespace NDock.Server
{
    [Export(typeof(IStatusCollector))]
    [ProviderMetadata("DefaultStatusCollector")]
    class DefaultStatusCollector : IStatusCollector
    {
        public void Collect(IEnumerable<KeyValuePair<AppServerMetadata, StatusInfoCollection>> appStatusList, ILog logger)
        {
            var sb = new StringBuilder();

            foreach (var app in appStatusList)
            {
                var meta = app.Key;
                var status = app.Value;

                sb.AppendLine(string.Format("{0} ----------------------------------", meta.Name));

                foreach(var info in meta.StatusFields)
                {
                    var infoValue = status[info.Key];

                    if (infoValue == null)
                        continue;

                    sb.AppendLine(string.Format("{0}: {1}", info.Name,
                            string.IsNullOrEmpty(info.Format) ? infoValue : string.Format(info.Format, infoValue)));
                }
            }

            logger.Info(sb.ToString());
        }
    }
}
