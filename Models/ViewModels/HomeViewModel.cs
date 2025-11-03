using KTX.Entities;

namespace KTX.Models.ViewModels
{
    public class HomeViewModel
    {
        public SinhVien SinhVien1 { get; set; }
        public HopDongPhong? HopDong1 { get; set; }

        public List<YeuCau>? YeuCauds { get; set; }
        public List<ThongBao>? Thongbaods { get; set; }
        public List<BaiDang>? BaiDangds { get; set; }
        public List<TraLoi>? TraLois { get; set; }
    }
}
