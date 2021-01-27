using EasyCommunication.Client.Connection;
using EasyCommunication.Host.Connection;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace CommunicationTests.Connection
{
    //Ports 9050 - 9099
    [Collection("Sequential")]
    public class EasyHostConnection
    {
        [Fact]
        public async Task StartsListeningOnOpen()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(500, 9050, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);
                Assert.True(host.Listening);
                host.Close();
            }
        }
        [Fact]
        public async Task StopsListeningOnClose()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(500, 9051, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);
                host.Close();

                //Buffer
                await Task.Delay(10);
                Assert.False(host.Listening);
            }
        }
        [Fact]
        public async Task ClientConnectionAddedWhenConnected()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(500, 9052, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);
                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9052);

                //Buffer
                await Task.Delay(10);
                Assert.True(host.ClientConnections.Count == 1);
                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TwoClientsConnectToHost()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(500, 9053, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);
                var clientOne = new EasyClient();
                var clientTwo = new EasyClient();
                clientOne.ConnectToHost(IPAddress.Loopback, 9053);
                clientTwo.ConnectToHost(IPAddress.Loopback, 9053);

                await CheckConnectionCount(host, 2);

                Assert.True(host.ClientConnections.Count == 2);

                clientOne.DisconnectFromHost();
                clientTwo.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task ClientReconnects()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(500, 9054, IPAddress.Loopback);
                host.Open();

                var client = new EasyClient();
                client.ConnectToHost(IPAddress.Loopback, 9054);

                //Buffer
                await Task.Delay(10);
                client.DisconnectFromHost();

                //Buffer
                await Task.Delay(250);
                client.ConnectToHost(IPAddress.Loopback, 9054);

                await CheckConnectionCount(host, 1, 1000, 10);

                Assert.Single(host.ClientConnections);
                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task HeartbeatTimeout()
        {
            for (int i = 0; i < 5; i++)
            {
                var host = new EasyHost(250, 9055, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);

                //Simulate EasyHost connecting but not responding to heartbeats
                using (var client = new TcpClient())
                    client.Connect(IPAddress.Loopback, 9055);

                //Buffer
                await Task.Delay(10);

                //Cause intentional timeout
                await Task.Delay(350);

                Assert.Empty(host.ClientConnections);
                host.Close();
            }
        }
        [Fact]
        public async Task TakenPortException()
        {
            var host1 = new EasyHost(250, 9056, IPAddress.Loopback);
            var host2 = new EasyHost(250, 9056, IPAddress.Loopback);
            host1.Open();
            await Task.Delay(100);
            Assert.Throws<SocketException>(() => host2.Open());
        }
        [Fact]
        public async Task TwoClientsConnectedWhenOneReconnects()
        {
            for (int i = 0; i < 1; i++)
            {
                var host = new EasyHost(500, 9057, IPAddress.Loopback);
                host.Open();

                //Buffer
                await Task.Delay(10);
                var client1 = new EasyClient();
                var client2 = new EasyClient();
                client1.ConnectToHost(IPAddress.Loopback, 9057);
                client2.ConnectToHost(IPAddress.Loopback, 9057);

                //Buffer
                await Task.Delay(10);
                client1.DisconnectFromHost();

                //Buffer
                await Task.Delay(100);
                client1.ConnectToHost(IPAddress.Loopback, 9057);

                await CheckConnectionCount(host, 2, 250, 10);

                Assert.True(host.ClientConnections.Count == 2);
                client1.DisconnectFromHost();
                client2.DisconnectFromHost();
                host.Close();
            }
        }

        private static async Task CheckConnectionCount(EasyHost host, int compareCount, int repetitions = 100, int delay = 100)
        {
            for (int j = 0; j < repetitions; j++)
            {
                await Task.Delay(delay);
                if (host.ClientConnections.Count == compareCount)
                    break;
            }
        }
    }
}
