using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class ThanNhan
{
    public int MaPh { get; set; }

    public int Msv { get; set; }

    public string? QuanHe { get; set; }

    public string? HoTen { get; set; }

    public string? Sdt { get; set; }

    public virtual SinhVien? MsvNavigation { get; set; } = null!;
}
