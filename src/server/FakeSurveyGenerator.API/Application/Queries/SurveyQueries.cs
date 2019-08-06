using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FakeSurveyGenerator.API.Application.Models;

namespace FakeSurveyGenerator.API.Application.Queries
{
    public class SurveyQueries : ISurveyQueries
    {
        private readonly string _connectionString;

        public SurveyQueries(string connectionString)
        {
            _connectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<SurveyModel> GetSurveyAsync(int id)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var lookup = new Dictionary<int, SurveyModel>();
            await connection.QueryAsync<SurveyModel, SurveyOptionModel, SurveyModel>(@"
                SELECT s.*, so.*
                FROM Survey.Survey s
                INNER JOIN Survey.SurveyOption so ON so.SurveyId = s.Id
                WHERE s.Id = @id
                ", (s, so) => {
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

            return result.First();
        }
    }
}
