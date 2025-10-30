using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace KTX.Models.ViewModels
{
    // ViewModel mới chỉ cho Edit
    public class EditThongTinViewModel
    {
        public string MaSinhVien { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có 10 số")]
        public string DienThoai { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        // Các field chỉ để hiển thị (không validate)
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string Khoa { get; set; }
    }
}
