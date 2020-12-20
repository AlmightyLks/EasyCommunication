using EasyCommunication.SharedTypes;
using System.Net.Sockets;

namespace EasyCommunication.Events.Host.EventArgs
{
    public class ReceivedDataEventArgs : IHostEventArgs
    {
        /// <summary>
        /// The sender's TcpClient connection
        /// </summary>
        public TcpClient Sender { get; internal set; }
        
        /// <summary>
        /// The Sender's port which is used for communication
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// The received data
        /// </summary>
        public object Data { get; internal set; }

        /// <summary>
        /// Whether the received data is a Heartbeat
        /// </summary>
        public bool IsHeartbeat { get; internal set; }
    }
}
