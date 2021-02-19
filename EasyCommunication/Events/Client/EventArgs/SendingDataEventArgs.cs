using EasyCommunication.SharedTypes;
using System;
using System.Net;
using System.Net.Sockets;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class SendingDataEventArgs : IClientEventArgs
    {
        public Connection? Receiver { get; internal set; }
        public DataType Type { get; internal set; }
        public object Data { get; internal set; }
        public bool Allow { get; set; }
    }
}
