using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Caching;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Common.Persistence;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail;

public sealed record GetSurveyDetailQuery(int Id) : IRequest<Result<SurveyModel, Error>>;

public sealed class GetSurveyDetailQueryHandler : IRequestHandler<GetSurveyDetailQuery, Result<SurveyModel, Error>>
{
    private readonly ISurveyContext _surveyContext;
    private readonly IMapper _mapper;
    private readonly ICache<SurveyModel> _cache;

    public GetSurveyDetailQueryHandler(ISurveyContext surveyContext, IMapper mapper, ICache<SurveyModel> cache)
    {
        _surveyContext = surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<Result<SurveyModel, Error>> Handle(GetSurveyDetailQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"{request.Id}";

        var (isCached, cachedSurvey) = await _cache.TryGetValueAsync(cacheKey, cancellationToken);

        if (isCached)
            return cachedSurvey!;

        var survey = await _surveyContext.Surveys
            .Include(s => s.Options)
            .Include(s => s.Owner)
            .ProjectTo<SurveyModel>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (survey is null)
            return Errors.General.NotFound(nameof(Survey), request.Id);

        await _cache.SetAsync(cacheKey, survey, 60, cancellationToken);

        return survey;
    }
}