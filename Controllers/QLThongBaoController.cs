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

            var viewModel = new ThongBaoListViewModel
            {
                ThongBaoChung = thongBaos
                    .Where(t => t.Msv == 0)
                    .Select(t => new ThongBaoViewModel
                    {
                        MaTB = t.MaTb,
                        MSV = 0,
                        HoTen = "Tất cả sinh viên",
                        TieuDe = t.TieuDe ?? "",
                        NoiDung = t.NoiDung ?? "",
                        NgayTB = t.NgayTb
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
                        NgayTB = t.NgayTb
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

        // POST: Tạo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QLThongBaoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DanhSachSinhVien = await _context.SinhViens
                    .Select(s => new SinhVienSelectViewModel
                    {
                        MSV = s.Msv,
                        HoTen = s.HoTen,
                        Email = s.Email
                    })
                    .ToListAsync();
                return View(model);
            }

            var ngayTB = DateOnly.FromDateTime(DateTime.Now);

            if (string.IsNullOrWhiteSpace(model.MSV) || model.MSV == "0")
            {
                // GỬI CHUNG
                _context.ThongBaos.Add(new ThongBao
                {
                    Msv = 0,
                    TieuDe = model.TieuDe,
                    NoiDung = model.NoiDung,
                    NgayTb = ngayTB
                });
            }
            else
            {
                if (int.TryParse(model.MSV, out int msv) && msv > 0)
                {
                    _context.ThongBaos.Add(new ThongBao
                    {
                        Msv = msv,
                        TieuDe = model.TieuDe,
                        NoiDung = model.NoiDung,
                        NgayTb = ngayTB
                    });
                }
                else
                {
                    ModelState.AddModelError("MSV", "Mã sinh viên không hợp lệ.");
                    model.DanhSachSinhVien = await _context.SinhViens
                        .Select(s => new SinhVienSelectViewModel
                        {
                            MSV = s.Msv,
                            HoTen = s.HoTen,
                            Email = s.Email
                        })
                        .ToListAsync();
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Gửi thông báo thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Xóa
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null)
                return Json(new { success = false, message = "Không tìm thấy." });

            _context.ThongBaos.Remove(tb);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}