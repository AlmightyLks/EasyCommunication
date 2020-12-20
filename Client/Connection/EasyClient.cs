using EasyCommunication.Events.Client.EventArgs;
using EasyCommunication.Events.Client.EventHandler;
using EasyCommunication.Helper;
using EasyCommunication.Logging;
using EasyCommunication.SharedTypes;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
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
        private ILogger logger;

        public EasyClient()
        {
            logger = new Logger();
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


            try
            {
                Client.Connect(address, port);
                logger.Info($"Connection attempt successfull");
            }
            catch
            {
                Connection = null;
                logger.Error($"Connection attempt failed");
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

                    //Wait for and deserialize the incoming data
                    var receivedData = binaryFormatter.Deserialize(Client.GetStream());

                    logger.Info($"Received request");

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
                    logger.Error($"Socket connection for {Connection.GetIPAndPort()} was closed.");

                    DisconnectFromHost();
                }
                catch (SocketException e)
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
                    logger.Error($"eee - Exception in listen:\n{Connection.GetIPAndPort()}");
                }
            }
        }
        private void StartListening()
            => Task.Run(() => RequestListening = ListenForRequests());
    }
}
