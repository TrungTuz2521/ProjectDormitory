using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class TienDienNuoc
{
    public int MaHddn { get; set; }

    public int MaP { get; set; }

    public int? SoDien { get; set; }

    public int? SoNuoc { get; set; }

    public int? GiaDien { get; set; }

    public int? GiaNuoc { get; set; }

    public decimal? TienDien { get; set; }

    public decimal? TienNuoc { get; set; }

    public decimal? TongTienDn { get; set; }

    public int? DotTtdn { get; set; }

    public DateOnly? Httdn { get; set; }

    public string? TrangThaiTtdn { get; set; }

    public DateOnly? NgayTtdn { get; set; }

    public virtual Phong MaPNavigation { get; set; } = null!;
    // ✅ THÊM Navigation Property
    public virtual ICollection<ChiTietThanhToanDienNuoc> ChiTietThanhToanDienNuocs { get; set; }

    public TienDienNuoc()
    {
        ChiTietThanhToanDienNuocs = new HashSet<ChiTietThanhToanDienNuoc>();
    }
}
