using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CREEOT.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "X-Api-Key";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expectedKey = config.GetValue<string>("InternalApiKey");

            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedKey)
                || !string.Equals(extractedKey, expectedKey))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid or missing API Key" });
                return;
            }
            await next();
        }
    }
}
