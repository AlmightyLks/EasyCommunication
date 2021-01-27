namespace EasyCommunication.SharedTypes
{
    public enum QueueStatus : byte
    {
        Queued = 0,
        IllegalFormat = 1,
        BufferTransgression = 2,
        NotOpen = 3
    }
}
