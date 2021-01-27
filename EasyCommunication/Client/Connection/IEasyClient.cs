using EasyCommunication.Events.Client.EventHandler;
using EasyCommunication.SharedTypes;
using System.Net;

namespace EasyCommunication.Client.Connection
{
    public interface IEasyClient
    {
        int BufferSize { get; set; }
        bool ClientConnected { get; }
        Connection? Connection { get; set; }
        ClientEventHandler EventHandler { get; }

        void ConnectToHost(IPAddress address, int port);
        void DisconnectFromHost();
        QueueStatus QueueData<T>(T data, DataType dataType = DataType.ProtoBuf);
    }
}