using EasyCommunication.SharedTypes;
using System.Net.Sockets;

namespace EasyCommunication.Events.Host.EventArgs
{
    public class SendingDataEventArgs : IHostEventArgs
    {
        /// <summary>
        /// The Receiver's TcpClient connection
        /// </summary>
        public TcpClient Receiver { get; internal set; }

        /// <summary>
        /// The data that is about to be sent
        /// </summary>
        public object Data { get; internal set; }

        /// <summary>
        /// Whether or not the data is a Heartbeat
        /// </summary>
        public bool IsHeartbeat { get; internal set; }

        /// <summary>
        /// Whether or not the transmitted data is a serialized object
        /// </summary>
        /// <remarks>
        /// <para>Indicates that <see cref="Data"/> is either a serialized object or as JSON-String</para>
        /// </remarks>
        public bool IsSerializable { get; internal set; }

        /// <summary>
        /// Whether or not the data transmittion shall be allowed
        /// </summary>
        public bool Allow { get; set; }
    }
}
