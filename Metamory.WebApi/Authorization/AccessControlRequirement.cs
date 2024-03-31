using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Authorization;

namespace Metamory.WebApi.Authorization;

public class AccessControlRequirement(AccessControlAction action) : IAuthorizationRequirement
{
    public AccessControlAction Action => action;
}


public class AccessControlRequirementHandler : AuthorizationHandler<AccessControlRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AccessControlRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            //TODO: do not read an deserialize this file on every call!!! Use a singleton or something
            if (new AccessControlAuthorizer("authorization-noAuth.config.xml").IsAuthorized(requirement, httpContext))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;

    }
}

[XmlRoot("accessControl")]
public class AccessControl
{
    [XmlAttribute("allowRegex")]
    public bool AllowRegex { get; set; }

    [XmlElement("site")]
    public List<Site> Sites { get; set; }

    public class Site
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("content")]
        public List<Content> Contents { get; set; }

        public class Content
        {
            [XmlAttribute("id")]
            public string Id { get; set; }

            [XmlElement("allow")]
            public List<Allow> Allows { get; set; }

            public class Allow
            {
                [XmlAttribute("action")]
                public AccessControlAction Action { get; set; }

                [XmlAttribute(AttributeName = "mustBeAuthorized")]
                public bool MustBeAuthorized { get; set; }


                [XmlElement("header", typeof(Header))]
                [XmlElement("querystring", typeof(Querystring))]
                public List<Rule> Rules { get; set; }

                public abstract class Rule
                {
                    protected IEnumerable<string> ResolveRoles(IEnumerable<string> roles) => roles.SelectMany(role => role switch
                    {
                        "editor" => ["editor", "contributor", "reviewer"],
                        "contributor" => ["contributor", "reviewer"],
                        "reviewer" => ["reviewer"],
                        _ => Array.Empty<string>()
                    }).Distinct();


                    public abstract bool IsMatch(HttpContext httpContext);
                }

                public class Header : Rule
                {
                    [XmlAttribute("name")]
                    public string Name { get; set; }

                    [XmlAttribute("value")]
                    public string Value { get; set; }

                    public override bool IsMatch(HttpContext httpContext)
                    {
                        var headers = httpContext.Request.Headers;

                        //TODO: check ALL form values if multiple
                        var roles = ResolveRoles(headers[Name]);

                        return roles.Contains(Value);
                    }
                }

                public class Querystring : Rule
                {
                    [XmlAttribute("name")]
                    public string Name { get; set; }

                    [XmlAttribute("value")]
                    public string Value { get; set; }

                    public override bool IsMatch(HttpContext httpContext)
                    {
                        var queryString = httpContext.Request.Query;

                        //TODO: check ALL querystring values if multiple
                        var roles = ResolveRoles(queryString[Name]);
                        return roles.Contains(Value);
                    }
                }
            }
        }
    }
}


internal class AccessControlAuthorizer
{
    private readonly Func<string, string, bool> IsMatch;

    private readonly AccessControl _accessControl;

    public AccessControlAuthorizer(string path)
    {
        var serializer = new XmlSerializer(typeof(AccessControl));
        using var textReader = new StreamReader(path);
        _accessControl = (AccessControl)serializer.Deserialize(textReader);

        IsMatch = _accessControl.AllowRegex
                ? (pattern, input) => pattern == ".*" ? true : new Regex(pattern).IsMatch(input)
                : (pattern, input) => input == pattern;

    }

    internal bool IsAuthorized(AccessControlRequirement requirement, HttpContext httpContext)
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
            .SelectMany(content => content.Allows.Where(allow => allow.Action == requirement.Action))
            .SelectMany(allow => allow.Rules.Where(rule => rule.IsMatch(httpContext)));

        return allowingRules.Any();
    }
}
