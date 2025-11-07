using KTX.Entities;
using KTX.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KTX.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QLThongBaoController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public QLThongBaoController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // GET: Danh sách
        public async Task<IActionResult> Index()
        {
            var thongBaos = await _context.ThongBaos
    .Include(t => t.MsvNavigation)
    .OrderByDescending(t => t.NgayTb)
    .ThenByDescending(t => t.MaTb)
    .ToListAsync();

            // XỬ LÝ VIEW MODEL
            var viewModel = new ThongBaoListViewModel
            {
                ThongBaoChung = thongBaos
                    .Where(t => t.Msv == 0)
                    .Select(t => new ThongBaoViewModel
                    {
                        MaTB = t.MaTb,
                        MSV = 0,
                       
                        TieuDe = t.TieuDe ?? "",
                        NoiDung = t.NoiDung ?? "",
                        NgayTB = t.NgayTb,
                        
                    })
                    .ToList(),

                ThongBaoRieng = thongBaos
                    .Where(t => t.Msv > 0)
                    .Select(t => new ThongBaoViewModel
                    {
                        MaTB = t.MaTb,
                        MSV = t.Msv,
                        HoTen = t.MsvNavigation?.HoTen ?? "Không xác định",
                        TieuDe = t.TieuDe ?? "",
                        NoiDung = t.NoiDung ?? "",
                        NgayTB = t.NgayTb,
                        
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: Tạo
        public async Task<IActionResult> Create()
        {
            var model = new QLThongBaoViewModel
            {
                DanhSachSinhVien = await _context.SinhViens
                    .Select(s => new SinhVienSelectViewModel
                    {
                        MSV = s.Msv,
                        HoTen = s.HoTen,
                        Email = s.Email
                    })
                    .OrderBy(s => s.HoTen)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QLThongBaoViewModel model)
        {
            Console.WriteLine($"[POST] MSV: '{model.MSV}' | Tiêu đề: '{model.TieuDe}'");

            if (!ModelState.IsValid)
            {
                await LoadSinhVienList(model);
                return View(model);
            }

            try
            {
                var tb = new ThongBao
                {
                    TieuDe = model.TieuDe?.Trim(),
                    NoiDung = model.NoiDung?.Trim(),
                    NgayTb = DateOnly.FromDateTime(DateTime.Now)
                };
                string msvInput = (model.MSV ?? "").Trim();
                // XỬ LÝ MSV
                if (string.IsNullOrWhiteSpace(msvInput) || msvInput == "0")
                {
                    tb.Msv = 0;
                    Console.WriteLine("[INFO] Gửi CHUNG - Msv = 0");
                }
                else if (int.TryParse(model.MSV.Trim(), out int msv) && msv > 0)
                {
                    var sv = await _context.SinhViens.AnyAsync(s => s.Msv == msv);
                    if (!sv)
                    {
                        ModelState.AddModelError("MSV", "Sinh viên không tồn tại.");
                        await LoadSinhVienList(model);
                        return View(model);
                    }
                    tb.Msv = msv;
                    Console.WriteLine($"[INFO] Gửi RIÊNG - Msv = {msv}");
                }
                else
                {
                    ModelState.AddModelError("MSV", "MSV không hợp lệ.");
                    await LoadSinhVienList(model);
                    return View(model);
                }
                tb.MaTb = _context.ThongBaos.Any() ? _context.ThongBaos.Max(tb => tb.MaTb) + 1 : 1;
                _context.ThongBaos.Add(tb);
                int result = await _context.SaveChangesAsync();

                Console.WriteLine($"[DB] Đã lưu: {result} bản ghi");

                if (result > 0)
                {
                    TempData["Success"] = "Gửi thông báo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Lưu thất bại!";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI] {ex.Message}");
                TempData["Error"] = "Lỗi hệ thống!";
                await LoadSinhVienList(model);
                return View(model);
            }
        }
        private async Task LoadSinhVienList(QLThongBaoViewModel model)
        {
            model.DanhSachSinhVien = await _context.SinhViens
                .Select(s => new SinhVienSelectViewModel
                {
                    MSV = s.Msv,
                    HoTen = s.HoTen,
                    Email = s.Email
                })
                .OrderBy(s => s.HoTen)
                .ToListAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]  // nên giữ để bảo mật
        public async Task<IActionResult> Delete(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null)
                return RedirectToAction(nameof(Index)); // hoặc thông báo lỗi

            _context.ThongBaos.Remove(tb);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}