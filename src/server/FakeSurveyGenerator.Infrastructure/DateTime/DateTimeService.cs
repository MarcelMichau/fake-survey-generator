using FakeSurveyGenerator.Application.Common.DateTime;

namespace FakeSurveyGenerator.Infrastructure.DateTime;
public sealed class DateTimeService : IDateTime
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
