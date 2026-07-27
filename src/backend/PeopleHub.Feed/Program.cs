using PeopleHub.Application;
using PeopleHub.Infrastructure;
using PeopleHub.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, "people-hub-feed");
builder.Services.AddHostedService<FeedMaterializerWorker>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok("healthy"));

app.Run();
