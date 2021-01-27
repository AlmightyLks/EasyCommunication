namespace EasyCommunication.Events.Client
{
    public class Events
    {
        /// <summary>
        /// Delegate for Communication Events for the <see cref="EasyCommunication.Client.Connection.EasyClient"/>
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="ev"></param>
        public delegate void CommunicationEvent<TEvent>(TEvent ev) where TEvent : IClientEventArgs;
    }
}
