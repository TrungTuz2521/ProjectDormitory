using System.Security.Claims;
using KTX.Entities; // Giả định SinhVienKtxContext và các Entities nằm đây
using KTX.Models.ViewModels; // Namespace chứa RulesAnnouncementsViewModel
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Giả định tên Controller là ThongBaoController
namespace KTX.Controllers;

[Authorize]
public class ThongBaoController(SinhVienKtxContext context) : Controller
{
    private readonly SinhVienKtxContext _context = context;

    public async Task<IActionResult> Index()
    {
        // 1. Lấy MSV của người dùng đang đăng nhập
        var maSinhVienString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int maSinhVienInt = 0;

        // Kiểm tra và chuyển đổi MSV
        if (!string.IsNullOrEmpty(maSinhVienString) &&
            !int.TryParse(maSinhVienString, out maSinhVienInt))
        {
            // Xử lý lỗi nếu MSV không hợp lệ (nên chuyển hướng hoặc báo lỗi)
            // Tạm thời bỏ qua nếu không thể Parse, MSV sẽ là 0
        }

        // BƯỚC 2: Khởi tạo ViewModel (đã được fix để tránh NullReferenceException)
        var viewModel = new RulesAnnouncementsViewModel();

        // 3. Truy vấn Thông báo chung (General Announcements)
        // Giả định: Thông báo chung có trường MSV là NULL
        var generalAnnouncements = await _context.ThongBaos
            .Where(tb => tb.Msv == null)
            .OrderByDescending(tb => tb.NgayTb) // Sắp xếp theo ngày mới nhất
            .Select(tb => new RulesAnnouncementsViewModel.ThongBaoItem
            {
                MaTB = tb.MaTb.ToString(),
                TieuDe = tb.TieuDe,
                NoiDung = tb.NoiDung,
                NgayTB = tb.NgayTb.HasValue
             ? tb.NgayTb.Value.ToDateTime(TimeOnly.MinValue) // Chuyển đổi DateOnly sang DateTime
             : DateTime.MinValue
            })
            .ToListAsync();

        // 4. Truy vấn Thông báo cá nhân (My Notifications)
        // Giả định: Thông báo cá nhân có trường MSV khớp với MSV của người dùng đang đăng nhập
        var personalNotifications = await _context.ThongBaos
            .Where(tb => tb.Msv == maSinhVienInt)
            .OrderByDescending(tb => tb.NgayTb) // Sắp xếp theo ngày mới nhất
            .Select(tb => new RulesAnnouncementsViewModel.ThongBaoItem
            {
                MaTB = tb.MaTb.ToString(),
                TieuDe = tb.TieuDe,
                NoiDung = tb.NoiDung,
                NgayTB = tb.NgayTb.HasValue
             ? tb.NgayTb.Value.ToDateTime(TimeOnly.MinValue) // Chuyển đổi DateOnly sang DateTime
             : DateTime.MinValue
            })
            .ToListAsync();

        // 5. Gán dữ liệu vào ViewModel
        viewModel.GeneralAnnouncements = generalAnnouncements;
        viewModel.PersonalNotifications = personalNotifications;

        // 6. Truyền ViewModel đã được khởi tạo và gán dữ liệu
        return View(viewModel);
    }
}