using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyLog;
using NRack.Base;
using NRack.Base.Provider;

namespace NRack.Server
{
    [Export(typeof(IStatusCollector))]
    [ProviderMetadata("DefaultStatusCollector")]
    class DefaultStatusCollector : IStatusCollector
    {
        private void CollectAppServer(AppServerStatus app, StringBuilder sb)
        {
            var meta = app.Metadata;
            var status = app.DataCollection;

            sb.AppendLine(string.Format("{0} ----------------------------------", status.Name));

            foreach (var info in meta.StatusFields)
            {
                var infoValue = status[info.Key];

                if (infoValue == null)
                    continue;

                sb.AppendLine(string.Format("{0}: {1}", info.Name,
                        string.IsNullOrEmpty(info.Format) ? infoValue : string.Format(info.Format, infoValue)));
            }
        }

        public void Collect(AppServerStatus bootstrapStatus, IEnumerable<AppServerStatus> appStatusList, ILog logger)
        {
            var sb = new StringBuilder();

            CollectAppServer(bootstrapStatus, sb);

            foreach (var app in appStatusList)
            {
                CollectAppServer(app, sb);
            }

            logger.Info(sb.ToString());
        }
    }
}
