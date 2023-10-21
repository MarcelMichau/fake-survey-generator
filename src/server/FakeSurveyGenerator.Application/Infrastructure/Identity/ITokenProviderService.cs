namespace FakeSurveyGenerator.Application.Infrastructure.Identity;

public interface ITokenProviderService
{
    string? GetToken();
}