using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.Events.Host.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.Serialization;
using EasyCommunication.SharedTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyCommunication
{
    /// <summary>
    /// Listens for incoming <see cref="Client.Connection.EasyClient"/> connections
    /// </summary>
    /// <remarks>
    /// <para>Send and receive data from connected clients</para>
    /// </remarks>
    public sealed class EasyHost
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Listening { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public uint BufferSize { get; set; }

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
        /// Clients and their amount of heartbeats since last query
        /// </summary>
        internal Dictionary<TcpClient, int> Heartbeats { get; private set; }

        /// <summary>
        /// The port on which the Host is listening for incoming connections
        /// </summary>
        public int ListeningPort { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private ConcurrentQueue<QueueEntity> dataQueue;

        /// <summary>
        /// 
        /// </summary>
        private bool isClosed;

        /// <summary>
        /// 
        /// </summary>
        public int HeartbeatInterval { get; private set; }

        /// <summary>
        /// Creates an instance of <see cref="EasyHost"/> with a Heartbeat Interval, a Listening Port and a Listening Address.
        /// </summary>
        /// <param name="heartbeatInterval">Heartbeat Interval</param>
        /// <param name="listeningPort">Listening Port for <see cref="TcpListener"/></param>
        /// <param name="listeningAddress">Listening Address for <see cref="TcpListener"/></param>
        /// <param name="logger"><see cref="ILogger"/> DI instance</param>
        public EasyHost(int heartbeatInterval, ushort listeningPort, IPAddress listeningAddress)
        {
            BufferSize = 1024;
            dataQueue = new ConcurrentQueue<QueueEntity>();
            ListeningPort = listeningPort;
            this.HeartbeatInterval = heartbeatInterval;

            EventHandler = new HostEventHandler();
            ClientConnections = new ConcurrentDictionary<TcpClient, int>();
            TcpListener = new TcpListener(listeningAddress, listeningPort);
            Heartbeats = new Dictionary<TcpClient, int>();
            isClosed = true;
            Listening = false;
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

            isClosed = false;
            Listening = true;

            new Task(() => ListenForClient()).Start();
            new Task(async () => await SendQueuedData()).Start();
            new Task(async () => await CheckHeartbeats()).Start();
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
            foreach (var conn in ClientConnections)
            {
                SendData(new byte[] { (byte)DataType.Disconnect, 0, 0, 0, 0 }, conn.Key);
                conn.Key.Close();
            }

            TcpListener.Stop();
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
                Debug.WriteLine($"{DateTime.Now}:{DateTime.Now.Millisecond}");
                buffer = SerializationHelper.GetBuffer(data, dataType);
                Debug.WriteLine($"{DateTime.Now}:{DateTime.Now.Millisecond}");
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
                if (!ClientConnections.Keys.Contains(receiver))
                    return;
                if (!receiver.Connected)
                    return;

                //If not heartbeat or disconnect
                if (data.Length > 5)
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
                        Heartbeats.Add(acceptedClient, 1);

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
            byte[] receivingBuffer;
            while (!isClosed)
            {
                try
                {
                    if (!acceptedClient.Connected)
                        return;

                    receivingBuffer = new byte[BufferSize];
                    int bytesRead = 0;
                    bytesRead = acceptedClient.GetStream().Read(receivingBuffer, 0, receivingBuffer.Length);

                    byte[] dataBuffer = new byte[bytesRead];
                    Array.Resize(ref receivingBuffer, bytesRead);
                    receivingBuffer.AsSpan().CopyTo(dataBuffer);

                    //if (dataBuffer.Length > 5)
                    //    Debug.WriteLine($"EasyHost: Received {(DataType)dataBuffer[0]} | {string.Join(" ", dataBuffer)}");

                    //Do not accept empty buffers
                    if (bytesRead == 0)
                        continue;

                    new Task(() => HandleData(dataBuffer, clientInfo)).Start();
                }
                catch (SocketException e)
                {
                    Debug.WriteLine($"EasyHost: Socket connection for {clientInfo.Value} was closed.\n{e}");

                    //Remove client from storage
                    ClientConnections.Remove(clientInfo.Key);
                    Heartbeats.Remove(clientInfo.Key);

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
                    Heartbeats.Remove(clientInfo.Key);

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
            IEnumerable<ReceivedBuffer> buffers = data.GetStackedBuffers();

            if (data.Length > 5)
                Debug.WriteLine($"EasyHost: Received before: {(DataType)data[0]} | {string.Join(" ", data)}");

            foreach (var buffer in buffers)
                if (buffer.Data.Length > 0)
                    Debug.WriteLine($"EasyHost: Received after: {buffer.DataType} | {string.Join(" ", buffer.Data)}");

            foreach (ReceivedBuffer buffer in buffers)
            {
                //if (buffer.Data.Length != 0)
                //    Debug.WriteLine($"EasyHost: Received {buffer.DataType} | {string.Join(" ", buffer.Data)}");

                if (buffer.Data.Length == 0)
                {
                    switch (buffer.DataType)
                    {
                        case DataType.HostHeartbeat:
                            {
                                Debug.WriteLine("EasyHost: HostHeartbeat received");
                                if (Heartbeats.ContainsKey(clientInfo.Key))
                                    Heartbeats[clientInfo.Key]++;
                                else
                                    Heartbeats.Add(clientInfo.Key, 1);
                            }
                            break;
                        case DataType.ClientHeartbeat:
                            {
                                Debug.WriteLine("EasyHost: ClientHeartbeat received");
                                SendData(new byte[] { (byte)DataType.ClientHeartbeat, 0, 0, 0, 0 }, clientInfo.Key);
                            }
                            break;
                        case DataType.Disconnect:
                            {
                                Heartbeats.Remove(clientInfo.Key);
                                ClientConnections.Remove(clientInfo);
                                clientInfo.Key.Close();
                                EventHandler.InvokeClientDisconnected(new ClientDisconnectedEventArgs() { Client = clientInfo.Key, Port = clientInfo.Value });
                            }
                            break;
                    }
                }
                else
                {
                    //Received Data Event
                    var receivedArgs = new ReceivedDataEventArgs()
                    {
                        Sender = clientInfo.Key,
                        Port = clientInfo.Value,
                        ReceivedBuffer = buffer
                    };

                    lock (this)
                    {
                        EventHandler.InvokeReceivedData(receivedArgs);
                    }
                }
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
                await Task.Delay(0);
                if (dataQueue.Count == 0)
                    continue;
                if (dataQueue.TryDequeue(out QueueEntity entitiy))
                    SendData(entitiy.Data, entitiy.Receiver);
            }
        }

        /// <summary>
        /// Check all for dead connections and resend heartbeats
        /// </summary>
        private async Task CheckHeartbeats()
        {
            while (!isClosed)
            {
                foreach (var connection in Heartbeats.ToArray())
                {
                    if (isClosed)
                        return;
                    try
                    {
                        if (!Heartbeats.TryGetValue(connection.Key, out int hbCount))
                            return;

                        if (hbCount == 0) //If no hearbeats have been returned
                        {
                            Debug.WriteLine($"EasyHost: No hearbeats received from port {connection.Value}. Connection closed.");

                            //Remove from storage
                            Heartbeats.Remove(connection.Key);
                            ClientConnections.Remove(connection.Key);

                            //Close connection
                            connection.Key?.Close();
                            //connection.Key.Client.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"EasyHost: Heartbeats exception thrown in CheckHeartbeats:\n{e}");
                    }
                }

                foreach (var _ in Heartbeats.ToList())
                    Heartbeats[_.Key] = 0;

                SendHeartbeats();

                await Task.Delay(HeartbeatInterval);
            }
        }

        /// <summary>
        /// Send every connected TcpClient a heartbeat, if connected
        /// </summary>
        private void SendHeartbeats()
        {
            if (isClosed)
                return;

            foreach (var connection in Heartbeats.ToArray())
            {
                if (isClosed)
                    return;
                try
                {
                    if (!connection.Key.Connected)
                        continue;

                    //Send out Heartbeats to every client
                    SendData(new byte[] { (byte)DataType.HostHeartbeat, 0, 0, 0, 0 }, connection.Key);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"EasyHost: Error sending Heartbeats:\n{e}");
                }
            }
        }
    }
}
