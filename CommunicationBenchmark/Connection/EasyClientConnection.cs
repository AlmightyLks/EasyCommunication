using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using EasyCommunication;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CommunicationBenchmark.Connection
{
    [SimpleJob(
        RunStrategy.Throughput,
        launchCount: 1,
        warmupCount: 2,
        targetCount: 3
        )]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class EasyClientConnection
    {
        private EasyClient _client;
        private EasyHost _host;
        [GlobalSetup]
        public void GlobalSetup()
        {
            _client = new EasyClient(500);
            _host = new EasyHost(750, 9999, IPAddress.Loopback);
        }
        [IterationSetup]
        public void Setup()
        {
            _host.Open();
        }
        [IterationCleanup]
        public void Cleanup()
        {
            _client.DisconnectFromHost();
            _host.Close();
        }
        [Benchmark]
        public void Connect()
        {
            _client.ConnectToHost(IPAddress.Loopback, 9999);
            while (!_client.ClientConnected) ;
        }
        [Benchmark]
        public async Task Reconnect()
        {
            _client.ConnectToHost(IPAddress.Loopback, 9999);
            await Task.Delay(1);
            _client.DisconnectFromHost();
            await Task.Delay(1);
            _client.ConnectToHost(IPAddress.Loopback, 9999);
            await Task.Delay(1);
            while (_host.ClientConnections.Count == 0) ;
        }
        [Benchmark]
        public void HeartbeatDisconnect()
        {
            //Simulate EasyHost connecting but not responding to heartbeats
            using (var client = new TcpClient())
                client.Connect(IPAddress.Loopback, 9999);

            while (_host.ClientConnections.Count == 1) ;
        }
    }
}
