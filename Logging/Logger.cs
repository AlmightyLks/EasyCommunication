using System;
using System.Reflection;

namespace EasyCommunication.Logging
{
    internal sealed class Logger : ILogger
    {
        public void Info(string data)
        {
            Console.ResetColor();
            Console.WriteLine($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");
        }
        public void Warn(string data)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");
            Console.ResetColor();
        }
        public void Error(string data)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");
            Console.ResetColor();
        }
        private string CustomDateTime()
            => $"{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()}";
    }
}
