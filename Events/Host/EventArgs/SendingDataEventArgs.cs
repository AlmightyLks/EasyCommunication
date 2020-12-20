using EasyCommunication.SharedTypes;
using System.Net.Sockets;

namespace EasyCommunication.Events.Host.EventArgs
{
    public class SendingDataEventArgs : IHostEventArgs
    {
        public TcpClient Receiver { get; internal set; }
        public object Data { get; internal set; }
        public bool IsHeartbeat { get; internal set; }
        public bool Allow { get; set; }
    }
}
