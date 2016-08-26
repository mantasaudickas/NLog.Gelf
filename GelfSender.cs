using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Common;

namespace NLog.Gelf
{
    public class GelfSender
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private readonly bool _debugEnabled;

        public GelfSender(string serverUrl, bool debugEnabled = false)
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue {NoCache = true};

            _serverUrl = serverUrl;
            _debugEnabled = debugEnabled;
        }

        private class Proxy : IWebProxy
        {
            public Uri GetProxy(Uri destination)
            {
                return destination;
            }

            public bool IsBypassed(Uri host)
            {
                return false;
            }

            public ICredentials Credentials { get; set; }
        }

        public HttpResponseMessage Send(GelfMessage message)
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

            if (message.Fields != null)
            {
                foreach (var pair in message.Fields)
                {
                    Add(json, "_" + pair.Key, pair.Value);
                }
            }

            var body = JsonConvert.SerializeObject(json);
            if (_debugEnabled)
                InternalLogger.Debug($"Sending to {_serverUrl} message: {body}");

            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            var url = new Uri(_serverUrl);

            var response = Task.Run(async () => await _httpClient.PostAsync(url, content));
            var result = response.Result;

            if (_debugEnabled)
                InternalLogger.Debug($"Response status code {result.StatusCode}.");

            return result;
        }

        private void Add(JObject json, string property, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            json.Add(property, value);
        }
    }
}
