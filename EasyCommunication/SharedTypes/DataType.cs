using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCommunication.SharedTypes
{
    public enum DataType : byte
    {
        Heartbeat = 1,
        Disconnect,
        JsonString,
        ProtoBuf,
        Bool,
        Byte,
        Short,
        Int,
        Long ,
        Float,
        Double,
        String
    }
}
