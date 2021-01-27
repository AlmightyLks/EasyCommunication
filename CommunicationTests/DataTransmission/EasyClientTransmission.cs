using EasyCommunication.Client.Connection;
using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.Host.Connection;
using EasyCommunication.SharedTypes;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CommunicationTests.DataTransmission
{
    //Ports 9400 - 9449
    [Collection("Sequential")]
    public class EasyClientTransmission
    {
        [Fact]
        public async Task TransmitShort()
        {
            for (int i = 0; i < 3; i++)
            {
                short result = 0;
                short expected = 1;

                var host = new EasyHost(1000, 9400, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9400);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.Short)
                        result = BitConverter.ToInt16(ev.Data);
                };
                var status = client.QueueData(expected, DataType.Short);

                await Task.Delay(150);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitInteger()
        {
            for (int i = 0; i < 3; i++)
            {
                int result = 0;
                int expected = 1;

                var host = new EasyHost(1000, 9401, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9401);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.Int)
                        result = BitConverter.ToInt32(ev.Data);
                };
                var status = client.QueueData(expected, DataType.Int);

                await Task.Delay(150);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitLong()
        {
            for (int i = 0; i < 3; i++)
            {
                long result = 0;
                long expected = 1;

                var host = new EasyHost(1000, 9402, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9402);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.Long)
                        result = BitConverter.ToInt64(ev.Data);
                };
                var status = client.QueueData(expected, DataType.Long);

                await Task.Delay(150);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitFloat()
        {
            for (int i = 0; i < 3; i++)
            {
                float result = 0;
                float expected = 1.1f;

                var host = new EasyHost(1000, 9403, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9403);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.Float)
                        result = BitConverter.ToSingle(ev.Data);
                };
                var status = client.QueueData(expected, DataType.Float);

                await Task.Delay(150);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitDouble()
        {
            for (int i = 0; i < 3; i++)
            {
                double result = 0;
                double expected = 1.1d;

                var host = new EasyHost(1000, 9404, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9404);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.Double)
                        result = BitConverter.ToDouble(ev.Data);
                };
                var status = client.QueueData(expected, DataType.Double);

                await Task.Delay(150);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitBool()
        {
            for (int i = 0; i < 3; i++)
            {
                bool result = false;
                bool expected = true;

                var host = new EasyHost(1000, 9405, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9405);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.Bool)
                        result = BitConverter.ToBoolean(ev.Data);
                };
                var status = client.QueueData(expected, DataType.Bool);

                await Task.Delay(150);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitString()
        {
            for (int i = 0; i < 3; i++)
            {
                string result = "Before";
                string expected = "After";

                var host = new EasyHost(1000, 9406, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9406);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.String)
                        result = Encoding.UTF8.GetString(ev.Data);
                };
                var status = client.QueueData(expected, DataType.String);

                await Task.Delay(150);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitJsonString()
        {
            for (int i = 0; i < 3; i++)
            {
                Point result = new Point(0, 0);
                Point expected = new Point(1, 1);

                var host = new EasyHost(1000, 9407, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9407);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.JsonString)
                        result = JsonConvert.DeserializeObject<Point>(Encoding.UTF8.GetString(ev.Data));
                };
                var status = client.QueueData(expected, DataType.JsonString);

                await Task.Delay(150);

                Assert.Equal(expected, result);

                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TransmitProtoBuf()
        {
            for (int i = 0; i < 3; i++)
            {
                ProtoBufType result = new ProtoBufType(0, 0);
                ProtoBufType expected = new ProtoBufType(1, 1);

                var host = new EasyHost(1000, 9408, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9408);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    if (ev.Type == DataType.ProtoBuf)
                    {
                        using MemoryStream memStream = new MemoryStream(ev.Data);
                        memStream.Position = 0;
                        result = Serializer.Deserialize<ProtoBufType>(memStream);
                    }
                };
                var status = client.QueueData(expected, DataType.ProtoBuf);

                await Task.Delay(150);

                Assert.Equal(expected, result);

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
