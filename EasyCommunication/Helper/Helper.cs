using EasyCommunication.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyCommunication.Helper
{
    public static class Helper
    {
        /// <summary>
        /// Gets the TcpListener's port
        /// </summary>
        /// <remarks>
        /// <para>If null, -1</para>
        /// </remarks>
        /// <param name="listener"></param>
        /// <returns>TcpListener's port</returns>
        public static int GetPort(this TcpListener listener)
        {
            try
            {
                return (listener?.LocalEndpoint as IPEndPoint)?.Port ?? -1;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the TcpClient's port
        /// </summary>
        /// <remarks>
        /// <para>If null, -1</para>
        /// </remarks>
        /// <param name="client"></param>
        /// <returns>TcpClient's port</returns>
        public static int GetPort(this TcpClient client)
        {
            try
            {
                return (client?.Client?.RemoteEndPoint as IPEndPoint)?.Port ?? -1;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the TcpListener's IPv4 Address
        /// </summary>
        /// <remarks>
        /// <para>If null, Unknown</para>
        /// </remarks>
        /// <param name="listener"></param>
        /// <returns>TcpListener's IPv4 Address</returns>
        public static string GetIPv4(this TcpListener listener)
        {
            try
            {
                IPAddress.TryParse((listener?.LocalEndpoint as IPEndPoint)?.Address.ToString(), out IPAddress ip);
                return ip?.ToString() ?? "Unknown";
            }
            catch
            {
                return "<Unknown>";
            }
        }

        /// <summary>
        /// Gets the TcpClient's IPv4 Address
        /// </summary>
        /// <remarks>
        /// <para>If null, Unknown</para>
        /// </remarks>
        /// <param name="client"></param>
        /// <returns>TcpClient's IPv4 Address</returns>
        public static string GetIPv4(this TcpClient client)
        {
            try
            {
                IPAddress.TryParse((client?.Client?.RemoteEndPoint as IPEndPoint)?.Address.ToString(), out IPAddress ip);
                return ip?.ToString() ?? "Unknown";
            }
            catch
            {
                return "<Unknown>";
            }
        }

        /// <summary>
        /// Combines <see cref="Connection"/> into a IPAddress:Port string
        /// </summary>
        /// <remarks>
        /// <para>If null, Unknown</para>
        /// </remarks>
        /// <param name="connection"></param>
        /// <returns>Connection's IPAddress:Port string</returns>
        public static string GetIPAndPort(this Connection? connection)
        {
            try
            {
                return connection == null ? "<Unknown>" : $"{connection.Value.IPAddress}:{connection.Value.Port}";
            }
            catch
            {
                return "<Unknown>";
            }
        }
    }
}
