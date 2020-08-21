using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CSharpFunctionalExtensions;
using FakeSurveyGenerator.Application.Common.Errors;
using FakeSurveyGenerator.Application.Common.Identity;
using FakeSurveyGenerator.Application.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FakeSurveyGenerator.Application.Surveys.Queries.GetUserSurveys
{
    public sealed class GetUserSurveysQueryHandler : IRequestHandler<GetUserSurveysQuery, Result<List<UserSurveyModel>, Error>>
    {
        private readonly IUserService _userService;
        private readonly ISurveyContext _surveyContext;
        private readonly IMapper _mapper;

        public GetUserSurveysQueryHandler(IUserService userService, ISurveyContext surveyContext, IMapper mapper)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _surveyContext = surveyContext ?? throw new ArgumentNullException(nameof(surveyContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result<List<UserSurveyModel>, Error>> Handle(GetUserSurveysQuery request,
            CancellationToken cancellationToken)
        {
            var userInfo = await _userService.GetUserInfo(cancellationToken);

            var surveyOwner =
                await _surveyContext.Users.FirstAsync(user => user.ExternalUserId == userInfo.Id,
                    cancellationToken);

            var surveys = await _surveyContext.Surveys
                .Include(s => s.Options)
                .Include(s => s.Owner)
                .Where(s => s.Owner.Id == surveyOwner.Id)
                .ProjectTo<UserSurveyModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result.Success<List<UserSurveyModel>, Error>(surveys);
        }
    }
}
