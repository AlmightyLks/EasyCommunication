using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCommunication.SharedTypes
{
    public struct ReceivedBuffer
    {
        public DataType DataType { get; set; }
        public byte[] Data { get; set; }
    }
}
