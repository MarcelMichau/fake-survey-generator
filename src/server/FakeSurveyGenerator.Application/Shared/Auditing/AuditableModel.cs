namespace FakeSurveyGenerator.Application.Shared.Auditing;

public record AuditableModel
{
    public required string? CreatedBy { get; init; }

    public required DateTimeOffset CreatedOn { get; init; }

    public required string? ModifiedBy { get; init; }

    public required DateTimeOffset? ModifiedOn { get; init; }
}