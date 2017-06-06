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
            this.ServerUrl = serverUrl;
            this.DebugEnabled = debugEnabled;
        }

        protected abstract bool Send(string message);

        public void Send(GelfMessage message)
        {
            var json = new JObject();
            this.Add(json, "short_message", message.ShortMessage);
            this.Add(json, "full_message", message.FullMessage);
            this.Add(json, "host", message.Host);
            this.Add(json, "level", message.Level.ToString());
            this.Add(json, "facility", message.Facility);
            this.Add(json, "_levelName", message.LevelName);
            this.Add(json, "_exception_type", message.ExceptionType);
            this.Add(json, "_exception_message", message.ExceptionMessage);
            this.Add(json, "_exception_stack_trace", message.StackTrace);
            this.Add(json, "_logger", message.Logger);

            if (message.Fields != null)
            {
                foreach (var pair in message.Fields)
                {
                    this.Add(json, "_" + pair.Key, pair.Value);
                }
            }

            var body = JsonConvert.SerializeObject(json);
            if (this.DebugEnabled)
                InternalLogger.Debug($"Sending to {this.ServerUrl} message: {body}");
            var result = this.Send(body);

            if (this.DebugEnabled)
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