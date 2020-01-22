using System;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.SeedWork;
using FakeSurveyGenerator.Infrastructure.EntityConfigurations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Infrastructure
{
    public class SurveyContext : DbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;

        public SurveyContext(DbContextOptions options) : base(options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
        }

        public SurveyContext(DbContextOptions options, IMediator mediator) : base(options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public const string DefaultSchema = "Survey";

        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyOption> SurveyOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SurveyEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new SurveyOptionEntityTypeConfiguration());
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            await _mediator.DispatchDomainEventsAsync(this);

            await base.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
