using EasyCommunication.Client.Connection;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class DisconnectedFromHostEventArgs : IClientEventArgs
    {
        public Connection? Connection { get; internal set; }
        public bool Reconnect { get; set; }
    }
}
