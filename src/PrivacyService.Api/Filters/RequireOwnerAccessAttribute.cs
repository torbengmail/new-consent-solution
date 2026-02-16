using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrivacyService.Api.Middleware;

namespace PrivacyService.Api.Filters;

/// <summary>
/// Action filter that validates the authenticated user has access to the owner ID
/// specified in the request. Checks both direct action arguments (query/route params)
/// and properties on body objects (DTOs).
/// </summary>
/// <param name="parameterName">
/// The name of the query/route parameter or DTO property containing the owner ID.
/// Defaults to "OwnerId" for body DTOs. Use "owner_id" for query parameters.
/// </param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RequireOwnerAccessAttribute(string parameterName = "OwnerId") : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var ownerId = ResolveOwnerId(context);

        if (ownerId.HasValue && !context.HttpContext.User.HasOwnerAccess(ownerId.Value))
        {
            context.Result = new ForbidResult();
            return;
        }

        base.OnActionExecuting(context);
    }

    private int? ResolveOwnerId(ActionExecutingContext context)
    {
        // 1. Try direct action argument (query param, route value)
        if (context.ActionArguments.TryGetValue(parameterName, out var directValue))
            return ToInt(directValue);

        // 2. Search body objects for a property with the given name
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null) continue;
            var prop = arg.GetType().GetProperty(parameterName);
            if (prop == null) continue;
            return ToInt(prop.GetValue(arg));
        }

        return null;
    }

    private static int? ToInt(object? value) => value switch
    {
        int i => i,
        _ => null
    };
}
