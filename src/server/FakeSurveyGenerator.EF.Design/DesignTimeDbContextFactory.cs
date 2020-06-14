using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace FakeSurveyGenerator.EF.Design
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SurveyContext>
    {
        public SurveyContext CreateDbContext(string[] args)
        {
            const string connectionString =
                "Server=localhost;Database=FakeSurveyGenerator;user id=SA;pwd=<YourStrong!Passw0rd>";

            var builder = new DbContextOptionsBuilder<SurveyContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(SurveyContext).Assembly.FullName));
            return new SurveyContext(builder.Options, new Mock<IUserService>().Object, new NullLogger<SurveyContext>());
        }
    }
}
