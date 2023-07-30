namespace Metamory.Api.Repositories;


public interface IContentStatusEntity
{
	public DateTimeOffset? Timestamp { get; set; }   // In Azure table storage, this would be TableEntity.Timestamp
	public string ContentId { get; set; }   // in Azure table storage, this would be TableEntity.PartitionKey
	public string VersionId { get; set; }
	public DateTimeOffset StartTime { get; set; }
	public string Status { get; set; }
	public string Responsible { get; set; }
}