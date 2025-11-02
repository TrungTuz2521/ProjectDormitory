using System;
using System.Collections.Generic;

namespace KTX.Entities;

public partial class Admin
{
    public int MaAdmin { get; set; }

    public string TenDn { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string VaiTro { get; set; } = null!;
}
