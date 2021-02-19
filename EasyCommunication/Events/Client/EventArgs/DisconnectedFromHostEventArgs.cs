using EasyCommunication.SharedTypes;

namespace EasyCommunication.Events.Client.EventArgs
{
    public class DisconnectedFromHostEventArgs : IClientEventArgs
    {
        public Connection? Connection { get; internal set; }
    }
}
