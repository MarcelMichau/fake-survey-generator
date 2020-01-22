using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail
{
    public class GetSurveyDetailWithEntityFrameworkQueryHandler : IRequestHandler<GetSurveyDetailQuery, SurveyModel>
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;

        public GetSurveyDetailWithEntityFrameworkQueryHandler(ISurveyRepository surveyRepository, IMapper mapper, IDistributedCache cache)
        {
            _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _cache = cache ?? throw new ArgumentNullException(nameof(_cache));
        }

        public async Task<SurveyModel> Handle(GetSurveyDetailQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"FakeSurveyGenerator:Survey:{request.Id.ToString()}";

            var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrWhiteSpace(cachedValue))
                return System.Text.Json.JsonSerializer.Deserialize<SurveyModel>(cachedValue);

            var survey = await _surveyRepository.GetAsync(request.Id);

            if (survey == null)
                throw new KeyNotFoundException();

            await _cache.SetStringAsync(cacheKey, survey.ToString(), new DistributedCacheEntryOptions
            {
                SlidingExpiration = new TimeSpan(1, 0, 0)
            }, cancellationToken);

            return _mapper.Map<SurveyModel>(survey);
        }
    }
}
