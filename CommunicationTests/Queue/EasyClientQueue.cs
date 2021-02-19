using EasyCommunication;
using EasyCommunication.SharedTypes;
using EasyCommunication.SharedTypes;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CommunicationTests.Queue
{
    //Ports 9150 - 9199
    [Collection("Sequential")]
    public class EasyClientQueue
    {
        [Fact]
        public async Task QueueStatusIllegalFormat()
        {
            var host = new EasyHost(2000, 9150, IPAddress.Loopback);
            host.Open();

            //Buffer
            await Task.Delay(5);
            var client = new EasyClient(500);
            client.ConnectToHost(IPAddress.Loopback, 9150);

            //Buffer
            await Task.Delay(5);
            QueueStatus status = client.QueueData(new Random(), DataType.ProtoBuf);

            //Buffer
            await Task.Delay(5);
            Assert.True(status == QueueStatus.IllegalFormat);
            client.DisconnectFromHost();
            host.Close();
        }
        [Fact]
        public async Task QueueStatusQueued()
        {
            var host = new EasyHost(2000, 9151, IPAddress.Loopback);
            host.Open();

            //Buffer
            await Task.Delay(5);
            var client = new EasyClient(500);
            client.ConnectToHost(IPAddress.Loopback, 9151);

            //Buffer
            await Task.Delay(5);

            QueueStatus status = client.QueueData("", DataType.String);
            Assert.True(status == QueueStatus.Queued);
            client.DisconnectFromHost();
            host.Close();
        }
        [Fact]
        public async Task QueueStatusBufferTransgression()
        {
            var host = new EasyHost(2000, 9152, IPAddress.Loopback);
            host.Open();

            var client = new EasyClient(500) { BufferSize = 3 };
            client.ConnectToHost(IPAddress.Loopback, 9152);

            //Buffer
            await Task.Delay(5);
            //Exceed Buffer
            QueueStatus status = client.QueueData(Encoding.UTF8.GetString(new byte[] { 1, 1, 1, 1 }), DataType.String);

            Assert.True(status == QueueStatus.BufferTransgression);
            client.DisconnectFromHost();
            host.Close();
        }
        [Fact]
        public async Task QueueStatusNotOpen()
        {
            //Buffer
            await Task.Delay(5);
            var client = new EasyClient(500);

            //Buffer
            await Task.Delay(5);

            QueueStatus status = client.QueueData("", DataType.String);
            Assert.True(status == QueueStatus.NotOpen);
        }
    }
}
