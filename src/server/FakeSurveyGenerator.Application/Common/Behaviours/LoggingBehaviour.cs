using System;
using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Identity;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace FakeSurveyGenerator.Application.Common.Behaviours
{
    public sealed class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
    {
        private readonly ILogger _logger;
        private readonly IUser _user;

        public LoggingBehaviour(ILogger<TRequest> logger, IUser user)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            var name = typeof(TRequest).Name;

            _logger.LogInformation("Fake Survey Generator Request: {Name} {@Request} {@CurrentUserIdentity}",
                name, request, new CurrentUserIdentity(_user.Id, _user.DisplayName, _user.EmailAddress));

            return Task.CompletedTask;
        }

        internal class CurrentUserIdentity
        {
            public string Id { get; }
            public string DisplayName { get; }
            public string EmailAddress { get; }

            public CurrentUserIdentity(string id, string displayName, string emailAddress)
            {
                Id = id;
                DisplayName = displayName;
                EmailAddress = emailAddress;
            }
        }
    }
}