namespace KTX.Models.ViewModels;

public class RoomManagementViewModel
{
    // Thông tin sinh viên
    public string MaSinhVien { get; set; } = string.Empty;
    public string HoTen { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;

    // Thông tin phòng
    public string? MaPhong { get; set; }
    public string? LoaiPhong { get; set; }
    public int SoGiuongHienTai { get; set; }
    public int SoGiuongToiDa { get; set; }
    public string TinhTrangPhong { get; set; } = string.Empty;
    public decimal? TongTienP { get; set; }

    // Thông tin hợp đồng
    public string? MaHopDong { get; set; }
    public DateTime? NgayBatDau { get; set; }
    public DateTime? NgayKetThuc { get; set; }
    public bool IsContractActive { get; set; }
    public int RemainingDays { get; set; }

    // 🪙 Tiền phòng
    public decimal? TienPhongHangThang { get; set; } // tiền cố định hàng tháng
    public List<TienPhongInfo> LichSuThanhToan { get; set; } = new(); // lịch sử thanh toán

    // Tính năng phòng
    public List<string> RoomFeatures { get; set; } = new();

    // Danh sách bạn cùng phòng
    public List<RoommateInfo> Roommates { get; set; } = new();

    // Danh sách yêu cầu
    public List<YeuCauInfo> DanhSachYeuCau { get; set; } = new();

    // Các trạng thái có thể thao tác trên giao diện
    public bool CanRequestRoomChange { get; set; }
    public bool CanCancelContract { get; set; }
}

// DTO phụ cho lịch sử tiền phòng
public class TienPhongInfo
{
    public int Thang { get; set; }
    public int Nam { get; set; }
    public decimal SoTien { get; set; }
    public DateTime? NgayThanhToan { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public decimal? TongTienP { get; set; }
}


// DTO cho yêu cầu
public class YeuCauInfo
{
    public int MaYc { get; set; }
    public string LoaiYc { get; set; } = string.Empty;
    public string NoiDungYc { get; set; } = string.Empty;
    public DateOnly? NgayGuiYc { get; set; }
    public string TrangThaiYc { get; set; } = string.Empty;
    public DateOnly? NgayXuLy { get; set; } = null; // Nullable, mặc định null nếu không có trong DB
}