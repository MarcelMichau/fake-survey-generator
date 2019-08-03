using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using FakeSurveyGenerator.API.Application.Queries;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Infrastructure;
using FakeSurveyGenerator.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace FakeSurveyGenerator.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            var connectionString = _configuration.GetConnectionString(nameof(SurveyContext));

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);

            services.AddScoped<ISurveyQueries>(s => new SurveyQueries(connectionString));
            services.AddScoped<ISurveyRepository, SurveyRepository>();

            services.AddDbContext<SurveyContext>
                (options => options.UseSqlServer(connectionString, b => b.MigrationsAssembly("FakeSurveyGenerator.Infrastructure")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Fake Survey Generator API",
                    Version = "v1",
                    Description = "This is an API. That generates surveys. Fake ones. For fun. That is all.",
                    Contact = new OpenApiContact
                    {
                        Name = "Marcel Michau",
                        Email = string.Empty,
                        Url = new Uri("https://marcelmichau.dev"),
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddHealthChecks()
                .AddDbContextCheck<SurveyContext>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fake Survey Generator API V1");
            });

            UpdateDatabase(app, env);
        }

        private static void UpdateDatabase(IApplicationBuilder app, IHostEnvironment env)
        {
            if (!env.IsDevelopment()) // Only migrate database on startup when running in Development environment
                return;

            using var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();

            using var context = serviceScope.ServiceProvider.GetService<SurveyContext>();
            if (context.Database.IsSqlServer()) // Do not migrate database when running integration tests with in-memory database
                context.Database.Migrate();
        }
    }
}
