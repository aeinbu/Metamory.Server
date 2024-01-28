using Microsoft.AspNetCore.Authorization;

namespace Metamory.WebApi.Authorization;

public class RoleRequirement : IAuthorizationRequirement
{
    public string[] AcceptedRoles { get; set; }

    public RoleRequirement(params string[] queryStringKeys)
    {
        AcceptedRoles = queryStringKeys;
    }
}


public class RoleRequirementHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            if (requirement.AcceptedRoles.Any(role => httpContext.Request.Query.Any(x => x.Key == "role" && x.Value == role)))
            {
                context.Succeed(requirement);
            }
            else if(requirement.AcceptedRoles.Any(role => httpContext.Request.Headers.Any(x => x.Key == "role" && x.Value == role))){
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}
