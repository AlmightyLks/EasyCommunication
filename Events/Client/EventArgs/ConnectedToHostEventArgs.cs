using System.Net.Sockets;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class ConnectedToHostEventArgs : IClientEventArgs
    {
        public TcpClient Client { get; internal set; }
        public int HostPort { get; internal set; }
    }
}
