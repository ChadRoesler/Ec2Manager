using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Ec2Manager.Models
{
    public class ErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandler> _logger;

        public ErrorHandler(RequestDelegate next, ILogger<ErrorHandler> logger)
        {
            _next = next;
            _logger = logger;

        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
                _logger.LogError(context.Request.Path.ToString().TrimStart('/'));
                //  Handle specific HTTP status codes
                switch (context.Response.StatusCode)
                {
                    case 404:
                        HandlePageNotFound(context);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e}");
                //  Handle uncaught global exceptions (treat as 500 error)
                HandleException(context, e);
            }
            finally
            {
            }
        }

        //  500
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

        //  404
        private static void HandlePageNotFound(HttpContext context)
        {
            //  Display an information page that displays the bad url using a cookie
            string pageNotFound = context.Request.Path.ToString().TrimStart('/');
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