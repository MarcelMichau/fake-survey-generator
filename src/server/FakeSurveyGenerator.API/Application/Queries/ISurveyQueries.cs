using System.Threading.Tasks;
using FakeSurveyGenerator.API.Application.Models;

namespace FakeSurveyGenerator.API.Application.Queries
{
    public interface ISurveyQueries
    {
        Task<SurveyModel> GetSurveyAsync(int id);
    }
}
