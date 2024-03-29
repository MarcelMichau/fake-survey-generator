﻿using AutoMapper;
using JetBrains.Annotations;

namespace FakeSurveyGenerator.Application.Shared.Mappings;

public interface IMapFrom<T>
{
    [UsedImplicitly]
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}