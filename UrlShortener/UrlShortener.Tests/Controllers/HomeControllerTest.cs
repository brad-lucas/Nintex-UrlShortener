using Microsoft.VisualStudio.TestTools.UnitTesting;
using NintexUrlShortener.Controllers;
using System.Web.Mvc;

namespace NintexUrlShortener.Tests.Controllers
{
    [TestClass]
    public sealed class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
    }
}