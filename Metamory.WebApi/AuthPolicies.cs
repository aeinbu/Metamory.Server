namespace Metamory.WebApi
{
    internal static class AuthPolicies
    {
        // public const string SystemAdminRole = "SystemAdmin";
        // public const string SiteAdminRole = "SiteAdmin";
        public const string EditorRole = "Editor";
        public const string ContributorRole = "Contributor";
        public const string ReviewerRole = "Reviewer";
        public const string SiteIdClaim = "SiteId";
    }
}