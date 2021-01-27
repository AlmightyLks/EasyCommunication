using EasyCommunication.Events.Host.EventHandler;
using EasyCommunication.SharedTypes;
using System.Collections.Generic;
using System.Net.Sockets;

namespace EasyCommunication.Host.Connection
{
    public interface IEasyHost
    {
        int BufferSize { get; set; }
        IDictionary<TcpClient, int> ClientConnections { get; }
        HostEventHandler EventHandler { get; }
        bool Listening { get; }
        int ListeningPort { get; }
        TcpListener TcpListener { get; }

        void Close();
        void Open();
        QueueStatus QueueData<T>(T data, TcpClient receiver, DataType dataType = DataType.ProtoBuf);
    }
}