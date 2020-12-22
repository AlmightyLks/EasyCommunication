using EasyCommunication.Events.Client.EventArgs;
using EasyCommunication.Events.Client.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.Host.Connection;
using EasyCommunication.SharedTypes;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace EasyCommunication.Client.Connection
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
        /// Whether or not the Client is connected with an <see cref="EasyHost"/>
        /// </summary>
        public bool ClientConnected => Client is null ? false : Client.Connected;

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
        /// Task which listens for incoming <see cref="EasyCommunication.Host.Connection.EasyHost"/> requests
        /// </summary>
        internal Task RequestListening { get; private set; }


        /// <summary>
        /// BinaryFormatter for serializing data into the Client's NetworkStream
        /// </summary>
        private BinaryFormatter binaryFormatter;

        /// <summary>
        /// <see cref="ILogger"/> instance, responsible for logging
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Creates an instance of <see cref="EasyClient"/>
        /// </summary>
        /// <param name="logger"><see cref="ILogger"/> DI instance</param>
        public EasyClient(ILogger logger = null)
        {
            this.logger = logger ?? new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Connection = null;
            Client = new TcpClient();
            binaryFormatter = new BinaryFormatter();
            EventHandler = new ClientEventHandler();
        }


        /// <summary>
        /// Connect to an <see cref="EasyHost"/>
        /// </summary>
        /// <param name="address">IPAddress to connect to</param>
        /// <param name="port">Port to connect to</param>
        /// <returns>Whether the connection has been established or not</returns>
        public bool ConnectToHost(IPAddress address, int port)
        {
            //If the client, for whatever reason, disconnected - Reconnect.
            if (ClientConnected)
                return false;

            RequestListening?.Dispose();    //Stop heartingbeating for old client
            Client = new TcpClient();       //Reset client

            bool result;
            try
            {
                Client.Connect(address, port);

                var cntArgs = new ConnectedToHostEventArgs() { Connection = Connection, Abort = false };
                EventHandler.InvokeConnectedToHost(cntArgs);

                if (cntArgs.Abort)
                {
                    DisconnectFromHost();
                    logger.Warning($"Connection attempt aborted");
                    result = false;
                }
                else
                {
                    StartListening();
                    logger.Information($"Connection attempt successfull");
                    result = true;
                }
            }
            catch
            {
                Connection = null;
                logger.Error($"Connection attempt failed");
                result = false;
            }

            if (result)
                Connection = new Connection() { IPAddress = address, Port = port };

            return result;
        }

        /// <summary>
        /// Disconnect from <see cref="EasyCommunication.Host.Connection.EasyHost"/>
        /// </summary>
        public void DisconnectFromHost()
        {
            if (!ClientConnected)
                return;

            var disconnectArgs = new DisconnectedFromHostEventArgs()
            {
                Connection = Connection
            };
            EventHandler.InvokeDisconnectedFromHost(disconnectArgs);

            Client.GetStream().Close();
            Client.Close();
            Connection = null;
        }

        /// <summary>
        /// Sends data to the connected <see cref="EasyHost"/>
        /// </summary>
        /// <typeparam name="T">Custom Type which has to be either JsonConvert'able or Serializable</typeparam>
        /// <param name="data">Data to send</param>
        /// <returns></returns>
        public SendStatus SendData<T>(T data)
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
            catch
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
                Receiver = Connection
            };
            EventHandler.InvokeSendingData(sendingArgs);

            if (!sendingArgs.Allow)
                return SendStatus.Disallowed;
            if (Connection is null)
                return SendStatus.NotConnected;

            try
            {
                binaryFormatter.Serialize(Client.GetStream(), actualData);
            }
            catch
            {
                return SendStatus.Unsuccessfull;
            }
            return SendStatus.Successfull;
        }

        /// <summary>
        /// Listens for incoming requests sent by <see cref="EasyCommunication.Host.Connection.EasyHost"/>
        /// </summary>
        /// <returns><see cref="RequestListening"/></returns>
        internal Task ListenForRequests()
        {
            for (; ; )
            {
                try
                {
                    if (!Client.Connected)
                        continue;

                    //Wait for and deserialize the incoming data
                    var receivedData = binaryFormatter.Deserialize(Client.GetStream());

                    //Received Data EventArgs
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
                catch (IOException)
                {
                    logger.Error($"Socket connection for {Connection.GetIPAndPort()} was closed.");
                    DisconnectFromHost();
                }
                catch (SocketException)
                {
                    logger.Error($"Socket connection for {Connection.GetIPAndPort()} was closed.");
                    DisconnectFromHost();
                }
                catch (SerializationException e)
                {
                    logger.Error($"\"{e.Message}\" {Connection.GetIPAndPort()}.");
                    DisconnectFromHost();
                }
                catch
                {
                    logger.Error($"Exception in listen:\n{Connection.GetIPAndPort()}");
                }
            }
        }

        /// <summary>
        /// Asynchronously starts and assigns <see cref="RequestListening"/>
        /// </summary>
        private void StartListening()
            => Task.Run(() => RequestListening = ListenForRequests());
    }
}
