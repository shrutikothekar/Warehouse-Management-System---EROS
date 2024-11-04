using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AuthSystem.Data;
using AuthSystem.Areas.Identity.Data;
using AspNetCoreHero.ToastNotification;
using DatabaseBackupService.Controllers;
using static DatabaseBackupService.Controllers.Backup;
//using eros.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection") ?? throw new InvalidOperationException("Connection string 'AuthDbContextConnection' not found.");

//ADD BACKUP
builder.Services.AddSingleton<Backup>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection");
    var pgDumpPath = builder.Configuration.GetValue<string>("pgDumpPath");
    var logger = sp.GetRequiredService<ILogger<Backup>>();
    return new Backup(connectionString, pgDumpPath, logger);
});
builder.Services.AddHostedService<BackupBackgroundService>();
//END


//var connectionString1 = builder.Configuration.GetConnectionString("TestApiDB") ?? throw new InvalidOperationException("Connection string 'TestApiDB' not found.");

builder.Services.AddDbContext<ErosDbContext>(options => options.UseNpgsql(connectionString));
//builder.Services.AddDbContext<ErosTestApiDbContext>(options => options.UseNpgsql(connectionString1));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ErosDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddNotyf(config => { config.DurationInSeconds = 5; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireUppercase = false;
});
builder.Services.AddDistributedMemoryCache(); // Use an in-memory cache for session data
builder.Services.AddSession(options =>
{
    //options.IdleTimeout = TimeSpan.FromMinutes(60); // Set session timeout
    //options.Cookie.HttpOnly = true;
    //options.Cookie.IsEssential = true;

    //options.IdleTimeout = TimeSpan.FromHours(2); // Set session timeout to 2 hours
    //options.IdleTimeout = TimeSpan.FromSeconds(9);
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
