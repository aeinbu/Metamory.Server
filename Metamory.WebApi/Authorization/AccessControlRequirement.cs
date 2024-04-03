using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Metamory.WebApi.Authorization;

public class AccessControlRequirement(Permission action) : IAuthorizationRequirement
{
    public Permission Action => action;
}

public enum Permission
{
    Review = 0x01,     // can see
    CreateOrModify = 0x02,     // can upload and edit
    ChangeStatus = 0x04      // can publish
}

public class AccessControlRequirementHandler : AuthorizationHandler<AccessControlRequirement>
{
    private readonly AccessControlAuthorizer _accessControlAuthorizer;

    public AccessControlRequirementHandler(IOptions<AccessControlConfiguration> accessControlConfigurationAccessor)
    {
        var accessControlConfiguration = accessControlConfigurationAccessor.Value;
        _accessControlAuthorizer = new AccessControlAuthorizer(accessControlConfiguration.Path);
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessControlRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            if (_accessControlAuthorizer.IsAuthorized(requirement, httpContext))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}


public class AccessControlAuthorizer
{
    private readonly Func<string, string, bool> IsMatch;

    private readonly AccessControl _accessControl;

    private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public AccessControlAuthorizer(string path)
    {
        var serializer = new XmlSerializer(typeof(AccessControl));
        using var textReader = new StreamReader(path);
        _accessControl = (AccessControl)serializer.Deserialize(textReader);

        Regex getRegex(string pattern) => _cache.GetOrCreate(pattern, (_) => new Regex(pattern, RegexOptions.Compiled));

        IsMatch = _accessControl.AllowRegex
                ? (pattern, input) => pattern == ".*" ? true : getRegex(pattern).IsMatch(input)
                : (pattern, input) => input == pattern;
    }

    public bool IsAuthorized(AccessControlRequirement requirement, HttpContext httpContext)
    {
        var siteId = (string)httpContext.GetRouteValue("siteId");
        var contentId = (string)httpContext.GetRouteValue("contentId");

        var matchingContents = _accessControl.Sites.Where(site => IsMatch(site.Id, siteId))
            .SelectMany(site => site.Contents.Where(content => IsMatch(content.Id, contentId)));

        // var denyingRules = matchingContents
        //     .SelectMany(content => content.Denies.Where(deny => deny.Action == requirement.Action))
        //     .SelectMany(deny => deny.Rules.Where(rule => rule.IsMatch(httpContext)));

        // if(denyingRules.Any())
        //     return false;

        var allowingRules = matchingContents
            .SelectMany(content => content.Allows.Where(allow => allow.Permission == requirement.Action))
            .Where(allow => allow.MustBeAuthorized ? httpContext.User.Identities.Any(identity => identity.IsAuthenticated) : true)
            .SelectMany(allow => allow.Rules.Where(rule => rule.IsMatch(httpContext)));

        return allowingRules.Any();
    }
}
