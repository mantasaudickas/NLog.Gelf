using System.Collections.Generic;

namespace NLog.Gelf
{
    public class GelfMessage
    {
        public string Facility { get; set; }

        public string FullMessage { get; set; }

        public string Host { get; set; }

        public string LevelName { get; set; }

        public int Level { get; set; }

        public string ShortMessage { get; set; }

        public string ExceptionType { get; set; }

        public string ExceptionMessage { get; set; }

        public string StackTrace { get; set; }

        public string Logger { get; set; }

        public string Notes { get; set; }

        public IDictionary<string, string> Fields { get; set; }
    }
}
