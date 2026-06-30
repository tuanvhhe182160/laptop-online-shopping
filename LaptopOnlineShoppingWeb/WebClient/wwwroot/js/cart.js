// Đọc cấu hình từ thẻ body
const API_BASE = 'https://localhost:7136'; // Hãy đảm bảo port này khớp với project WebAPI của bạn
const TOKEN = document.body.getAttribute('data-token');

// Hàm tạo Header có chứa Token bảo mật
function getHeaders() {
    const headers = { 'Content-Type': 'application/json' };
    if (TOKEN) {
        headers['Authorization'] = `Bearer ${TOKEN}`;
    }
    return headers;
}

// ĐÃ SỬA: Thay laptopId bằng variantId
async function addToCart(variantId, quantity) {
    const payload = { variantId, quantity };
    try {
        const response = await fetch(`${API_BASE}/api/Cart/items`, {
            method: 'POST',
            headers: getHeaders(),
            body: JSON.stringify(payload)
        });

        if (response.status === 401) return { ok: false, message: 'NOT_AUTHENTICATED' };
        if (response.ok) return { ok: true };

        const err = await response.text();
        return { ok: false, message: err };
    } catch (e) {
        console.error(e);
        return { ok: false, message: 'Không thể kết nối đến máy chủ API.' };
    }
}

// ĐÃ SỬA: Thay laptopId bằng variantId và thêm /api/ vào URL
async function updateCart(variantId, quantity) {
    if (quantity <= 0) { alert('Số lượng phải lớn hơn 0'); return; }
    const payload = { variantId, quantity: parseInt(quantity) };

    try {
        const response = await fetch(`${API_BASE}/api/Cart/items`, {
            method: 'PUT',
            headers: getHeaders(), // Đính kèm Token
            body: JSON.stringify(payload)
        });

        if (response.status === 401) { alert('Vui lòng đăng nhập để chỉnh sửa giỏ hàng'); return; }
        if (response.ok) { location.reload(); }
        else { alert('Cập nhật thất bại. Có thể vượt quá tồn kho.'); location.reload(); }
    } catch (e) { console.error(e); }
}

// ĐÃ SỬA: Thay laptopId bằng variantId và thêm /api/ vào URL
async function removeFromCart(variantId) {
    if (!confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) return;
    try {
        const response = await fetch(`${API_BASE}/api/Cart/items/${variantId}`, {
            method: 'DELETE',
            headers: getHeaders() // Đính kèm Token
        });

        if (response.status === 401) { alert('Vui lòng đăng nhập để xóa sản phẩm'); return; }
        if (response.ok) { location.reload(); } else { alert('Xóa thất bại'); }
    } catch (e) { console.error(e); }
}

// ĐÃ SỬA: Thêm /api/ vào URL
async function getCartCount() {
    if (!TOKEN) return 0; // Nếu không có token thì khỏi gọi API cho tốn tài nguyên
    try {
        const response = await fetch(`${API_BASE}/api/Cart`, {
            headers: getHeaders()
        });

        if (!response.ok) return 0;
        const cart = await response.json();

        if (!cart) return 0;
        if (Array.isArray(cart.items)) return cart.items.reduce((s, it) => s + (it.quantity || 0), 0);
        if (typeof cart.totalQuantity === 'number') return cart.totalQuantity;
        return 0;
    } catch (e) {
        console.error(e);
        return 0;
    }
}