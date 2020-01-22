using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FakeSurveyGenerator.Application.Common.Interfaces;
using FakeSurveyGenerator.Application.Surveys.Models;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetSurveyDetail
{
    //public class GetSurveyDetailWithDapperQueryHandler : IRequestHandler<GetSurveyDetailQuery, SurveyModel>
    //{
    //    private readonly string _connectionString;
    //    private readonly IDistributedCache _cache;

    //    public GetSurveyDetailWithDapperQueryHandler(IConnectionString connectionString, IDistributedCache cache)
    //    {
    //        _cache = cache;
    //        _connectionString = !string.IsNullOrWhiteSpace(connectionString.Value) ? connectionString.Value : throw new ArgumentNullException(nameof(connectionString));
    //    }

    //    public async Task<SurveyModel> Handle(GetSurveyDetailQuery request, CancellationToken cancellationToken)
    //    {
    //        var cacheKey = $"FakeSurveyGenerator:Survey:{request.Id.ToString()}";
    //        var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);

    //        if (!string.IsNullOrWhiteSpace(cachedValue))
    //            return System.Text.Json.JsonSerializer.Deserialize<SurveyModel>(cachedValue);

    //        await using var connection = new SqlConnection(_connectionString);
    //        connection.Open();

    //        var lookup = new Dictionary<int, SurveyModel>();
    //        await connection.QueryAsync<SurveyModel, SurveyOptionModel, SurveyModel>(@"
    //            SELECT s.*, so.*
    //            FROM Survey.Survey s
    //            INNER JOIN Survey.SurveyOption so ON so.SurveyId = s.Id
    //            WHERE s.Id = @id
    //            ", (s, so) =>
    //        {
    //            if (!lookup.TryGetValue(s.Id, out var survey))
    //            {
    //                lookup.Add(s.Id, survey = s);
    //            }
    //            if (survey.Options == null)
    //                survey.Options = new List<SurveyOptionModel>();
    //            survey.Options.Add(so);
    //            return survey;
    //        }, new { request.Id });

    //        var result = lookup.Values;

    //        if (!result.Any())
    //            throw new KeyNotFoundException();

    //        var surveyResult = result.First();

    //        await _cache.SetStringAsync(cacheKey, surveyResult.ToString(), new DistributedCacheEntryOptions
    //        {
    //            SlidingExpiration = new TimeSpan(1, 0, 0)
    //        }, cancellationToken);

    //        return surveyResult;
    //    }
    //}
}
