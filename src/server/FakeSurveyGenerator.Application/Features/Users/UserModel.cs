using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Shared.Auditing;

namespace FakeSurveyGenerator.Application.Features.Users;

public sealed record UserModel : AuditableModel
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = null!;
    public string EmailAddress { get; init; } = null!;
    public string ExternalUserId { get; init; } = null!;
}

public static class UserModelMappingExtensions
{
    public static UserModel MapToModel(this User user)
    {
        return new UserModel
        {
            Id = user.Id,
            DisplayName = user.DisplayName.Value,
            EmailAddress = user.EmailAddress.Value,
            ExternalUserId = user.ExternalUserId.Value,
            CreatedBy = user.CreatedBy,
            CreatedOn = user.CreatedOn,
            ModifiedBy = user.ModifiedBy,
            ModifiedOn = user.ModifiedOn
        };
    }
}