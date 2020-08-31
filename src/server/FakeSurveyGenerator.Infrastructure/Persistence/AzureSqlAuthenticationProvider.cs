using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    public class AzureSqlAuthenticationProvider : SqlAuthenticationProvider
    {
        public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
        {
            var tokenProvider = new AzureServiceTokenProvider();
            var appAuthenticationResult = await tokenProvider
                .GetAuthenticationResultAsync(parameters.Resource)
                .ConfigureAwait(false);

            return new SqlAuthenticationToken(appAuthenticationResult.AccessToken, appAuthenticationResult.ExpiresOn);
        }

        public override bool IsSupported(SqlAuthenticationMethod authenticationMethod)
        {
            return authenticationMethod == SqlAuthenticationMethod.ActiveDirectoryIntegrated;
        }
    }
}