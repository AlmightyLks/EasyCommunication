using EasyCommunication.Events.Client.EventArgs;
using EasyCommunication.Events.Client.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.Serialization;
using EasyCommunication.SharedTypes;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EasyCommunication.Connection
{
    /// <summary>
    /// Establish a connection and communicate with an <see cref="EasyHost"/>
    /// </summary>
    /// <remarks>
    /// <para>Send and receive data from a connected <see cref="EasyHost"/></para>
    /// </remarks>
    public class EasyClient
    {
        /// <summary>
        /// 
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// Whether or not the Client is connected with an <see cref="EasyHost"/>
        /// </summary>
        public bool ClientConnected => Client == null ? false : Client.Connected;

        /// <summary>
        /// EventHandler for EasyClient-events
        /// </summary>
        public ClientEventHandler EventHandler { get; private set; }

        /// <summary>
        /// Nullable <see cref="Connection"/> information
        /// </summary>
        /// <remarks>
        /// Null if not connected
        /// </remarks>
        public Connection? Connection { get; set; }

        /// <summary>
        /// Clients and their amount of heartbeats since last query
        /// </summary>
        internal int Heartbeats { get; private set; }

        /// <summary>
        /// TcpClient for establishing a connection
        /// </summary>
        internal TcpClient Client { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int HeartbeatInterval { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private DateTime lastHeartbeat;

        /// <summary>
        /// 
        /// </summary>
        private Encoding stringEncoding;

        /// <summary>
        /// 
        /// </summary>
        private Queue<byte[]> dataQueue;

        /// <summary>
        /// 
        /// </summary>
        private bool isDisconnected;

        /// <summary>
        /// Creates an instance of <see cref="EasyClient"/>
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> DI instance</param>
        public EasyClient(int heartbeatInterval, Encoding encoding = null)
        {
            BufferSize = 1024;
            Connection = null;
            Client = new TcpClient();
            EventHandler = new ClientEventHandler();
            dataQueue = new Queue<byte[]>();
            isDisconnected = true;
            stringEncoding = encoding ?? Encoding.UTF8;
            HeartbeatInterval = heartbeatInterval;
        }

        /// <summary>
        /// Connect to an <see cref="EasyHost"/>
        /// </summary>
        /// <param name="address">IPAddress to connect to</param>
        /// <param name="port">Port to connect to</param>
        /// <returns>Whether the connection has been established or not</returns>
        public void ConnectToHost(IPAddress address, int port)
        {
            //Reset client
            Client = new TcpClient();

            Connection = new Connection() { IPAddress = address, Port = port };

            try
            {
                Client.Connect(address, port);
                var evArgs = new ConnectedToHostEventArgs()
                {
                    Abort = false,
                    Connection = Connection
                };

                EventHandler.InvokeConnectedToHost(evArgs);

                if (!evArgs.Abort)
                {
                    Debug.WriteLine($"EasyClient: Connection attempt successfull");
                    new Task(() => ListenForRequests()).Start();
                    isDisconnected = false;
                    Heartbeats = 1;
                    new Task(async () => await SendQueuedData()).Start();
                    new Task(async () => await CheckHeartbeats()).Start();
                }
                else
                {
                    DisconnectFromHost();
                    Connection = null;
                }
            }
            catch (Exception e)
            {
                Connection = null;
                Debug.WriteLine($"EasyClient: Connection attempt failed");
            }
        }

        /// <summary>
        /// Disconnect from <see cref="EasyCommunication.Host.EasyHost"/>
        /// </summary>
        public void DisconnectFromHost()
        {
            SendData(new byte[] { (byte)DataType.Disconnect, 0, 0, 0, 0 });
            isDisconnected = true;
            Client.Close();
            Connection = null;
        }

        /// <summary>
        /// 
        /// </summary>
        private void DisconnectFromHostEvent()
        {
            var evArgs = new DisconnectedFromHostEventArgs()
            {
                Connection = Connection
            };
            EventHandler.InvokeDisconnectedFromHost(evArgs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public QueueStatus QueueData<T>(T data, DataType dataType)
        {
            if (isDisconnected)
                return QueueStatus.NotOpen;

            byte[] buffer;

            try
            {
                buffer = SerializationHelper.GetBuffer(data, dataType);
                if (buffer.Length > BufferSize)
                    return QueueStatus.BufferTransgression;
                dataQueue.Enqueue(buffer);
            }
            catch (SerializationException e)
            {
                return QueueStatus.IllegalFormat;
            }
            return QueueStatus.Queued;
        }

        /// <summary>
        /// Sends data to the connected <see cref="EasyHost"/>
        /// </summary>
        /// <typeparam name="T">Custom Type which has to be either JsonConvert'able or Serializable</typeparam>
        /// <param name="data">Data to send</param>
        /// <returns></returns> 
        internal void SendData(byte[] data)
        {
            if (isDisconnected)
                return;
            try
            {
                //If not heartbeat or disconnect
                if (data.Length > 1)
                {
                    var sendingArgs = new SendingDataEventArgs()
                    {
                        Allow = true,
                        Receiver = Connection,
                        Type = (DataType)data[0]
                    };

                    byte[] trimmedBuffer = new byte[data.Length - 1];
                    Array.Copy(data, 1, trimmedBuffer, 0, data.Length - 1);
                    sendingArgs.Data = trimmedBuffer;

                    EventHandler.InvokeSendingData(sendingArgs);

                    if (!sendingArgs.Allow)
                        return;
                    if (Connection is null)
                        return;
                }

                Client.GetStream().Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception in SendData:\n{e}");
            }
        }

        /// <summary>
        /// Listens for incoming requests sent by <see cref="EasyCommunication.Host.EasyHost"/>
        /// </summary>
        /// <returns><see cref="RequestListening"/></returns>
        internal void ListenForRequests()
        {
            byte[] receivingBuffer;
            while (!isDisconnected)
            {
                try
                {
                    if (!ClientConnected)
                        continue;

                    receivingBuffer = new byte[BufferSize];

                    Debug.WriteLine($"EasyClient: Waiting for Requests...");

                    int bytesRead = Client.GetStream().Read(receivingBuffer, 0, receivingBuffer.Length);

                    byte[] dataBuffer = new byte[bytesRead];
                    Array.Copy(receivingBuffer, dataBuffer, bytesRead);

                    //Do not accept empty buffers
                    if (bytesRead == 0)
                        continue;

                    if (bytesRead == 5)
                    {
                        switch ((DataType)dataBuffer[0])
                        {
                            case DataType.HostHeartbeat:
                                {
                                    Debug.WriteLine("EasyClient: HostHeartbeat received");
                                    SendData(new byte[] { (byte)DataType.HostHeartbeat, 0, 0, 0, 0 });
                                }
                                break;
                            case DataType.ClientHeartbeat:
                                {
                                    Debug.WriteLine("EasyClient: ClientHeartbeat received");
                                    Heartbeats++;
                                }
                                break;
                            case DataType.Disconnect:
                                {
                                    DisconnectFromHostEvent();
                                    DisconnectFromHost();
                                }
                                break;
                        }
                    }
                    else
                    {
                        new Task(() => HandleData(dataBuffer, bytesRead)).Start();
                    }
                }
                catch (IOException e)
                {
                    Debug.WriteLine($"EasyClient: EasyClient: Socket connection for {Connection.GetIPAndPort()} was closed.\n{e}");

                    if (Client != null && !Client.Connected)
                    {
                        DisconnectFromHost();
                    }
                    DisconnectFromHostEvent();
                }
                catch (SocketException e)
                {
                    Debug.WriteLine($"EasyClient: Socket connection for {Connection.GetIPAndPort()} was closed.\n{e}");

                    if (Client != null && !Client.Connected)
                    {
                        DisconnectFromHost();
                    }
                    DisconnectFromHostEvent();
                }
                catch (ObjectDisposedException e)
                {
                    Debug.WriteLine($"EasyClient: Socket connection for {Connection.GetIPAndPort()} was disposed.\n{e}");

                    if (Client != null && !Client.Connected)
                    {
                        DisconnectFromHost();
                    }
                    DisconnectFromHostEvent();
                }
                catch (InvalidOperationException e)
                {
                    Debug.WriteLine($"EasyClient: Socket connection for {Connection.GetIPAndPort()} was disposed.\n{e}");

                    if (Client != null && !Client.Connected)
                    {
                        DisconnectFromHost();
                    }
                    DisconnectFromHostEvent();
                }
                catch (SerializationException e)
                {
                    Debug.WriteLine($"EasyClient: \"{e.Message}\" {Connection.GetIPAndPort()}.");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"EasyClient: eee - Exception in listen:\n{Connection.GetIPAndPort()}\n{e}");

                    //Just to make sure, if something unexpected happens, close connection
                    if (Client != null && !Client.Connected)
                    {
                        DisconnectFromHost();
                    }
                    DisconnectFromHostEvent();
                }
            }
        }

        private void HandleData(byte[] buffer, int bytesRead)
        {
            //If less than or equal to 5 bytes long (Min. size), invalid.
            if (bytesRead <= 5)
                return;

            //Remove "meta-"data
            bytesRead -= 5;

            //Convert promised size
            uint dataLength = BitConverter.ToUInt32(buffer, 1);

            //If promised size does not equal the received data length
            if (bytesRead != dataLength)
                return;

            DataType dataType = (DataType)buffer[0];
            byte[] trimmedBuffer = new byte[dataLength];
            Array.Copy(buffer, 5, trimmedBuffer, 0, bytesRead);

            //Received Data Event
            var receivedArgs = new ReceivedDataEventArgs()
            {
                Sender = Connection,
                Type = dataType,
                Data = trimmedBuffer
            };

            EventHandler.InvokeReceivedData(receivedArgs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task SendQueuedData()
        {
            while (!isDisconnected)
            {
                try
                {
                    await Task.Delay(1);
                    if (dataQueue.Count == 0)
                        continue;
                    byte[] data = dataQueue.Dequeue();
                    SendData(data);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"EasyClient in SendQueuedData:\n{e}");
                }
            }
        }

        /// <summary>
        /// Check all for dead connections and resend heartbeats
        /// </summary>
        private async Task CheckHeartbeats()
        {
            while (!isDisconnected)
            {
                await Task.Delay(HeartbeatInterval);

                try
                {
                    if (Heartbeats == 0) //If no hearbeats have been returned
                    {
                        Debug.WriteLine($"EasyClient: No hearbeats received. Connection closed.");
                        DisconnectFromHost();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"EasyClient: Heartbeats exception thrown in CheckHeartbeats:\n{e}");
                }

                Heartbeats = 0;

                try
                {
                    //Send out Heartbeats to every client
                    SendData(new byte[] { (byte)DataType.ClientHeartbeat, 0, 0, 0, 0 });
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"EasyClient: Error sending Heartbeat:\n{e}");
                }
            }
        }
    }
}
