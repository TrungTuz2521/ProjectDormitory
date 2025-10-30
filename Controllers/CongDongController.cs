using System.Security.Claims;
using KTX.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KTX.Controllers
{
    public class CongDongController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public CongDongController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách bài đăng
        public async Task<IActionResult> Index()
        {
            var baiDangs = await _context.BaiDangs
                .Include(b => b.TraLois)
                .OrderByDescending(b => b.NgayDang)
                .ToListAsync();

            return View(baiDangs);
        }

        // Thêm bài đăng
        [HttpPost]
        public async Task<IActionResult> ThemBaiDang(string noiDung)
        {
            if (string.IsNullOrWhiteSpace(noiDung))
                return RedirectToAction("Index");

            var baiDang = new BaiDang
            {
                NoiDungBd = noiDung,
                Msv = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                NgayDang = DateOnly.FromDateTime(DateTime.Now)
            };

            baiDang.MaBd = _context.BaiDangs.Any() ? _context.BaiDangs.Max(y => y.MaBd) + 1 : 1; 
            _context.BaiDangs.Add(baiDang);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Thêm bình luận
        [HttpPost]
        public async Task<IActionResult> ThemTraLoi(int maBD, string noiDungTL)
        {
            if (string.IsNullOrWhiteSpace(noiDungTL))
                return RedirectToAction("Index");

            var traLoi = new TraLoi
            {
                MaBd = maBD,
                NoiDungTl = noiDungTL,
                Msv = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                NgayTl = DateOnly.FromDateTime(DateTime.Now)
            };
            traLoi.MaTl = _context.TraLois.Any() ? _context.TraLois.Max(y => y.MaTl) + 1 : 1;
            _context.TraLois.Add(traLoi);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
