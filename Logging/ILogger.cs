namespace EasyCommunication.Logging
{
    internal interface ILogger
    {
        void Info(string data);
        void Error(string data);
        void Warn(string data);
    }
}