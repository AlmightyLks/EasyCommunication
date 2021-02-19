using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using EasyCommunication;
using EasyCommunication.Events.Host.EventArgs;
using EasyCommunication.SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommunicationBenchmark.DataTransmission
{
    [SimpleJob(
        launchCount: 1,
        warmupCount: 2,
        targetCount: 3
        )]
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class EasyClientTransmission
    {
        private EasyClient _client;
        private EasyHost _host;
        [GlobalSetup]
        public void GlobalSetup()
        {
            _client = new EasyClient(500);
            _host = new EasyHost(750, 9998, IPAddress.Loopback);
        }
        [IterationSetup]
        public void Setup()
        {
            _host.Open();
            Task.Delay(0).GetAwaiter().GetResult(); //I have no idea.
            _client.ConnectToHost(IPAddress.Loopback, 9998);
        }
        [IterationCleanup]
        public void Cleanup()
        {
            _client.DisconnectFromHost();
            _host.Close();
        }
        [Benchmark]
        public void TransmitShort()
        {
            short result = 0;
            short expected = 1;

            _host.EventHandler.ReceivedData += delegate (ReceivedDataEventArgs ev)
            {
                if (ev.ReceivedBuffer.DataType == DataType.Short)
                    result = BitConverter.ToInt16(ev.ReceivedBuffer.Data);
            };

            var status = _client.QueueData(expected, DataType.Short);
            while (result != expected) ;
        }
    }
}
