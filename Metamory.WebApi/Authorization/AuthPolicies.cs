namespace Metamory.WebApi.Authorization;


internal static class Policies
{
    public const string RequireChangeStatusPermission = "ChangeStatus";
    public const string RequireCreateOrModifyPermission = "CreateOrEdit";
    public const string RequireReviewPermission = "Review";


    // public const string SystemAdminRole = "SystemAdmin";
    // public const string SiteAdminRole = "SiteAdmin";
}