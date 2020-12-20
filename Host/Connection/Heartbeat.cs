using EasyCommunication.Helper;
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
    internal class Heartbeat
    {
        internal Dictionary<TcpClient, int> Heartbeats { get; private set; }
        internal EasyHost EasyHost { get; set; }
        internal int HeartbeatInterval { get; set; }

        private Timer heartbeatTimer;

        internal Heartbeat(int heartbeatInterval)
        {
            HeartbeatInterval = heartbeatInterval;
            Heartbeats = new Dictionary<TcpClient, int>();
            heartbeatTimer = new Timer();
            EasyHost = null;

            Start();
        }

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
        internal void Stop()
        {
            heartbeatTimer.Elapsed -= HeartbeatTimer_Elapsed;
            heartbeatTimer.Stop();
            heartbeatTimer.Dispose();
        }

        private void HeartbeatTimer_Elapsed(object sender, ElapsedEventArgs e)
            => Task.Run(() => CheckHeartbeats());
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
                        Console.WriteLine($"No hearbeats received from port {connection.Value}. Connection closed.");

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
                Console.WriteLine($"Exception in Check Heartbeats:\n{e}");
            }
        }
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

                    Console.WriteLine($"Heartbeat sent for {connection.Value}: {SendStatus.Disallowed}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending Heartbeats:\n{e}");
                }
            }
        }
    }
}
