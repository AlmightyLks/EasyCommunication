namespace EasyCommunication.Events.Client
{
    public class Events
    {
        public delegate void OnCommunicationEvent<TEvent>(TEvent ev) where TEvent : IClientEventArgs;
    }
}
