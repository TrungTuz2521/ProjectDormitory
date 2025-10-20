using KTX.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 🔹 Kết nối database
builder.Services.AddDbContext<SinhVienKtxContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("KTX"));
});

// 🔹 Cấu hình Session
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

// ✅ Đặt phần Authentication TRƯỚC khi Build
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";           // Trang đăng nhập
        options.LogoutPath = "/Account/Logout";         // Trang đăng xuất
        options.AccessDeniedPath = "/Account/AccessDenied"; // Khi bị chặn quyền
        options.SlidingExpiration = true; // ✅ tự động gia hạn nếu user vẫn hoạt động
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ⚡ Thứ tự quan trọng:
app.UseAuthentication();   // ✅ Đặt trước Authorization
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
