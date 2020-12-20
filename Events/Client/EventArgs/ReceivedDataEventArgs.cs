using EasyCommunication.Client.Connection;
using EasyCommunication.SharedTypes;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class ReceivedDataEventArgs : IClientEventArgs
    {
        public Connection? Sender { get; internal set; }
        public object Data { get; internal set; }
        public bool IsHeartbeat { get; internal set; }
    }
}
