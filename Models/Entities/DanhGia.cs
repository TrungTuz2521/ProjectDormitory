using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTX.Entities;

public partial class DanhGia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 🔥 Thêm dòng này để tự tăn
    public int MaDg { get; set; }
    [ForeignKey("YeuCau")]
    public int MaYc { get; set; }

    public DateOnly? NgayGuiDg { get; set; }

    public string? NoiDungDg { get; set; }
        
    public string? DiemDg { get; set; }

    public virtual YeuCau MaYcNavigation { get; set; } = null!;
}
