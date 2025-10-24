using System;
using System.Linq;
using System.Security.Claims;
using KTX.Entities;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KTX.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public DanhGiaController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // =============================
        // HIỂN THỊ DANH SÁCH YÊU CẦU
        // =============================
        public IActionResult Index()
        {
            // Lấy mã sinh viên từ thông tin đăng nhập
            var msv = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(msv))
                return RedirectToAction("Login", "Account");

            var today = DateOnly.FromDateTime(DateTime.Now);

            var danhSach = _context.YeuCaus
                .Include(y => y.DanhGia)
                .Where(yc => yc.Msv.ToString() == msv && yc.NgayGuiYc.HasValue)
                .ToList()
                .Select(yc => new DanhGiaViewModel
                {
                    MaYC = yc.MaYc,
                    LoaiYC = yc.LoaiYc,
                    NoiDungYC = yc.NoiDungYc,
                    NgayGuiYC = yc.NgayGuiYc,
                    TrangThaiYC = yc.TrangThaiYc,
                    MaDG = yc.DanhGia.FirstOrDefault()?.MaDg,
                    NoiDungDG = yc.DanhGia.FirstOrDefault()?.NoiDungDg,
                    DiemDG = int.TryParse(yc.DanhGia.FirstOrDefault()?.DiemDg, out var diem) ? diem : (int?)null,
                    DanhSachDanhGia = yc.DanhGia.Select(dg => new DanhGiaItem
                    {
                        MaDG = dg.MaDg,
                        NoiDungDG = dg.NoiDungDg,
                        DiemDG = dg.DiemDg,
                        NgayGuiDG = dg.NgayGuiDg
                    }).ToList()
                })
                .ToList();

            return View(danhSach);
        }

        // =============================
        // GỬI YÊU CẦU MỚI
        // =============================
        [HttpPost]
        public IActionResult GuiYeuCau(string loaiYc, string noiDungYc)
        {
            if (string.IsNullOrWhiteSpace(loaiYc) || string.IsNullOrWhiteSpace(noiDungYc))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin yêu cầu.";
                return RedirectToAction("Index");
            }

            var msvClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(msvClaim))
            {
                TempData["Error"] = "Không thể xác định sinh viên.";
                return RedirectToAction("Index");
            }

            var yc = new YeuCau
            {
                Msv = int.Parse(msvClaim),
                LoaiYc = loaiYc,
                NoiDungYc = noiDungYc,
                NgayGuiYc = DateOnly.FromDateTime(DateTime.Now),
                TrangThaiYc = "Đang xử lý",
                
            };
            yc.MaYc = _context.YeuCaus.Any() ? _context.YeuCaus.Max(y => y.MaYc) + 1 : 1;
            _context.YeuCaus.Add(yc);
            _context.SaveChanges();

            TempData["Success"] = "Gửi yêu cầu thành công!";
            return RedirectToAction("Index");
        }

        // =============================
        // GỬI ĐÁNH GIÁ CHO YÊU CẦU
        // =============================
        [HttpPost]
        public IActionResult GuiDanhGia(int maYC, string noiDung, int diem)
        {
            if (string.IsNullOrWhiteSpace(noiDung) || diem < 1 || diem > 5)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin đánh giá.";
                return RedirectToAction("Index");
            }

            // Kiểm tra xem yêu cầu có tồn tại không
            var yc = _context.YeuCaus.FirstOrDefault(y => y.MaYc == maYC);
            if (yc == null)
            {
                TempData["Error"] = "Yêu cầu không tồn tại.";
                return RedirectToAction("Index");
            }

            // Nếu đã có đánh giá rồi thì cập nhật
            var existingDG = _context.DanhGia.FirstOrDefault(d => d.MaYc == maYC);
            if (existingDG != null)
            {
                existingDG.NoiDungDg = noiDung;
                existingDG.DiemDg = diem.ToString();
                existingDG.NgayGuiDg = DateOnly.FromDateTime(DateTime.Now);
            }
            else
            {
                var danhGia = new DanhGia
                {
                    MaYc = maYC,
                    NgayGuiDg = DateOnly.FromDateTime(DateTime.Now),
                    NoiDungDg = noiDung,
                    DiemDg = diem.ToString()
                };
                danhGia.MaDg = _context.DanhGia.Any()
     ? _context.DanhGia.Max(d => d.MaDg) + 1
    : 1;
                _context.DanhGia.Add(danhGia);
            }

            _context.SaveChanges();
            TempData["Success"] = "Gửi đánh giá thành công!";
            return RedirectToAction("Index");
        }
    }
}
