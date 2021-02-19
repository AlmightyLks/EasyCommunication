using EasyCommunication.Serialization;
using EasyCommunication.Helper;
using EasyCommunication.SharedTypes;
using ProtoBuf;
using System.Drawing;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommunicationTests.Serialization
{
    public class DataSerialization
    {
        [Fact]
        public void SerializeShort()
        {
            byte[] expected = new byte[] { (byte)DataType.Short, 2, 0, 0, 0, 2, 0 };
            byte[] result = SerializationHelper.GetBuffer((short)2, DataType.Short);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeInteger()
        {
            byte[] expected = new byte[] { (byte)DataType.Int, 4, 0, 0, 0, 2, 0, 0, 0 };
            byte[] result = SerializationHelper.GetBuffer(2, DataType.Int);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeLong()
        {
            byte[] expected = new byte[] { (byte)DataType.Long, 8, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0 };
            byte[] result = SerializationHelper.GetBuffer((long)2, DataType.Long);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeFloat()
        {
            byte[] expected = new byte[] { (byte)DataType.Float, 4, 0, 0, 0, 205, 204, 140, 63 };
            byte[] result = SerializationHelper.GetBuffer(1.1f, DataType.Float);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeDouble()
        {
            byte[] expected = new byte[] { (byte)DataType.Double, 8, 0, 0, 0, 154, 153, 153, 153, 153, 153, 241, 63 };
            byte[] result = SerializationHelper.GetBuffer(1.1d, DataType.Double);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeBool()
        {
            byte[] expected = new byte[] { (byte)DataType.Bool, 1, 0, 0, 0, 1 };
            byte[] result = SerializationHelper.GetBuffer(true, DataType.Bool);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeString()
        {
            byte[] expected = new byte[] { (byte)DataType.String, 5, 0, 0, 0, 72, 101, 108, 108, 111 };
            byte[] result = SerializationHelper.GetBuffer("Hello", DataType.String);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeJsonString()
        {
            Point test = new Point(1, 1);
            byte[] expected = new byte[] { (byte)DataType.JsonString, 6, 0, 0, 0, 34, 49, 44, 32, 49, 34 };
            byte[] result = SerializationHelper.GetBuffer(test, DataType.JsonString);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeProtoBuf()
        {
            ProtoBufType test = new ProtoBufType(1, 1);
            byte[] expected = new byte[] { (byte)DataType.ProtoBuf, 4, 0, 0, 0, 8, 1, 16, 1 };
            byte[] result = SerializationHelper.GetBuffer(test, DataType.ProtoBuf);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void SerializeByteArray()
        {
            byte[] data = new byte[] { 1, 1, 1 };
            byte[] expected = new byte[] { (byte)DataType.ByteArray, 3, 0, 0, 0, 1, 1, 1 };
            byte[] result = SerializationHelper.GetBuffer(data, DataType.ByteArray);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void ConvertToReceivedBuffer()
        {
            byte[] expectedData = SerializationHelper.GetBuffer(2, DataType.Int);
            ReceivedBuffer[] expected = new ReceivedBuffer[1];

            expected[0] = new ReceivedBuffer()
            {
                DataType = DataType.Int,
                Data = new byte[] { 2, 0, 0, 0 }
            };

            ReceivedBuffer[] result = expectedData.GetStackedBuffers().ToArray();

            Assert.True(ReceivedBuffersEquals(expected, result));
        }
        [Fact]
        public void ConvertToReceivedBuffers()
        {
            byte[] expectedData = new byte[] {
                (byte)DataType.Int, 4, 0, 0, 0, 2, 0, 0, 0,
                (byte)DataType.HostHeartbeat, 0, 0, 0, 0,
                (byte)DataType.Int, 4, 0, 0, 0, 2, 0, 0, 0,
            };
            ReceivedBuffer[] expected = new ReceivedBuffer[3];

            expected[0] = new ReceivedBuffer()
            {
                DataType = DataType.Int,
                Data = new byte[] { 2, 0, 0, 0 }
            };
            expected[1] = new ReceivedBuffer()
            {
                DataType = DataType.HostHeartbeat,
                Data = new byte[0]
            };
            expected[2] = new ReceivedBuffer()
            {
                DataType = DataType.Int,
                Data = new byte[] { 2, 0, 0, 0 }
            };

            ReceivedBuffer[] result = expectedData.GetStackedBuffers().ToArray();

            Assert.True(ReceivedBuffersEquals(expected, result));
        }
        private static bool ReceivedBuffersEquals(ReceivedBuffer[] receivedBuffers1, ReceivedBuffer[] receivedBuffers2)
        {
            if (receivedBuffers1.Length != receivedBuffers2.Length)
                return false;

            for (int i = 0; i < receivedBuffers1.Length; i++)
            {
                if (receivedBuffers1[i].DataType != receivedBuffers2[i].DataType)
                    return false;

                if (receivedBuffers1[i].Data.Length != receivedBuffers2[i].Data.Length)
                    return false;

                for (int j = 0; j < receivedBuffers1[i].Data.Length; j++)
                {
                    if (receivedBuffers1[i].Data[j] != receivedBuffers2[i].Data[j])
                        return false;
                }
            }

            return true;
        }
    }
    [ProtoContract]
    internal class ProtoBufType
    {
        [ProtoMember(1)] public int Num1 { get; set; }
        [ProtoMember(2)] public int Num2 { get; set; }
        public ProtoBufType(int num1, int num2)
        {
            Num1 = num1;
            Num2 = num2;
        }
    }
}
