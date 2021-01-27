using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace EasyCommunication.SharedTypes
{
    public struct QueueEntity
    {
        public byte[] Data { get; set; }
        public TcpClient Receiver { get; set; }
    }
}
