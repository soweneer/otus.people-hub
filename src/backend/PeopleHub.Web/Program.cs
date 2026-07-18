using PeopleHub.Application;
using PeopleHub.Extensions;
using PeopleHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddAuth(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwagger();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "PeopleHub API v1");
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA: все не-API маршруты отдают React-приложение
app.MapFallbackToFile("index.html");
app.Run();
