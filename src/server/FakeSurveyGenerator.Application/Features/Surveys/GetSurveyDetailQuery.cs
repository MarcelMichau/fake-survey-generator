using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Errors;
using FakeSurveyGenerator.Application.Shared.Identity;
using FluentValidation;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record GetSurveyDetailQuery(int Id) : IQuery<Result<SurveyModel, Error>>;

[UsedImplicitly]
public sealed class GetSurveyDetailQueryValidator : AbstractValidator<GetSurveyDetailQuery>
{
    public GetSurveyDetailQueryValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
    }
}

public sealed class GetSurveyDetailQueryHandler(
    SurveyContext surveyContext,
    IUserService userService,
    HybridCache cache,
    IValidator<GetSurveyDetailQuery> validator)
    : IQueryHandler<GetSurveyDetailQuery, Result<SurveyModel, Error>>
{
    // The shared (Redis) copy is removed when a Survey is deleted, but other instances may keep their in-memory copy
    // until it expires, so keep the in-memory lifetime short.
    private static readonly HybridCacheEntryOptions CacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };

    private readonly HybridCache _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IValidator<GetSurveyDetailQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    private static string SurveyKey(int id) => $"survey:{id}";

    public async Task<Result<SurveyModel, Error>> Handle(GetSurveyDetailQuery request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Errors.General.ValidationError(validationResult);
        }

        var key = SurveyKey(request.Id);

        var survey = await _cache.GetOrCreateAsync(key, async token =>
        {
            var survey = await _surveyContext.Surveys
                .AsNoTracking()
                .SelectToModel()
                .FirstOrDefaultAsync(s => s.Id == request.Id, token);
            return survey;
        }, CacheEntryOptions, cancellationToken: cancellationToken);

        if (survey is null)
        {
            // HybridCache stores null results too. Do not keep a "not found" around, otherwise a Survey which is
            // created later would keep returning NotFound until the entry expires.
            await _cache.RemoveAsync(key, cancellationToken);
            return Errors.General.NotFound(nameof(Survey), request.Id);
        }

        // Users may only read their own Surveys. Checked after the cache lookup as the cached model is not user-specific.
        // Someone else's Survey is reported exactly like a missing one, so that ids cannot be probed for existence.
        var userInfo = await _userService.GetUserInfo(cancellationToken);
        if (survey.OwnerExternalUserId != userInfo.Id)
            return Errors.General.NotFound(nameof(Survey), request.Id);

        return survey;
    }
}