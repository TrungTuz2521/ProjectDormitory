using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class Phong
{
    public int MaP { get; set; }

    public string? TinhTrang { get; set; }

    public int? HienO { get; set; }

    public int? ToiDaO { get; set; }

    public virtual ICollection<HopDongPhong> HopDongPhongs { get; set; } = new List<HopDongPhong>();

    public virtual ICollection<TienDienNuoc> TienDienNuocs { get; set; } = new List<TienDienNuoc>();
}
