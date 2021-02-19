using EasyCommunication.SharedTypes;
using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.Serialization;
using EasyCommunication.SharedTypes;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using EasyCommunication;
using System.Diagnostics;

namespace CommunicationTests.DataTransmission
{
    //Ports 9400 - 9449
    [Collection("Sequential")]
    public class EasyClientTransmission
    {
        [Fact]
        public async Task TransmitShort()
        {
            for (int i = 0; i < 1; i++)
            {
                short result = 0;
                short expected = 1;

                var host = new EasyHost(1000, 9400, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9400);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Short)
                        result = BitConverter.ToInt16(ev.ReceivedBuffer.Data);
                };
                var status = client.QueueData(expected, DataType.Short);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitInteger()
        {
            for (int i = 0; i < 1; i++)
            {
                int result = 0;
                int expected = 1;

                var host = new EasyHost(1000, 9401, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9401);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Int)
                        result = BitConverter.ToInt32(ev.ReceivedBuffer.Data);
                };
                var status = client.QueueData(expected, DataType.Int);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitLong()
        {
            for (int i = 0; i < 1; i++)
            {
                long result = 0;
                long expected = 1;

                var host = new EasyHost(1000, 9402, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9402);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Long)
                        result = BitConverter.ToInt64(ev.ReceivedBuffer.Data);
                };
                var status = client.QueueData(expected, DataType.Long);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitFloat()
        {
            for (int i = 0; i < 1; i++)
            {
                float result = 0;
                float expected = 1.1f;

                var host = new EasyHost(1000, 9403, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9403);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Float)
                        result = BitConverter.ToSingle(ev.ReceivedBuffer.Data);
                };
                var status = client.QueueData(expected, DataType.Float);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitDouble()
        {
            for (int i = 0; i < 1; i++)
            {
                double result = 0;
                double expected = 1.1d;

                var host = new EasyHost(1000, 9404, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9404);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Double)
                        result = BitConverter.ToDouble(ev.ReceivedBuffer.Data);
                };
                var status = client.QueueData(expected, DataType.Double);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitBool()
        {
            for (int i = 0; i < 1; i++)
            {
                bool result = false;
                bool expected = true;

                var host = new EasyHost(1000, 9405, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9405);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Bool)
                        result = BitConverter.ToBoolean(ev.ReceivedBuffer.Data);
                };
                var status = client.QueueData(expected, DataType.Bool);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitString()
        {
            for (int i = 0; i < 1; i++)
            {
                string result = "Before";
                string expected = "After";

                var host = new EasyHost(1000, 9406, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9406);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.String)
                        result = Encoding.UTF8.GetString(ev.ReceivedBuffer.Data);
                };
                var status = client.QueueData(expected, DataType.String);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitJsonString()
        {
            for (int i = 0; i < 1; i++)
            {
                Point result = new Point(0, 0);
                Point expected = new Point(1, 1);

                var host = new EasyHost(1000, 9407, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9407);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.JsonString)
                        result = JsonConvert.DeserializeObject<Point>(Encoding.UTF8.GetString(ev.ReceivedBuffer.Data));
                };
                var status = client.QueueData(expected, DataType.JsonString);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitProtoBuf()
        {
            for (int i = 0; i < 1; i++)
            {
                ProtoBufType result = new ProtoBufType(0, 0);
                ProtoBufType expected = new ProtoBufType(1, 1);

                var host = new EasyHost(1000, 9408, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9408);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.ProtoBuf)
                    {
                        using MemoryStream memStream = new MemoryStream(ev.ReceivedBuffer.Data);
                        memStream.Position = 0;
                        result = Serializer.Deserialize<ProtoBufType>(memStream);
                    }
                };
                var status = client.QueueData(expected, DataType.ProtoBuf);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitByteArray()
        {

            for (int i = 0; i < 1; i++)
            {
                byte[] expected = Enumerable.Repeat<byte>(1, 10).ToArray();
                byte[] result = new byte[0];

                var host = new EasyHost(1000, 9409, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9409);
                await Task.Delay(5);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.ByteArray)
                    {
                        result = ev.ReceivedBuffer.Data;
                    }
                };
                var status = client.QueueData(expected, DataType.ByteArray);

                await Task.Delay(50);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task Transmit1MB()
        {
            for (int i = 0; i < 1; i++)
            {
                byte[] expected = Enumerable.Repeat<byte>(1, 1_048_576).ToArray();
                byte[] result = new byte[0];

                var host = new EasyHost(1000, 9410, IPAddress.Loopback)
                {
                    BufferSize = 1_050_000
                };
                host.Open();
                var client = new EasyClient(500)
                {
                    BufferSize = 1_050_000
                };
                client.ConnectToHost(IPAddress.Loopback, 9410);
                await Task.Delay(5);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.ByteArray)
                    {
                        result = ev.ReceivedBuffer.Data;
                    }
                };
                var status = client.QueueData(expected, DataType.ByteArray);

                await Task.Delay(125);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task Transmit5Integers()
        {
            for (int i = 0; i < 100; i++)
            {
                List<int> expected = new List<int>()
                {
                    1, 2, 3, 4, 5
                };
                List<int> result = new List<int>();

                var host = new EasyHost(1000, 9411, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9411);
                await Task.Delay(5);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Int)
                    {
                        result.Add(BitConverter.ToInt32(ev.ReceivedBuffer.Data));
                    }
                };
                foreach (var item in expected)
                    client.QueueData(item, DataType.Int);

                await Task.Delay(125);

                Assert.Equal(expected, result.OrderBy(_ => _).ToList());

                client.DisconnectFromHost();
                host.Close();
                Debug.WriteLine("-------------------");
            }
        }

        [ProtoContract]
        class ProtoBufType
        {
            [ProtoMember(1)] public int Num1 { get; set; }
            [ProtoMember(2)] public int Num2 { get; set; }

            public static bool operator ==(ProtoBufType left, ProtoBufType right)
                => left.Num1 == right.Num1 && left.Num2 == right.Num2;
            public static bool operator !=(ProtoBufType left, ProtoBufType right)
                => left.Num1 != right.Num1 || left.Num2 != right.Num2;
            public override bool Equals(object obj)
                => obj is ProtoBufType type && type == this;

            public ProtoBufType()
            {
                Num1 = 0;
                Num2 = 0;
            }
            public ProtoBufType(int num1, int num2)
            {
                Num1 = num1;
                Num2 = num2;
            }
        }
    }
}
