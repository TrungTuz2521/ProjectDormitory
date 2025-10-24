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

        // Thông tin tiện điện nước
        public int? SoDien { get; set; }
        public int? SoNuoc { get; set; }
        public decimal? GiaDien { get; set; }
        public decimal? GiaNuoc { get; set; }
        public DateTime? ThoiGianGhi { get; set; }

        // Chi tiết dịch vụ
        public List<DichVuThanhToanViewModel> DanhSachDichVu { get; set; }

        // Tổng tiền các loại
        public decimal TienPhong { get; set; }
        public decimal TienDien { get; set; }
        public decimal TienNuoc { get; set; }
        public decimal TongTienDichVu { get; set; }
        public decimal TongCong { get; set; }

        // Thông tin thanh toán
        public bool DaThanhToan { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public string TrangThaiThanhToan { get; set; }

        // Thông tin QR Payment
        public string QRCodeUrl { get; set; }
        public BankInfoViewModel ThongTinNganHang { get; set; }
        public List<BankInfoViewModel> DanhSachNganHang { get; internal set; }
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