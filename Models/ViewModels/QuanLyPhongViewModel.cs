using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTX.Models
{
    // ViewModel cho danh sách phòng
    public class PhongViewModel
    {
        public int MaP { get; set; }

        [Display(Name = "Tình trạng")]
        public string? TinhTrang { get; set; }

        [Display(Name = "Số người ở")]
        public int HienO { get; set; }

        [Display(Name = "Số giường")]
        public int ToiDaO { get; set; }

        [Display(Name = "Loại phòng")]
        public string? LoaiPhong { get; set; }

        [Display(Name = "Giá phòng")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TienPhong { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [Display(Name = "SL sinh viên")]
        public int SoLuongSinhVien { get; set; }
    }

    // ViewModel cho chi tiết phòng
    public class ChiTietPhongViewModel
    {
        public int MaP { get; set; }

        [Display(Name = "Loại phòng")]
        public string? LoaiPhong { get; set; }

        [Display(Name = "Tình trạng")]
        public string? TinhTrang { get; set; }

        [Display(Name = "Số giường")]
        public int ToiDaO { get; set; }

        [Display(Name = "Số người ở hiện tại")]
        public int HienO { get; set; }

        [Display(Name = "Giới tính")]
        public string? GioiTinh { get; set; }

        [Display(Name = "Tiền phòng")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TienPhong { get; set; }

        [Display(Name = "Tiền cọc")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TienCoc { get; set; }

        public List<SinhVienPhongViewModel> DanhSachSinhVien { get; set; } = new List<SinhVienPhongViewModel>();
        public List<HopDongViewModel> LichSuHopDong { get; set; } = new List<HopDongViewModel>();
        public List<DienNuocViewModel> ChiSoDienNuoc { get; set; } = new List<DienNuocViewModel>();
    }

    // ViewModel cho sinh viên trong phòng
    public class SinhVienPhongViewModel
    {
        public int Msv { get; set; }

        [Display(Name = "Họ tên")]
        public string? HoTen { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? Sdt { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateOnly? NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateOnly? NgayKetThuc { get; set; }
    }

    // ViewModel cho lịch sử hợp đồng
    public class HopDongViewModel
    {
        public int MaHd { get; set; }
        public int Msv { get; set; }

        [Display(Name = "Sinh viên")]
        public string? TenSinhVien { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateOnly? NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateOnly? NgayKetThuc { get; set; }

        [Display(Name = "Ngày ký")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateOnly? NgayKi { get; set; }

        [Display(Name = "Trạng thái")]
        public string? TrangThaiHd { get; set; }

        [Display(Name = "Tiền cọc")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TienCoc { get; set; }

        [Display(Name = "Tiền phòng")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TienP { get; set; }
    }

    // ViewModel cho điện nước
    public class DienNuocViewModel
    {
        public int MaHddn { get; set; }

        [Display(Name = "Đợt")]
        public int? DotTtdn { get; set; }

        [Display(Name = "Tiền điện")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TienDien { get; set; }

        [Display(Name = "Tiền nước")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TienNuoc { get; set; }

        [Display(Name = "Tổng tiền")]
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TongTienDn { get; set; }

        [Display(Name = "Ngày thanh toán")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateOnly? NgayTtdn { get; set; }

        [Display(Name = "Trạng thái")]
        public string? TrangThaiTtdn { get; set; }
    }

    // ViewModel cho cập nhật phòng
    public class CapNhatPhongViewModel
    {
        public int MaP { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tình trạng")]
        [Display(Name = "Tình trạng")]
        public string? TinhTrang { get; set; }

        [Display(Name = "Số người ở hiện tại")]
        [Range(0, int.MaxValue, ErrorMessage = "Số người ở phải >= 0")]
        public int HienO { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số giường tối đa")]
        [Display(Name = "Số giường tối đa")]
        [Range(1, 20, ErrorMessage = "Số giường phải từ 1-20")]
        public int ToiDaO { get; set; }
    }
    public class ThemPhongViewModel
    {
        public int MaP { get; set; }
        public string LoaiPhong { get; set; }
        public string TinhTrang { get; set; }
        public int ToiDaO { get; set; }
        public string GioiTinh { get; set; }
        public decimal TienPhong { get; set; }
        public decimal? TienCoc { get; set; }
    }
}