using Microsoft.AspNetCore.Server.Kestrel.Core;
using PeopleHub.Chats;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Grpc;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue("Ports:Grpc", 8081),
        listen => listen.Protocols = HttpProtocols.Http2);
    options.ListenAnyIP(builder.Configuration.GetValue("Ports:Metrics", 8091),
        listen => listen.Protocols = HttpProtocols.Http1);
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<RequestIdServerInterceptor>();
    options.Interceptors.Add<MetricsInterceptor>();
    options.Interceptors.Add<ErrorHandlingInterceptor>();
});
builder.Services.AddChats(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IDbMigrator>().MigrateAsync();
}

app.MapGrpcService<DialogsGrpcService>();
app.MapMetrics();

app.Run();
