namespace FakeSurveyGenerator.Application.Shared.Auditing;

public record AuditableModel
{
    public string? CreatedBy { get; init; }

    public DateTimeOffset CreatedOn { get; init; }

    public string? ModifiedBy { get; init; }

    public DateTimeOffset? ModifiedOn { get; init; }
}