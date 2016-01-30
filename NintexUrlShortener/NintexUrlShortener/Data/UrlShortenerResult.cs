using System.Net;

namespace NintexUrlShortener.Data
{
    public sealed class UrlShortenerResult
    {
        public string Message { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}