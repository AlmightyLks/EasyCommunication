using ProtoBuf;
using System;
using System.IO;

namespace EasyCommunication.Helper
{
    public static class ProtoBufExtension
    {
        public static bool TryDeserialize<T>(this byte[] data, out T result)
        {
            Type type = typeof(T);
            bool success = data.TryDeserialize(out object obj, type);
            result = (T)obj;
            return success;
        }
        public static bool TryDeserialize(this byte[] data, out object result, Type type)
        {
            result = null;

            try
            {
                if (Serializer.NonGeneric.CanSerialize(type))
                {
                    result = Serializer.Deserialize(type, new MemoryStream(data));
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
    }
}
