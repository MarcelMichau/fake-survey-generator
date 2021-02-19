using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    public class AzureSqlAuthenticationProvider : SqlAuthenticationProvider
    {
        public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
        {
            var tokenRequestContext = new TokenRequestContext(new[] { parameters.Resource });

            var credential = new DefaultAzureCredential();

            var tokenResponse = await credential
                .GetTokenAsync(tokenRequestContext)
                .ConfigureAwait(false);

            return new SqlAuthenticationToken(tokenResponse.Token, tokenResponse.ExpiresOn);
        }

        public override bool IsSupported(SqlAuthenticationMethod authenticationMethod)
        {
            return authenticationMethod == SqlAuthenticationMethod.ActiveDirectoryIntegrated;
        }
    }
}