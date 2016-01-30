using System;

namespace NintexUrlShortener.Data
{
    public interface IUrlShortener
    {
        UrlShortenerResult Inflate(string id);

        UrlShortenerResult Shorten(string url, Func<string, string> getUrlFunc);
    }
}