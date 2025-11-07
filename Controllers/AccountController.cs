using KTX.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KTX.Models;

namespace KTX.Controllers
{
    public class AccountController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public AccountController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Hash password
                string Password = model.Password.Trim();

                // 🟩 1️⃣ Kiểm tra tài khoản admin trước
                var admin = _context.Admins.FirstOrDefault(a =>
                    a.TenDn.Trim() == model.Username.Trim() &&
                    a.MatKhau.Trim() == Password);

                if (admin != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.MaAdmin.ToString()),
                new Claim(ClaimTypes.Name, admin.TenDn),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim("VaiTro", admin.VaiTro ?? "")
            };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                    );

                    HttpContext.Session.SetString("Admin", admin.TenDn);
                    HttpContext.Session.SetString("VaiTro", admin.VaiTro ?? "");

                    // 👉 Chuyển đến trang quản trị
                    return RedirectToAction("Index", "Dashboard");
                }

                // Tìm sinh viên trong database
                var user = _context.SinhViens.FirstOrDefault(u =>
                    u.TenDn.Trim() == model.Username.Trim() &&
                    u.MatKhau.Trim() == Password);

                if (user != null)
                {
                    // Tạo claims cho user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Msv.ToString()),
                        new Claim(ClaimTypes.Name, user.TenDn),
                        new Claim("HoTen", user.HoTen ?? ""),
                        new Claim(ClaimTypes.Role, "SinhVien")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(15)
                    };

                    // Đăng nhập
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties
                        );

                    // Lưu session
                    HttpContext.Session.SetInt32("UserMSV", user.Msv);
                    HttpContext.Session.SetString("TenDn", user.TenDn);
                    HttpContext.Session.SetString("HoTen", user.HoTen ?? "");

                    // Chuyển hướng sau khi đăng nhập
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                }
            }

            return View(model);
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return Redirect("~/Home/Index");

        }

        // Hàm hash password bằng SHA256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
