using EasyCommunication.Events.Client.EventArgs;
using static EasyCommunication.Events.Client.Events;

namespace EasyCommunication.Events.Client.EventHandler
{
    /// <summary>
    /// EventHandler for <see cref="EasyCommunication.Client.Connection.EasyClient"/>-Events
    /// </summary>
    public class ClientEventHandler
    {
        /// <summary>
        /// Event fired when an <see cref="EasyCommunication.Client.Connection.EasyClient"/> connected to an <see cref="EasyCommunication.Host.Connection.EasyHost"/>
        /// </summary>
        public event CommunicationEvent<ConnectedToHostEventArgs> ConnectedToHost;

        /// <summary>
        /// Event fired when data is received from an <see cref="EasyCommunication.Host.Connection.EasyHost"/>
        /// </summary>
        public event CommunicationEvent<ReceivedDataEventArgs> ReceivedData;

        /// <summary>
        /// Event fired when an <see cref="EasyCommunication.Client.Connection.EasyClient"/> is sending data to an <see cref="EasyCommunication.Host.Connection.EasyHost"/>
        /// </summary>
        public event CommunicationEvent<SendingDataEventArgs> SendingData;

        internal void InvokeConnectedToHost(ConnectedToHostEventArgs ev) => ConnectedToHost?.Invoke(ev);
        internal void InvokeReceivedData(ReceivedDataEventArgs ev) => ReceivedData?.Invoke(ev);
        internal void InvokeSendingData(SendingDataEventArgs ev) => SendingData?.Invoke(ev);
    }
}
