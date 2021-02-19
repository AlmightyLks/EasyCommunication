using BenchmarkDotNet.Running;
using CommunicationBenchmark.Connection;
using CommunicationBenchmark.DataTransmission;
using CommunicationBenchmark.Serialization;

namespace CommunicationBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            var smth = new EasyClientTransmission();
            smth.GlobalSetup();
            smth.Setup();
            smth.TransmitShort();
#endif
            BenchmarkRunner.Run<EasyClientTransmission>();
            BenchmarkRunner.Run<EasyClientConnection>();
            BenchmarkRunner.Run<DataSerialization>();
        }
    }
}
