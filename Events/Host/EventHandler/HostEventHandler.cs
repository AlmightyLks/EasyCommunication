using EasyCommunication.Events.Host.EventArgs;
using static EasyCommunication.Events.Host.Events;

namespace EasyCommunication.Events.Host.EventHandler
{
    /// <summary>
    /// EventHandler for <see cref="EasyCommunication.Host.Connection.EasyHost"/>-Events
    /// </summary>
    public class HostEventHandler
    {
        /// <summary>
        /// Event fired when an <see cref="EasyCommunication.Client.Connection.EasyClient"/> connects
        /// </summary>
        public event OnCommunicationEvent<ClientConnectedEventArgs> ClientConnected;

        /// <summary>
        /// Event fired when an <see cref="EasyCommunication.Client.Connection.EasyClient"/> disconnects
        /// </summary>
        public event OnCommunicationEvent<ClientDisconnectedEventArgs> ClientDisconnected;

        /// <summary>
        /// Event fired when data is received from an <see cref="EasyCommunication.Client.Connection.EasyClient"/>
        /// </summary>
        public event OnCommunicationEvent<ReceivedDataEventArgs> ReceivedData;

        /// <summary>
        /// Event fired when an <see cref="EasyCommunication.Host.Connection.EasyHost"/> is sending data to an <see cref="EasyCommunication.Client.Connection.EasyClient"/>
        /// </summary>
        public event OnCommunicationEvent<SendingDataEventArgs> SendingData;

        internal void InvokeClientConnected(ClientConnectedEventArgs ev) => ClientConnected?.Invoke(ev);
        internal void InvokeClientDisconnected(ClientDisconnectedEventArgs ev) => ClientDisconnected?.Invoke(ev);
        internal void InvokeReceivedData(ReceivedDataEventArgs ev) => ReceivedData?.Invoke(ev);
        internal void InvokeSendingData(SendingDataEventArgs ev) => SendingData?.Invoke(ev);
    }
}
