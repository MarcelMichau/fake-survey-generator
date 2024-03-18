using FakeSurveyGenerator.Application;
using FakeSurveyGenerator.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var host = builder.Build();
host.Run();