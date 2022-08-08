using Microsoft.AspNetCore.Authentication.Cookies;
using System.Diagnostics;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel();
builder.WebHost.UseIIS();
builder.WebHost.UseIISIntegration();
builder.WebHost.UseUrls("https://*:4388");
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddAuthentication("GSRecordMining").AddCookie("GSRecordMining", options =>
{
    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = "GSRecordMining",
        Path = "/",
        SameSite = SameSiteMode.Strict,
        SecurePolicy = CookieSecurePolicy.SameAsRequest,
        MaxAge = DateTime.Now.Subtract(DateTime.UtcNow).Add(TimeSpan.FromDays(2))
    };
    options.Events = new CookieAuthenticationEvents
    {

    };
    options.LoginPath = new PathString("/LogIn");
    options.AccessDeniedPath = new PathString("/LogIn");
    options.LogoutPath = new PathString("/LogOut");
    options.ReturnUrlParameter = "RequestPath";
    options.SlidingExpiration = true;
});
builder.Services.AddDbContext<GSRecordMining.Models.RecordContext>();
builder.Services.AddSingleton<GSRecordMining.Services.EncodeService>();
builder.Services.AddSingleton<GSRecordMining.Services.DBService>();
builder.Services.AddSingleton<GSRecordMining.Services.SmbService>();

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{action}/{id?}",
        defaults: new { controller = "Dashboard", action = "Index" }
);

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(OnStarted);
app.Lifetime.ApplicationStopping.Register(OnStopping);
app.Lifetime.ApplicationStopped.Register(OnStopped);

void OnStarted()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        Process.Start(new ProcessStartInfo("https://localhost:4388") { UseShellExecute = true });
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        Process.Start("xdg-open", "https://localhost:4388");
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        Process.Start("open", "https://localhost:4388");
    }
    else
    {
        // throw 
    }
}
void OnStopping()
{

}
void OnStopped()
{

}

app.Run();
