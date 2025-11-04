using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KTX.Entities;
using KTX.Models;

namespace KTX.Controllers
{
    public class QuanLyPhongController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public QuanLyPhongController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // GET: Phong/Index - Danh sách phòng
        public async Task<IActionResult> Index(string? searchTinhTrang, string? searchLoaiPhong)
        {
            var phongs = _context.Phongs
                .Include(p => p.HopDongPhongs)
                    .ThenInclude(hd => hd.MsvNavigation)
                .AsQueryable();

            // Lọc theo tình trạng
            if (!string.IsNullOrEmpty(searchTinhTrang))
            {
                phongs = phongs.Where(p => p.TinhTrang == searchTinhTrang);
            }

            // Lọc theo loại phòng
            if (!string.IsNullOrEmpty(searchLoaiPhong))
            {
                phongs = phongs.Where(p => p.LoaiPhong == searchLoaiPhong);
            }

            var danhSachPhong = await phongs
                .OrderBy(p => p.MaP)
                .Select(p => new PhongViewModel
                {
                    MaP = p.MaP,
                    TinhTrang = p.TinhTrang,
                    HienO = p.HienO ?? 0,
                    ToiDaO = p.ToiDaO ?? 0,
                    LoaiPhong = p.LoaiPhong,
                    TienPhong = p.TienPhong ?? 0,
                    GioiTinh = p.GioiTinh,
                    SoLuongSinhVien = p.HienO ?? 0
                })
                .ToListAsync();

            // Lấy danh sách tình trạng và loại phòng cho filter
            ViewBag.DanhSachTinhTrang = await _context.Phongs
                .Select(p => p.TinhTrang)
                .Distinct()
                .ToListAsync();

            ViewBag.DanhSachLoaiPhong = await _context.Phongs
                .Select(p => p.LoaiPhong)
                .Distinct()
                .ToListAsync();

            ViewBag.SearchTinhTrang = searchTinhTrang;
            ViewBag.SearchLoaiPhong = searchLoaiPhong;

            return View(danhSachPhong);
        }

        // GET: Phong/Details/5 - Chi tiết phòng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phongs
                .Include(p => p.HopDongPhongs
                    .Where(hd => hd.TrangThaiHd == "Đăng Kí Thành Công" || hd.TrangThaiHd == "Đã thanh toán" || hd.TrangThaiHd == "Đã chuyển phòng"))
                    .ThenInclude(hd => hd.MsvNavigation)
                .Include(p => p.TienDienNuocs.OrderByDescending(t => t.DotTtdn))
                .FirstOrDefaultAsync(p => p.MaP == id);

            if (phong == null)
            {
                return NotFound();
            }

            // Lấy lịch sử hợp đồng
            var lichSuHopDong = await _context.HopDongPhongs
                .Include(hd => hd.MsvNavigation)
                .Where(hd => hd.MaP == id)
                .OrderByDescending(hd => hd.NgayKi)
                .Select(hd => new HopDongViewModel
                {
                    MaHd = hd.MaHd,
                    Msv = hd.Msv,
                    TenSinhVien = hd.MsvNavigation.HoTen,
                    NgayBatDau = hd.NgayBatDau,
                    NgayKetThuc = hd.NgayKetThuc,
                    NgayKi = hd.NgayKi,
                    TrangThaiHd = hd.TrangThaiHd,
                    TienCoc = hd.TienCoc ?? 0,
                    TienP = hd.TienP ?? 0
                })
                .ToListAsync();

            var chiTietPhong = new ChiTietPhongViewModel
            {
                MaP = phong.MaP,
                LoaiPhong = phong.LoaiPhong,
                TinhTrang = phong.TinhTrang,
                ToiDaO = phong.ToiDaO ?? 0,
                HienO = phong.HienO ?? 0,
                GioiTinh = phong.GioiTinh,
                TienPhong = phong.TienPhong ?? 0,
                TienCoc = phong.TienCoc ?? 0,
                DanhSachSinhVien = phong.HopDongPhongs
                    .Select(hd => new SinhVienPhongViewModel
                    {
                        Msv = hd.Msv,
                        HoTen = hd.MsvNavigation.HoTen,
                        Email = hd.MsvNavigation.Email,
                        Sdt = hd.MsvNavigation.Sdt,
                        NgayBatDau = hd.NgayBatDau,
                        NgayKetThuc = hd.NgayKetThuc
                    })
                    .ToList(),
                LichSuHopDong = lichSuHopDong,
                ChiSoDienNuoc = phong.TienDienNuocs
                    .Select(t => new DienNuocViewModel
                    {
                        MaHddn = t.MaHddn,
                        DotTtdn = t.DotTtdn,
                        TienDien = t.TienDien ?? 0,
                        TienNuoc = t.TienNuoc ?? 0,
                        TongTienDn = t.TongTienDn ?? 0,
                        NgayTtdn = t.NgayTtdn,
                        TrangThaiTtdn = t.TrangThaiTtdn
                    })
                    .ToList()
            };

            return View(chiTietPhong);
        }

        // GET: Phong/Edit/5 - Form cập nhật tình trạng
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null)
            {
                return NotFound();
            }

            var model = new CapNhatPhongViewModel
            {
                MaP = phong.MaP,
                TinhTrang = phong.TinhTrang,
                HienO = phong.HienO ?? 0,
                ToiDaO = phong.ToiDaO ?? 0
            };

            return View(model);
        }

        // POST: Phong/Edit/5 - Cập nhật tình trạng phòng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CapNhatPhongViewModel model)
        {
            if (id != model.MaP)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var phong = await _context.Phongs.FindAsync(id);
                    if (phong == null)
                    {
                        return NotFound();
                    }

                    phong.TinhTrang = model.TinhTrang;
                    phong.HienO = model.HienO;
                    phong.ToiDaO = model.ToiDaO;

                    _context.Update(phong);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật tình trạng phòng thành công!";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhongExists(model.MaP))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(model);
        }

        // GET: Phong/Create - Form thêm phòng mới
        public IActionResult Create()
        {
            return View();
        }

        // POST: Phong/Create - Thêm phòng mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThemPhongViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra mã phòng đã tồn tại chưa
                if (await _context.Phongs.AnyAsync(p => p.MaP == model.MaP))
                {
                    ModelState.AddModelError("MaP", "Mã phòng này đã tồn tại!");
                    return View(model);
                }

                var phong = new Phong
                {
                    MaP = model.MaP,
                    LoaiPhong = model.LoaiPhong,
                    TinhTrang = model.TinhTrang,
                    ToiDaO = model.ToiDaO,
                    HienO = 0, // Phòng mới chưa có ai ở
                    GioiTinh = model.GioiTinh,
                    TienPhong = model.TienPhong,
                    TienCoc = model.TienCoc
                };

                _context.Phongs.Add(phong);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Thêm phòng {model.MaP} thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // GET: Phong/Delete/5 - Xác nhận xóa phòng
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phong = await _context.Phongs
                .Include(p => p.HopDongPhongs)
                .FirstOrDefaultAsync(p => p.MaP == id);

            if (phong == null)
            {
                return NotFound();
            }

            // Kiểm tra xem phòng có hợp đồng đang hiệu lực không
            var hasActiveContract = phong.HopDongPhongs.Any(hd => hd.TrangThaiHd == "Đang hiệu lực");
            ViewBag.HasActiveContract = hasActiveContract;

            var model = new PhongViewModel
            {
                MaP = phong.MaP,
                LoaiPhong = phong.LoaiPhong,
                TinhTrang = phong.TinhTrang,
                HienO = phong.HienO ?? 0,
                ToiDaO = phong.ToiDaO ?? 0,
                GioiTinh = phong.GioiTinh,
                TienPhong = phong.TienPhong ?? 0
            };

            return View(model);
        }

        // POST: Phong/Delete/5 - Xóa phòng
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phong = await _context.Phongs
                .Include(p => p.HopDongPhongs)
                .FirstOrDefaultAsync(p => p.MaP == id);

            if (phong == null)
            {
                return NotFound();
            }

            // Kiểm tra xem phòng có hợp đồng đang hiệu lực không
            if (phong.HopDongPhongs.Any(hd => hd.TrangThaiHd == "Đang hiệu lực"))
            {
                TempData["ErrorMessage"] = "Không thể xóa phòng đang có sinh viên ở!";
                return RedirectToAction(nameof(Delete), new { id = id });
            }

            _context.Phongs.Remove(phong);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa phòng {id} thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool PhongExists(int id)
        {
            return _context.Phongs.Any(e => e.MaP == id);
        }
    }
}
