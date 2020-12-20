namespace EasyCommunication.Events.Host
{
    public class Events
    {
        public delegate void OnCommunicationEvent<TEvent>(TEvent ev) where TEvent : IHostEventArgs;
    }
}
