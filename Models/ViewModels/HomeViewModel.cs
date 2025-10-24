using System.Collections.Generic;
using KTX.Entities;

namespace KTX.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<ThongBao> Thongbaods { get; set; } = new List<ThongBao>();
        public List<YeuCau> YeuCauds { get; set; } = new List<YeuCau>();
        public HopDongPhong HopDong1 { get; set; }
        //public TienDienNuoc LatestUtilityBill { get; set; }
        public List<BaiDang> BaiDangds { get; set; } = new List<BaiDang>();
       
    }
}