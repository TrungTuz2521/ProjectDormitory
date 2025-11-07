// KTX/Models/ViewModels/QLThongBaoViewModel.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KTX.Models.ViewModels
{
    public class QLThongBaoViewModel
    {
        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string TieuDe { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        [StringLength(2000)]
        public string NoiDung { get; set; }

        public string? MSV { get; set; } // Optional
        public List<SinhVienSelectViewModel>? DanhSachSinhVien { get; set; }
    }

    public class SinhVienSelectViewModel
    {
        public int MSV { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}