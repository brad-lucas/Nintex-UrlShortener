using NintexUrlShortener.Data;
using System;
using System.Net;
using System.Web.Http;

namespace NintexUrlShortener.Controllers
{
    /// <summary>
    /// API controller that handles URL operations - namely, shortening and inflating.
    /// </summary>
    //[RoutePrefix("url")]
    public sealed class UrlShortenerApiController : ApiController
    {
        private const string InflateRouteName = "InflateUrlRoute";

        /// <summary>
        /// The locally stored URL shortener to be used for operations.
        /// </summary>
        private readonly IUrlShortener urlShortener;

        /// <summary>
        /// Instantiates a new <see cref="UrlShortenerApiController" />.
        /// </summary>
        /// <remarks>DependencyInjection + IoC patterns would be best for overall design to allow for unit testing and would eliminate this ctor.</remarks>
        public UrlShortenerApiController()
            : this(new UrlShortener())
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="UrlShortenerApiController" />.
        /// </summary>
        /// <param name="urlShortener">The URL shortener to be used for operations.</param>
        /// <remarks>Needed for unit testing, though DependencyInjection + IoC patterns would be best and eliminate parameterless ctor.</remarks>
        internal UrlShortenerApiController(IUrlShortener urlShortener)
        {
            this.urlShortener = urlShortener;
        }

        /// <summary>
        /// API method for inflating a URL that has been shortened.
        /// </summary>
        /// <param name="id">The ID of the URL to inflate.</param>
        /// <returns>
        /// An <see cref="IHttpActionResult"/> indicating the success of the operation.
        /// </returns>
        [Route("{id}", Name = InflateRouteName)]
        [HttpGet]
        public IHttpActionResult InflateShortenedUrl(string id = null)
        {
            return this.PerformUrlShortenerOperation(() => this.urlShortener.Inflate(id));
        }

        /// <summary>
        /// API method for shortening a URL.
        /// </summary>
        /// <param name="url">The URL to shorten.</param>
        /// <returns>
        /// An <see cref="IHttpActionResult"/> indicating the success of the operation.
        /// </returns>
        [Route("shorten")]
        [HttpPost]
        public IHttpActionResult ShortenUrl(string url = null)
        {
            return this.PerformUrlShortenerOperation(
                () => this.urlShortener.Shorten(url, hash => this.Url.Link(InflateRouteName, new { id = hash })));
        }

        /// <summary>
        /// Helper method used to abstract common functionality in performing a URL shortener operation and then returning the appropriate API response.
        /// </summary>
        /// <param name="urlShortenerFunc">URL shortener operation to be performed.</param>
        /// <returns>An <see cref="IHttpActionResult" /> to be returned by the controller's public action.</returns>
        private IHttpActionResult PerformUrlShortenerOperation(Func<UrlShortenerResult> urlShortenerFunc)
        {
            // invoke the operation and get the UrlShortenerResult
            var result = urlShortenerFunc.Invoke();

            // handle translate resulting status code to a proper Web API response
            switch (result.StatusCode)
            {
                case HttpStatusCode.OK:
                    return this.Ok<string>(result.Message);

                case HttpStatusCode.BadRequest:
                    return this.BadRequest(result.Message);

                case HttpStatusCode.NotFound:
                    return this.NotFound();
            }

            throw new InvalidOperationException("Unhandled HttpStatusCode returned by UrlShortener function.");
        }
    }
}