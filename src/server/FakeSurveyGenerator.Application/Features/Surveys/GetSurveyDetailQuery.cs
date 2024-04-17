using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Caching;
using FakeSurveyGenerator.Application.Shared.Errors;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record GetSurveyDetailQuery(int Id) : IRequest<Result<SurveyModel, Error>>;

public sealed class GetSurveyDetailQueryValidator : AbstractValidator<GetSurveyDetailQuery>
{
    public GetSurveyDetailQueryValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
    }
}

public sealed class GetSurveyDetailQueryHandler(
    SurveyContext surveyContext,
    ICache<SurveyModel> cache)
    : IRequestHandler<GetSurveyDetailQuery, Result<SurveyModel, Error>>
{
    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly ICache<SurveyModel> _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    public async Task<Result<SurveyModel, Error>> Handle(GetSurveyDetailQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{request.Id}";

        var (isCached, cachedSurvey) = await _cache.TryGetValueAsync(cacheKey, cancellationToken);

        if (isCached)
            return cachedSurvey!;

        var survey = await _surveyContext.Surveys
            .Include(s => s.Owner)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (survey is null)
            return Errors.General.NotFound(nameof(Survey), request.Id);

        await _cache.SetAsync(cacheKey, survey.MapToModel(), 60, cancellationToken);

        return survey.MapToModel();
    }
}