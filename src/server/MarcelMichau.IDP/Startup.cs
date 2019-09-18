using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace MarcelMichau.IDP
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients(
                    _configuration.GetValue<string>("FAKE_SURVEY_GENERATOR_UI_CLIENT_URL"),
                    _configuration.GetValue<string>("FAKE_SURVEY_GENERATOR_SWAGGER_CLIENT_URL")))
                .AddTestUsers(Config.GetUsers());

            if (_environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("Key material not configured.");
            }

            services.AddControllersWithViews();

            if (_configuration.GetValue<bool>("IS_CLUSTER_ENV"))
            {
                services.AddDataProtection(opts => { opts.ApplicationDiscriminator = "identityserver"; })
                    .PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(new ConfigurationOptions
                    {
                        EndPoints = {_configuration.GetValue<string>("REDIS_URL")},
                        Password = _configuration.GetValue<string>("REDIS_PASSWORD"),
                        Ssl = _configuration.GetValue<bool>("REDIS_SSL"),
                        DefaultDatabase = _configuration.GetValue<int>("REDIS_DEFAULT_DATABASE")
                    }), "DataProtection-Keys");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase(new PathString("/identity"));

            //app.Use((context, next) =>
            //{
            //    if (context.Request.Path.Value.Contains("/identity"))
            //        return next();

            //    context.Request.PathBase = new PathString("/identity");
            //    return next();
            //});

            app.UseAuthorization();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
        }
    }
}