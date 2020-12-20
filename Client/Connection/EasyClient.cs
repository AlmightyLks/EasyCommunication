using EasyCommunication.Events.Client.EventArgs;
using EasyCommunication.Events.Client.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.SharedTypes;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace EasyCommunication.Client.Connection
{
    public class EasyClient
    {
        public bool ClientConnected => Client is null ? false : Client.Connected;
        public ClientEventHandler EventHandler { get; private set; }
        public Connection? Connection { get; set; }
        internal TcpClient Client { get; set; }
        internal Task RequestListening { get; private set; }

        private BinaryFormatter binaryFormatter;

        public EasyClient()
        {
            Connection = null;
            Client = new TcpClient();
            binaryFormatter = new BinaryFormatter();
            EventHandler = new ClientEventHandler();
        }

        public void ConnectToHost(IPAddress address, int port)
        {
            //If the client, for whatever reason, disconnected - Reconnect.
            if (ClientConnected)
                return;

            //Stop heartingbeating for old client
            RequestListening?.Dispose();

            //Reset client
            Client = new TcpClient();

            Connection = new Connection() { IPAddress = address, Port = port };

            //Console.WriteLine($"Attemping connecting to {Connection.GetIPAndPort()}");

            try
            {
                Client.Connect(address, port);
                Console.WriteLine($"Connection attempt successfull");
            }
            catch
            {
                Connection = null;
                Console.WriteLine($"Connection attempt failed");
            }

            StartListening();
        }
        public void DisconnectFromHost()
        {
            if (!ClientConnected)
                return;

            Client.GetStream().Close();
            Client.Close();
        }
        public SendStatus SendData<T>(T data)
        {
            var sendingArgs = new SendingDataEventArgs()
            {
                Allow = true,
                Data = data,
                IsHeartbeat = (data is HeartbeatPing),
                Receiver = Connection
            };

            EventHandler.InvokeSendingData(sendingArgs);

            if (!sendingArgs.Allow)
                return SendStatus.Disallowed;

            if (Connection is null)
                return SendStatus.NotConnected;

            try
            {
                binaryFormatter.Serialize(Client.GetStream(), data);
            }
            catch
            {
                return SendStatus.Unsuccessfull;
            }

            return SendStatus.Successfull;
        }
        internal Task ListenForRequests()
        {
            for (; ; )
            {
                try
                {
                    if (!Client.Connected)
                        continue;


                    Console.WriteLine($"Waiting for Requests...");

                    //Wait for and deserialize the incoming data
                    var receivedData = binaryFormatter.Deserialize(Client.GetStream());

                    //Received Data Event
                    var receivedArgs = new ReceivedDataEventArgs()
                    {
                        Sender = Connection,
                        Data = receivedData,
                        IsHeartbeat = false
                    };

                    if (receivedData is HeartbeatPing)
                    {
                        receivedArgs.IsHeartbeat = true;
                        SendData(new HeartbeatPing());
                    }
                    else
                    {
                        receivedArgs.IsHeartbeat = false;
                    }

                    EventHandler.InvokeReceivedData(receivedArgs);
                }
                catch (IOException e)
                {
                    Console.WriteLine($"Socket connection for {Connection.GetIPAndPort()} was closed.");

                    DisconnectFromHost();
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Socket connection for {Connection.GetIPAndPort()} was closed.");

                    DisconnectFromHost();
                }
                catch (SerializationException e)
                {
                    Console.WriteLine($"\"{e.Message}\" {Connection.GetIPAndPort()}.");

                    DisconnectFromHost();
                }
                catch
                {
                    Console.WriteLine($"eee - Exception in listen:\n{Connection.GetIPAndPort()}");
                }
            }
        }
        private void StartListening()
            => Task.Run(() => RequestListening = ListenForRequests());
    }
}
