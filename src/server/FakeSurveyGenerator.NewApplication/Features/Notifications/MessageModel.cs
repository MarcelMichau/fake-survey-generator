namespace FakeSurveyGenerator.Application.Features.Notifications;

public sealed record MessageModel(string From, string To, string Subject, string Body);