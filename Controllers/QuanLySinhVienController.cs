// ✅ FIXED QuanLySinhVienController.cs - Đầy đủ và tối ưu

using KTX.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace KTX.Controllers
{
    public class QuanLySinhVienController : Controller
    {
        private readonly SinhVienKtxContext _context;
        private const int PageSize = 15;

        public QuanLySinhVienController(SinhVienKtxContext context)
        {
            _context = context;
        }

        // ========== INDEX ==========
        public async Task<IActionResult> Index(
            string searchTerm,
            string phongFilter,
            string trangThaiFilter,
            string khoaFilter,
            string gioiTinhFilter,
            int page = 1)
        {
            ViewBag.Title = "Quản lý Sinh viên";

            try
            {
                var query = _context.SinhViens.AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.Trim().ToLower();
                    query = query.Where(sv =>
                        sv.Msv.ToString().Contains(searchTerm) ||
                        sv.HoTen.ToLower().Contains(searchTerm) ||
                        (sv.Email != null && sv.Email.ToLower().Contains(searchTerm)) ||
                        sv.Sdt.Contains(searchTerm)
                    );
                }

                // Lọc theo khoa
                if (!string.IsNullOrEmpty(khoaFilter))
                {
                    query = query.Where(sv => sv.Khoa == khoaFilter);
                }

                // Lọc theo giới tính
                if (!string.IsNullOrEmpty(gioiTinhFilter))
                {
                    query = query.Where(sv => sv.GioiTinh == gioiTinhFilter);
                }

                // Lọc theo phòng
                if (!string.IsNullOrEmpty(phongFilter))
                {
                    var sinhVienTrongPhong = _context.HopDongPhongs
                        .Where(hd => hd.MaP == int.Parse(phongFilter) && hd.TrangThaiHd == "Đăng Kí Thành Công")
                        .Select(hd => hd.Msv)
                        .Distinct();

                    query = query.Where(sv => sinhVienTrongPhong.Contains(sv.Msv));
                }

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(trangThaiFilter))
                {
                    if (trangThaiFilter == "DangO")
                    {
                        var DangO = _context.HopDongPhongs
                            .Where(hd => hd.TrangThaiHd == "Đăng Kí Thành Công")
                            .Select(hd => hd.Msv)
                            .Distinct();
                        query = query.Where(sv => DangO.Contains(sv.Msv));
                    }
                    else if (trangThaiFilter == "ChuaO")
                    {
                        var DangO = _context.HopDongPhongs
                            .Where(hd => hd.TrangThaiHd == "Đăng Kí Thành Công")
                            .Select(hd => hd.Msv)
                            .Distinct();
                        query = query.Where(sv => !DangO.Contains(sv.Msv));
                    }
                }

                // Pagination
                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

                var sinhViens = await query
                    .OrderBy(sv => sv.HoTen)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // ViewBag data
                ViewBag.DanhSachPhong = await _context.Phongs.OrderBy(p => p.MaP).ToListAsync();
                ViewBag.DanhSachKhoa = await _context.SinhViens
                    .Where(sv => !string.IsNullOrEmpty(sv.Khoa))
                    .Select(sv => sv.Khoa)
                    .Distinct()
                    .OrderBy(k => k)
                    .ToListAsync();

                ViewBag.SearchTerm = searchTerm;
                ViewBag.PhongFilter = phongFilter;
                ViewBag.TrangThaiFilter = trangThaiFilter;
                ViewBag.KhoaFilter = khoaFilter;
                ViewBag.GioiTinhFilter = gioiTinhFilter;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalRecords = totalRecords;

                // Thống kê
                var tongSinhVien = await _context.SinhViens.CountAsync();
                var svDangO = await _context.HopDongPhongs
                    .Where(hd => hd.TrangThaiHd == "Đăng Kí Thành Công")
                    .Select(hd => hd.Msv)
                    .Distinct()
                    .CountAsync();

                ViewBag.TongSinhVien = tongSinhVien;
                ViewBag.SvDangO = svDangO;
                ViewBag.SvChuaO = tongSinhVien - svDangO;

                return View(sinhViens);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in Index: {ex.Message}\n{ex.StackTrace}");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách sinh viên.";
                return View(new List<SinhVien>());
            }
        }

        // ========== CREATE - GET ==========
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm sinh viên mới";
            return View();
        }

        // ========== CREATE - POST ✅ FIXED & CLEAN ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SinhVien sinhVien)
        {
            try
            {
                int maxMsv = await _context.SinhViens.AnyAsync()
                    ? await _context.SinhViens.MaxAsync(sv => sv.Msv)
                    : 0;

                sinhVien.Msv = maxMsv + 1;

                // ✅ Sinh TenDN tự động (ví dụ: dùng mã sinh viên làm tên đăng nhập)
                sinhVien.TenDn = sinhVien.Msv.ToString();

                // ✅ Đặt mật khẩu mặc định = mã sinh viên
                sinhVien.MatKhau = sinhVien.Msv.ToString();

                _context.SinhViens.Add(sinhVien);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Thêm sinh viên thành công! Mã mới: {sinhVien.Msv}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "(không có inner exception)";
                Console.WriteLine($"❌ ERROR in Create: {ex.Message}\nINNER: {inner}\nSTACK: {ex.StackTrace}");
                TempData["Error"] = $"Có lỗi xảy ra: {inner}";
                return View(sinhVien);
            }


        }


        // ========== EDIT - GET ==========
        public async Task<IActionResult> Edit(int id)
        {
            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien == null)
            {
                TempData["Error"] = "Không tìm thấy sinh viên!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Title = "Chỉnh sửa thông tin sinh viên";
            return View(sinhVien);
        }

        // ========== EDIT - POST ✅ FIXED ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SinhVien sinhVien)
        {
            if (id != sinhVien.Msv)
            {
                return NotFound();
            }

            try
            {
                ModelState.Remove("MatKhau");
                ModelState.Remove("Avatar");

                var existing = await _context.SinhViens.FindAsync(id);
                if (existing == null)
                {
                    TempData["Error"] = "Không tìm thấy sinh viên!";
                    return RedirectToAction(nameof(Index));
                }

                // Cập nhật (GIỮ NGUYÊN password và avatar)
                existing.HoTen = sinhVien.HoTen;
                existing.GioiTinh = sinhVien.GioiTinh;
                existing.NgaySinh = sinhVien.NgaySinh;
                existing.Sdt = sinhVien.Sdt;
                existing.Email = sinhVien.Email;
                existing.Khoa = sinhVien.Khoa;

                _context.SinhViens.Update(existing);
                await _context.SaveChangesAsync();

                TempData["Success"] = "✅ Cập nhật thông tin thành công!";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in Edit: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return View(sinhVien);
            }
        }

        // ========== DETAILS ==========
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var sinhVien = await _context.SinhViens.FirstOrDefaultAsync(sv => sv.Msv == id);
                if (sinhVien == null)
                {
                    TempData["Error"] = $"Không tìm thấy sinh viên có mã {id}";
                    return RedirectToAction(nameof(Index));
                }

                var hopDongHienTai = await _context.HopDongPhongs
                    .Include(h => h.MaPNavigation)
                    .FirstOrDefaultAsync(h => h.Msv == id && h.TrangThaiHd == "Đăng Kí Thành Công");

                var lichSuHopDong = await _context.HopDongPhongs
                    .Include(h => h.MaPNavigation)
                    .Where(h => h.Msv == id)
                    .OrderByDescending(h => h.NgayBatDau)
                    .ToListAsync();

                var lichSuTienPhong = await (from t in _context.TienPhongs
                                             join h in _context.HopDongPhongs on t.MaHd equals h.MaHd
                                             where h.Msv == id
                                             orderby t.NgayTtp descending
                                             select t).Take(10).ToListAsync();

                var lichSuDienNuoc = await (from d in _context.TienDienNuocs
                                            join h in _context.HopDongPhongs on d.MaP equals h.MaP
                                            where h.Msv == id
                                            orderby d.NgayTtdn descending
                                            select d).Take(10).ToListAsync();

                var lichSuYeuCau = await _context.YeuCaus
                    .Where(y => y.Msv == id)
                    .OrderByDescending(y => y.NgayGuiYc)
                    .Take(10)
                    .ToListAsync();

                ViewBag.HopDongHienTai = hopDongHienTai;
                ViewBag.LichSuHopDong = lichSuHopDong;
                ViewBag.LichSuTienPhong = lichSuTienPhong;
                ViewBag.LichSuDienNuoc = lichSuDienNuoc;
                ViewBag.LichSuYeuCau = lichSuYeuCau;

                var phong = hopDongHienTai?.MaPNavigation;
                ViewBag.Phong = phong;
                ViewBag.SoNguoiTrongPhong = phong?.HienO ?? 1;

                return View(sinhVien);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in Details: {ex.Message}");
                TempData["Error"] = "Lỗi khi tải chi tiết sinh viên: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ========== CẤP PHÒNG - GET ==========
        public async Task<IActionResult> CapPhong(int id)
        {
            if (id <= 0) return NotFound();

            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien == null)
            {
                TempData["Error"] = "Không tìm thấy sinh viên!";
                return RedirectToAction(nameof(Index));
            }

            // 🔹 Lọc phòng còn chỗ và phù hợp giới tính
            var phongConCho = await _context.Phongs
                .Where(p => p.HienO < p.ToiDaO &&
                            (p.GioiTinh == sinhVien.GioiTinh || p.GioiTinh == null))
                .OrderBy(p => p.MaP)
                .ToListAsync();

            ViewBag.SinhVien = sinhVien;
            ViewBag.PhongConCho = phongConCho;
            ViewBag.Title = "Cấp phòng cho sinh viên";

            return View();
        }


        // ========== CẤP PHÒNG - POST ✅ (ĐÃ FIX) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapPhong(int msv, int maPhong, DateTime ngayBatDau, DateTime ngayKetThuc)
        {
            try
            {
                // Validate
                if (msv <= 0 || maPhong <= 0)
                {
                    TempData["Error"] = "Dữ liệu không hợp lệ!";
                    return RedirectToAction("CapPhong", new { id = msv });
                }

                if (ngayKetThuc <= ngayBatDau)
                {
                    TempData["Error"] = "Ngày kết thúc phải sau ngày bắt đầu!";
                    return RedirectToAction("CapPhong", new { id = msv });
                }

                // Kiểm tra sinh viên
                var sinhVien = await _context.SinhViens.FindAsync(msv);
                if (sinhVien == null)
                {
                    TempData["Error"] = "Không tìm thấy sinh viên!";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra sinh viên đã có phòng chưa
                bool daCoPhong = await _context.HopDongPhongs
                    .AnyAsync(h => h.Msv == msv && h.TrangThaiHd == "Đăng Kí Thành Công");

                if (daCoPhong)
                {
                    TempData["Error"] = "Sinh viên đã có phòng!";
                    return RedirectToAction("Details", new { id = msv });
                }

                // Lấy thông tin phòng
                var phong = await _context.Phongs.FindAsync(maPhong);
                if (phong == null || phong.HienO >= phong.ToiDaO)
                {
                    TempData["Error"] = "Phòng đã đầy hoặc không tồn tại!";
                    return RedirectToAction("CapPhong", new { id = msv });
                }

                // ✅ Lấy mã hợp đồng mới
                int maxMaHd = await _context.HopDongPhongs.AnyAsync()
                    ? await _context.HopDongPhongs.MaxAsync(h => h.MaHd)
                    : 0;

                // ✅ Tạo hợp đồng mới, lấy thông tin phòng để điền vào
                var hopDong = new HopDongPhong
                {
                    MaHd = maxMaHd + 1,
                    Msv = msv,
                    MaP = maPhong,
                    LoaiP = phong.LoaiPhong,      // <== Thêm loại phòng
                    NgayBatDau = DateOnly.FromDateTime(ngayBatDau),
                    NgayKetThuc = DateOnly.FromDateTime(ngayKetThuc),
                    NgayKi = DateOnly.FromDateTime(DateTime.Now),
                    TienCoc = phong.TienCoc,      // <== Thêm tiền cọc
                    TienP = phong.TienPhong,      // <== Thêm tiền phòng
                    TrangThaiHd = "Đăng Kí Thành Công"
                };

                // ✅ Cập nhật số người đang ở trong phòng
                phong.HienO = (phong.HienO ?? 0) + 1;
                _context.Phongs.Update(phong);

                // ✅ Lưu hợp đồng vào database
                _context.HopDongPhongs.Add(hopDong);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Cấp phòng {phong.MaP} thành công!";
                return RedirectToAction("Details", new { id = msv });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in CapPhong: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("CapPhong", new { id = msv });
            }
        }


        // ========== CHUYỂN PHÒNG - GET ==========
        public async Task<IActionResult> ChuyenPhong(int id)
        {
            if (id <= 0) return NotFound();

            var sinhVien = await _context.SinhViens.FindAsync(id);
            if (sinhVien == null)
            {
                TempData["Error"] = "Không tìm thấy sinh viên!";
                return RedirectToAction(nameof(Index));
            }

            // 🔹 Lấy hợp đồng hiện tại của SV (chỉ hợp đồng đang hiệu lực)
            var hopDongHienTai = await _context.HopDongPhongs
                .Include(h => h.MaPNavigation)
                .FirstOrDefaultAsync(h => h.Msv == id && h.TrangThaiHd == "Đăng Kí Thành Công");

            if (hopDongHienTai == null)
            {
                TempData["Error"] = "Sinh viên chưa có phòng!";
                return RedirectToAction("Details", new { id });
            }

            // 🔹 Lọc phòng còn chỗ, khác phòng hiện tại, cùng giới tính
            var phongConCho = await _context.Phongs
                .Where(p => p.HienO < p.ToiDaO &&
                            p.MaP != hopDongHienTai.MaP &&
                            (p.GioiTinh == sinhVien.GioiTinh || p.GioiTinh == null))
                .OrderBy(p => p.MaP)
                .ToListAsync();

            ViewBag.SinhVien = sinhVien;
            ViewBag.HopDongHienTai = hopDongHienTai;
            ViewBag.PhongConCho = phongConCho;
            ViewBag.Title = "Chuyển phòng cho sinh viên";

            return View();
        }


        // ========== CHUYỂN PHÒNG - POST (CÓ THÊM LOAIPHONG + TIỀN) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChuyenPhong(int msv, int maPhongMoi, string lyDo)
        {
            try
            {
                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(lyDo))
                {
                    TempData["Error"] = "Vui lòng nhập lý do chuyển phòng!";
                    return RedirectToAction("ChuyenPhong", new { id = msv });
                }

                if (maPhongMoi <= 0)
                {
                    TempData["Error"] = "Vui lòng chọn phòng mới!";
                    return RedirectToAction("ChuyenPhong", new { id = msv });
                }

                // Tìm hợp đồng hiện tại
                var hopDongCuInfo = await _context.HopDongPhongs
                    .AsNoTracking()
                    .Include(h => h.MaPNavigation)
                    .FirstOrDefaultAsync(h => h.Msv == msv && h.TrangThaiHd == "Đăng Kí Thành Công");

                if (hopDongCuInfo == null)
                {
                    TempData["Error"] = "Không tìm thấy hợp đồng hiện tại!";
                    return RedirectToAction("Details", new { id = msv });
                }

                // Kiểm tra phòng mới
                var phongMoi = await _context.Phongs.FindAsync(maPhongMoi);
                if (phongMoi == null || phongMoi.HienO >= phongMoi.ToiDaO)
                {
                    TempData["Error"] = "Phòng mới đã đầy hoặc không tồn tại!";
                    return RedirectToAction("ChuyenPhong", new { id = msv });
                }

                if (hopDongCuInfo.MaP == maPhongMoi)
                {
                    TempData["Error"] = "Không thể chuyển sang cùng phòng hiện tại!";
                    return RedirectToAction("ChuyenPhong", new { id = msv });
                }

                int maPhongCu = hopDongCuInfo.MaP;
                string tenPhongCu = hopDongCuInfo.MaPNavigation?.MaP.ToString() ?? "(không rõ)";

                // 🔹 Lấy hợp đồng cũ để cập nhật trạng thái
                var hopDongCu = await _context.HopDongPhongs
                    .FirstOrDefaultAsync(h => h.MaHd == hopDongCuInfo.MaHd);

                if (hopDongCu != null)
                {
                    hopDongCu.TrangThaiHd = "Đã chuyển phòng";
                    hopDongCu.NgayKetThuc = DateOnly.FromDateTime(DateTime.Now);
                }

                // 🔹 Giảm số người phòng cũ
                if (maPhongCu != maPhongMoi)
                {
                    var phongCu = await _context.Phongs.FindAsync(maPhongCu);
                    if (phongCu != null && (phongCu.HienO ?? 0) > 0)
                    {
                        phongCu.HienO--;
                        _context.Phongs.Update(phongCu);
                    }
                }

                // 🔹 Sinh mã hợp đồng mới
                int maxMaHd = await _context.HopDongPhongs.AnyAsync()
                    ? await _context.HopDongPhongs.MaxAsync(h => h.MaHd)
                    : 0;

                // 🔹 Tạo hợp đồng mới (có loại phòng, tiền phòng, tiền cọc)
                var hopDongMoi = new HopDongPhong
                {
                    MaHd = maxMaHd + 1,
                    Msv = msv,
                    MaP = maPhongMoi,
                    LoaiP = phongMoi.LoaiPhong,
                    NgayBatDau = DateOnly.FromDateTime(DateTime.Now),
                    NgayKetThuc = hopDongCuInfo.NgayKetThuc ?? DateOnly.FromDateTime(DateTime.Now.AddMonths(6)),
                    TrangThaiHd = "Đăng Kí Thành Công",
                    NgayKi = DateOnly.FromDateTime(DateTime.Now),
                    TienCoc = phongMoi.TienCoc,
                    TienP = phongMoi.TienPhong
                };

                _context.HopDongPhongs.Add(hopDongMoi);

                // 🔹 Tăng số người ở phòng mới
                phongMoi.HienO = (phongMoi.HienO ?? 0) + 1;
                _context.Phongs.Update(phongMoi);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Đã chuyển từ phòng {tenPhongCu} sang phòng {phongMoi.MaP} thành công!";
                return RedirectToAction("Details", new { id = msv });
            }
            catch (DbUpdateException dbEx)
            {
                var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                Console.WriteLine($"❌ LỖI DATABASE khi chuyển phòng: {inner}");
                TempData["Error"] = $"Lỗi database: {inner}";
                return RedirectToAction("ChuyenPhong", new { id = msv });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ LỖI CHUNG khi chuyển phòng: {ex.Message}\n{ex.StackTrace}");
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("ChuyenPhong", new { id = msv });
            }
        }

        // ========== TẠO HỢP ĐỒNG MỚI ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoHopDongMoi(int msv, int maPhong, DateTime ngayBatDau, DateTime ngayKetThuc, string ghiChu)
        {
            try
            {
                // Validate
                if (msv <= 0 || maPhong <= 0)
                {
                    TempData["Error"] = "Dữ liệu không hợp lệ!";
                    return RedirectToAction("Details", new { id = msv });
                }

                if (ngayKetThuc <= ngayBatDau)
                {
                    TempData["Error"] = "Ngày kết thúc phải sau ngày bắt đầu!";
                    return RedirectToAction("Details", new { id = msv });
                }

                var sinhVien = await _context.SinhViens.FindAsync(msv);
                if (sinhVien == null)
                {
                    TempData["Error"] = "Không tìm thấy sinh viên!";
                    return RedirectToAction("Details", new { id = msv });
                }

                var phong = await _context.Phongs.FindAsync(maPhong);
                if (phong == null || phong.HienO >= phong.ToiDaO)
                {
                    TempData["Error"] = "Phòng đã đầy hoặc không tồn tại!";
                    return RedirectToAction("Details", new { id = msv });
                }

                // Kiểm tra trùng thời gian
                var ngayBatDauDateOnly = DateOnly.FromDateTime(ngayBatDau);
                var ngayKetThucDateOnly = DateOnly.FromDateTime(ngayKetThuc);

                var hopDongTrung = await _context.HopDongPhongs
                    .AnyAsync(h => h.Msv == msv &&
                                  h.TrangThaiHd == "Đăng Kí Thành Công" &&
                                  ((h.NgayBatDau <= ngayBatDauDateOnly && h.NgayKetThuc >= ngayBatDauDateOnly) ||
                                   (h.NgayBatDau <= ngayKetThucDateOnly && h.NgayKetThuc >= ngayKetThucDateOnly)));

                if (hopDongTrung)
                {
                    TempData["Warning"] = "Sinh viên đã có hợp đồng trong khoảng thời gian này!";
                    return RedirectToAction("Details", new { id = msv });
                }

                // Tạo hợp đồng
                var hopDong = new HopDongPhong
                {
                    Msv = msv,
                    MaP = maPhong,
                    NgayBatDau = ngayBatDauDateOnly,
                    NgayKetThuc = ngayKetThucDateOnly,
                    TrangThaiHd = "Đăng Kí Thành Công",
                    NgayKi = DateOnly.FromDateTime(DateTime.Now)
                };
                _context.HopDongPhongs.Add(hopDong);

                phong.HienO++;
                _context.Phongs.Update(phong);

                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Tạo hợp đồng phòng {phong.MaP} thành công!";
                return RedirectToAction("Details", new { id = msv });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in TaoHopDongMoi: {ex.Message}");
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Details", new { id = msv });
            }
        }

        // ========== GIA HẠN HỢP ĐỒNG ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaHanHopDong(int msv, DateTime ngayKetThucMoi, string lyDoGiaHan)
        {
            try
            {
                var hopDong = await _context.HopDongPhongs
                    .Include(h => h.MaPNavigation)
                    .FirstOrDefaultAsync(h => h.Msv == msv && h.TrangThaiHd == "Đăng Kí Thành Công");

                if (hopDong == null)
                {
                    TempData["Error"] = "Không tìm thấy hợp đồng đang hoạt động!";
                    return RedirectToAction("Details", new { id = msv });
                }

                var ngayKetThucMoiDateOnly = DateOnly.FromDateTime(ngayKetThucMoi);
                if (ngayKetThucMoiDateOnly <= hopDong.NgayKetThuc)
                {
                    TempData["Error"] = "Ngày kết thúc mới phải sau ngày kết thúc hiện tại!";
                    return RedirectToAction("Details", new { id = msv });
                }

                var ngayKetThucCu = hopDong.NgayKetThuc;
                hopDong.NgayKetThuc = ngayKetThucMoiDateOnly;

                _context.HopDongPhongs.Update(hopDong);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Gia hạn hợp đồng thành công! Từ {ngayKetThucCu:dd/MM/yyyy} đến {ngayKetThucMoiDateOnly:dd/MM/yyyy}";
                return RedirectToAction("Details", new { id = msv });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GiaHanHopDong: {ex.Message}");
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Details", new { id = msv });
            }
        }

        // ========== HỦY HỢP ĐỒNG ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyHopDong(int msv, string lyDoHuy, DateTime ngayHuy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(lyDoHuy))
                {
                    TempData["Error"] = "Vui lòng nhập lý do hủy hợp đồng!";
                    return RedirectToAction("Details", new { id = msv });
                }

                var hopDong = await _context.HopDongPhongs
                    .Include(h => h.MaPNavigation)
                    .FirstOrDefaultAsync(h => h.Msv == msv && h.TrangThaiHd == "Đăng Kí Thành Công");

                if (hopDong == null)
                {
                    TempData["Error"] = "Không tìm thấy hợp đồng đang hoạt động!";
                    return RedirectToAction("Details", new { id = msv });
                }

                var tenPhong = hopDong.MaPNavigation?.MaP.ToString();

                hopDong.TrangThaiHd = "Đã hủy";
                var ngayHuyDateOnly = DateOnly.FromDateTime(ngayHuy);
                if (ngayHuyDateOnly < hopDong.NgayKetThuc)
                {
                    hopDong.NgayKetThuc = ngayHuyDateOnly;
                }

                _context.HopDongPhongs.Update(hopDong);

                var phong = await _context.Phongs.FindAsync(hopDong.MaP);
                if (phong != null && phong.HienO > 0)
                {
                    phong.HienO--;
                    _context.Phongs.Update(phong);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Đã hủy hợp đồng phòng {tenPhong} thành công!";
                return RedirectToAction("Details", new { id = msv });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in HuyHopDong: {ex.Message}");
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Details", new { id = msv });
            }
        }

        // ========== RESET PASSWORD ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            try
            {
                var sinhVien = await _context.SinhViens.FindAsync(id);
                if (sinhVien == null)
                {
                    return NotFound();
                }

                var matKhauMoi = sinhVien.Msv.ToString();
                sinhVien.MatKhau = HashPassword(matKhauMoi);

                _context.SinhViens.Update(sinhVien);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Reset mật khẩu thành công! Mật khẩu mới: {matKhauMoi}";
                return RedirectToAction("Details", new { id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in ResetPassword: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi reset mật khẩu.";
                return RedirectToAction("Details", new { id });
            }
        }

        // ========== DELETE ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var sinhVien = await _context.SinhViens.FindAsync(id);
                if (sinhVien == null)
                {
                    TempData["Error"] = "Không tìm thấy sinh viên!";
                    return RedirectToAction(nameof(Index));
                }

                

                _context.SinhViens.Remove(sinhVien);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ Đã xóa sinh viên {sinhVien.HoTen}!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in Delete: {ex.Message}");
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        // ========== API: GET PHÒNG TRỐNG ==========
        [HttpGet]
        public async Task<IActionResult> GetPhongTrong()
        {
            try
            {
                var danhSachPhong = await _context.Phongs
                    .Where(p => p.HienO < p.ToiDaO)
                    .Select(p => new
                    {
                        maP = p.MaP,
                        sucChua = p.ToiDaO,
                        soNguoiDangO = p.HienO,
                        conLai = p.ToiDaO - p.HienO
                    })
                    .OrderBy(p => p.maP)
                    .ToListAsync();

                Console.WriteLine($"✅ GetPhongTrong: Trả về {danhSachPhong.Count} phòng");
                return Json(danhSachPhong);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetPhongTrong: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== API: KIỂM TRA PHÒNG TRỐNG ==========
        [HttpGet]
        public async Task<IActionResult> KiemTraPhongTrong(int maPhong)
        {
            try
            {
                var phong = await _context.Phongs.FindAsync(maPhong);
                if (phong == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng" });

                var toiDa = phong.ToiDaO;
                var hienO = phong.HienO;
                var soChoConLai = toiDa - hienO;

                if (soChoConLai < 0) soChoConLai = 0;

                return Json(new
                {
                    success = true,
                    conTrong = soChoConLai > 0,
                    soChoConLai = soChoConLai,
                    thongTin = new
                    {
                        maP = phong.MaP,
                        sucChua = toiDa,
                        soNguoiDangO = hienO
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in KiemTraPhongTrong: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ========== API: KIỂM TRA HỢP ĐỒNG TRÙNG ==========
        [HttpPost]
        public async Task<IActionResult> KiemTraHopDongTrung(int msv, DateTime ngayBatDau, DateTime ngayKetThuc)
        {
            try
            {
                var ngayBatDauDateOnly = DateOnly.FromDateTime(ngayBatDau);
                var ngayKetThucDateOnly = DateOnly.FromDateTime(ngayKetThuc);

                var hopDongTrung = await _context.HopDongPhongs
                    .Include(h => h.MaPNavigation)
                    .FirstOrDefaultAsync(h => h.Msv == msv &&
                                        h.TrangThaiHd == "Đăng Kí Thành Công" &&
                                        ((h.NgayBatDau <= ngayBatDauDateOnly && h.NgayKetThuc >= ngayBatDauDateOnly) ||
                                         (h.NgayBatDau <= ngayKetThucDateOnly && h.NgayKetThuc >= ngayKetThucDateOnly)));

                if (hopDongTrung != null)
                {
                    return Json(new
                    {
                        coTrung = true,
                        message = $"Sinh viên đã có hợp đồng phòng {hopDongTrung.MaPNavigation?.MaP} từ {hopDongTrung.NgayBatDau:dd/MM/yyyy} đến {hopDongTrung.NgayKetThuc:dd/MM/yyyy}"
                    });
                }

                return Json(new { coTrung = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in KiemTraHopDongTrung: {ex.Message}");
                return Json(new { error = true, message = ex.Message });
            }
        }

        // ========== API: GET QUICK INFO (AJAX) ==========
        [HttpGet]
        public async Task<IActionResult> GetQuickInfo(int id)
        {
            try
            {
                var sinhVien = await _context.SinhViens.FindAsync(id);
                if (sinhVien == null)
                {
                    return NotFound();
                }

                // 🔹 Lấy hợp đồng mới nhất của sinh viên, có phòng (nếu có)
                var hopDong = await _context.HopDongPhongs
                    .Include(h => h.MaPNavigation)
                    .Where(h => h.Msv == id)
                    .OrderByDescending(h => h.NgayKi)
                    .FirstOrDefaultAsync();

                // 🔹 Nếu không có hợp đồng => hiển thị "Chưa có phòng"
                string phong = "Chưa có phòng";
                string trangThai = "Chưa có phòng";

                if (hopDong != null && hopDong.MaPNavigation != null)
                {
                    phong = hopDong.MaPNavigation.MaP.ToString();
                    trangThai = hopDong.TrangThaiHd switch
                    {
                        "Đăng Kí Thành Công" or "Đang Ở" => "Đang ở KTX",
                        _ => "Đã đăng ký"
                    };
                }

                return Json(new
                {
                    msv = sinhVien.Msv,
                    hoTen = sinhVien.HoTen,
                    gioiTinh = sinhVien.GioiTinh,
                    ngaySinh = sinhVien.NgaySinh.ToString("dd/MM/yyyy"),
                    sdt = sinhVien.Sdt,
                    email = sinhVien.Email,
                    khoa = sinhVien.Khoa,
                    avatar = sinhVien.Avatar,
                    phong,
                    trangThai
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in GetQuickInfo: {ex.Message}");
                return Json(new { error = true, message = ex.Message });
            }
        }

        // ========== UPLOAD AVATAR ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(int msv, IFormFile avatarFile)
        {
            try
            {
                if (avatarFile == null || avatarFile.Length == 0)
                {
                    TempData["Error"] = "Vui lòng chọn file ảnh!";
                    return RedirectToAction("Details", new { id = msv });
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(avatarFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)!";
                    return RedirectToAction("Details", new { id = msv });
                }

                var fileName = $"avatar_{msv}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(fileStream);
                }

                var sinhVien = await _context.SinhViens.FindAsync(msv);
                if (sinhVien != null)
                {
                    if (!string.IsNullOrEmpty(sinhVien.Avatar))
                    {
                        var oldFilePath = Path.Combine(uploadsFolder, sinhVien.Avatar);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    sinhVien.Avatar = fileName;
                    _context.SinhViens.Update(sinhVien);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "✅ Cập nhật avatar thành công!";
                }

                return RedirectToAction("Details", new { id = msv });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR in UploadAvatar: {ex.Message}");
                TempData["Error"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction("Details", new { id = msv });
            }
        }

        // ========== HELPER: HASH PASSWORD ==========
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}