using System.Reflection;
using System.Security.Claims;
using Metamory.Api;
using Metamory.Api.Providers.AzureStorage;
using Metamory.Api.Providers.FileSystem;
using Metamory.Api.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace Metamory.WebApi;


internal static class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.RegisterFrameworkServices();
        builder.RegisterMetamoryServices();

        var app = builder.Build();
        app.ConfigureApp();

        app.Run();
    }


    private static void RegisterFrameworkServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();

        services.AddAuthentication().AddJwtBearer();
        services.AddAuthorization(options =>
        {
            // options.AddPolicy(AuthPolicies.SystemAdminRole, policy => policy.RequireRole("SystemAdmin"));
            // options.AddPolicy(AuthPolicies.SiteAdmin, policy => policy.RequireRole("SiteAdmin"));
            options.AddPolicy(AuthPolicies.EditorRole, policy => policy.RequireRole("editor"));
            options.AddPolicy(AuthPolicies.ContributorRole, policy => policy.RequireRole("editor", "contributor"));
            options.AddPolicy(AuthPolicies.ReviewerRole, policy => policy.RequireRole("editor", "contributor", "reviewer"));
            options.AddPolicy(AuthPolicies.SiteIdClaim, policy => policy.AddRequirements(new SiteIdRequirement()));
        });
        builder.Services.AddSingleton<IAuthorizationHandler, SiteIdRequirementHandler>();

        services.AddTransient<ClaimsPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

        services.AddControllers();

        services.AddOptions();
    }


    private static void RegisterMetamoryServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        // // services.Configure<AzureStorageRepositoryConfiguration>(builder.Configuration.GetSection("AzureStorageRepositoryConfiguration"));
        // services.Configure<FileSystemRepositoryConfiguration>(builder.Configuration.GetSection("FileSystemRepositoryConfiguration"));

        ConfigureProvider(configuration.GetSection("Providers:ContentRepository"), builder.Configuration, services);
        ConfigureProvider(configuration.GetSection("Providers:StatusRepository"), builder.Configuration, services);

        services.AddTransient<ContentManagementService>();
        services.AddTransient<VersioningService>();
    }

    private static void ConfigureProvider(IConfigurationSection providerConfiguration, ConfigurationManager configuration, IServiceCollection services)
    {
        var assemblyFile = providerConfiguration.GetValue<string>("AssemblyFile");
        var typeName = providerConfiguration.GetValue<string>("TypeName") + "+Configurator";

        object[] constructorParams = new object[] { configuration, services };
        Activator.CreateInstanceFrom(assemblyFile, typeName, false, BindingFlags.CreateInstance, null, constructorParams, System.Globalization.CultureInfo.CurrentCulture, (object[])null);
    }

    private static void ConfigureApp(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // app.UseCors();

        app.MapControllers();
    }
}

public class ArjansTest{
    public ArjansTest(ConfigurationManager configuration, IServiceCollection services)
    {
        services.Configure<AzureStorageRepositoryConfiguration>(configuration.GetSection("AzureStorageRepositoryConfiguration"));
        // services.Configure<FileSystemRepositoryConfiguration>(configuration.GetSection("FileSystemRepositoryConfiguration"));

        services.AddTransient<IStatusRepository, AzureTableStatusRepository>();
        services.AddTransient<IContentRepository, AzureBlobContentRepository>();
        // services.AddTransient<IStatusRepository, FileStatusRepository>();
        // services.AddTransient<IContentRepository, FileContentRepository>();
   }
}