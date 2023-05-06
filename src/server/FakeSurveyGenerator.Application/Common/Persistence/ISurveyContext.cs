using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FakeSurveyGenerator.Application.Common.Persistence;

public interface ISurveyContext
{
    DbSet<User> Users { get; }
    DbSet<Survey> Surveys { get; }
    DbSet<SurveyOption> SurveyOptions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}