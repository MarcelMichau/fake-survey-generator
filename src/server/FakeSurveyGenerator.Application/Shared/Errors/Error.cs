using CSharpFunctionalExtensions;

namespace FakeSurveyGenerator.Application.Shared.Errors;

public class Error : ValueObject
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

public sealed class ValidationError : Error
{
    internal ValidationError(IDictionary<string, string[]> errors)
        : base("validation.failed", "One or more validation failures have occurred.")
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}