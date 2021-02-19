using EasyCommunication.SharedTypes;
using EasyCommunication.Events.Client.EventArgs;
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

namespace CommunicationTests.DataTransmission
{
    //Ports 9450 - 9599
    [Collection("Sequential")]
    public class EasyHostTransmission
    {
        [Fact]
        public async Task TransmitShort()
        {
            for (int i = 0; i < 1; i++)
            {
                short result = 0;
                short expected = 1;

                var host = new EasyHost(1000, 9450, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9450);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Short)
                        result = BitConverter.ToInt16(ev.ReceivedBuffer.Data);
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.Short);

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

                var host = new EasyHost(1000, 9451, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9451);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Int)
                        result = BitConverter.ToInt32(ev.ReceivedBuffer.Data);
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.Int);

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

                var host = new EasyHost(1000, 9452, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9452);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Long)
                        result = BitConverter.ToInt64(ev.ReceivedBuffer.Data);
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.Long);

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

                var host = new EasyHost(1000, 9453, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9453);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Float)
                        result = BitConverter.ToSingle(ev.ReceivedBuffer.Data);
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.Float);

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

                var host = new EasyHost(1000, 9454, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9454);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Double)
                        result = BitConverter.ToDouble(ev.ReceivedBuffer.Data);
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.Double);

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

                var host = new EasyHost(1000, 9455, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9455);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Bool)
                        result = BitConverter.ToBoolean(ev.ReceivedBuffer.Data);
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.Bool);

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

                var host = new EasyHost(1000, 9456, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9456);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.String)
                        result = Encoding.UTF8.GetString(ev.ReceivedBuffer.Data);
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.String);

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

                var host = new EasyHost(1000, 9457, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9457);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.JsonString)
                        result = JsonConvert.DeserializeObject<Point>(Encoding.UTF8.GetString(ev.ReceivedBuffer.Data));
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.JsonString);

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

                var host = new EasyHost(1000, 9458, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9458);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.ProtoBuf)
                    {
                        using MemoryStream memStream = new MemoryStream(ev.ReceivedBuffer.Data);
                        memStream.Position = 0;
                        result = Serializer.Deserialize<ProtoBufType>(memStream);
                    }
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.ProtoBuf);

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

                var host = new EasyHost(1000, 9459, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9459);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.ByteArray)
                    {
                        result = ev.ReceivedBuffer.Data;
                    }
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.ByteArray);

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

                var host = new EasyHost(1000, 9460, IPAddress.Loopback)
                {
                    BufferSize = 1_050_000
                };
                host.Open();
                var client = new EasyClient(500)
                {
                    BufferSize = 1_050_000
                };
                client.ConnectToHost(IPAddress.Loopback, 9460);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.ByteArray)
                    {
                        result = ev.ReceivedBuffer.Data;
                    }
                };
                var status = host.QueueData(expected, host.ClientConnections.Keys.ToArray()[0], DataType.ByteArray);

                await Task.Delay(125);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task Transmit5Integers()
        {
            for (int i = 0; i < 1; i++)
            {
                List<int> expected = new List<int>()
                {
                    1, 2, 3, 4, 5
                };
                List<int> result = new List<int>();

                var host = new EasyHost(1000, 9461, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9461);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Int)
                    {
                        result.Add(BitConverter.ToInt32(ev.ReceivedBuffer.Data));
                    }
                };
                foreach (var item in expected)
                    host.QueueData(item, host.ClientConnections.Keys.ToArray()[0], DataType.Int);

                await Task.Delay(75);

                Assert.Equal(expected, result.OrderBy(_ => _).ToList());

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task Transmit50Integers()
        {
            for (int i = 0; i < 1; i++)
            {
                List<int> expected = Enumerable.Range(0, 50).ToList();
                List<int> result = new List<int>();

                var host = new EasyHost(1000, 9462, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                client.ConnectToHost(IPAddress.Loopback, 9462);
                await Task.Delay(5);

                client.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.ReceivedBuffer.DataType == DataType.Int)
                    {
                        result.Add(BitConverter.ToInt32(ev.ReceivedBuffer.Data));
                    }
                };
                foreach (var item in expected)
                    host.QueueData(item, host.ClientConnections.Keys.ToArray()[0], DataType.Int);

                await Task.Delay(1000);

                Assert.Equal(expected, result.OrderBy(_ => _).ToList());

                client.DisconnectFromHost();
                host.Close();
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
