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

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");
app.MapRazorPages();
app.Run();
