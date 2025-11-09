using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using KTX.Entities;
using KTX.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KTX.Controllers
{
    [Authorize(Roles = "SinhVien")]
    public class ThanhToanController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public ThanhToanController(SinhVienKtxContext context)
        {
            _context = context;
        }

        #region Danh sách ngân hàng

        private List<BankInfoViewModel> GetDanhSachNganHang()
        {
            return new List<BankInfoViewModel>
            {
                new BankInfoViewModel
                {
                    MaNganHang = "MBBank",
                    TenNganHang = "Ngân hàng TMCP Quân đội",
                    TenVietTat = "MB Bank",
                    Logo = "/images/banks/mbbank.png",
                    SoTaiKhoan = "0399955675",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "BIDVBank",
                    TenNganHang = "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam",
                    TenVietTat = "BIDV",
                    Logo = "/images/banks/bidv.png",
                    SoTaiKhoan = "0399955675",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "TCBank",
                    TenNganHang = "Ngân hàng TMCP Kỹ thương Việt Nam",
                    TenVietTat = "Techcombank",
                    Logo = "/images/banks/techcombank.png",
                    SoTaiKhoan = "21112005200510",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
            };
        }

        private BankInfoViewModel GetDefaultBankInfo()
        {
            return GetDanhSachNganHang().First();
        }

        #endregion
        #region Phương thức kiểm tra thanh toán chi tiết

        /// <summary>
        /// ✅ Kiểm tra RIÊNG tiền phòng đã thanh toán chưa
        /// </summary>
        private async Task<bool> KiemTraTienPhongDaThu(int maHD)
        {
            var tienPhong = await _context.TienPhongs
                .FirstOrDefaultAsync(t => t.MaHd == maHD);

            return tienPhong?.TrangThaiTtp == "Đã thanh toán";
        }

       
        /// <summary>
        /// ✅ FIXED: Kiểm tra RIÊNG điện nước đã thanh toán chưa
        /// Xử lý đúng logic: 
        /// - Chưa có hóa đơn = không cần thanh toán = trả về true (không chặn)
        /// - Có hóa đơn = kiểm tra trạng thái thanh toán
        /// </summary>
        private async Task<bool> KiemTraDienNuocDaThu(int maHD, int msv)
        {
            var hopDong = await _context.HopDongPhongs
                .FirstOrDefaultAsync(h => h.MaHd == maHD);

            if (hopDong == null) return false;

            var tienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.MaP == hopDong.MaP)
                .OrderByDescending(t => t.DotTtdn)
                .ThenByDescending(t => t.MaHddn)
                .FirstOrDefaultAsync();

            // ✅ QUAN TRỌNG: Nếu chưa có hóa đơn điện nước
            // → Nghĩa là chưa đến kỳ thanh toán điện nước
            // → Trả về TRUE để KHÔNG CHẶN thanh toán tiền phòng
            if (tienDienNuoc == null)
            {
                return true;
            }

            // ✅ Có hóa đơn → kiểm tra chi tiết thanh toán của sinh viên
            var chiTiet = await _context.ChiTietThanhToanDienNuocs
                .AsNoTracking()
                .Where(ct => ct.MaHddn == tienDienNuoc.MaHddn && ct.Msv == msv)
                .Select(ct => new { TrangThai = ct.TrangThai ?? "Chưa thanh toán" })
                .FirstOrDefaultAsync();

 
            return chiTiet?.TrangThai == "Đã thanh toán";
        }

        #endregion


        // Nếu không có id, lấy hợp đồng mới nhất của sinh viên đang đăng nhập
        public async Task<IActionResult> Index(int? id)
        {
            var msvClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(msvClaim) || !int.TryParse(msvClaim, out int msv))
            {
                return RedirectToAction("Login", "Account");
            }

            // Nếu không có id, lấy hợp đồng mới nhất
            if (!id.HasValue)
            {
                var latestHopDong = await _context.HopDongPhongs
                    .Include(h => h.MsvNavigation)
                    .Include(h => h.MaPNavigation)
                    .Include(h => h.TienPhongs)
                    .Where(h => h.Msv == msv)
                    .OrderByDescending(h => h.NgayBatDau)
                    .FirstOrDefaultAsync();

                if (latestHopDong == null)
                {
                    TempData["ErrorMessage"] = "Bạn chưa có hợp đồng nào.";
                    return RedirectToAction("Index", "Home");
                }
                id = latestHopDong.MaHd;
            }
            else
            {
                // Kiểm tra quyền truy cập
                var hopDong = await _context.HopDongPhongs
                    .FirstOrDefaultAsync(h => h.MaHd == id.Value && h.Msv == msv);

                if (hopDong == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hợp đồng hoặc bạn không có quyền xem.";
                    return RedirectToAction("Index", "Home");
                }
            }

            // Lấy ViewModel đầy đủ
            var viewModel = await GetChiTietViewModel(id.Value, msv);

            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chi tiết thanh toán.";
                return RedirectToAction("Index", "Home");
            }

            // ✅ THÊM: Truyền thông tin trạng thái từng loại vào ViewBag
            ViewBag.TienPhongDaThu = await KiemTraTienPhongDaThu(id.Value);
            ViewBag.DienNuocDaThu = await KiemTraDienNuocDaThu(id.Value, msv);

            // ✅✅✅ THÊM DÒNG NÀY - Truyền danh sách ngân hàng vào ViewBag
            ViewBag.DanhSachNganHang = GetDanhSachNganHang();
            return View(viewModel);
        }

        // Lấy ViewModel đầy đủ
        /// <summary>
        /// ✅ SAFE VERSION: Phương thức helper tạo ViewModel chi tiết thanh toán
        /// Xử lý an toàn các giá trị NULL từ database
        /// </summary>
        private async Task<ThanhToanDetailViewModel> GetChiTietViewModel(int maHD, int msv)
        {
            var hopDong = await _context.HopDongPhongs
                .Include(h => h.MsvNavigation)
                .Include(h => h.MaPNavigation)
                .Include(h => h.TienPhongs)
                .FirstOrDefaultAsync(h => h.MaHd == maHD && h.Msv == msv);

            if (hopDong == null)
            {
                return null;
            }

            // ✅ Lấy hóa đơn điện nước MỚI NHẤT của phòng
            var tienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.MaP == hopDong.MaP)
                .OrderByDescending(t => t.DotTtdn)
                .ThenByDescending(t => t.MaHddn)
                .FirstOrDefaultAsync();

            // ✅ Lấy chi tiết thanh toán - SAFE QUERY (không load navigation properties)
            ChiTietThanhToanDienNuoc chiTietThanhToan = null;

            if (tienDienNuoc != null)
            {
                // Query an toàn - chỉ lấy các field cần thiết
                chiTietThanhToan = await _context.ChiTietThanhToanDienNuocs
                    .AsNoTracking() // Không track để tránh lỗi navigation
                    .Where(ct => ct.MaHddn == tienDienNuoc.MaHddn && ct.Msv == msv)
                    .Select(ct => new ChiTietThanhToanDienNuoc
                    {
                        MaCtttdn = ct.MaCtttdn,
                        MaHddn = ct.MaHddn,
                        Msv = ct.Msv,
                        SoTienPhai = ct.SoTienPhai,
                        SoTienDaTra = ct.SoTienDaTra,
                        TrangThai = ct.TrangThai ?? "Chưa thanh toán", // ✅ Handle NULL
                        NgayThanhToan = ct.NgayThanhToan,
                        GhiChu = ct.GhiChu ?? "" // ✅ Handle NULL
                    })
                    .FirstOrDefaultAsync();
            }

            // ✅ Số người trong phòng
            int soNguoiTrongPhong = await LaySoNguoiTrongPhong(hopDong.MaP);

            // ✅ Tổng tiền điện nước của PHÒNG (chưa chia)
            decimal tongTienDienPhong = tienDienNuoc?.TienDien ?? 0;
            decimal tongTienNuocPhong = tienDienNuoc?.TienNuoc ?? 0;
            decimal tongTienDienNuocPhong = tongTienDienPhong + tongTienNuocPhong;

            // ✅ Tiền điện nước của SINH VIÊN NÀY (đã chia)
            decimal tienDienMotNguoi = 0;
            decimal tienNuocMotNguoi = 0;

            if (chiTietThanhToan != null)
            {
                // Lấy từ bảng chi tiết (chính xác nhất)
                decimal soTienDienNuoc = chiTietThanhToan.SoTienPhai;

                // Tính tỷ lệ để hiển thị riêng điện và nước
                if (tongTienDienNuocPhong > 0)
                {
                    decimal tyLeDien = tongTienDienPhong / tongTienDienNuocPhong;
                    tienDienMotNguoi = Math.Round(soTienDienNuoc * tyLeDien, 0);
                    tienNuocMotNguoi = soTienDienNuoc - tienDienMotNguoi;
                }
            }
            else if (tienDienNuoc != null)
            {
                // Fallback: Tính chia đều nếu chưa có chi tiết
                tienDienMotNguoi = soNguoiTrongPhong > 1
                    ? Math.Round(tongTienDienPhong / soNguoiTrongPhong, 0)
                    : tongTienDienPhong;

                tienNuocMotNguoi = soNguoiTrongPhong > 1
                    ? Math.Round(tongTienNuocPhong / soNguoiTrongPhong, 0)
                    : tongTienNuocPhong;
            }

            // ✅ Tiền phòng (KHÔNG chia)
            decimal tienPhong = hopDong.TienP ?? 0;

            // ✅ TỔNG CỘNG sinh viên này phải trả
            decimal tongCong = tienPhong + tienDienMotNguoi + tienNuocMotNguoi;

            // Lấy thông tin thanh toán tiền phòng
            var tienPhongRecord = hopDong.TienPhongs.FirstOrDefault();

            // ✅ Trạng thái thanh toán điện nước của sinh viên

            // ✅ Trạng thái thanh toán - PHÂN BIỆT rõ ràng
            bool phongDaThu = tienPhongRecord?.TrangThaiTtp?.ToLower() == "đã thanh toán";

            bool dienNuocDaThu;
            bool coDienNuoc = tienDienNuoc != null;

            if (!coDienNuoc)
            {
                // Chưa có hóa đơn → không cần thanh toán điện nước
                dienNuocDaThu = true; // Không chặn thanh toán
            }
            else
            {
                // Có hóa đơn → kiểm tra trạng thái
                dienNuocDaThu = chiTietThanhToan?.TrangThai?.ToLower() == "đã thanh toán";
            }

            // ✅ Chỉ coi là "đã thanh toán đủ" khi:
            // - Tiền phòng đã thanh toán
            // - VÀ (chưa có điện nước HOẶC điện nước đã thanh toán)
            bool tatCaDaThu = phongDaThu && dienNuocDaThu;


            // ✅ Tạo ViewModel đầy đủ - SAFE với null coalescing
            var viewModel = new ThanhToanDetailViewModel
            {
                // Thông tin hợp đồng
                MaHD = hopDong.MaHd,
                NgayBatDau = hopDong.NgayBatDau?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                NgayKetThuc = hopDong.NgayKetThuc?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                TrangThaiHopDong = hopDong.TrangThaiHd ?? "Chưa xác định",

                // Thông tin sinh viên
                MaSV = hopDong.Msv.ToString(),
                HoTen = hopDong.MsvNavigation?.HoTen ?? "N/A",
                SoDienThoai = hopDong.MsvNavigation?.Sdt ?? "",
                Email = hopDong.MsvNavigation?.Email ?? "",

                // Thông tin phòng
                MaPhong = hopDong.MaP.ToString(),
                TenPhong = $"Phòng {hopDong.MaP}",
                GiaPhong = hopDong.TienP ?? 0,

                // ✅ Thông tin điện nước - CHI TIẾT
                SoDien = tienDienNuoc?.SoDien,
                GiaDien = tienDienNuoc?.GiaDien,
                TongTienDienPhong = tongTienDienPhong,

                SoNuoc = tienDienNuoc?.SoNuoc,
                GiaNuoc = tienDienNuoc?.GiaNuoc,
                TongTienNuocPhong = tongTienNuocPhong,

                // ✅ Tổng hợp
                ThoiGianGhi = tienDienNuoc?.Httdn?.ToDateTime(TimeOnly.MinValue),
                SoNguoiTrongPhong = soNguoiTrongPhong,
                TongTienDienNuocPhong = tongTienDienNuocPhong,

                // Danh sách dịch vụ
                DanhSachDichVu = new List<DichVuThanhToanViewModel>(),
                TongTienDichVu = 0,

                // ✅ Tổng tiền (của sinh viên này)
                // ✅ Tổng tiền (của sinh viên này)
                TienPhong = tienPhong,
                TienDien = tienDienMotNguoi,
                TienNuoc = tienNuocMotNguoi,
                TongCong = tongCong,

                // ✅ QUAN TRỌNG: Các property về trạng thái
                CoDienNuoc = coDienNuoc, // Flag để biết có hóa đơn điện nước không

                // ✅ THÊM: Trạng thái riêng từng loại
                TienPhongDaThanhToan = phongDaThu,
                DienNuocDaThanhToan = dienNuocDaThu,

                DaThanhToan = tatCaDaThu, // Tổng hợp
                NgayThanhToan = chiTietThanhToan?.NgayThanhToan?.ToDateTime(TimeOnly.MinValue)
                 ?? tienPhongRecord?.NgayTtp?.ToDateTime(TimeOnly.MinValue),

                // ✅ Trạng thái hiển thị chi tiết hơn
                TrangThaiThanhToan = !coDienNuoc
        ? (phongDaThu ? "Đã thanh toán tiền phòng" : "Chưa thanh toán tiền phòng")
        : (tatCaDaThu ? "Đã thanh toán đầy đủ" : "Chưa thanh toán đủ"),
                // Thông tin ngân hàng
                ThongTinNganHang = GetDefaultBankInfo(),
                DanhSachNganHang = GetDanhSachNganHang()
            };

            return viewModel;
        }



        #region Lịch sử thanh toán

        // GET: ThanhToan/LichSu
        #region Lịch sử thanh toán

        // GET: ThanhToan/LichSu
        public async Task<IActionResult> LichSu()
        {
            var msvClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(msvClaim) || !int.TryParse(msvClaim, out int msv))
            {
                return BadRequest("Không xác định được sinh viên");
            }

            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Msv == msv);

            if (sinhVien == null)
            {
                return NotFound("Không tìm thấy sinh viên");
            }

            var danhSachHopDong = await _context.HopDongPhongs
                .Include(h => h.MaPNavigation)
                .Include(h => h.TienPhongs)
                .Where(h => h.Msv == msv)
                .OrderByDescending(h => h.NgayBatDau)
                .ToListAsync();

            // ✅ Khởi tạo List đúng kiểu
            var lichSuThanhToan = new List<LichSuThanhToanViewModel>();

            foreach (var hopDong in danhSachHopDong)
            {
                // ✅ 1. TIỀN PHÒNG
                var tienPhongRecord = hopDong.TienPhongs.FirstOrDefault();

                lichSuThanhToan.Add(new LichSuThanhToanViewModel
                {
                    MaHD = hopDong.MaHd,
                    MaPhong = hopDong.MaP.ToString(),
                    TenPhong = $"Phòng {hopDong.MaP}",
                    NgayBatDau = hopDong.NgayBatDau?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    NgayKetThuc = hopDong.NgayKetThuc?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,

                    LoaiThanhToan = "Tiền phòng",
                    MaHoaDon = $"HD{hopDong.MaHd}",
                    KyThanhToan = hopDong.NgayBatDau.HasValue
                        ? $"Tháng {hopDong.NgayBatDau.Value.Month:00}/{hopDong.NgayBatDau.Value.Year}"
                        : "N/A",
                    GhiChu = "Tiền thuê phòng theo hợp đồng",

                    TongTien = hopDong.TienP ?? 0,
                    DaThanhToan = tienPhongRecord?.TrangThaiTtp == "Đã thanh toán",
                    NgayThanhToan = tienPhongRecord?.NgayTtp?.ToDateTime(TimeOnly.MinValue),
                    TrangThaiThanhToan = tienPhongRecord?.TrangThaiTtp == "Đã thanh toán"
                        ? "Đã thanh toán"
                        : "Chưa thanh toán"
                });

                // ✅ 2. TIỀN ĐIỆN NƯỚC
                var tienDienNuoc = await _context.TienDienNuocs
                    .Where(t => t.MaP == hopDong.MaP)
                    .OrderByDescending(t => t.DotTtdn)
                    .ThenByDescending(t => t.MaHddn)
                    .FirstOrDefaultAsync();

                if (tienDienNuoc != null)
                {
                    var chiTietDienNuoc = await _context.ChiTietThanhToanDienNuocs
                        .AsNoTracking()
                        .Where(ct => ct.MaHddn == tienDienNuoc.MaHddn && ct.Msv == msv)
                        .Select(ct => new
                        {
                            ct.SoTienPhai,
                            ct.SoTienDaTra,
                            TrangThai = ct.TrangThai ?? "Chưa thanh toán",
                            ct.NgayThanhToan
                        })
                        .FirstOrDefaultAsync();

                    decimal tienDienNuocMotNguoi = 0;
                    if (chiTietDienNuoc != null)
                    {
                        tienDienNuocMotNguoi = chiTietDienNuoc.SoTienPhai;
                    }
                    else
                    {
                        int soNguoi = await LaySoNguoiTrongPhong(hopDong.MaP);
                        decimal tongDienNuoc = (tienDienNuoc.TienDien ?? 0) + (tienDienNuoc.TienNuoc ?? 0);
                        tienDienNuocMotNguoi = soNguoi > 1
                            ? Math.Round(tongDienNuoc / soNguoi, 0)
                            : tongDienNuoc;
                    }

                    lichSuThanhToan.Add(new LichSuThanhToanViewModel
                    {
                        MaHD = hopDong.MaHd,
                        MaPhong = hopDong.MaP.ToString(),
                        TenPhong = $"Phòng {hopDong.MaP}",
                        NgayBatDau = hopDong.NgayBatDau?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                        NgayKetThuc = hopDong.NgayKetThuc?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,

                        LoaiThanhToan = "Tiền điện nước",
                        MaHoaDon = $"HDDN{tienDienNuoc.MaHddn}",
                        KyThanhToan = tienDienNuoc.Httdn.HasValue
                            ? $"Tháng {tienDienNuoc.Httdn.Value.Month:00}/{tienDienNuoc.Httdn.Value.Year}"
                            : "N/A",
                        GhiChu = $"Điện: {tienDienNuoc.SoDien ?? 0} kWh, Nước: {tienDienNuoc.SoNuoc ?? 0} m³",

                        TongTien = tienDienNuocMotNguoi,
                        DaThanhToan = chiTietDienNuoc?.TrangThai == "Đã thanh toán",
                        NgayThanhToan = chiTietDienNuoc?.NgayThanhToan?.ToDateTime(TimeOnly.MinValue),
                        TrangThaiThanhToan = chiTietDienNuoc?.TrangThai == "Đã thanh toán"
                            ? "Đã thanh toán"
                            : "Chưa thanh toán"
                    });
                }
            }

            var viewModel = new DanhSachLichSuViewModel
            {
                MaSV = msv.ToString(),
                HoTen = sinhVien.HoTen ?? "N/A",
                DanhSachThanhToan = lichSuThanhToan,
                TongDaThanhToan = lichSuThanhToan.Where(l => l.DaThanhToan).Sum(l => l.TongTien),
                TongChuaThanhToan = lichSuThanhToan.Where(l => !l.DaThanhToan).Sum(l => l.TongTien),
                SoLanDaThanhToan = lichSuThanhToan.Count(l => l.DaThanhToan),
                SoLanChuaThanhToan = lichSuThanhToan.Count(l => !l.DaThanhToan)
            };

            return PartialView("LichSuPartial", viewModel);
        }

        #endregion

        #endregion

        #region Thanh toán QR

        // GET: ThanhToan/ThanhToanQR/5
        public async Task<IActionResult> ThanhToanQR(int id)
        {
            var msvClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(msvClaim) || !int.TryParse(msvClaim, out int msv))
            {
                return RedirectToAction("Login", "Account");
            }

            var hopDong = await _context.HopDongPhongs
                .Include(h => h.MsvNavigation)
                .Include(h => h.TienPhongs)
                .FirstOrDefaultAsync(h => h.MaHd == id && h.Msv == msv);

            if (hopDong == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hợp đồng hoặc bạn không có quyền xem.";
                return RedirectToAction("Index");
            }

            // ✅ Kiểm tra đã thanh toán chưa
            bool daThanhToan = await KiemTraDaThanhToan(hopDong, msv);

            if (daThanhToan)
            {
                TempData["ErrorMessage"] = "Hợp đồng này đã được thanh toán";
                return RedirectToAction(nameof(Index), new { id });
            }

            // ✅ Tính tổng tiền theo chi tiết mới
            decimal tongTien = await TinhTongTienMotNguoi(hopDong, msv);
            string noiDung = $"THANHTOAN HD{id} P{hopDong.MaP}";

            var viewModel = new ThanhToanQRViewModel
            {
                MaHD = id,
                SoTien = tongTien,
                NoiDung = noiDung,
                TenKhachHang = hopDong.MsvNavigation.HoTen,
                MaPhong = hopDong.MaP.ToString(),
                DanhSachNganHang = GetDanhSachNganHang()
            };

            return View(viewModel);
        }

        // POST: ThanhToan/TaoQRCode
        [HttpPost]
        public IActionResult TaoQRCode([FromBody] ThanhToanQRViewModel model)
        {
            if (string.IsNullOrEmpty(model.MaNganHang))
            {
                return BadRequest(new { success = false, message = "Vui lòng chọn ngân hàng" });
            }

            var nganHang = GetDanhSachNganHang().FirstOrDefault(nh => nh.MaNganHang == model.MaNganHang);

            if (nganHang == null)
            {
                return BadRequest(new { success = false, message = "Ngân hàng không hợp lệ" });
            }

            try
            {
                string qrUrl = $"https://img.vietqr.io/image/{model.MaNganHang}-{nganHang.SoTaiKhoan}-compact2.jpg?" +
                              $"amount={model.SoTien}&addInfo={Uri.EscapeDataString(model.NoiDung)}&accountName={Uri.EscapeDataString(nganHang.TenTaiKhoan)}";

                return Json(new
                {
                    success = true,
                    qrCodeUrl = qrUrl,
                    nganHang
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Lỗi tạo QR: {ex.Message}" });
            }
        }

        #endregion

        #region Xác nhận thanh toán

        // POST: ThanhToan/XacNhan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan([FromBody] XacNhanThanhToanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var msvClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(msvClaim) || !int.TryParse(msvClaim, out int msv))
            {
                return Json(new { success = false, message = "Không xác định được sinh viên" });
            }

            var hopDong = await _context.HopDongPhongs
                .Include(h => h.TienPhongs)
                .FirstOrDefaultAsync(h => h.MaHd == model.MaHD && h.Msv == msv);

            if (hopDong == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hợp đồng hoặc bạn không có quyền thao tác" });
            }

            // ✅ Kiểm tra đã thanh toán chưa
            bool daThanhToan = await KiemTraDaThanhToan(hopDong, msv);
            if (daThanhToan)
            {
                return Json(new { success = false, message = "Hợp đồng đã được thanh toán" });
            }

            try
            {
                // ✅ 1. Cập nhật tiền phòng
                var tienPhongRecord = hopDong.TienPhongs.FirstOrDefault();
                if (tienPhongRecord != null)
                {
                    tienPhongRecord.TrangThaiTtp = "Đã thanh toán";
                    tienPhongRecord.NgayTtp = DateOnly.FromDateTime(model.NgayThanhToan);
                    _context.TienPhongs.Update(tienPhongRecord);
                }
                else
                {
                    int maxMaHdp = await _context.TienPhongs.AnyAsync()
                        ? await _context.TienPhongs.MaxAsync(t => t.MaHdp)
                        : 0;

                    var newTienPhong = new TienPhong
                    {
                        MaHdp = maxMaHdp + 1,
                        MaHd = hopDong.MaHd,
                        TongTienP = model.SoTien,
                        TrangThaiTtp = "Đã thanh toán",
                        NgayTtp = DateOnly.FromDateTime(model.NgayThanhToan),
                        HanTtp = hopDong.NgayKetThuc
                    };
                    _context.TienPhongs.Add(newTienPhong);
                }

                // ✅ 2. Cập nhật chi tiết điện nước của sinh viên
                var tienDienNuoc = await _context.TienDienNuocs
                    .Where(t => t.MaP == hopDong.MaP)
                    .OrderByDescending(t => t.DotTtdn)
                    .ThenByDescending(t => t.MaHddn)
                    .FirstOrDefaultAsync();

                if (tienDienNuoc != null)
                {
                    var chiTietThanhToan = await _context.ChiTietThanhToanDienNuocs
                        .FirstOrDefaultAsync(ct => ct.MaHddn == tienDienNuoc.MaHddn && ct.Msv == msv);

                    if (chiTietThanhToan != null)
                    {
                        chiTietThanhToan.TrangThai = "Đã thanh toán";
                        chiTietThanhToan.SoTienDaTra = chiTietThanhToan.SoTienPhai;
                        chiTietThanhToan.NgayThanhToan = DateOnly.FromDateTime(model.NgayThanhToan);
                        _context.ChiTietThanhToanDienNuocs.Update(chiTietThanhToan);

                        // ✅ 3. Kiểm tra tất cả sinh viên trong phòng đã thanh toán chưa
                        var tatCaDaThu = await _context.ChiTietThanhToanDienNuocs
                            .Where(ct => ct.MaHddn == tienDienNuoc.MaHddn)
                            .AllAsync(ct => ct.TrangThai == "Đã thanh toán");

                        if (tatCaDaThu)
                        {
                            tienDienNuoc.TrangThaiTtdn = "Đã thanh toán";
                            tienDienNuoc.NgayTtdn = DateOnly.FromDateTime(model.NgayThanhToan);
                            _context.TienDienNuocs.Update(tienDienNuoc);
                        }
                    }
                }

                // ✅ 4. Cập nhật trạng thái hợp đồng
                hopDong.TrangThaiHd = "Đã thanh toán";
                _context.HopDongPhongs.Update(hopDong);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Xác nhận thanh toán thành công! Cảm ơn bạn đã thanh toán đúng hạn.",
                    redirectUrl = Url.Action(nameof(Index), new { id = model.MaHD })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        #endregion

        #region Phương thức hỗ trợ

        /// <summary>
        /// Lấy số người đang ở trong phòng
        /// </summary>
        private async Task<int> LaySoNguoiTrongPhong(int maPhong)
        {
            var phong = await _context.Phongs
                .FirstOrDefaultAsync(p => p.MaP == maPhong);

            int soNguoi = phong?.HienO ?? 0;
            return soNguoi > 0 ? soNguoi : 1;
        }

        /// <summary>
        /// ✅ UPDATED: Tính tổng tiền MỘT SINH VIÊN phải trả
        /// Sử dụng ChiTietThanhToanDienNuoc để lấy số tiền chính xác
        /// </summary>
        /// <summary>
        /// ✅ FIXED: Tính tổng tiền MỘT SINH VIÊN phải trả
        /// Xử lý an toàn các giá trị NULL từ database
        /// </summary>
        private async Task<decimal> TinhTongTienMotNguoi(HopDongPhong hopDong, int msv)
        {
            // 1. Tiền phòng (KHÔNG chia)
            decimal tienPhong = hopDong.TienP ?? 0;

            // 2. Lấy hóa đơn điện nước mới nhất
            var tienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.MaP == hopDong.MaP)
                .OrderByDescending(t => t.DotTtdn)
                .ThenByDescending(t => t.MaHddn)
                .FirstOrDefaultAsync();

            decimal tienDienNuocMotNguoi = 0;

            if (tienDienNuoc != null)
            {
                // 3. ✅ Query an toàn - chỉ lấy các field cần thiết, không load navigation properties
                var chiTiet = await _context.ChiTietThanhToanDienNuocs
                    .AsNoTracking() // Không track để tránh lỗi
                    .Where(ct => ct.MaHddn == tienDienNuoc.MaHddn && ct.Msv == msv)
                    .Select(ct => new
                    {
                        ct.SoTienPhai,
                        ct.SoTienDaTra,
                        TrangThai = ct.TrangThai ?? "Chưa thanh toán" // ✅ Handle NULL
                    })
                    .FirstOrDefaultAsync();

                if (chiTiet != null)
                {
                    // ✅ Lấy từ bảng chi tiết (chính xác nhất)
                    tienDienNuocMotNguoi = chiTiet.SoTienPhai;
                }
                else
                {
                    // ✅ Fallback: Tính chia đều nếu chưa có chi tiết
                    int soNguoi = await LaySoNguoiTrongPhong(hopDong.MaP);
                    decimal tongDienNuoc = (tienDienNuoc.TienDien ?? 0) + (tienDienNuoc.TienNuoc ?? 0);

                    tienDienNuocMotNguoi = soNguoi > 1
                        ? Math.Round(tongDienNuoc / soNguoi, 0, MidpointRounding.AwayFromZero)
                        : tongDienNuoc;
                }
            }

            return tienPhong + tienDienNuocMotNguoi;
        }
        /// <summary>
        /// ✅ NEW: Kiểm tra sinh viên đã thanh toán đủ cả tiền phòng và điện nước chưa
        /// </summary>
        /// <summary>
        /// ✅ FIXED: Kiểm tra sinh viên đã thanh toán đủ cả tiền phòng và điện nước chưa
        /// </summary>
        /// <summary>
        /// ✅ FIXED: Kiểm tra trạng thái thanh toán chính xác
        /// </summary>
        /// <summary>
        /// ✅ FIXED: Kiểm tra trạng thái thanh toán theo nghiệp vụ
        /// - Nếu CHƯA CÓ hóa đơn điện nước: chỉ cần thanh toán tiền phòng
        /// - Nếu ĐÃ CÓ hóa đơn điện nước: phải thanh toán CẢ tiền phòng VÀ điện nước
        /// </summary>
        private async Task<bool> KiemTraDaThanhToan(HopDongPhong hopDong, int msv)
        {
            // 1. Kiểm tra tiền phòng (BẮT BUỘC)
            var tienPhongRecord = hopDong.TienPhongs?.FirstOrDefault();
            bool phongDaThu = tienPhongRecord?.TrangThaiTtp == "Đã thanh toán";

            // 2. Kiểm tra xem có hóa đơn điện nước không
            var tienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.MaP == hopDong.MaP)
                .OrderByDescending(t => t.DotTtdn)
                .ThenByDescending(t => t.MaHddn)
                .FirstOrDefaultAsync();

            // ✅ Nếu CHƯA CÓ hóa đơn điện nước → chỉ cần thanh toán tiền phòng
            if (tienDienNuoc == null)
            {
                return phongDaThu; // Chỉ cần tiền phòng đã thanh toán
            }

            // ✅ Nếu CÓ hóa đơn điện nước → phải thanh toán CẢ tiền phòng VÀ điện nước
            var chiTiet = await _context.ChiTietThanhToanDienNuocs
                .AsNoTracking()
                .Where(ct => ct.MaHddn == tienDienNuoc.MaHddn && ct.Msv == msv)
                .Select(ct => new
                {
                    TrangThai = ct.TrangThai ?? "Chưa thanh toán"
                })
                .FirstOrDefaultAsync();

            bool dienNuocDaThu = chiTiet?.TrangThai == "Đã thanh toán";

            // Phải thanh toán ĐỦ cả tiền phòng VÀ điện nước
            return phongDaThu && dienNuocDaThu;
        }
        #endregion

    }

}