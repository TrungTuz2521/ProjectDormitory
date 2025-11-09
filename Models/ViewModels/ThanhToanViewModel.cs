using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTX.ViewModels
{
    /// <summary>
    /// ViewModel cho chi tiết thanh toán
    /// </summary>
    public class ThanhToanDetailViewModel
    {
        // Thông tin hợp đồng
        public int MaHD { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThaiHopDong { get; set; } = string.Empty;

        // Thông tin sinh viên
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Thông tin phòng
        public string MaPhong { get; set; } = string.Empty;
        public string TenPhong { get; set; } = string.Empty;
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
        /// <summary>
        /// ✅ Kiểm tra xem phòng có hóa đơn điện nước chưa
        /// </summary>
        public bool CoDienNuoc { get; set; }
        /// <summary>
        /// Trạng thái thanh toán tiền phòng
        /// </summary>
        public bool TienPhongDaThanhToan { get; set; }

        /// <summary>
        /// Trạng thái thanh toán điện nước
        /// </summary>
        public bool DienNuocDaThanhToan { get; set; }



        // ✅ Thông tin phòng
        public int SoNguoiTrongPhong { get; set; } // Từ HienO của Phong

        // ✅ Tiền điện nước CỦA PHÒNG (chưa chia)
        public decimal TongTienDienPhong { get; set; }
        public decimal TongTienNuocPhong { get; set; }
        public decimal TongTienDienNuocPhong { get; set; }

        // Chi tiết dịch vụ
        public List<DichVuThanhToanViewModel> DanhSachDichVu { get; set; } = new();

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
        public string TrangThaiThanhToan { get; set; } = string.Empty;

        // Thông tin QR Payment
        public string QRCodeUrl { get; set; } = string.Empty;
        public BankInfoViewModel ThongTinNganHang { get; set; } = new();
        public List<BankInfoViewModel> DanhSachNganHang { get; set; } = new();

        // ✅ Các thuộc tính helper để hiển thị rõ ràng hơn

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

    /// <summary>
    /// ViewModel cho lịch sử thanh toán
    /// </summary>
    public class LichSuThanhToanViewModel
    {
        public int MaHD { get; set; }
        public string MaPhong { get; set; } = string.Empty;
        public string TenPhong { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }

        // Phân loại thanh toán
        public string LoaiThanhToan { get; set; } = string.Empty; // "Tiền phòng" hoặc "Tiền điện nước"
        public string MaHoaDon { get; set; } = string.Empty;
        public string KyThanhToan { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;

        // Thông tin thanh toán
        public decimal TongTien { get; set; }
        public bool DaThanhToan { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public string TrangThaiThanhToan { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel cho danh sách lịch sử thanh toán
    /// </summary>
    public class DanhSachLichSuViewModel
    {
        public string MaSV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public List<LichSuThanhToanViewModel> DanhSachThanhToan { get; set; } = new();
        public decimal TongDaThanhToan { get; set; }
        public decimal TongChuaThanhToan { get; set; }
        public int SoLanDaThanhToan { get; set; }
        public int SoLanChuaThanhToan { get; set; }
    }

    /// <summary>
    /// ViewModel cho dịch vụ thanh toán
    /// </summary>
    public class DichVuThanhToanViewModel
    {
        public int MaDichVu { get; set; }
        public string TenDichVu { get; set; } = string.Empty;
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }

    /// <summary>
    /// ViewModel cho thanh toán QR
    /// </summary>
    public class ThanhToanQRViewModel
    {
        public int MaHD { get; set; }
        public decimal SoTien { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaPhong { get; set; } = string.Empty;
        public string MaNganHang { get; set; } = string.Empty;
        public List<BankInfoViewModel> DanhSachNganHang { get; set; } = new();
        public string QRCodeUrl { get; set; } = string.Empty;
        public BankInfoViewModel? NganHangDuocChon { get; set; }
    }

    /// <summary>
    /// ViewModel cho thông tin ngân hàng
    /// </summary>
    public class BankInfoViewModel
    {
        public string MaNganHang { get; set; } = string.Empty;
        public string TenNganHang { get; set; } = string.Empty;
        public string TenVietTat { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string SoTaiKhoan { get; set; } = string.Empty;
        public string TenTaiKhoan { get; set; } = string.Empty;
        public string ChiNhanh { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel cho xác nhận thanh toán
    /// </summary>
    public class XacNhanThanhToanViewModel
    {
        [Required]
        public int MaHD { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giao dịch")]
        public string MaGiaoDich { get; set; } = string.Empty;

        public decimal SoTien { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày thanh toán")]
        public DateTime NgayThanhToan { get; set; }

        public string GhiChu { get; set; } = string.Empty;
    }
}