using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using JobPortal.Data.DataContext;
using JobPortal.Data.Entities;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Thêm d?ch v? vào container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(); // Cho phép biên d?ch l?i Razor View trong runtime.

var connectionString = builder.Configuration.GetConnectionString("JobPortalContextConnection")
    ?? throw new InvalidOperationException("Chu?i k?t n?i 'JobPortalContextConnection' không ???c tìm th?y.");

// C?u hình DbContext v?i SQL Server.
builder.Services.AddDbContext<DataDbContext>(options =>
    options.UseSqlServer(connectionString));

// C?u hình chính sách cookie.
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // ??nh ngh?a xem có c?n s? ??ng ý c?a ng??i dùng v?i cookie không thi?t y?u hay không.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None; // C?u hình SameSite cho cookie.
});

// ??ng ký d?ch v? DbContext cho vi?c s? d?ng ng?n h?n (Transient).
builder.Services.AddTransient<DataDbContext>();


builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequiredLength = 6; // ?? dài m?t kh?u t?i thi?u.
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false; 
    options.SignIn.RequireConfirmedAccount = false; 
})
    .AddEntityFrameworkStores<DataDbContext>() // S? d?ng DbContext cho Identity.
    .AddDefaultTokenProviders(); // Thêm các nhà cung c?p token m?c ??nh.

// ??ng ký UserManager và SignInManager ?? qu?n lý ng??i dùng và ??ng nh?p.
builder.Services.AddTransient<UserManager<AppUser>, UserManager<AppUser>>();
builder.Services.AddTransient<SignInManager<AppUser>, SignInManager<AppUser>>();

// C?u hình Session cho ?ng d?ng.
builder.Services.AddDistributedMemoryCache(); // S? d?ng b? nh? phân tán cho session.
builder.Services.AddSession(options => {
    options.Cookie.Name = "jobportal"; // ??t tên cookie cho session.
    options.IdleTimeout = new TimeSpan(0, 30, 0); // Th?i gian session h?t h?n (30 phút).
});

// C?u hình cookie xác th?c.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true; // Ch? s? d?ng cookie trong HTTP, không dùng trong JavaScript.
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Th?i gian cookie h?t h?n.
 
});

builder.Services.AddSession(); // ??ng ký d?ch v? Session.

var app = builder.Build();

// C?u hình pipeline x? lý HTTP request.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // B?t HTTP Strict Transport Security (HSTS) v?i th?i h?n m?c ??nh là 30 ngày.
}

app.UseHttpsRedirection(); // Chuy?n h??ng m?i yêu c?u HTTP sang HTTPS.

app.UseStaticFiles(); // Cho phép truy c?p các t?p t?nh (CSS, JS, hình ?nh).

app.UseRouting(); 

app.UseAuthentication(); 

app.UseAuthorization(); 

app.UseSession(); 

// C?u hình tuy?n m?c ??nh cho ?ng d?ng.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// C?u hình tuy?n dành riêng cho các khu v?c (areas).
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});

app.Run(); 
