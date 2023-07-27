using System.Security.Claims;
using Metamory.Api;
using Metamory.Api.Providers.FileSystem;
using Metamory.Api.Repositories;
using Metamory.WebApi.Policies;
using Metamory.WebApi.Policies.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Metamory.WebApi
{


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
            var services = builder.Services;

            services.Configure<FileRepositoryConfiguration>(builder.Configuration.GetSection("FileRepositoryConfiguration"));

            services.AddTransient<ContentManagementService>();
            services.AddTransient<IAuthorizationPolicy, NoAuthorizationPolicy>();
            services.AddTransient<IStatusRepository, FileStatusRepository>();
            services.AddTransient<IContentRepository, FileContentRepository>();
            services.AddTransient<VersioningService>();
            services.AddTransient<CanonicalizeService>();
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
}