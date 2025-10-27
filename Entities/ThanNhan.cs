using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KTX.Entities;

public partial class ThanNhan
{
    public int MaPh { get; set; }

    public int Msv { get; set; }
    [Required(ErrorMessage = "Quan hệ không được để trống")]
    [StringLength(50, ErrorMessage = "Quan hệ không được quá 50 ký tự")]
    public string? QuanHe { get; set; }
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
    public string? HoTen { get; set; }
    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại phải có 10-11 chữ số")]
    public string? Sdt { get; set; }

    public virtual SinhVien MsvNavigation { get; set; } = null!;
}
