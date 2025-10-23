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

    public virtual ICollection<DanhGium> DanhGia { get; set; } = new List<DanhGium>();

    public virtual SinhVien MsvNavigation { get; set; } = null!;
    public DateOnly? NgayXuLy { get; internal set; }
}
