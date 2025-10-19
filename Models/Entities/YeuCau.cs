using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class YeuCau
{
    public int MaYc { get; set; }

    public int Msv { get; set; }

    public string? LoaiYc { get; set; }

    public string? NoiDungYc { get; set; }

    public DateOnly? NgayGuiYc { get; set; }

    public string? TrangThaiYc { get; set; }

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual SinhVien MsvNavigation { get; set; } = null!;
}
