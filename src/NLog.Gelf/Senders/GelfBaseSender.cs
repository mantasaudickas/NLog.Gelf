using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Common;

namespace NLog.Gelf.Senders
{    
    public abstract class GelfBaseSender
    {
        protected readonly bool DebugEnabled;

        protected readonly string ServerUrl;

        protected GelfBaseSender(string serverUrl, bool debugEnabled = false)
        {
            ServerUrl = serverUrl;
            DebugEnabled = debugEnabled;
        }

        protected abstract bool Send(string message);

        public void Send(GelfMessage message)
        {
            var json = new JObject();
            Add(json, "short_message", message.ShortMessage);
            Add(json, "full_message", message.FullMessage);
            Add(json, "host", message.Host);
            Add(json, "level", message.Level.ToString());
            Add(json, "facility", message.Facility);
            Add(json, "_levelName", message.LevelName);
            Add(json, "_exception_type", message.ExceptionType);
            Add(json, "_exception_message", message.ExceptionMessage);
            Add(json, "_exception_stack_trace", message.StackTrace);
            Add(json, "_logger", message.Logger);

            if (message.Fields != null && message.Fields.Count > 0)
            {
                foreach (var pair in message.Fields)
                {
                    Add(json, "_" + pair.Key, pair.Value);
                }
            }

            var body = JsonConvert.SerializeObject(json);
            if (DebugEnabled)
                InternalLogger.Debug($"Sending to {ServerUrl} message: {body}");

            var result = Send(body);

            if (DebugEnabled)
                InternalLogger.Debug($"Response {(result ? "successful" : "failed")}.");
        }

        private void Add(JObject json, string property, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            json.Add(property, value);
        }
    }
}