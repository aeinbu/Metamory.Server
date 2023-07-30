using Metamory.Api.Repositories;

namespace Metamory.Api.Providers.FileSystem;


public class FileContentStatusEntity : IContentStatusEntity
{
	public DateTimeOffset? Timestamp { get; set; }
	public string ContentId { get; set; }
	public string VersionId { get; set; }
	public DateTimeOffset StartTime { get; set; }
	public string Status { get; set; }
	public string Responsible { get; set; }

	public override string ToString()
	{
		return $"{Timestamp:o};{ContentId};{VersionId};{StartTime:o};{Status};{Responsible}";
	}

	public static FileContentStatusEntity FromString(string line)
	{
		var parts = line.Split(';');
		if (parts.Length != 6)
			throw new ArgumentException(); //TODO: Throw appropriate exception

		return new FileContentStatusEntity
		{
			Timestamp = DateTimeOffset.Parse(parts[0]),
			ContentId = parts[1],
			VersionId = parts[2],
			StartTime = DateTimeOffset.Parse(parts[3]),
			Status = parts[4],
			Responsible = parts[5],
		};
	}
}