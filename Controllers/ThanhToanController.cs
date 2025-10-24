using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KTX.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KTX.ViewModels;

namespace KTX.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly SinhVienKtxContext _context;

        public ThanhToanController(SinhVienKtxContext context)
        {
            _context = context;
        }

        private List<BankInfoViewModel> GetDanhSachNganHang()
        {
            return new List<BankInfoViewModel>
            {
                new BankInfoViewModel
                {
                    MaNganHang = "970416",
                    TenNganHang = "Ngân hàng TMCP Á Châu",
                    TenVietTat = "ACB",
                    Logo = "/images/banks/acb.png",
                    SoTaiKhoan = "1234567890",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "970415",
                    TenNganHang = "Ngân hàng TMCP Công thương Việt Nam",
                    TenVietTat = "VietinBank",
                    Logo = "/images/banks/vietinbank.png",
                    SoTaiKhoan = "1234567890",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "970422",
                    TenNganHang = "Ngân hàng TMCP Quân đội",
                    TenVietTat = "MB Bank",
                    Logo = "/images/banks/mbbank.png",
                    SoTaiKhoan = "1234567890",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "970418",
                    TenNganHang = "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam",
                    TenVietTat = "BIDV",
                    Logo = "/images/banks/bidv.png",
                    SoTaiKhoan = "1234567890",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "970405",
                    TenNganHang = "Ngân hàng TMCP Ngoại thương Việt Nam",
                    TenVietTat = "Vietcombank",
                    Logo = "/images/banks/vietcombank.png",
                    SoTaiKhoan = "1234567890",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "970407",
                    TenNganHang = "Ngân hàng TMCP Kỹ thương Việt Nam",
                    TenVietTat = "Techcombank",
                    Logo = "/images/banks/techcombank.png",
                    SoTaiKhoan = "1234567890",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "970423",
                    TenNganHang = "Ngân hàng TMCP Tiên Phong",
                    TenVietTat = "TPBank",
                    Logo = "/images/banks/tpbank.png",
                    SoTaiKhoan = "1234567890",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                },
                new BankInfoViewModel
                {
                    MaNganHang = "970403",
                    TenNganHang = "Ngân hàng TMCP Sài Gòn Thương Tín",
                    TenVietTat = "Sacombank",
                    Logo = "/images/banks/sacombank.png",
                    SoTaiKhoan = "1234567890",
                    TenTaiKhoan = "CONG TY KTX",
                    ChiNhanh = "Chi nhánh Hà Nội"
                }
            };
        }

        private BankInfoViewModel GetDefaultBankInfo()
        {
            return GetDanhSachNganHang().First();
        }


        public async Task<IActionResult> Index(int? id)
        {
            // Nếu không có id, lấy hợp đồng mới nhất (giả sử có xác thực để lấy Mã SV)
            if (!id.HasValue)
            {
                var latestHopDong = await _context.HopDongPhongs
                    .Include(h => h.MsvNavigation)
                    .Include(h => h.MaPNavigation)
                    .Include(h => h.TienPhongs)
                    .OrderByDescending(h => h.NgayBatDau)
                    .FirstOrDefaultAsync();

                if (latestHopDong == null)
                {
                    return NotFound("Không tìm thấy hợp đồng nào.");
                }
                id = latestHopDong.MaHd;
            }

            var result = await ChiTiet(id.Value);
            if (result is ViewResult viewResult)
            {
                var model = viewResult.Model as ThanhToanDetailViewModel;
                model.DanhSachNganHang = GetDanhSachNganHang(); // Thêm danh sách ngân hàng
                return View(model);
            }
            return result;
        }

        // GET: ThanhToan/ChiTiet/5
        public async Task<IActionResult> ChiTiet(int id)
        {
            var hopDong = await _context.HopDongPhongs
                .Include(h => h.MsvNavigation)
                .Include(h => h.MaPNavigation)
                .Include(h => h.TienPhongs)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hopDong == null)
            {
                return NotFound();
            }

            // Lấy thông tin tiền điện nước của phòng
            var tienDienNuoc = await _context.TienDienNuocs
                .Where(t => t.MaP == hopDong.MaP)
                .OrderByDescending(t => t.MaHddn)
                .FirstOrDefaultAsync();

            // Tính tiền
            decimal tienPhong = hopDong.TienP ?? 0;
            decimal tienDien = tienDienNuoc?.TienDien ?? 0;
            decimal tienNuoc = tienDienNuoc?.TienNuoc ?? 0;

            // Lấy tổng tiền phòng từ TienPhong
            var tienPhongRecord = hopDong.TienPhongs.FirstOrDefault();
            decimal tongTienPhong = tienPhongRecord?.TongTienP ?? 0;

            decimal tongCong = tienPhong + tienDien + tienNuoc;

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

                // Thông tin điện nước
                SoDien = tienDienNuoc?.SoDien,
                SoNuoc = tienDienNuoc?.SoNuoc,
                GiaDien = tienDienNuoc?.GiaDien,
                GiaNuoc = tienDienNuoc?.GiaNuoc,
                ThoiGianGhi = tienDienNuoc?.Httdn?.ToDateTime(TimeOnly.MinValue),

                // Danh sách dịch vụ (nếu có)
                DanhSachDichVu = new List<DichVuThanhToanViewModel>(),

                // Tổng tiền
                TienPhong = tienPhong,
                TienDien = tienDien,
                TienNuoc = tienNuoc,
                TongTienDichVu = 0,
                TongCong = tongCong,

                // Trạng thái thanh toán
                DaThanhToan = tienPhongRecord?.TrangThaiTtp == "Đã thanh toán",
                NgayThanhToan = tienPhongRecord?.NgayTtp?.ToDateTime(TimeOnly.MinValue),
                TrangThaiThanhToan = tienPhongRecord?.TrangThaiTtp ?? "Chưa thanh toán",

                // Thông tin ngân hàng mặc định
                ThongTinNganHang = GetDefaultBankInfo()
            };

            return View(viewModel);
        }

        // GET: ThanhToan/LichSu
        public async Task<IActionResult> LichSu(string maSV)
        {
            if (string.IsNullOrEmpty(maSV))
            {
                return BadRequest("Mã sinh viên không hợp lệ");
            }

            if (!int.TryParse(maSV, out int msvInt))
            {
                return BadRequest("Mã sinh viên không hợp lệ");
            }

            var sinhVien = await _context.SinhViens
                .FirstOrDefaultAsync(sv => sv.Msv == msvInt);

            if (sinhVien == null)
            {
                return NotFound("Không tìm thấy sinh viên");
            }

            var danhSachHopDong = await _context.HopDongPhongs
                .Include(h => h.MaPNavigation)
                .Include(h => h.TienPhongs)
                .Where(h => h.Msv == msvInt)
                .OrderByDescending(h => h.NgayBatDau)
                .ToListAsync();

            var lichSuThanhToan = new List<LichSuThanhToanViewModel>();

            foreach (var h in danhSachHopDong)
            {
                var tienDienNuoc = await _context.TienDienNuocs
                    .Where(t => t.MaP == h.MaP)
                    .OrderByDescending(t => t.MaHddn)
                    .FirstOrDefaultAsync() ?? new TienDienNuoc();

                var tienPhongRecord = h.TienPhongs.FirstOrDefault();

                lichSuThanhToan.Add(new LichSuThanhToanViewModel
                {
                    MaHD = h.MaHd,
                    MaPhong = h.MaP.ToString(),
                    TenPhong = $"Phòng {h.MaP}",
                    NgayBatDau = h.NgayBatDau?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    NgayKetThuc = h.NgayKetThuc?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    TongTien = TinhTongTien(h, tienDienNuoc),
                    DaThanhToan = tienPhongRecord?.TrangThaiTtp == "Đã thanh toán",
                    NgayThanhToan = tienPhongRecord?.NgayTtp?.ToDateTime(TimeOnly.MinValue),
                    TrangThaiThanhToan = tienPhongRecord?.TrangThaiTtp ?? "Chưa thanh toán"
                });
            }

            var viewModel = new DanhSachLichSuViewModel
            {
                MaSV = maSV,
                HoTen = sinhVien.HoTen,
                DanhSachThanhToan = lichSuThanhToan,
                TongDaThanhToan = lichSuThanhToan.Where(l => l.DaThanhToan).Sum(l => l.TongTien),
                TongChuaThanhToan = lichSuThanhToan.Where(l => !l.DaThanhToan).Sum(l => l.TongTien),
                SoLanDaThanhToan = lichSuThanhToan.Count(l => l.DaThanhToan),
                SoLanChuaThanhToan = lichSuThanhToan.Count(l => !l.DaThanhToan)
            };

            return PartialView("LichSuPartial", viewModel);
        }

        // GET: ThanhToan/ThanhToanQR/5
        public async Task<IActionResult> ThanhToanQR(int id)
        {
            var hopDong = await _context.HopDongPhongs
                .Include(h => h.MsvNavigation)
                .Include(h => h.TienPhongs)
                .FirstOrDefaultAsync(h => h.MaHd == id);

            if (hopDong == null)
            {
                return NotFound();
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

            decimal tongTien = TinhTongTien(hopDong, tienDienNuoc);
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
                return BadRequest("Vui lòng chọn ngân hàng");
            }

            var nganHang = GetDanhSachNganHang().FirstOrDefault(nh => nh.MaNganHang == model.MaNganHang);

            if (nganHang == null)
            {
                return BadRequest("Ngân hàng không hợp lệ");
            }

            // Tạo QR Code URL theo chuẩn VietQR
            string qrUrl = $"https://img.vietqr.io/image/{model.MaNganHang}-{nganHang.SoTaiKhoan}-compact2.jpg?" +
                          $"amount={model.SoTien}&addInfo={Uri.EscapeDataString(model.NoiDung)}&accountName={Uri.EscapeDataString(nganHang.TenTaiKhoan)}";

            return Json(new
            {
                success = true,
                qrCodeUrl = qrUrl,
                nganHang = nganHang
            });
        }

        // POST: ThanhToan/XacNhan
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(XacNhanThanhToanViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var hopDong = await _context.HopDongPhongs
                .Include(h => h.TienPhongs)
                .FirstOrDefaultAsync(h => h.MaHd == model.MaHD);

            if (hopDong == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hợp đồng" });
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
                }
                else
                {
                    var newTienPhong = new TienPhong
                    {
                        MaHdp = hopDong.MaHd * 100 + 1, // Tạo ID tạm thời
                        MaHd = hopDong.MaHd,
                        TongTienP = hopDong.TienP,
                        TrangThaiTtp = "Đã thanh toán",
                        NgayTtp = DateOnly.FromDateTime(model.NgayThanhToan),
                        HanTtp = hopDong.NgayKetThuc
                    };
                    _context.TienPhongs.Add(newTienPhong);
                }

                // Cập nhật trạng thái hợp đồng
                hopDong.TrangThaiHd = "Đã thanh toán";

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Xác nhận thanh toán thành công",
                    redirectUrl = Url.Action(nameof(ChiTiet), new { id = model.MaHD })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Phương thức hỗ trợ
        private decimal TinhTongTien(HopDongPhong hopDong, TienDienNuoc tienDienNuoc)
        {
            decimal tienPhong = hopDong.TienP ?? 0;
            decimal tienDien = tienDienNuoc?.TienDien ?? 0;
            decimal tienNuoc = tienDienNuoc?.TienNuoc ?? 0;

            return tienPhong + tienDien + tienNuoc;
        }

        
    }
}