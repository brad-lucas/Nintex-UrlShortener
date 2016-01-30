using System.Net;

namespace NintexUrlShortener.Data
{
    public sealed class UrlShortenerResult
    {
        public UrlShortenerResult(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }

        public string Message { get; set; }

        public HttpStatusCode StatusCode { get; private set; }
    }
}