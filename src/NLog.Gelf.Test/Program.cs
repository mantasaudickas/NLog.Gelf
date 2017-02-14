using System;
using System.Diagnostics;

namespace NLog.Gelf.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new LogFactory();
            var logger = factory.GetLogger("Test.Logger");

            var timer = Stopwatch.StartNew();
            for (int i = 0; i < 10000; ++i)
            {
                logger.Log(LogLevel.Info, $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fffffff}");
            }
            timer.Stop();
            Console.WriteLine("Time spent: {0}", timer.ElapsedMilliseconds);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }
    }
}