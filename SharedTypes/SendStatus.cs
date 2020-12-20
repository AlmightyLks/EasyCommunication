namespace EasyCommunication.SharedTypes
{
    /// <summary>
    /// Status for Sending Data
    /// </summary>
    public enum SendStatus
    {
        /// <summary>
        /// Data sent successfully
        /// </summary>
        Successfull = 0,

        /// <summary>
        /// Data sent unsuccessfully
        /// </summary>
        Unsuccessfull = 1,

        /// <summary>
        /// Client not connected
        /// </summary>
        NotConnected = 2,

        /// <summary>
        /// The transmission was disallowed
        /// </summary>
        Disallowed = 3
    }
}
