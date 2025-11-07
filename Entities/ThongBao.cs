using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class ThongBao
{
    public int MaTb { get; set; }

    public int Msv { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public DateOnly? NgayTb { get; set; }

    public virtual SinhVien MsvNavigation { get; set; } = null!;

    public virtual ICollection<SinhVien> Msvs { get; set; } = new List<SinhVien>();
}
