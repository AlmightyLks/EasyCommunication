using EasyCommunication.SharedTypes;
using System;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class ReceivedDataEventArgs : IClientEventArgs
    {
        public Connection? Sender { get; internal set; }
        public ReceivedBuffer ReceivedBuffer { get; internal set; }
    }
}
