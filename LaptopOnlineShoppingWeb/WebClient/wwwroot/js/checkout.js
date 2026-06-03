// Bọc tất cả vào sự kiện DOMContentLoaded để tạo Scope (phạm vi) cục bộ
document.addEventListener("DOMContentLoaded", () => {

    // Nhốt 2 biến này vào trong, chúng sẽ không đụng độ với cart.js nữa
    const API_BASE = document.body.getAttribute('data-api-base') || '';
    const TOKEN = document.body.getAttribute('data-token');

    // Hàm getHeaders cũng được nhốt vào trong
    function getHeaders() {
        const headers = { 'Content-Type': 'application/json' };
        if (TOKEN) headers['Authorization'] = `Bearer ${TOKEN}`;
        return headers;
    }

    const form = document.getElementById("checkoutForm");
    if (form) {
        form.addEventListener("submit", async (e) => {
            // Chặn form submit kiểu cũ (chặn cái dấu ? trên URL)
            e.preventDefault();

            // Nếu không có token, báo lỗi đăng nhập
            if (!TOKEN) {
                if (confirm('Phiên đăng nhập đã hết hạn. Chuyển đến trang đăng nhập?')) {
                    window.location.href = '/Auth/CustomerLogin';
                }
                return;
            }

            const isBuyNow = document.getElementById("isBuyNow").value === "true";
            const laptopId = parseInt(document.getElementById("laptopId").value);
            let quantity = parseInt(document.getElementById("quantity").value || 0);

            if (isBuyNow) {
                const qElem = document.getElementById('buyNowQuantity');
                if (qElem) quantity = parseInt(qElem.value) || 1;
            }

            const shippingAddress = document.getElementById("shippingAddress").value;
            const paymentMethod = document.getElementById("paymentMethod").value;

            // Xóa customerId khỏi Payload và URL
            if (isBuyNow) {
                const payload = { laptopId, quantity, shippingAddress, paymentMethod };
                await processCheckout(`${API_BASE}/api/Orders/checkout-direct`, payload);
            } else {
                const payload = { shippingAddress, paymentMethod };
                await processCheckout(`${API_BASE}/api/Orders/checkout-cart`, payload);
            }
        });
    }

    // Đưa hàm processCheckout vào trong để nó dùng được getHeaders()
    async function processCheckout(url, payload) {
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: getHeaders(), // Gắn Token
                body: JSON.stringify(payload)
            });

            if (response.status === 401) {
                Swal.fire({ title: 'Lỗi', text: 'Vui lòng đăng nhập lại.', icon: 'error' });
                return;
            }

            if (response.ok) {
                let orderId = null;
                try {
                    const json = await response.json();
                    orderId = json && (json.orderId || json.id || json.orderID);
                } catch { }

                Swal.fire({
                    title: 'Đặt hàng thành công!',
                    text: orderId ? `Mã đơn hàng: #${orderId}` : 'Đơn hàng đã được tạo.',
                    icon: 'success',
                    timer: 2000,
                    showConfirmButton: false
                }).then(() => {
                    // Chuyển hướng về trang lịch sử mua hàng
                    window.location.href = "/Storefront/OrderHistory";
                });
            } else {
                const err = await response.text();
                Swal.fire({ title: 'Lỗi', text: err, icon: 'error' });
            }
        } catch (e) {
            console.error(e);
            Swal.fire({ title: 'Lỗi', text: 'Lỗi kết nối.', icon: 'error' });
        }
    }
});