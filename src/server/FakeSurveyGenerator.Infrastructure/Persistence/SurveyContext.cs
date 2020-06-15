using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Persistence;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using FakeSurveyGenerator.Domain.SeedWork;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Infrastructure.Persistence
{
    public sealed class SurveyContext : DbContext, ISurveyContext
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public const string DefaultSchema = "Survey";

        public DbSet<User> Users { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyOption> SurveyOptions { get; set; }

        public SurveyContext(DbContextOptions options, IUserService userService, ILogger<SurveyContext> logger) : base(options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public SurveyContext(DbContextOptions options, IMediator mediator, IUserService userService, ILogger<SurveyContext> logger) : base(options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                SetAuditProperties();

                await _mediator.DispatchDomainEventsAsync(this, cancellationToken);

                SetAuditProperties(); // This needs to be run again to set the audit properties of any entities that were added/modified in any Domain Event Handlers after dispatch

                var recordsAffected = await base.SaveChangesAsync(cancellationToken);

                return recordsAffected;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred during a database update");
                return 0;
            }
        }

        private void SetAuditProperties()
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _userService.GetUserIdentity();
                        entry.Entity.CreatedOn = DateTimeOffset.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedBy = _userService.GetUserIdentity();
                        entry.Entity.ModifiedOn = DateTimeOffset.UtcNow;
                        break;
                    case EntityState.Detached:
                        break;
                    case EntityState.Unchanged:
                        break;
                    case EntityState.Deleted:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
