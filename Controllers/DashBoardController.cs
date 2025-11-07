using KTX.Entities;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var now = DateTime.Now;
            var todayDateOnly = DateOnly.FromDateTime(now);
            var threshold7Days = todayDateOnly.AddDays(-7);
            var futureDate15 = todayDateOnly.AddDays(15);
            var futureDate30 = todayDateOnly.AddDays(30);

            // === THỐNG KÊ CƠ BẢN ===
            vm.TongSinhVien = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                .Select(h => h.Msv)
                .Distinct()
                .CountAsync();

            vm.TongPhong = await _context.Phongs.CountAsync();

            vm.PhongDangSuDung = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                .Select(h => h.MaP)
                .Distinct()
                .CountAsync();

            vm.PhongTrong = await _context.Phongs.CountAsync(p => p.TinhTrang == "Trống");
            vm.PhongDangBaoTri = await _context.Phongs.CountAsync(p => p.TinhTrang == "Đang bảo trì");
            vm.PhongDay = await _context.Phongs.CountAsync(p => p.HienO >= p.ToiDaO);

            vm.TyLeLapDay = vm.TongPhong > 0
                ? (decimal)Math.Round(vm.PhongDangSuDung * 100.0 / vm.TongPhong, 1)
                : 0;

            // === CẢNH BÁO QUAN TRỌNG ===
            vm.HoaDonChuaThanhToan = await _context.TienPhongs
                .CountAsync(t => t.TrangThaiTtp == "Chưa Thanh Toán");

            // Hóa đơn quá hạn: so sánh DateOnly với DateOnly (không dùng ToDateTime trong LINQ)
            vm.HoaDonQuaHan = await _context.TienPhongs
                .CountAsync(t => t.TrangThaiTtp == "Chưa Thanh Toán"
                    && t.HanTtp.HasValue
                    && t.HanTtp.Value < todayDateOnly);

            vm.PhongVuotSoLuong = await _context.Phongs.CountAsync(p => p.HienO > p.ToiDaO);

            // Hợp đồng sắp hết hạn (count) - so sánh DateOnly
            vm.HopDongSapHetHan = await _context.HopDongPhongs
                .CountAsync(h => h.TrangThaiHd == "Đăng Kí Thành Công"
                    && h.NgayKetThuc.HasValue
                    && h.NgayKetThuc.Value >= todayDateOnly
                    && h.NgayKetThuc.Value <= futureDate30);

            // Yêu cầu quá hạn: so sánh ngày gửi <= threshold7Days
            vm.YeuCauQuaHan = await _context.YeuCaus
                .CountAsync(y => (y.TrangThaiYc == "Chờ xử lý" || y.TrangThaiYc == "Đang xử lý")
                    && y.NgayGuiYc.HasValue
                    && y.NgayGuiYc.Value <= threshold7Days);

            // === THỐNG KÊ SINH VIÊN ===
            vm.SinhVienMoi = await _context.HopDongPhongs
                .CountAsync(h => h.NgayBatDau.HasValue
                    && h.NgayBatDau.Value.Month == now.Month
                    && h.NgayBatDau.Value.Year == now.Year);

            vm.SinhVienSapHetHan = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công"
                    && h.NgayKetThuc.HasValue
                    && h.NgayKetThuc.Value >= todayDateOnly
                    && h.NgayKetThuc.Value <= futureDate15)
                .Select(h => h.Msv)
                .Distinct()
                .CountAsync();

            // Sinh viên nợ tiền: dùng DateOnly so sánh
            vm.SinhVienNoTien = await _context.TienPhongs
                .Where(t => t.TrangThaiTtp == "Chưa Thanh Toán"
                    && t.HanTtp.HasValue
                    && t.HanTtp.Value < todayDateOnly)
                .Join(_context.HopDongPhongs,
                    t => t.MaHd,
                    h => h.MaHd,
                    (t, h) => h.Msv)
                .Distinct()
                .CountAsync();

            // === THỐNG KÊ TÀI CHÍNH ===
            var thang = now.Month;
            var nam = now.Year;

            vm.TienPhongThangNay = await _context.TienPhongs
                .Where(t => t.NgayTtp.HasValue
                    && t.NgayTtp.Value.Month == thang
                    && t.NgayTtp.Value.Year == nam)
                .SumAsync(t => (decimal)(t.TongTienP ?? 0));

            vm.TienDienNuocThangNay = await _context.TienDienNuocs
                .Where(t => t.NgayTtdn.HasValue
                    && t.NgayTtdn.Value.Month == thang
                    && t.NgayTtdn.Value.Year == nam
                    && t.TrangThaiTtdn == "Đã thanh toán")
                .SumAsync(t => (decimal)(t.TongTienDn ?? 0));

            vm.TongDoanhThu = vm.TienPhongThangNay + vm.TienDienNuocThangNay;

            vm.TongTienConNo = await _context.TienPhongs
                .Where(t => t.TrangThaiTtp == "Chưa Thanh Toán")
                .SumAsync(t => (decimal)(t.TongTienP ?? 0));

            var tongHoaDon = await _context.TienPhongs.CountAsync();
            var hoaDonDaTT = await _context.TienPhongs.CountAsync(t => t.TrangThaiTtp == "Đã Thanh Toán");
            vm.TyLeThanhToan = tongHoaDon > 0 ? (int)Math.Round(hoaDonDaTT * 100.0 / tongHoaDon) : 0;



    

            // === THỐNG KÊ YÊU CẦU ===
            vm.YeuCauChoXuLyCount = await _context.YeuCaus.CountAsync(y => y.TrangThaiYc == "Chờ xử lý");
            vm.YeuCauDangXuLyCount = await _context.YeuCaus.CountAsync(y => y.TrangThaiYc == "Đang xử lý");
            vm.YeuCauDaXuLyThangNay = await _context.YeuCaus
                .CountAsync(y => y.TrangThaiYc == "Đã xử lý"
                    && y.NgayXuLy.HasValue
                    && y.NgayXuLy.Value.Month == thang
                    && y.NgayXuLy.Value.Year == nam);

            // === DANH SÁCH CẦN XỬ LÝ ===
            // Lấy dữ liệu cần thiết (DateOnly được trả về) rồi map sang ViewModel trên memory
            var rawYeuCau = await _context.YeuCaus
                .Where(y => y.TrangThaiYc == "Chờ xử lý" || y.TrangThaiYc == "Đang xử lý")
                .OrderBy(y => y.NgayGuiYc.HasValue && y.NgayGuiYc.Value <= threshold7Days ? 0 : 1) // ưu tiên quá hạn
                .ThenByDescending(y => y.NgayGuiYc)
                .Take(5)
                .Select(y => new
                {
                    y.MaYc,
                    y.LoaiYc,
                    y.NoiDungYc,
                    y.TrangThaiYc,
                    y.Msv,
                    HoTen = y.MsvNavigation != null ? y.MsvNavigation.HoTen : null,
                    NgayGuiYc = y.NgayGuiYc // DateOnly?
                })
                .ToListAsync();

            vm.YeuCauMoiNhat = rawYeuCau
                .Select(y => new YeuCauViewModel
                {
                    MaYc = y.MaYc.ToString(),
                    LoaiYc = y.LoaiYc ?? "",
                    NoiDungYc = y.NoiDungYc ?? "",
                    TrangThaiYc = y.TrangThaiYc ?? "",
                    Msv = y.Msv.ToString(),
                    HoTen = y.HoTen ?? "",
                    NgayGuiYc = y.NgayGuiYc.HasValue ? y.NgayGuiYc.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    SoNgayCho = y.NgayGuiYc.HasValue ? (int)(todayDateOnly.DayNumber - y.NgayGuiYc.Value.DayNumber) : 0
                })
                .ToList();

            // HÓA ĐƠN QUÁ HẠN (TOP 5) - lấy minimal fields then map in-memory
            var rawHoaDonQuaHan = await _context.TienPhongs
                .Where(t => t.TrangThaiTtp == "Chưa Thanh Toán"
                    && t.HanTtp.HasValue
                    && t.HanTtp.Value < todayDateOnly)
                .OrderBy(t => t.HanTtp)
                .Take(5)
                .Select(t => new
                {
                    t.MaHd,
                    MaP = t.MaHdNavigation != null ? t.MaHdNavigation.MaP : (int?)null,
                    Msv = t.MaHdNavigation != null ? t.MaHdNavigation.Msv : (int?)null,
                    HoTen = t.MaHdNavigation != null && t.MaHdNavigation.MsvNavigation != null
                        ? t.MaHdNavigation.MsvNavigation.HoTen : null,
                    t.TongTienP,
                    HanTtp = t.HanTtp
                })
                .ToListAsync();

            vm.DanhSachHoaDonQuaHan = rawHoaDonQuaHan
                .Select(t => new HoaDonQuaHanViewModel
                {
                    MaHD = t.MaHd.ToString(),
                    Msv = t.Msv?.ToString() ?? "",
                    HoTen = t.HoTen ?? "",
                    MaPhong = t.MaP.HasValue ? t.MaP.Value.ToString() : "",
                    LoaiHoaDon = "Tiền phòng",
                    SoTien = (decimal)(t.TongTienP ?? 0),
                    NgayHetHan = t.HanTtp.HasValue ? t.HanTtp.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                    SoNgayQuaHan = t.HanTtp.HasValue ? (int)(todayDateOnly.DayNumber - t.HanTtp.Value.DayNumber) : 0
                })
                .ToList();

            // PHÒNG CẢNH BÁO (take 5)
            vm.PhongCanhBao = await _context.Phongs
                .Where(p => p.HienO > p.ToiDaO || p.TinhTrang == "Đang bảo trì")
                .Take(5)
                .Select(p => new PhongCanhBaoViewModel
                {
                    MaPhong = p.MaP.ToString(),
                    HienO = p.HienO ?? 0,
                    ToiDa = p.ToiDaO ?? 0,
                    TinhTrang = p.TinhTrang ?? "",
                    LyDoCanhBao = p.HienO > p.ToiDaO ? "Vượt số lượng" : "Đang bảo trì"
                })
                .ToListAsync();

            // HỢP ĐỒNG SẮP HẾT HẠN (TOP 5) - map in-memory for DateTime fields
            var rawHopDong = await _context.HopDongPhongs
                .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công"
                    && h.NgayKetThuc.HasValue
                    && h.NgayKetThuc.Value >= todayDateOnly
                    && h.NgayKetThuc.Value <= futureDate30)
                .OrderBy(h => h.NgayKetThuc)
                .Take(5)
                .Select(h => new
                {
                    h.MaHd,
                    h.Msv,
                    HoTen = h.MsvNavigation != null ? h.MsvNavigation.HoTen : null,
                    h.MaP,
                    NgayBatDau = h.NgayBatDau,
                    NgayKetThuc = h.NgayKetThuc
                })
                .ToListAsync();

            vm.DanhSachHopDongSapHetHan = rawHopDong
                .Select(h => new HopDongSapHetHanViewModel
                {
                    MaHD = h.MaHd.ToString(),
                    Msv = h.Msv.ToString(),
                    HoTen = h.HoTen ?? "",
                    MaPhong = h.MaP.ToString(),
                    NgayBatDau = h.NgayBatDau.HasValue ? h.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                    NgayKetThuc = h.NgayKetThuc.HasValue ? h.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                    SoNgayConLai = h.NgayKetThuc.HasValue ? h.NgayKetThuc.Value.DayNumber - todayDateOnly.DayNumber : 0
                })
                .ToList();

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
                    .SumAsync(t => (double)(t.TongTienP ?? 0));

                var tienDienNuoc = await _context.TienDienNuocs
                    .Where(t => t.NgayTtdn.HasValue &&
                               t.NgayTtdn.Value.Month == thang.Month &&
                               t.NgayTtdn.Value.Year == thang.Year)
                    .SumAsync(t => (double)(t.TongTienDn ?? 0));

                data.Add(new
                {
                    thang = $"T{thang.Month}/{thang.Year}",
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
            var phongDangSuDung = await _context.Phongs.CountAsync(p => p.TinhTrang == "Đang sử dụng");
            var phongTrong = await _context.Phongs.CountAsync(p => p.TinhTrang == "Trống");
            var phongBaoTri = await _context.Phongs.CountAsync(p => p.TinhTrang == "Đang bảo trì");
            var phongDay = await _context.Phongs.CountAsync(p => p.TinhTrang == "Đầy");

            return Json(new
            {
                labels = new[] { "Đang sử dụng", "Trống", "Đầy", "Bảo trì" },
                data = new[] { phongDangSuDung, phongTrong, phongDay, phongBaoTri }
            });
        }

        [HttpGet]
        public async Task<JsonResult> GetYeuCauTheoLoaiChart()
        {
            var yeuCauTheoLoai = await _context.YeuCaus
                .Where(y => y.NgayGuiYc.HasValue
                    && y.NgayGuiYc.Value.Month == DateTime.Now.Month
                    && y.NgayGuiYc.Value.Year == DateTime.Now.Year)
                .GroupBy(y => y.LoaiYc)
                .Select(g => new { loai = g.Key, count = g.Count() })
                .ToListAsync();

            return Json(new
            {
                labels = yeuCauTheoLoai.Select(y => y.loai).ToArray(),
                data = yeuCauTheoLoai.Select(y => y.count).ToArray()
            });
        }
    }
}