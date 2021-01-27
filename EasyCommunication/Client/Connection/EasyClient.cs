using EasyCommunication.Events.Client.EventArgs;
using EasyCommunication.Events.Client.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.Host.Connection;
using EasyCommunication.Logging;
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

namespace EasyCommunication.Client.Connection
{
    /// <summary>
    /// Establish a connection and communicate with an <see cref="EasyHost"/>
    /// </summary>
    /// <remarks>
    /// <para>Send and receive data from a connected <see cref="EasyHost"/></para>
    /// </remarks>
    public class EasyClient : IEasyClient
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
        /// TcpClient for establishing a connection
        /// </summary>
        internal TcpClient Client { get; set; }

        /// <summary>
        /// <see cref="ILogger"/> instance, responsible for logging
        /// </summary>
        private ILogger logger;

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
        public EasyClient(Encoding encoding = null)
        {
            logger = new Logger();
            BufferSize = 1024;
            Connection = null;
            Client = new TcpClient();
            EventHandler = new ClientEventHandler();
            dataQueue = new Queue<byte[]>();
            isDisconnected = true;
            stringEncoding = encoding ?? Encoding.UTF8;
        }

        /// <summary>
        /// Connect to an <see cref="EasyHost"/>
        /// </summary>
        /// <param name="address">IPAddress to connect to</param>
        /// <param name="port">Port to connect to</param>
        /// <returns>Whether the connection has been established or not</returns>
        public void ConnectToHost(IPAddress address, int port)
        {
            //If the client, for whatever reason, disconnected - Reconnect.
            if (ClientConnected)
                return;

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
                    StartListening();
                    isDisconnected = false;
                    new Task(async () => await SendQueuedData()).Start();
                }
                else
                {
                    DisconnectFromHost();
                    Connection = null;
                }
            }
            catch(Exception e)
            {
                Connection = null;
                Debug.WriteLine($"EasyClient: Connection attempt failed");
            }
        }

        /// <summary>
        /// Disconnect from <see cref="EasyCommunication.Host.Connection.EasyHost"/>
        /// </summary>
        public void DisconnectFromHost()
        {
            if (!ClientConnected)
                return;

            SendData(new byte[] { (byte)DataType.Disconnect });
            CloseConnection();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CloseConnection()
        {
            isDisconnected = true;
            Client.Close();

            var evArgs = new DisconnectedFromHostEventArgs()
            {
                Connection = Connection,
                Reconnect = false
            };
            EventHandler.InvokeDisconnectedFromHost(evArgs);
            if (evArgs.Reconnect)
                ConnectToHost(Connection.Value.IPAddress, Connection.Value.Port);
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
                if (data[0] != (byte)DataType.Heartbeat && data[0] != (byte)DataType.Disconnect)
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
        /// Listens for incoming requests sent by <see cref="EasyCommunication.Host.Connection.EasyHost"/>
        /// </summary>
        /// <returns><see cref="RequestListening"/></returns>
        internal void ListenForRequests()
        {
            byte[] buffer;
            while (!isDisconnected)
            {
                try
                {
                    if (!Client.Connected)
                        continue;

                    buffer = new byte[BufferSize];

                    Debug.WriteLine($"EasyClient: Waiting for Requests...");

                    //Wait for and deserialize the incoming data

                    int bytesRead = Client.GetStream().Read(buffer, 0, buffer.Length);
                    byte[] trimmedBuffer = new byte[bytesRead];
                    Array.Copy(buffer, trimmedBuffer, bytesRead);

                    if (trimmedBuffer.Length == 1 && trimmedBuffer[0] == (byte)DataType.Heartbeat)
                    {
                        SendData(new byte[] { (byte)DataType.Heartbeat });
                    }
                    else if (trimmedBuffer.Length == 1 && trimmedBuffer[0] == (byte)DataType.Disconnect)
                    {
                        CloseConnection();
                        break;
                    }
                    else
                    {
                        byte[] trimmedData = new byte[trimmedBuffer.Length - 1];
                        Array.Copy(trimmedBuffer, 1, trimmedData, 0, trimmedData.Length);

                        //Received Data Event
                        var receivedArgs = new ReceivedDataEventArgs()
                        {
                            Sender = Connection,
                            Type = (DataType)trimmedBuffer[0],
                            Data = trimmedData
                        };

                        EventHandler.InvokeReceivedData(receivedArgs);
                    }
                }
                catch (IOException e)
                {
                    Debug.WriteLine($"EasyClient: EasyClient: Socket connection for {Connection.GetIPAndPort()} was closed.\n{e}");

                    if (Client != null && !Client.Connected)
                        DisconnectFromHost();
                }
                catch (SocketException e)
                {
                    Debug.WriteLine($"EasyClient: EasyClient: Socket connection for {Connection.GetIPAndPort()} was closed.\n{e}");

                    if (Client != null && !Client.Connected)
                        CloseConnection();
                }
                catch (SerializationException e)
                {
                    Debug.WriteLine($"EasyClient: \"{e.Message}\" {Connection.GetIPAndPort()}.");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"EasyClient: eee - Exception in listen:\n{Connection.GetIPAndPort()}\n{e}");
                }
            }
        }

        /// <summary>
        /// Asynchronously starts and assigns <see cref="RequestListening"/>
        /// </summary>
        private void StartListening()
            => new Task(() => ListenForRequests()).Start();

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
                    await Task.Delay(75);
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
    }
}
