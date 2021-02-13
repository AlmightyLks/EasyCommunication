using EasyCommunication.Connection;
using EasyCommunication.SharedTypes;
using System;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class ReceivedDataEventArgs : IClientEventArgs
    {
        public Connection.Connection? Sender { get; internal set; }
        public DataType Type { get; internal set; }
        public byte[] Data { get; internal set; }
    }
}
