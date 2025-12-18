using FinMate.web.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------

// MVC (Controllers + Views)
builder.Services.AddControllersWithViews();

// HttpClientFactory (used in AuthController + ApiClient)
builder.Services.AddHttpClient();

// Needed because ApiClient reads token from cookies via HttpContext
builder.Services.AddHttpContextAccessor();

// Your ApiClient service (DI)
builder.Services.AddScoped<ApiClient>();

// (Optional but safe) TempData + Session support if you ever need it
// builder.Services.AddSession();

var app = builder.Build();

// -------------------- Middleware --------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// If you ever use [Authorize] later, keep these:
app.UseAuthentication();
app.UseAuthorization();

// (Optional) If you enable session above
// app.UseSession();

// -------------------- Routes --------------------

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
