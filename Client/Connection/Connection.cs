using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace EasyCommunication.Client.Connection
{
    public struct Connection
    {
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }
    }
}
