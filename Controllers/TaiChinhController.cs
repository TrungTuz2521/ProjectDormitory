using System;
using System.Linq;
using System.Threading.Tasks;
using KTX.Entities;
using KTX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KTX.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TaiChinhController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public TaiChinhController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // GET: TaiChinh/Index
        public IActionResult Index(string loai = "phong", int? dotTtdn = null)
        {
            var viewModel = new TaiChinhViewModel
            {
                LoaiHienThi = loai,
                DotTtdnHienTai = dotTtdn
            };

            // Lấy danh sách đợt điện nước
            viewModel.DanhSachDotTtdn = _context.TienDienNuocs
                .Where(t => t.DotTtdn.HasValue)
                .Select(t => t.DotTtdn.Value)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            // === TIỀN PHÒNG: THEO PHÒNG + SINH VIÊN ===
            if (loai == "phong")
            {
                var phongData = _context.Phongs
                    .Include(p => p.HopDongPhongs)
                        .ThenInclude(h => h.MsvNavigation)
                    .Include(p => p.HopDongPhongs)
                        .ThenInclude(h => h.TienPhongs)
                    .Where(p => p.HopDongPhongs.Any(h => h.TrangThaiHd == "Đăng Kí Thành Công"))
                    .Select(p => new PhongTienPhongDetail
                    {
                        MaP = p.MaP,
                        SinhViens = p.HopDongPhongs
                            .Where(h => h.TrangThaiHd == "Đăng Kí Thành Công")
                            .Select(h => new SinhVienTienPhong
                            {
                                Msv = h.Msv.ToString(),
                                HoTen = h.MsvNavigation.HoTen,
                                TienPhong = h.TienPhongs
                                    .OrderByDescending(t => t.HanTtp)
                                    .FirstOrDefault()
                            }).ToList()
                    })
                    .OrderBy(p => p.MaP)
                    .ToList();

                viewModel.ChiTietTienPhongTheoPhong = phongData;
            }

            // === TIỀN ĐIỆN NƯỚC: THEO PHÒNG + ĐỢT + CHI TIẾT SINH VIÊN ===
            else if (loai == "diennuoc")
            {
                var query = _context.TienDienNuocs
                    .Include(t => t.MaPNavigation)
                        .ThenInclude(p => p.HopDongPhongs)
                        .ThenInclude(h => h.MsvNavigation)
                    .Include(t => t.ChiTietThanhToanDienNuocs) // ✅ Include chi tiết
                        .ThenInclude(ct => ct.MsvNavigation)
                    .AsQueryable();

                if (dotTtdn.HasValue)
                    query = query.Where(t => t.DotTtdn == dotTtdn.Value);

                var dienNuocData = query
                    .Select(t => new PhongTienDienNuocDetail
                    {
                        MaP = t.MaP,
                        DotTtdn = t.DotTtdn,
                        TongTien = t.TongTienDn ?? 0,

                        // ✅ Lấy chi tiết thanh toán của từng sinh viên
                        SinhViens = t.ChiTietThanhToanDienNuocs
                            .Select(ct => new SinhVienTienDienNuoc
                            {
                                Msv = ct.Msv.ToString(),
                                HoTen = ct.MsvNavigation.HoTen,
                                MaCtttdn = ct.MaCtttdn,
                                SoTienPhai = ct.SoTienPhai,
                                SoTienDaTra = ct.SoTienDaTra,
                                TrangThaiCaNhan = ct.TrangThai,
                                NgayThanhToan = ct.NgayThanhToan,
                                TienDienNuoc = t  // Hóa đơn chung của phòng
                            }).ToList()
                    })
                    .ToList();

                // ✅ Tính trạng thái phòng dựa trên chi tiết thanh toán
                foreach (var phong in dienNuocData)
                {
                    phong.TongSinhVien = phong.SinhViens.Count;
                    phong.SoDaThu = phong.SinhViens.Count(sv => sv.TrangThaiCaNhan == "Đã thanh toán");

                    if (phong.SoDaThu == phong.TongSinhVien && phong.TongSinhVien > 0)
                        phong.TrangThai = "Đã thanh toán";
                    else if (phong.SoDaThu > 0)
                        phong.TrangThai = $"Đã thu {phong.SoDaThu}/{phong.TongSinhVien}";
                    else
                        phong.TrangThai = "Chưa thanh toán";
                }

                viewModel.ChiTietDienNuocTheoPhong = dienNuocData
                    .OrderByDescending(g => g.DotTtdn)
                    .ThenBy(g => g.MaP)
                    .ToList();
            }

            return View(viewModel);
        }

        // ✅ NHẬP ĐIỆN NƯỚC - Tự động tạo chi tiết cho từng sinh viên
        [HttpGet]
        public IActionResult NhapDienNuoc()
        {
            var viewModel = new NhapDienNuocViewModel();

            ViewBag.Phongs = _context.Phongs
                .Select(p => new { p.MaP, TenPhong = "Phòng " + p.MaP })
                .ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapDienNuocAsync(NhapDienNuocViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Phongs = _context.Phongs
                    .Select(p => new { p.MaP, TenPhong = "Phòng " + p.MaP })
                    .ToList();
                return View(model);
            }

            // Tính toán tiền điện nước PHÒNG
            decimal tienDien = (model.SoDien ?? 0) * model.GiaDien;
            decimal tienNuoc = (model.SoNuoc ?? 0) * model.GiaNuoc;
            decimal tongTien = tienDien + tienNuoc;

            // Tạo hóa đơn điện nước cho phòng
            var dienNuoc = new TienDienNuoc
            {
                MaP = model.MaP,
                SoDien = model.SoDien,
                SoNuoc = model.SoNuoc,
                GiaDien = (int?)model.GiaDien,
                GiaNuoc = (int?)model.GiaNuoc,
                TienDien = tienDien,
                TienNuoc = tienNuoc,
                TongTienDn = tongTien,
                DotTtdn = model.DotTtdn,
                TrangThaiTtdn = "Chưa thanh toán",
                Httdn = DateOnly.FromDateTime(DateTime.Now)
            };

            dienNuoc.MaHddn = await _context.TienDienNuocs.AnyAsync()
                    ? await _context.TienDienNuocs.MaxAsync(d => d.MaHddn) + 1
                    : 1;

            _context.TienDienNuocs.Add(dienNuoc);
            await _context.SaveChangesAsync();

            // ✅ TẠO CHI TIẾT THANH TOÁN CHO TỪNG SINH VIÊN TRONG PHÒNG
            var sinhViensInRoom = await _context.HopDongPhongs
                .Where(h => h.MaP == model.MaP &&
                       (h.TrangThaiHd == "Đang hiệu lực" || h.TrangThaiHd == "Đăng Kí Thành Công"))
                .ToListAsync();

            int soNguoi = sinhViensInRoom.Count;

            if (soNguoi > 0)
            {
                // Chia đều tiền cho mỗi sinh viên
                decimal tienMoiNguoi = Math.Round(tongTien / soNguoi, 0);

                // Lấy mã chi tiết lớn nhất
                int maxMaCtttdn = await _context.ChiTietThanhToanDienNuocs.AnyAsync()
                    ? await _context.ChiTietThanhToanDienNuocs.MaxAsync(ct => ct.MaCtttdn)
                    : 0;

                foreach (var hd in sinhViensInRoom)
                {
                    maxMaCtttdn++;

                    var chiTiet = new ChiTietThanhToanDienNuoc
                    {
                        MaCtttdn = maxMaCtttdn,
                        MaHddn = dienNuoc.MaHddn,
                        Msv = hd.Msv,
                        SoTienPhai = tienMoiNguoi,
                        SoTienDaTra = null,
                        TrangThai = "Chưa thanh toán",
                        NgayThanhToan = null
                    };

                    _context.ChiTietThanhToanDienNuocs.Add(chiTiet);
                }

                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Đã nhập chỉ số điện nước cho phòng {model.MaP} (Tháng {model.DotTtdn}) - Chia cho {soNguoi} sinh viên";
            return RedirectToAction(nameof(Index), new { loai = "diennuoc", dotTtdn = model.DotTtdn });
        }

        // ✅ THANH TOÁN ĐIỆN NƯỚC CÁ NHÂN
        [HttpGet]
        public IActionResult ThanhToanDienNuocCaNhan(int maCtttdn)
        {
            var chiTiet = _context.ChiTietThanhToanDienNuocs
                .Include(ct => ct.MsvNavigation)
                .Include(ct => ct.MaHddnNavigation)
                    .ThenInclude(hd => hd.MaPNavigation)
                .FirstOrDefault(ct => ct.MaCtttdn == maCtttdn);

            if (chiTiet == null) return NotFound();

            return View(chiTiet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToanDienNuocCaNhanAsync(int maCtttdn)
        {
            var chiTiet = await _context.ChiTietThanhToanDienNuocs
                .Include(ct => ct.MaHddnNavigation)
                .FirstOrDefaultAsync(ct => ct.MaCtttdn == maCtttdn);

            if (chiTiet == null) return NotFound();

            // ✅ Cập nhật trạng thái thanh toán của sinh viên
            chiTiet.TrangThai = "Đã thanh toán";
            chiTiet.SoTienDaTra = chiTiet.SoTienPhai;
            chiTiet.NgayThanhToan = DateOnly.FromDateTime(DateTime.Now);

            _context.Update(chiTiet);
            await _context.SaveChangesAsync();

            // ✅ KIỂM TRA: Nếu TẤT CẢ sinh viên trong phòng đã thanh toán
            //    → Cập nhật trạng thái hóa đơn phòng thành "Đã thanh toán"
            var tatCaDaThu = await _context.ChiTietThanhToanDienNuocs
                .Where(ct => ct.MaHddn == chiTiet.MaHddn)
                .AllAsync(ct => ct.TrangThai == "Đã thanh toán");

            if (tatCaDaThu)
            {
                var hoaDon = await _context.TienDienNuocs.FindAsync(chiTiet.MaHddn);
                if (hoaDon != null)
                {
                    hoaDon.TrangThaiTtdn = "Đã thanh toán";
                    hoaDon.NgayTtdn = DateOnly.FromDateTime(DateTime.Now);
                    _context.Update(hoaDon);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Thanh toán điện nước thành công!";
            return RedirectToAction(nameof(Index), new
            {
                loai = "diennuoc",
                dotTtdn = chiTiet.MaHddnNavigation?.DotTtdn
            });
        }

        // Chi tiết tiền phòng
        public IActionResult ChiTietTienPhong(int id)
        {
            var tienPhong = _context.TienPhongs
                .Include(t => t.MaHdNavigation)
                    .ThenInclude(h => h.MsvNavigation)
                .Include(t => t.MaHdNavigation)
                    .ThenInclude(h => h.MaPNavigation)
                .FirstOrDefault(t => t.MaHdp == id);

            if (tienPhong == null) return NotFound();
            return View(tienPhong);
        }

        // Chi tiết tiền điện nước (xem toàn bộ hóa đơn phòng)
        public IActionResult ChiTietDienNuoc(int id)
        {
            var tienDienNuoc = _context.TienDienNuocs
                .Include(t => t.MaPNavigation)
                .Include(t => t.ChiTietThanhToanDienNuocs)
                    .ThenInclude(ct => ct.MsvNavigation)
                .FirstOrDefault(t => t.MaHddn == id);

            if (tienDienNuoc == null) return NotFound();
            return View(tienDienNuoc);
        }

        // Thanh toán tiền phòng
        public IActionResult ThanhToanTienPhong(int id)
        {
            var tienPhong = _context.TienPhongs
                .Include(t => t.MaHdNavigation)
                    .ThenInclude(h => h.MsvNavigation)
                .FirstOrDefault(t => t.MaHdp == id);

            if (tienPhong == null) return NotFound();
            return View(tienPhong);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThanhToanTienPhong(int id, string phuongThucTt)
        {
            var tienPhong = _context.TienPhongs.Find(id);
            if (tienPhong == null) return NotFound();

            tienPhong.TrangThaiTtp = "Đã thanh toán";
            tienPhong.NgayTtp = DateOnly.FromDateTime(DateTime.Now);
            _context.Update(tienPhong);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Thanh toán tiền phòng thành công!";
            return RedirectToAction(nameof(Index), new { loai = "phong" });
        }

        // Thống kê
        public IActionResult ThongKe()
        {
            var thongKe = new ThongKeTaiChinhViewModel
            {
                TongTienPhongChuaThu = _context.TienPhongs
                    .Where(t => t.TrangThaiTtp != "Đã thanh toán")
                    .Sum(t => t.TongTienP ?? 0),
                TongTienPhongDaThu = _context.TienPhongs
                    .Where(t => t.TrangThaiTtp == "Đã thanh toán")
                    .Sum(t => t.TongTienP ?? 0),

                // ✅ Tính theo chi tiết thanh toán
                TongTienDienNuocChuaThu = _context.ChiTietThanhToanDienNuocs
                    .Where(ct => ct.TrangThai != "Đã thanh toán")
                    .Sum(ct => ct.SoTienPhai),
                TongTienDienNuocDaThu = _context.ChiTietThanhToanDienNuocs
                    .Where(ct => ct.TrangThai == "Đã thanh toán")
                    .Sum(ct => ct.SoTienDaTra ?? 0),

                SoHoaDonPhongChuaThu = _context.TienPhongs
                    .Count(t => t.TrangThaiTtp != "Đã thanh toán"),
                SoHoaDonDienNuocChuaThu = _context.ChiTietThanhToanDienNuocs
                    .Count(ct => ct.TrangThai != "Đã thanh toán")
            };

            thongKe.TongDoanhThu = thongKe.TongTienPhongDaThu + thongKe.TongTienDienNuocDaThu;
            thongKe.TongCongNo = thongKe.TongTienPhongChuaThu + thongKe.TongTienDienNuocChuaThu;

            return View(thongKe);
        }
    }
}