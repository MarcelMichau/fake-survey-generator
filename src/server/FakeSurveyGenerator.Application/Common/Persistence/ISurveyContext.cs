using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FakeSurveyGenerator.Application.Common.Interfaces
{
    public interface ISurveyContext
    {
        DbSet<Survey> Surveys { get; set; }
        DbSet<SurveyOption> SurveyOptions { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry<TEntity> Entry<TEntity>([NotNull] TEntity entity) where TEntity : class;
    }
}
