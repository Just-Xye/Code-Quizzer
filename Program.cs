using Code_Quizzer.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Code_Quizzer.Models;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// --- 1. SERVICES CONFIGURATION ---
// =========================================================================

// Register SQL Server Database Context (Must come BEFORE Identity adds stores)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity Services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders(); // Added for robust multi-auth token tracking

// Add MVC Support
builder.Services.AddControllersWithViews();

// Register Session Services (Required for HttpContext.Session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// =========================================================================
// --- 2. MIDDLEWARE PIPELINE (CRITICAL ORDER MATTERS) ---
// =========================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();

// 1. Establish path routing first
app.UseRouting();

// 2. Initialize temporary/active key-value data structures
app.UseSession();

// 3. Inspect cookies and construct the 'User' object (FIXED: Added this)
app.UseAuthentication();

// 4. Determine if the constructed 'User' has clearance to access targeted URLs
app.UseAuthorization();

// Assets and Static Files
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Place this code right above your final app.Run(); line
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Fire your custom runtime data insertion
        Code_Quizzer.Data.DbInitializer.SeedData(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unhandled exception occurred during application database initialization.");
    }
}

app.Run();