using System;
using System.Collections.Generic;

namespace KTX.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Thống kê cơ bản
        public int TongSinhVien { get; set; }
        public decimal TyLeLapDay { get; set; }
        public int YeuCauChoXuLy { get; set; }
        public decimal TongDoanhThu { get; set; }

        // Thống kê phòng chi tiết
        public int TongPhong { get; set; }
        public int PhongDangSuDung { get; set; }
        public int PhongTrong { get; set; }
        public int PhongDangBaoTri { get; set; }
        public int PhongDay { get; set; }

        // Cảnh báo quan trọng
        public int HoaDonChuaThanhToan { get; set; }
        public int HoaDonQuaHan { get; set; }
        public int PhongVuotSoLuong { get; set; }
        public int HopDongSapHetHan { get; set; }
        public int YeuCauQuaHan { get; set; }

        // Thống kê sinh viên
        public int SinhVienMoi { get; set; }
        public int SinhVienSapHetHan { get; set; }
        public int SinhVienNoTien { get; set; }

        // Thống kê tài chính
        public decimal TienPhongThangNay { get; set; }
        public decimal TienDienNuocThangNay { get; set; }
        public decimal TongTienConNo { get; set; }
        public int TyLeThanhToan { get; set; }

        // Thống kê yêu cầu
        public int YeuCauChoXuLyCount { get; set; }
        public int YeuCauDangXuLyCount { get; set; }
        public int YeuCauDaXuLyThangNay { get; set; }

        // Danh sách cần xử lý (FIX: Renamed to avoid duplicate)
        public List<YeuCauViewModel> YeuCauMoiNhat { get; set; } = new();
        public List<HoaDonQuaHanViewModel> DanhSachHoaDonQuaHan { get; set; } = new();
        public List<PhongCanhBaoViewModel> PhongCanhBao { get; set; } = new();
        public List<HopDongSapHetHanViewModel> DanhSachHopDongSapHetHan { get; set; } = new();
    }

    public class YeuCauViewModel
    {
        public required string MaYc { get; set; }
        public required string LoaiYc { get; set; }
        public required string NoiDungYc { get; set; }
        public required string TrangThaiYc { get; set; }
        public required string Msv { get; set; }
        public required string HoTen { get; set; }
        public DateTime? NgayGuiYc { get; set; }
        public int SoNgayCho { get; set; }
    }

    public class HoaDonQuaHanViewModel
    {
        public required string MaHD { get; set; }
        public required string Msv { get; set; }
        public required string HoTen { get; set; }
        public required string MaPhong { get; set; }
        public required string LoaiHoaDon { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayHetHan { get; set; }
        public int SoNgayQuaHan { get; set; }
    }

    public class PhongCanhBaoViewModel
    {
        public required string MaPhong { get; set; }
        public int HienO { get; set; }
        public int ToiDa { get; set; }
        public required string TinhTrang { get; set; }
        public required string LyDoCanhBao { get; set; }
    }

    public class HopDongSapHetHanViewModel
    {
        public required string MaHD { get; set; }
        public required string Msv { get; set; }
        public required string HoTen { get; set; }
        public required string MaPhong { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SoNgayConLai { get; set; }
    }
}