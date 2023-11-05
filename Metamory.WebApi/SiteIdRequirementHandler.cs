using Microsoft.AspNetCore.Authorization;

namespace Metamory.WebApi;


public class SiteIdRequirement : IAuthorizationRequirement
{
}


public class SiteIdRequirementHandler : AuthorizationHandler<SiteIdRequirement>
{
    private const string claimNamespace = "https://metamory.server";

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SiteIdRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            var currentSiteId = (string)httpContext.GetRouteValue("siteId");
            if (context.User.Claims.Any(claim => claim.Type == $"{claimNamespace}/siteId" && claim.Value == currentSiteId))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}