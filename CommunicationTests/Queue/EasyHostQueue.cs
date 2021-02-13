using EasyCommunication.Connection;
using EasyCommunication.SharedTypes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CommunicationTests.Queue
{
    //Ports 9200 - 9249
    [Collection("Sequential")]
    public class EasyHostQueue
    {
        [Fact]
        public async Task QueueStatusIllegalFormat()
        {
            var host = new EasyHost(2000, 9200, IPAddress.Loopback);
            host.Open();

            //Buffer
            await Task.Delay(5);
            QueueStatus status = host.QueueData(new Random(), null, DataType.ProtoBuf);

            Assert.True(status == QueueStatus.IllegalFormat);
            host.Close();
        }
        [Fact]
        public async Task QueueStatusQueued()
        {
            var host = new EasyHost(2000, 9201, IPAddress.Loopback);
            host.Open();

            //Buffer
            await Task.Delay(5);
            QueueStatus status = host.QueueData("", null, DataType.String);

            Assert.True(status == QueueStatus.Queued);
            host.Close();
        }
        [Fact]
        public async Task QueueStatusBufferTransgression()
        {
            var host = new EasyHost(2000, 9202, IPAddress.Loopback)
            {
                BufferSize = 3
            };
            host.Open();

            //Buffer
            await Task.Delay(5);
            //Exceed Buffer
            QueueStatus status = host.QueueData(Encoding.UTF8.GetString(new byte[] { 1, 1, 1, 1 }), null, DataType.String);

            Assert.True(status == QueueStatus.BufferTransgression);
            host.Close();
        }
        [Fact]
        public async Task QueueStatusNotOpen()
        {
            var host = new EasyHost(2000, 9203, IPAddress.Loopback);
            
            //Buffer
            await Task.Delay(5);
            //Exceed Buffer
            QueueStatus status = host.QueueData("", null, DataType.String);

            Assert.True(status == QueueStatus.NotOpen);
        }
    }
}
