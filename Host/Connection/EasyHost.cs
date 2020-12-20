using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.Events.Host.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.SharedTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace EasyCommunication.Host.Connection
{
    public class EasyHost
    {
        public TcpListener TcpListener { get; private set; }
        public Dictionary<TcpClient, int> ClientConnections { get; private set; }
        public HostEventHandler EventHandler { get; private set; }
        internal Heartbeat Heartbeat { get; private set; }
        public int ListeningPort { get; private set; }

        private BinaryFormatter binaryFormatter;

        public EasyHost(int heartbeatInterval, int listeningPort, IPAddress listeningAddress)
        {
            ListeningPort = listeningPort;

            Heartbeat = new Heartbeat(heartbeatInterval);
            Heartbeat.EasyHost = this;

            EventHandler = new HostEventHandler();
            ClientConnections = new Dictionary<TcpClient, int>();
            TcpListener = new TcpListener(listeningAddress, listeningPort);
            binaryFormatter = new BinaryFormatter();
        }

        public void Open()
        {
            Heartbeat.Start();
            Task.Run(() => ListenForClient());
        }
        public void Close()
        {
            Heartbeat.Stop();

            foreach (var smth in ClientConnections)
                smth.Key.Close();

            TcpListener.Stop();
        }

        public SendStatus SendData<T>(T data, TcpClient receiver)
        {
            try
            {
                var sendingArgs = new SendingDataEventArgs()
                {
                    Receiver = receiver,
                    Data = data,
                    Allow = true,
                    IsHeartbeat = data is HeartbeatPing
                };

                EventHandler.InvokeSendingData(sendingArgs);

                if (sendingArgs.Allow)
                    binaryFormatter.Serialize(receiver.GetStream(), data);
                else
                    return SendStatus.Disallowed;
            }
            catch (Exception e)
            {
                return SendStatus.Unsuccessfull;
            }
            return SendStatus.Successfull;
        }

        private void ListenForClient()
        {
            TcpListener.Start();
            Console.WriteLine($"Host is listening for connections on {TcpListener.GetIPv4()}:{TcpListener.GetPort()}");

            for (; ; )
            {
                try
                {
                    if (!TcpListener.Server.IsBound)
                        return;


                    //Listen for clients
                    TcpClient acceptedClient = TcpListener.AcceptTcpClient();

                    var port = acceptedClient.GetPort();

                    var connectArgs = new ClientConnectedEventArgs()
                    {
                        Client = acceptedClient,
                        Port = port,
                        Allow = true
                    };

                    EventHandler.InvokeClientConnected(connectArgs);

                    if (connectArgs.Allow)
                    {
                        ClientConnections.Add(acceptedClient, port);
                        Heartbeat.Heartbeats.Add(acceptedClient, 1);

                        Console.WriteLine($"Accepted connection for {acceptedClient.GetIPv4()}:{acceptedClient.GetPort()}");
                        //Handle each client individually.
                        Task.Run(() => { HandleRequests(acceptedClient); });
                    }
                    else
                    {
                        Console.WriteLine($"Declined connection for {acceptedClient.GetIPv4()}:{acceptedClient.GetPort()}");
                        acceptedClient.Close();
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine($"Stopped listening");
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception in ListenForClient:\n{e}");
                }
            }
        }
        private void HandleRequests(TcpClient acceptedClient)
        {
            //Find connection
            var clientInfo = ClientConnections.First((_) => _.Key == acceptedClient);

            for (; ; )
            {
                try
                {
                    if (!acceptedClient.Connected)
                        return;

                    //Accept SharedInfo data from the client.
                    object receivedData = binaryFormatter.Deserialize(acceptedClient.GetStream());
                    Task.Run(() => HandleData(receivedData, clientInfo));
                }
                catch (IOException)
                {
                    Console.WriteLine($"Socket connection for {clientInfo.Value} was closed.");

                    //Remove client from storage
                    ClientConnections.Remove(clientInfo.Key);
                    Heartbeat.Heartbeats.Remove(clientInfo.Key);

                    //End connections.
                    //acceptedClient.GetStream().Close();
                    acceptedClient.Close();
                    break;
                }
                catch (SerializationException e)
                {
                    Console.WriteLine($"\"{e.Message}\" {clientInfo.Value}.");

                    //Remove from storage
                    ClientConnections.Remove(clientInfo.Key);
                    Heartbeat.Heartbeats.Remove(clientInfo.Key);
                    Heartbeat.Heartbeats.Remove(clientInfo.Key);

                    //End connections.
                    acceptedClient.GetStream().Close();
                    acceptedClient.Close();
                    break;
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine($"\"{e.Message}\" {clientInfo.Value}.");
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\"{e.Message}\" {clientInfo.Value}.");
                    break;
                }
            }
        }
        private void HandleData(object receivedData, KeyValuePair<TcpClient, int> clientInfo)
        {
            try
            {
                //Received Data Event
                var receivedArgs = new ReceivedDataEventArgs()
                {
                    Sender = clientInfo.Key,
                    Port = clientInfo.Value,
                    Data = receivedData,
                    IsHeartbeat = false
                };

                if (receivedData is HeartbeatPing)
                {
                    receivedArgs.IsHeartbeat = true;

                    if (Heartbeat.Heartbeats.ContainsKey(clientInfo.Key))
                        Heartbeat.Heartbeats[clientInfo.Key]++;
                    else
                        Heartbeat.Heartbeats.Add(clientInfo.Key, 1);

                    EventHandler.InvokeReceivedData(receivedArgs);
                }
                else
                {
                    EventHandler.InvokeReceivedData(receivedArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in Handle data:\n{e}");
            }
        }
    }
}
