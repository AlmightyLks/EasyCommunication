using EasyCommunication.Events.Client.EventArgs;
using static EasyCommunication.Events.Client.Events;

namespace EasyCommunication.Events.Client.EventHandler
{
    /// <summary>
    /// EventHandler for <see cref="EasyCommunication.Client.EasyClient"/>-Events
    /// </summary>
    public class ClientEventHandler
    {
        /// <summary>
        /// Event fired when an <see cref="EasyCommunication.Client.EasyClient"/> connected to an <see cref="EasyCommunication.Host.EasyHost"/>
        /// </summary>
        public event CommunicationEvent<ConnectedToHostEventArgs> ConnectedToHost;

        /// <summary>
        /// Event fired when an <see cref="EasyCommunication.Client.EasyClient"/> loses connection from an <see cref="EasyCommunication.Host.EasyHost"/>
        /// </summary>
        public event CommunicationEvent<DisconnectedFromHostEventArgs> DisconnectedFromHost;

        /// <summary>
        /// Event fired when data is received from an <see cref="EasyCommunication.Host.EasyHost"/>
        /// </summary>
        public event CommunicationEvent<ReceivedDataEventArgs> ReceivedData;

        /// <summary>
        /// Event fired when an <see cref="EasyCommunication.Client.EasyClient"/> is sending data to an <see cref="EasyCommunication.Host.EasyHost"/>
        /// </summary>
        public event CommunicationEvent<SendingDataEventArgs> SendingData;


        internal void InvokeConnectedToHost(ConnectedToHostEventArgs ev) => ConnectedToHost?.Invoke(ev);
        internal void InvokeDisconnectedFromHost(DisconnectedFromHostEventArgs ev) => DisconnectedFromHost?.Invoke(ev);
        internal void InvokeReceivedData(ReceivedDataEventArgs ev) => ReceivedData?.Invoke(ev);
        internal void InvokeSendingData(SendingDataEventArgs ev) => SendingData?.Invoke(ev);
    }
}
