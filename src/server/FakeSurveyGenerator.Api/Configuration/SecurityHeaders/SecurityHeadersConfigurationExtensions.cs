namespace FakeSurveyGenerator.Api.Configuration.SecurityHeaders;

internal static class SecurityHeadersConfigurationExtensions
{
    extension(WebApplication app)
    {
        public IApplicationBuilder UseSecurityHeadersConfiguration()
        {
            var policyCollection = new HeaderPolicyCollection()
                .AddCrossOriginOpenerPolicy(x => x.UnsafeNone()); // Required for OpenAPI Docs Sign-In with PKCE to work in browsers

            app.UseSecurityHeaders(policyCollection);

            return app;
        }
    }
}
