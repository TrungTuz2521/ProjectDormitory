using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class DanhGia
{
    public int MaDg { get; set; }

    public int MaYc { get; set; }

    public DateOnly? NgayGuiDg { get; set; }

    public string? NoiDungDg { get; set; }

    public string? DiemDg { get; set; }

    public virtual YeuCau MaYcNavigation { get; set; } = null!;
}
