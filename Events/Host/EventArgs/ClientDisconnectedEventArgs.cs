using System.Net.Sockets;

namespace EasyCommunication.Events.Host.EventArgs
{
    public class ClientDisconnectedEventArgs : IHostEventArgs
    {
        /// <summary>
        /// The disconnected TcpClient
        /// </summary>
        public TcpClient Client { get; internal set; }

        /// <summary>
        /// The client's port which was used for communication
        /// </summary>
        public int Port { get; internal set; }
    }
}
