using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using PeopleHub.Application;
using PeopleHub.Application.Abstractions;
using PeopleHub.Auth;
using PeopleHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtTokenIssuer>();
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
                 ?? throw new MissingMemberException("Jwt configuration section is absent");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        // Фронтенд — React SPA: вместо редиректов на страницу логина возвращаем коды статуса
        opt.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        opt.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

// ---------------------------------------------------------------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddPeopleHubApplication();
var dbConnectionString = builder.Configuration.GetConnectionString("PostgreSql");
if (string.IsNullOrEmpty(dbConnectionString))
    throw new MissingMemberException("Connection string is absent");
builder.Services.AddPeopleHubInfrastructure(dbConnectionString);
// ---------------------------------------------------------------------------------------------------------------------------------

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "PeopleHub API",
        Version = "v1"
    });
    options.AddSecurityDefinition("bearerAuth", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT-токен из POST /api/login (без префикса 'Bearer ')"
    });
    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        [new Microsoft.OpenApi.OpenApiSecuritySchemeReference("bearerAuth", document)] = []
    });
});

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
