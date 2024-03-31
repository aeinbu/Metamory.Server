namespace Metamory.WebApi.Authorization;

public enum AccessControlAction
{
    Review          = 0x01,     // can see
    CreateOrModify  = 0x02,     // can upload and edit
    Publish         = 0x04      // can publish
}


// public enum AccessControlRole
// {
//     Editor = AccessControlAction.Publish | AccessControlAction.CreateOrModify | AccessControlAction.Review,
//     Contributor = AccessControlAction.CreateOrModify | AccessControlAction.Review,
//     Reviewer = AccessControlAction.Review,
// }