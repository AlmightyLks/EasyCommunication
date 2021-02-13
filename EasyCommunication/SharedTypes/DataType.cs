namespace EasyCommunication.SharedTypes
{
    public enum DataType : byte
    {
        HostHeartbeat = 1,
        ClientHeartbeat,
        Disconnect,
        ByteArray,
        ProtoBuf,
        JsonString,
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
