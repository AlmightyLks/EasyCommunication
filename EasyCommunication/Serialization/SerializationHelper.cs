using EasyCommunication.SharedTypes;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace EasyCommunication.Serialization
{
    public static class SerializationHelper
    {
        public static byte[] GetBuffer<T>(T data, DataType dataType, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            byte[] buffer = new byte[0];
            switch (dataType)
            {
                case DataType.ClientHeartbeat:
                    {
                        buffer = new byte[] { (byte)DataType.ClientHeartbeat };
                        WriteLength(buffer, (uint)buffer.Length);
                        break;
                    }
                case DataType.HostHeartbeat:
                    {
                        buffer = new byte[] { (byte)DataType.HostHeartbeat };
                        WriteLength(buffer, (uint)buffer.Length);
                        break;
                    }
                case DataType.JsonString:
                    {
                        var jsonString = JsonConvert.SerializeObject(data);
                        byte[] arr = encoding.GetBytes(jsonString);
                        buffer = new byte[arr.Length + 5];
                        WriteLength(buffer, (uint)arr.Length);
                        buffer[0] = (byte)DataType.JsonString;
                        Array.Copy(arr, 0, buffer, 5, arr.Length);
                        break;
                    }
                case DataType.ByteArray:
                    {
                        byte[] arr = (byte[])(object)data;
                        buffer = new byte[arr.Length + 5];
                        WriteLength(buffer, (uint)arr.Length);
                        buffer[0] = (byte)DataType.ByteArray;
                        Array.Copy(arr, 0, buffer, 5, arr.Length);
                        break;
                    }
                case DataType.ProtoBuf when Serializer.NonGeneric.CanSerialize(typeof(T)):
                    {
                        byte[] arr;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Serializer.Serialize(stream, data);
                            arr = stream.ToArray();
                            buffer = new byte[arr.Length + 5];
                            WriteLength(buffer, (uint)arr.Length);
                            buffer[0] = (byte)DataType.ProtoBuf;
                            Array.Copy(arr, 0, buffer, 5, arr.Length);
                        }
                        break;
                    }
                default:
                    {
                        switch (dataType)
                        {
                            case DataType.Bool:
                                {
                                    byte[] arr = BitConverter.GetBytes((bool)(object)data);
                                    buffer = new byte[arr.Length + 5];
                                    WriteLength(buffer, (uint)arr.Length);
                                    buffer[0] = (byte)DataType.Bool;
                                    Array.Copy(arr, 0, buffer, 5, arr.Length);
                                    break;
                                }
                            case DataType.Byte:
                                {
                                    byte[] arr = new byte[] { (byte)(object)data };
                                    buffer = new byte[arr.Length + 5];
                                    WriteLength(buffer, (uint)arr.Length);
                                    buffer[0] = (byte)DataType.Byte;
                                    Array.Copy(arr, 0, buffer, 5, arr.Length);
                                    break;
                                }
                            case DataType.Short:
                                {
                                    byte[] arr = BitConverter.GetBytes((short)(object)data);
                                    buffer = new byte[arr.Length + 5];
                                    WriteLength(buffer, (uint)arr.Length);
                                    buffer[0] = (byte)DataType.Short;
                                    Array.Copy(arr, 0, buffer, 5, arr.Length);
                                    break;
                                }
                            case DataType.Int:
                                {
                                    byte[] arr = BitConverter.GetBytes((int)(object)data);
                                    buffer = new byte[arr.Length + 5];
                                    WriteLength(buffer, (uint)arr.Length);
                                    buffer[0] = (byte)DataType.Int;
                                    Array.Copy(arr, 0, buffer, 5, arr.Length);
                                    break;
                                }
                            case DataType.Long:
                                {
                                    byte[] arr = BitConverter.GetBytes((long)(object)data);
                                    buffer = new byte[arr.Length + 5];
                                    WriteLength(buffer, (uint)arr.Length);
                                    buffer[0] = (byte)DataType.Long;
                                    Array.Copy(arr, 0, buffer, 5, arr.Length);
                                    break;
                                }
                            case DataType.Float:
                                {
                                    byte[] arr = BitConverter.GetBytes((float)(object)data);
                                    buffer = new byte[arr.Length + 5];
                                    WriteLength(buffer, (uint)arr.Length);
                                    buffer[0] = (byte)DataType.Float;
                                    Array.Copy(arr, 0, buffer, 5, arr.Length);
                                    break;
                                }
                            case DataType.Double:
                                {
                                    byte[] arr = BitConverter.GetBytes((double)(object)data);
                                    buffer = new byte[arr.Length + 5];
                                    WriteLength(buffer, (uint)arr.Length);
                                    buffer[0] = (byte)DataType.Double;
                                    Array.Copy(arr, 0, buffer, 5, arr.Length);
                                    break;
                                }
                            case DataType.String:
                                {
                                    byte[] arr = encoding.GetBytes((string)(object)data);
                                    buffer = new byte[arr.Length + 5];
                                    WriteLength(buffer, (uint)arr.Length);
                                    buffer[0] = (byte)DataType.String;
                                    Array.Copy(arr, 0, buffer, 5, arr.Length);
                                    break;
                                }
                            default:
                                throw new SerializationException("Illegal Format");
                        }
                        break;
                    }
            }
            return buffer;
        }
        private static void WriteLength(byte[] buffer, uint length)
        {
            var lengthBytes = BitConverter.GetBytes(length);
            buffer[1] = lengthBytes[0];
            buffer[2] = lengthBytes[1];
            buffer[3] = lengthBytes[2];
            buffer[4] = lengthBytes[3];
        }
    }
}
