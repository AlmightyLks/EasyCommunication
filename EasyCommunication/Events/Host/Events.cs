namespace EasyCommunication.Events.Host
{
    public class Events
    {
        /// <summary>
        /// Delegate for Communication Events for the <see cref="EasyCommunication.Host.Connection.EasyHost"/>
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="ev"></param>
        public delegate void CommunicationEvent<TEvent>(TEvent ev) where TEvent : IHostEventArgs;
    }
}
