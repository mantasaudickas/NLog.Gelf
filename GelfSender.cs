using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NLog.Gelf
{
    public class GelfSender
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string _serverUrl;

        public GelfSender(string serverUrl)
        {
            _serverUrl = serverUrl;
        }

        public HttpResponseMessage Send(GelfMessage message)
        {
            var json = new JObject();
            Add(json, "facility", message.Facility);
            Add(json, "full_message", message.FullMessage);
            Add(json, "host", message.Host);
            Add(json, "level", message.Level.ToString());
            Add(json, "levelName", message.LevelName);
            Add(json, "short_message", message.ShortMessage);
            Add(json, "_exception_type", message.ExceptionType);
            Add(json, "_exception_message", message.ExceptionMessage);
            Add(json, "_exception_stack_trace", message.StackTrace);
            Add(json, "_logger", message.Logger);

            if (message.Fields != null)
            {
                foreach (var pair in message.Fields)
                {
                    Add(json, pair.Key, pair.Value);
                }
            }

            var body = JsonConvert.SerializeObject(json);

            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
            var url = new Uri(_serverUrl);

            var response = Task.Run(async () => await _httpClient.PostAsync(url, content));
            return response.Result;
        }

        private void Add(JObject json, string property, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            json.Add(property, value);
        }
    }
}
