using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NintexUrlShortener.Data
{
    public sealed class UrlShortener : IUrlShortener
    {
        internal const string ShortenUrlInvalidMessage = "URL provided is not valid.";

        internal const string ShortenUrlNullOrEmptyMessage = "Must provide a URL.";

        private static readonly Lazy<KeyValuePair<string, int>> DefaultKeyValuePairLazy = new Lazy<KeyValuePair<string, int>>();

        /// <summary>
        /// Lookup table in-memory rather than rolling out a stored DB / distributed cache version of storage.
        /// </summary>
        /// <remarks>Downside of implementation is that memory will get wiped on redeployment.</remarks>
        private static readonly Lazy<IDictionary<string, int>> LookupTableLazy = new Lazy<IDictionary<string, int>>(() => new ConcurrentDictionary<string, int>());

        /// <summary>
        /// Tries to inflate a URL based on an encoded internal system ID.
        /// </summary>
        /// <param name="id">An encoded ID used to reverse-lookup a long URL.</param>
        /// <returns>A <see cref="UrlShortenerResult" /> indicating the outcome of the inflation operation, along with the inflated URL if successful.</returns>
        public UrlShortenerResult Inflate(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new UrlShortenerResult(HttpStatusCode.BadRequest)
                {
                    Message = "Must provide an ID."
                };
            }

            var numericId = UrlShortenerEncoder.Decode(id);

            var matchingKeyValuePair = LookupTableLazy.Value.FirstOrDefault(ltv => ltv.Value == numericId);
            if (matchingKeyValuePair.Equals(DefaultKeyValuePairLazy.Value))
            {
                return new UrlShortenerResult(HttpStatusCode.NotFound);
            }

            return new UrlShortenerResult(HttpStatusCode.OK)
            {
                Message = matchingKeyValuePair.Key
            };
        }

        /// <summary>
        /// Tries to shorten a URL.
        /// </summary>
        /// <param name="url">A URL to shorten.</param>
        /// <param name="getUrlFunc">A function, prescribed by the caller, used to generate the shortened URL.</param>
        /// <returns>A <see cref="UrlShortenerResult" /> indicating the outcome of the shortening operation, along with the shortened URL if successful.</returns>
        public UrlShortenerResult Shorten(string url, Func<string, string> getUrlFunc)
        {
            // should never happen
            if (getUrlFunc == null)
            {
                throw new ArgumentNullException("getUrlFunc");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return new UrlShortenerResult(HttpStatusCode.BadRequest)
                {
                    Message = ShortenUrlNullOrEmptyMessage
                };
            }

            if (url.All(char.IsDigit))
            {
                return new UrlShortenerResult(HttpStatusCode.BadRequest)
                {
                    Message = ShortenUrlInvalidMessage
                };
            }

            if (!url.StartsWith("http"))
            {
                url = "http://" + url; // assume HTTP (not HTTPS)
            }

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri) || !uri.IsWellFormedOriginalString())
            {
                return new UrlShortenerResult(HttpStatusCode.BadRequest)
                {
                    Message = ShortenUrlInvalidMessage
                };
            }

            int shortUrlId;

            // potential optimization: strip "www." (concerns about redirecting to the right host)
            string urlForLookup = uri.AbsoluteUri.TrimEnd('?').TrimEnd('#');

            if (LookupTableLazy.Value.ContainsKey(urlForLookup))
            {
                shortUrlId = LookupTableLazy.Value[urlForLookup];
            }
            else
            {
                shortUrlId = LookupTableLazy.Value.Count;
                LookupTableLazy.Value.Add(urlForLookup, shortUrlId);
            }

            return new UrlShortenerResult(HttpStatusCode.OK)
            {
                Message = getUrlFunc.Invoke(UrlShortenerEncoder.Encode(shortUrlId))
            };
        }
    }
}