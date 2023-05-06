namespace FakeSurveyGenerator.Infrastructure.Identity;

public interface ITokenProviderService
{
    string? GetToken();
}