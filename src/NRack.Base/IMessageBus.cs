using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NDock.Base
{
    /// <summary>
    /// NDock message bus interface
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Sends message of one specific topic to the specific receiver.
        /// </summary>
        /// <param name="receiver">The receiver.</param>
        /// <param name="topic">The topic.</param>
        /// <param name="message">The message.</param>
        void Send(IAppEndPoint receiver, string topic, object message);

        /// <summary>
        /// Sends message of one specific topic to global system.
        /// </summary>
        /// <param name="topic">The topic.</param>
        /// <param name="message">The message.</param>
        void Send(string topic, object message);

        /// <summary>
        /// Register the topic message handler.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toppic">The toppic.</param>
        /// <param name="topicHandler">The topic handler.</param>
        void On<T>(string toppic, Action<string, T> topicHandler);
    }
}
