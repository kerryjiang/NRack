using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base.Metadata
{
    /// <summary>
    /// Server StatusInfo Metadata
    /// </summary>
    public class StatusInfoKeys
    {
        #region Shared

        /// <summary>
        /// The cpu usage
        /// </summary>
        public const string CpuUsage = "CpuUsage";

        /// <summary>
        /// The memory usage
        /// </summary>
        public const string MemoryUsage = "MemoryUsage";

        /// <summary>
        /// The total thread count
        /// </summary>
        public const string TotalThreadCount = "TotalThreadCount";

        /// <summary>
        /// The available working threads count
        /// </summary>
        public const string AvailableWorkingThreads = "AvailableWorkingThreads";

        /// <summary>
        /// The available completion port threads count
        /// </summary>
        public const string AvailableCompletionPortThreads = "AvailableCompletionPortThreads";

        /// <summary>
        /// The max working threads count
        /// </summary>
        public const string MaxWorkingThreads = "MaxWorkingThreads";

        /// <summary>
        /// The max completion port threads count
        /// </summary>
        public const string MaxCompletionPortThreads = "MaxCompletionPortThreads";

        #endregion

        #region For server instance
        /// <summary>
        /// 	<c>true</c> if this instance is running; otherwise, <c>false</c>.
        /// </summary>
        public const string IsRunning = "IsRunning";

        #endregion

    }
}
