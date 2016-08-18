namespace NLog.Gelf
{
    internal static class LogLevelExtension
    {
        private const int Emergency = 0;
        //private const int Alert = 1;
        //private const int Critical = 2;
        private const int Error = 3;
        private const int Warning = 4;
        //private const int Notice = 5;
        private const int Info = 6;
        private const int Debug = 7;

        public static int GelfSeverity(this LogLevel logLevel)
        {
            if (logLevel == LogLevel.Error) return Error;
            if (logLevel == LogLevel.Warn) return Warning;
            if (logLevel == LogLevel.Info) return Info;
            return logLevel == LogLevel.Fatal ? Emergency : Debug;
        }
    }
}
