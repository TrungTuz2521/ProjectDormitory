from PIL import Image, ImageDraw, ImageFont

def tao_thiep_20_10(ten_nguoi_nhan="Bạn", ten_nguoi_gui="Tớ", ten_file_luu="thiep_20_10.png"):
    # 1. Tạo một ảnh mới (Nền cho thiệp)
    # Kích thước 800x600, màu hồng nhạt (#FFDAB9)
    try:
        img = Image.new('RGB', (800, 600), color='#FFDAB9')
    except Exception as e:
        print(f"Lỗi khi tạo ảnh: {e}")
        return

    # Khởi tạo đối tượng vẽ
    d = ImageDraw.Draw(img)

    # 2. Chọn và tải font chữ (Bạn cần có file .ttf trong máy)
    # Nếu không có font 'arial.ttf', Python sẽ báo lỗi.
    # Bạn có thể thử các font mặc định khác hoặc tải font về.
    try:
        # Thử tải font Arial (thường có sẵn trên Windows/Mac)
        font_tieu_de = ImageFont.truetype("arial.ttf", 60)
        font_chu_lon = ImageFont.truetype("arial.ttf", 36)
        font_chu_nho = ImageFont.truetype("arial.ttf", 24)
    except IOError:
        # Nếu không tìm thấy font, sử dụng font mặc định của Pillow
        print("⚠️ Không tìm thấy font 'arial.ttf', đang sử dụng font mặc định.")
        font_tieu_de = ImageFont.load_default()
        font_chu_lon = ImageFont.load_default()
        font_chu_nho = ImageFont.load_default()

    # 3. Nội dung thiệp
    tieu_de = "🌺 Chúc Mừng Ngày Phụ Nữ Việt Nam 🌺"
    ngay_le = "20/10"
    loi_chuc = f"Chúc {ten_nguoi_nhan} luôn xinh đẹp, hạnh phúc và thành công!"
    loi_gui = f"Thân gửi từ {ten_nguoi_gui}"

    mau_chu = (139, 0, 0)  # Đỏ đậm

    # 4. Vẽ chữ lên thiệp
    
    # Tiêu đề
    w, h = d.textsize(tieu_de, font=font_chu_lon)
    d.text(((800 - w) / 2, 50), tieu_de, fill=mau_chu, font=font_chu_lon)

    # Ngày lễ (To, ở trung tâm)
    w_ngay, h_ngay = d.textsize(ngay_le, font=font_tieu_de)
    d.text(((800 - w_ngay) / 2, 150), ngay_le, fill=(255, 69, 0), font=font_tieu_de) # Màu cam đỏ

    # Lời chúc
    w_loi, h_loi = d.textsize(loi_chuc, font=font_chu_lon)
    d.text(((800 - w_loi) / 2, 300), loi_chuc, fill=mau_chu, font=font_chu_lon)
    
    # Lời gửi
    w_gui, h_gui = d.textsize(loi_gui, font=font_chu_nho)
    d.text((800 - w_gui - 50, 500), loi_gui, fill=(100, 100, 100), font=font_chu_nho) # Màu xám

    # 5. Lưu ảnh
    try:
        img.save(ten_file_luu)
        print(f"\n✅ Đã tạo thiệp thành công và lưu tại: {ten_file_luu}")
    except Exception as e:
        print(f"Lỗi khi lưu ảnh: {e}")

# --- Cách sử dụng ---
# Thay đổi tên người nhận và người gửi tùy ý
tao_thiep_20_10(ten_nguoi_nhan="Cô Giáo", ten_nguoi_gui="Học Trò")

# Bạn cũng có thể mở thiệp để xem ngay (tùy thuộc vào hệ điều hành)
# img.show()