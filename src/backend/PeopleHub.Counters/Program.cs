using PeopleHub.Counters;
using PeopleHub.Counters.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<RequestIdServerInterceptor>();
    options.Interceptors.Add<ErrorHandlingInterceptor>();
});
builder.Services.AddCounters(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<CountersGrpcService>();

app.Run();
