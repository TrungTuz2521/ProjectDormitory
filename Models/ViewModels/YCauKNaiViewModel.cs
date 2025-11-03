namespace KTX.Models.ViewModels
{
    public class YCauKNaiViewModel
    {
        public int MaYC { get; set; }
        public string MSV { get; set; } = null!;
        public string HoTen { get; set; } = null!;
        public string LoaiYC { get; set; } = null!;
        public string NoiDungYC { get; set; } = null!;
        public string TrangThai { get; set; } = null!;
        public DateOnly? NgayGuiYC { get; set; }

        // MỚI: Phòng
        public string? Phong { get; set; }
        public int Tong { get; set; }
        public int ChoXuLy { get; set; }
        public int DangXuLy { get; set; }
        public int DaXuLy { get; set; }
    }
}