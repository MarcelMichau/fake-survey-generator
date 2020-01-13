using System.Threading;
using System.Threading.Tasks;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Notifications.Models;
using MediatR;

namespace FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey
{
    public class SurveyCreated : INotification
    {
        public int SurveyId { get; private set; }

        public SurveyCreated(int surveyId)
        {
            SurveyId = surveyId;
        }

        public class SurveyCreatedHandler : INotificationHandler<SurveyCreated>
        {
            private readonly INotificationService _notification;

            public SurveyCreatedHandler(INotificationService notification)
            {
                _notification = notification;
            }

            public async Task Handle(SurveyCreated notification, CancellationToken cancellationToken)
            {
                await _notification.SendAsync(new MessageDto {
                    Body = $"Survey with ID: {notification.SurveyId} created",
                    From = "System",
                    Subject = "New Survey Created",
                    To = "Whom It May Concern"
                });
            }
        }
    }
}
