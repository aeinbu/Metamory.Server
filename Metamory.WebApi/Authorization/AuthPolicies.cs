namespace Metamory.WebApi.Authorization;


internal static class Policies
{
    public const string RequireListContentPermission = "ListContent";
    public const string RequireChangeStatusPermission = "ChangeStatus";
    public const string RequireCreateOrModifyPermission = "CreateOrEdit";
    public const string RequireReviewPermission = "Review";
    public const string RequireDeletePermission = "Delete";

    // public const string SystemAdminRole = "SystemAdmin";
    // public const string SiteAdminRole = "SiteAdmin";
}