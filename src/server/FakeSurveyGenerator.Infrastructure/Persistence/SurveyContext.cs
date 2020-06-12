using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Persistence;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    public sealed class SurveyContext : DbContext, ISurveyContext
    {
        private readonly IMediator _mediator;

        public const string DefaultSchema = "Survey";

        public DbSet<User> Users { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyOption> SurveyOptions { get; set; }

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _mediator.DispatchDomainEventsAsync(this);

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
