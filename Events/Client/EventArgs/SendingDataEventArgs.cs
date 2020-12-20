using EasyCommunication.Client.Connection;
using EasyCommunication.SharedTypes;
using System.Net;
using System.Net.Sockets;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class SendingDataEventArgs : IClientEventArgs
    {
        public Connection? Receiver { get; internal set; }
        public object Data { get; internal set; }
        public bool IsHeartbeat { get; internal set; }
        public bool IsSerializable { get; internal set; }
        public bool Allow { get; set; }
    }
}
