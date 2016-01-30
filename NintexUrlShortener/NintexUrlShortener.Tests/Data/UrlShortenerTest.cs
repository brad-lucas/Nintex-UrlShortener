using Microsoft.VisualStudio.TestTools.UnitTesting;
using NintexUrlShortener.Data;
using System;
using System.Net;

namespace NintexUrlShortener.Tests.Data
{
    [TestClass]
    public sealed class UrlShortenerTest
    {
        private const string FakeApiBaseUrl = "http://mysolutionislive.com/";

        private const string HttpPrefix = "http://";

        private const string HttpsPrefix = "https://";

        private const string NintexComWithoutWww = "nintex.com";

        private static readonly string NintexComWithWww = "www." + NintexComWithoutWww;

        private static readonly string NintexComWithWwwAndHttp = HttpPrefix + NintexComWithWww;

        private static readonly string NintexComWithoutWwwAndHttp = HttpPrefix + NintexComWithoutWww;

        private static readonly string NintexComWithWwwAndHttps = HttpsPrefix + NintexComWithWww;

        private static readonly string NintexComWithoutWwwAndHttps = HttpsPrefix + NintexComWithoutWww;

        private UrlShortener urlShortener;

        [TestInitialize]
        public void Initialize()
        {
            this.urlShortener = new UrlShortener();
        }

        [TestMethod]
        public void UrlShortener_Inflate()
        {
            var result = this.urlShortener.Inflate(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            result = this.urlShortener.Inflate("a");
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);

            // set up some valid hits on lookup
            this.UrlShortener_Shorten();

            result = this.urlShortener.Inflate("b");
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(new Uri(NintexComWithWwwAndHttp, UriKind.Absolute).AbsoluteUri, result.Message);

            result = this.urlShortener.Inflate(Guid.NewGuid().ToString());
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
        }

        [TestMethod]
        public void UrlShortener_Shorten()
        {
            var result = this.TestShorten(NintexComWithoutWww);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'a', result.Message);
            
            // should get the same result from any following request, though that needs to be tested more at the UrlShortener level
            result = this.TestShorten(NintexComWithoutWww);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'a', result.Message);
            
            result = this.TestShorten(NintexComWithWww);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'b', result.Message);

            // we default to prepending http:// as means of trying to dedupe
            result = this.TestShorten(NintexComWithWwwAndHttp);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'b', result.Message);

            result = this.TestShorten(NintexComWithWwwAndHttps);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'c', result.Message);

            result = this.TestShorten(NintexComWithWwwAndHttps + '#');
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'c', result.Message);

            result = this.TestShorten(NintexComWithWwwAndHttps + "/#");
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'c', result.Message);

            result = this.TestShorten(NintexComWithWwwAndHttps + "/?");
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'c', result.Message);

            result = this.TestShorten(NintexComWithWwwAndHttps + "/?somequerystring=abc123");
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'd', result.Message);

            // double-check original, shouold be the same
            result = this.TestShorten(NintexComWithoutWww);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.IsNotNull(result.Message);
            Assert.AreEqual(FakeApiBaseUrl + 'a', result.Message);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UrlShortener_Shorten_GetUrlFuncIsNull()
        {
            this.urlShortener.Shorten(Guid.NewGuid().ToString(), null);
        }

        [TestMethod]
        public void UrlShortener_Shorten_UrlIsNull()
        {
            var result = this.TestShorten(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual(UrlShortener.ShortenUrlNullOrEmptyMessage, result.Message);
        }

        [TestMethod]
        public void UrlShortener_Shorten_UrlIsInvalidOrNotWellFormed()
        {
            Action<string> testInvalidOrNotWellFormed = input =>
            {
                var result = this.TestShorten(input);
                Assert.IsNotNull(result);
                Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
                Assert.AreEqual(UrlShortener.ShortenUrlInvalidMessage, result.Message);
            };

            testInvalidOrNotWellFormed.Invoke("123");
            testInvalidOrNotWellFormed.Invoke("http://");
        }

        private static string GetMockInflatedUrl(string hash)
        {
            return FakeApiBaseUrl + hash;
        }

        private UrlShortenerResult TestShorten(string url)
        {
            return this.urlShortener.Shorten(url, hash => GetMockInflatedUrl(hash));
        }
    }
}
