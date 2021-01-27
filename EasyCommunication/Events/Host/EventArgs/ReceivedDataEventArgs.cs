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
        /// 
        /// </summary>
        public DataType Type { get; internal set; }

        /// <summary>
        /// The received data
        /// </summary>
        public byte[] Data { get; internal set; }
    }
}
