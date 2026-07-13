using Microsoft.AspNetCore.Authentication.Cookies;
using PeopleHub.Domain;
using PeopleHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opt =>
    {
        opt.LoginPath = new PathString("/Account/SignIn");
        opt.Events.OnRedirectToLogin = context =>
        {
                context.Request.Path.StartsWithSegments("/post"))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }

            return Task.CompletedTask;
        };
    });

// ---------------------------------------------------------------------------------------------------------------------------------
builder.Services.AddPeopleHubDomain();
var dbConnectionString = builder.Configuration.GetConnectionString("PostgreSql");
if (string.IsNullOrEmpty(dbConnectionString))
    throw new MissingMemberException("Connection string is absent");
builder.Services.AddPeopleHubInfrastructure(dbConnectionString);
// ---------------------------------------------------------------------------------------------------------------------------------

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "PeopleHub API",
        Version = "v1"
    });
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");
app.MapRazorPages();
app.Run();
