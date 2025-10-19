using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class TienPhong
{
    public int MaHdp { get; set; }

    public int MaHd { get; set; }

    public decimal? TongTienP { get; set; }

    public DateOnly? HanTtp { get; set; }

    public string? TrangThaiTtp { get; set; }

    public virtual HopDongPhong MaHdNavigation { get; set; } = null!;
}
