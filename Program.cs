using Microsoft.EntityFrameworkCore;
using QazaqQuest.Data;
using QazaqQuest.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "QazaqQuest.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromHours(2);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AppDataService>();
builder.Services.AddScoped<UserStoreService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<SocialService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    DatabaseInitializer.EnsureSchema(dbContext);

    var dataService = scope.ServiceProvider.GetRequiredService<AppDataService>();
    dataService.EnsureSeedData();
    var socialService = scope.ServiceProvider.GetRequiredService<SocialService>();
    socialService.EnsureSeedData();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseStatusCodePages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
