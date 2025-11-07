// KTX/Models/ViewModels/QLThongBaoViewModel.cs
namespace KTX.Models.ViewModels
{
    public class QLThongBaoViewModel
    {
        public string MSV { get; set; } = string.Empty; // string để binding từ dropdown
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;

        public List<SinhVienSelectViewModel> DanhSachSinhVien { get; set; } = new();
    }

    public class SinhVienSelectViewModel
    {
        public int MSV { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}