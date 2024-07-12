using CSharpFunctionalExtensions;

namespace FakeSurveyGenerator.Application.Shared.Errors;

public sealed class Error : ValueObject
{
    internal Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }
    public string Message { get; }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Code;
    }
}