namespace KTX.Models.ViewModels;

public class RulesAnnouncementsViewModel
{
    public List<ThongBaoItem> GeneralAnnouncements { get; set; } = new List<ThongBaoItem>();
    public List<ThongBaoItem> PersonalNotifications { get; set; } = new List<ThongBaoItem>();

    public class ThongBaoItem
    {
        public string MaTB { get; set; } // Giả định MaTB là string
        public string TieuDe { get; set; }
        public string NoiDung { get; set; }
        public DateTime NgayTB { get; set; } // Giả định NgayTB là DateTime
                                             // Bổ sung các trường khác nếu cần
    }
}