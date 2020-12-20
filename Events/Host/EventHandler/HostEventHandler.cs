using EasyCommunication.Events.Host.EventArgs;
using static EasyCommunication.Events.Host.Events;

namespace EasyCommunication.Events.Host.EventHandler
{
    public class HostEventHandler
    {
        public event OnCommunicationEvent<ClientConnectedEventArgs> ClientConnected;
        public event OnCommunicationEvent<ReceivedDataEventArgs> ReceivedData;
        public event OnCommunicationEvent<SendingDataEventArgs> SendingData;

        internal void InvokeClientConnected(ClientConnectedEventArgs ev) => ClientConnected?.Invoke(ev);
        internal void InvokeReceivedData(ReceivedDataEventArgs ev) => ReceivedData?.Invoke(ev);
        internal void InvokeSendingData(SendingDataEventArgs ev) => SendingData?.Invoke(ev);
    }
}
