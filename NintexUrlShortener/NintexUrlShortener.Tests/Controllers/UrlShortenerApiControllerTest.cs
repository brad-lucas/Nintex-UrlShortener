using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NintexUrlShortener.Controllers;
using System.Web.Http.Routing;

namespace NintexUrlShortener.Tests.Controllers
{
    [TestClass]
    public sealed class UrlShortenerApiControllerTest
    {
        private Mock<UrlHelper> mockUrlHelper;

        private UrlShortenerApiController controller;

        [TestInitialize]
        public void Initialize()
        {
            this.mockUrlHelper = new Mock<UrlHelper>(MockBehavior.Strict);

            this.controller = new UrlShortenerApiController
            {
                Url = this.mockUrlHelper.Object
            };
        }

        [TestMethod]
        public void UrlShortenerApiControllerTest_ValidUrl()
        {
            var brad = this.controller.ShortenUrl("http://www.google.com");
        }

        private void SetUpMockUrlHelperForInflateRoute(string hash)
        {
            this.mockUrlHelper
                .Setup(urlHelper => urlHelper.Link(string.Empty, It.IsAny<object>()));
        }
    }
}
