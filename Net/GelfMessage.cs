using System.Collections.Generic;
using Newtonsoft.Json;

namespace NLog.Gelf.Net
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GelfMessage
    {
        [JsonProperty("facility")]
        public string Facility { get; set; }

        [JsonProperty("full_message")]
        public string FullMessage { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("short_message")]
        public string ShortMessage { get; set; }

        [JsonProperty("_exception_type")]
        public string ExceptionType { get; set; }

        [JsonProperty("_exception_message")]
        public string ExceptionMessage { get; set; }

        [JsonProperty("_exception_stack_trace")]
        public string StackTrace { get; set; }

        [JsonProperty("_logger")]
        public string Logger { get; set; }

        [JsonProperty("_notes")]
        public string Notes { get; set; }

        public IDictionary<string, string> Fields { get; set; }
    }
}
