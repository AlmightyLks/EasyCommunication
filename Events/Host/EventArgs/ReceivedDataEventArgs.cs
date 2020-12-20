using EasyCommunication.SharedTypes;
using System.Net.Sockets;

namespace EasyCommunication.Events.Host.EventArgs
{
    public class ReceivedDataEventArgs : IHostEventArgs
    {
        public TcpClient Sender { get; internal set; }
        public int Port { get; internal set; }
        public object Data { get; internal set; }
        public bool IsHeartbeat { get; internal set; }
    }
}
