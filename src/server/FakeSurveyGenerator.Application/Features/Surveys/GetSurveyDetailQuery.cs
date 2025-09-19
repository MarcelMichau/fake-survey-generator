using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Abstractions;
using FakeSurveyGenerator.Application.Domain.Surveys;
using FakeSurveyGenerator.Application.Infrastructure.Persistence;
using FakeSurveyGenerator.Application.Shared.Caching;
using FakeSurveyGenerator.Application.Shared.Errors;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Features.Surveys;

public sealed record GetSurveyDetailQuery(int Id) : IQuery<Result<SurveyModel, Error>>;

public sealed class GetSurveyDetailQueryValidator : AbstractValidator<GetSurveyDetailQuery>
{
    public GetSurveyDetailQueryValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
    }
}

public sealed class GetSurveyDetailQueryHandler(
    SurveyContext surveyContext,
    ICache<SurveyModel?> cache,
    IValidator<GetSurveyDetailQuery> validator)
    : IQueryHandler<GetSurveyDetailQuery, Result<SurveyModel, Error>>
{
    private readonly ICache<SurveyModel?> _cache = cache ?? throw new ArgumentNullException(nameof(cache));

    private readonly SurveyContext _surveyContext =
        surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));

    private readonly IValidator<GetSurveyDetailQuery> _validator = validator ?? throw new ArgumentNullException(nameof(validator));

    public async Task<Result<SurveyModel, Error>> Handle(GetSurveyDetailQuery request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Errors.General.ValidationError(validationResult);
        }

        var cacheKey = $"{request.Id}";

        var survey = await _cache.GetOrCreateAsync(cacheKey, async token =>
        {
            var survey = await _surveyContext.Surveys
                .Include(s => s.Owner)
                .SelectToModel()
                .FirstOrDefaultAsync(s => s.Id == request.Id, token);
            return survey;
        }, cancellationToken);

        if (survey is null)
            return Errors.General.NotFound(nameof(Survey), request.Id);

        return survey;
    }
}