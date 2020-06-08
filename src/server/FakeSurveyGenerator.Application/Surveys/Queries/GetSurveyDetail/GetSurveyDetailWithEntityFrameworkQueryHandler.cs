using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail
{
    public sealed class GetSurveyDetailWithEntityFrameworkQueryHandler : IRequestHandler<GetSurveyDetailQuery, Result<SurveyModel, Error>>
    {
        private readonly ISurveyContext _surveyContext;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public GetSurveyDetailWithEntityFrameworkQueryHandler(ISurveyContext surveyContext, IMapper mapper, IDistributedCache cache)
        {
            _surveyContext = surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<Result<SurveyModel, Error>> Handle(GetSurveyDetailQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"FakeSurveyGenerator:Survey:{request.Id}";

            var cachedValue = await _cache.GetAsync(cacheKey, cancellationToken);

            if (cachedValue != null && cachedValue.Length > 0)
                return Result.Success<SurveyModel, Error>(JsonSerializer.Deserialize<SurveyModel>(cachedValue));

            var survey = await _surveyContext.Surveys
                .Include(s => s.Options)
                .ProjectTo<SurveyModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

            if (survey == null)
                return Result.Failure<SurveyModel, Error>(Errors.General.NotFound(nameof(Survey), request.Id));

            await _cache.SetStringAsync(cacheKey, survey.ToString(), new DistributedCacheEntryOptions
            {
                SlidingExpiration = new TimeSpan(1, 0, 0)
            }, cancellationToken);

            return Result.Success<SurveyModel, Error>(survey);
        }
    }
}
