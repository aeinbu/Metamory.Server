namespace Metamory.Api.Providers.AzureStorage;


public class AzureStorageRepositoryConfiguration : IAzureBlobContentRepositoryConfiguration, IAzureTableStatusRepositoryConfiguration
{
    public string AccountName { get; set; }
    public string ConnectionString { get; set; }
}