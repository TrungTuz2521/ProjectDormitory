using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTX.ViewModels
{
    // ViewModel cho chi tiết thanh toán
    public class ThanhToanDetailViewModel
    {
        // Thông tin hợp đồng
        public int MaHD { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThaiHopDong { get; set; }

        // Thông tin sinh viên
        public string MaSV { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }

        // Thông tin phòng
        public string MaPhong { get; set; }
        public string TenPhong { get; set; }
        public decimal GiaPhong { get; set; }

        // ✅ Thông tin điện nước - CHI TIẾT
        public int? SoDien { get; set; }  // Số điện tiêu thụ (kWh)
        public int? SoNuoc { get; set; }  // Số nước tiêu thụ (m³)
        public decimal? GiaDien { get; set; }
        public decimal? GiaNuoc { get; set; }
        public DateTime? ThoiGianGhi { get; set; }

        // ✅ Chỉ số cũ/mới (nếu có trong database)
        public int? SoDienCu { get; set; }
        public int? SoDienMoi { get; set; }
        public int? SoNuocCu { get; set; }
        public int? SoNuocMoi { get; set; }

        // ✅ Thông tin phòng
        public int SoNguoiTrongPhong { get; set; } // Từ HienO của Phong

        // ✅ Tiền điện nước CỦA PHÒNG (chưa chia)
        public decimal TongTienDienPhong { get; set; }
        public decimal TongTienNuocPhong { get; set; }
        public decimal TongTienDienNuocPhong { get; set; }

        // Chi tiết dịch vụ
        public List<DichVuThanhToanViewModel> DanhSachDichVu { get; set; }

        // ✅ Tổng tiền các loại
        public decimal TienPhong { get; set; }  // Tiền phòng (KHÔNG chia)

        // ⚠️ TienDien và TienNuoc là tiền ĐÃ CHIA cho 1 người
        public decimal TienDien { get; set; }   // = TongTienDienPhong / SoNguoiTrongPhong
        public decimal TienNuoc { get; set; }   // = TongTienNuocPhong / SoNguoiTrongPhong

        public decimal TongTienDichVu { get; set; }

        // ✅ TongCong = Tổng tiền SINH VIÊN NÀY phải trả
        public decimal TongCong { get; set; }   // = TienPhong + TienDien + TienNuoc + TongTienDichVu

        // Thông tin thanh toán
        public bool DaThanhToan { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public string TrangThaiThanhToan { get; set; }

        // Thông tin QR Payment
        public string QRCodeUrl { get; set; }
        public BankInfoViewModel ThongTinNganHang { get; set; }
        public List<BankInfoViewModel> DanhSachNganHang { get; set; }

        // ✅ THÊM: Các thuộc tính helper để hiển thị rõ ràng hơn

        /// <summary>
        /// Tổng tiền điện + nước của 1 người (đã chia)
        /// </summary>
        public decimal TongTienDienNuocMoiNguoi => TienDien + TienNuoc;

        /// <summary>
        /// Kiểm tra có chia tiền hay không (có > 1 người trong phòng)
        /// </summary>
        public bool CoChiaTien => SoNguoiTrongPhong > 1;

        /// <summary>
        /// Số ngày còn lại đến hạn thanh toán
        /// </summary>
        public int SoNgayConLai => (NgayKetThuc - DateTime.Now).Days;

        /// <summary>
        /// Đã quá hạn thanh toán chưa
        /// </summary>
        public bool DaQuaHan => SoNgayConLai < 0 && !DaThanhToan;
    }

    // ViewModel cho dịch vụ trong thanh toán
    public class DichVuThanhToanViewModel
    {
        public string TenDichVu { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    // ViewModel cho lịch sử thanh toán
    public class LichSuThanhToanViewModel
    {
        public int MaHD { get; set; }
        public string MaPhong { get; set; }
        public string TenPhong { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public decimal TongTien { get; set; }
        public bool DaThanhToan { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public string TrangThaiThanhToan { get; set; }
    }

    // ViewModel cho danh sách lịch sử thanh toán
    public class DanhSachLichSuViewModel
    {
        public string MaSV { get; set; }
        public string HoTen { get; set; }
        public List<LichSuThanhToanViewModel> DanhSachThanhToan { get; set; }
        public decimal TongDaThanhToan { get; set; }
        public decimal TongChuaThanhToan { get; set; }
        public int SoLanDaThanhToan { get; set; }
        public int SoLanChuaThanhToan { get; set; }
    }

    // ViewModel cho thanh toán QR Code
    public class ThanhToanQRViewModel
    {
        [Required]
        public int MaHD { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngân hàng")]
        public string MaNganHang { get; set; }

        public decimal SoTien { get; set; }
        public string NoiDung { get; set; }

        // Thông tin hiển thị
        public string TenKhachHang { get; set; }
        public string MaPhong { get; set; }
        public List<BankInfoViewModel> DanhSachNganHang { get; set; }

        // QR Code
        public string QRCodeUrl { get; set; }
        public BankInfoViewModel NganHangDuocChon { get; set; }
    }

    // ViewModel cho thông tin ngân hàng
    public class BankInfoViewModel
    {
        public string MaNganHang { get; set; }
        public string TenNganHang { get; set; }
        public string TenVietTat { get; set; }
        public string Logo { get; set; }
        public string SoTaiKhoan { get; set; }
        public string TenTaiKhoan { get; set; }
        public string ChiNhanh { get; set; }
    }

    // ViewModel cho xác nhận thanh toán
    public class XacNhanThanhToanViewModel
    {
        [Required]
        public int MaHD { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giao dịch")]
        public string MaGiaoDich { get; set; }

        public decimal SoTien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày thanh toán")]
        public DateTime NgayThanhToan { get; set; }

        public string GhiChu { get; set; }
    }
}