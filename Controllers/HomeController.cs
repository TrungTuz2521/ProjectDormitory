using KTX.Entities;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;

namespace KTX.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public HomeController(SinhVienKtxContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy MSV từ claim
            var msvString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra rỗng hoặc không hợp lệ
            if (string.IsNullOrEmpty(msvString))
                return RedirectToAction("Index", "Login");

            // Chuyển sang int nếu Msv là kiểu int trong database
            if (!int.TryParse(msvString, out int msv))
                return RedirectToAction("Index", "Login");

            // --- phần code phía dưới giữ nguyên ---
            var hopDong = _context.HopDongPhongs
                .Include(h => h.MaPNavigation)
                .FirstOrDefault(h => h.Msv == msv);

            var sinhVien = _context.SinhViens
                .FirstOrDefault(s => s.Msv == msv);

            var yeuCaus = _context.YeuCaus
                .Where(y => y.Msv == msv)
                .OrderByDescending(y => y.NgayGuiYc)
                .ToList();

            var thongBaos = _context.ThongBaos
                .OrderByDescending(t => t.NgayTb)
                .Take(10)
                .ToList();

            var baiDangs = _context.BaiDangs
                .OrderByDescending(b => b.NgayDang)
                .Take(10)
                .ToList();

            var traLois = _context.TraLois
                .OrderByDescending(tl => tl.NgayTl)
                .Take(10)
                .ToList();

            var vm = new HomeViewModel
            {
               SinhVien1 = sinhVien,
                HopDong1 = hopDong,
                YeuCauds = yeuCaus,
                Thongbaods = thongBaos,
                BaiDangds = baiDangs,
                TraLois = traLois,
               
            };

            return View(vm);
        }
    }

}
