using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.SeedWork;

namespace FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate
{
    public interface ISurveyRepository : IRepository<Survey>
    {
        void Add(Survey survey);

        void Update(Survey survey);

        Task<Survey> GetAsync(int surveyId);
    }
}
