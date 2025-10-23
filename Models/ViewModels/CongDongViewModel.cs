//using System.ComponentModel.DataAnnotations;

//namespace KTX.Models.ViewModels
//{

//    public class CongDongViewModel
//    {
//        public List<BaiVietViewModel> BaiViets { get; set; } = [];
//        public string? DanhMucHienTai { get; set; }
//        public string? TimKiem { get; set; }
//        public int TrangHienTai { get; set; } = 1;
//        public int TongSoTrang { get; set; } = 1;
//    }




//    // ViewModel cho bình luận
//    public class BinhLuanViewModel
//    {
//        public int MaBinhLuan { get; set; }
//        public string NoiDung { get; set; } = string.Empty;
//        public DateTime NgayBinhLuan { get; set; }
//        public int Msv { get; set; }
//        public string TenNguoiBinhLuan { get; set; } = string.Empty;
//        public string? AvatarNguoiBinhLuan { get; set; }
//    }

//    // ViewModel để tạo bài viết mới
//    public class TaoBaiVietViewModel
//    {
//        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
//        [StringLength(200, ErrorMessage = "Tiêu đề không quá 200 ký tự")]
//        [Display(Name = "Tiêu đề")]
//        public string TieuDe { get; set; } = string.Empty;

//        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
//        [Display(Name = "Nội dung")]
//        public string NoiDung { get; set; } = string.Empty;

//        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
//        [Display(Name = "Danh mục")]
//        public string DanhMuc { get; set; } = "Giao lưu";

//        [Display(Name = "Hình ảnh")]
//        public List<IFormFile>? HinhAnhs { get; set; }
//    }

//    // ViewModel cho bình luận
//    public class ThemBinhLuanViewModel
//    {
//        [Required]
//        public int MaBaiViet { get; set; }

//        [Required(ErrorMessage = "Vui lòng nhập nội dung bình luận")]
//        [StringLength(500, ErrorMessage = "Bình luận không quá 500 ký tự")]
//        public string NoiDung { get; set; } = string.Empty;
//    }

//}
