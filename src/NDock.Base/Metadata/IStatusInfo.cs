using System;

namespace NDock.Base
{
    public interface IStatusInfo
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key
        /// </value>
        string Key { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the short name.
        /// </summary>
        /// <value>
        /// The short name.
        /// </value>
        string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        string Format { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        int Order { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether [output in perf log].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [output in perf log]; otherwise, <c>false</c>.
        /// </value>
        bool OutputInPerfLog { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        Type DataType { get; set; }
    }
}

