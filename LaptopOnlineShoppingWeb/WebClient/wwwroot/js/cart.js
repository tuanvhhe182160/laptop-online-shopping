const API_BASE = "https://localhost:7196/api"; // Thay đổi port theo thực tế của WebAPI

// Giả sử customerId được lưu trong cookie/session. Ở đây mock là 1.
const customerId = 1;

async function addToCart(laptopId, quantity) {
    const payload = { laptopId, quantity };
    try {
        const response = await fetch(`${API_BASE}/customers/${customerId}/cart/items`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (response.ok) {
            alert("Đã thêm vào giỏ hàng");
        } else {
            const err = await response.text();
            alert("Lỗi: " + err);
        }
    } catch (e) {
        console.error(e);
        alert("Có lỗi xảy ra khi gọi API");
    }
}

async function updateCart(laptopId, quantity) {
    if (quantity <= 0) {
        alert("Số lượng phải lớn hơn 0");
        return;
    }
    const payload = { laptopId, quantity: parseInt(quantity) };
    try {
        const response = await fetch(`${API_BASE}/customers/${customerId}/cart/items`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (response.ok) {
            location.reload();
        } else {
            alert("Cập nhật thất bại. Có thể vượt quá tồn kho.");
            location.reload();
        }
    } catch (e) {
        console.error(e);
    }
}

async function removeFromCart(laptopId) {
    if (!confirm("Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?")) return;
    try {
        const response = await fetch(`${API_BASE}/customers/${customerId}/cart/items/${laptopId}`, {
            method: 'DELETE'
        });
        if (response.ok) {
            location.reload();
        } else {
            alert("Xóa thất bại");
        }
    } catch (e) {
        console.error(e);
    }
}
