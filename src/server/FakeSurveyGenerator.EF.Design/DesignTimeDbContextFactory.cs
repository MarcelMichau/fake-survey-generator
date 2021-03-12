using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Infrastructure.Persistence;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace FakeSurveyGenerator.EF.Design
{
    [UsedImplicitly]
    internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SurveyContext>
    {
        public SurveyContext CreateDbContext(string[] args)
        {
            const string connectionString =
                "Server=localhost;Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>";

            var builder = new DbContextOptionsBuilder<SurveyContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(SurveyContext).Assembly.FullName));
            return new SurveyContext(builder.Options, Substitute.For<IUserService>(), new NullLogger<SurveyContext>());
        }
    }
}
