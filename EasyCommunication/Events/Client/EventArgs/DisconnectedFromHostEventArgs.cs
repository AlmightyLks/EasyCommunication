using EasyCommunication.Connection;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class DisconnectedFromHostEventArgs : IClientEventArgs
    {
        public Connection.Connection? Connection { get; internal set; }
    }
}
