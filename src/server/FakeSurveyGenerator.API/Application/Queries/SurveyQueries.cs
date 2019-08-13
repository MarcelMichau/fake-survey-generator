using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FakeSurveyGenerator.API.Application.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace FakeSurveyGenerator.API.Application.Queries
{
    public class SurveyQueries : ISurveyQueries
    {
        private readonly string _connectionString;
        private readonly IDistributedCache _cache;

        public SurveyQueries(string connectionString, IDistributedCache cache)
        {
            _cache = cache;
            _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<SurveyModel> GetSurveyAsync(int id)
        {
            var cachedValue = await _cache.GetStringAsync(id.ToString());

            if (!string.IsNullOrWhiteSpace(cachedValue))
                return System.Text.Json.JsonSerializer.Deserialize<SurveyModel>(cachedValue);

            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var lookup = new Dictionary<int, SurveyModel>();
            await connection.QueryAsync<SurveyModel, SurveyOptionModel, SurveyModel>(@"
                SELECT s.*, so.*
                FROM Survey.Survey s
                INNER JOIN Survey.SurveyOption so ON so.SurveyId = s.Id
                WHERE s.Id = @id
                ", (s, so) =>
            {
                if (!lookup.TryGetValue(s.Id, out var survey))
                {
                    lookup.Add(s.Id, survey = s);
                }
                if (survey.Options == null)
                    survey.Options = new List<SurveyOptionModel>();
                survey.Options.Add(so);
                return survey;
            }, new { id });

            var result = lookup.Values;

            if (!result.Any())
                throw new KeyNotFoundException();

            var surveyResult = result.First();

            await _cache.SetStringAsync(id.ToString(), System.Text.Json.JsonSerializer.Serialize(surveyResult), new DistributedCacheEntryOptions
            {
                SlidingExpiration = new TimeSpan(1, 0, 0)
            });

            return surveyResult;
        }
    }
}
