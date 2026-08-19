using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;
using Ybt.Core.Interfaces;
using Ybt.Data.Context;
using Ybt.Data.Repositories;
using Ybt.Service.Services;
using FluentValidation.AspNetCore;
using FluentValidation;
using Ybt.Data;
using System.Reflection;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure Forwarded Headers for Reverse Proxies (Nginx, Docker, Render, Railway, Azure)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add services to the container.
var connectionString = GetPostgresConnectionString(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<AppUser, AppRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LogoutPath = "/Account/Logout";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/ybt-yonetim"))
        {
            context.Response.Redirect("/ybt-yonetim?ReturnUrl=" + System.Net.WebUtility.UrlEncode(context.Request.Path + context.Request.QueryString));
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("strict-limit", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));

// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

app.UseForwardedHeaders();

// Security Headers
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseMiddleware<Ybt.Web.Middleware.ExceptionMiddleware>();
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        
        context.Database.Migrate();
        await DbInitializer.SeedAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

// Block legacy admin routes completely
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLowerInvariant();
    if (path != null && (path == "/administrative" || path.StartsWith("/administrative/") ||
                         path == "/adminstrator" || path.StartsWith("/adminstrator/") ||
                         path == "/admin" || path.StartsWith("/admin/")))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});

app.MapControllerRoute(
    name: "ybt-yonetim-login",
    pattern: "ybt-yonetim",
    defaults: new { controller = "Account", action = "AdminLogin" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-dashboard",
    areaName: "Admin",
    pattern: "ybt-yonetim/panel/{action=Index}/{id?}",
    defaults: new { controller = "Dashboard" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-etkinlikler",
    areaName: "Admin",
    pattern: "ybt-yonetim/etkinlikler/{action=Index}/{id?}",
    defaults: new { controller = "Events" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-bloglar",
    areaName: "Admin",
    pattern: "ybt-yonetim/bloglar/{action=Index}/{id?}",
    defaults: new { controller = "Blogs" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-projeler",
    areaName: "Admin",
    pattern: "ybt-yonetim/projeler/{action=Index}/{id?}",
    defaults: new { controller = "Projects" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-kullanicilar",
    areaName: "Admin",
    pattern: "ybt-yonetim/kullanicilar/{action=Index}/{id?}",
    defaults: new { controller = "Users" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-basvurular",
    areaName: "Admin",
    pattern: "ybt-yonetim/basvurular/{action=Index}/{id?}",
    defaults: new { controller = "Applications" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-hakkimizda",
    areaName: "Admin",
    pattern: "ybt-yonetim/hakkimizda-icerikleri/{action=Index}/{id?}",
    defaults: new { controller = "AboutFeatures" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-ekip",
    areaName: "Admin",
    pattern: "ybt-yonetim/ekip-uyeleri/{action=Index}/{id?}",
    defaults: new { controller = "TeamMembers" });

app.MapAreaControllerRoute(
    name: "ybt-yonetim-fallback",
    areaName: "Admin",
    pattern: "ybt-yonetim/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine("\n==================================================");
    Console.WriteLine(" 🚀 YBT Web Uygulaması Başlatıldı!");
    Console.WriteLine(" 🌐 HTTP:  http://localhost:5261");
    Console.WriteLine(" 🔒 HTTPS: https://localhost:7277");
    Console.WriteLine("==================================================\n");
});

app.Run();

static string GetPostgresConnectionString(IConfiguration configuration)
{
    // 1. Check for DATABASE_URL (common in Render, Railway, Heroku, Supabase, Neon)
    var rawUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(rawUrl))
    {
        if (rawUrl.StartsWith("postgres://") || rawUrl.StartsWith("postgresql://"))
        {
            var uri = new Uri(rawUrl);
            var userInfo = uri.UserInfo.Split(':');
            var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');

            return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true;";
        }
        return rawUrl;
    }

    // 2. Fall back to standard ConnectionStrings:DefaultConnection
    return configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration or environment variables.");
}
