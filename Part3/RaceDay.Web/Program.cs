using System.Globalization;
using RaceDay.Web.Services;

//use the same date and number formats everywhere, so the site looks the same locally and in Docker
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

//session setup: the website remembers who is logged in and keeps the API's session cookie
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//the website only talks to the API through this client, never to the database
builder.Services.AddHttpClient<ApiClient>(client =>
{
    string baseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5022/";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    //cookies are handled by hand so each website visitor keeps their own API session
    UseCookies = false,
    AllowAutoRedirect = false
});

var app = builder.Build();

//visitors see a friendly error page instead of raw error details
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

//sessions must come before the controllers
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();