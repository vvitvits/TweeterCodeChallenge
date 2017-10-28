using System;
using System.Net;

namespace TweetController
{
    public class WebClient : IWebClient, IDisposable
    {
        private System.Net.WebClient _client;

        public WebClient()
        {
            _client = new System.Net.WebClient();
        }

        public WebHeaderCollection Headers => _client.Headers;

        public void Dispose()
        {
            _client?.Dispose();
        }

        public string DownloadString(string url)
        {
            return _client.DownloadString(url);
        }

        public string UploadString(string address, string data)
        {
            return _client.UploadString(address, data);
        }
    }
}
