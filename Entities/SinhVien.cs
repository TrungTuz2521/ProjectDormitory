using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class SinhVien
{
    public int Msv { get; set; }

    public string HoTen { get; set; } = null!;

    public string GioiTinh { get; set; } = null!;

    public DateOnly NgaySinh { get; set; }

    public string Sdt { get; set; } = null!;

    public string? Email { get; set; }

    public string? Khoa { get; set; }

    public string? Avatar { get; set; }

    public string TenDn { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public virtual ICollection<BaiDang> BaiDangs { get; set; } = new List<BaiDang>();

    public virtual ICollection<HopDongPhong> HopDongPhongs { get; set; } = new List<HopDongPhong>();

    public virtual ICollection<ThanNhan> ThanNhans { get; set; } = new List<ThanNhan>();

    public virtual ICollection<ThongBao> ThongBaos { get; set; } = new List<ThongBao>();

    public virtual ICollection<YeuCau> YeuCaus { get; set; } = new List<YeuCau>();
    // ✅ THÊM Navigation Property
    public virtual ICollection<ChiTietThanhToanDienNuoc> ChiTietThanhToanDienNuocs { get; set; }

    public SinhVien()
    {
        // ... các collection hiện tại ...
        ChiTietThanhToanDienNuocs = new HashSet<ChiTietThanhToanDienNuoc>();
    }
}
