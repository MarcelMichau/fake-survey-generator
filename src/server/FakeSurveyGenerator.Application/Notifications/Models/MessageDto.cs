namespace FakeSurveyGenerator.Application.Notifications.Models
{
    public sealed class MessageDto
    {
        public string From { get; init; }
        public string To { get; init; }
        public string Subject { get; init; }
        public string Body { get; init; }
    }
}
