using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using MusicApp.Data;
using MusicApp.Services;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Load .env vars into IConfiguration (override appsettings.json)
builder.Configuration.AddEnvironmentVariables(prefix: "");

var connString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "";
if (!string.IsNullOrEmpty(connString))
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connString;

var cloudName  = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? "";
var apiKey     = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")     ?? "";
var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")    ?? "";
if (!string.IsNullOrEmpty(cloudName))
    builder.Configuration["Cloudinary:CloudName"] = cloudName;
if (!string.IsNullOrEmpty(apiKey))
    builder.Configuration["Cloudinary:ApiKey"] = apiKey;
if (!string.IsNullOrEmpty(apiSecret))
    builder.Configuration["Cloudinary:ApiSecret"] = apiSecret;

var mailHost     = Environment.GetEnvironmentVariable("MAIL_HOST")     ?? "";
var mailPort     = Environment.GetEnvironmentVariable("MAIL_PORT")     ?? "";
var mailUsername = Environment.GetEnvironmentVariable("MAIL_USERNAME") ?? "";
var mailPassword = Environment.GetEnvironmentVariable("MAIL_PASSWORD") ?? "";
if (!string.IsNullOrEmpty(mailHost))
    builder.Configuration["Mail:Host"] = mailHost;
if (!string.IsNullOrEmpty(mailPort))
    builder.Configuration["Mail:Port"] = mailPort;
if (!string.IsNullOrEmpty(mailUsername))
    builder.Configuration["Mail:Username"] = mailUsername;
if (!string.IsNullOrEmpty(mailPassword))
    builder.Configuration["Mail:Password"] = mailPassword;

var appBaseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "";
if (!string.IsNullOrEmpty(appBaseUrl))
    builder.Configuration["App:BaseUrl"] = appBaseUrl;

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")!));

// ── Authentication ────────────────────────────────────────────────────────
builder.Services.AddAuthentication().AddCookie(options =>
{
    var auth = builder.Configuration.GetSection("Auth");
    options.Cookie.Name = auth["CookieName"] ?? "music_auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromDays(
        int.Parse(auth["ExpireDays"] ?? "7"));
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/login";
});
builder.Services.AddAuthorization();

// ── Services ───────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IToastService, ToastService>();

// ── Blazor + MVC ───────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages(); // enables .cshtml Razor Pages like /logout, /auth/login
builder.Services.AddControllers();

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

// ── Database init ──────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

app.MapRazorComponents<MusicApp.Components.App>()
    .AddInteractiveServerRenderMode();
app.MapRazorPages(); // enables .cshtml Razor Pages
app.MapControllers();

await app.RunAsync();
