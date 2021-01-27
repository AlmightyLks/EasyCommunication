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
        /// 
        /// </summary>
        public DataType Type { get; internal set; }

        /// <summary>
        /// The data that is about to be sent
        /// </summary>
        public byte[] Data { get; internal set; }

        /// <summary>
        /// Whether or not the data transmittion shall be allowed
        /// </summary>
        public bool Allow { get; set; }
    }
}
