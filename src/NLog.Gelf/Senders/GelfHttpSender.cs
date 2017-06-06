using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using NLog.Common;

namespace NLog.Gelf.Senders
{
    public class GelfHttpSender : GelfBaseSender
    {
        private readonly HttpClient _httpClient;

        public GelfHttpSender(string serverUrl, bool debugEnabled = false)
            : base (serverUrl, debugEnabled)
        {
            this._httpClient = new HttpClient();

            this._httpClient.DefaultRequestHeaders.ExpectContinue = false;
            this._httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue {NoCache = true};
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

        protected override bool Send(string message)
        {
            var body = message;

            var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            var url = new Uri(this.ServerUrl);

            var result = this._httpClient.PostAsync(url, content).GetAwaiter().GetResult();
            if (!result.IsSuccessStatusCode)
                InternalLogger.Error($"Unable to send log message: {result.ReasonPhrase}");

            return result.IsSuccessStatusCode;
        }
    }
}
