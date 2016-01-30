using System.Web.Mvc;

namespace NintexUrlShortener.Controllers
{
    public sealed class HomeController : Controller
    {
        public ActionResult Index()
        {
            this.ViewBag.Title = "Home Page";

            return this.View();
        }
    }
}
