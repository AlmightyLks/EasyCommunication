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
            BenchmarkRunner.Run<EasyClientTransmission>();
            BenchmarkRunner.Run<EasyClientConnection>();
            BenchmarkRunner.Run<DataSerialization>();
        }
    }
}
