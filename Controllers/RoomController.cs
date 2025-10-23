using KTX.Entities;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KTX.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public RoomController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // GET: /Room/
        public async Task<IActionResult> Index()
        {
            var maSinhVienString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maSinhVienString) || !int.TryParse(maSinhVienString, out int maSinhVienInt))
                return RedirectToAction("Login", "Account");

            var sinhVien = await _context.SinhViens
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Msv == maSinhVienInt);

            if (sinhVien == null)
                return NotFound("Không tìm thấy sinh viên.");

            var today = DateTime.Today;

            var hopDongActive = (await _context.HopDongPhongs
                .Include(h => h.MaPNavigation)
                .Where(h => h.Msv == maSinhVienInt
                         && h.TrangThaiHd == "Đăng Kí Thành Công"
                         && h.NgayKetThuc.HasValue)
                .ToListAsync())
                .FirstOrDefault(h => h.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue).Date >= today);

            var vm = new RoomManagementViewModel
            {
                MaSinhVien = maSinhVienString,
                HoTen = sinhVien.HoTen ?? string.Empty,
                AvatarUrl = sinhVien.Avatar ?? string.Empty
            };

            if (hopDongActive == null || hopDongActive.MaPNavigation == null)
            {
                vm.IsContractActive = false;
                vm.CanRequestRoomChange = false;
                vm.CanCancelContract = false;
                return View(vm);
            }

            var phong = hopDongActive.MaPNavigation;

            var roommatesData = await _context.HopDongPhongs
                .AsNoTracking()
                .Include(h => h.MsvNavigation)
                .Where(h => h.MaP == phong.MaP
                    && h.TrangThaiHd == "Đăng Kí Thành Công"
                    && h.NgayKetThuc.HasValue
                    && h.Msv != maSinhVienInt)
                .ToListAsync();

            var roommates = roommatesData
                .Where(h => h.NgayKetThuc!.Value.ToDateTime(TimeOnly.MinValue).Date >= today)
                .Select(h =>
                {
                    var hoTen = h.MsvNavigation?.HoTen ?? "N/A";
                    var initial = "?";

                    if (!string.IsNullOrWhiteSpace(hoTen) && hoTen != "N/A")
                    {
                        var parts = hoTen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            var lastName = parts[parts.Length - 1];
                            if (lastName.Length > 0)
                            {
                                initial = lastName[0].ToString().ToUpper();
                            }
                        }
                    }

                    return new RoommateInfo
                    {
                        MaSinhVien = h.Msv.ToString(),
                        HoTen = hoTen,
                        Initial = initial
                    };
                })
                .ToList();

            // Lấy tiền phòng mới nhất
            var tienPhongGanNhat = await _context.TienPhongs
                .AsNoTracking()
                .Where(t => t.MaHd == hopDongActive.MaHd)
                .OrderByDescending(t => t.HanTtp)
                .FirstOrDefaultAsync();

            vm.RoomFeatures = new List<string> { "WiFi miễn phí", "Nước 24/7", "Điện ổn định", "Nội thất đầy đủ" };

            if (tienPhongGanNhat != null)
            {
                vm.TienPhongHangThang = tienPhongGanNhat.TongTienP;
            }

            vm.MaPhong = phong.MaP.ToString();
            vm.LoaiPhong = hopDongActive.LoaiP;
            vm.SoGiuongHienTai = phong.HienO ?? 0;
            vm.SoGiuongToiDa = phong.ToiDaO ?? 0;
            vm.TinhTrangPhong = phong.TinhTrang ?? string.Empty;
            vm.MaHopDong = hopDongActive.MaHd.ToString();
            vm.NgayBatDau = hopDongActive.NgayBatDau?.ToDateTime(TimeOnly.MinValue);
            vm.NgayKetThuc = hopDongActive.NgayKetThuc?.ToDateTime(TimeOnly.MinValue);
            vm.IsContractActive = vm.NgayKetThuc.HasValue && vm.NgayKetThuc.Value.Date >= today;
            vm.RemainingDays = vm.NgayKetThuc.HasValue ? (vm.NgayKetThuc.Value.Date - today).Days : 0;
            vm.Roommates = roommates;

            vm.CanRequestRoomChange = vm.IsContractActive;
            vm.CanCancelContract = vm.IsContractActive;

            // Lấy danh sách yêu cầu của sinh viên
            vm.DanhSachYeuCau = await _context.YeuCaus
                .AsNoTracking()
                .Where(y => y.Msv == maSinhVienInt)
                .OrderByDescending(y => y.NgayGuiYc)
                .Select(y => new YeuCauInfo
                {
                    MaYc = y.MaYc,
                    LoaiYc = y.LoaiYc ?? string.Empty,
                    NoiDungYc = y.NoiDungYc ?? string.Empty,
                    NgayGuiYc = y.NgayGuiYc,
                    TrangThaiYc = y.TrangThaiYc ?? "Chưa xử lý"
                    // Bỏ NgayXuLy nếu không có trong database
                })
                .ToListAsync();

            return View(vm);
        }

        // === YÊU CẦU ĐỔI PHÒNG - FIXED ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestChangeRoom(string NoiDungYc, string? TargetMaP)
        {
            var maSinhVienString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maSinhVienString) || !int.TryParse(maSinhVienString, out int maSinhVienInt))
            {
                TempData["Error"] = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(NoiDungYc))
            {
                TempData["Error"] = "Vui lòng nhập lý do đổi phòng.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem sinh viên có hợp đồng đang hoạt động không
            var today = DateTime.Today;
            var hopDongActive = (await _context.HopDongPhongs
                .AsNoTracking()
                .Where(h => h.Msv == maSinhVienInt
                         && h.TrangThaiHd == "Đăng Kí Thành Công"
                         && h.NgayKetThuc.HasValue)
                .ToListAsync())
                .FirstOrDefault(h => h.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue).Date >= today);

            if (hopDongActive == null)
            {
                TempData["Error"] = "Bạn không có hợp đồng đang hoạt động để đổi phòng.";
                return RedirectToAction(nameof(Index));
            }

            // Tạo nội dung yêu cầu
            string noiDungFinal = NoiDungYc.Trim();
            if (!string.IsNullOrWhiteSpace(TargetMaP))
            {
                noiDungFinal += $" | Phòng mong muốn: {TargetMaP.Trim()}";
            }

            // Lấy MaYc lớn nhất để tránh duplicate key
            var maxMaYc = await _context.YeuCaus.AnyAsync()
                ? await _context.YeuCaus.MaxAsync(y => y.MaYc)
                : 0;

            var yeuCau = new YeuCau
            {
                MaYc = maxMaYc + 1,
                Msv = maSinhVienInt,
                LoaiYc = "Đổi phòng",
                NoiDungYc = noiDungFinal,
                NgayGuiYc = DateOnly.FromDateTime(DateTime.Today),
                TrangThaiYc = "Đang xử lý"
            };

            try
            {
                _context.YeuCaus.Add(yeuCau);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Yêu cầu đổi phòng đã được gửi thành công! Chúng tôi sẽ xem xét và phản hồi trong vòng 5-7 ngày làm việc.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi khi gửi yêu cầu: {ex.InnerException?.Message ?? ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // === YÊU CẦU HỦY HỢP ĐỒNG ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestCancelRoom(string NoiDungYc)
        {
            var maSinhVienString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maSinhVienString) || !int.TryParse(maSinhVienString, out int maSinhVienInt))
            {
                TempData["Error"] = "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(NoiDungYc))
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy hợp đồng.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra hợp đồng đang hoạt động
            var today = DateTime.Today;
            var hopDongActive = (await _context.HopDongPhongs
                .AsNoTracking()
                .Where(h => h.Msv == maSinhVienInt
                         && h.TrangThaiHd == "Đăng Kí Thành Công"
                         && h.NgayKetThuc.HasValue)
                .ToListAsync())
                .FirstOrDefault(h => h.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue).Date >= today);

            if (hopDongActive == null)
            {
                TempData["Error"] = "Bạn không có hợp đồng đang hoạt động để hủy.";
                return RedirectToAction(nameof(Index));
            }

            // Lấy MaYc lớn nhất để tránh duplicate key
            var maxMaYc = await _context.YeuCaus.AnyAsync()
                ? await _context.YeuCaus.MaxAsync(y => y.MaYc)
                : 0;

            var yeuCau = new YeuCau
            {
                MaYc = maxMaYc + 1,
                Msv = maSinhVienInt,
                LoaiYc = "Hủy hợp đồng",
                NoiDungYc = NoiDungYc.Trim(),
                NgayGuiYc = DateOnly.FromDateTime(DateTime.Today),
                TrangThaiYc = "Đang xử lý"
            };

            try
            {
                _context.YeuCaus.Add(yeuCau);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Yêu cầu hủy hợp đồng đã được gửi thành công. Chúng tôi sẽ liên hệ với bạn sớm nhất.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Có lỗi khi gửi yêu cầu: {ex.InnerException?.Message ?? ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}