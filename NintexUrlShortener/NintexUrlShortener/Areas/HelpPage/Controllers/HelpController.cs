using NintexUrlShortener.Areas.HelpPage.ModelDescriptions;
using NintexUrlShortener.Areas.HelpPage.Models;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace NintexUrlShortener.Areas.HelpPage.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the help page.
    /// </summary>
    public sealed class HelpController : Controller
    {
        private const string ErrorViewName = "Error";

        public HelpController()
            : this(GlobalConfiguration.Configuration)
        {
        }

        public HelpController(HttpConfiguration config)
        {
            this.Configuration = config;
        }

        public HttpConfiguration Configuration { get; private set; }

        public ServicesContainer Services
        {
            get { return this.Configuration.Services; }
        }

        public ActionResult Index()
        {
            this.ViewBag.DocumentationProvider = this.Services.GetDocumentationProvider();
            return this.View(this.Services.GetApiExplorer().ApiDescriptions);
        }

        public ActionResult Api(string apiId)
        {
            if (!string.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = this.Configuration.GetHelpPageApiModel(apiId);
                if (apiModel != null)
                {
                    return this.View(apiModel);
                }
            }

            return this.View(ErrorViewName);
        }

        public ActionResult ResourceModel(string modelName)
        {
            if (!string.IsNullOrEmpty(modelName))
            {
                ModelDescriptionGenerator modelDescriptionGenerator = this.Configuration.GetModelDescriptionGenerator();

                ModelDescription modelDescription;
                if (modelDescriptionGenerator.GeneratedModels.TryGetValue(modelName, out modelDescription))
                {
                    return this.View(modelDescription);
                }
            }

            return this.View(ErrorViewName);
        }
    }
}