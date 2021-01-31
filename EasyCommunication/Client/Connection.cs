using System.Net;

namespace EasyCommunication.Client
{
    /// <summary>
    /// Information about the IPAddress and the Port from the <see cref="Connection"/>
    /// </summary>
    public struct Connection
    {
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }
    }
}
