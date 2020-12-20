using EasyCommunication.Events.Client.EventArgs;
using EasyCommunication.Client.Connection;
using EasyCommunication.Helper;
using System;
using System.Net;
using System.Threading.Tasks;
using EasyCommunication.SharedTypes;

namespace EasyClientTest
{
    public class Program
    {
        public static EasyClient Client { get; set; }
        static async Task Main()
        {
            Console.Title = "EasyClient";

            //Delay for EasyHost to go ahead for simultaneous project start
            await Task.Delay(1000);

            //Instantiate your EasyClient
            Client = new EasyClient();

            //Subscribe your desired events
            Client.EventHandler.ReceivedData += EventHandler_ReceivedData;
            Client.EventHandler.ConnectedToHost += EventHandler_ConnectedToHost;
            Client.EventHandler.SendingData += EventHandler_SendingData;

            //Connect to the specified EasyHost
            Client.ConnectToHost(IPAddress.Loopback, 8000);

            //I will just leave if it couldn't establish a connection
            if (!Client.ClientConnected)
            {
                Console.WriteLine("Client could not be connected, aborting.");
                await Task.Delay(3500);
                return;
            }

            //I will just create some Serializable type
            SerializableImportantData myImportantSerializableData = new SerializableImportantData()
            {
                ImportantMessage = "You need to know this!"
            };

            //Send my data to the Host
            //The type is serializable, thus can be serialized into the NetworkStream as an object
            Client.SendData(myImportantSerializableData);

            //Wait for 1 seconds as buffer
            await Task.Delay(1000);

            //I will just create some Serializable type
            ImportantData myImportantNonSerializableData = new ImportantData()
            {
                ImportantMessage = "You need to know this!"
            };

            //Send my data to the Host
            //The type is not serializable, thus has to be converted into a Json-String and serialized into the NetworkStream as string
            Client.SendData(myImportantNonSerializableData);

            await Task.Delay(-1);
        }

        private static void EventHandler_SendingData(SendingDataEventArgs ev)
        {
            //Print out everything that is not a heartbeat
            if (!ev.IsHeartbeat)
                Console.WriteLine($"I am transmiting \"{ev.Data}\"!");
        }

        private static void EventHandler_ConnectedToHost(ConnectedToHostEventArgs ev)
        {
            //Note that connection has been established successfully
            Console.WriteLine($"I will abort connection with {ev.Connection.GetIPAndPort()}");
            ev.Abort = true;
        }

        private static void EventHandler_ReceivedData(ReceivedDataEventArgs ev)
        {
            //Print out everything that is not a heartbeat
            if (!ev.IsHeartbeat)
                Console.WriteLine($"I received \"{ev.Data}\"!");
        }
    }
}
