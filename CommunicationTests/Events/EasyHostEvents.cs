using EasyCommunication.SharedTypes;
using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using EasyCommunication;

namespace CommunicationTests.Events
{
    //Ports 9250 - 9399
    [Collection("Sequential")]
    public class EasyHostEvents
    {
        [Fact]
        public async Task TriggerClientConnected()
        {
            for (int i = 0; i < 1; i++)
            {
                int triggered = 0;
                var host = new EasyHost(2000, 9250, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);

                host.EventHandler.ClientConnected += delegate (ClientConnectedEventArgs ev)
                {
                    triggered++;
                };
                client.ConnectToHost(IPAddress.Loopback, 9250);

                //Buffer for connection & event
                await Task.Delay(5);
                Assert.True(triggered == 1);
                client.DisconnectFromHost();
                host.Close();
            }
        }
        [Fact]
        public async Task TriggerClientDisconnected()
        {
            for (int i = 0; i < 1; i++)
            {
                int triggered = 0;
                var host = new EasyHost(2000, 9251, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);

                host.EventHandler.ClientDisconnected += delegate (ClientDisconnectedEventArgs ev)
                {
                    triggered++;
                };
                client.ConnectToHost(IPAddress.Loopback, 9251);
                client.DisconnectFromHost();

                //Buffer for connection & event
                await Task.Delay(5);
                Assert.True(triggered == 1);
                host.Close();
            }
        }
        [Fact]
        public async Task TriggerReceivedData()
        {
            for (int i = 0; i < 1; i++)
            {
                int triggered = 0;
                var host = new EasyHost(2000, 9252, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    triggered++;
                };
                client.ConnectToHost(IPAddress.Loopback, 9252);
                await Task.Delay(5);
                client.QueueData("Good evening", DataType.String);

                //Buffer for connection & event
                await Task.Delay(25);
                Assert.True(triggered == 1);
                host.Close();
                client.DisconnectFromHost();
            }
        }
        [Fact]
        public async Task TriggerSendingData()
        {
            for (int i = 0; i < 1; i++)
            {
                int triggered = 0;
                var host = new EasyHost(2000, 9253, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);

                host.EventHandler.SendingData += delegate (SendingDataEventArgs ev)
                {
                    triggered++;
                };
                client.ConnectToHost(IPAddress.Loopback, 9253);
                await Task.Delay(5);
                host.QueueData("Good evening", host.ClientConnections.Keys.ToArray()[0], DataType.String);

                //Buffer for connection & event
                await Task.Delay(25);
                Assert.True(triggered == 1);
                host.Close();
                client.DisconnectFromHost();
            }
        }
        [Fact]
        public async Task ReceivedDataIdentical()
        {
            for (int i = 0; i < 1; i++)
            {
                bool equals = false;
                var host = new EasyHost(2000, 9254, IPAddress.Loopback);
                host.Open();
                var client = new EasyClient(500);
                string data = "Good evening";

                host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
                {
                    equals = (data == Encoding.UTF8.GetString(ev.ReceivedBuffer.Data));
                };
                client.ConnectToHost(IPAddress.Loopback, 9254);
                await Task.Delay(5);
                var status = client.QueueData(data, DataType.String);

                //Buffer for connection & event
                await Task.Delay(25);
                Assert.True(equals);
                host.Close();
                client.DisconnectFromHost();
            }
        }
    }
}
