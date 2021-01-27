using EasyCommunication.Client.Connection;
using EasyCommunication.Host.Connection;
using Moq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace CommunicationTests.Connection
{
    //Ports 9000 - 9049
    [Collection("Sequential")]
    public class EasyClientConnection
    {
        [Fact]
        public void ConnectedFalseWhenNotConnected()
        {
            for (int i = 0; i < 5; i++)
            {
                var easyClient = new EasyClient();
                Assert.False(easyClient.ClientConnected);
            }
        }
        [Fact]
        public async Task ConnectedTrueWhenConnected()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(500, 9000, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);

                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9000);

                //Buffer
                await Task.Delay(10);

                Assert.True(client.ClientConnected);
                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task ConnectedFalseWhenDisconnected()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(500, 9001, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);

                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9001);

                //Buffer
                await Task.Delay(10);

                client.DisconnectFromHost();

                //Buffer
                await Task.Delay(10);

                Assert.False(client.ClientConnected);
                host.Close();
            }
        }
        [Fact]
        public async Task ConnectTrueWhenReconnect()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(500, 9002, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);

                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9002);

                //Buffer
                await Task.Delay(10);
                client.DisconnectFromHost();

                //Buffer
                await Task.Delay(250);

                client.ConnectToHost(IPAddress.Loopback, 9002);

                //Buffer
                await Task.Delay(50);
                Assert.True(client.ClientConnected);
                client.DisconnectFromHost();
                host.Close();
            }
        }
    }
}