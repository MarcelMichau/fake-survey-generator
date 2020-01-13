using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Application.Surveys.Commands.CreateSurvey;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.SeedWork;
using MediatR;
using Moq;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Surveys.Commands.CreateSurvey
{
    public class CreateSurveyCommandTests
    {
        private readonly IMapper _testMapper;
        private readonly ISurveyRepository _testSurveyRepository;

        public CreateSurveyCommandTests()
        {
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _testMapper = configurationProvider.CreateMapper();
            var mockRepository = new Mock<ISurveyRepository>();

            mockRepository.SetupGet(r => r.UnitOfWork).Returns(new Mock<IUnitOfWork>().Object);

            _testSurveyRepository = mockRepository.Object;
        }

        [Fact]
        public async Task Handle_GivenValidRequest_ShouldRaiseSurveyCreatedNotification()
        {
            var topic = "Tabs or spaces?";
            var numberOfRespondents = 1;
            var respondentType = "Developers";

            var options = new List<SurveyOptionDto>
            {
                new SurveyOptionDto
                {
                    OptionText = "Tabs"
                },
                new SurveyOptionDto
                {
                    OptionText = "Spaces"
                }
            };

            var mockMediator = new Mock<IMediator>();

            var createSurveyCommand = new CreateSurveyCommand(topic, numberOfRespondents, respondentType, options);

            var sut = new CreateSurveyCommandHandler(_testSurveyRepository, _testMapper, mockMediator.Object);

            var result = await sut.Handle(createSurveyCommand, CancellationToken.None);

            Assert.Equal(topic, result.Topic);
            Assert.Equal(numberOfRespondents, result.NumberOfRespondents);
            Assert.Equal(respondentType, result.RespondentType);
            Assert.True(result.CreatedOn < DateTime.UtcNow, "The createdOn date was not in the past");

            mockMediator.Verify(m => m.Publish(It.IsAny<SurveyCreated>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
