using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Ec2Manager.Models
{
    /// <summary>
    /// Middleware to handle errors and log them.
    /// </summary>
    public class ErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorHandler"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        public ErrorHandler(RequestDelegate next, ILogger<ErrorHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to handle the HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task that represents the completion of request processing.</returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
                _logger.LogInformation(context.Request.Path.ToString().TrimStart('/'));
                if (context.Response.StatusCode == 404)
                {
                    HandlePageNotFound(context);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An unhandled exception has occurred.");
                HandleException(context, e);
            }
        }

        /// <summary>
        /// Handles exceptions by setting a cookie and redirecting to the error page.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="e">The exception that occurred.</param>
        private static void HandleException(HttpContext context, Exception e)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMilliseconds(10000),
                IsEssential = true
            };
            context.Response.Cookies.Append("Error", e.Message, cookieOptions);
            context.Response.Redirect("/Error");
        }

        /// <summary>
        /// Handles 404 errors by setting a cookie and redirecting to the page not found page.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        private static void HandlePageNotFound(HttpContext context)
        {
            var pageNotFound = context.Request.Path.ToString().TrimStart('/');
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddMilliseconds(10000),
                IsEssential = true
            };
            context.Response.Cookies.Append("PageNotFound", pageNotFound, cookieOptions);
            context.Response.Redirect("/PageNotFound");
        }
    }
}