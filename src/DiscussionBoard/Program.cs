
using System;
using System.IO;
using DiscussionBoard.Authorization;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

namespace DiscussionBoard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string logPath = Path.Combine(
                builder.Environment.ContentRootPath,
                "Logs",
                "discussionboard-.json");

            builder.Services.AddSerilog((services, configuration) =>
                configuration
                    .WriteTo.Console()
                    .WriteTo.File(
                        new RenderedCompactJsonFormatter(),
                        logPath,
                        rollingInterval: RollingInterval.Day,
                        shared: true)
            );

            var connectionString =
                builder.Configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlite(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Enable ASP.NET Core Identity roles.
            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                    options.SignIn.RequireConfirmedAccount =
                        !builder.Environment.IsDevelopment())
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Register the shared owner-or-admin authorization policy.
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    ApplicationAuthorizationPolicies.IsOwnerOrAdmin,
                    policy =>
                    {
                        policy.RequireAuthenticatedUser();
                        policy.AddRequirements(
                            new IsOwnerOrAdminRequirement());
                    });
            });

            // Register the authorization handler.
            builder.Services.AddScoped<
                Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
                IsOwnerOrAdminHandler>();

            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Forum");
                options.Conventions.AuthorizeFolder("/Forums");
                options.Conventions.AuthorizeFolder("/Discussion");
                options.Conventions.AuthorizeFolder("/Discussions");
                options.Conventions.AuthorizePage("/MyProfile");

                options.Conventions.AllowAnonymousToPage("/Index");
                options.Conventions.AllowAnonymousToPage("/Privacy");
            });


            builder.Services.AddSingleton<ILogReader, JsonLogReader>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<ForumService>();
            builder.Services.AddScoped<DiscussionThreadService>();
            builder.Services.AddScoped<IDiscussionThreadService>(
                services => services.GetRequiredService<DiscussionThreadService>());

            builder.Services.AddScoped<PostService>();
            builder.Services.AddScoped<IPostService>(
                services => services.GetRequiredService<PostService>());


            builder.Services.AddScoped<
                IApplicationAuthorizationService,
                ApplicationAuthorizationService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapRazorPages()
                .WithStaticAssets();

            app.Run();
        }
    }
}



