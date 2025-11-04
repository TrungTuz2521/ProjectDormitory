using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTX.Entities;

public partial class HopDongPhong
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MaHd { get; set; }


    public int Msv { get; set; }

    public int MaP { get; set; }

    public string? LoaiP { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public DateOnly? NgayKi { get; set; }

    public decimal? TienCoc { get; set; }

    public decimal? TienP { get; set; }

    public string? TrangThaiHd { get; set; }

    public virtual Phong MaPNavigation { get; set; } = null!;

    public virtual SinhVien MsvNavigation { get; set; } = null!;

    public virtual ICollection<TienPhong> TienPhongs { get; set; } = new List<TienPhong>();
}
