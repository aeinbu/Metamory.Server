using System;
using Azure;
using Azure.Data.Tables;

namespace Metamory.Api.Repositories;


public class AzureTableContentStatusEntity : IContentStatusEntity, ITableEntity
{
	public DateTimeOffset? Timestamp { get; set; }
	public string ContentId { get; set; }
	public string VersionId { get; set; }
	public DateTimeOffset StartTime { get; set; }
	public string Status { get; set; }
	public string Responsible { get; set; }

	string ITableEntity.PartitionKey { get => ContentId; set => ContentId = value; }
	string ITableEntity.RowKey { get => VersionId; set => VersionId = value; }
	ETag ITableEntity.ETag { get; set; }
}