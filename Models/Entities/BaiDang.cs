using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class BaiDang
{
    public int MaBd { get; set; }

    public int Msv { get; set; }

    public string? NoiDungBd { get; set; }

    public DateOnly? NgayDang { get; set; }

    public virtual SinhVien MsvNavigation { get; set; } = null!;

    public virtual ICollection<TraLoi> TraLois { get; set; } = new List<TraLoi>();
}
