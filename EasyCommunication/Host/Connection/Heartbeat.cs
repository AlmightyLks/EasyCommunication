using EasyCommunication.Client.Connection;
using EasyCommunication.Logging;
using EasyCommunication.SharedTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace EasyCommunication.Host.Connection
{
    /// <summary>
    /// Heartbeat class, responsible for heartbeating connected <see cref="EasyClient"/>s
    /// </summary>
    internal sealed class Heartbeat : IDisposable
    {
        /// <summary>
        /// Clients and their amount of heartbeats since last query
        /// </summary>
        internal Dictionary<TcpClient, int> Heartbeats { get; private set; }

        /// <summary>
        /// <see cref="Connection.EasyHost"/> instance, providing connected <see cref="EasyClient"/>s
        /// </summary>
        internal EasyHost EasyHost { get; set; }

        /// <summary>
        /// Interval for querying heartbeats
        /// </summary>
        internal int HeartbeatInterval { get; set; }

        /// <summary>
        /// <see cref="ILogger"/> instance, responsible for logging
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// 
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Creates an instance of <see cref="Heartbeat"/>
        /// </summary>
        /// <param name="heartbeatInterval">Interval for querying heartbeats</param>
        internal Heartbeat(int heartbeatInterval)
        {
            logger = new Logger();

            HeartbeatInterval = heartbeatInterval;
            Heartbeats = new Dictionary<TcpClient, int>();
            EasyHost = null;
            isDisposed = false;
            new Task(async () => await CheckHeartbeats()).Start();
        }

        /// <summary>
        /// Check all for dead connections and resend heartbeats
        /// </summary>
        private async Task CheckHeartbeats()
        {
            while(!isDisposed)
            {
                foreach (var connection in Heartbeats.ToArray())
                {
                    if (isDisposed)
                        return;
                    try
                    {
                        if (!Heartbeats.TryGetValue(connection.Key, out int hbCount))
                            return;

                        if (hbCount == 0) //If no hearbeats have been returned
                        {
                            logger.Warn($"No hearbeats received from port {connection.Value}. Connection closed.");

                            //Remove from storage
                            Heartbeats.Remove(connection.Key);
                            EasyHost.ClientConnections.Remove(connection.Key);

                            //Close connection
                            connection.Key?.Close();
                            //connection.Key.Client.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Heartbeats exception thrown in CheckHeartbeats:\n{e}");
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
            if (isDisposed)
                return;
            Debug.WriteLine(Heartbeats.Count);
            foreach (var connection in Heartbeats.ToArray())
            {
                if (isDisposed)
                    return;
                try
                {
                    if (!connection.Key.Connected)
                        continue;

                    //Send out Heartbeats to every client
                    EasyHost.SendData(new byte[] { (byte)DataType.Heartbeat }, connection.Key);
                }
                catch (Exception e)
                {
                    logger.Error($"Error sending Heartbeats:\n{e}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            EasyHost = null;
        }
    }
}
