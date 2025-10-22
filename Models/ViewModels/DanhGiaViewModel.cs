using System.ComponentModel.DataAnnotations;

namespace KTX.Models.ViewModels
{
    public class DanhGiaViewModel
    {
        public int MaYC { get; set; }
        public string LoaiYC { get; set; } = string.Empty;
        public string NoiDungYC { get; set; } = string.Empty;
        public DateOnly? NgayGuiYC { get; set; }
        public string TrangThaiYC { get; set; } = string.Empty;

        // Thông tin đánh giá (nếu có)
        public int? MaDG { get; set; }
        public string? NoiDungDG { get; set; }
        public int? DiemDG { get; set; }

        // Cho form gửi đánh giá
        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(300, ErrorMessage = "Nội dung đánh giá không quá 300 ký tự")]
        public string NoiDungDanhGia { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Điểm đánh giá từ 1 đến 5")]
        public int DiemDanhGia { get; set; }
        public List<DanhGiaItem>? DanhSachDanhGia { get; set; }
    }
    public class DanhGiaItem
    {
        public int MaDG { get; set; }
        public string? NoiDungDG { get; set; }
        public string? DiemDG { get; set; }
        public DateOnly? NgayGuiDG { get; set; }
    }
}
