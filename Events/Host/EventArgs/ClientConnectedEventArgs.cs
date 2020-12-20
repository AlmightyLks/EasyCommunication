using EasyCommunication.SharedTypes;
using System.Net.Sockets;

namespace EasyCommunication.Events.Host.EventArgs
{
    public class ClientConnectedEventArgs : IHostEventArgs
    {
        public TcpClient Client { get; internal set; }
        public int Port { get; internal set; }
        public bool Allow { get; set; }
    }
}
