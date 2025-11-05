using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KTX.Entities;
using KTX.Models;

namespace KTX.Controllers
{
    public class TaiChinhController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public TaiChinhController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // GET: TaiChinh/Index
        public IActionResult Index(string loai = "phong", int? dotTtdn = null)
        {
            var viewModel = new TaiChinhViewModel
            {
                LoaiHienThi = loai,
                DotTtdnHienTai = dotTtdn
            };

            // Lấy danh sách đợt điện nước
            viewModel.DanhSachDotTtdn = _context.TienDienNuocs
                .Where(t => t.DotTtdn.HasValue)
                .Select(t => t.DotTtdn.Value)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            // === TIỀN PHÒNG: THEO PHÒNG + SINH VIÊN ===
            if (loai == "phong")
            {
                var phongData = _context.Phongs
                    .Include(p => p.HopDongPhongs)
                        .ThenInclude(h => h.MsvNavigation)
                    .Include(p => p.HopDongPhongs)
                        .ThenInclude(h => h.TienPhongs)
                    .Where(p => p.HopDongPhongs.Any(h => h.TrangThaiHd == "Đăng Kí Thành Công"))
                    .Select(p => new PhongTienPhongDetail
                    {
                        MaP = p.MaP,
                        SinhViens = p.HopDongPhongs
                            .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                            .Select(h => new SinhVienTienPhong
                            {
                                Msv = h.Msv.ToString(),
                                HoTen = h.MsvNavigation.HoTen,
                                TienPhong = h.TienPhongs
                                    .OrderByDescending(t => t.HanTtp)
                                    .FirstOrDefault()
                            }).ToList()
                    })
                    .OrderBy(p => p.MaP)
                    .ToList();

                viewModel.ChiTietTienPhongTheoPhong = phongData;
            }

            // === TIỀN ĐIỆN NƯỚC: THEO PHÒNG + ĐỢT + SINH VIÊN ===
            else if (loai == "diennuoc")
            {
                var query = _context.TienDienNuocs.AsQueryable();

                query = query.Include(t => t.MaPNavigation)
                                .ThenInclude(p => p.HopDongPhongs)
                                .ThenInclude(h => h.MsvNavigation);

                if (dotTtdn.HasValue)
                    query = query.Where(t => t.DotTtdn == dotTtdn.Value);

                var dienNuocData = query
                    .GroupBy(t => new { t.MaP, t.DotTtdn })
                    .Select(g => new PhongTienDienNuocDetail
                    {
                        MaP = g.Key.MaP,
                        DotTtdn = g.Key.DotTtdn,
                        TongTien = g.Sum(t => t.TongTienDn ?? 0),
                        TrangThai = g.All(t => t.TrangThaiTtdn == "Đã thanh toán") ? "Đã thanh toán" :
                                   g.Any(t => t.TrangThaiTtdn == "Đã thanh toán") ? "Thanh toán 1 phần" : "Chưa thanh toán",
                        SinhViens = g.First().MaPNavigation.HopDongPhongs
                            .Where(h => h.TrangThaiHd == "Đang hiệu lực")
                            .Select(h => new SinhVienTienDienNuoc
                            {
                                Msv = h.Msv.ToString()  ,
                                HoTen = h.MsvNavigation.HoTen,
                                TienDienNuoc = g.FirstOrDefault(x => x.MaP == h.MaP)
                            }).ToList()
                    })
                    .OrderByDescending(g => g.DotTtdn)
                    .ThenBy(g => g.MaP)
                    .ToList();

                viewModel.ChiTietDienNuocTheoPhong = dienNuocData;
            }

            return View(viewModel);
        }

        // Chi tiết tiền phòng
        public IActionResult ChiTietTienPhong(int id)
        {
            var tienPhong = _context.TienPhongs
                .Include(t => t.MaHdNavigation)
                    .ThenInclude(h => h.MsvNavigation)
                .Include(t => t.MaHdNavigation)
                    .ThenInclude(h => h.MaPNavigation)
                .FirstOrDefault(t => t.MaHdp == id);

            if (tienPhong == null) return NotFound();
            return View(tienPhong);
        }

        // Chi tiết tiền điện nước
        public IActionResult ChiTietDienNuoc(int id)
        {
            var tienDienNuoc = _context.TienDienNuocs
                .Include(t => t.MaPNavigation)
                    .ThenInclude(p => p.HopDongPhongs)
                    .ThenInclude(h => h.MsvNavigation)
                .FirstOrDefault(t => t.MaHddn == id);

            if (tienDienNuoc == null) return NotFound();
            return View(tienDienNuoc);
        }

        // Thanh toán tiền phòng
        public IActionResult ThanhToanTienPhong(int id)
        {
            var tienPhong = _context.TienPhongs
                .Include(t => t.MaHdNavigation)
                    .ThenInclude(h => h.MsvNavigation)
                .FirstOrDefault(t => t.MaHdp == id);

            if (tienPhong == null) return NotFound();
            return View(tienPhong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThanhToanTienPhong(int id, string phuongThucTt)
        {
            var tienPhong = _context.TienPhongs.Find(id);
            if (tienPhong == null) return NotFound();

            tienPhong.TrangThaiTtp = "Đã thanh toán";
            tienPhong.NgayTtp = DateOnly.FromDateTime(DateTime.Now);
            _context.Update(tienPhong);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thanh toán tiền phòng thành công!";
            return RedirectToAction(nameof(Index), new { loai = "phong" });
        }

        // Thanh toán điện nước
        public IActionResult ThanhToanDienNuoc(int id)
        {
            var tienDienNuoc = _context.TienDienNuocs
                .Include(t => t.MaPNavigation)
                .FirstOrDefault(t => t.MaHddn == id);

            if (tienDienNuoc == null) return NotFound();
            return View(tienDienNuoc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThanhToanDienNuoc(int id, string phuongThucTt)
        {
            var tienDienNuoc = _context.TienDienNuocs.Find(id);
            if (tienDienNuoc == null) return NotFound();

            tienDienNuoc.TrangThaiTtdn = "Đã thanh toán";
            tienDienNuoc.NgayTtdn = DateOnly.FromDateTime(DateTime.Now);
            _context.Update(tienDienNuoc);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thanh toán điện nước thành công!";
            return RedirectToAction(nameof(Index), new
            {
                loai = "diennuoc",
                dotTtdn = tienDienNuoc.DotTtdn ?? (int?)null
            });
        }

        // Thống kê
        public IActionResult ThongKe()
        {
            var thongKe = new ThongKeTaiChinhViewModel
            {
                TongTienPhongChuaThu = _context.TienPhongs
                    .Where(t => t.TrangThaiTtp != "Đã thanh toán")
                    .Sum(t => t.TongTienP ?? 0),
                TongTienPhongDaThu = _context.TienPhongs
                    .Where(t => t.TrangThaiTtp == "Đã thanh toán")
                    .Sum(t => t.TongTienP ?? 0),
                TongTienDienNuocChuaThu = _context.TienDienNuocs
                    .Where(t => t.TrangThaiTtdn != "Đã thanh toán")
                    .Sum(t => t.TongTienDn ?? 0),
                TongTienDienNuocDaThu = _context.TienDienNuocs
                    .Where(t => t.TrangThaiTtdn == "Đã thanh toán")
                    .Sum(t => t.TongTienDn ?? 0),
                SoHoaDonPhongChuaThu = _context.TienPhongs
                    .Count(t => t.TrangThaiTtp != "Đã thanh toán"),
                SoHoaDonDienNuocChuaThu = _context.TienDienNuocs
                    .Count(t => t.TrangThaiTtdn != "Đã thanh toán")
            };

            thongKe.TongDoanhThu = thongKe.TongTienPhongDaThu + thongKe.TongTienDienNuocDaThu;
            thongKe.TongCongNo = thongKe.TongTienPhongChuaThu + thongKe.TongTienDienNuocChuaThu;

            return View(thongKe);
        }
    }
}