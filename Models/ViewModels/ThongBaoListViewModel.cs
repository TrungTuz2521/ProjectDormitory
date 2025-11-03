// KTX/Models/ViewModels/ThongBaoListViewModel.cs
namespace KTX.Models.ViewModels
{
    public class ThongBaoListViewModel
    {
        public List<ThongBaoViewModel> ThongBaoChung { get; set; } = new();
        public List<ThongBaoViewModel> ThongBaoRieng { get; set; } = new();
    }
}