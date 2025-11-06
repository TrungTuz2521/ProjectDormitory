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

        #region Trang chính

        // GET: ThanhToan
        public async Task<IActionResult> Index(int? id)
        {
            // Lấy Mã SV từ ClaimTypes.NameIdentifier
            var msvClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(msvClaim) || !int.TryParse(msvClaim, out int msv))
            {
                return RedirectToAction("Login", "Account");
            }

            // Nếu không có id, lấy hợp đồng mới nhất của sinh viên đang đăng nhập
            if (!id.HasValue)
            {
                var latestHopDong = await _context.HopDongPhongs
                    .Include(h => h.MsvNavigation)
                    .Include(h => h.MaPNavigation)
                    .Include(h => h.TienPhongs)
                    .Where(h => h.Msv == msv) // Lọc theo sinh viên đang đăng nhập
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
                TempData["ErrorMessage"] = "Không tìm thấy thông tin thanh toán.";
                return RedirectToAction("Index", "Home");
            }

            return View(viewModel);
        }

        #endregion

        #region Chi tiết thanh toán

        // GET: ThanhToan/ChiTiet/5
        public async Task<IActionResult> ChiTiet(int id)
        {
            // Lấy MSV từ Claims
            var msvClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(msvClaim) || !int.TryParse(msvClaim, out int msv))
            {
                return RedirectToAction("Login", "Account");
            }

            var viewModel = await GetChiTietViewModel(id, msv);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hợp đồng hoặc bạn không có quyền xem.";
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        /// <summary>
        /// ✅ Phương thức helper tạo ViewModel chi tiết thanh toán
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

            // Lấy thông tin tiền điện nước của phòng
            var tienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.MaP == hopDong.MaP)
                .OrderByDescending(t => t.MaHddn)
                .FirstOrDefaultAsync();

            // ✅ BƯỚC 1: Lấy số người trong phòng
            int soNguoiTrongPhong = await LaySoNguoiTrongPhong(hopDong.MaP);

            // ✅ BƯỚC 2: Lấy tổng tiền điện nước CỦA PHÒNG (chưa chia)
            decimal tongTienDienPhong = tienDienNuoc?.TienDien ?? 0;
            decimal tongTienNuocPhong = tienDienNuoc?.TienNuoc ?? 0;
            decimal tongTienDienNuocPhong = tongTienDienPhong + tongTienNuocPhong;

            // ✅ BƯỚC 3: Tính tiền điện nước MỖI NGƯỜI (đã chia)
            decimal tienDienMotNguoi = soNguoiTrongPhong > 1
                ? Math.Round(tongTienDienPhong / soNguoiTrongPhong, 0, MidpointRounding.AwayFromZero)
                : tongTienDienPhong;

            decimal tienNuocMotNguoi = soNguoiTrongPhong > 1
                ? Math.Round(tongTienNuocPhong / soNguoiTrongPhong, 0, MidpointRounding.AwayFromZero)
                : tongTienNuocPhong;

            // ✅ BƯỚC 4: Tính tiền phòng (KHÔNG chia)
            decimal tienPhong = hopDong.TienP ?? 0;

            // ✅ BƯỚC 5: Tính TỔNG CỘNG sinh viên này phải trả
            decimal tongCong = tienPhong + tienDienMotNguoi + tienNuocMotNguoi;

            // Lấy thông tin thanh toán
            var tienPhongRecord = hopDong.TienPhongs.FirstOrDefault();

            // ✅ Tạo ViewModel đầy đủ
            var viewModel = new ThanhToanDetailViewModel
            {
                // Thông tin hợp đồng
                MaHD = hopDong.MaHd,
                NgayBatDau = hopDong.NgayBatDau?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                NgayKetThuc = hopDong.NgayKetThuc?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                TrangThaiHopDong = hopDong.TrangThaiHd ?? "Chưa xác định",

                // Thông tin sinh viên
                MaSV = hopDong.Msv.ToString(),
                HoTen = hopDong.MsvNavigation.HoTen,
                SoDienThoai = hopDong.MsvNavigation.Sdt,
                Email = hopDong.MsvNavigation.Email ?? "",

                // Thông tin phòng
                MaPhong = hopDong.MaP.ToString(),
                TenPhong = $"Phòng {hopDong.MaP}",
                GiaPhong = hopDong.TienP ?? 0,

                // ✅ Thông tin điện - CHI TIẾT
                SoDien = tienDienNuoc?.SoDien,
                GiaDien = tienDienNuoc?.GiaDien,
                TongTienDienPhong = tongTienDienPhong,

                // ✅ Thông tin nước - CHI TIẾT
                SoNuoc = tienDienNuoc?.SoNuoc,
                GiaNuoc = tienDienNuoc?.GiaNuoc,

                TongTienNuocPhong = tongTienNuocPhong,

                // ✅ Tổng hợp điện nước
                ThoiGianGhi = tienDienNuoc?.Httdn?.ToDateTime(TimeOnly.MinValue),
                SoNguoiTrongPhong = soNguoiTrongPhong,
                TongTienDienNuocPhong = tongTienDienNuocPhong,

                // Danh sách dịch vụ
                DanhSachDichVu = new List<DichVuThanhToanViewModel>(),
                TongTienDichVu = 0,

                // ✅ Tổng tiền (Đã chia theo số người)
                TienPhong = tienPhong,              // KHÔNG chia
                TienDien = tienDienMotNguoi,        // ĐÃ chia
                TienNuoc = tienNuocMotNguoi,        // ĐÃ chia
                TongCong = tongCong,                // Tổng sinh viên này phải trả

                // Trạng thái thanh toán
                DaThanhToan = tienPhongRecord?.TrangThaiTtp == "Đã thanh toán",
                NgayThanhToan = tienPhongRecord?.NgayTtp?.ToDateTime(TimeOnly.MinValue),
                TrangThaiThanhToan = tienPhongRecord?.TrangThaiTtp ?? "Chưa thanh toán",

                // Thông tin ngân hàng
                ThongTinNganHang = GetDefaultBankInfo(),
                DanhSachNganHang = GetDanhSachNganHang()
            };

            return viewModel;
        }

        #endregion

        #region Lịch sử thanh toán

        // GET: ThanhToan/LichSu
        public async Task<IActionResult> LichSu()
        {
            // Lấy MSV từ Claims
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

            var lichSuThanhToan = new List<LichSuThanhToanViewModel>();

            foreach (var h in danhSachHopDong)
            {
                var tienDienNuoc = await _context.TienDienNuocs
                    .Where(t => t.MaP == h.MaP)
                    .OrderByDescending(t => t.MaHddn)
                    .FirstOrDefaultAsync();

                var tienPhongRecord = h.TienPhongs.FirstOrDefault();

                // ✅ Tính tổng tiền đã chia theo số người
                decimal tongTien = await TinhTongTienMotNguoi(h, tienDienNuoc);

                lichSuThanhToan.Add(new LichSuThanhToanViewModel
                {
                    MaHD = h.MaHd,
                    MaPhong = h.MaP.ToString(),
                    TenPhong = $"Phòng {h.MaP}",
                    NgayBatDau = h.NgayBatDau?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    NgayKetThuc = h.NgayKetThuc?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    TongTien = tongTien,
                    DaThanhToan = tienPhongRecord?.TrangThaiTtp == "Đã thanh toán",
                    NgayThanhToan = tienPhongRecord?.NgayTtp?.ToDateTime(TimeOnly.MinValue),
                    TrangThaiThanhToan = tienPhongRecord?.TrangThaiTtp ?? "Chưa thanh toán"
                });
            }

            var viewModel = new DanhSachLichSuViewModel
            {
                MaSV = msv.ToString(),
                HoTen = sinhVien.HoTen,
                DanhSachThanhToan = lichSuThanhToan,
                TongDaThanhToan = lichSuThanhToan.Where(l => l.DaThanhToan).Sum(l => l.TongTien),
                TongChuaThanhToan = lichSuThanhToan.Where(l => !l.DaThanhToan).Sum(l => l.TongTien),
                SoLanDaThanhToan = lichSuThanhToan.Count(l => l.DaThanhToan),
                SoLanChuaThanhToan = lichSuThanhToan.Count(l => !l.DaThanhToan)
            };

            return PartialView("LichSuPartial", viewModel);
        }

        #endregion

        #region Thanh toán QR

        // GET: ThanhToan/ThanhToanQR/5
        public async Task<IActionResult> ThanhToanQR(int id)
        {
            // Lấy MSV từ Claims
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

            var tienPhongRecord = hopDong.TienPhongs.FirstOrDefault();
            bool daThanhToan = tienPhongRecord?.TrangThaiTtp == "Đã thanh toán";

            if (daThanhToan)
            {
                TempData["ErrorMessage"] = "Hợp đồng này đã được thanh toán";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            var tienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.MaP == hopDong.MaP)
                .OrderByDescending(t => t.MaHddn)
                .FirstOrDefaultAsync();

            // ✅ Tính tổng tiền đã chia theo số người
            decimal tongTien = await TinhTongTienMotNguoi(hopDong, tienDienNuoc);
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
                // Tạo QR Code URL theo chuẩn VietQR
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

            // Lấy MSV từ Claims để kiểm tra quyền
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

            var tienPhongRecord = hopDong.TienPhongs.FirstOrDefault();

            if (tienPhongRecord != null && tienPhongRecord.TrangThaiTtp == "Đã thanh toán")
            {
                return Json(new { success = false, message = "Hợp đồng đã được thanh toán" });
            }

            try
            {
                // Cập nhật hoặc tạo mới record TienPhong
                if (tienPhongRecord != null)
                {
                    tienPhongRecord.TrangThaiTtp = "Đã thanh toán";
                    tienPhongRecord.NgayTtp = DateOnly.FromDateTime(model.NgayThanhToan);
                    _context.TienPhongs.Update(tienPhongRecord);
                }
                else
                {
                    // Tạo MaHdp mới (tự động tăng)
                    int maxMaHdp = await _context.TienPhongs
                        .Where(t => t.MaHd == hopDong.MaHd)
                        .Select(t => t.MaHdp)
                        .DefaultIfEmpty(0)
                        .MaxAsync();

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

                // Cập nhật trạng thái hợp đồng
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
        /// ✅ Lấy số người đang ở trong phòng từ trường HienO
        /// </summary>
        private async Task<int> LaySoNguoiTrongPhong(int maPhong)
        {
            var phong = await _context.Phongs
                .FirstOrDefaultAsync(p => p.MaP == maPhong);

            int soNguoi = phong?.HienO ?? 0;

            // ✅ Trả về ít nhất là 1 để tránh chia cho 0
            return soNguoi > 0 ? soNguoi : 1;
        }

        /// <summary>
        /// ✅ Tính tổng tiền MỘT NGƯỜI phải trả (đã chia điện nước theo số người)
        /// </summary>
        private async Task<decimal> TinhTongTienMotNguoi(HopDongPhong hopDong, TienDienNuoc tienDienNuoc)
        {
            // Tiền phòng (KHÔNG chia)
            decimal tienPhong = hopDong.TienP ?? 0;

            // Lấy số người trong phòng
            int soNguoiTrongPhong = await LaySoNguoiTrongPhong(hopDong.MaP);

            // Tính tiền điện nước đã chia
            decimal tongTienDien = tienDienNuoc?.TienDien ?? 0;
            decimal tongTienNuoc = tienDienNuoc?.TienNuoc ?? 0;

            decimal tienDienMotNguoi = soNguoiTrongPhong > 1
                ? Math.Round(tongTienDien / soNguoiTrongPhong, 0, MidpointRounding.AwayFromZero)
                : tongTienDien;

            decimal tienNuocMotNguoi = soNguoiTrongPhong > 1
                ? Math.Round(tongTienNuoc / soNguoiTrongPhong, 0, MidpointRounding.AwayFromZero)
                : tongTienNuoc;

            return tienPhong + tienDienMotNguoi + tienNuocMotNguoi;
        }

        #endregion
    }
}