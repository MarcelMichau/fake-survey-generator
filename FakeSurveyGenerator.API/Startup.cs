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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            var connectionString = _configuration.GetConnectionString(nameof(SurveyContext));

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddMediatR(typeof(Startup).GetTypeInfo().Assembly);

            services.AddScoped<ISurveyQueries>(s => new SurveyQueries(connectionString));
            services.AddScoped<ISurveyRepository, SurveyRepository>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseHealthChecks("/health");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fake Survey Generator API V1");
            });

            UpdateDatabase(app, env);
        }

        private static void UpdateDatabase(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (!env.IsDevelopment()) // Only migrate database on startup when running in Development environment
                return;

            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<SurveyContext>())
                {
                    if (context.Database.IsSqlServer()) // Do not migrate database when running integration tests with in-memory database
                        context.Database.Migrate();
                }
            }
        }
    }
}
