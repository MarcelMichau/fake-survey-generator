using FakeSurveyGenerator.Application;
using FakeSurveyGenerator.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.AddInfrastructure();
builder.AddApplication();

var host = builder.Build();
host.Run();