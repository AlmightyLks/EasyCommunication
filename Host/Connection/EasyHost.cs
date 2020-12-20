using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.Events.Host.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.Logging;
using EasyCommunication.SharedTypes;
using Newtonsoft.Json;
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
    /// <summary>
    /// Listens for incoming <see cref="Client.Connection.EasyClient"/> connections
    /// </summary>
    /// <remarks>
    /// <para>Send and receive data from connected clients</para>
    /// </remarks>
    public class EasyHost
    {
        /// <summary>
        /// The TcpListener used to listen for incoming connections.
        /// </summary>
        public TcpListener TcpListener { get; private set; }

        /// <summary>
        /// All Connections with received Heartbeats since last query,
        /// used by <see cref="Connection.Heartbeat"/>.
        /// </summary>
        public Dictionary<TcpClient, int> ClientConnections { get; private set; }

        /// <summary>
        /// EventHandler for EasyHost-events
        /// </summary>
        public HostEventHandler EventHandler { get; private set; }

        /// <summary>
        /// <see cref="Connection.Heartbeat"/> instance, responsible for heartbeating connected clients
        /// </summary>
        internal Heartbeat Heartbeat { get; private set; }

        /// <summary>
        /// The port on which the Host is listening for incoming connections
        /// </summary>
        public int ListeningPort { get; private set; }

        /// <summary>
        /// BinaryFormatter for serializing data into the Client's NetworkStream
        /// </summary>
        private BinaryFormatter binaryFormatter;

        /// <summary>
        /// <see cref="ILogger"/> instance, responsible for logging
        /// </summary>
        private ILogger logger;


        /// <summary>
        /// Creates an instance of <see cref="EasyHost"/> with a Heartbeat Interval, a Listening Port and a Listening Address.
        /// </summary>
        /// <param name="heartbeatInterval">Heartbeat Interval</param>
        /// <param name="listeningPort">Listening Port for <see cref="TcpListener"/></param>
        /// <param name="listeningAddress">Listening Address for <see cref="TcpListener"/></param>
        public EasyHost(int heartbeatInterval, int listeningPort, IPAddress listeningAddress)
        {
            logger = new Logger();

            ListeningPort = listeningPort;

            Heartbeat = new Heartbeat(heartbeatInterval, logger);
            Heartbeat.EasyHost = this;

            EventHandler = new HostEventHandler();
            ClientConnections = new Dictionary<TcpClient, int>();
            TcpListener = new TcpListener(listeningAddress, listeningPort);
            binaryFormatter = new BinaryFormatter();
        }


        /// <summary>
        /// Open the <see cref="TcpListener"/> to listen for connections
        /// </summary>
        /// <remarks>
        /// <para>Starts listening for incoming connection</para>
        /// <para>Starts querying Heartbeats</para>
        /// </remarks>
        public void Open()
        {
            Heartbeat.Start();
            Task.Run(() => ListenForClient());
        }

        /// <summary>
        /// Closes the <see cref="TcpListener"/> from listening for connections
        /// </summary>
        /// <remarks>
        /// <para>Stops listening for incoming connection</para>
        /// <para>Stops querying Heartbeats</para>
        /// <para>Closes all connections</para>
        /// </remarks>
        public void Close()
        {
            Heartbeat.Stop();

            foreach (var smth in ClientConnections)
                smth.Key.Close();

            TcpListener.Stop();
        }

        /// <summary>
        /// Sends data to the specified receiver
        /// </summary>
        /// <typeparam name="T">Custom Type which has to be either JsonConvert'able or Serializable</typeparam>
        /// <param name="data">Data to send</param>
        /// <param name="receiver">Receiver of the data</param>
        /// <returns></returns>
        public SendStatus SendData<T>(T data, TcpClient receiver)
        {
            object actualData;

            var isSerializable = typeof(T).IsSerializable;

            try
            {
                if (!isSerializable)
                    actualData = JsonConvert.SerializeObject(data);
                else
                    actualData = data;
            }
            catch (Exception e)
            {
                logger.Error($"\"{typeof(T).Name}\" -> Neither Serializable nor JsonConvert'able type.");
                return SendStatus.Unsuccessfull;
            }

            var sendingArgs = new SendingDataEventArgs()
            {
                Allow = true,
                Data = actualData,
                IsHeartbeat = (data is HeartbeatPing),
                IsSerializable = isSerializable,
                Receiver = receiver
            };

            EventHandler.InvokeSendingData(sendingArgs);

            try
            {
                if (sendingArgs.Allow)
                    binaryFormatter.Serialize(receiver.GetStream(), actualData);
                else
                    return SendStatus.Disallowed;
            }
            catch (Exception e)
            {
                return SendStatus.Unsuccessfull;
            }

            return SendStatus.Successfull;
        }

        /// <summary>
        /// Start listening for connections
        /// </summary>
        private void ListenForClient()
        {
            TcpListener.Start();
            logger.Info($"Host is listening for connections on {TcpListener.GetIPv4()}:{TcpListener.GetPort()}");

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

                        logger.Info($"Accepted connection for {acceptedClient.GetIPv4()}:{acceptedClient.GetPort()}");
                        //Handle each client individually.
                        Task.Run(() => { HandleRequests(acceptedClient); });
                    }
                    else
                    {
                        logger.Warn($"Declined connection for {acceptedClient.GetIPv4()}:{acceptedClient.GetPort()}");
                        acceptedClient.Close();
                    }
                }
                catch (SocketException)
                {
                    logger.Error($"Stopped listening");
                    break;
                }
                catch (Exception e)
                {
                    logger.Error($"Exception in ListenForClient:\n{e}");
                }
            }
        }

        /// <summary>
        /// Once the connection is accepted, each connection is listened for incoming requests
        /// </summary>
        /// <param name="acceptedClient"></param>
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
                    var receivedData = binaryFormatter.Deserialize(acceptedClient.GetStream());
                    Task.Run(() => HandleData(receivedData, clientInfo));
                }
                catch (IOException e)
                {
                    logger.Error($"Socket connection for {clientInfo.Value} was closed.");

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
                    logger.Error($"\"{e.Message}\" {clientInfo.Value}.");

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
                    logger.Error($"\"{e.Message}\" {clientInfo.Value}.");
                    break;
                }
                catch (Exception e)
                {
                    logger.Error($"\"{e.Message}\" {clientInfo.Value}.");
                    break;
                }
            }
        }

        /// <summary>
        /// One data is received, it is being handled here
        /// </summary>
        /// <param name="receivedData">The received data</param>
        /// <param name="clientInfo">Information about the sender</param>
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
                logger.Error($"Exception in Handle data:\n{e}");
            }
        }
    }
}
