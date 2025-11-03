// KTX/Models/ViewModels/ThongBaoViewModel.cs
namespace KTX.Models.ViewModels
{
    public class ThongBaoViewModel
    {
        public int MaTB { get; set; }
        public int MSV { get; set; } // int, không string
        public string HoTen { get; set; } = "Tất cả sinh viên";
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateOnly? NgayTB { get; set; }

        public string NgayTBFormatted => NgayTB.HasValue
            ? $"{NgayTB.Value.Day:00}/{NgayTB.Value.Month:00}/{NgayTB.Value.Year}"
            : "-";
    }
}