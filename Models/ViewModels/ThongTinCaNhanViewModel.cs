using System.ComponentModel.DataAnnotations;
using KTX.Models.ViewModels;

namespace KTX.Models.ViewModels
{
    public class ThongTinCaNhanViewModel
    {
        // Thêm ? để cho phép giá trị null (trong trường hợp SV chưa có phòng)
        public string? MaSinhVien { get; set; }
        public string? HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? DienThoai { get; set; }
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }
        public string? Khoa { get; set; }
        public string? AvatarUrl { get; set; }

        // Thông tin Phòng ở (từ Phong Entity)
        public string MaPhong { get; set; }
        public string? TinhTrangPhong { get; set; }
        public int SoGiuongHienTai { get; set; }
        public int SoGiuongToiDa { get; set; }
        public string? LoaiPhong { get; set; }

        // Thông tin Hợp đồng
        public string? MaHopDong { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }

        // Cung cấp giá trị mặc định cho List
        public List<RoommateInfo> BanCungPhong { get; set; } = new List<RoommateInfo>();
    }
}