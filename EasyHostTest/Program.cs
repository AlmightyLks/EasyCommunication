using EasyCommunication.Helper;
using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.Host.Connection;
using System;
using System.Net;
using System.Threading.Tasks;

namespace EasyHostTest
{
    public class Program
    {
        public static EasyHost Host { get; set; }
        static async Task Main()
        {
            Console.Title = "EasyHost";

            //Instantiate your EasyHost
            Host = new EasyHost(6000,                   //Heartbeat interval of 6 seconds
                                8000,                   //Listen on Port 8000
                                IPAddress.Loopback);    //On loopback / localhost / 127.0.0.1

            //Subscribe your desired events
            Host.EventHandler.ReceivedData += EventHandler_ReceivedData;
            Host.EventHandler.SendingData += EventHandler_SendingData;
            Host.EventHandler.ClientConnected += EventHandler_ClientConnected;

            //Open your Host's connection
            Host.Open();

            await Task.Delay(-1);
        }

        private static void EventHandler_ClientConnected(ClientConnectedEventArgs ev)
        {
            Console.WriteLine($"I am now connected with {ev.Client.GetIPv4()}:{ev.Port}");
        }

        private static void EventHandler_SendingData(SendingDataEventArgs ev)
        {
            if (!ev.IsHeartbeat)
                Console.WriteLine($"I am transmiting \"{ev.Data}\"!");
        }

        private static void EventHandler_ReceivedData(ReceivedDataEventArgs ev)
        {
            if (!ev.IsHeartbeat)
                Console.WriteLine($"I received \"{ev.Data}\"!");
        }
    }
}
