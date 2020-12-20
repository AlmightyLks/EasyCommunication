namespace EasyCommunication.SharedTypes
{
    //Neither serializable, not jsonconvertable
    public class SomeTest
    {
        public SomeTest Test { get; set; }
        public SomeTest()
        {
            Test = this;
        }
    }
}
