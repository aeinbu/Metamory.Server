namespace Metamory.Api.Providers.FileSystem;


public class FileCanonicalizeService : ICanonicalizeService
{
    public string Canonicalize(string id)
    {
        ///TODO: remove or replace all chars that are invalid for files, like like /, \ and .
        return id;
    }
}