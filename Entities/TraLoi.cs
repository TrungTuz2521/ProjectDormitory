using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class TraLoi
{
    public int MaTl { get; set; }

    public int Msv { get; set; }

    public int MaBd { get; set; }

    public string? NoiDungTl { get; set; }

    public DateOnly? NgayTl { get; set; }

    public virtual BaiDang MaBdNavigation { get; set; } = null!;
}
