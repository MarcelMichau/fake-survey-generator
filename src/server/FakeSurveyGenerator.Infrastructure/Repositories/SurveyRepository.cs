using System;
using System.Threading.Tasks;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Infrastructure.Repositories
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly SurveyContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public SurveyRepository(SurveyContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Add(Survey survey)
        {
            _context.Surveys.Add(survey);
        }

        public void Update(Survey survey)
        {
            _context.Entry(survey).State = EntityState.Modified;
        }

        public async Task<Survey> GetAsync(int surveyId)
        {
            var survey = await _context.Surveys.FindAsync(surveyId);

            if (survey != null)
            {
                await _context.Entry(survey)
                    .Collection(s => s.Options).LoadAsync();
            }

            return survey;
        }
    }
}
