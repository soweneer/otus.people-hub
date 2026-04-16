using Microsoft.AspNetCore.Authentication.Cookies;
using PeopleHub.Domain;
using PeopleHub.Infrastructure;
using PeopleHub.Shared;

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

builder.Services.AddPeopleHubShared();
builder.Services.AddPeopleHubDomain();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();
builder.Services.AddPeopleHubInfrastructure(app.Configuration);
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
