using System;
using System.Reflection;

namespace EasyCommunication.Logging
{
    internal interface ILogger
    {
        void Info(string data);
        void Error(string data);
        void Warn(string data);
    }

    /// <summary>
    /// Internal EasyCommunication's Logger
    /// </summary>
    internal sealed class Logger : ILogger
    {
        /// <summary>
        /// Log Info
        /// </summary>
        /// <param name="data"></param>
        public void Info(string data)
            => SendLog($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");

        /// <summary>
        /// Log Warn
        /// </summary>
        /// <param name="data"></param>
        public void Warn(string data)
            => SendLog($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");

        /// <summary>
        /// Log Error
        /// </summary>
        /// <param name="data"></param>
        public void Error(string data)
            => SendLog($"{CustomDateTime()} | {Assembly.GetCallingAssembly().GetName().Name}: {data}");

        /// <summary>
        /// Send Log to Console
        /// </summary>
        /// <param name="log"></param>
        /// <param name="color"></param>
        private void SendLog(string log, ConsoleColor color = ConsoleColor.Green)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(log);
            Console.ResetColor();
        }

        /// <summary>
        /// Custom DateTime String
        /// </summary>
        /// <returns></returns>
        private string CustomDateTime()
            => $"{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToLongTimeString()}";
    }
}
