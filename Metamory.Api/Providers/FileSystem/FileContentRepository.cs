using Metamory.Api.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Metamory.Api.Providers.FileSystem;


public interface IFileContentRepositoryConfiguration
{
	string ContentRootPath { get; }
}


public class FileContentRepository : IContentRepository
{
	public class Configurator
	{
		public Configurator(IConfigurationSection configuration, IServiceCollection services, string key)
		{
			services.Configure<FileSystemRepositoryConfiguration>(configuration.GetSection("ProviderConfiguration:FileSystemRepositoryConfiguration"));
			services.AddKeyedTransient<IContentRepository, FileContentRepository>(key);
			services.AddKeyedTransient<ICanonicalizeService, FileCanonicalizeService>(key);
		}
	}


	private const string METADATA_EXTENSION = "metadata.csv";

	private readonly IFileContentRepositoryConfiguration _configuration;


	public FileContentRepository(IOptions<FileSystemRepositoryConfiguration> configurationAccessor)
	{
		_configuration = configurationAccessor.Value;
		if (_configuration.ContentRootPath == null)
		{
			throw new Exception("Missing or invalid configuration for FileSystemRepositoryConfiguration");
		}
	}


	public async Task<string> DownloadContentToStreamAsync(string siteId, string contentId, string versionId, Stream memoryStream)
	{
		var contentFilePath = Path.Combine(_configuration.ContentRootPath, siteId, contentId, versionId);
		var metadataFilePath = Path.ChangeExtension(contentFilePath, METADATA_EXTENSION);

		using var sr = new StreamReader(metadataFilePath);
		var line = await sr.ReadLineAsync();
		var metadata = ContentMetadataEntity.FromString(line);

		using var fileStream = File.OpenRead(contentFilePath);
		await fileStream.CopyToAsync(memoryStream);

		return metadata.ContentType;
	}


	public async Task AddContentAsync(string siteId, string contentId, string versionId, Stream contentStream, string contentType, DateTimeOffset now, string previousVersionId, string author, string label)
	{
		var folderPath = Path.Combine(_configuration.ContentRootPath, siteId, contentId);
		Directory.CreateDirectory(folderPath);

		var contentFilePath = Path.Combine(folderPath, versionId);
		var metadataFilePath = Path.ChangeExtension(contentFilePath, METADATA_EXTENSION);

		using var sw = new StreamWriter(metadataFilePath, false);
		var metadata = new ContentMetadataEntity
		{
			Timestamp = now,
			VersionId = versionId,
			PreviousVersionId = previousVersionId,
			Label = label,
			ContentType = contentType,
			Author = author
		};
		await sw.WriteLineAsync(metadata.ToString());

		contentStream.Seek(0, SeekOrigin.Begin);
		using var fileStream = File.OpenWrite(contentFilePath);
		await contentStream.CopyToAsync(fileStream);
	}


	public async Task<IEnumerable<ContentMetadataEntity>> GetVersionsAsync(string siteId, string contentId)
	{
		var folderPath = Path.Combine(_configuration.ContentRootPath, siteId, contentId);

		if (!Directory.Exists(folderPath))
		{
			return Enumerable.Empty<ContentMetadataEntity>();
		}

		var metadataTasks = Directory.GetFiles(folderPath, $"*.{METADATA_EXTENSION}")
									 .Select(async filename => await FromFile(filename))
									 .ToArray();

		return await Task.WhenAll(metadataTasks);
	}


	private static async Task<ContentMetadataEntity> FromFile(string path)
	{
		using var sr = new StreamReader(path);
		var s = await sr.ReadLineAsync();
		return ContentMetadataEntity.FromString(s);
	}

    public Task<IEnumerable<string>> ListContentAsync(string siteId)
    {
		var folderPath = Path.Combine(_configuration.ContentRootPath, siteId);
		if (!Directory.Exists(folderPath))
		{
			return Task.FromResult(Enumerable.Empty<string>());
		}

		var contentIds = Directory.GetDirectories(folderPath).Select(Path.GetFileName);
		return Task.FromResult(contentIds);
	}
}