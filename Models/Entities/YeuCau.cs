using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTX.Entities;

public partial class YeuCau
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaYc { get; set; }

    public int Msv { get; set; }

    public string? LoaiYc { get; set; }

    public string? NoiDungYc { get; set; }

    public DateOnly? NgayGuiYc { get; set; }

    public string? TrangThaiYc { get; set; }

    public virtual ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();

    public virtual SinhVien MsvNavigation { get; set; } = null!;
}
