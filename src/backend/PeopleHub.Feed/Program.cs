using PeopleHub.Application;
using PeopleHub.Feed.Auth;
using PeopleHub.Feed.Services;
using PeopleHub.Feed.WebSockets;
using PeopleHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, "people-hub-feed");
builder.Services.AddFeedAuth(builder.Configuration);
builder.Services.AddSingleton<FeedNotificationPublisher>();
builder.Services.AddHostedService<FeedMaterializerWorker>();
builder.Services.AddSingleton<FeedConnectionRegistry>();
builder.Services.AddSingleton<FeedSubscriber>();

var app = builder.Build();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
    KeepAliveTimeout = TimeSpan.FromSeconds(15)
});
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("healthy"));
app.MapFeedWebSocket();

app.Run();
