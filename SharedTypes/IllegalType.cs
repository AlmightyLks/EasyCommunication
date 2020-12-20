namespace EasyCommunication.SharedTypes
{
    //Neither Serializable, nor JsonConvert'able

    /// <summary>
    /// Intentional demonstration of a type which is neither Serializable nor JsonConvert'able.
    /// </summary>
    public sealed class IllegalType
    {
        /// <summary>
        /// Self-reference
        /// </summary>
        public IllegalType Test { get; set; }
        public IllegalType()
        {
            Test = this;
        }
    }
}
