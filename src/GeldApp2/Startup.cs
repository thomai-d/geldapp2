using Abstrakt.AspNetCore;
using Autofac;
using GeldApp2.Application.Commands.Users;
using GeldApp2.Application.Logging;
using GeldApp2.Application.Services;
using GeldApp2.Configuration;
using GeldApp2.Database;
using GeldApp2.Database.Abstractions;
using GeldApp2.Middleware;
using GeldApp2.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Serilog;
using Serilog.Events;
using System;
using System.Reflection;
using System.Text;

namespace GeldApp2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            this.ConfigureLogging();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMediatR(typeof(ChangePasswordCommand));

            // Configure stuff.
            services.Configure<IpBlockerSettings>(Configuration.GetSection("IpBlockerSettings"));

            services.AddHostedService<PerformanceMonitorService>();

            services.AddScoped<ISqlQuery, SqlQuery>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IDatabaseMigrator, DatabaseMigrator>();
            services.AddSingleton<IIpBlockerService, IpBlockerService>();

            this.InjectPreloadedObjects(services);

            if (string.IsNullOrEmpty(Configuration["MysqlConnectionString"])
                && !Runtime.IsIntegrationTesting)
            {
                throw new ApplicationException("MysqlConnectionString setting is empty!");
            }

            services.AddHttpContextAccessor();
            services.AddDbContext<GeldAppContext>(opt => opt.UseMySql(
                                Configuration["MysqlConnectionString"], mopt =>
                                mopt.ServerVersion(new Version(8, 1, 13), ServerType.MySql)));

            this.ConfigureAuthentication(services);

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<AutofacModule>();
        }

        public void Configure(
            IServiceScopeFactory scopeFactory,
            IDatabaseMigrator dbMigrator,
            IApplicationBuilder app,
            ILogger<Startup> log,
            IHostingEnvironment env)
        {
            var version = typeof(Startup).Assembly
                            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                            .InformationalVersion;
            log.LogInformation(Events.Startup, "Starting {ServerVersion}", version);

            using (var scope = scopeFactory.CreateScope())
            {
                dbMigrator.Migrate();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseMiddleware<IpBlockerMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMiddleware<UserPreloadMiddleware>();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            if (!Runtime.IsIntegrationTesting)
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "ClientApp";

                    if (env.IsDevelopment())
                    {
                        spa.UseAngularCliServer(npmScript: "start");
                    }
                });
            }
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            var authSettings = Configuration.GetSection("AuthenticationSettings");
            services.Configure<AuthenticationSettings>(authSettings);

            var secret = authSettings["JwtTokenSecret"];
            if (string.IsNullOrEmpty(secret))
                throw new ApplicationException("You should really really think about setting a JwtTokenSecret...");

            var jwtKey = Encoding.ASCII.GetBytes(secret);
            var auth = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

            auth.AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = true;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                };
            });
        }

        private void InjectPreloadedObjects(IServiceCollection services)
        {
            services.AddScoped<User>(serviceProvider =>
            {
                var accessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

                if (accessor.HttpContext.Items.TryGetValue("currentUser", out var user))
                {
                    return (User)user;
                }

                return default;
            });
        }

        private void ConfigureLogging()
        {
            if (Runtime.IsIntegrationTesting)
                return;

            var loggerConfig = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(this.Configuration)
            .Enrich.WithProperty("Server", Environment.MachineName)
            .WriteTo.ColoredConsole(
                LogEventLevel.Debug,
                "{NewLine}{Timestamp:HH:mm:ss} [{Level}] ({CorrelationToken}) {SourceContext}: {Message}{NewLine}{Exception}");

            // Enable SEQ logging.
            var seqServer = Configuration["SeqTarget"];
            if (!string.IsNullOrEmpty(seqServer))
            {
                loggerConfig.WriteTo.Seq(seqServer, queueSizeLimit: 1000, restrictedToMinimumLevel: LogEventLevel.Information);
            }

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}
