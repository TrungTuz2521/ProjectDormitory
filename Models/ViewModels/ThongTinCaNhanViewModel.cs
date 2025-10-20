using KTX.Models.ViewModels;

namespace KtxManagement.Models.ViewModels
{
    public class ThongTinCaNhanViewModel
    {
        // Thêm ? để cho phép giá trị null (trong trường hợp SV chưa có phòng)
        public string? MaSinhVien { get; set; }
        public string? HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? DienThoai { get; set; }
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