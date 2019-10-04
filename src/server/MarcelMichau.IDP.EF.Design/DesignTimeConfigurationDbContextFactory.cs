using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarcelMichau.IDP.EF.Design
{
    public class DesignTimeConfigurationDbContextFactory : IDesignTimeDbContextFactory<ConfigurationDbContext>
    {
        public ConfigurationDbContext CreateDbContext(string[] args)
        {
            const string connectionString =
                "Server=localhost;Database=ef-core-design;Trusted_Connection=True;MultipleActiveResultSets=true";

            var builder = new DbContextOptionsBuilder<ConfigurationDbContext>();
            builder.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name));
            return new ConfigurationDbContext(builder.Options, new ConfigurationStoreOptions());
        }
    }
}
