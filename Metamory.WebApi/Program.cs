using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Metamory.Api;
using Metamory.Api.Providers.AzureStorage;
using Metamory.Api.Providers.FileSystem;
using Metamory.Api.Repositories;
using Metamory.WebApi.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Metamory.WebApi;

internal static class Program
{
    public const string claimNamespace = "https://metamory.server";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.RegisterEndpoints();
        builder.RegisterDataProtection();
        builder.RegisterFrameworkServices();
        builder.RegisterMetamoryServices();

        var app = builder.Build();
        app.ConfigureApp();

        app.Run();
    }


    private static void RegisterEndpoints(this WebApplicationBuilder builder)
    {
        builder.WebHost.UseKestrel(options =>
        {
            var noAuthMode = builder.Configuration.GetValue<bool>("NoAuth");
            if (noAuthMode)
            {
                options.Listen(IPAddress.Any, 5000);
            }

            var cert_file = "/https/cert.pfx";
            var cert_password = builder.Configuration.GetValue<string>("CertificatePassword");
            if (!string.IsNullOrEmpty(cert_password) && File.Exists(cert_file))
            {
                options.Listen(IPAddress.Any, 5001, listenOptions => { listenOptions.UseHttps(cert_file, cert_password); });
            }
        });
    }

    private static void RegisterDataProtection(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var dataProtection = services.AddDataProtection();

        // // TODO: Remove warnings for data-encryption
        // dataProtection.UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
        // {
        //     EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        //     ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
        // });

        // //TODO: allow to  configure using other storage mechanisms (Azure Blobs? Azure Key Vault?)
        // if (Directory.Exists("/data-protection-keys"))
        // {
        //     dataProtection.PersistKeysToFileSystem(new DirectoryInfo(@"/data-protection-keys"));
        // }
    }

    private static void RegisterFrameworkServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddHttpContextAccessor();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            // options.Audience = "https://metamory.server2/";
            // options.TokenValidationParameters = new TokenValidationParameters
            // {
            //     RoleClaimType = $"{claimNamespace}/roles",
            // };
        });

        services.AddAuthorization(options =>
        {
            // options.AddPolicy(AuthPolicies.SystemAdminRole, policy => policy.RequireRole("SystemAdmin"));
            // options.AddPolicy(AuthPolicies.SiteAdmin, policy => policy.RequireRole("SiteAdmin"));

            options.AddPolicy(Policies.RequireChangeStatusPermission, policy => policy.AddRequirements(new AccessControlRequirement(Permission.ChangeStatus)));
            options.AddPolicy(Policies.RequireCreateOrModifyPermission, policy => policy.AddRequirements(new AccessControlRequirement(Permission.CreateOrModify)));
            options.AddPolicy(Policies.RequireReviewPermission, policy => policy.AddRequirements(new AccessControlRequirement(Permission.Review)));
        });

        var noAuthMode = builder.Configuration.GetValue<bool>("NoAuth");
        var path = noAuthMode ? "authorization-noAuth.config.xml" : "/authorization/accessControl.config.xml";
        builder.Services.AddSingleton<IAuthorizationHandler>(sp => new AccessControlRequirementHandler(path));

        services.AddTransient<ClaimsPrincipal>(provider => provider.GetService<IHttpContextAccessor>().HttpContext.User);

        services.AddControllers();

        services.AddOptions();
    }

    private static void RegisterMetamoryServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var services = builder.Services;

        // new Metamory.Api.Providers.AzureStorage.AzureBlobContentRepository.Configurator(builder.Configuration, services);
        // new Metamory.Api.Providers.AzureStorage.AzureTableStatusRepository.Configurator(builder.Configuration, services);

        // new Metamory.Api.Providers.FileSystem.FileContentRepository.Configurator(builder.Configuration, services);
        // new Metamory.Api.Providers.FileSystem.FileStatusRepository.Configurator(builder.Configuration, services);

        var metamoryApiConfiguration = configuration.GetSection("Metamory.Api");
        ConfigureProvider(metamoryApiConfiguration.GetSection("Providers:ContentRepository"), metamoryApiConfiguration, services);
        ConfigureProvider(metamoryApiConfiguration.GetSection("Providers:StatusRepository"), metamoryApiConfiguration, services);

        // THIS is the one for content. How to create an additional for authorization/permissions
        services.AddTransient<VersioningService>();
        services.AddTransient<ContentManagementService>(svc => new ContentManagementService(
            svc.GetService<IStatusRepository>(),
            svc.GetService<IContentRepository>(),
            svc.GetService<VersioningService>(),
            svc.GetService<ICanonicalizeService>()
            ));
        // services.AddTransient<ContentManagementService>();
    }

    private static void ConfigureProvider(IConfigurationSection providerConfiguration, IConfigurationSection configuration, IServiceCollection services)
    {
        var assemblyFile = providerConfiguration.GetValue<string>("AssemblyFile");
        var typeName = providerConfiguration.GetValue<string>("TypeName") + "+Configurator";

        object[] constructorParams = [configuration, services];
        Activator.CreateInstanceFrom(assemblyFile, typeName, false, BindingFlags.CreateInstance, null, constructorParams, System.Globalization.CultureInfo.CurrentCulture, (object[])null);
    }

    private static void ConfigureApp(this WebApplication app)
    {
        var noAuthMode = app.Configuration.GetValue<bool>("NoAuth");
        if (noAuthMode)
        {
            app.Logger.LogWarning("Metamory is running in no auth mode.");
        }

        if (app.Environment.IsDevelopment() || noAuthMode)
        {
            app.UseDeveloperExceptionPage();
        }

        if (!app.Environment.IsDevelopment() && !noAuthMode)
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        // app.UseCors();

        app.MapControllers();
    }
}
