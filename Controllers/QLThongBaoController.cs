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
                    .Where(t => t.Msv == null)
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
                        MSV = t.Msv.Value,
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
        public async Task<IActionResult> Create([Bind("MSV,TieuDe,NoiDung")] QLThongBaoViewModel model)
        {
            Console.WriteLine($"[POST] MSV: '{model.MSV}' | Tiêu đề: '{model.TieuDe}'");

            if (!ModelState.IsValid)
            {
                await LoadSinhVienList(model);
                return Json(new
                {
                    success = false,
                    errors = ModelState.ToDictionary(
                    k => k.Key,
                    v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                )
                });
            }

            try
            {
                // Kiểm tra Tiêu đề và Nội dung
                if (string.IsNullOrWhiteSpace(model.TieuDe))
                {
                    ModelState.AddModelError("TieuDe", "Tiêu đề không được để trống");
                    await LoadSinhVienList(model);
                    return Json(new { success = false });
                }

                if (string.IsNullOrWhiteSpace(model.NoiDung))
                {
                    ModelState.AddModelError("NoiDung", "Nội dung không được để trống");
                    await LoadSinhVienList(model);
                    return Json(new { success = false });
                }

                // ====== Tạo mới Thông báo ======
                var tb = new ThongBao
                {
                    TieuDe = model.TieuDe.Trim(),
                    NoiDung = model.NoiDung.Trim(),
                    NgayTb = DateOnly.FromDateTime(DateTime.Now),
                    Msv = null
                };

                string msvInput = (model.MSV ?? "").Trim();

                // Xử lý MSV (gửi chung hoặc riêng)
                if (string.IsNullOrWhiteSpace(msvInput))
                {
                    tb.Msv = null; // Gửi chung
                    Console.WriteLine("[INFO] Gửi CHUNG - Msv = null");
                }
                else if (int.TryParse(msvInput, out int msv) && msv > 0)
                {
                    bool svTonTai = await _context.SinhViens.AnyAsync(s => s.Msv == msv);
                    if (!svTonTai)
                    {
                        ModelState.AddModelError("MSV", "Sinh viên không tồn tại");
                        await LoadSinhVienList(model);
                        return Json(new { success = false });
                    }

                    tb.Msv = msv;
                    Console.WriteLine($"[INFO] Gửi RIÊNG - Msv = {msv}");
                }
                else
                {
                    ModelState.AddModelError("MSV", "MSV không hợp lệ");
                    await LoadSinhVienList(model);
                    return Json(new { success = false });
                }

                // ====== Tạo MaTb ======
                tb.MaTb = await _context.ThongBaos.AnyAsync()
                    ? await _context.ThongBaos.MaxAsync(x => x.MaTb) + 1
                    : 1;

                _context.ThongBaos.Add(tb);
                int result = await _context.SaveChangesAsync();

                Console.WriteLine($"[DB] Đã lưu: {result} bản ghi");

                if (result > 0)
                {
                    TempData["Success"] = "Gửi thông báo thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "Lưu thất bại!";
                    await LoadSinhVienList(model);
                    return View(model);
                }
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"[DB LỖI] {dbEx.Message}");
                return Json(new { success = false, message = "Lỗi database!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LỖI] {ex.Message}");
                return Json(new { success = false, message = "Lỗi hệ thống!" });
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