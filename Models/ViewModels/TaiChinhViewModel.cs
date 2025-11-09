using System;
using System.Collections.Generic;
using KTX.Entities;

namespace KTX.Models
{
    public class TaiChinhViewModel
    {
        public string LoaiHienThi { get; set; }
        public int? DotTtdnHienTai { get; set; }
        public List<int> DanhSachDotTtdn { get; set; }
        public List<PhongTienPhongDetail> ChiTietTienPhongTheoPhong { get; set; }
        public List<PhongTienDienNuocDetail> ChiTietDienNuocTheoPhong { get; set; }

        public TaiChinhViewModel()
        {
            DanhSachDotTtdn = new List<int>();
            ChiTietTienPhongTheoPhong = new List<PhongTienPhongDetail>();
            ChiTietDienNuocTheoPhong = new List<PhongTienDienNuocDetail>();
        }
    }

    // Chi tiết tiền phòng
    public class PhongTienPhongDetail
    {
        public int MaP { get; set; }
        public List<SinhVienTienPhong> SinhViens { get; set; }

        public PhongTienPhongDetail()
        {
            SinhViens = new List<SinhVienTienPhong>();
        }
    }

    public class SinhVienTienPhong
    {
        public string Msv { get; set; }
        public string HoTen { get; set; }
        public TienPhong TienPhong { get; set; }
    }

    // ✅ Chi tiết tiền điện nước - CẬP NHẬT
    public class PhongTienDienNuocDetail
    {
        public int MaP { get; set; }
        public int? DotTtdn { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; }

        // ✅ THÊM: Thông tin chi tiết thanh toán
        public int TongSinhVien { get; set; }
        public int SoDaThu { get; set; }

        public List<SinhVienTienDienNuoc> SinhViens { get; set; }

        public PhongTienDienNuocDetail()
        {
            SinhViens = new List<SinhVienTienDienNuoc>();
        }
    }

    // ✅ Chi tiết sinh viên - CẬP NHẬT
    public class SinhVienTienDienNuoc
    {
        public string Msv { get; set; }
        public string HoTen { get; set; }

        // Thông tin hóa đơn chung của phòng
        public TienDienNuoc TienDienNuoc { get; set; }

        // ✅ THÊM: Thông tin chi tiết của sinh viên này
        public int? MaCtttdn { get; set; }  // Mã chi tiết thanh toán
        public decimal SoTienPhai { get; set; }  // Tiền sinh viên này phải trả
        public decimal? SoTienDaTra { get; set; }  // Tiền đã trả
        public string TrangThaiCaNhan { get; set; }  // Trạng thái cá nhân
        public DateOnly? NgayThanhToan { get; set; }
    }

    // Thống kê tài chính
    public class ThongKeTaiChinhViewModel
    {
        public decimal TongTienPhongChuaThu { get; set; }
        public decimal TongTienPhongDaThu { get; set; }
        public decimal TongTienDienNuocChuaThu { get; set; }
        public decimal TongTienDienNuocDaThu { get; set; }
        public int SoHoaDonPhongChuaThu { get; set; }
        public int SoHoaDonDienNuocChuaThu { get; set; }
        public decimal TongDoanhThu { get; set; }
        public decimal TongCongNo { get; set; }
    }

    // ViewModel nhập điện nước
    public class NhapDienNuocViewModel
    {
        public int MaP { get; set; }
        public int? SoDien { get; set; }
        public int? SoNuoc { get; set; }
        public decimal GiaDien { get; set; } = 3000;
        public decimal GiaNuoc { get; set; } = 15000;
        public int DotTtdn { get; set; }
    }
}