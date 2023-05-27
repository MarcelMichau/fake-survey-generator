﻿using AutoMapper;
using FakeSurveyGenerator.Application.Common.Mappings;
using FakeSurveyGenerator.Domain.AggregatesModel.SurveyAggregate;

namespace FakeSurveyGenerator.Application.Surveys.Models;

public sealed record SurveyOptionModel : IMapFrom<SurveyOption>
{
    public string OptionText { get; init; } = null!;
    public int NumberOfVotes { get; init; }
    public int PreferredNumberOfVotes { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SurveyOption, SurveyOptionModel>()
            .ForMember(dest => dest.OptionText, opts => opts.MapFrom(src => src.OptionText.Value));
    }
}