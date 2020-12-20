namespace EasyCommunication.SharedTypes
{
    //Neither Serializable, nor JsonConvert'able
    internal sealed class SomeTest
    {
        internal SomeTest Test { get; set; }
        internal SomeTest()
        {
            Test = this;
        }
    }
}
