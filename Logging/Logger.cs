using System;
using System.Reflection;    

namespace EasyCommunication.Logging
{
    internal sealed class Logger : ILogger
    {
        public void Info(string data)
            => SendLog($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");
        public void Warn(string data)
            => SendLog($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");
        public void Error(string data)
            => SendLog($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");

        private void SendLog(string log, ConsoleColor color = ConsoleColor.Green)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
            Console.ResetColor();
        }
        private string CustomDateTime()
            => $"{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()}";
    }
}
