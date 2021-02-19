using EasyCommunication.SharedTypes;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyCommunication.Helper
{
    public static class Helper
    {
        public static IEnumerable<ReceivedBuffer> GetStackedBuffers(this byte[] buffer)
        {
            List<ReceivedBuffer> result = new List<ReceivedBuffer>();
            int offset = 0;

            //If less than to 5 bytes long (Min. size), invalid.
            if (buffer.Length < 5)
                return result;

            do
            {
                //Convert promised size
                int dataLength = BitConverter.ToInt32(buffer, offset + 1);

                //If promised size + 5 id-bytes is greater than the received data length
                if ((dataLength + 5) > buffer.Length)
                    break;

                byte[] trimmedBuffer = new byte[dataLength];
                Array.Copy(buffer, 5, trimmedBuffer, 0, dataLength);

                result.Add(new ReceivedBuffer()
                {
                    DataType = (DataType)buffer[offset],
                    Data = trimmedBuffer
                });

                //Offset to size id for next
                offset += (dataLength + 5);

            } while (offset < buffer.Length);

            return result;
        }

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
