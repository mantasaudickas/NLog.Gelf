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

            while (true)
            {
                var timer = Stopwatch.StartNew();
                for (int i = 0; i < 100; ++i)
                {
                    logger.Log(LogLevel.Info, $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fffffff}");
                }
                timer.Stop();
                Console.WriteLine("Time spent: {0}", timer.ElapsedMilliseconds);

                Console.WriteLine("Press ANY key to repeat, ESC to exit");

                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                    break;
            }
        }
    }
}