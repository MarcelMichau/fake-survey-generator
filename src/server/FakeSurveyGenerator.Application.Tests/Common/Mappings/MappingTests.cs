using System;
using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Application.Surveys.Models;
using FakeSurveyGenerator.Application.Users.Models;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;
using FakeSurveyGenerator.Domain.AggregatesModel.UserAggregate;
using Xunit;

namespace FakeSurveyGenerator.Application.Tests.Common.Mappings;

public sealed class MappingTests
{
    private readonly IConfigurationProvider _configuration;
    private readonly IMapper _mapper;

    public MappingTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void ShouldHaveValidConfiguration()
    {
        _configuration.AssertConfigurationIsValid();
    }

    [Theory]
    [InlineData(typeof(Survey), typeof(SurveyModel))]
    [InlineData(typeof(SurveyOption), typeof(SurveyOptionModel))]
    [InlineData(typeof(User), typeof(UserModel))]
    public void ShouldSupportMappingFromSourceToDestination(Type source, Type destination)
    {
        var instance = Activator.CreateInstance(source, true);

        _mapper.Map(instance, source, destination);
    }
}