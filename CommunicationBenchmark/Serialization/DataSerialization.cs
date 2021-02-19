using EasyCommunication.Serialization;
using EasyCommunication.SharedTypes;
using EasyCommunication.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ProtoBuf;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;

namespace CommunicationBenchmark.Serialization
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
    public class DataSerialization
    {
        [Benchmark]
        [Arguments(2)]
        public void SerializeShort(short input)
        {
            _ = SerializationHelper.GetBuffer(input, DataType.Short);
        }
        [Benchmark]
        [Arguments(2)]
        public void SerializeInteger(int input)
        {
            _ = SerializationHelper.GetBuffer(input, DataType.Int);
        }
        [Benchmark]
        [Arguments(2)]
        public void SerializeLong(long input)
        {
            _ = SerializationHelper.GetBuffer(input, DataType.Long);
        }
        [Benchmark]
        [Arguments(1.1f)]
        public void SerializeFloat(float input)
        {
            _ = SerializationHelper.GetBuffer(input, DataType.Float);
        }
        [Benchmark]
        [Arguments(1.1d)]
        public void SerializeDouble(Double input)
        {
            _ = SerializationHelper.GetBuffer(input, DataType.Double);
        }
        [Benchmark]
        [Arguments(true)]
        public void SerializeBool(bool input)
        {
            _ = SerializationHelper.GetBuffer(input, DataType.Bool);
        }
        [Benchmark]
        [Arguments("Hello")]
        public void SerializeString(string input)
        {
            _ = SerializationHelper.GetBuffer(input, DataType.String);
        }
        [Benchmark]
        public void SerializeJsonString()
        {
            _ = SerializationHelper.GetBuffer(new Point(1, 1), DataType.JsonString);
        }
        [Benchmark]
        public void SerializeProtoBuf()
        {
            _ = SerializationHelper.GetBuffer(new ProtoBufType(1, 1), DataType.ProtoBuf);
        }
        [Benchmark]
        public void SerializeByteArray()
        {
            _ = SerializationHelper.GetBuffer(new byte[] { 1, 0, 0, 0 }, DataType.ByteArray);
        }
        [Benchmark]
        public void GetStackedBuffer()
        {
            byte[] expectedData = new byte[] {
                (byte)DataType.Int, 4, 0, 0, 0, 2, 0, 0, 0
            };
            _ = expectedData.GetStackedBuffers();
        }
        [Benchmark]
        public void GetStackedBuffers()
        {
            byte[] expectedData = new byte[] {
                (byte)DataType.Int, 4, 0, 0, 0, 2, 0, 0, 0,
                (byte)DataType.HostHeartbeat, 0, 0, 0, 0,
                (byte)DataType.Int, 4, 0, 0, 0, 0, 0, 0, 0,
            };

            _ = expectedData.GetStackedBuffers();
        }
    }
    [ProtoContract]
    internal class ProtoBufType
    {
        [ProtoMember(1)] public int Num1 { get; set; }
        [ProtoMember(2)] public int Num2 { get; set; }
        public ProtoBufType(int num1, int num2)
        {
            Num1 = num1;
            Num2 = num2;
        }
    }
}
