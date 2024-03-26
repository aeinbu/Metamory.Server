using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Metamory.Api;
using Metamory.Api.Providers.AzureStorage;
using Metamory.Api.Providers.FileSystem;
using Metamory.Api.Repositories;
using Metamory.WebApi.Authorization;
using Metamory.WebApi.Instrumentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http.Features;
using OpenTelemetry.Metrics;


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
        builder.RegisterInstrumentation();

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
            options.Audience = "https://metamory.server/";
        });

        // var noAuthMode = builder.Configuration.GetValue<bool>("NoAuth");
        services.AddAuthorization(options =>
        {
            // options.AddPolicy(AuthPolicies.SystemAdminRole, policy => policy.RequireRole("SystemAdmin"));
            // options.AddPolicy(AuthPolicies.SiteAdmin, policy => policy.RequireRole("SiteAdmin"));

            options.AddPolicy(Policies.RequireChangeStatusPermission, policy => policy.AddRequirements(new AccessControlRequirement(Permission.ChangeStatus)));
            options.AddPolicy(Policies.RequireCreateOrModifyPermission, policy => policy.AddRequirements(new AccessControlRequirement(Permission.CreateOrModify)));
            options.AddPolicy(Policies.RequireReviewPermission, policy => policy.AddRequirements(new AccessControlRequirement(Permission.Review)));
        });
        builder.Services.AddSingleton<IAuthorizationHandler, AccessControlRequirementHandler>();

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

        ConfigureProvider(configuration.GetSection("Providers:ContentRepository"), builder.Configuration, services);
        ConfigureProvider(configuration.GetSection("Providers:StatusRepository"), builder.Configuration, services);

        services.Configure<AccessControlConfiguration>(configuration.GetSection("AccessControl"));

        services.AddTransient<ContentManagementService>();
        services.AddTransient<VersioningService>();
    }

    private static void ConfigureProvider(IConfigurationSection providerConfiguration, ConfigurationManager configuration, IServiceCollection services)
    {
        var assemblyFile = providerConfiguration.GetValue<string>("AssemblyFile");
        var typeName = providerConfiguration.GetValue<string>("TypeName") + "+Configurator";

        object[] constructorParams = [configuration, services];
        Activator.CreateInstanceFrom(assemblyFile, typeName, false, BindingFlags.CreateInstance, null, constructorParams, System.Globalization.CultureInfo.CurrentCulture, (object[])null);
    }

    private static void RegisterInstrumentation(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder.AddPrometheusExporter();

                builder.AddAspNetCoreInstrumentation();
                builder.AddRuntimeInstrumentation();
                builder.AddMeter(
                    // "Microsoft.AspNetCore.Hosting",
                    // "Microsoft.AspNetCore.Server.Kestrel",
                    PublicationMetrics.MeterName
                );

                // builder.AddView("http.server.request.duration",
                //     new ExplicitBucketHistogramConfiguration
                //     {
                //         Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                //        0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
                //     });
                // builder.AddView("publication.contentServed.timeTaken",
                //     new ExplicitBucketHistogramConfiguration
                //     {
                //         Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                //        0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
                //     });
            });

        services.AddSingleton<PublicationMetrics>();
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

        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        app.UseAuthentication();

        // app.UseCors();

        app.MapControllers();
    }
}
