namespace Metamory.Api.Providers.AzureStorage;


public class AzureCanonicalizeService : ICanonicalizeService
{
    public string Canonicalize(string id)
    {
        //TODO: Remove or replace all characters that are invalid for Azure Storage
        return id.Replace("-", "");
    }
}