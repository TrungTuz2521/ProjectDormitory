using KTX.Entities;

namespace KTX.Models
{
    public class TaiChinhViewModel
    {
        public string LoaiHienThi { get; set; } = "phong";
        public int? DotTtdnHienTai { get; set; }
        public List<int> DanhSachDotTtdn { get; set; } = new();

        public List<PhongTienPhongDetail> ChiTietTienPhongTheoPhong { get; set; } = new();
        public List<PhongTienDienNuocDetail> ChiTietDienNuocTheoPhong { get; set; } = new();
    }

    public class PhongTienPhongDetail
    {
        public int MaP { get; set; }
        public List<SinhVienTienPhong> SinhViens { get; set; } = new();
    }

    public class SinhVienTienPhong
    {
        public string Msv { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public TienPhong? TienPhong { get; set; }
    }

    public class PhongTienDienNuocDetail
    {
        public int MaP { get; set; }
        public int? DotTtdn { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; } = null!;
        public List<SinhVienTienDienNuoc> SinhViens { get; set; } = new();
    }

    public class SinhVienTienDienNuoc
    {
        public string Msv { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public TienDienNuoc? TienDienNuoc { get; set; }
    }
    public class ThongKeTaiChinhViewModel
    {
        public decimal TongTienPhongDaThu { get; set; }
        public decimal TongTienPhongChuaThu { get; set; }
        public decimal TongTienDienNuocDaThu { get; set; }
        public decimal TongTienDienNuocChuaThu { get; set; }
        public int SoHoaDonPhongChuaThu { get; set; }
        public int SoHoaDonDienNuocChuaThu { get; set; }

        public decimal TongDoanhThu { get; set; }
        public decimal TongCongNo { get; set; }
    }
    public class NhapDienNuocViewModel
    {
        public int MaP { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public int? SoDien { get; set; }
        public int? SoNuoc { get; set; }
        public int GiaDien { get; set; } = 3500;
        public int GiaNuoc { get; set; } = 10000;
        public int DotTtdn { get; set; }
    }
}