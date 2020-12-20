using EasyCommunication.Events.Client.EventArgs;
using static EasyCommunication.Events.Client.Events;

namespace EasyCommunication.Events.Client.EventHandler
{
    public class ClientEventHandler
    {
        public event OnCommunicationEvent<ConnectedToHostEventArgs> ConnectedToHost;
        public event OnCommunicationEvent<ReceivedDataEventArgs> ReceivedData;
        public event OnCommunicationEvent<SendingDataEventArgs> SendingData;

        internal void InvokeConnectedToHost(ConnectedToHostEventArgs ev) => ConnectedToHost?.Invoke(ev);
        internal void InvokeReceivedData(ReceivedDataEventArgs ev) => ReceivedData?.Invoke(ev);
        internal void InvokeSendingData(SendingDataEventArgs ev) => SendingData?.Invoke(ev);
    }
}
