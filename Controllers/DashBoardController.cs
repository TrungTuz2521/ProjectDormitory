using KTX.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KTX.Models;
using Microsoft.EntityFrameworkCore;

namespace KTX.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public DashBoardController(SinhVienKtxContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Dashboard";

            // Tổng số sinh viên đang ở
            var tongSinhVien = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                .Select(h => h.Msv)
                .Distinct()
                .CountAsync();

            // Tổng số phòng
            var tongPhong = await _context.Phongs.CountAsync();

            // Số phòng đang sử dụng
            var phongDangSuDung = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                .Select(h => h.MaP)
                .Distinct()
                .CountAsync();

            // Tỷ lệ lấp đầy phòng
            var tyLeLapDay = tongPhong > 0 ? (phongDangSuDung * 100.0 / tongPhong) : 0;

            // Số yêu cầu đang chờ xử lý
            var yeuCauChoCLy = await _context.YeuCaus
                .Where(y => y.TrangThaiYc == "Đang xử lý")
                .CountAsync();

            // Tổng doanh thu tháng này
            var thangHienTai = DateTime.Now.Month;
            var namHienTai = DateTime.Now.Year;

            var tongTienPhong = await _context.TienPhongs
                .Where(t => t.NgayTtp.HasValue &&
                           t.NgayTtp.Value.Month == thangHienTai &&
                           t.NgayTtp.Value.Year == namHienTai)
                .SumAsync(t => t.TongTienP ?? 0);

            var tongTienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.NgayTtdn.HasValue &&
                           t.NgayTtdn.Value.Month == thangHienTai &&
                           t.NgayTtdn.Value.Year == namHienTai &&
                           t.TrangThaiTtdn == "Đã thanh toán")
                .SumAsync(t => t.TongTienDn ?? 0);

            var tongDoanhThu = tongTienPhong + tongTienDienNuoc;

            // Hóa đơn chưa thanh toán
            var hoaDonChuaThanhToan = await _context.TienPhongs
                .Where(t => t.TrangThaiTtp == "Chưa Thanh Toán")//&& t.HanTtp < DateTime.Now)
                .CountAsync();

            // Phòng vượt số lượng
            var phongVuotSoLuong = await _context.Phongs
                .Where(p => p.HienO > p.ToiDaO)
                .CountAsync();

            // Danh sách hóa đơn chưa thanh toán gần đây
            //var hoaDonGanDay = await _context.TienPhongs
            //    .Include(t => t.MaHd)
            //    .Where(t => t.TrangThaiHd == null)
            //    .OrderBy(t => t.HanTT)
            //    .Take(5)
            //    .ToListAsync();

            // Yêu cầu mới nhất
            var yeuCauMoiNhat = await _context.YeuCaus
    .Where(y => y.TrangThaiYc == "Đang xử lý")
    .OrderByDescending(y => y.NgayGuiYc)
    .Take(5)
    .ToListAsync(); // ✅ trả về entity đầy đủ

            ViewBag.YeuCauMoiNhat = yeuCauMoiNhat;




            ViewBag.TongSinhVien = tongSinhVien;
            ViewBag.TyLeLapDay = Math.Round(tyLeLapDay, 2);
            ViewBag.YeuCauChoCLy = yeuCauChoCLy;
            ViewBag.TongDoanhThu = tongDoanhThu;
            ViewBag.HoaDonChuaThanhToan = hoaDonChuaThanhToan;
            ViewBag.PhongVuotSoLuong = phongVuotSoLuong;
            //ViewBag.HoaDonGanDay = hoaDonGanDay;
            ViewBag.YeuCauMoiNhat = yeuCauMoiNhat;

            return View();
        }

        // API cho biểu đồ doanh thu 6 tháng gần đây
        [HttpGet]
        public async Task<JsonResult> GetDoanhThuChart()
        {
            var data = new List<object>();
            var now = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                var thang = now.AddMonths(-i);

                var tienPhong = await _context.TienPhongs
                    .Where(t => t.NgayTtp.HasValue &&
                               t.NgayTtp.Value.Month == thang.Month &&
                               t.NgayTtp.Value.Year == thang.Year)
                    .SumAsync(t => t.TongTienP ?? 0);

                var tienDienNuoc = await _context.TienDienNuocs
                    .Where(t => t.NgayTtdn.HasValue &&
                               t.NgayTtdn.Value.Month == thang.Month &&
                               t.NgayTtdn.Value.Year == thang.Year)
                    .SumAsync(t => t.TongTienDn ?? 0);

                data.Add(new
                {
                    thang = $"Tháng {thang.Month}/{thang.Year}",
                    tienPhong = tienPhong,
                    tienDienNuoc = tienDienNuoc,
                    tong = tienPhong + tienDienNuoc
                });
            }

            return Json(data);
        }

        // API cho biểu đồ tỷ lệ phòng
        [HttpGet]
        public async Task<JsonResult> GetTyLePhongChart()
        {
            var tongPhong = await _context.Phongs.CountAsync();
            var phongDangSuDung = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                .Select(h => h.MaP)
                .Distinct()
                .CountAsync();
            var phongTrong = tongPhong - phongDangSuDung;

            return Json(new
            {
                labels = new[] { "Phòng đang sử dụng", "Phòng trống" },
                data = new[] { phongDangSuDung, phongTrong }
            });
        }
    }
}