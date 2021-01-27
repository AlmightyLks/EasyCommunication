using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.Events.Host.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.Logging;
using EasyCommunication.SharedTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace EasyCommunication.Host.Connection
{
    /// <summary>
    /// Listens for incoming <see cref="Client.Connection.EasyClient"/> connections
    /// </summary>
    /// <remarks>
    /// <para>Send and receive data from connected clients</para>
    /// </remarks>
    public sealed class EasyHost : IEasyHost
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Listening { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// The TcpListener used to listen for incoming connections.
        /// </summary>
        public TcpListener TcpListener { get; private set; }

        /// <summary>
        /// All Connections with their initial port
        /// </summary>
        public IDictionary<TcpClient, int> ClientConnections { get; private set; }

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
        /// <see cref="ILogger"/> instance, responsible for logging
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// 
        /// </summary>
        private Queue<QueueEntity> dataQueue;

        /// <summary>
        /// 
        /// </summary>
        private bool isClosed;

        /// <summary>
        /// 
        /// </summary>
        private int heartbeatInterval;

        /// <summary>
        /// Creates an instance of <see cref="EasyHost"/> with a Heartbeat Interval, a Listening Port and a Listening Address.
        /// </summary>
        /// <param name="heartbeatInterval">Heartbeat Interval</param>
        /// <param name="listeningPort">Listening Port for <see cref="TcpListener"/></param>
        /// <param name="listeningAddress">Listening Address for <see cref="TcpListener"/></param>
        /// <param name="logger"><see cref="ILogger"/> DI instance</param>
        public EasyHost(int heartbeatInterval, ushort listeningPort, IPAddress listeningAddress)
        {
            logger = new Logger();

            BufferSize = 1024;
            dataQueue = new Queue<QueueEntity>();
            ListeningPort = listeningPort;
            this.heartbeatInterval = heartbeatInterval;

            EventHandler = new HostEventHandler();
            ClientConnections = new ConcurrentDictionary<TcpClient, int>();
            TcpListener = new TcpListener(listeningAddress, listeningPort);
            isClosed = true;
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
            TcpListener.Start();

            Heartbeat = new Heartbeat(heartbeatInterval);
            Heartbeat.EasyHost = this;

            isClosed = false;
            Listening = true;

            new Task(async () => await SendQueuedData()).Start();
            new Task(() => ListenForClient()).Start();
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
            foreach (var smth in ClientConnections)
                smth.Key.Close();

            TcpListener.Stop();
            Heartbeat.Dispose();
            Listening = false;
            isClosed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="receiver"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public QueueStatus QueueData<T>(T data, TcpClient receiver, DataType dataType)
        {
            if (isClosed)
                return QueueStatus.NotOpen;

            byte[] buffer;

            try
            {
                buffer = SerializationHelper.GetBuffer(data, dataType);
                if (buffer.Length > BufferSize)
                    return QueueStatus.BufferTransgression;
                dataQueue.Enqueue(new QueueEntity() { Receiver = receiver, Data = buffer });
            }
            catch (SerializationException e)
            {
                return QueueStatus.IllegalFormat;
            }
            return QueueStatus.Queued;
        }

        /// <summary>
        /// Sends data to the specified receiver
        /// </summary>
        /// <typeparam name="T">Custom Type which has to be either JsonConvert'able or Serializable</typeparam>
        /// <param name="data">Data to send</param>
        /// <param name="receiver">Receiver of the data</param>
        /// <returns></returns>
        internal void SendData(byte[] data, TcpClient receiver)
        {
            if (isClosed)
                return;
            try
            {
                if (!ClientConnections.Any(_ => _.Key == receiver))
                    return;
                if (!receiver.Connected)
                    return;

                if (data[0] != (byte)DataType.Heartbeat && data[0] != (byte)DataType.Disconnect)
                {
                    var sendingArgs = new SendingDataEventArgs()
                    {
                        Allow = true,
                        Receiver = receiver,
                        Type = (DataType)data[0]
                    };

                    byte[] trimmedBuffer = new byte[data.Length - 1];
                    Array.Copy(data, 1, trimmedBuffer, 0, data.Length - 1);
                    sendingArgs.Data = trimmedBuffer;

                    EventHandler.InvokeSendingData(sendingArgs);

                    if (!sendingArgs.Allow)
                        return;
                }

                receiver.GetStream().Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine($"EasyHost in SendData:\n{e}");
            }
        }

        /// <summary>
        /// Start listening for connections
        /// </summary>
        private void ListenForClient()
        {
            Debug.WriteLine($"EasyHost: Host is listening for connections on {TcpListener.GetIPv4()}:{TcpListener.GetPort()}");

            while (!isClosed)
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

                        Debug.WriteLine($"EasyHost: Accepted connection for {acceptedClient.GetIPv4()}:{acceptedClient.GetPort()}");
                        //Handle each client individually.
                        new Task(() => HandleRequests(acceptedClient)).Start();
                    }
                    else
                    {
                        Debug.WriteLine($"EasyHost: Declined connection for {acceptedClient.GetIPv4()}:{acceptedClient.GetPort()}");
                        if (acceptedClient != null && !acceptedClient.Connected)
                            acceptedClient.Close();
                    }
                }
                catch (SocketException)
                {
                    Debug.WriteLine($"EasyHost: Stopped listening");
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"EasyHost: Exception in ListenForClient:\n{e}");
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

            while (!isClosed)
            {
                try
                {
                    if (!acceptedClient.Connected)
                        return;

                    byte[] buffer = new byte[BufferSize];
                    int bytesRead = acceptedClient.GetStream().Read(buffer, 0, buffer.Length);
                    byte[] trimmedBuffer = new byte[bytesRead];
                    Array.Copy(buffer, trimmedBuffer, bytesRead);
                    new Task(() => HandleData(trimmedBuffer, clientInfo)).Start();
                }
                catch (SocketException e)
                {
                    Debug.WriteLine($"EasyHost: Socket connection for {clientInfo.Value} was closed.\n{e}");

                    //Remove client from storage
                    ClientConnections.Remove(clientInfo.Key);
                    Heartbeat.Heartbeats.Remove(clientInfo.Key);

                    //End connections.
                    if (acceptedClient != null && !acceptedClient.Connected)
                    {
                        acceptedClient.Close();
                        EventHandler.InvokeClientDisconnected(new ClientDisconnectedEventArgs() { Client = acceptedClient, Port = clientInfo.Value });
                    }
                    break;
                }
                catch (IOException e)
                {
                    Debug.WriteLine($"EasyHost: Socket connection for {clientInfo.Value} was closed.\n{e}");

                    //Remove client from storage
                    ClientConnections.Remove(clientInfo.Key);
                    Heartbeat.Heartbeats.Remove(clientInfo.Key);

                    if (acceptedClient != null && !acceptedClient.Connected)
                    {
                        //End connections.
                        acceptedClient.Close();
                        EventHandler.InvokeClientDisconnected(new ClientDisconnectedEventArgs() { Client = acceptedClient, Port = clientInfo.Value });
                    }
                    break;
                }
                catch (InvalidOperationException e)
                {
                    Debug.WriteLine($"EasyHost: \"{e}\" {clientInfo.Value}.");
                    break;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"EasyHost: \"{e}\" {clientInfo.Value}.");
                    break;
                }
            }
        }

        /// <summary>
        /// One data is received, it is being handled here
        /// </summary>
        /// <param name="data">The received data</param>
        /// <param name="clientInfo">Information about the sender</param>
        private void HandleData(byte[] data, KeyValuePair<TcpClient, int> clientInfo)
        {
            if (data.Length == 1 && data[0] == (byte)DataType.Heartbeat)
            {
                if (Heartbeat.Heartbeats.ContainsKey(clientInfo.Key))
                    Heartbeat.Heartbeats[clientInfo.Key]++;
                else
                    Heartbeat.Heartbeats.Add(clientInfo.Key, 1);
            }
            else if (data.Length == 1 && data[0] == (byte)DataType.Disconnect)
            {
                Heartbeat.Heartbeats.Remove(clientInfo.Key);
                ClientConnections.Remove(clientInfo);
                clientInfo.Key.Close();
                EventHandler.InvokeClientDisconnected(new ClientDisconnectedEventArgs() { Client = clientInfo.Key, Port = clientInfo.Value });
            }
            else
            {
                //Split datatype identifier & data
                DataType type = (DataType)data[0];
                byte[] trimmedBuffer = new byte[data.Length - 1];
                Array.Copy(data, 1, trimmedBuffer, 0, trimmedBuffer.Length);

                //Received Data Event
                var receivedArgs = new ReceivedDataEventArgs()
                {
                    Sender = clientInfo.Key,
                    Port = clientInfo.Value,
                    Type = type,
                    Data = trimmedBuffer
                };
                EventHandler.InvokeReceivedData(receivedArgs);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task SendQueuedData()
        {
            while (!isClosed)
            {
                await Task.Delay(75);
                if (dataQueue.Count == 0)
                    continue;
                QueueEntity entitiy = dataQueue.Dequeue();
                SendData(entitiy.Data, entitiy.Receiver);
            }
        }
    }
}
