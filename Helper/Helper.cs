using EasyCommunication.Client.Connection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyCommunication.Helper
{
    public static class Helper
    {
        public static int GetPort(this TcpListener listener)
            => (listener.LocalEndpoint as IPEndPoint)?.Port ?? -1;
        public static int GetPort(this TcpClient client)
            => (client.Client.RemoteEndPoint as IPEndPoint)?.Port ?? -1;

        public static string GetIPv4(this TcpListener listener)
        {
            IPAddress ip;
            IPAddress.TryParse((listener.LocalEndpoint as IPEndPoint)?.Address.ToString(), out ip);
            return ip.ToString() ?? "Unknown";
        }
        public static string GetIPv4(this TcpClient client)
        {
            IPAddress ip;
            IPAddress.TryParse((client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString(), out ip);
            return ip.ToString() ?? "Unknown";
        }

        public static string GetIPAndPort(this Connection? connection)
            => connection is null ? "<Unknown>" : $"{connection.Value.IPAddress}:{connection.Value.Port}";
    }
}
