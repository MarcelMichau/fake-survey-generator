using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FakeSurveyGenerator.Application.Common.Persistence;

public interface ISurveyContext
{
    DbSet<User> Users { get; }
    DbSet<Survey> Surveys { get; }
    DbSet<SurveyOption> SurveyOptions { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity) where TEntity : class;
}