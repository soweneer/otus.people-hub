using Microsoft.AspNetCore.Server.Kestrel.Core;
using PeopleHub.Counters;
using PeopleHub.Counters.Grpc;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(builder.Configuration.GetValue("Ports:Grpc", 8083),
        listen => listen.Protocols = HttpProtocols.Http2);
    options.ListenAnyIP(builder.Configuration.GetValue("Ports:Metrics", 8093),
        listen => listen.Protocols = HttpProtocols.Http1);
});

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<RequestIdServerInterceptor>();
    options.Interceptors.Add<ErrorHandlingInterceptor>();
});
builder.Services.AddCounters(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<CountersGrpcService>();
app.MapMetrics();

app.Run();
