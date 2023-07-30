

namespace Metamory.Api.Providers.FileSystem
{
	public class FileSystemRepositoryConfiguration : IFileContentRepositoryConfiguration, IFileStatusRepositoryConfiguration
	{
		public string RootPath { get; set; }

        string IFileContentRepositoryConfiguration.ContentRootPath => RootPath;

		string IFileStatusRepositoryConfiguration.StatusRootPath => RootPath;
	}
}