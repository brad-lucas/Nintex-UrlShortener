using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NintexUrlShortener.Controllers;
using NintexUrlShortener.Data;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;

namespace NintexUrlShortener.Tests.Controllers
{
    [TestClass]
    public sealed class UrlShortenerApiControllerTest
    {
        private const string NintexComWithoutWww = "nintex.com";

        private Mock<IUrlShortener> mockUrlShortener;

        private Mock<UrlHelper> mockUrlHelper;

        private UrlShortenerApiController controller;

        [TestInitialize]
        public void Initialize()
        {
            this.mockUrlHelper = new Mock<UrlHelper>(MockBehavior.Strict);

            this.mockUrlShortener = new Mock<IUrlShortener>(MockBehavior.Strict);

            this.controller = new UrlShortenerApiController(this.mockUrlShortener.Object)
            {
                Configuration = new Mock<HttpConfiguration>().Object,
                Request = new Mock<HttpRequestMessage>().Object,
                Url = this.mockUrlHelper.Object
            };
        }

        [TestMethod]
        public async Task UrlShortenerApiControllerTest_InfalteShortenedUrl()
        {
            this.mockUrlShortener
                .Setup(us => us.Inflate("123"))
                .Returns(new UrlShortenerResult(HttpStatusCode.OK) { Message = NintexComWithoutWww });

            var result = await this.controller.InflateShortenedUrl("123").ExecuteAsync(CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
            Assert.AreEqual(NintexComWithoutWww, await GetResponseContentTrimmed(result));

            this.mockUrlShortener
                .Setup(us => us.Inflate("456"))
                .Returns(new UrlShortenerResult(HttpStatusCode.BadRequest));

            result = await this.controller.InflateShortenedUrl("456").ExecuteAsync(CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);

            this.mockUrlShortener
                .Setup(us => us.Inflate("789"))
                .Returns(new UrlShortenerResult(HttpStatusCode.NotFound));

            result = await this.controller.InflateShortenedUrl("789").ExecuteAsync(CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.NotFound, result.StatusCode);
        }

        [TestMethod]
        public async Task UrlShortenerApiControllerTest_Shorten_UrlShortenerSaysSuccess()
        {
            var mockInflatedUrl = GetMockInflatedUrl("abc123");

            this.mockUrlShortener
                .Setup(us => us.Shorten(NintexComWithoutWww, It.IsAny<Func<string, string>>()))
                .Returns(new UrlShortenerResult(HttpStatusCode.OK) { Message = mockInflatedUrl });

            var result = await this.controller.ShortenUrl(NintexComWithoutWww).ExecuteAsync(CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
            Assert.AreEqual(mockInflatedUrl, await GetResponseContentTrimmed(result));

            // should get the same result from any following request, though that needs to be tested more at the UrlShortener level
            result = await this.controller.ShortenUrl(NintexComWithoutWww).ExecuteAsync(CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessStatusCode);
            Assert.AreEqual(mockInflatedUrl, await GetResponseContentTrimmed(result));
        }

        [TestMethod]
        public async Task UrlShortenerApiControllerTest_Shorten_UrlShortenerSaysInvalid()
        {
            this.mockUrlShortener
                .Setup(us => us.Shorten(It.IsAny<string>(), It.IsAny<Func<string, string>>()))
                .Returns(new UrlShortenerResult(HttpStatusCode.BadRequest));

            var result = await this.controller.ShortenUrl(Guid.NewGuid().ToString()).ExecuteAsync(CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.IsNull(result.Content);

            this.mockUrlShortener
                .Setup(us => us.Shorten(It.IsAny<string>(), It.IsAny<Func<string, string>>()))
                .Returns(new UrlShortenerResult(HttpStatusCode.BadRequest) { Message = "abc123" });

            result = await this.controller.ShortenUrl(Guid.NewGuid().ToString()).ExecuteAsync(CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.IsTrue((await GetResponseContentTrimmed(result)).Contains("abc123")); // error states don't just return the string, it's wrapped as a "Message"
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task UrlShortenerApiControllerTest_Shorten_UnsupportedUrlShortenerStatusCode()
        {
            this.mockUrlShortener
                .Setup(us => us.Shorten(It.IsAny<string>(), It.IsAny<Func<string, string>>()))
                .Returns(new UrlShortenerResult(HttpStatusCode.Ambiguous));

            var result = await this.controller.ShortenUrl(Guid.NewGuid().ToString()).ExecuteAsync(CancellationToken.None);
        }

        private static string GetMockInflatedUrl(string hash)
        {
            return "http://mysolutionislive.com/" + hash;
        }

        private static async Task<string> GetResponseContentTrimmed(HttpResponseMessage response)
        {
            return (await response.Content.ReadAsStringAsync()).Trim('"');
        }

        private void SetUpMockUrlHelperForInflateRoute(string hash)
        {
            this.mockUrlHelper
                .Setup(urlHelper => urlHelper.Link(UrlShortenerApiController.InflateRouteName, new { id = hash }))
                .Returns(GetMockInflatedUrl(hash));
        }
    }
}
