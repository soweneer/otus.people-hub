using PeopleHub.Chats;
using PeopleHub.Chats.Db;
using PeopleHub.Chats.Grpc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options => options.Interceptors.Add<RequestIdServerInterceptor>());
builder.Services.AddChats(builder.Configuration);

var app = builder.Build();

if (app.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<IDbMigrator>().MigrateAsync();
}

app.MapGrpcService<DialogsGrpcService>();

app.Run();
