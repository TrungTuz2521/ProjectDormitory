using KTX.Entities;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KTX.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashBoardController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public DashBoardController(SinhVienKtxContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel();

            // Tổng số sinh viên đang ở
            vm.TongSinhVien = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                .Select(h => h.Msv)
                .Distinct()
                .CountAsync();

            // Tổng số phòng
            var tongPhong = await _context.Phongs.CountAsync();

            // Phòng đang sử dụng
            var phongDangSuDung = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                .Select(h => h.MaP)
                .Distinct()
                .CountAsync();

            // Tỷ lệ lấp đầy
            vm.TyLeLapDay = tongPhong > 0 ? Math.Round(phongDangSuDung * 100.0 / tongPhong, 2) : 0;

            // Yêu cầu chờ xử lý
            vm.YeuCauChoCLy = await _context.YeuCaus
                .CountAsync(y => y.TrangThaiYc == "Đang xử lý");

            // Doanh thu tháng này
            var thang = DateTime.Now.Month;
            var nam = DateTime.Now.Year;

            var tienPhong = await _context.TienPhongs
                .Where(t => t.NgayTtp.HasValue && t.NgayTtp.Value.Month == thang && t.NgayTtp.Value.Year == nam)
                .SumAsync(t => t.TongTienP ?? 0);

            var tienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.NgayTtdn.HasValue && t.NgayTtdn.Value.Month == thang && t.NgayTtdn.Value.Year == nam && t.TrangThaiTtdn == "Đã thanh toán")
                .SumAsync(t => t.TongTienDn ?? 0);

            vm.TongDoanhThu = tienPhong + tienDienNuoc;

            // Hóa đơn chưa thanh toán
            vm.HoaDonChuaThanhToan = await _context.TienPhongs.CountAsync(t => t.TrangThaiTtp == "Chưa Thanh Toán");

            // Phòng vượt số lượng
            vm.PhongVuotSoLuong = await _context.Phongs.CountAsync(p => p.HienO > p.ToiDaO);

            // Yêu cầu mới nhất
            vm.YeuCauMoiNhat = await _context.YeuCaus
                .Where(y => y.TrangThaiYc == "Đang xử lý" || y.TrangThaiYc == "Chờ xử lý"|| y.TrangThaiYc == "Đã xử lý")
                .OrderByDescending(y => y.NgayGuiYc)
                .Take(5)
                .Select(y => new YeuCauViewModel
                {
                    LoaiYc = y.LoaiYc,
                    NoiDungYc = y.NoiDungYc,
                    TrangThaiYc = y.TrangThaiYc,
                    Msv =y.Msv.ToString(),
                    NgayGuiYc =  y.NgayGuiYc
                })
                .ToListAsync();

            return View(vm);
        }

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
                    tienPhong,
                    tienDienNuoc,
                    tong = tienPhong + tienDienNuoc
                });
            }

            return Json(data);
        }

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
