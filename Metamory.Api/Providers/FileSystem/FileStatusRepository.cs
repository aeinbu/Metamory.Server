using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Metamory.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Metamory.Api.Providers.FileSystem;


public interface IFileStatusRepositoryConfiguration
{
	string StatusRootPath { get; }
}


public class FileStatusRepository : IStatusRepository
{
	public class Configurator
	{
		public Configurator(ConfigurationManager configuration, IServiceCollection services)
		{
            var metamoryConfiguration = configuration.GetSection("Metamory.Api");
			services.Configure<FileSystemRepositoryConfiguration>(metamoryConfiguration.GetSection("ProviderConfiguration:FileSystemRepositoryConfiguration"));
			services.AddTransient<IStatusRepository, FileStatusRepository>();
		}
	}


	private const string CONTENTSTATUS_FILENAME = "StatusEntries.csv";
	private readonly IFileStatusRepositoryConfiguration _configuration;


	public FileStatusRepository(IOptions<FileSystemRepositoryConfiguration> configurationAccessor)
	{
		_configuration = configurationAccessor.Value;
	}


	public async Task<IEnumerable<IContentStatusEntity>> GetStatusEntriesAsync(string siteId, string contentId)
	{
		var filePath = Path.Combine(_configuration.StatusRootPath, siteId, contentId, CONTENTSTATUS_FILENAME);

		if (!File.Exists(filePath))
		{
			return Enumerable.Empty<FileContentStatusEntity>();
		}

		using var sr = new StreamReader(filePath);
		var everything = await sr.ReadToEndAsync();
		return everything.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
						 .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
						 .Select(line => FileContentStatusEntity.FromString(line));
	}


	public async Task AddStatusEntryAsync(string siteId, IContentStatusEntity statusEntry)
	{
		var folderPath = Path.Combine(_configuration.StatusRootPath, siteId, statusEntry.ContentId);
		Directory.CreateDirectory(folderPath);

		var filePath = Path.Combine(folderPath, CONTENTSTATUS_FILENAME);
		var writeHeader = !File.Exists(filePath);
		using var sw = new StreamWriter(filePath, true);
		if (writeHeader)
		{
			await sw.WriteLineAsync("#Timestamp;ContentId;VersionId;StartTime;Status;Responsible");
		}

		await sw.WriteLineAsync(statusEntry.ToString());
	}

	public IContentStatusEntity CreateContentStatusEntity(string contentId, DateTimeOffset timestamp, DateTimeOffset startTime, string versionId, string status, string responsible)
	{
		return new FileContentStatusEntity
		{
			ContentId = contentId,
			Timestamp = timestamp,
			StartTime = startTime,
			VersionId = versionId,
			Status = status,
			Responsible = responsible
		};
	}
}