using FakeSurveyGenerator.Application.Domain.Users;
using FakeSurveyGenerator.Application.Shared.Auditing;

namespace FakeSurveyGenerator.Application.Features.Users;

public sealed record UserModel : AuditableModel
{
    public required int Id { get; init; }
    public required string DisplayName { get; init; }
    public required string EmailAddress { get; init; }
    public required string ExternalUserId { get; init; }
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