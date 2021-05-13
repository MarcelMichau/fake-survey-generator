namespace FakeSurveyGenerator.Application.Notifications.Models
{
    public sealed record MessageDto(string From, string To, string Subject, string Body);
}
