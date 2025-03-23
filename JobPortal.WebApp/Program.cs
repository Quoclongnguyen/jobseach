using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data.DataContext;
using JobPortal.Data.Entities;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Th�m d?ch v? v�o container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Cho ph�p bi�n d?ch l?i Razor View trong runtime.

var connectionString = builder.Configuration.GetConnectionString("JobPortalContextConnection")
    ?? throw new InvalidOperationException("Chu?i k?t n?i 'JobPortalContextConnection' kh�ng ???c t�m th?y.");

// C?u h�nh DbContext v?i SQL Server.
builder.Services.AddDbContext<DataDbContext>(options =>
    options.UseSqlServer(connectionString));

// C?u h�nh ch�nh s�ch cookie.
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // ??nh ngh?a xem c� c?n s? ??ng � c?a ng??i d�ng v?i cookie kh�ng thi?t y?u hay kh�ng.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None; // C?u h�nh SameSite cho cookie.
});

// ??ng k� d?ch v? DbContext cho vi?c s? d?ng ng?n h?n (Transient).
builder.Services.AddTransient<DataDbContext>();


builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequiredLength = 6; // ?? d�i m?t kh?u t?i thi?u.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false; 
    options.SignIn.RequireConfirmedAccount = false; 
})
    .AddEntityFrameworkStores<DataDbContext>() // S? d?ng DbContext cho Identity.
    .AddDefaultTokenProviders(); // Th�m c�c nh� cung c?p token m?c ??nh.

// ??ng k� UserManager v� SignInManager ?? qu?n l� ng??i d�ng v� ??ng nh?p.
builder.Services.AddTransient<UserManager<AppUser>, UserManager<AppUser>>();
builder.Services.AddTransient<SignInManager<AppUser>, SignInManager<AppUser>>();

// C?u h�nh Session cho ?ng d?ng.
builder.Services.AddDistributedMemoryCache(); // S? d?ng b? nh? ph�n t�n cho session.
builder.Services.AddSession(options => {
    options.Cookie.Name = "jobportal"; // ??t t�n cookie cho session.
    options.IdleTimeout = new TimeSpan(0, 30, 0); // Th?i gian session h?t h?n (30 ph�t).
});

// C?u h�nh cookie x�c th?c.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // Ch? s? d?ng cookie trong HTTP, kh�ng d�ng trong JavaScript.
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Th?i gian cookie h?t h?n.
 
});

builder.Services.AddSession(); // ??ng k� d?ch v? Session.

var app = builder.Build();

// C?u h�nh pipeline x? l� HTTP request.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // B?t HTTP Strict Transport Security (HSTS) v?i th?i h?n m?c ??nh l� 30 ng�y.
}

app.UseHttpsRedirection(); // Chuy?n h??ng m?i y�u c?u HTTP sang HTTPS.

app.UseStaticFiles(); // Cho ph�p truy c?p c�c t?p t?nh (CSS, JS, h�nh ?nh).

app.UseRouting(); 

app.UseAuthentication(); 

app.UseAuthorization(); 

app.UseSession(); 

// C?u h�nh tuy?n m?c ??nh cho ?ng d?ng.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// C?u h�nh tuy?n d�nh ri�ng cho c�c khu v?c (areas).
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});

app.Run(); 
