using FakeSurveyGenerator.Infrastructure;
using FakeSurveyGenerator.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FakeSurveyGenerator.EF.Design
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SurveyContext>
    {
        public SurveyContext CreateDbContext(string[] args)
        {
            const string connectionString =
                "Server=localhost;Database=ef-core-design;Trusted_Connection=True;MultipleActiveResultSets=true";

            var builder = new DbContextOptionsBuilder<SurveyContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly("FakeSurveyGenerator.Infrastructure"));
            return new SurveyContext(builder.Options);
        }
    }
}
