namespace FakeSurveyGenerator.Application.Common.Identity
{
    public interface IUser
    {
        string Id { get; }
        string DisplayName { get; }
        string EmailAddress { get; }
    }
}
