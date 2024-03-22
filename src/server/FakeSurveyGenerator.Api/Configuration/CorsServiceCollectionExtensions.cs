namespace FakeSurveyGenerator.Api.Configuration;

internal static class CorsConfigurationExtensions
{
    public static IHostApplicationBuilder AddCorsConfiguration(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsDevelopment())
            return builder; // CORS is only used for local development when running in the Aspire Host

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(configurePolicy =>
            {
                configurePolicy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        
        return builder;
    }
}
