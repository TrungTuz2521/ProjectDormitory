namespace KTX.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TongSinhVien { get; set; }
        public double TyLeLapDay { get; set; }
        public int YeuCauChoCLy { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int HoaDonChuaThanhToan { get; set; }
        public int PhongVuotSoLuong { get; set; }

        public List<YeuCauViewModel> YeuCauMoiNhat { get; set; } = new();
    }

    public class YeuCauViewModel
    {
        public string? LoaiYc { get; set; }
        public string? NoiDungYc { get; set; }
        public string? TrangThaiYc { get; set; }
        public string? Msv { get; set; }
        public DateOnly? NgayGuiYc { get; set; }
    }
}
