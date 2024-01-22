using Microsoft.AspNetCore.Authorization;

namespace Metamory.WebApi.Authorization;

public class QueryStringRequirement : IAuthorizationRequirement
{
    public string[] QueryStringKeys { get; set; }

    public QueryStringRequirement(params string[] queryStringKeys)
    {
        QueryStringKeys = queryStringKeys;
    }
}


public class QueryStringRequirementHandler : AuthorizationHandler<QueryStringRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, QueryStringRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            if (requirement.QueryStringKeys.Any(key => httpContext.Request.Query.Any(x => x.Key == "role" && x.Value == key)))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}
