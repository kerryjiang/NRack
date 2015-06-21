using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDock.Server.Isolation
{
    public class PerformanceCounterInfo
    {
        /// <summary>
        /// Gets or sets the performance counter category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the performance counter name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }


        /// <summary>
        /// Gets or sets the status information key.
        /// </summary>
        /// <value>
        /// The status information key.
        /// </value>
        public string StatusInfoKey { get; set; }

        /// <summary>
        /// Gets or sets the performacne counter data reader.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        public Func<float, object> Read { get; set; }
    }
}
