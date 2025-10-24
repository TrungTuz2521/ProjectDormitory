using System;
using System.Linq;
using System.Security.Claims;
using KTX.Entities;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            // Lấy ID người dùng từ claims (giả sử người dùng đã đăng nhập qua ASP.NET Identity hoặc tương tự)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, redirect đến trang login
                return RedirectToAction("Index", "Login"); // Hoặc trang login của bạn
            }

            // Tạo HomeViewModel và lấy dữ liệu trực tiếp từ DB
            var homeViewModel = new HomeViewModel();

            // Lấy 5 thông báo gần nhất
            homeViewModel.Thongbaods = _context.ThongBaos
                .Where(tb => tb.Msv.ToString() == userId  ) // Lọc theo user hoặc thông báo công khai
               
                .Take(5)
                .ToList();

            // Lấy các yêu cầu đang chờ xử lý
            homeViewModel.YeuCauds = _context.YeuCaus
                .Where(y => y.Msv.ToString() == userId && y.TrangThaiYc == "Đang xử lí") 
                .OrderByDescending(y => y.NgayGuiYc)
                .Take(3)
                .ToList();

            // Lấy hợp đồng phòng hiện tại
            homeViewModel.HopDong1 = _context.HopDongPhongs
                .Where(hd => hd.Msv.ToString() == userId && hd.TrangThaiHd == "Đăng Kí Thành Công") // Giả sử có trường IsActive
                .FirstOrDefault();

            //// Lấy hóa đơn điện nước mới nhất
            //homeViewModel.LatestUtilityBill = _context.TienDienNuoc
            //    .Where(tdn => tdn.RoomId == homeViewModel.CurrentContract?.RoomId) // Liên kết với phòng từ hợp đồng
            //    .OrderByDescending(tdn => tdn.BillDate)
            //    .FirstOrDefault();

            // Lấy bài đăng cộng đồng gần đây
            homeViewModel.BaiDangds = _context.BaiDangs
                .OrderByDescending(bd => bd.NgayDang)
                .Take(4)
                .ToList();
            return View(homeViewModel);
        }
    }
}