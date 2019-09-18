using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                        EndPoints = { _configuration.GetValue<string>("REDIS_URL") },
                        Password = _configuration.GetValue<string>("REDIS_PASSWORD"),
                        Ssl = _configuration.GetValue<bool>("REDIS_SSL"),
                        DefaultDatabase = _configuration.GetValue<int>("REDIS_DEFAULT_DATABASE")
                    }), "DataProtection-Keys");
            }

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                foreach (var network in GetNetworks(NetworkInterfaceType.Ethernet))
                {
                    options.KnownNetworks.Add(network);
                }
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.Use((context, next) =>
            //{
            //    var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();

            //    logger.LogInformation("------- Request Headers START ------");

            //    foreach (var (key, value) in context.Request.Headers)
            //    {
            //        logger.LogInformation($"Header Name: {key}, Header Value: {value}");
            //    }

            //    logger.LogInformation("------- Request Headers END ------");

            //    return next();
            //});

            app.UseForwardedHeaders();

            //app.Use((context, next) =>
            //{
            //    var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();

            //    logger.LogInformation("------- Request Headers After UseForwardedHeaders START ------");

            //    foreach (var (key, value) in context.Request.Headers)
            //    {
            //        logger.LogInformation($"Header Name: {key}, Header Value: {value}");
            //    }

            //    logger.LogInformation("------- Request Headers After UseForwardedHeaders END ------");

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

        private static IEnumerable<IPNetwork> GetNetworks(NetworkInterfaceType type)
        {

            foreach (var item in NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.NetworkInterfaceType == type && n.OperationalStatus == OperationalStatus.Up)  // get all operational networks of a given type
                .Select(n => n.GetIPProperties())   // get the IPs
                .Where(n => n.GatewayAddresses.Any())) // where the IPs have a gateway defined
            {
                var ipInfo = item.UnicastAddresses.FirstOrDefault(i => i.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork); // get the first cluster-facing IP address
                if (ipInfo == null) { continue; }

                // convert the mask to bits
                var maskBytes = ipInfo.IPv4Mask.GetAddressBytes();
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(maskBytes);
                }
                var maskBits = new BitArray(maskBytes);

                // count the number of "true" bits to get the CIDR mask
                var cidrMask = maskBits.Cast<bool>().Count(b => b);

                // convert my application's ip address to bits
                var ipBytes = ipInfo.Address.GetAddressBytes();
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(maskBytes);
                }
                var ipBits = new BitArray(ipBytes);

                // and the bits with the mask to get the start of the range
                var maskedBits = ipBits.And(maskBits);

                // Convert the masked IP back into an IP address
                var maskedIpBytes = new byte[4];
                maskedBits.CopyTo(maskedIpBytes, 0);
                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(maskedIpBytes);
                }
                var rangeStartIp = new IPAddress(maskedIpBytes);

                // return the start IP and CIDR mask
                yield return new IPNetwork(rangeStartIp, cidrMask);
            }
        }
    }
}