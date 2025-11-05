using KTX.Entities;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KTX.Controllers
{
    [Authorize]
    public class YCauKNaiController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public YCauKNaiController(SinhVienKtxContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ds = await _context.YeuCaus
                .Include(y => y.MsvNavigation)
                .OrderByDescending(y => y.NgayGuiYc)
                .Select(y => new
                {
                    YC = y,
                    SinhVien = y.MsvNavigation,
                    // CHỈ LẤY MaP – NHANH & AN TOÀN
                    MaP = _context.HopDongPhongs
                        .Where(h => h.Msv == y.Msv && h.TrangThaiHd == "Đăng Kí Thành Công" || h.TrangThaiHd == "Đã thanh toán")
                        .OrderByDescending(h => h.NgayBatDau)
                        .Select(h => h.MaP)
                        .FirstOrDefault()
                })
                .Select(temp => new YCauKNaiViewModel
                {
                    MaYC = temp.YC.MaYc,
                    MSV = temp.YC.Msv.ToString(),
                    HoTen = temp.SinhVien.HoTen,
                    LoaiYC = temp.YC.LoaiYc ?? "",
                    NoiDungYC = temp.YC.NoiDungYc ?? "",
                    TrangThai = temp.YC.TrangThaiYc ?? "Chờ xử lý",
                    NgayGuiYC = temp.YC.NgayGuiYc,
                    Phong = temp.MaP != 0
                        ? $"Phòng {temp.MaP}"
                        : "Chưa có phòng"

                })

                .ToListAsync();

            return View(ds);
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThaiMoi)
        {
            var yc = await _context.YeuCaus.FindAsync(id);
            if (yc == null) return Json(new { success = false });

            yc.TrangThaiYc = trangThaiMoi;
            await _context.SaveChangesAsync();

            return Json(new { success = true, trangThai = trangThaiMoi });
        }

        public async Task<IActionResult> ThongKe()
        {
            var stats = new
            {
                Tong = await _context.YeuCaus.CountAsync(),
                ChoXuLy = await _context.YeuCaus.CountAsync(y => y.TrangThaiYc == "Chờ xử lý"),
                DangXuLy = await _context.YeuCaus.CountAsync(y => y.TrangThaiYc == "Đang xử lý"),
                DaXuLy = await _context.YeuCaus.CountAsync(y => y.TrangThaiYc == "Đã xử lý")
            };

            return View(stats);
        }
    }
}