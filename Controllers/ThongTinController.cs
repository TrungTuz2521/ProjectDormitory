using System.Diagnostics;
using System.Security.Claims;
using KTX.Entities;
using KTX.Models.ViewModels;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// FIX: Áp dụng File-Scoped Namespace (C# 10)
namespace KTX.Controllers;

[Authorize]
// FIX: Sử dụng Primary Constructor để đơn giản hóa việc khởi tạo _context
public class ThongTinController(SinhVienKtxContext context) : Controller
{
    // FIX: Không cần khai báo lại _context nếu dùng Primary Constructor
    private readonly SinhVienKtxContext _context = context;

    public async Task<IActionResult> ThongTinCaNhan()
    {
        // 1. Lấy MSV từ Claim và chuyển sang int
        var maSinhVienString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(maSinhVienString) ||
            !int.TryParse(maSinhVienString, out int maSinhVienInt))
        {
            return RedirectToAction("Login", "Account");
        }

        // 2. Truy vấn sinh viên với HopDong và Phong
        var sinhVien = await _context.SinhViens
            .Include(s => s.HopDongPhongs)
                .ThenInclude(hd => hd.MaPNavigation) // Navigation property đến Phong
            .FirstOrDefaultAsync(s => s.Msv == maSinhVienInt);

        if (sinhVien == null)
        {
            return NotFound("Không tìm thấy hồ sơ sinh viên.");
        }

        // FIX: Khai báo DateOnly và TimeOnly ra ngoài LINQ query để tránh lỗi EF Core
        var today = DateOnly.FromDateTime(DateTime.Today);
        var minTime = TimeOnly.MinValue; // Khắc phục lỗi "optional arguments" trong ToDateTime

        // 3. Lấy hợp đồng đang Active
        var hopDongActive = sinhVien.HopDongPhongs?
            .Where(hd => hd.TrangThaiHd == "Đăng Kí Thành Công" &&
                         hd.NgayKetThuc.HasValue &&
                         today <= hd.NgayKetThuc.Value)
            .OrderByDescending(hd => hd.NgayBatDau)
            .FirstOrDefault();

        // 4. Nếu không có hợp đồng hoặc phòng
        if (hopDongActive == null || hopDongActive.MaPNavigation == null)
        {
            // FIX: Sử dụng new() đơn giản hóa (Target-typed new)
            return View(new ThongTinCaNhanViewModel
            {
                MaSinhVien = maSinhVienString,
                HoTen = sinhVien.HoTen ?? string.Empty,
                NgaySinh = sinhVien.NgaySinh.ToDateTime(TimeOnly.MinValue),
                GioiTinh = sinhVien.GioiTinh,
                DienThoai = sinhVien.Sdt ?? string.Empty,
                Email = sinhVien.Email ?? string.Empty,
                Khoa = sinhVien.Khoa,
                AvatarUrl = sinhVien.Avatar,
                BanCungPhong = []
            });
        }

        var phong = hopDongActive.MaPNavigation;

        // 5. Lấy danh sách bạn cùng phòng

        // BƯỚC 1: Truy vấn và lấy dữ liệu cần thiết từ Database (Database Query)
        // Lấy về các HopDongPhong thỏa mãn điều kiện, bao gồm HoTen và Msv.
        var roommatesData = await _context.HopDongPhongs
            .Where(hd => hd.MaP == phong.MaP &&
                         hd.TrangThaiHd == "Đăng Kí Thành Công" &&
                         hd.NgayKetThuc.HasValue &&
                         today <= hd.NgayKetThuc.Value &&
                         hd.Msv != maSinhVienInt)
            .Select(hd => new // Sử dụng kiểu ẩn danh để chỉ lấy dữ liệu cần thiết
            {
                // Lấy HoTen và Msv (mà EF Core có thể dịch)
                HoTen = hd.MsvNavigation.HoTen,
                Msv = hd.Msv
            })
            .ToListAsync(); // Chạy truy vấn SQL và kéo dữ liệu về bộ nhớ.

        // BƯỚC 2: Xử lý logic phức tạp trong bộ nhớ (In-Memory Processing)
        // Dùng LINQ to Objects (trên List đã lấy về) để thực hiện các thao tác chuỗi phức tạp.
        var roommates = roommatesData.Select(r => new RoommateInfo
        {
            // Logic xử lý chuỗi phức tạp (Split, Last, Substring, ToUpper) hoạt động tốt trên bộ nhớ
            Initial = !string.IsNullOrEmpty(r.HoTen) &&
                      r.HoTen.Contains(' ')
                        ? r.HoTen.Split(' ').Last().Substring(0, 1).ToUpper()
                        : "?",
            HoTen = r.HoTen ?? "N/A",
            MaSinhVien = r.Msv.ToString()
        }).ToList();

        // Thay thế biến 'roommates' ban đầu bằng 'roommates' mới này trong ViewModel.

        // 6. Tạo ViewModel
        var viewModel = new ThongTinCaNhanViewModel // FIX: Có thể dùng new()
        {
            // Thông tin sinh viên
            MaSinhVien = maSinhVienString,
            HoTen = sinhVien.HoTen ?? string.Empty,
            NgaySinh = sinhVien.NgaySinh.ToDateTime(TimeOnly.MinValue),
            GioiTinh = sinhVien.GioiTinh,
            DienThoai = sinhVien.Sdt ?? string.Empty,
            Email = sinhVien.Email ?? string.Empty,
            Khoa = sinhVien.Khoa,
            AvatarUrl = sinhVien.Avatar,

            // Thông tin phòng
            MaPhong = phong.MaP.ToString(),
            LoaiPhong = hopDongActive.LoaiP,
            TinhTrangPhong = phong.TinhTrang,
            SoGiuongHienTai = phong.HienO ?? 0,
            SoGiuongToiDa = phong.ToiDaO ?? 0,

            // Thông tin hợp đồng
            // FIX: Chuyển đổi MaHd (int) sang string
            MaHopDong = hopDongActive.MaHd.ToString(),
            NgayBatDau = hopDongActive.NgayBatDau.HasValue
                // FIX: Dùng biến minTime để tránh lỗi "optional arguments"
                ? hopDongActive.NgayBatDau.Value.ToDateTime(minTime)
                : DateTime.MinValue,
            NgayKetThuc = hopDongActive.NgayKetThuc.HasValue
                // FIX: Dùng biến minTime để tránh lỗi "optional arguments"
                ? hopDongActive.NgayKetThuc.Value.ToDateTime(minTime)
                : DateTime.MinValue,

            // Danh sách bạn cùng phòng
            BanCungPhong = roommates
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var maSinhVienString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(maSinhVienString) ||
            !int.TryParse(maSinhVienString, out int maSinhVienInt))
        {
            return RedirectToAction("Login", "Account");
        }

        var sinhVien = await _context.SinhViens
            .FirstOrDefaultAsync(s => s.Msv == maSinhVienInt);

        if (sinhVien == null)
        {
            return NotFound();
        }

        // FIX: Sử dụng new() đơn giản hóa
        var viewModel = new ThongTinCaNhanViewModel
        {
            MaSinhVien = maSinhVienString,
            HoTen = sinhVien.HoTen ?? string.Empty,
            NgaySinh = sinhVien.NgaySinh.ToDateTime(TimeOnly.MinValue),
            GioiTinh = sinhVien.GioiTinh,
            DienThoai = sinhVien.Sdt ?? string.Empty,
            Email = sinhVien.Email ?? string.Empty,
            Khoa = sinhVien.Khoa
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ThongTinCaNhanViewModel model)
    {
        if (!ModelState.IsValid)
        {
            
            return View(model);
        }

        var maSinhVienString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(maSinhVienString) ||
            !int.TryParse(maSinhVienString, out int maSinhVienInt))
        {
            return RedirectToAction("Login", "Account");
        }

        var sinhVien = await _context.SinhViens
            .FirstOrDefaultAsync(s => s.Msv == maSinhVienInt);

        if (sinhVien == null)
        {
            return NotFound();
        }

        // Cập nhật thông tin
        // Chỉ gán 2 field cần thay đổi
        sinhVien.Sdt = model.DienThoai?.Trim() ;
        sinhVien.Email = model.Email?.Trim();

        try
        {
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction(nameof(ThongTinCaNhan));
        }
        catch (DbUpdateException ex)
        {
            TempData["Error"] = $"Lỗi khi lưu dữ liệu: {ex.InnerException?.Message ?? ex.Message}";
            return View(model);
        }


    }
}