using System.Xml.Serialization;

namespace Metamory.WebApi.Authorization;

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
    }

    public class Content
    {
        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlElement("allow")]
        public List<Allow> Allows { get; set; }
    }

    public class Allow
    {
        [XmlAttribute("permission")]
        public Permission Permission { get; set; }

        [XmlAttribute(AttributeName = "mustBeAuthorized")]
        public bool MustBeAuthorized { get; set; }

        [XmlElement("header", typeof(Header))]
        [XmlElement("querystring", typeof(Querystring))]
        [XmlElement("claim", typeof(Claim))]
        [XmlElement("username", typeof(Username))]
        public List<Rule> Rules { get; set; }
    }

    public abstract class Rule
    {
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
            var roles = headers[Name].Distinct();
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
            var roles = queryString[Name].Distinct();
            return roles.Contains(Value);
        }
    }

    public class Claim : Rule
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }

        public override bool IsMatch(HttpContext httpContext)
        {
            var claims = httpContext.User.Claims;
            var roleClaims = claims.Where(claim => claim.Type == Type).Select(claim => claim.Value).Distinct();
            return roleClaims.Contains(Value);
        }
    }

    public class Username : Rule
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        public override bool IsMatch(HttpContext httpContext)
        {
            var username = httpContext.User.Identity.Name;
            return username == Name;
        }
    }

}
