using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KTX.Entities
{
    /// <summary>
    /// Chi tiết thanh toán điện nước cho từng sinh viên
    /// Mỗi hóa đơn điện nước sẽ có nhiều chi tiết (theo số sinh viên trong phòng)
    /// </summary>
    [Table("ChiTietThanhToanDienNuoc")]
    public class ChiTietThanhToanDienNuoc
    {
        [Key]
        [Column("MaCTTTDN")]
        public int MaCtttdn { get; set; }

        [Column("MaHDDN")]
        public int MaHddn { get; set; }

        [Column("MSV")]
        public int Msv { get; set; }

        /// <summary>
        /// Số tiền sinh viên này phải trả (đã chia theo số người)
        /// </summary>
        [Column("SoTienPhai", TypeName = "decimal(18,0)")]
        public decimal SoTienPhai { get; set; }

        /// <summary>
        /// Số tiền đã trả (null nếu chưa thanh toán)
        /// </summary>
        [Column("SoTienDaTra", TypeName = "decimal(18,0)")]
        public decimal? SoTienDaTra { get; set; }

        /// <summary>
        /// Trạng thái: "Chưa thanh toán", "Đã thanh toán"
        /// </summary>
        [Column("TrangThai")]
        [StringLength(50)]
        public string TrangThai { get; set; }

        [Column("NgayThanhToan")]
        public DateOnly? NgayThanhToan { get; set; }

        [Column("GhiChu")]
        [StringLength(500)]
        public string GhiChu { get; set; }

        // Navigation properties
        [ForeignKey("MaHddn")]
        public virtual TienDienNuoc MaHddnNavigation { get; set; }

        [ForeignKey("Msv")]
        public virtual SinhVien MsvNavigation { get; set; }
    }
}