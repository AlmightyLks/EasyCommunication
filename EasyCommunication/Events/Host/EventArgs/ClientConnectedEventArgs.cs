using EasyCommunication.SharedTypes;
using System.Net.Sockets;

namespace EasyCommunication.Events.Host.EventArgs
{
    public class ClientConnectedEventArgs : IHostEventArgs
    {
        /// <summary>
        /// The connected TcpClient
        /// </summary>
        public TcpClient Client { get; internal set; }

        /// <summary>
        /// The Sender's port which is used for communication
        /// </summary>
        public int Port { get; internal set; }

        /// <summary>
        /// Whether or not the connection shall be allowed
        /// </summary>
        public bool Allow { get; set; }
    }
}
