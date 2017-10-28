using System;
using System.Net;

namespace TweetController
{
    public interface IWebClient:IDisposable
    {
        string DownloadString(string url);

        string UploadString(string url, string content);

        WebHeaderCollection Headers { get; }
    }
}
