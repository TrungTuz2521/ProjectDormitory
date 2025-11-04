// sinhvien-details.js - FIXED: Bỏ LoaiP và Gia

document.addEventListener('DOMContentLoaded', function () {
    console.log('✅ sinhvien-details.js loaded');
    initializeFormValidation();
    initializeModalHandlers();
    setupRealTimeValidation();
    autoHideToasts();
});

// ==================== LOAD DANH SÁCH PHÒNG ====================
async function loadDanhSachPhong() {
    try {
        const select = document.getElementById('selectPhong');
        if (!select) {
            console.log('⚠️ Không tìm thấy select phòng');
            return;
        }

        console.log('🔄 Đang load danh sách phòng...');

        select.innerHTML = '<option value="">⏳ Đang tải...</option>';
        select.disabled = true;

        const response = await fetch('/QuanLySinhVien/GetPhongTrong');

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        console.log('📦 Dữ liệu phòng:', data);

        select.innerHTML = '<option value="">-- Chọn phòng --</option>';
        select.disabled = false;

        if (!Array.isArray(data) || data.length === 0) {
            const option = document.createElement('option');
            option.value = "";
            option.textContent = "❌ Không có phòng trống";
            option.disabled = true;
            select.appendChild(option);
            showToast('Không có phòng trống!', 'warning');
            return;
        }

        data.forEach(phong => {
            const option = document.createElement('option');
            option.value = phong.maP;
            option.textContent = `Phòng ${phong.maP} (Còn ${phong.conLai}/${phong.sucChua} chỗ)`;

            option.setAttribute('data-succhua', phong.sucChua);
            option.setAttribute('data-songuoi', phong.soNguoiDangO);
            option.setAttribute('data-conlai', phong.conLai);

            select.appendChild(option);
        });

        console.log(`✅ Đã load ${data.length} phòng`);
        showToast(`Tìm thấy ${data.length} phòng còn trống`, 'success');

    } catch (error) {
        console.error('❌ Lỗi:', error);
        showToast('Không thể tải danh sách phòng: ' + error.message, 'error');

        const select = document.getElementById('selectPhong');
        if (select) {
            select.innerHTML = '<option value="">❌ Lỗi tải dữ liệu</option>';
            select.disabled = false;
        }
    }
}

window.loadDanhSachPhong = loadDanhSachPhong;
window.loadDanhSachPhongRazor = loadDanhSachPhong;

// ==================== CẬP NHẬT THÔNG TIN PHÒNG ====================
window.updateRoomInfo = function (select) {
    const roomInfoDisplay = document.getElementById('roomInfoDisplay');

    if (!roomInfoDisplay) {
        console.error('❌ Không tìm thấy roomInfoDisplay');
        return;
    }

    if (select.value) {
        const selectedOption = select.options[select.selectedIndex];
        const sucChua = selectedOption.getAttribute('data-succhua') || '-';
        const soNguoi = selectedOption.getAttribute('data-songuoi') || '-';
        const conLai = selectedOption.getAttribute('data-conlai') || '-';

        const sucChuaEl = document.getElementById('sucChuaInfo');
        const soNguoiEl = document.getElementById('soNguoiInfo');
        const conLaiEl = document.getElementById('conLaiInfo');

        if (sucChuaEl) sucChuaEl.textContent = sucChua + ' người';
        if (soNguoiEl) soNguoiEl.textContent = soNguoi + ' người';
        if (conLaiEl) conLaiEl.textContent = conLai + ' chỗ';

        roomInfoDisplay.classList.remove('d-none');

        console.log('📊 Thông tin phòng:', { sucChua, soNguoi, conLai });
    } else {
        roomInfoDisplay.classList.add('d-none');
    }
}

// ==================== VALIDATION ====================
function initializeFormValidation() {
    // Form tạo hợp đồng
    const formTaoHopDong = document.getElementById('taoHopDongForm');
    if (formTaoHopDong) {
        formTaoHopDong.addEventListener('submit', function (e) {
            e.preventDefault();

            if (!validateForm(this)) {
                return false;
            }

            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="bi bi-hourglass-split me-1"></i>Đang xử lý...';

            try {
                this.submit();
            } catch (error) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
                showToast('Có lỗi xảy ra!', 'error');
            }
        });
    }

    // Form cấp phòng
    const formCapPhong = document.querySelector('form[action*="CapPhong"]');
    if (formCapPhong && formCapPhong.id !== 'taoHopDongForm') {
        formCapPhong.addEventListener('submit', function (e) {
            if (!validateForm(this)) {
                e.preventDefault();
                return false;
            }
        });
    }

    // Form chuyển phòng
    const formChuyenPhong = document.querySelector('form[action*="ChuyenPhong"]');
    if (formChuyenPhong) {
        formChuyenPhong.addEventListener('submit', function (e) {
            const lyDo = this.querySelector('textarea[name="lyDo"], input[name="lyDo"]');

            if (!validateForm(this)) {
                e.preventDefault();
                return false;
            }

            if (lyDo && (!lyDo.value.trim() || lyDo.value.trim().length < 10)) {
                e.preventDefault();
                showToast('Lý do chuyển phòng phải có ít nhất 10 ký tự!', 'warning');
                lyDo.focus();
                return false;
            }

            if (!confirm('Bạn có chắc chắn muốn chuyển phòng?')) {
                e.preventDefault();
                return false;
            }
        });
    }
}

function validateForm(form) {
    const maPhong = form.querySelector('select[name="maPhong"], select[name="maPhongMoi"]');
    const ngayBatDau = form.querySelector('input[name="ngayBatDau"]');
    const ngayKetThuc = form.querySelector('input[name="ngayKetThuc"]');

    // Kiểm tra phòng
    if (maPhong && !maPhong.value) {
        showToast('Vui lòng chọn phòng!', 'warning');
        maPhong.focus();
        return false;
    }

    // Kiểm tra ngày (nếu có)
    if (ngayBatDau && ngayKetThuc) {
        if (!ngayBatDau.value || !ngayKetThuc.value) {
            showToast('Vui lòng chọn đầy đủ ngày bắt đầu và kết thúc!', 'warning');
            return false;
        }

        const start = new Date(ngayBatDau.value);
        const end = new Date(ngayKetThuc.value);

        if (end <= start) {
            showToast('Ngày kết thúc phải sau ngày bắt đầu!', 'error');
            ngayKetThuc.focus();
            return false;
        }

        const diffDays = Math.ceil((end - start) / (1000 * 60 * 60 * 24));
        if (diffDays < 30) {
            if (!confirm('Hợp đồng ngắn hơn 1 tháng. Bạn có chắc chắn?')) {
                return false;
            }
        }
    }

    return true;
}

// ==================== VALIDATION GIA HẠN ====================
function initializeGiaHanValidation() {
    const formGiaHan = document.querySelector('#giaHanModal form');
    if (formGiaHan) {
        formGiaHan.addEventListener('submit', function (e) {
            const input = this.querySelector('input[name="ngayKetThucMoi"]');
            if (!input) return;

            const ngayMoi = new Date(input.value);
            const minDate = new Date(input.min);

            if (ngayMoi <= minDate) {
                e.preventDefault();
                showToast('Ngày kết thúc mới phải sau ngày hiện tại!', 'error');
                input.focus();
                return false;
            }
        });
    }
}

// ==================== VALIDATION HỦY HỢP ĐỒNG ====================
function initializeHuyHopDongValidation() {
    const formHuy = document.querySelector('#huyHopDongModal form');
    if (formHuy) {
        formHuy.addEventListener('submit', function (e) {
            const lyDo = this.querySelector('textarea[name="lyDoHuy"]');
            if (!lyDo) return;

            const text = lyDo.value.trim();

            if (!text) {
                e.preventDefault();
                showToast('Vui lòng nhập lý do hủy hợp đồng!', 'warning');
                lyDo.focus();
                return false;
            }

            if (text.length < 10) {
                e.preventDefault();
                showToast('Lý do hủy phải có ít nhất 10 ký tự!', 'warning');
                lyDo.focus();
                return false;
            }

            if (!confirm('BẠN CÓ CHẮC CHẮN MUỐN HỦY HỢP ĐỒNG?\n\nHành động này không thể hoàn tác!')) {
                e.preventDefault();
                return false;
            }
        });
    }
}

// ==================== MODAL HANDLERS ====================
function initializeModalHandlers() {
    document.querySelectorAll('.modal').forEach(modal => {
        modal.addEventListener('shown.bs.modal', function () {
            const firstInput = this.querySelector('input:not([type="hidden"]):not([readonly]), select:not([disabled]), textarea:not([readonly])');
            if (firstInput) {
                setTimeout(() => firstInput.focus(), 100);
            }

            // Load phòng cho modal tạo hợp đồng
            if (this.id === 'taoHopDongModal') {
                loadDanhSachPhong();
            }
        });

        modal.addEventListener('hidden.bs.modal', function () {
            const form = this.querySelector('form');
            if (form) {
                form.reset();
                const display = document.getElementById('roomInfoDisplay');
                if (display) display.classList.add('d-none');
            }
        });
    });

    initializeGiaHanValidation();
    initializeHuyHopDongValidation();
}

// ==================== REAL-TIME VALIDATION ====================
function setupRealTimeValidation() {
    document.querySelectorAll('input[name="ngayBatDau"]').forEach(inputStart => {
        const form = inputStart.closest('form');
        if (!form) return;

        const inputEnd = form.querySelector('input[name="ngayKetThuc"]');
        if (!inputEnd) return;

        inputStart.addEventListener('change', function () {
            const start = new Date(this.value);
            const minEnd = new Date(start);
            minEnd.setDate(minEnd.getDate() + 1);

            inputEnd.min = minEnd.toISOString().split('T')[0];

            if (!inputEnd.value) {
                const suggested = new Date(start);
                suggested.setMonth(suggested.getMonth() + 6);
                inputEnd.value = suggested.toISOString().split('T')[0];
            }
        });

        [inputStart, inputEnd].forEach(input => {
            input.addEventListener('change', function () {
                if (inputStart.value && inputEnd.value) {
                    const start = new Date(inputStart.value);
                    const end = new Date(inputEnd.value);
                    const months = Math.round((end - start) / (1000 * 60 * 60 * 24 * 30));

                    if (months > 0) {
                        showToast(`⏱️ Thời gian hợp đồng: ${months} tháng`, 'info');
                    }
                }
            });
        });
    });
}

// ==================== UTILITIES ====================
function showToast(message, type = 'info') {
    const alertClass = {
        'success': 'alert-success',
        'error': 'alert-danger',
        'warning': 'alert-warning',
        'info': 'alert-info'
    }[type] || 'alert-info';

    const icon = {
        'success': 'bi-check-circle-fill',
        'error': 'bi-x-circle-fill',
        'warning': 'bi-exclamation-triangle-fill',
        'info': 'bi-info-circle-fill'
    }[type] || 'bi-info-circle-fill';

    const toast = document.createElement('div');
    toast.className = `alert ${alertClass} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3`;
    toast.style.zIndex = '9999';
    toast.style.minWidth = '300px';
    toast.style.maxWidth = '500px';
    toast.style.boxShadow = '0 4px 12px rgba(0,0,0,0.15)';
    toast.innerHTML = `
        <i class="bi ${icon} me-2"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 150);
    }, 4000);
}

function autoHideToasts() {
    document.querySelectorAll('.alert:not(.alert-dismissible)').forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.3s';
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    });
}

console.log('✅ sinhvien-details.js loaded successfully');