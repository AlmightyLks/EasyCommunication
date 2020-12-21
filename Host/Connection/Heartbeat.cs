using EasyCommunication.Client.Connection;
using EasyCommunication.Helper;
using EasyCommunication.Logging;
using EasyCommunication.SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace EasyCommunication.Host.Connection
{
    /// <summary>
    /// Heartbeat class, responsible for heartbeating connected <see cref="EasyClient"/>s
    /// </summary>
    internal class Heartbeat
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
        /// Timer responsible for <see cref="HeartbeatInterval"/>
        /// </summary>
        private Timer heartbeatTimer;

        /// <summary>
        /// <see cref="ILogger"/> instance, responsible for logging
        /// </summary>
        private ILogger logger;

        /// <summary>
        /// Creates an instance of <see cref="Heartbeat"/>
        /// </summary>
        /// <param name="heartbeatInterval">Interval for querying heartbeats</param>
        /// <param name="logger">Logger DI</param>
        internal Heartbeat(int heartbeatInterval, ILogger logger)
        {
            this.logger = logger;

            HeartbeatInterval = heartbeatInterval;
            Heartbeats = new Dictionary<TcpClient, int>();
            heartbeatTimer = new Timer();
            EasyHost = null;

            Start();
        }

        /// <summary>
        /// Start <see cref="heartbeatTimer"/>
        /// </summary>
        internal void Start()
        {
            if (heartbeatTimer != null)
                Stop();

            heartbeatTimer = new Timer();
            heartbeatTimer.Elapsed += HeartbeatTimer_Elapsed;
            heartbeatTimer.Interval = HeartbeatInterval;
            heartbeatTimer.AutoReset = true;
            heartbeatTimer.Start();
        }
        /// <summary>
        /// Stop <see cref="heartbeatTimer"/>
        /// </summary>
        internal void Stop()
        {
            heartbeatTimer.Elapsed -= HeartbeatTimer_Elapsed;
            heartbeatTimer.Stop();
            heartbeatTimer.Dispose();
        }

        /// <summary>
        /// Elapsed <see cref="heartbeatTimer"/> event-method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
            => Task.Run(() => CheckHeartbeats());
        /// <summary>
        /// Check all for dead connections and resend heartbeats
        /// </summary>
        private void CheckHeartbeats()
        {
            try
            {
                foreach (var connection in EasyHost.ClientConnections.ToArray())
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
                        connection.Key.Client.Close();
                    }
                }

                foreach (var _ in Heartbeats.ToList())
                    Heartbeats[_.Key] = 0;

                SendHeartbeats();
            }
            catch (Exception e)
            {
                logger.Error($"Exception in Check Heartbeats:\n{e}");
            }
        }
        /// <summary>
        /// Send every connected TcpClient a heartbeat, if connected
        /// </summary>
        private void SendHeartbeats()
        {
            foreach (var connection in EasyHost.ClientConnections.ToArray())
            {
                try
                {
                    if (!connection.Key.Connected)
                        continue;

                    //Send out Heartbeats to every client
                    SendStatus status = EasyHost.SendData(new HeartbeatPing(), connection.Key);

                    logger.Info($"Heartbeat sent for {connection.Value}: {status}");
                }
                catch (Exception e)
                {
                    logger.Error($"Error sending Heartbeats:\n{e}");
                }
            }
        }
    }
}
